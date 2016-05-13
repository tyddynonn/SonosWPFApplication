using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using System.IO;


namespace SonosWpfApplication
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {

        private DataSet ds;
        public MemoryStream stream = null;

        public DataWindow()
        {
            InitializeComponent();
            ds = new DataSet();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (stream != null)
            {
                ShowPlayList(stream);
            }

        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "";

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (*.xml)|*.xml";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();
            
            // Get the selected file name  
            if (result == true)
            {
                filePath = dlg.FileName;
            }

            if (!String.IsNullOrEmpty(filePath))
            {
                byte[] content = File.ReadAllBytes(filePath);
                ShowPlayList(new MemoryStream(content));
            }
        }
        
        private void ShowPlayList(MemoryStream stream)
    {
                ds.ReadXml(stream);
                DataTable dt = ds.Tables[0];
                foreach (DataColumn dc in dt.Columns)
                {
                    DataGridTextColumn  dgc = new DataGridTextColumn ();
                    dgc.Header=dc.Caption;
                    dgc.Binding = new Binding(dc.ColumnName);
                    dgItems.Columns.Add(dgc);
                }
                dgItems.ItemsSource  = ds.Tables[0].DefaultView ;
                dgItems.Items.Refresh();

    }


        private void btnDups_Click(object sender, RoutedEventArgs e)
        {
            // Extract Duplicates...
            //SELECT item.ItemID, item.title, item.album, item.originalTrackNumber, item.albumArtist, item.creator
            //FROM item
            //WHERE (((item.title) In (SELECT [title] FROM [item] As Tmp GROUP BY [title],[album] HAVING Count(*)>1  And [album] = [item].[album])))
            //ORDER BY item.title, item.album;

            DataTable dt = ds.Tables[0];
            var dups = dt.AsEnumerable()
            .GroupBy(dr => new { Title = dr.Field<string>("title"), Album=dr.Field<string>("album")})
            .Where(g => g.Count() > 1)
            .SelectMany(g => g);
           
            // copy the result set into a new Data Table and display that...

            DataTable dtDups = new DataTable("Duplicates");

            foreach (DataColumn dc in dt.Columns)
            {
                dtDups.Columns.Add(dc.Caption, dc.DataType );
            }

            foreach (DataRow dr in dups)
            {
                dtDups.Rows.Add(dr.ItemArray );
            } 
            txtNumDups.Text=dups.Count() + " duplicates found";
            dgItems.ItemsSource = dtDups.DefaultView ;           
            dgItems.Items.Refresh();
        }
    }
}
