using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CargoWise.Bi.Development.Common;
using CargoWise.Common;
using ReportMappingRow = CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet.ReportMappingConfigRow;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
	public partial class MainWindow : Window, IDisposable
	{
		int reportIndex = 1;
		void reportAddNew_Click(object sender, RoutedEventArgs e)
		{
			var name = $"New_Report_{reportIndex++}";
			if (BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingConfig.Any(r => r.Name.Equals(name)))
			{
				return;
			}
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingConfig.AddReportMappingConfigRow(name);
			reportGrid.Items.Refresh();
		}

		void reportDelete_Click(object sender, RoutedEventArgs e)
		{
			var report = GetCurrentSelectedReport();
			if (report != null)
			{
				var tables = report.GetReportMappingTableConfigRows().ToList();
				foreach (var table in tables)
				{
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingTableConfig.RemoveReportMappingTableConfigRow(table);
				}
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingConfig.RemoveReportMappingConfigRow(report);
				reportGrid.Items.Refresh();
			}
		}

		void reportGird_SelectedChanged(object sender, SelectionChangedEventArgs e)
		{
			RefreshReportMappedTablesGrid();
		}

		void RefreshReportMappedTablesGrid()
		{
			var currentItem = GetCurrentSelectedReport();
			stagingTableMappingWrappers?.ForEach(wrapper => wrapper.Reset(currentItem));
		}

		ReportMappingRow GetCurrentSelectedReport()
		{
			if (reportGrid.SelectedItem != null && reportGrid.SelectedItem is DataRowView)
			{
				return ((DataRowView)reportGrid.SelectedItem).Row as ReportMappingRow;
			}
			return null;
		}

		void reportMappingTab_Loaded(object sender, RoutedEventArgs e)
		{
			var stagingTableConfigs = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw);
			stagingTableMappingWrappers.AddRange(stagingTableConfigs.Select(cfg => new StagingTableMappingWrapper
			{
				SourceSchema = cfg.SourceSchema,
				StagingTable = cfg.SourceTable,
			}));
			stagingTableMappingWrappers.Sort();
			var view = new ListCollectionView(stagingTableMappingWrappers);

			reportStagingTableGrid.ItemsSource = view;
			reportGrid.ItemsSource = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingConfig.DefaultView;
		}

		internal class StagingTableMappingWrapper : INotifyPropertyChanged, IComparable<StagingTableMappingWrapper>
		{
			bool mapped;
			public bool Mapped
			{
				get => mapped;
				set
				{
					if (mapped != value && CurrentReport != null)
					{
						var mappingCofig = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingTableConfig;
						var rows = mappingCofig.Where(r => r.ReportName == CurrentReport.Name && r.SourceSchema == SourceSchema && r.StagingTable == StagingTable).ToList();
						if (value && rows.IsNullOrEmpty())
						{
							BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingTableConfig.AddReportMappingTableConfigRow(CurrentReport, SourceSchema, StagingTable);
						}
						else if (!value)
						{
							foreach (var row in rows)
							{
								BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingTableConfig.RemoveReportMappingTableConfigRow(row);
							}
						}
						mapped = value;
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mapped)));
					}
				}
			}
			public void Reset(ReportMappingRow currentReport)
			{
				CurrentReport = currentReport;
				if (CurrentReport is null)
				{
					mapped = false;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mapped)));
					return;
				}

				var mappingCofig = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.ReportMappingTableConfig;
				mapped = mappingCofig.Any(r => r.ReportName == CurrentReport.Name && r.SourceSchema == SourceSchema && r.StagingTable == StagingTable);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mapped)));
			}

			public ReportMappingRow CurrentReport;

			string sourceSchema;
			public string SourceSchema
			{
				get => sourceSchema;
				set => sourceSchema = value;
			}

			string stagingTable;

			public string StagingTable
			{
				get => stagingTable;
				set => stagingTable = value;
			}

			#region Impl
			public int CompareTo(StagingTableMappingWrapper other)
			{
				var mapped = this.mapped.CompareTo(other.mapped);
				var table = stagingTable.CompareTo(other.stagingTable);
				var schema = sourceSchema.CompareTo(other.sourceSchema);
				
				return mapped == 0
					? table == 0
					? schema : table : mapped;
			}

			public event PropertyChangedEventHandler PropertyChanged;
			#endregion Impl
		}

		readonly List<StagingTableMappingWrapper> stagingTableMappingWrappers = new List<StagingTableMappingWrapper>();
	}
	#endregion
}
