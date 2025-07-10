using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion

	public partial class MainWindow : Window, IDisposable
	{
		#region EDW Custom Tables

		BiAutomationConfigDataSet.EdwCustomTableConfigRow GetCustomTableCurrentItemRow()
		{
			BiAutomationConfigDataSet.EdwCustomTableConfigRow result = null;

			if (customTableConfigGrid.SelectedItem != null && ((DataRowView)customTableConfigGrid.SelectedItem).Row != null)
			{
				result = (BiAutomationConfigDataSet.EdwCustomTableConfigRow)((DataRowView)customTableConfigGrid.SelectedItem).Row;
				if (result.RowState == DataRowState.Detached)
				{
					result = null;
				}
			}

			return result;
		}

		void RefreshQueryTextBoxes(BiAutomationConfigDataSet.EdwCustomTableConfigRow customTable)
		{
			customTableViewTextBox.Text = customTable.CreateViewQuery;
			customTableInitialLoadTextBox.Text = customTable.InitialLoadQuery;
			customTableIncrementalLoadTextBox.Text = customTable.IncrementalLoadQuery;
			customTableCustomIndexTextBox.Text = customTable.CustomIndex;
		}

		void customTableViewTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var currentItem = GetCustomTableCurrentItemRow();
			if (currentItem != null)
			{
				currentItem.CreateViewQuery = customTableViewTextBox.Text;
			}
		}

		void customTableInitialLoadTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var currentItem = GetCustomTableCurrentItemRow();
			if (currentItem != null)
			{
				currentItem.InitialLoadQuery = customTableInitialLoadTextBox.Text;
			}
		}

		void customTableIncrementalLoadTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var currentItem = GetCustomTableCurrentItemRow();
			if (currentItem != null)
			{
				currentItem.IncrementalLoadQuery = customTableIncrementalLoadTextBox.Text;
			}
		}

		void customTableConfigGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var currentItem = GetCustomTableCurrentItemRow();
			if (currentItem != null)
			{
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void customTableConfigGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetCustomTableCurrentItemRow();
			if (currentItem != null)
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SortCustomTableDependencyOrder();
				customColumnConfigGrid.ItemsSource =
							new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomColumnConfig,
								"TableName = '" + currentItem.Name + "'",
								"Name",
								DataViewRowState.CurrentRows);
				RefreshQueryTextBoxes(currentItem);
			}
		}

		void addCustomTableButton_Click(object sender, RoutedEventArgs e)
		{
			int counter = 1;
			while (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig.Any(t => t.Name == string.Format(CultureInfo.InvariantCulture, "NewTable({0})", counter)))
			{
				counter++;
			}

			var dependencyOrder = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig.Max(t => t.DependencyOrder) + 1;

			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("", string.Format(CultureInfo.InvariantCulture, "NewTable({0})", counter), dependencyOrder, "", "", "", "", false, "");
			customTableConfigGrid.Items.Refresh();
		}

		void deleteCustomTableButton_Click(object sender, RoutedEventArgs e)
		{
			if (customTableConfigGrid.SelectedItem != null)
			{
				customTableConfigGrid.SelectionChanged -= customTableConfigGrid_SelectionChanged;

				var selectedItem = (BiAutomationConfigDataSet.EdwCustomTableConfigRow)((DataRowView)customTableConfigGrid.SelectedItem).Row;
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig.RemoveEdwCustomTableConfigRow(selectedItem);
				customTableConfigGrid.Items.Refresh();

				customTableConfigGrid.SelectionChanged += customTableConfigGrid_SelectionChanged;
			}
		}

		void addCustomColumnButton_Click(object sender, RoutedEventArgs e)
		{
			int counter = 1;
			var customTable = GetCustomTableCurrentItemRow();
			var columns = customTable.GetEdwCustomColumnConfigRows();
			while (columns.Any(c => c.Name == string.Format(CultureInfo.InvariantCulture, "__NewColumn({0})", counter)))
			{
				counter++;
			}
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomColumnConfig.AddEdwCustomColumnConfigRow(customTable, string.Format(CultureInfo.InvariantCulture, "__NewColumn({0})", counter), "varchar", 100, 0, 0, "");
			customColumnConfigGrid.Items.Refresh();
		}

		void deleteCustomColumnButton_Click(object sender, RoutedEventArgs e)
		{
			var selectedItems = new List<BiAutomationConfigDataSet.EdwCustomColumnConfigRow>();

			if (customColumnConfigGrid.SelectedItems.Count > 0)
			{
				foreach (DataRowView selectedItem in customColumnConfigGrid.SelectedItems)
				{
					selectedItems.Add((BiAutomationConfigDataSet.EdwCustomColumnConfigRow)(selectedItem.Row));
				}
			}
			else if (customColumnConfigGrid.SelectedCells.Any())
			{
				foreach (DataGridCellInfo selectedCell in customColumnConfigGrid.SelectedCells)
				{
					var selectedItem = (BiAutomationConfigDataSet.EdwCustomColumnConfigRow)((DataRowView)selectedCell.Item).Row;
					if (!selectedItems.Contains(selectedItem))
					{
						selectedItems.Add(selectedItem);
					}
				}
			}

			foreach (var selectedItem in selectedItems)
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomColumnConfig.RemoveEdwCustomColumnConfigRow(selectedItem);
			}

			customColumnConfigGrid.Items.Refresh();
		}

		void TestCustomTableQueryButton_Click(object sender, RoutedEventArgs e)
		{
			Invoke(() =>
			{
				BiFiles.CWSharedPath = CWSharedPath;
				string source = null;
				string query = null;
				string operation = null;

				var customTable = GetCustomTableCurrentItemRow();
				source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", customTable.Schema, customTable.Name);

				if (customTableViewTab.IsSelected)
				{
					query = string.Format(CultureInfo.InvariantCulture,
@"IF EXISTS
	(SELECT NULL FROM sys.views v INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
		WHERE v.name = '{0}' AND s.name = '{1}')
	DROP VIEW [{1}].[{0}]

GO

{2}

GO", customTable.ViewName, customTable.Schema, customTableViewTextBox.Text);
					operation = "Create View";
				}
				else if (customTableInitialLoadTab.IsSelected)
				{
					query = customTableInitialLoadTextBox.Text;
					operation = "Initial Load";
				}
				else if (customTableIncrementalLoadTab.IsSelected)
				{
					query = customTableIncrementalLoadTextBox.Text;
					operation = "Incremental Load";
				}

				if (!string.IsNullOrWhiteSpace(query))
				{
					var errorMsg = SchemaSynchroniser.TestSingleQuery(source, query, operation);
					if (string.IsNullOrEmpty(errorMsg))
					{
						BiLogger.Complete("Query executed without errors.");
					}
					else
					{
						BiLogger.Fail(errorMsg);
					}
				}
				else
				{
					BiLogger.StartTask("No query to test.");
				}
			});
		}

		void CopyCustomTableQueryToClipboard_Click(object sender, RoutedEventArgs e)
		{
			if (customTableViewTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(customTableViewTextBox);
			}
			else if (customTableInitialLoadTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(customTableInitialLoadTextBox);
			}
			else if (customTableIncrementalLoadTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(customTableIncrementalLoadTextBox);
			}
		}

		void SaveCustomIndexForCustomTableButton_Click(object sender, RoutedEventArgs e)
		{
			Invoke(() =>
			{
				BiFiles.CWSharedPath = CWSharedPath;
				var query = customTableCustomIndexTextBox.Text;
				var customTable = GetCustomTableCurrentItemRow();
				if (!customTable.HasErrors)
				{
					if (ValidateCustomIndex(query, $"{customTable.Schema}.{customTable.Name}"))
					{
						customTable.CustomIndex = query;
						BiLogger.Complete("Custom index saved successfully.");
					}
					else
					{
						BiLogger.Fail("Validation failed. Please check your input.");
					}
				}
				else
				{
					BiLogger.Fail("Save failed. Check EDW custom table for errors.");
				}
			});
		}

		void EdwCustomTableQueryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem)
			{
				SaveCustomTableCustomIndexButton.IsEnabled = false;
				SaveCustomTableCustomIndexButton.Visibility = Visibility.Collapsed;
				TestCustomTableQueryButton.IsEnabled = true;
				TestCustomTableQueryButton.Visibility = Visibility.Visible;

				var tabItem = (TabItem)e.AddedItems[0];
				if ((string)tabItem.Header == (string)customTableCustomIndexTab.Header)
				{
					var currentItem = GetCustomTableCurrentItemRow();
					if (!currentItem.HasErrors)
					{
						SaveCustomTableCustomIndexButton.IsEnabled = true;
						SaveCustomTableCustomIndexButton.Visibility = Visibility.Visible;
						TestCustomTableQueryButton.IsEnabled = false;
						TestCustomTableQueryButton.Visibility = Visibility.Collapsed;
					}
				}
			}
		}

		#endregion
	}

	#endregion
}
