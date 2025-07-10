using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF.Test;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
#if DEBUG
	[TestExcludeWPFControlFromBasher]
#endif
	public partial class NewColumnPopup : Window, IDisposable
	{
		readonly DataTable dataTable;
		readonly bool isAdd;
		readonly int currentResultSetColumnIndex;
		public NewColumnPopup(DataTable dataTable, bool isAdd = true, int currentResultSetColumnIndex = 0)
		{
			InitializeComponent();
			this.dataTable = dataTable;
			this.isAdd = isAdd;
			this.currentResultSetColumnIndex = currentResultSetColumnIndex;
			if (!this.isAdd)
			{
				ActionButton.Content = "Edit";  // Not using ZorKArchitecture
				Title = "Edit column";
				columnName.Text = dataTable.Columns[this.currentResultSetColumnIndex].ColumnName.Replace("_x2024_", ".");
				if (dataTable.Columns[this.currentResultSetColumnIndex].DataType.Equals(typeof(long))) // Not using ZorKArchitecture
				{
					columnType.SelectedIndex = 1;
				}
				else if (dataTable.Columns[this.currentResultSetColumnIndex].DataType.Equals(typeof(Double))) // Not using ZorKArchitecture
				{
					columnType.SelectedIndex = 2;
				}
				else if (dataTable.Columns[this.currentResultSetColumnIndex].DataType.Equals(typeof(DateTime))) // Not using ZorKArchitecture
				{
					columnType.SelectedIndex = 3;
				}
				else if (dataTable.Columns[this.currentResultSetColumnIndex].DataType.Equals(typeof(Boolean))) // Not using ZorKArchitecture
				{
					columnType.SelectedIndex = 4;
				}
				else
				{
					columnType.SelectedIndex = 0;
				}
				if (dataTable.Rows.Count > 0)
				{
					columnType.IsEnabled = false;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Not using ZorKArchitectures")]
		void AddButton_Click(object sender, RoutedEventArgs e)
		{
			DataTable dataTableClone = null;

			if (string.IsNullOrWhiteSpace(columnName.Text))
			{
				MessageBox.Show("Invalid column name", "Invalid data");  // Not using ZorKArchitecture
				return;
			}
			if (columnType.SelectedIndex < 0)
			{
				MessageBox.Show("Invalid column type", "Invalid data"); // Not using ZorKArchitecture
				return;
			}
			DataColumn dataColumn;
			if (isAdd)
			{
				dataColumn = new DataColumn();
				dataColumn.AllowDBNull = true;
			}
			else
			{
				dataTableClone = dataTable.Clone();
				dataColumn = dataTableClone.Columns[currentResultSetColumnIndex];
				dataColumn.DefaultValue = DBNull.Value;
			}
			dataColumn.ColumnName = columnName.Text.Replace(".", "_x2024_");
			dataColumn.Caption = columnName.Text;
			dataColumn.ColumnMapping = MappingType.Attribute;
			String selecteColumnType = ((ComboBoxItem)columnType.SelectedItem).Content.ToString();
			try {
				if (selecteColumnType.Equals("Long")) // Not using ZorKArchitecture
				{
					dataColumn.DataType = typeof(long);
					dataColumn.DefaultValue = 0;
				}
				else if (selecteColumnType.Equals("Double")) // Not using ZorKArchitecture
				{
					dataColumn.DataType = typeof(Double);
					dataColumn.DefaultValue = 0.0;
				}
				else if (selecteColumnType.Equals("DateTime")) // Not using ZorKArchitecture
				{
					dataColumn.DataType = typeof(DateTime);
					dataColumn.DefaultValue = DateTime.Now;  // Not using ZorKArchitectures
				}
				else if (selecteColumnType.Equals("Boolean")) // Not using ZorKArchitecture
				{
					dataColumn.DataType = typeof(bool);
					dataColumn.DefaultValue = false;
				}
				else
				{
					dataColumn.DataType = typeof(string);
					dataColumn.DefaultValue = "";
				}

				if (isAdd)
				{
					dataTable.Columns.Add(dataColumn);
				}
				else
				{
					DataSet dataSet = dataTable.DataSet;
					for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; ++rowIndex)
					{
						dataTableClone.ImportRow(dataTable.Rows[rowIndex]);
						int columnIndex = dataTableClone.Columns.IndexOf(dataColumn.ColumnName);
						dataTableClone.Rows[rowIndex][columnIndex] = dataTable.Rows[rowIndex][currentResultSetColumnIndex];
					}
					dataSet.Tables.Remove(dataTable);
					dataSet.Tables.Add(dataTableClone);
				}
			}
			catch (ConstraintException ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); // Not using ZorKArchitecture
			}
			catch (DuplicateNameException ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); // Not using ZorKArchitecture
			}
			catch (ArgumentException aex)
			{
				MessageBox.Show(aex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); // Not using ZorKArchitecture
			}
			DialogResult = true;
			Close();
		}

		void CancelButtion_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		#region Dispose

		bool disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
				}
				disposed = true;
			}
		}

		#endregion

	}
	#endregion
}
