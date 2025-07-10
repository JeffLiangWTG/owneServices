using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Bi.Common;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
	public partial class MainWindow : Window, IDisposable
	{
		#region EDW Model View

		BiAutomationConfigDataSet.EdwModelViewTableConfigRow GetModelViewTableCurrentItemRow()
		{
			BiAutomationConfigDataSet.EdwModelViewTableConfigRow result = null;

			if (modelViewTableConfigGrid.SelectedItem != null && ((DataRowView)modelViewTableConfigGrid.SelectedItem).Row != null)
			{
				result = (BiAutomationConfigDataSet.EdwModelViewTableConfigRow)((DataRowView)modelViewTableConfigGrid.SelectedItem).Row;
				if (result.RowState == DataRowState.Detached)
				{
					result = null;
				}
			}

			return result;
		}

		void RefreshQueryTextBoxes(BiAutomationConfigDataSet.EdwModelViewTableConfigRow modelView)
		{
			createModelViewTextBox.Text = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetCreateModelViewQuery(modelView);
		}

		void edwModelTableConfigGrid2_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetEdwTableConfigCurrentItemRow();
			if (currentItem != null)
			{
				edwModelColumnConfigGrid2.ItemsSource =
							new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig,
								"TableName = '" + currentItem.Name + "'",
								"Name",
								DataViewRowState.CurrentRows);
			}
		}

		void modelViewTableConfigGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetModelViewTableCurrentItemRow();
			if (currentItem != null)
			{
				modelViewColumnConfigGrid.ItemsSource =
							new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewColumnConfig,
								"ViewName = '" + currentItem.Name + "'",
								"Name",
								DataViewRowState.CurrentRows);
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void modelViewTableConfigGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var currentItem = GetModelViewTableCurrentItemRow();
			if (currentItem != null)
			{
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void modelViewColumnConfigGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetModelViewTableCurrentItemRow();
			if (currentItem != null)
			{
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void modelViewColumnConfigGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var currentItem = GetModelViewTableCurrentItemRow();
			if (currentItem != null)
			{
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void addModelViewTableButton_Click(object sender, RoutedEventArgs e)
		{
			int counter = 1;
			while (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig.Any(t => t.Name == string.Format(CultureInfo.InvariantCulture, "{0}NewTable({1})", BiConstants.EdwModelViewPrefix, counter)))
			{
				counter++;
			}
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig.AddEdwModelViewTableConfigRow("", string.Format(CultureInfo.InvariantCulture, "{0}NewTable({1})", BiConstants.EdwModelViewPrefix, counter), "", "", "");
			modelViewTableConfigGrid.Items.Refresh();
		}

		void deleteModelViewTableButton_Click(object sender, RoutedEventArgs e)
		{
			if (modelViewTableConfigGrid.SelectedItem != null)
			{
				var selectedItem = (BiAutomationConfigDataSet.EdwModelViewTableConfigRow)((DataRowView)modelViewTableConfigGrid.SelectedItem).Row;
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig.RemoveEdwModelViewTableConfigRow(selectedItem);
				modelViewTableConfigGrid.Items.Refresh();
			}
		}

		void addModelViewColumnButton_Click(object sender, RoutedEventArgs e)
		{
			int counter = 1;
			var modelView = GetModelViewTableCurrentItemRow();
			var columns = modelView.GetEdwModelViewColumnConfigRows();
			while (columns.Any(c => c.Name == string.Format(CultureInfo.InvariantCulture, "__NewColumn({0})", counter)))
			{
				counter++;
			}
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewColumnConfig.AddEdwModelViewColumnConfigRow(modelView, string.Format(CultureInfo.InvariantCulture, "__NewColumn({0})", counter), "", "", "");
			modelViewColumnConfigGrid.Items.Refresh();
			RefreshQueryTextBoxes(modelView);
		}

		void deleteModelViewColumnButton_Click(object sender, RoutedEventArgs e)
		{
			var selectedItems = new List<BiAutomationConfigDataSet.EdwModelViewColumnConfigRow>();

			if (modelViewColumnConfigGrid.SelectedItems.Count > 0)
			{
				foreach (DataRowView selectedItem in modelViewColumnConfigGrid.SelectedItems)
				{
					selectedItems.Add((BiAutomationConfigDataSet.EdwModelViewColumnConfigRow)(selectedItem.Row));
				}
			}
			else if (modelViewColumnConfigGrid.SelectedCells.Any())
			{
				foreach (DataGridCellInfo selectedCell in modelViewColumnConfigGrid.SelectedCells)
				{
					var selectedItem = (BiAutomationConfigDataSet.EdwModelViewColumnConfigRow)((DataRowView)selectedCell.Item).Row;
					if (!selectedItems.Contains(selectedItem))
					{
						selectedItems.Add(selectedItem);
					}
				}
			}

			if (selectedItems.Any())
			{
				var modelView = selectedItems[0].GetModelView();

				foreach (var selectedItem in selectedItems)
				{
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewColumnConfig.RemoveEdwModelViewColumnConfigRow(selectedItem);
				}

				modelViewColumnConfigGrid.Items.Refresh();
				RefreshQueryTextBoxes(modelView);
			}
		}

		void populateModelViewColumnsButton_Click(object sender, RoutedEventArgs e)
		{
			var modelView = GetModelViewTableCurrentItemRow();
			SchemaSynchroniser.PopulateModelViewColumns(modelView);
			modelView.Validate();
		}

		void denormTableConfigGrid2_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetDenormalizedTableCurrentItemRow();
			if (currentItem != null)
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SortAggregateTableDependencyOrder();
				denormColumnConfigGrid2.ItemsSource =
							new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedColumnConfig,
								"TableName = '" + currentItem.Name + "'",
								"Name",
								DataViewRowState.CurrentRows);
			}
		}

		void edwModelTableConfigGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetEdwTableConfigCurrentItemRow();
			if (currentItem != null)
			{
				edwModelColumnConfigGrid.ItemsSource =
							new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig,
								"TableName = '" + currentItem.Name + "'",
								"Name",
								DataViewRowState.CurrentRows);
			}
		}

		void copyModelViewColumns_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(modelViewColumnConfigGrid);
		}

		#endregion
	}
	#endregion
}
