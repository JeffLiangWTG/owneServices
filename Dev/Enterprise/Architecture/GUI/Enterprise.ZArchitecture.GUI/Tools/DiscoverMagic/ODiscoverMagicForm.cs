using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Windows.UI;

namespace Enterprise.Core.Forms
{
	public partial class ODiscoverMagicForm : KForm
	{
		readonly System.ComponentModel.Container components;

		public ODiscoverMagicForm()
		{
			InitializeComponent();
			fInitialGridTop = listBoxWithGrid.Top;
			fInitialGridHeight = listBoxWithGrid.Height;
		}

		public new object DataSource
		{
			get { return listBoxWithGrid.DataSource; }
			set
			{
				listBoxWithGrid.DataSource = value;
				ButtonsVisible = (value is DataSet);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Implementation

		protected int fInitialGridTop, fInitialGridHeight;

		protected bool ButtonsVisible
		{
			get { return this.ShowOriginalButton.Visible; }
			set
			{
				this.ShowOriginalButton.Visible = value;
				this.ShowRowsInDBButton.Visible = value;
				if (value)
				{
					ControlDpiScalingHelper.SetTop(ref listBoxWithGrid, fInitialGridTop, false);
					ControlDpiScalingHelper.SetHeight(ref listBoxWithGrid, fInitialGridHeight, false);
				}
				else
				{
					ControlDpiScalingHelper.SetTop(ref listBoxWithGrid, 0, true);
					ControlDpiScalingHelper.SetHeight(ref listBoxWithGrid, Height, false);
				}
			}
		}

		#region Show Original Rows

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		protected void OnShowOriginal_Click(object sender, System.EventArgs e)
		{
			var originalTable = GetOriginalRowsFromTable(listBoxWithGrid.CurrentTable);
			ShowTableInNewWindow(originalTable, "Original Rows");
		}

		DataTable GetOriginalRowsFromTable(DataTable dataTable)
		{
			var result = dataTable.Clone();

			foreach (DataRow row in dataTable.Rows)
			{
				if (row.HasVersion(DataRowVersion.Original))
				{
					var newRow = result.NewRow();
					foreach (DataColumn column in row.Table.Columns)
					{
						newRow[column.ColumnName] = row[column, DataRowVersion.Original];
					}
					result.Rows.Add(newRow);
				}
			}
			return result;
		}

		#endregion

		#region Show Rows in DB

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		protected void OnShowRowsInDB_Click(object sender, System.EventArgs e)
		{
			var tableInDB = GetDataTableInDB(listBoxWithGrid.CurrentTable);
			ShowTableInNewWindow(tableInDB, "Rows in Database");
		}

		DataTable GetDataTableInDB(DataTable dataTable)
		{
			var query = GetDataTableQuery(dataTable);
			var dbRows = new RowFactory().Load(dataTable.TableName, query);
			var dbTable = dataTable.Clone();
			foreach (var row in dbRows)
			{
				var newRow = dbTable.NewRow();
				foreach (DataColumn column in row.Table.Columns)
				{
					newRow[column.ColumnName] = row[column];
				}
				dbTable.Rows.Add(newRow);
			}

			return dbTable;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetDataTableQuery(DataTable dataTable)
		{
			var query = new ZQuery();
			var primaryKeys = dataTable.PrimaryKey;
			if (primaryKeys.Length == 1)
			{
				var pkColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(primaryKeys[0].ColumnName, dataTable.TableName);
				var rowValues = (from DataRow row in dataTable.Rows select row[pkColumn.Name]).ToArray();

				query.AddToFilter(pkColumn, rowValues);
			}
			else if (primaryKeys.Length > 1)
			{
				foreach (DataRow row in dataTable.Rows)
				{
					var rowQuery = new ZQuery();
					foreach (var pkColumn in primaryKeys)
					{
						rowQuery.AddToFilter(ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(pkColumn.ColumnName, dataTable.TableName), row[pkColumn]);
					}
					query.AddToFilter(rowQuery, JoinCondition.Or);
				}
			}

			return query;
		}

		#endregion

		#region ShowTableInNewWindow

		protected void ShowTableInNewWindow(DataTable table, string caption)
		{
			var copiedTable = table.Copy();
			foreach (DataColumn column in copiedTable.Columns)
			{
				column.ReadOnly = true;
			}

			var form = new ODiscoverMagicForm
			{
				DataSource = copiedTable
			};

			form.Text += " - " + caption;
			form.Show();
		}

		#endregion

		#endregion
	}
}
