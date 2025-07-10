using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	internal partial class ZDataSetListBoxWithGrid : ZUserControl
	{
		public ZDataSetListBoxWithGrid()
		{
			InitializeComponent();
			listBox.SelectedIndexChanged += ListBoxSelectedIndexChanged;
		}

		void ListBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			var dataTable = DataSet.Tables.Cast<DataTable>().FirstOrDefault(t => listBox.SelectedItem != null && t.TableName == (string)listBox.SelectedItem);
			SetupGridDataSource(dataTable);
		}

		void TableGridViewOnMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				gridView.ExtensionMenu.Show(gridView, e.Location);
			}
		}

		public new object DataSource
		{
			get { return dataSource; }
			set
			{
				dataSource = value;
				listBox.DataSource = DataSet != null ? DataSet.Tables.Cast<DataTable>().Select(t => t.TableName).ToArray() : null;

				if (listBox.DataSource == null)
				{
					var dataTable = dataSource as DataTable;
					SetupGridDataSource(dataTable);

					listBox.Visible = false;
					ControlDpiScalingHelper.SetLeft(ref gridView, 0, true);
					ControlDpiScalingHelper.SetWidth(ref gridView, this.Width, false);
				}
				else
				{
					listBox.Visible = true;
					ControlDpiScalingHelper.SetLeft(ref gridView, GRID_DISTANCEFROMLEFT, true);
					ControlDpiScalingHelper.SetWidth(ref gridView, this.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(GRID_DISTANCEFROMLEFT + GRID_DISTANCEFROMRIGHT), false);
				}
			}
		}

		void SetupGridDataSource(DataTable dataTable)
		{
			gridView.InstallDataTableSafe(dataTable, true);
			gridView.SetDateColumnsToSecondsFormat();
		}

		object dataSource;
		const int GRID_DISTANCEFROMLEFT = 192;
		const int GRID_DISTANCEFROMRIGHT = 3;

		DataSet DataSet
		{
			get { return dataSource as DataSet; }
		}

		public DataTable CurrentTable
		{
			get { return gridView.OriginalDataTable; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				listBox.SelectedIndexChanged -= ListBoxSelectedIndexChanged;
			}
			base.Dispose(disposing);
		}

#if DEBUG
		internal ZListBox GetListBoxForTest
		{
			get { return listBox; }
		}

		internal DataTableGridView GridViewForTest
		{
			get { return gridView; }
		}
#endif
	}
}
