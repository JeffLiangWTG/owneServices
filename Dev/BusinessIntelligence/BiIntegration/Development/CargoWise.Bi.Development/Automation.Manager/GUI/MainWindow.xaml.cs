using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;
using CargoWise.Bi.Configuration;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.PowerBiSync;
using CargoWise.Bi.Development.SchemaSync;
using CargoWise.Bi.Development.SsasBuilder;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Main.Navigation.WPF.Test;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
#if DEBUG
	[TestExcludeWPFControlFromBasher]
#endif
	public partial class MainWindow : Window, IDisposable
	{
		public MainWindow()
		{
			Db.DisableSchemaVersionCheckPermanently();
			InitializeComponent();
			RegisterLoggerEvent();
			InitializeSearchView();

			CWSharedPathSet = File.Exists(CWSharedPathTextFile);

			AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnLeftMouseButtonDown), true);
		}

		void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			var senderElement = e.OriginalSource as FrameworkElement;
			if (senderElement == null)
			{
				return;
			}
			var row = senderElement.DataContext as DataRowView;

			if (row != null)
			{
				var source = e.Source as DataGrid;
				if (source != null)
				{
					if (source.SelectionUnit == DataGridSelectionUnit.FullRow)
					{
						source.SelectedItem = row;
					}
				}
			}
		}

		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (!CWSharedPathSet)
			{
				CWSharedPathPopup cwSharedPathPopup = new CWSharedPathPopup();
				bool? value = cwSharedPathPopup.ShowDialog();
				if (value ?? false)
				{
					SaveCWSharedPath(cwSharedPathPopup.CWSharedPath.Text);
				}
			}

			BiAutomationConfigLoaderForDevelopment.Instance.ResetConfiguration();
			LoadConfiguration();

			cdcTableConfigGrid.ItemsSource = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.DefaultView;
			modelViewTableConfigGrid.ItemsSource = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig.DefaultView;
			ssasCubesGrid.ItemsSource = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasCubes.DefaultView;
			cdcTableConfigGrid.Visibility = Visibility.Collapsed;
			cdcColumnConfigGrid.Visibility = Visibility.Collapsed;
			edwTableConfigGrid.Visibility = Visibility.Collapsed;
			edwColumnConfigGrid.Visibility = Visibility.Collapsed;
		}

		void windowAutomationManager_Closing(object sender, CancelEventArgs e)
		{
			if (runnerTask != null && !runnerTask.IsCompleted)
			{
				cts.Cancel();
				runnerTask.Wait();
			}

			RunInAnotherThread(() =>
			{
				using (Db.DisableSchemaVersionCheck())
				using (var connection = Db.NewAdminConnection(BiDatabase.ServerName, Db.SqlMasterDb))
				{
					BiDatabase.DropTemplateDatabase(connection);
					BiDatabase.DropBiDatabases(connection);
				}
			});
		}

		void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem)
			{
				var tabItem = (TabItem)e.AddedItems[0];
				if ((string)tabItem.Header == (string)configurationTab.Header)
				{
					edwModelQueryGrid.Visibility = Visibility.Visible;
					LoggerGridSplitter.Visibility = Visibility.Visible;
					Grid.SetColumnSpan(LoggerTextBox, 1);

					createModelViewTab.Visibility = Visibility.Collapsed;
					createModelViewForDenormTab.Visibility = Visibility.Collapsed;
					customIndexQueryForBaseTab.Visibility = Visibility.Visible;
					customIndexQueryForDenormTab.Visibility = Visibility.Collapsed;
					initialLoadQueryForBaseTab.Visibility = Visibility.Visible;
					initialLoadQueryForDenormTab.Visibility = Visibility.Collapsed;
					insertQueryTab.Visibility = Visibility.Visible;
					deleteQueryTab.Visibility = Visibility.Visible;
					incrementalLoadQueryTab.Visibility = Visibility.Collapsed;
					initialLoadQueryForBaseTab.IsSelected = true;

					edwTableConfigGrid_SelectionChanged(this, null);
				}
				else if ((string)tabItem.Header == (string)denormTableTab.Header)
				{
					edwModelQueryGrid.Visibility = Visibility.Visible;
					LoggerGridSplitter.Visibility = Visibility.Visible;
					Grid.SetColumnSpan(LoggerTextBox, 1);

					createModelViewTab.Visibility = Visibility.Collapsed;
					createModelViewForDenormTab.Visibility = Visibility.Visible;
					customIndexQueryForBaseTab.Visibility = Visibility.Collapsed;
					customIndexQueryForDenormTab.Visibility = Visibility.Visible;
					initialLoadQueryForBaseTab.Visibility = Visibility.Collapsed;
					initialLoadQueryForDenormTab.Visibility = Visibility.Visible;
					incrementalLoadQueryTab.Visibility = Visibility.Visible;
					insertQueryTab.Visibility = Visibility.Collapsed;
					deleteQueryTab.Visibility = Visibility.Collapsed;
					createModelViewForDenormTab.IsSelected = true;

					denormColumnConfigGrid_SelectionChanged(this, null);
				}
				else if ((string)tabItem.Header == (string)customTableTab.Header)
				{
					edwModelQueryGrid.Visibility = Visibility.Hidden;
					LoggerGridSplitter.Visibility = Visibility.Hidden;
					Grid.SetColumnSpan(LoggerTextBox, 2);

					customTableConfigGrid_SelectionChanged(this, null);
				}
				else if ((string)tabItem.Header == (string)modelViewTab.Header)
				{
					edwModelQueryGrid.Visibility = Visibility.Visible;
					LoggerGridSplitter.Visibility = Visibility.Visible;
					Grid.SetColumnSpan(LoggerTextBox, 1);

					createModelViewTab.Visibility = Visibility.Visible;
					createModelViewForDenormTab.Visibility = Visibility.Collapsed;
					initialLoadQueryForBaseTab.Visibility = Visibility.Collapsed;
					initialLoadQueryForDenormTab.Visibility = Visibility.Collapsed;
					incrementalLoadQueryTab.Visibility = Visibility.Collapsed;
					insertQueryTab.Visibility = Visibility.Collapsed;
					deleteQueryTab.Visibility = Visibility.Collapsed;
					incrementalLoadQueryTab.Visibility = Visibility.Collapsed;
					createModelViewTab.IsSelected = true;

					modelViewColumnConfigGrid_SelectionChanged(this, null);
				}
				else if ((string)tabItem.Header == (string)ssasCubesTab.Header)
				{
					edwModelQueryGrid.Visibility = Visibility.Hidden;
					LoggerGridSplitter.Visibility = Visibility.Hidden;
					Grid.SetColumnSpan(LoggerTextBox, 2);
				}
				else if ((string)tabItem.Header == (string)sourceAndModelViewMappingTab.Header)
				{
					edwModelQueryGrid.Visibility = Visibility.Hidden;
					LoggerGridSplitter.Visibility = Visibility.Hidden;
					Grid.SetColumnSpan(LoggerTextBox, 2);
				}
			}
		}

		void showSettings_Click(object sender, RoutedEventArgs e)
		{
			SettingsPopup settingsPopup = new SettingsPopup(BiDatabase.ServerName, SsasProjectBuilder.AnalysisServerName, CWSharedPath);
			bool? value = settingsPopup.ShowDialog();
			if (value ?? false)
			{
				BiDatabase.ServerName = settingsPopup.mainServer.Text;
				SsasProjectBuilder.AnalysisServerName = settingsPopup.analysisServer.Text;
				BiFileLogger.Instance.SaveMainServerInstanceFile(BiDatabase.ServerName);
				BiFileLogger.Instance.SaveAnalysisServerInstanceFile(SsasProjectBuilder.AnalysisServerName);
				SaveCWSharedPath(settingsPopup.CWSharedPath.Text);
			}
			settingsPopup.Close();
			settingsPopup.Dispose();
		}

		#region Ribbon Button Click Events

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void syncConfiguration_Click(object sender, RoutedEventArgs e)
		{
			RunInAnotherThread(() =>
			{
				try
				{
					BiFiles.CWSharedPath = CWSharedPath;
					DisplayProgress("Synchronising configuration");
					InitializeStatistics();
					SchemaSynchroniser.SaveBiAutomationConfiguration();
					SchemaSynchroniser.SyncSchemaConfiguration();
					SsasProjectBuilder.BuildSsasProject();
					PowerBiSynchroniser.Sync();
					SchemaSynchroniser.SaveConfiguration();
					SchemaSynchroniser.TestQueryForAllEdwTables();
					BiSolutionCompiler.CompileRegistrationProject();
					ReloadStatistics();
					BiLogger.Complete("Database Schema Configuration has been synchronised successfully");
				}
				catch (BiDatabaseSyncException ex)
				{
					BiLogger.Fail(ex.Message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					BiLogger.Fail(ex);
				}
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void validateConfiguration_Click(object sender, RoutedEventArgs e)
		{
			RunInAnotherThread(() =>
			{
				try
				{
					BiLogger.StartTask("Validating BI configuration");
					DisplayProgress("Validating BI configuration");
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.Validate();
					BiLogger.Complete("Configuration settings are valid");
					DisplayProgress("Configuration settings are valid");
				}
				catch (BiConfigurationException ex)
				{
					BiLogger.Fail(ex.Message);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					BiLogger.Fail(ex);
				}
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void undoCheckouts_Click(object sender, RoutedEventArgs e)
		{
			RunInAnotherThread(() =>
			{
				try
				{
					DisplayProgress("Undoing checked out files");
					new BiManager().UndoCheckout();
					BiLogger.Complete();
					DisplayProgress("Undo check out of files completed");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					BiLogger.Fail(ex);
				}
			});
		}

		void mainServerName_Changed(object sender, TextChangedEventArgs e)
		{
			var textBox = sender as RibbonTextBox;
			BiDatabase.ServerName = textBox.Text;
			BiFileLogger.Instance.SaveMainServerInstanceFile(textBox.Text);
		}

		void analysisServerName_Changed(object sender, TextChangedEventArgs e)
		{
			var textBox = sender as RibbonTextBox;
			SsasProjectBuilder.AnalysisServerName = textBox.Text;
			BiFileLogger.Instance.SaveAnalysisServerInstanceFile(textBox.Text);
		}

		#endregion

		#region SSAS

		void buildSsasProject_Click(object sender, RoutedEventArgs e)
		{
			RunInAnotherThread(() =>
			{
				DisplayProgress("Building SSAS Project");
				try
				{
					BiFiles.CWSharedPath = CWSharedPath;
					SsasProjectBuilder.BuildSsasProject();
					SchemaSynchroniser.SaveBiAutomationConfiguration();
					SchemaSynchroniser.TestQueryForAllEdwTables();
					BiSolutionCompiler.CompileRegistrationProject();
					ReloadStatistics();
				}
				catch (BiDatabaseSyncException ex)
				{
					BiLogger.Fail(ex.Message);
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					BiLogger.Fail("Building SSAS Project failed", exception);
				}
			});
		}

		void sortCubeFile_Click(object sender, RoutedEventArgs e)
		{
			RunInAnotherThread(() =>
			{
				DisplayProgress("Sort Cubes");
				try
				{
					SsasProjectBuilder.SortCubes();
					BiSolutionCompiler.CompileRegistrationProject();
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					BiLogger.Fail(exception);
				}
			});
		}

		#endregion

		#region Logger

		void LoggerTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			LoggerTextBox.ScrollToEnd();
		}

		void openLogFile_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BiFileLogger.OpenLogFile();
			}
			catch (Win32Exception ex)
			{
				if (ex.Message.Contains("The system cannot find the file specified"))
				{
					DisplayProgress("Log file is still empty");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				DisplayProgress("Open log file unsuccessful: " + ex.Message);
			}
		}

		#endregion

		#region Search View

		void showSearchOptions_Click(object sender, RoutedEventArgs e)
		{
			var button = (MenuItem)sender;
			if (button.Header.ToString() == "Show Search Options")
			{
				ResizeTableSearchRow();
				ResizeColumnSearchRow();
				button.Header = "Hide Search Options";

				var cell = ComponentHelper.FindCellFromDataGrid(tableSearchGrid);
				cell.IsEditing = true;
				cell.Focus();
			}
			else
			{
				tableSearchRow.Height = new GridLength(0);
				columnSearchRow.Height = new GridLength(0);
				edwTableSearchRow.Height = new GridLength(0);
				edwColumnSearchRow.Height = new GridLength(0);
				button.Header = "Show Search Options";
			}
		}

		void ClearTableSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			tableSearchGrid.CommitEdit();
			ClearSearchParameters(TableSearchParameters);
			tableSearchGrid.Items.Refresh();
			ResizeTableSearchRow();
		}

		void AddTableSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			tableSearchGrid.CommitEdit();
			AddSearchParameter(TableSearchParameters);
			tableSearchGrid.Items.Refresh();
			ResizeTableSearchRow();
		}

		void RemoveTableSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			tableSearchGrid.CommitEdit();
			RemoveSearchParameter(TableSearchParameters, ((sender as Button).Tag) as SearchParameter);
			tableSearchGrid.Items.Refresh();
			ResizeTableSearchRow();
		}

		void ClearColumnSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			columnSearchGrid.CommitEdit();
			ClearSearchParameters(ColumnSearchParameters);
			columnSearchGrid.Items.Refresh();
			ResizeColumnSearchRow();
		}

		void AddColumnSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			columnSearchGrid.CommitEdit();
			AddSearchParameter(ColumnSearchParameters);
			columnSearchGrid.Items.Refresh();
			ResizeColumnSearchRow();
		}

		void RemoveColumnSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			columnSearchGrid.CommitEdit();
			RemoveSearchParameter(ColumnSearchParameters, ((sender as Button).Tag) as SearchParameter);
			columnSearchGrid.Items.Refresh();
			ResizeColumnSearchRow();
		}

		void ClearEdwTableSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			edwTableSearchGrid.CommitEdit();
			ClearSearchParameters(TableSearchParameters);
			edwTableSearchGrid.Items.Refresh();
			ResizeEdwTableSearchRow();
		}

		void AddEdwTableSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			edwTableSearchGrid.CommitEdit();
			AddSearchParameter(TableSearchParameters);
			edwTableSearchGrid.Items.Refresh();
			ResizeEdwTableSearchRow();
		}

		void RemoveEdwTableSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			edwTableSearchGrid.CommitEdit();
			RemoveSearchParameter(TableSearchParameters, ((sender as Button).Tag) as SearchParameter);
			edwTableSearchGrid.Items.Refresh();
			ResizeEdwTableSearchRow();
		}

		void ClearEdwColumnSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			edwColumnSearchGrid.CommitEdit();
			ClearSearchParameters(ColumnSearchParameters);
			edwColumnSearchGrid.Items.Refresh();
			ResizeEdwColumnSearchRow();
		}

		void AddEdwColumnSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			edwColumnSearchGrid.CommitEdit();
			AddSearchParameter(ColumnSearchParameters);
			edwColumnSearchGrid.Items.Refresh();
			ResizeEdwColumnSearchRow();
		}

		void RemoveEdwColumnSearchParameter_Click(object sender, RoutedEventArgs e)
		{
			edwColumnSearchGrid.CommitEdit();
			RemoveSearchParameter(ColumnSearchParameters, ((sender as Button).Tag) as SearchParameter);
			edwColumnSearchGrid.Items.Refresh();
			ResizeEdwColumnSearchRow();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void TableSearch_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var searchResults = BamSearchView.Search(TableSearchParameters, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig);
				searchResults.Sort = "SourceTable";
				cdcTableConfigGrid.ItemsSource = searchResults;
				RecalculateCdcTableStats();
				if (TableSearchParameters.ParameterList.Any(p => !string.IsNullOrEmpty(p.Category) && !string.IsNullOrEmpty(p.Value)))
				{
					tableSummaryStatsText.Text += string.Format(CultureInfo.InvariantCulture, "\tSearch Result: {0} found", searchResults.Count);
				}

				if (searchResults.Count > 0)
				{
					cdcTableConfigGrid.CurrentItem = searchResults[0];
					cdcTableConfigGrid_SelectionChanged(this, null);
					ColumnSearchParameters.RefreshOptions(searchResults[0]["SourceTable"].ToString());
					columnSearchGrid.Items.Refresh();
				}
				else
				{
					cdcColumnConfigGrid.ItemsSource = null;
					ColumnSearchParameters.RefreshOptions(null);
					columnSearchGrid.Items.Refresh();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BiLogger.Fail(ex.Message);
			}
		}

		void tableSearchGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentColumn = ((DataGrid)e.Source).CurrentColumn;
			if (currentColumn != null &&
				(string)currentColumn.Header == "Search Field" &&
				(e.RemovedItems.Count == 0 ||
				 e.AddedItems[0] != e.RemovedItems[0]))
			{
				var cell = ComponentHelper.FindCellFromDataGrid(tableSearchGrid, tableSearchGrid.SelectedIndex);
				cell.IsEditing = true;
				cell.Focus();

				tableSearchRefreshNeeded = true;

				columnSearchGrid.UnselectAll();
			}
		}

		void tableSearchGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			var column = ((DataGrid)sender).CurrentCell.Column;
			if (tableSearchRefreshNeeded && column != null && (string)(column.Header) == "Value")
			{
				tableSearchGrid.CommitEdit();
				tableSearchGrid.Items.Refresh();
				tableSearchRefreshNeeded = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void ColumnSearch_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var currentItem = GetTableConfigCurrentItemRow();
				var searchResults = BamSearchView.Search(ColumnSearchParameters, BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig, currentItem.SourceTable);
				searchResults.Sort = "SourceColumn";
				cdcColumnConfigGrid.ItemsSource = searchResults;
				RecalculateCdcColumnStats();
				if (ColumnSearchParameters.ParameterList.Any(p => !string.IsNullOrEmpty(p.Category) && !string.IsNullOrEmpty(p.Value)))
				{
					columnSummaryStatsText.Text += string.Format(CultureInfo.InvariantCulture, "\tSearch Result: {0} found", searchResults.Count);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BiLogger.Fail(ex.Message);
			}
		}

		void columnSearchGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentColumn = ((DataGrid)e.Source).CurrentColumn;
			if (currentColumn != null &&
				(string)currentColumn.Header == "Search Field" &&
				(e.RemovedItems.Count == 0 ||
				 e.AddedItems[0] != e.RemovedItems[0]))
			{
				var cell = ComponentHelper.FindCellFromDataGrid(columnSearchGrid, columnSearchGrid.SelectedIndex);
				cell.IsEditing = true;
				cell.Focus();

				columnSearchRefreshNeeded = true;

				tableSearchGrid.UnselectAll();
			}
		}

		void columnSearchGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			var column = ((DataGrid)sender).CurrentCell.Column;
			if (columnSearchRefreshNeeded && column != null && (string)(column.Header) == "Value")
			{
				columnSearchGrid.CommitEdit();
				columnSearchGrid.Items.Refresh();
				columnSearchRefreshNeeded = false;
			}
		}

		#endregion

		#region Generic Null Value in DataGrid handler

		void GenericDataGrid_AutoGeneratedColumns(object sender, EventArgs e)
		{
			foreach (DataGridColumn dataGridColumn in ((DataGrid)sender).Columns)
			{
				if (dataGridColumn.GetType() == typeof(DataGridTextColumn))
				{
					DataGridTextColumn dataGridTextColumn = (DataGridTextColumn)dataGridColumn;
					dataGridTextColumn.Binding.TargetNullValue = "<DBNull>";
				}
			}
		}

		void resultSetDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			if (e.PropertyName.Contains("_x2024_") && e.Column is DataGridTextColumn)
			{
				DataGridBoundColumn dataGridBoundColumn = e.Column as DataGridBoundColumn;
				dataGridBoundColumn.Binding = new Binding(e.PropertyName);
				dataGridBoundColumn.Header = e.PropertyName.Replace("_x2024_", ".");
			}
		}

		#endregion

		#region Copy Grid Details

		void copyBaseTables_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(edwTableConfigGrid);
		}

		void copyBaseTableColumns_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(edwColumnConfigGrid);
		}

		void copyDenormalizedTables_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(denormTableConfigGrid);
		}

		void copyDenormalizedTableColumns_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(denormColumnConfigGrid);
		}

		void copyCustomTables_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(denormTableConfigGrid);
		}

		void copyCustomColumns_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(denormTableConfigGrid);
		}

		void copyModelViews_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(modelViewTableConfigGrid);
		}

		void copySourceTables_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(cdcTableConfigGrid);
		}

		void copySourceColumns_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(cdcColumnConfigGrid);
		}

		void copyMapping_Click(object sender, RoutedEventArgs e)
		{
			copyGridToClipboard(sourceModelViewMappingGrid);
		}

		#endregion

		#region Copy Query To Clipboard

		void CopyToClipboard_Click(object sender, RoutedEventArgs e)
		{
			if (createModelViewTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(createModelViewTextBox);
			}
			else if (createModelViewForDenormTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(createModelViewForDenormTextBox);
			}
			else if (initialLoadQueryForBaseTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(initialLoadQueryForBaseTextBox);
			}
			else if (initialLoadQueryForDenormTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(initialLoadQueryForDenormTextBox);
			}
			else if (insertQueryTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(insertQueryTextBox);
			}
			else if (deleteQueryTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(deleteQueryTextBox);
			}
			else if (incrementalLoadQueryTab.IsSelected)
			{
				CopyToClipboardIfNotEmpty(incrementalLoadQueryForDenormTextBox);
			}
		}

		void CopyToClipboardIfNotEmpty(TextBox textBox)
		{
			var query = textBox.Text.Trim();
			if (!string.IsNullOrEmpty(query))
			{
				Clipboard.SetText(query); // Not using ZorKArchitecture
			}
		}

		#endregion

		void packagePowerBiFiles_Click(object sender, RoutedEventArgs e)
		{
			RunInAnotherThread(() =>
			{
				try
				{
					DisplayProgress("Packaging Power BI Files");
					PowerBiSynchroniser.Sync();
					BiLogger.Complete();
				}
				catch (BiDatabaseSyncException ex)
				{
					BiLogger.Fail(ex.Message);
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					BiLogger.Fail("Power BI Report File and Custom Visual synchronisation failed", exception);
				}
			});
		}

		void AddTableToAuditMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var selectedTable = (CdcTableConfigRow)((DataRowView)cdcTableConfigGrid.SelectedItem).Row;
			if (selectedTable != null)
			{
				if (!selectedTable.TableInAudit)
				{
					BiLogger.Complete($"[{selectedTable.SourceSchema}.{selectedTable.SourceTable}] has been added to Audit.");
					foreach (var cdcColumn in selectedTable.GetCdcColumnConfigRows())
					{
						cdcColumn.ColumnInAudit = true;
					}
				}
				else
				{
					BiLogger.Complete($"[{selectedTable.SourceSchema}.{selectedTable.SourceTable}] is already enabled for Audit.");
				}
			}
		}

		void RemoveTableFromAuditMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var selectedTable = (CdcTableConfigRow)((DataRowView)cdcTableConfigGrid.SelectedItem).Row;
			if (selectedTable != null)
			{
				if (selectedTable.TableInAudit)
				{
					BiLogger.Complete($"[{selectedTable.SourceSchema}.{selectedTable.SourceTable}] has been removed from Audit.");
					foreach (var cdcColumn in selectedTable.GetCdcColumnConfigRows())
					{
						cdcColumn.ColumnInAudit = false;
					}
				}
				else
				{
					BiLogger.Complete($"[{selectedTable.SourceSchema}.{selectedTable.SourceTable}] isn't enabled for Audit.");
				}
			}
		}

		void EdwModelQueryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem)
			{
				SaveCustomIndexButton.IsEnabled = false;
				SaveCustomIndexButton.Visibility = Visibility.Collapsed;
				QueryButton.IsEnabled = true;
				QueryButton.Visibility = Visibility.Visible;

				var tabItem = (TabItem)e.AddedItems[0];
				if ((string)tabItem.Header == (string)customIndexQueryForBaseTab.Header || (string)tabItem.Header == (string)customIndexQueryForDenormTab.Header)
				{
					var currentItem = GetTableConfigCurrentItemRow();
					if (!currentItem.HasErrors)
					{
						SaveCustomIndexButton.IsEnabled = true;
						SaveCustomIndexButton.Visibility = Visibility.Visible;
						QueryButton.IsEnabled = false;
						QueryButton.Visibility = Visibility.Collapsed;
					}
				}
			}
		}
	}
	#endregion
}
