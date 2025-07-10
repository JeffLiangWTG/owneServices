using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Bi.Product.Manager.GUI
{
	public partial class EdwControl : ZUserControl
	{
		#region SuppressResourceStringsCheckRegion

		public EdwControl()
		{
			this.SkipSettingChildControlReadOnly = true;
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			RefreshInfo();
		}

		void refreshButton_Click(object sender, EventArgs e)
		{
			RefreshInfo();
		}

		public void RefreshInfo()
		{
			ThreadRunner.RunInAnotherThread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var tokenSource = new CancellationTokenSource();
					var parentForm = (this.ParentForm as BiManagerForm);
					parentForm?.ShowLoadingTextAsync(tokenSource.Token);

					var bo = (BindingSource.DataSource as BiMonitorBusinessObject);
					if (bo != null)
					{
						bo.RefreshEdwInformation();
					}
					if (!parentForm.IsDisposed && !parentForm.IsDisposing)
					{
						Invoke(new Action(() =>
						{
							sourceTablesGrid.SetDataBinding(bo.EdwInformation, "EdwSourceTables");
							stagingTablesGrid.SetDataBinding(bo.EdwInformation, "EdwStagingTables");
							baseTablesGrid.SetDataBinding(bo.EdwInformation, "EdwBaseTables");
							aggregateTablesGrid.SetDataBinding(bo.EdwInformation, "EdwAggregateTables");
							customTablesGrid.SetDataBinding(bo.EdwInformation, "EdwCustomTables");
							RecreateHandle();
						}));
					}

					tokenSource.Cancel();
				}
			});
		}

		void initialLoadButton_Click(object sender, EventArgs e)
		{
			var selectedStagingTables = stagingTablesGrid.SelectedElements.Select(r => ((EdwStagingTable)r).Name);
			var selectedBaseTables = baseTablesGrid.SelectedElements.Select(r => ((EdwBaseTable)r).Name);
			var selectedAggregateTables = aggregateTablesGrid.SelectedElements.Select(r => ((EdwAggregateTable)r).Name);
			var selectedCustomTables = customTablesGrid.SelectedElements.Select(r => ((EdwCustomTable)r).Name);

			var totalCount = selectedStagingTables.Count() + selectedBaseTables.Count() + selectedAggregateTables.Count() + selectedCustomTables.Count();

			string message;

			if (!selectedStagingTables.Any() && !selectedBaseTables.Any() && !selectedAggregateTables.Any() && !selectedCustomTables.Any())
			{
				message = "This will run initial load for all tables during the next EDW ETL execution. Are you sure?";
			}
			else if (totalCount > 20)
			{
				message = string.Format(CultureInfo.InvariantCulture, "This will run initial load for {0} tables and their dependencies during the next EDW ETL execution. Are you sure?", totalCount);
			}
			else
			{
				message = string.Format(CultureInfo.InvariantCulture, "This will run initial load for the following tables and their dependencies during the next EDW ETL execution:{0}{1}{2}{3}\r\n\r\nAre you sure?",
					selectedStagingTables.Any() ? "\r\nStaging Tables:\r\n\t" + string.Join("\r\n\t", selectedStagingTables) : "",
					selectedBaseTables.Any() ? "\r\nBase Tables:\r\n\t" + string.Join("\r\n\t", selectedBaseTables) : "",
					selectedAggregateTables.Any() ? "\r\nAggregate Tables:\r\n\t" + string.Join("\r\n\t", selectedAggregateTables) : "",
					selectedCustomTables.Any() ? "\r\nCustom Tables:\r\n\t" + string.Join("\r\n\t", selectedCustomTables) : "");
			}

			using (var msgBox = new ZMessageBox(message, "Run Initial Load", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
			{
				if (msgBox.ShowDialog() == DialogResult.Yes)
				{
					ThreadRunner.RunInAnotherThread(() =>
					{
						EdwTableManager.RunInitialLoad(selectedStagingTables, selectedBaseTables, selectedAggregateTables, selectedCustomTables);
						refreshButton_Click(sender, e);
					});
				}
			}
		}

		void updateTranslationButton_Click(object sender, EventArgs e)
		{
			string message = "This will run incremental load for the Translation Lookup table during the next EDW ETL execution. Are you sure?";

			using (var msgBox = new ZMessageBox(message, "Update Translation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
			{
				if (msgBox.ShowDialog() == DialogResult.Yes)
				{
					ThreadRunner.RunInAnotherThread(() =>
					{
						EdwTableManager.UpdateTranslationTable();
					});
				}
			}
		}

		#endregion
	}
}
