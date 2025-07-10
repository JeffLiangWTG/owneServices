using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	internal class DataTableGridView : ZDataGridView
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		const string RowStateName = "Row State";
		const string CommaSymbol = ", ";
		const string DoubleQuotationMarks = "\"";
		const string NullString = "Null";

		readonly List<string> ByteColumnNames = new List<string>();

		public DataTable OriginalDataTable
		{
			get;
			set;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		public void InstallDataTableSafe(DataTable dataTable, bool showRowState)
		{
			OriginalDataTable = dataTable;
			try
			{
#if !WINZOR
				DataSource = ConvertDataTypeForDisplay(dataTable, showRowState);
#else
				if (dataTable != null)
				{
					var modifiedDataTable = ConvertDataTypeForDisplay(dataTable, showRowState);
					DataSource = modifiedDataTable;
					TableStyles.Add(GetDataGridColumnFromTable(modifiedDataTable));
				}
#endif
			}
			catch (InvalidOperationException)
			{
				UserNotification.Instance.ShowError("This table is too large or complicated to display.\n");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		public override ContextMenu ExtensionMenu
		{
			get
			{
				if (extensionMenu == null)
				{
					extensionMenu = new ContextMenu();
					extensionMenu.MenuItems.Add("Select All", (sender, args) => SeleteAll());
					extensionMenu.MenuItems.Add("-");
					extensionMenu.MenuItems.Add("Copy", (sender, args) => Copy(false));
					extensionMenu.MenuItems.Add("Copy With Headers", (sender, args) => Copy(true));
					extensionMenu.MenuItems.Add("Copy As CSV", (sender, args) => CopyAsCsv());
					extensionMenu.MenuItems.Add("Copy As Insert SQL", (sender, args) => CopyAsInsertSql());
				}

				return extensionMenu;
			}
		}
		ContextMenu extensionMenu;

		#region Base Operation

		void SeleteAll()
		{
			if (Rows.Count > 0)
			{
				Focus();
				SelectAll();
			}
		}

		void Copy(bool isWithHeaders)
		{
			if (this.Rows.Count > 0)
			{
				Focus();
				ClipboardCopyMode = isWithHeaders
					? DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
					: DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

				SafeClipboard.SetDataObject(GetClipboardContent());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Date formatting, not actual text")]
		internal void SetDateColumnsToSecondsFormat()
		{
#if WINZOR
			ChangeDateTimeColumnsFormat();
#else
			foreach (DataGridViewColumn column in Columns)
			{
				if (column.ValueType == typeof(DateTime) || column.ValueType == typeof(ZDateTime))
				{
					column.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
				}
			}
#endif
		}

#endregion

		#region Copy As CSV

		void CopyAsCsv()
		{
			var str = BuildCsvString();
			CopyStringToClipboard(str);
		}

		string BuildCsvString()
		{
			var cells = SelectedCells.Cast<DataGridViewCell>().ToArray();
			if (cells.Any())
			{
				var rows = cells.Select(c => c.OwningRow).OrderBy(row => row.Index).Distinct().ToArray();
				var columns = cells.Select(owningColumn => owningColumn.OwningColumn)
					.Where(column => column.HeaderText != RowStateName)
					.Distinct()
					.OrderBy(column => column.Index)
					.ToArray();

				return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", GetColumnNameString(columns, DoubleQuotationMarks), System.Environment.NewLine, GetStringBuilderValuesForCells(rows, columns, false));
			}

			return string.Empty;
		}

		#endregion

		#region Copy As Insert SQL

		void CopyAsInsertSql()
		{
			var str = BuildInsertString();
			CopyStringToClipboard(str);
		}

		string BuildInsertString()
		{
			if (OriginalDataTable != null)
			{
				var name = OriginalDataTable.TableName;
				var cells = SelectedCells.Cast<DataGridViewCell>().ToArray();
				if (!string.IsNullOrEmpty(name) && cells.Any())
				{
					var rows = cells.Select(c => c.OwningRow).OrderBy(row => row.Index).Distinct().ToArray();
					var columns = cells.Select(owningColumn => owningColumn.OwningColumn)
						.Where(column => column.HeaderText != RowStateName)
						.Distinct()
						.OrderBy(column => column.Index)
						.ToArray();

					if (rows.Any() && columns.Any())
					{
						return string.Format(CultureInfo.InvariantCulture,
							"INSERT INTO {0}{1}({2}){1}VALUES{1}{3}",
							name,
							System.Environment.NewLine,
							GetColumnNameString(columns),
							GetStringBuilderValuesForCells(rows, columns, true).ToStringWithDelimiterBetweenAppends("," + System.Environment.NewLine));
					}
				}
			}

			return string.Empty;
		}

		#endregion

		#region Base Methods

		DataTable ConvertDataTypeForDisplay(DataTable dataTable, bool showRowState)
		{
			if (dataTable == null)
			{
				return null;
			}

			ByteColumnNames.Clear();

			var newTable = dataTable.Clone();
			if (showRowState)
			{
				if (!newTable.Columns.Contains(RowStateName))
				{
					newTable.Columns.Add(RowStateName);
				}
				newTable.Columns[RowStateName].ReadOnly = false;
			}

			foreach (DataColumn column in newTable.Columns)
			{
				if (column.DataType == typeof(byte[]))
				{
					column.DataType = typeof(string);
					ByteColumnNames.Add(column.ColumnName);
				}
			}

			foreach (DataRow row in dataTable.Rows)
			{
				var newRow = newTable.NewRow();
				foreach (DataColumn column in dataTable.Columns)
				{
					var oldValue = row.RowState == DataRowState.Deleted
						? row[column.ColumnName, DataRowVersion.Original]
						: row[column.ColumnName];

					if (ByteColumnNames.Contains(column.ColumnName))
					{
						var valueAsByte = oldValue as byte[];
						if (valueAsByte != null)
						{
							newRow[column.ColumnName] = string.Join("", valueAsByte.Select(x => ((int)x).ToString("X").PadLeft(2, '0')));
						}
					}
					else
					{
						newRow[column.ColumnName] = oldValue;
					}
				}

				if (showRowState)
				{
					newRow[RowStateName] = row.RowState;
				}
				newTable.Rows.Add(newRow);
			}

			if (showRowState)
			{
				newTable.Columns[RowStateName].ReadOnly = true;
			}

			return newTable;
		}

		void CopyStringToClipboard(string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				SafeClipboard.SetText(str);
			}
		}

		string GetColumnNameString(DataGridViewColumn[] columns, string markerSymbol = "")
		{
			var sb = new ZStringBuilder();
			foreach (var column in columns)
			{
				sb.Append(string.Format(CultureInfo.InvariantCulture, "{0}{1}{0}", markerSymbol, column.DataPropertyName));
			}

			return sb.ToStringWithDelimiterBetweenAppends(CommaSymbol);
		}

		ZStringBuilder GetStringBuilderValuesForCells(DataGridViewRow[] rows, DataGridViewColumn[] columns, bool isSql)
		{
			var sb = new ZStringBuilder();
			foreach (var row in rows)
			{
				var columnBuilder = new ZStringBuilder();
				foreach (var column in columns)
				{
					columnBuilder.Append(GetCellValue(row.Cells[column.Index], isSql));
				}

				var columnValues = columnBuilder.ToStringWithDelimiterBetweenAppends(CommaSymbol);
				if (isSql)
				{
					sb.Append(string.Format(CultureInfo.InvariantCulture, "({0})", columnValues));
				}
				else
				{
					sb.AppendLine(columnValues);
				}
			}

			return sb;
		}

		string GetCellValue(DataGridViewCell cell, bool isSql)
		{
			return isSql ? GetCellValueForSqlFormat(cell) : GetCellValueForCsvFormat(cell);
		}

		string GetCellValueForCsvFormat(DataGridViewCell cell)
		{
			var cellValue = GetCellValueBaseFormat(cell);
			if (cellValue == NullString)
			{
				cellValue = string.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, "{0}{1}{0}", DoubleQuotationMarks, cellValue);
		}

		string GetCellValueForSqlFormat(DataGridViewCell cell)
		{
			var cellValue = GetCellValueBaseFormat(cell);
			if (cellValue == NullString)
			{
				return cellValue;
			}

			#region SuppressResourceStringsCheckRegion

			var columnName = cell.OwningColumn.DataPropertyName;
			if (ByteColumnNames.Contains(columnName))
			{
				cellValue = string.Format(CultureInfo.InvariantCulture, "dbo.CLRCompressStringAsBytes({0})", cellValue);
			}
			else if (cell.ValueType == typeof(Boolean))
			{
				cellValue = cellValue == "True" ? "1" : "0";
			}

			#endregion

			return string.Format(CultureInfo.InvariantCulture, "'{0}'", cellValue);
		}

		string GetCellValueBaseFormat(DataGridViewCell cell)
		{
			if (cell == null || cell.Value == DBNull.Value || cell.Value == null)
			{
				return NullString;
			}

			var cellValue = cell.Value;
			if (cell.ValueType == typeof(DateTime))
			{
				return Convert.ToDateTime(cellValue, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
			}
			if (cell.ValueType == typeof(ZDateTime))
			{
				return new ZDateTime(cellValue).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
			}

			return cellValue.ToString();
		}

		#endregion
	}
}
