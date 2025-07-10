using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Types;
using Microsoft.AspNetCore.Components;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZDataGridView : ZGrid
	{
		public ZDataGridView() : base()
		{
			ContextMenu = ExtensionMenu;
			HookUpdateSelectedCells(ContextMenu);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		const string RowStateName = "Row State";

		public bool AllowUserToAddRows { get; set; }
		public bool AllowUserToDeleteRows { get; set; }
		public DataGridViewColumnHeadersHeightSizeMode ColumnHeadersHeightSizeMode { get; set; }
		DataGridViewClipboardCopyMode clipboardCopyMode;

		[Browsable(true)]
		[DefaultValue(DataGridViewClipboardCopyMode.EnableWithAutoHeaderText)]
		public DataGridViewClipboardCopyMode ClipboardCopyMode
		{
			get
			{
				return clipboardCopyMode;
			}
			set
			{
				if (!ClientUtils.IsEnumValid(value, (int)value, 0, 3))
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(DataGridViewClipboardCopyMode));
				}

				clipboardCopyMode = value;
			}
		}

		protected List<DataGridViewRow> Rows = new List<DataGridViewRow>();

		protected new List<DataGridViewColumn> Columns = new List<DataGridViewColumn>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		public virtual ContextMenu ExtensionMenu { get; set; }

		protected void SelectAll()
		{
			if (DataGridRowsLength > 0)
			{
				SelectAllElements();
				UpdateSelectedCells(this, EventArgs.Empty);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Date formatting, not actual text")]
		internal void ChangeDateTimeColumnsFormat()
		{
			if (TableStyles.Count == 0)
			{
				return;
			}

			foreach (DataGridColumnStyle column in TableStyles[0].GridColumnStyles)
			{
				if (column is DataGridTextBoxColumn textBoxColumn)
				{
					if (textBoxColumn.PropertyDescriptor != null &&
						(textBoxColumn.PropertyDescriptor.PropertyType == typeof(DateTime) || textBoxColumn.PropertyDescriptor.PropertyType == typeof(ZDateTime)))
					{
						textBoxColumn.Format = "yyyy-MM-dd HH:mm:ss";
					}
				}
			}
		}

		protected readonly List<DataGridViewCell> SelectedCells = new List<DataGridViewCell>();

		void HookUpdateSelectedCells(ContextMenu contextMenu)
		{
			contextMenu.Popup += UpdateSelectedCells;
		}

		void UnhookUpdateSelectedCells(ContextMenu contextMenu)
		{
			contextMenu.Popup -= UpdateSelectedCells;
		}

		protected override async Task OnMouseDownAsync(WebMouseEventArgs e, HitTestInfo hitTest, ElementReference? elementReference = null)
		{
			await base.OnMouseDownAsync(e, hitTest, elementReference);
			if (hitTest.Type == HitTestType.ColumnHeader)
			{
				await OnCellFocusInAsync(new WinzorFocusInEventArgs(), CurrentCell.RowNumber, CurrentCell.ColumnNumber);
			}
		}

		void UpdateSelectedCells(object sender, EventArgs e)
		{
			RefreshDataGridViewRow();
			SelectedCells.Clear();
			if (SelectedRowCount > 0)
			{
				var selectedRows = GetSelectedRowIndexes();
				foreach (var row in selectedRows)
				{
					int columnLength = Columns.Count;
					for (int column = 0; column < columnLength; column++)
					{
						SelectedCells.Add(GetDataGridViewCell(row, column));
					}
				}
			}
			else
			{
				SelectedCells.Add(GetDataGridViewCell(CurrentCell.RowNumber, CurrentCell.ColumnNumber));
			}
		}

		DataGridViewCell GetDataGridViewCell(int row, int column)
		{
			return Rows[row].Cells[column];
		}

		protected DataGridTableStyle GetDataGridColumnFromTable(DataTable dataTable)
		{
			SelectedCells.Clear();
			CreateColumns(dataTable);
			var tableStyle = new DataGridTableStyle { MappingName = dataTable.TableName };
			if (TableStyles.Count > 0)
			{
				foreach (var columnStyle in TableStyles[0].GridColumnStyles)
				{
					((DataGridColumnStyle)columnStyle).Dispose();
				}
				TableStyles.Clear();
			}
			foreach (DataColumn column in dataTable.Columns)
			{
				var type = column.DataType;
				DataGridColumnStyle dataGridColumnStyle;
				if (type.Equals(typeof(bool)) || type.Equals(typeof(CheckState)))
				{
					dataGridColumnStyle = new ZCheckBoxColumnStyle(new ZCheckBoxColumnStyleInfo(column.ColumnName, PreferredColumnWidth));
				}
				else
				{
					dataGridColumnStyle = new ZDataGridTextBoxColumnStyle(new ZDataGridTextBoxColumnInfo(column.ColumnName, PreferredColumnWidth));
				}
				dataGridColumnStyle.MappingName = column.ColumnName;
				dataGridColumnStyle.HeaderText = column.ColumnName;
				dataGridColumnStyle.ReadOnly = column.ReadOnly;
				dataGridColumnStyle.Alignment = HorizontalAlignment.Left;
				tableStyle.GridColumnStyles.Add(dataGridColumnStyle);
			}
			return tableStyle;
		}

		void CreateColumns(DataTable dataTable)
		{
			Columns.Clear();
			for (int c = 0; c < dataTable.Columns.Count; c++)
			{
				DataGridViewColumn dataGridViewColumn = new DataGridViewColumn(c)
				{
					Name = dataTable.Columns[c].ColumnName,
					HeaderText = dataTable.Columns[c].ColumnName,
					DataPropertyName = dataTable.Columns[c].ColumnName,
					ValueType = dataTable.Columns[c].DataType,
				};
				Columns.Add(dataGridViewColumn);
			}
		}

		void RefreshDataGridViewRow()
		{
			Rows.Clear();
			for (int r = 0; r < DataGridRowsLength; r++)
			{
				DataGridViewRow dataGridViewRow = new DataGridViewRow(r);
				for (int c = 0; c < Columns.Count; c++)
				{
					var cellValue = this[r, c];
					if (cellValue == null || string.IsNullOrEmpty(cellValue.ToString()) || DataGridRows[r] is DataGridAddNewRow)
					{
						cellValue = null;
					}
					DataGridViewCell dataGridViewCell = new DataGridViewCell(dataGridViewRow, Columns[c], r, c)
					{
						Value = cellValue,
						ValueType = Columns[c].ValueType,
					};
					dataGridViewRow.Cells.Add(dataGridViewCell);
				}
				Rows.Add(dataGridViewRow);
			}
		}

		public virtual object GetClipboardContent()
		{
			var selectionBuilder = new StringBuilder();
			var cells = SelectedCells.Cast<DataGridViewCell>().ToArray();
			if (cells.Any())
			{
				var rows = cells.Select(c => c.OwningRow).OrderBy(row => row.Index).Distinct().ToArray();
				var columns = cells.Select(owningColumn => owningColumn.OwningColumn)
					.Where(column => column.HeaderText != RowStateName)
					.Distinct()
					.OrderBy(column => column.Index)
					.ToArray();

				var delimiter = "\t";

				if (ClipboardCopyMode == DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText)
				{
					for (var c = 0; c < columns.Length; c++)
					{
						selectionBuilder.Append(columns[c].HeaderText);

						if (c < (columns.Length - 1))
						{
							selectionBuilder.Append(delimiter);
						}
					}
					selectionBuilder.AppendLine();
				}

				for (var r = 0; r < rows.Length; r++)
				{
					for (var c = 0; c < columns.Length; c++)
					{
						var cellValue = rows[r].Cells[columns[c].Index].Value ?? string.Empty;
						selectionBuilder.Append(cellValue.ToString());

						if (c < (columns.Length - 1))
						{
							selectionBuilder.Append(delimiter);
						}
					}
					selectionBuilder.AppendLine();
				}
			}
			return selectionBuilder.ToString();
		}

		protected override void Dispose(bool disposing)
		{
			DataSource = null;
			UnhookUpdateSelectedCells(ContextMenu);
			Columns.Clear();
			Rows.Clear();
			SelectedCells.Clear();
			if (TableStyles.Count > 0)
			{
				foreach (var columnStyle in TableStyles[0].GridColumnStyles)
				{
					((DataGridColumnStyle)columnStyle).Dispose();
				}
				TableStyles[0].GridColumnStyles.Clear();
				TableStyles.Clear();
			}
			base.Dispose(disposing);
		}
	}
}
