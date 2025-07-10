using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Bi.Product.Manager.GUI
{
	public partial class AuditControl : ZUserControl
	{
		#region SuppressResourceStringsCheckRegion

		public AuditControl()
		{
			this.SkipSettingChildControlReadOnly = true;
			InitializeComponent();
			cdcHistorySummaryToDateBox.DateTimeValue = ZDateTime.Now;
			cdcHistorySummaryFromDateBox.DateTimeValue = ZDateTime.Now.AddDays(-1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3012:AvoidBoolLiteralsInLargerBoolExpressions", Justification = "#if directive")]
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (
#if DEBUG
					true ||
#endif
					Env.CurrentUser.IsSupportUser || ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem())
			{
				cdcHistorySummaryTabPage.TabVisible = true;
			}
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
						bo.RefreshAuditInformation();
						if (!parentForm.IsDisposed && !parentForm.IsDisposing)
						{
							Invoke(new Action(() =>
							{
								auditTablesGrid.SetDataBinding(bo.AuditInformation, "AuditTables");
								dataLossGrid.SetDataBinding(bo.AuditInformation, "DataLoss");
								subscribersGrid.SetDataBinding(bo.AuditInformation, "Subscribers");
								RecreateHandle();

								var index = zTabControl1.SelectedIndex;
								foreach (ZTabPage tabPage in zTabControl1.TabPages)
								{
									if (tabPage.TabVisible)
									{
										zTabControl1.SelectedTab = tabPage;
									}
								}
								zTabControl1.SelectedIndex = index;
							}));
						}
					}

					tokenSource.Cancel();
				}
			});
		}

		void CdcHistorySummaryFilterButton_Click(object sender, EventArgs e)
		{
			ThreadRunner.RunInAnotherThread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var tokenSource = new CancellationTokenSource();
					var parentForm = (this.ParentForm as BiManagerForm);
					parentForm?.ShowLoadingTextAsync(tokenSource.Token);

					var isUtcChecked = cdcHistorySummaryUtcCheckBox.Checked;

					var tableFilter = cdcHistorySummaryTableNameTextBox.Text;
					var fromDateFilter = isUtcChecked ?
						cdcHistorySummaryFromDateBox.DateTimeValue :
						cdcHistorySummaryFromDateBox.DateTimeValue.IsEmpty ?
							ZDateTime.Empty :
							new ZDateTime(Env.Time.GetUtcFromLocalTime(cdcHistorySummaryFromDateBox.DateTimeValue.ToDateTime()));
					var toDateFilter = isUtcChecked ?
						cdcHistorySummaryToDateBox.DateTimeValue :
						cdcHistorySummaryToDateBox.DateTimeValue.IsEmpty ?
							ZDateTime.Empty :
							new ZDateTime(Env.Time.GetUtcFromLocalTime(cdcHistorySummaryToDateBox.DateTimeValue.ToDateTime()));

					if (!fromDateFilter.IsEmpty && !toDateFilter.IsEmpty && fromDateFilter > toDateFilter)
					{
						using (var msgBox = new ZMessageBox("Invalid date range provided.", "Filter Results", MessageBoxButtons.OK, MessageBoxIcon.Information))
						{
							ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
						}
					}
					else
					{
						var bo = (BindingSource.DataSource as BiMonitorBusinessObject);
						int summaryCount = 0;
						if (bo != null)
						{
							summaryCount = bo.FilterCdcHistorySummary(tableFilter, fromDateFilter, toDateFilter);
							if (!parentForm.IsDisposed && !parentForm.IsDisposing)
							{
								Invoke(new Action(() =>
								{
									cdcHistorySummaryGrid.SetDataBinding(bo.AuditInformation, "CdcHistorySummary");
									RecreateHandle();

									if (bo.AuditInformation.CdcHistorySummary.Count < summaryCount)
									{
										cdcHistorySummaryTextBox.Text = string.Format(CultureInfo.InvariantCulture, "{0} out of {1} results", bo.AuditInformation.CdcHistorySummary.Count, summaryCount);
										var message = string.Format(CultureInfo.InvariantCulture, "Showing {0} out of {1} results.", bo.AuditInformation.CdcHistorySummary.Count, summaryCount);
										using (var msgBox = new ZMessageBox(message, "Filter Results", MessageBoxButtons.OK, MessageBoxIcon.Information))
										{
											ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
										}
									}
									else
									{
										cdcHistorySummaryTextBox.Text = string.Format(CultureInfo.InvariantCulture, "{0} result(s)", bo.AuditInformation.CdcHistorySummary.Count);
									}
								}));
							}
						}
					}

					tokenSource.Cancel();
				}
			});
		}

		#endregion
	}
}
