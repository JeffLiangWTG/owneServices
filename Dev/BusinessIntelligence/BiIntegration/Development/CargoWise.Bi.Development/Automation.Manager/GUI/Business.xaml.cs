using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CargoWise.Bi.Configuration;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync;
using CargoWise.BuildTools;
using CargoWise.Common;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
	public partial class MainWindow : Window, IDisposable
	{
		readonly string CWSharedPathTextFile = Path.Combine(BuildConstants.LocalEnterprisePath, "cwSharedPath.txt");

		public bool CWSharedPathSet
		{
			get => cwSharedPathSet;
			set => cwSharedPathSet = value;
		}
		bool cwSharedPathSet;

		string CWSharedPath
		{
			get
			{
				return cwSharedPathSet ? cwSharedPath ?? (cwSharedPath = File.ReadAllText(CWSharedPathTextFile)) : null;
			}
			set => cwSharedPath = value;
		}
		string cwSharedPath;

		public void SaveCWSharedPath(string path)
		{
			File.WriteAllText(CWSharedPathTextFile, path);
			CWSharedPath = path;
			CWSharedPathSet = true;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void LoadConfiguration()
		{
			InitializeStatistics();

			try
			{
				SchemaSynchroniser.LoadBiConfiguration();
				BiLogger.Complete("Loading complete");
			}
			catch (BiConfigurationException ex)
			{
				BiLogger.Fail(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BiLogger.Fail(ex);
			}
			finally
			{
				ReloadStatistics();
			}
		}

		void InitializeStatistics()
		{
			Invoke(() =>
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.ColumnChanged -= CdcTableConfig_ColumnChanged;
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.ColumnChanged -= CdcColumnConfig_ColumnChanged;
			});
		}

		void ReloadStatistics()
		{
			Invoke(() =>
			{
				DataTypeList2.ItemsSource = edwDataTypeArray;
				DataTypeList3.ItemsSource = edwDataTypeArray;
				DataTypeList4.ItemsSource = edwDataTypeArray;
				DataTypeList5.ItemsSource = edwDataTypeArray;
				DataTypeList6.ItemsSource = edwDataTypeArray;
				DataTypeList7.ItemsSource = edwDataTypeArray;
				KeepAsList.ItemsSource = keepAsArray;
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.ColumnChanged += CdcTableConfig_ColumnChanged;
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.ColumnChanged += CdcColumnConfig_ColumnChanged;

				CdcTableConfig_ColumnChanged(this, null);
				CdcColumnConfig_ColumnChanged(this, null);

				if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.Any())
				{
					var items = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.DefaultView;
					cdcTableConfigGrid.ItemsSource = items;
					cdcTableConfigGrid.CurrentItem = items[0];

					cdcColumnConfigGrid.Items.Refresh();

					cdcTableConfigGrid.Visibility = Visibility.Visible;
					cdcColumnConfigGrid.Visibility = Visibility.Visible;
					cdcTableConfigGrid_SelectionChanged(this, null);

					RecalculateCdcTableStats();
				}

				if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.Any())
				{
					var items = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.DefaultView;
					items.Sort = "DependencyOrder";
					edwTableConfigGrid.ItemsSource = items;
					edwTableConfigGrid.CurrentItem = items[0];

					edwTableConfigGrid_SelectionChanged(this, null);

					edwTableConfigGrid.Visibility = Visibility.Visible;
					edwColumnConfigGrid.Visibility = Visibility.Visible;
					edwTableEditButtons.Height = new GridLength(30);
					edwColumnEditButtons.Height = new GridLength(30);

					var itemsForModelView = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.DefaultView;
					itemsForModelView.Sort = "DependencyOrder";
					edwModelTableConfigGrid.ItemsSource = itemsForModelView;
					edwModelTableConfigGrid.CurrentItem = itemsForModelView[0];
					edwModelTableConfigGrid_SelectionChanged(this, null);

					edwModelTableConfigGrid2.ItemsSource = itemsForModelView;
					edwModelTableConfigGrid2.CurrentItem = itemsForModelView[0];
					edwModelTableConfigGrid2_SelectionChanged(this, null);
				}

				if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig.Any())
				{
					var items = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig.DefaultView;
					items.Sort = "Name";
					denormTableConfigGrid.ItemsSource = items;
					denormTableConfigGrid.CurrentItem = items[0];
					denormTableConfigGrid_SelectionChanged(this, null);

					denormTableConfigGrid2.ItemsSource = items;
					denormTableConfigGrid2.CurrentItem = items[0];
					denormTableConfigGrid2_SelectionChanged(this, null);
				}

				if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig.Any())
				{
					var items = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwCustomTableConfig.DefaultView;
					items.Sort = "DependencyOrder";
					customTableConfigGrid.ItemsSource = items;
					customTableConfigGrid.CurrentItem = items[0];
					customTableConfigGrid_SelectionChanged(this, null);
				}

				if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasCubes.Any())
				{
					ssasCubesGrid.CommitEdit();
					ssasTablesGrid.CommitEdit();
					ssasCubesGrid.Items.Refresh();
					ssasTablesGrid.Items.Refresh();
				}

				if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig.Any())
				{
					var items = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwModelViewTableConfig.DefaultView;
					items.Sort = "Schema";
					modelViewTableConfigGrid.ItemsSource = items;
					modelViewTableConfigGrid.CurrentItem = items[0];
				}
			});
		}

		#region Threading

		void RunInAnotherThread(Action target)
		{
			if (runnerTask == null || runnerTask.IsCompleted)
			{
				cts = new CancellationTokenSource();
				runnerTask = Task.Run(() =>
				{
					cts.Token.ThrowIfCancellationRequested();
					target();
				}, cts.Token);
			}
		}

		void Invoke(Action target)
		{
			Dispatcher.BeginInvoke(target);
		}

		Task runnerTask;
		CancellationTokenSource cts;

		#endregion

		#region BI Commands

		void RecalculateCdcTableStats()
		{
			if (GetTableConfigCurrentItemRow() != null)
			{
				var dataView = ((DataView)cdcTableConfigGrid.ItemsSource);
				if (dataView != null)
				{
					var tableSummaryStats = new StringBuilder();

					foreach (DataRow row in dataView.ToTable(true, "Action").Rows)
					{
						var customView = dataView.ToTable().DefaultView;
						var action = row["Action"].ToString();
						customView.RowFilter = string.Format(CultureInfo.InvariantCulture, "[Action] = '{0}'", action);
						tableSummaryStats.Append(customView.Count.ToString(CultureInfo.InvariantCulture) + " " + action + "\t");
					}

					tableSummaryStatsText.Text = "\t" + tableSummaryStats.ToString();
				}
			}
			else
			{
				tableSummaryStatsText.Text = "No details available";
			}
		}

		void RecalculateCdcColumnStats()
		{
			if (GetTableConfigCurrentItemRow() != null)
			{
				var dataView = ((DataView)cdcColumnConfigGrid.ItemsSource);
				if (dataView != null)
				{
					var columnSummaryStats = new StringBuilder();

					foreach (DataRow row in dataView.ToTable(true, "Action").Rows)
					{
						var customView = dataView.ToTable().DefaultView;
						var action = row["Action"].ToString();
						customView.RowFilter = string.Format(CultureInfo.InvariantCulture, "[Action] = '{0}'", action);
						columnSummaryStats.Append(customView.Count.ToString(CultureInfo.InvariantCulture) + " " + action + "\t");
					}

					columnSummaryStatsText.Text = "\t" + columnSummaryStats.ToString();
				}
			}
			else
			{
				columnSummaryStatsText.Text = "No details available";
			}
		}

		#endregion

		#region Logger

		void RegisterLoggerEvent()
		{
			BiFileLogger.Initialize();
			BiLogger.Instance.OnStartTask += new BiLoggerEvent(Log);
			BiLogger.Instance.OnStartSubtask += new BiLoggerEvent(Log);
			BiLogger.Instance.OnCompleted += new BiLoggerEvent(Complete);
			BiLogger.Instance.OnFailed += new BiLoggerEvent(Fail);
		}

		void Log(string message, DateTime time)
		{
			Invoke(() =>
			{
				LoggerTextBox.Foreground = Brushes.Black;
				if (!string.IsNullOrEmpty(message))
				{
					var newLine = string.IsNullOrEmpty(LoggerTextBox.Text) ? "" : "\r\n";
					LoggerTextBox.Text += string.Format(CultureInfo.InvariantCulture, "{0}{1} : {2}", newLine, time.ToLongTimeString(), message);
				}
			});
		}

		void Complete(string message, DateTime time)
		{
			Invoke(() =>
			{
				DisplayMessage(message, time, Brushes.Green);
			});
		}

		void Fail(string message, DateTime time)
		{
			Invoke(() =>
			{
				DisplayMessage(message, time, Brushes.Red);
			});
		}

		void DisplayMessage(string message, DateTime time, SolidColorBrush color)
		{
			LoggerTextBox.Foreground = color;
			if (!string.IsNullOrEmpty(message))
			{
				var newLine = string.IsNullOrEmpty(LoggerTextBox.Text) ? "" : "\r\n";
				LoggerTextBox.Text += string.Format(CultureInfo.InvariantCulture, "{0}{1} : {2}", newLine, time.ToLongTimeString(), message);
				ProgressStatus.Content = message;
			}
		}

		void DisplayProgress(string message)
		{
			if (runnerTask == null || runnerTask.IsCompleted)
			{
				cts = new CancellationTokenSource();
				runnerTask = Task.Run(() =>
				{
					cts.Token.ThrowIfCancellationRequested();
					ProgressStatus.Content = message;
				}, cts.Token);
			}
		}

		#endregion

		#region Search View

		public SearchView BamSearchView
		{
			get
			{
				return searchView ?? (searchView = new SearchView());
			}
		}
		SearchView searchView;
		public readonly SearchParameterCollection TableSearchParameters = new SearchParameterCollection(CollectionType.TableParameters);
		public readonly SearchParameterCollection ColumnSearchParameters = new SearchParameterCollection(CollectionType.ColumnParameters);

		void InitializeSearchView()
		{
			TableSearchCategories.ItemsSource = BamSearchView.TableSearchCategories;
			// Add source table default param here
			AddDefaultSearchParameter(TableSearchParameters);
			tableSearchGrid.ItemsSource = TableSearchParameters.ParameterList;

			ColumnSearchCategories.ItemsSource = BamSearchView.ColumnSearchCategories;
			// Add source column default param here
			AddDefaultSearchParameter(ColumnSearchParameters);
			columnSearchGrid.ItemsSource = ColumnSearchParameters.ParameterList;
		}

		void ClearSearchParameters(SearchParameterCollection parameterList)
		{
			parameterList.Clear();
			AddSearchParameter(parameterList);
		}

		void AddDefaultSearchParameter(SearchParameterCollection parameterList)
		{
			switch (parameterList.Type)
			{
				case CollectionType.TableParameters:
					{
						var tempParameter = new SearchParameter("Source Table", "", CollectionType.TableParameters, null);
						parameterList.Add(tempParameter.Category, tempParameter.Options.FirstOrDefault() ?? "", GetTableConfigCurrentItemRow()?.SourceTable);
						break;
					}
				case CollectionType.ColumnParameters:
					{
						var tempParameter = new SearchParameter("Source Column", "", CollectionType.ColumnParameters, null);
						parameterList.Add(tempParameter.Category, tempParameter.Options.FirstOrDefault() ?? "", GetTableConfigCurrentItemRow()?.SourceTable);
						break;
					}
				default:
					{
						parameterList.Add("", "", null);
						break;
					}
			}
		}

		void AddSearchParameter(SearchParameterCollection parameterList)
		{
			var currentItem = GetTableConfigCurrentItemRow();
			if (currentItem != null)
			{
				parameterList.Add("", "", currentItem.SourceTable);
			}
			else
			{
				parameterList.Add("", "", null);
			}
		}

		void RemoveSearchParameter(SearchParameterCollection parameterList, SearchParameter parameter)
		{
			parameterList.Remove(parameter);
			if (parameterList.Count == 0)
			{
				AddSearchParameter(parameterList);
			}
		}

		void ResizeTableSearchRow()
		{
			tableSearchRow.Height = new GridLength(50 + TableSearchParameters.Count * 23);
		}

		void ResizeColumnSearchRow()
		{
			columnSearchRow.Height = new GridLength(50 + ColumnSearchParameters.Count * 23);
		}

		void ResizeEdwTableSearchRow()
		{
			edwTableSearchRow.Height = new GridLength(50 + TableSearchParameters.Count * 23);
		}

		void ResizeEdwColumnSearchRow()
		{
			edwColumnSearchRow.Height = new GridLength(50 + ColumnSearchParameters.Count * 23);
		}

		bool tableSearchRefreshNeeded = true;

		bool columnSearchRefreshNeeded = true;

		#endregion

		#region Copy Grid Details

		void copyGridToClipboard(DataGrid grid)
		{
			var builder = new StringBuilder();

			var columns = grid.Columns;
			builder.AppendLine(string.Join("\t", columns.Select(c => c.Header)));

			if (grid.Items.Count > 0)
			{
				if (grid.SelectedItem == null)
				{
					grid.SelectAll();
				}

				var isDataView = grid.SelectedItem is DataRowView;

				var type = isDataView ?
					(grid.SelectedItem as DataRowView).Row.GetType() :
					grid.SelectedItem.GetType();

				foreach (var item in grid.ItemsSource)
				{
					var values = new List<string>();

					foreach (var column in columns.Select(c => c.SortMemberPath))
					{
						var obj = type.GetProperty(column).GetValue(isDataView ? (item as DataRowView).Row : item);
						if (obj != null)
						{
							var regex = new Regex(@"\s+", RegexOptions.IgnoreCase);
							var value = regex.Replace(obj.ToString(), " ");
							values.Add(value);
						}
						else
						{
							values.Add(null);
						}
					}

					builder.AppendLine(string.Join("\t", values));
				}
			}

			Clipboard.SetText(builder.ToString()); // Not using ZorKArchitecture
		}

		#endregion
	}

	#endregion
}
