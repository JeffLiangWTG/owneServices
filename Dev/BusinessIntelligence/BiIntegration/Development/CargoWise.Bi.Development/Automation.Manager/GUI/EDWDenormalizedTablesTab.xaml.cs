using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Development.Common;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
	public partial class MainWindow : Window, IDisposable
	{
		#region EDW Denormalized Tables

		BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow GetDenormalizedTableCurrentItemRow()
		{
			BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow result = null;

			if (denormTableConfigGrid.SelectedItem != null && ((DataRowView)denormTableConfigGrid.SelectedItem).Row != null)
			{
				result = (BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow)((DataRowView)denormTableConfigGrid.SelectedItem).Row;
				if (result.RowState == DataRowState.Detached)
				{
					result = null;
				}
			}

			return result;
		}

		void RefreshQueryTextBoxes(BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow denormTable)
		{
			createModelViewForDenormTextBox.Text = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetCreateDenormalizedTableViewQuery(denormTable);
			initialLoadQueryForDenormTextBox.Text = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetInitialLoadQueryForDenormalizedTable(denormTable);
			incrementalLoadQueryForDenormTextBox.Text = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetIncrementalLoadQueryForDenormalizedTable(denormTable);
			customIndexQueryForDenormTextBox.Text = denormTable.CustomIndex;
		}

		void denormTableConfigGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var currentItem = GetDenormalizedTableCurrentItemRow();
			if (currentItem != null)
			{
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void denormTableConfigGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetDenormalizedTableCurrentItemRow();
			if (currentItem != null)
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SortAggregateTableDependencyOrder();
				denormColumnConfigGrid.ItemsSource =
							new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedColumnConfig,
								"TableName = '" + currentItem.Name + "'",
								"Name",
								DataViewRowState.CurrentRows);
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void deleteDenormalizedTableButton_Click(object sender, RoutedEventArgs e)
		{
			if (denormTableConfigGrid.SelectedItem != null)
			{
				denormTableConfigGrid.SelectionChanged -= denormTableConfigGrid_SelectionChanged;

				var selectedItem = (BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow)((DataRowView)denormTableConfigGrid.SelectedItem).Row;
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig.RemoveEdwDenormalizedTableConfigRow(selectedItem);
				denormTableConfigGrid.Items.Refresh();

				denormTableConfigGrid.SelectionChanged += denormTableConfigGrid_SelectionChanged;
			}
		}

		void deleteDenormalizedColumnButton_Click(object sender, RoutedEventArgs e)
		{
			var selectedItems = new List<BiAutomationConfigDataSet.EdwDenormalizedColumnConfigRow>();

			if (denormColumnConfigGrid.SelectedItems.Count > 0)
			{
				foreach (DataRowView selectedItem in denormColumnConfigGrid.SelectedItems)
				{
					selectedItems.Add((BiAutomationConfigDataSet.EdwDenormalizedColumnConfigRow)(selectedItem.Row));
				}
			}
			else if (denormColumnConfigGrid.SelectedCells.Any())
			{
				foreach (DataGridCellInfo selectedCell in denormColumnConfigGrid.SelectedCells)
				{
					var selectedItem = (BiAutomationConfigDataSet.EdwDenormalizedColumnConfigRow)((DataRowView)selectedCell.Item).Row;
					if (!selectedItems.Contains(selectedItem))
					{
						selectedItems.Add(selectedItem);
					}
				}
			}

			foreach (var selectedItem in selectedItems)
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedColumnConfig.RemoveEdwDenormalizedColumnConfigRow(selectedItem);
			}

			denormColumnConfigGrid.Items.Refresh();
		}

		void denormColumnConfigGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetDenormalizedTableCurrentItemRow();
			if (currentItem != null)
			{
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void denormColumnConfigGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var currentItem = GetDenormalizedTableCurrentItemRow();
			if (currentItem != null)
			{
				RefreshQueryTextBoxes(currentItem);
			}
		}

		#endregion
	}
	#endregion
}
