using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync;
using CargoWise.Common;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
	public partial class MainWindow : Window, IDisposable
	{
		#region Analysis Models

		void ssasTablesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RefreshQueryBox();
		}

		BiAutomationConfigDataSet.SsasCubesRow GetSsasCubeCurrentItemRow()
		{
			BiAutomationConfigDataSet.SsasCubesRow result = null;

			if (ssasCubesGrid.SelectedItem != null && ((DataRowView)ssasCubesGrid.SelectedItem).Row != null)
			{
				result = (BiAutomationConfigDataSet.SsasCubesRow)((DataRowView)ssasCubesGrid.SelectedItem).Row;
				if (result.RowState == DataRowState.Detached)
				{
					result = null;
				}
			}

			return result;
		}

		BiAutomationConfigDataSet.SsasTablesRow GetSsasTableCurrentItemRow()
		{
			BiAutomationConfigDataSet.SsasTablesRow result = null;

			if (ssasTablesGrid.SelectedItem != null && ((DataRowView)ssasTablesGrid.SelectedItem).Row != null)
			{
				result = (BiAutomationConfigDataSet.SsasTablesRow)((DataRowView)ssasTablesGrid.SelectedItem).Row;
				if (result.RowState == DataRowState.Detached)
				{
					result = null;
				}
			}

			return result;
		}

		void RefreshQueryBox()
		{
			var currentItem = GetSsasTableCurrentItemRow();
			if (currentItem != null)
			{
				QueryTextBox.Text = (!currentItem.IsCalculated) ? FormatQuery(currentItem.Query) : currentItem.Query;
			}
		}

		string FormatQuery(string query)
		{
			var result = query;

			if (!string.IsNullOrEmpty(result))
			{
				result = result.Trim(new[] { ' ', '\r', '\n' });

				var regex = new Regex(@"\s+(?=([^\]])*(?=(\[|$)))", RegexOptions.IgnoreCase);
				result = regex.Replace(result, " ");

				regex = new Regex(@"^SELECT\s+", RegexOptions.IgnoreCase);
				result = regex.Replace(result, "SELECT\r\n\t");

				regex = new Regex(@"\s*,\s*", RegexOptions.IgnoreCase);
				result = regex.Replace(result, ",\r\n\t");

				regex = new Regex(@"\s+FROM\s+(?=([^\]])*(?=(\[|$)))", RegexOptions.IgnoreCase);
				result = regex.Replace(result, "\r\nFROM ");

				regex = new Regex(@"\s+(?<JoinType>(INNER|LEFT|RIGHT|FULL)\s+)*JOIN\s+(?=([^\]])*(?=(\[|$)))", RegexOptions.IgnoreCase);
				Match match = regex.Match(result);
				if (match.Success)
				{
					var joinType = match.Groups["JoinType"].Value;
					result = regex.Replace(result, string.Format(CultureInfo.InvariantCulture, "\r\n{0}JOIN ", joinType));
				}

				regex = new Regex(@"\s+ON\s+(?=([^\]])*(?=(\[|$)))", RegexOptions.IgnoreCase);
				result = regex.Replace(result, "\r\n\tON ");
			}

			return result;
		}

		void ssasCubesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var currentItem = GetSsasCubeCurrentItemRow();
			if (currentItem != null)
			{
				ssasTablesGrid.ItemsSource =
							new DataView(BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SsasTables,
								"SsasModelLogicalName = '" + currentItem.SsasModelLogicalName + "'",
								"TableName",
								DataViewRowState.CurrentRows);
				RefreshQueryBox();
			}
		}

		void TestQueryButton_Click(object sender, RoutedEventArgs e)
		{
			Invoke(() =>
			{
				BiFiles.CWSharedPath = CWSharedPath;
				var ssasCube = GetSsasCubeCurrentItemRow();
				var ssasTable = GetSsasTableCurrentItemRow();
				var source = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", ssasCube.SsasModelFileName, ssasTable.TableName);
				if (!ssasTable.IsCalculated)
				{
					var errorMsg = SchemaSynchroniser.TestSingleQuery(source, QueryTextBox.Text, "Partition Query");
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
					BiLogger.Fail(ssasTable.TableName + " is a calculated table and DAX query cannot be tested");
				}
			});
		}

		void CopyQueryToClipboard_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(QueryTextBox.Text.Trim()))
			{
				Clipboard.SetText(QueryTextBox.Text.Trim()); // Not using ZorKArchitecture
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void testQueryForAllModelsButton_Click(object sender, RoutedEventArgs e)
		{
			Invoke(() =>
			{
				try
				{
					BiFiles.CWSharedPath = CWSharedPath;
					SchemaSynchroniser.TestQueryForAllEdwTables();
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

		#endregion
	}
	#endregion
}
