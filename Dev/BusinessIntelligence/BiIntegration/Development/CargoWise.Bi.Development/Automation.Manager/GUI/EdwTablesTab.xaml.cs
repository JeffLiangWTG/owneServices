using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Bi.Common;
using CargoWise.Bi.Configuration;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync;
using CargoWise.Common;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
	public partial class MainWindow : Window, IDisposable
	{
		#region EDW Tables

		BiAutomationConfigDataSet.EdwTableConfigRow GetEdwTableConfigCurrentItemRow()
		{
			BiAutomationConfigDataSet.EdwTableConfigRow result = null;

			if (edwTableConfigGrid.SelectedItem != null && ((DataRowView)edwTableConfigGrid.SelectedItem).Row != null)
			{
				result = (BiAutomationConfigDataSet.EdwTableConfigRow)((DataRowView)edwTableConfigGrid.SelectedItem).Row;
				if (result.RowState == DataRowState.Detached)
				{
					result = null;
				}
			}

			return result;
		}

		BiAutomationConfigDataSet.EdwColumnConfigRow GetEdwColumnConfigCurrentItemRow()
		{
			BiAutomationConfigDataSet.EdwColumnConfigRow result = null;

			if (edwColumnConfigGrid.SelectedItem != null && ((DataRowView)edwColumnConfigGrid.SelectedItem).Row != null)
			{
				result = (BiAutomationConfigDataSet.EdwColumnConfigRow)((DataRowView)edwColumnConfigGrid.SelectedItem).Row;
				if (result.RowState == DataRowState.Detached)
				{
					result = null;
				}
			}

			return result;
		}

		readonly string[] edwDataTypeArray = new[] { "bigint", "binary", "bit", "char", "date", "datetime", "datetimeoffset", "decimal", "geography", "image", "int", "money", "nvarchar", "smalldatetime", "smallint", "tinyint", "uniqueidentifier", "varbinary", "varchar" };
		readonly string[] keepAsArray = new[] { "", "RefValue1", "RefValue2", "RefValue3", "RefValue4", "RefValue5" };

		void ReorderEdwTableTransformId(string tableName)
		{
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.SuspendValidation();
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.SuspendValidation();

			var transformId = 1;
			foreach (var edwTable in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => t.Name == tableName).OrderBy(t => transformId))
			{
				if (edwTable.TransformId != transformId)
				{
					edwTable.TransformId = transformId++;
				}
				else
				{
					transformId++;
				}
			}

			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.ResumeValidation();
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.ResumeValidation();
		}

		string modifiedRowName;

		void RefreshQueryTextBoxes(BiAutomationConfigDataSet.EdwTableConfigRow edwTable)
		{
			edwTable.Validate();
			initialLoadQueryForBaseTextBox.Text = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetInitialLoadQueryForEdwTable(edwTable);
			customIndexQueryForBaseTextBox.Text = edwTable.CustomIndex;
			deleteQueryTextBox.Text = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetIncrementalDeleteQueryForEdwTable(edwTable);
			insertQueryTextBox.Text = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetIncrementalInsertQueryForEdwTable(edwTable);
			if (edwTable.HasErrors)
			{
				QueryButton.IsEnabled = false;
			}
			else
			{
				QueryButton.IsEnabled = true;
			}
		}

		void edwTableConfigGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			var currentItem = GetEdwTableConfigCurrentItemRow();
			if (currentItem != null)
			{
				modifiedRowName = currentItem.Name;
			}
		}

		void edwTableConfigGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var currentItem = GetEdwTableConfigCurrentItemRow();
			if (currentItem != null)
			{
				if (modifiedRowName != currentItem.Name)
				{
					if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Count(t => t.Name == currentItem.Name && t.TransformId == currentItem.TransformId) > 1)
					{
						BiLogger.Fail(string.Format(CultureInfo.InvariantCulture, "Table '{0}' already exists. Choose another name.", currentItem.Name));
						currentItem.Name = modifiedRowName;
					}
					else
					{
						BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EnforceConstraints = false;
						BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.SuspendValidation();

						foreach (var column in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.Where(c => c.TableName == modifiedRowName && c.TransformId == currentItem.TransformId))
						{
							column.TableName = currentItem.Name;
							if (column.Name == modifiedRowName.Replace(BiConstants.EdwBaseTablePrefix, "") + "Key")
							{
								column.Name = currentItem.BaseName + "Key";
							}
							else if (column.Name == modifiedRowName.Replace(BiConstants.EdwBaseTablePrefix, "") + "ID")
							{
								column.Name = currentItem.BaseName + "ID";
							}
						}

						BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.AcceptChanges();
						BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.ResumeValidation();
						BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EnforceConstraints = true;

						ReorderEdwTableTransformId(modifiedRowName);
						ReorderEdwTableTransformId(currentItem.Name);
					}
				}

				RefreshQueryTextBoxes(currentItem);
			}
		}

		void cdcTableConfigGrid_SelectionChanged(object sender, EventArgs e)
		{
			var currentItem = GetTableConfigCurrentItemRow();
			if (currentItem != null)
			{
				cdcColumnConfigGrid.ItemsSource =
							new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig,
								"SourceTable = '" + currentItem.SourceTable + "'",
								"SourceColumn",
								DataViewRowState.CurrentRows);
			}

			if (e != null)
			{
				var addedItems = ((SelectionChangedEventArgs)e).AddedItems;
				if (addedItems != null && addedItems.Count > 0)
				{
					var tableName = ((DataRowView)addedItems[0]).Row["SourceTable"].ToString();
					ColumnSearchParameters.RefreshOptions(tableName);
					columnSearchGrid.Items.Refresh();
				}
			}

			RecalculateCdcColumnStats();
		}

		void CdcTableConfig_ColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
			Invoke(() =>
			{
				configSavingStatus.Content = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.HasChanges() ? "Unsaved Changes" : "No Changes";
			});
		}

		void CdcColumnConfig_ColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
			Invoke(() =>
			{
				configSavingStatus.Content = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.HasChanges() ? "Unsaved Changes" : "No Changes";
			});
		}

		BiAutomationConfigDataSet.CdcTableConfigRow GetTableConfigCurrentItemRow()
		{
			BiAutomationConfigDataSet.CdcTableConfigRow result = null;

			if (cdcTableConfigGrid.SelectedItem != null && ((DataRowView)cdcTableConfigGrid.SelectedItem).Row != null)
			{
				result = (BiAutomationConfigDataSet.CdcTableConfigRow)((DataRowView)cdcTableConfigGrid.SelectedItem).Row;
				if (result.RowState == DataRowState.Detached)
				{
					result = null;
				}
			}

			return result;
		}

		void edwTableConfigGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetEdwTableConfigCurrentItemRow();
			if (currentItem != null)
			{
				var readonlyHeaders = new[] { "Model Column Name", "Data Type", "Max Length", "Precision", "Scale", "Is Partition Key" };
				if (currentItem.TransformId != 1)
				{
					foreach (var column in edwColumnConfigGrid.Columns.Where(c => readonlyHeaders.Contains(c.Header)))
					{
						column.IsReadOnly = true;
					}

					populateEdwColumnsButton.IsEnabled = false;
					addEdwColumnButton.IsEnabled = false;
					deleteEdwColumnButton.IsEnabled = false;
				}
				else
				{
					foreach (var column in edwColumnConfigGrid.Columns.Where(c => readonlyHeaders.Contains(c.Header)))
					{
						column.IsReadOnly = false;
					}

					populateEdwColumnsButton.IsEnabled = true;
					addEdwColumnButton.IsEnabled = true;
					deleteEdwColumnButton.IsEnabled = true;
				}

				edwColumnConfigGrid.ItemsSource =
					new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig,
						"TableName = '" + currentItem.Name + "' AND TransformId = '" + currentItem.TransformId + "'",
						"Name",
						DataViewRowState.CurrentRows);

				RefreshQueryTextBoxes(currentItem);

				foreach (DataRowView item in cdcTableConfigGrid.Items)
				{
					var cdcTable = (BiAutomationConfigDataSet.CdcTableConfigRow)item.Row;
					if (cdcTable.SourceTable == currentItem.StagingTable)
					{
						cdcTableConfigGrid.SelectedItem = item;
						break;
					}
				}
			}
		}

		void addEdwTableButton_Click(object sender, RoutedEventArgs e)
		{
			int counter = 1;
			while (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Any(t => t.Name == string.Format(CultureInfo.InvariantCulture, "{0}NewTable({1})", BiConstants.EdwBaseTablePrefix, counter)))
			{
				counter++;
			}
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.AddEdwTableConfigRow("", string.Format(CultureInfo.InvariantCulture, "{0}NewTable({1})", BiConstants.EdwBaseTablePrefix, counter), "", "dbo", "", "", 1, false, -1, "");
			edwTableConfigGrid.Items.Refresh();
			edwColumnConfigGrid.Items.Refresh();
		}

		void deleteEdwTableButton_Click(object sender, RoutedEventArgs e)
		{
			if (edwTableConfigGrid.SelectedItem != null)
			{
				edwTableConfigGrid.SelectionChanged -= edwTableConfigGrid_SelectionChanged;
				var selectedItem = (BiAutomationConfigDataSet.EdwTableConfigRow)((DataRowView)edwTableConfigGrid.SelectedItem).Row;

				var transformCount = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Count(t => t.Name == selectedItem.Name);
				var referencingTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => t.GetEdwColumnConfigRows().Any(c => c.ParentTable == selectedItem.Name)).FirstOrDefault();
				if (transformCount == 1 && referencingTable != null)
				{
					BiLogger.Fail(string.Format(CultureInfo.InvariantCulture, "Cannot delete model table [{0}]. It is referenced by model table [{1}].", selectedItem.Name, referencingTable.Name));
				}
				else
				{
					var tableName = selectedItem.Name;
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.RemoveEdwTableConfigRow(selectedItem);
					edwTableConfigGrid.Items.Refresh();
					ReorderEdwTableTransformId(tableName);
				}

				edwTableConfigGrid.SelectionChanged += edwTableConfigGrid_SelectionChanged;
			}
		}

		void populateEdwColumnsButton_Click(object sender, RoutedEventArgs e)
		{
			var edwTable = GetEdwTableConfigCurrentItemRow();
			SchemaSynchroniser.PopulateEdwColumns(edwTable);
			edwTable.Validate();
		}

		void cloneButton_Click(object sender, RoutedEventArgs e)
		{
			var edwTable = GetEdwTableConfigCurrentItemRow();
			if (!edwTable.HasErrors)
			{
				var cloneTable = SchemaSynchroniser.CloneEdwTable(edwTable);

				foreach (DataRowView item in edwTableConfigGrid.Items)
				{
					var table = (BiAutomationConfigDataSet.EdwTableConfigRow)item.Row;
					if (table.Name == cloneTable.Name && table.TransformId == cloneTable.TransformId)
					{
						edwTableConfigGrid.SelectedItem = item;
						break;
					}
				}
			}
			else
			{
				BiLogger.Fail("Cannot clone EDW table with errors.");
			}
		}

		void addEdwColumnButton_Click(object sender, RoutedEventArgs e)
		{
			int counter = 1;
			var edwTable = GetEdwTableConfigCurrentItemRow();
			var edwColumns = edwTable.GetEdwColumnConfigRows();
			while (edwColumns.Any(c => c.Name == string.Format(CultureInfo.InvariantCulture, "__NewColumn({0})", counter)))
			{
				counter++;
			}
			foreach (var table in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => t.Name == edwTable.Name))
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(table.Name, table.TransformId, "varchar", 100, 0, 0, string.Format(CultureInfo.InvariantCulture, "__NewColumn({0})", counter), "", false, false, "", "", "", "", "", "", false);
			}
			edwColumnConfigGrid.Items.Refresh();
		}

		void deleteEdwColumnButton_Click(object sender, RoutedEventArgs e)
		{
			var selectedItems = new List<BiAutomationConfigDataSet.EdwColumnConfigRow>();

			if (edwColumnConfigGrid.SelectedItems.Count > 0)
			{
				foreach (DataRowView selectedItem in edwColumnConfigGrid.SelectedItems)
				{
					selectedItems.Add((BiAutomationConfigDataSet.EdwColumnConfigRow)(selectedItem.Row));
				}
			}
			else if (edwColumnConfigGrid.SelectedCells.Any())
			{
				foreach (DataGridCellInfo selectedCell in edwColumnConfigGrid.SelectedCells)
				{
					var selectedItem = (BiAutomationConfigDataSet.EdwColumnConfigRow)((DataRowView)selectedCell.Item).Row;
					if (!selectedItems.Contains(selectedItem))
					{
						selectedItems.Add(selectedItem);
					}
				}
			}

			var shouldRefresh = false;
			foreach (var selectedItem in selectedItems)
			{
				if (deleteEdwColumn(selectedItem))
				{
					shouldRefresh = true;
				}
			}

			if (shouldRefresh)
			{
				edwColumnConfigGrid.Items.Refresh();

				try
				{
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.Validate();
				}
				catch (BiConfigurationException ex)
				{
					BiLogger.Fail(ex.Message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					BiLogger.Fail(ex);
				}
			}
		}

		bool deleteEdwColumn(BiAutomationConfigDataSet.EdwColumnConfigRow selectedItem)
		{
			var isDeleted = false;
			var referencingColumn = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.Where(c => c.ParentColumn == selectedItem.Name && c.ParentTable == selectedItem.GetEdwTable().Name).FirstOrDefault();

			if (selectedItem.Name == selectedItem.GetEdwTable().BaseName + "ID")
			{
				BiLogger.Fail(string.Format(CultureInfo.InvariantCulture, "Cannot delete primary key column [{0}] for model table [{1}].", selectedItem.Name, selectedItem.GetEdwTable().Name));
			}
			else if (selectedItem.Name == selectedItem.GetEdwTable().BaseName + "Key")
			{
				BiLogger.Fail(string.Format(CultureInfo.InvariantCulture, "Cannot delete key column [{0}] for model table [{1}].", selectedItem.Name, selectedItem.GetEdwTable().Name));
			}
			else if (referencingColumn != null)
			{
				BiLogger.Fail(string.Format(CultureInfo.InvariantCulture, "Cannot delete model column [{0}].[{1}]. It is referenced by model column [{2}].[{3}].", selectedItem.GetEdwTable().Name, selectedItem.Name, referencingColumn.GetEdwTable().Name, referencingColumn.Name));
			}
			else
			{
				var edwTable = GetEdwTableConfigCurrentItemRow();
				var columnName = selectedItem.Name;

				foreach (var table in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Where(t => t.Name == edwTable.Name))
				{
					var edwColumn = table.GetEdwColumnConfigRows().Where(c => c.Name == columnName).FirstOrDefault();
					if (edwColumn != null)
					{
						BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.RemoveEdwColumnConfigRow(edwColumn);
					}
				}
				isDeleted = true;
			}
			return isDeleted;
		}

		void edwColumnConfigGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			var currentItem = GetEdwColumnConfigCurrentItemRow();
			if (currentItem != null)
			{
				modifiedRowName = currentItem.Name;
			}
		}

		void edwColumnConfigGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var currentItem = (DataRowView)e.Row.DataContext;
			if (currentItem != null)
			{
				try
				{
					var propagatedColumns = new[] { "Model Column Name", "Data Type", "Max Length", "Precision", "Scale", "Is Partition Key" };
					if (propagatedColumns.Contains(e.Column.Header))
					{
						var edwColumn = GetEdwColumnConfigCurrentItemRow();
						foreach (var column in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.Where(c => c.Name == modifiedRowName && c.TableName == edwColumn.TableName && c.TransformId != edwColumn.TransformId))
						{
							switch (e.Column.Header)
							{
								case "Model Column Name":
									column.Name = edwColumn.Name;
									break;
								case "Data Type":
									column.DataType = edwColumn.DataType;
									break;
								case "Max Length":
									column.MaxLength = edwColumn.MaxLength;
									break;
								case "Precision":
									column.Precision = edwColumn.Precision;
									break;
								case "Scale":
									column.Scale = edwColumn.Scale;
									break;
								case "Is Partition Key":
									column.IsPartitionKey = edwColumn.IsPartitionKey;
									break;
								default:
									break;
							}
						}
					}

					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SortBaseTableDependencyOrder();
				}
				catch (BiConfigurationException ex)
				{
					BiLogger.Fail(ex.Message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					BiLogger.Fail(ex);
				}
				var currentTable = GetEdwTableConfigCurrentItemRow();
				RefreshQueryTextBoxes(currentTable);
			}
		}

		void QueryButton_Click(object sender, RoutedEventArgs e)
		{
			Invoke(() =>
			{
				BiFiles.CWSharedPath = CWSharedPath;
				string source = null;
				string query = null;
				string operation = null;

				if (createModelViewTab.IsSelected)
				{
					var modelView = GetModelViewTableCurrentItemRow();
					source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", modelView.Schema, modelView.Name);
					query = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetDropAndCreateModelViewQuery(modelView);
					operation = "Create View";
				}
				else if (configurationTab.IsSelected)
				{
					var edwTable = GetEdwTableConfigCurrentItemRow();
					source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", edwTable.Schema, edwTable.Name);

					if (initialLoadQueryForBaseTab.IsSelected)
					{
						query = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetInitialLoadQueryForEdwTableTesting(edwTable);
						operation = "Initial Load";
					}
					else if (insertQueryTab.IsSelected)
					{
						query = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetIncrementalInsertQueryForEdwTableTesting(edwTable);
						operation = "Incremental Insert";
					}
					else if (deleteQueryTab.IsSelected)
					{
						query = deleteQueryTextBox.Text;
						operation = "Incremental Delete";
					}
					else if (customIndexQueryForBaseTab.IsSelected)
					{
						query = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetEditableCustomIndexScriptQueryForEdwTable(edwTable);
						operation = "Create Custom Index";
					}
				}
				else if (denormTableTab.IsSelected)
				{
					var denormTable = GetDenormalizedTableCurrentItemRow();
					source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", denormTable.Schema, denormTable.Name);

					if (createModelViewForDenormTab.IsSelected)
					{
						query = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetDropAndCreateDenormalizedTableViewQuery(denormTable);
						operation = "Create View";
					}
					else if (initialLoadQueryForDenormTab.IsSelected)
					{
						query = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetDropAndCreateInitialLoadQueryForDenormalizedTable(denormTable);
						operation = "Initial Load";
					}
					else if (incrementalLoadQueryTab.IsSelected)
					{
						query = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetDropAndCreateIncrementalLoadQueryForDenormalizedTable(denormTable);
						operation = "Incremental Load";
					}
					else if (customIndexQueryForDenormTab.IsSelected)
					{
						query = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.GetEditableCustomIndexScriptQueryForDenormalizedTable(denormTable);
						operation = "Create Custom Index";
					}
				}

				var errorMsg = SchemaSynchroniser.TestSingleQuery(source, query, operation);
				if (string.IsNullOrEmpty(errorMsg))
				{
					BiLogger.Complete("Query executed without errors.");
				}
				else
				{
					BiLogger.Fail(errorMsg);
				}
			});
		}

		void SaveCustomIndexButton_Click(object sender, RoutedEventArgs e)
		{
			Invoke(() =>
			{
				BiFiles.CWSharedPath = CWSharedPath;
				string query = null;

				if (configurationTab.IsSelected)
				{
					query = customIndexQueryForBaseTextBox.Text;
					var edwTable = GetEdwTableConfigCurrentItemRow();
					if (!edwTable.HasErrors)
					{
						if (ValidateCustomIndex(query, $"{edwTable.Schema}.{edwTable.Name}"))
						{
							edwTable.CustomIndex = query;
							BiLogger.Complete("Custom index saved successfully.");
						}
						else
						{
							BiLogger.Fail("Validation failed. Please check your input.");
						}
					}
					else
					{
						BiLogger.Fail("Save failed. Check EDW base table for errors.");
					}
				}
				else if (denormTableTab.IsSelected)
				{
					query = customIndexQueryForDenormTextBox.Text;
					var denormTable = GetDenormalizedTableCurrentItemRow();
					if (!denormTable.HasErrors)
					{
						if (ValidateCustomIndex(query, $"{denormTable.Schema}.{denormTable.Name}"))
						{
							denormTable.CustomIndex = query;
							BiLogger.Complete("Custom index saved successfully.");
						}
						else
						{
							BiLogger.Fail("Validation failed. Please check your input of Custom Index for errors.");
						}
					}
					else
					{
						BiLogger.Fail("Save failed. Check EDW denormalized table for errors.");
					}
				}
			});
		}

		bool ValidateCustomIndex(string query, string fullTableName)
		{
			var pattern = @"CREATE\s.*\sINDEX\s\[?(?<indexName>.*)\]?\sON\s\[?(?<schemaName>\w+)\]?\.\[?(?<tableName>\w+)\]?\s";
			var indexes = query.SplitByLine();
			foreach (var index in indexes)
			{
				if (!string.IsNullOrEmpty(index))
				{
					var match = Regex.Match(index, pattern, RegexOptions.IgnoreCase);
					if (!match.Success || !match.Result("${schemaName}.${tableName}").Equals(fullTableName))
					{
						BiLogger.Fail($"The full table name of custom index: {index} is not match to it's table: {fullTableName}.");
						return false;
					}
				}
			}
			return true;
		}

		#endregion
	}
	#endregion
}
