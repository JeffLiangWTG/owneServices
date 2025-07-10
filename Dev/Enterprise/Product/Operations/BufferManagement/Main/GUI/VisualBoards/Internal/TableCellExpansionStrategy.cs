using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	class TableCellExpansionStrategy
	{
		readonly TableLayoutPanel table;
		readonly BMBoardSectionViewModel viewModel;

		public TableCellExpansionStrategy(TableLayoutPanel table, BMBoardSectionViewModel viewModel)
		{
			this.table = table;
			this.viewModel = viewModel;
		}

		public void ExpandOrCollapseCell(CellContent cell, ExpansionMode expansionMode = ExpansionMode.Toggle)
		{
			ExpandOrCollapseCells(new List<CellContent>(1) { cell }, expansionMode);
		}

		public void ExpandOrCollapseCells(IEnumerable<CellContent> cells, ExpansionMode expansionMode = ExpansionMode.Toggle)
		{
			var columnsToExpandOrCollapse = new Dictionary<int, ExpansionMode>();
			var rowsToExpandOrCollapse = new Dictionary<int, ExpansionMode>();
			expansionMode = expansionMode == ExpansionMode.Default ? ExpansionMode.Toggle : expansionMode;
			var firstCardCell = viewModel.ComponentGrid.Cells.FirstOrDefault(x => x.ContentType == CellContentType.Cards);

			foreach (var cell in cells)
			{
				if (cell != null)
				{
					var overriddenExpansionMode = ExpansionMode.Default;
					var control = table.GetControlFromPosition(cell.Column, cell.Row);
					if (control != null)
					{
						var isAffectingColumns = cell.Row == 0;

						if (firstCardCell != null)
						{
							if (viewModel.Orientation == BMBoardSectionOrientation.Vertical)
							{
								isAffectingColumns = cell.Row < firstCardCell.Row;
							}
							else
							{
								isAffectingColumns = cell.Column >= firstCardCell.Column;
							}
						}

						if (isAffectingColumns)
						{
							var span = table.GetColumnSpan(control);

							if (expansionMode == ExpansionMode.Toggle && span > 1)
							{
								overriddenExpansionMode = IsColumnExpanded(cell.Column) ? ExpansionMode.CollapseIfNotCollapsed : ExpansionMode.ExpandIfNotExpanded;
							}

							for (var col = cell.Column; col < cell.Column + span; col++)
							{
								if (!columnsToExpandOrCollapse.ContainsKey(col))
								{
									columnsToExpandOrCollapse.Add(col, overriddenExpansionMode);
								}
							}
						}
						else
						{
							var span = table.GetRowSpan(control);

							if (expansionMode == ExpansionMode.Toggle && span > 1)
							{
								overriddenExpansionMode = IsRowExpanded(cell.Row) ? ExpansionMode.CollapseIfNotCollapsed : ExpansionMode.ExpandIfNotExpanded;
							}

							for (var row = cell.Row; row < cell.Row + span; row++)
							{
								if (!rowsToExpandOrCollapse.ContainsKey(row))
								{
									rowsToExpandOrCollapse.Add(row, overriddenExpansionMode);
								}
							}
						}
					}
				}
				else
				{
					ErrorReporter.ReportOnce("Should not pass in null cells");
				}
			}

			ExpandOrCollapseCells(columnsToExpandOrCollapse, rowsToExpandOrCollapse, expansionMode);
		}

		void ExpandOrCollapseCells(Dictionary<int, ExpansionMode> columnsToExpandOrCollapse, Dictionary<int, ExpansionMode> rowsToExpandOrCollapse, ExpansionMode expansionMode)
		{
			foreach (var column in columnsToExpandOrCollapse.Where(x => ShouldExpandOrCollapseColumn(x.Key, x.Value, expansionMode)))
			{
				ExpandOrCollapseColumn(column.Key);
			}

			foreach (var row in rowsToExpandOrCollapse.Where(x => ShouldExpandOrCollapseRow(x.Key, x.Value, expansionMode)))
			{
				ExpandOrCollapseRow(row.Key);
			}
		}

		bool ShouldExpandOrCollapseColumn(int column, ExpansionMode overriddenExpansionMode, ExpansionMode defaultExpansionMode)
		{
			return (defaultExpansionMode == ExpansionMode.Toggle && overriddenExpansionMode == ExpansionMode.Default) ||
				((defaultExpansionMode == ExpansionMode.ExpandIfNotExpanded || overriddenExpansionMode == ExpansionMode.ExpandIfNotExpanded) && !IsColumnExpanded(column)) ||
				((defaultExpansionMode == ExpansionMode.CollapseIfNotCollapsed || overriddenExpansionMode == ExpansionMode.CollapseIfNotCollapsed) && IsColumnExpanded(column));
		}

		bool ShouldExpandOrCollapseRow(int row, ExpansionMode overriddenExpansionMode, ExpansionMode defaultExpansionMode)
		{
			return (defaultExpansionMode == ExpansionMode.Toggle && overriddenExpansionMode == ExpansionMode.Default) ||
				((defaultExpansionMode == ExpansionMode.ExpandIfNotExpanded || overriddenExpansionMode == ExpansionMode.ExpandIfNotExpanded) && !IsRowExpanded(row)) ||
				((defaultExpansionMode == ExpansionMode.CollapseIfNotCollapsed || overriddenExpansionMode == ExpansionMode.CollapseIfNotCollapsed) && IsRowExpanded(row));
		}

		bool IsColumnExpanded(int column)
		{
			var columnStyle = table.ColumnStyles[column];
			return columnStyle.Width > viewModel.GetOriginalColumnWidth(column);
		}

		bool IsRowExpanded(int row)
		{
			var rowStyle = table.RowStyles[row];
			return rowStyle.Height > viewModel.GetOriginalRowHeight(row);
		}

		void ExpandOrCollapseColumn(int column)
		{
			var columnStyle = table.ColumnStyles[column];

			if (IsColumnExpanded(column))
			{
				columnStyle.Width = viewModel.GetOriginalColumnWidth(column);
			}
			else
			{
				columnStyle.Width *= BMConstants.ChannelExpansionFactor;
			}

			SetupColumnTasks(column);
		}

		void ExpandOrCollapseRow(int row)
		{
			var rowStyle = table.RowStyles[row];

			if (IsRowExpanded(row))
			{
				rowStyle.Height = viewModel.GetOriginalRowHeight(row);
			}
			else
			{
				rowStyle.Height *= BMConstants.ChannelExpansionFactor;
			}

			SetupRowTasks(row);
		}

		void SetupRowTasks(int row)
		{
			for (var col = 0; col < table.ColumnCount; col++)
			{
				SetupTasks(row, col);
			}

			InvalidateAllPanels();
		}

		void SetupColumnTasks(int col)
		{
			for (var row = 0; row < table.RowCount; row++)
			{
				SetupTasks(row, col);
			}

			InvalidateAllPanels();
		}

		void SetupTasks(int row, int col)
		{
			var control = table.GetControlFromPosition(col, row) as ITasksControl;
			if (control != null)
			{
				control.SetupTasks(disposeExistingTickets: false);
			}
		}

		void InvalidateAllPanels()
		{
			foreach (Control control in table.Controls)
			{
				control.Invalidate();
			}
		}

		public enum ExpansionMode
		{
			Default = 0,
			Toggle = 1,
			ExpandIfNotExpanded = 2,
			CollapseIfNotCollapsed = 3
		}
	}
}
