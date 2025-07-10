using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobProfitLossControl : ZUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public static class ProfitLossSummaryGridContext
		{
			public const string Ordinary = nameof(Ordinary);
			public const string Consol = nameof(Consol);
		}

		public JobProfitLossControl()
		{
			InitializeComponent();

			SummaryTabPage.TabInitialized += SummaryTabPage_TabInitialized;
			DetailsTabPage.TabInitialized += DetailsTabPage_TabInitialized;
			ProfitLossGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(ProfitLossGrid_ColourDeciding);
			GJCProfitLossGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(ProfitLossGrid_ColourDeciding);

			SetupProfitLossContextMenu();

			ProfitLossGrid.ColumnLayoutContext = ProfitLossSummaryGridContext.Ordinary;
			ProfitLossSummaryGrid.ColumnLayoutContext = ProfitLossSummaryGridContext.Ordinary;

			if (!DesignModeFinder.IsDesigning && !AccountingConfigurationRegistry.Instance.EnableGlobalChargesDetail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				GlobalJobCostingTabPage.TabVisible = false;
			}
		}

		void ProfitLossGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var row = e.ObjectAtRow is ProfitLossDetail ? e.ObjectAtRow as ProfitLossDetail : (e.ObjectAtRow as ProfitLossDetailView).ProfitLossDetail;

			if (row.ZY_Calc_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual)
			{
				e.Colour = Color.LightSalmon;
			}
			if (row.ZY_Calc_LineType == ZArchitecture.Core.TransactionLineTypes.Cost)
			{
				e.Colour = Color.Tomato;
			}
			if (row.ZY_Calc_LineType == ZArchitecture.Core.TransactionLineTypes.WIP)
			{
				e.Colour = Color.PaleGreen;
			}
			if (row.ZY_Calc_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue)
			{
				e.Colour = Color.MediumSeaGreen;
			}
		}

		#region Binding

		protected ZButton DetailsJobProfitReportButton;
		protected ZDropEdit SummaryRecognizedChargesDropEdit;
		protected ZButton JobProfitReportButton;
		protected ZDropEdit DetailsRecognizedChargesDropEdit;
		protected JobProfitLossTotalsControl SummaryJobProfitLossTotalsControl;
		protected ZGrid ProfitLossSummaryGrid;
		protected JobProfitLossTotalsControl DetailsJobProfitLossTotalsControl;
		protected ZGrid ProfitLossGrid;
		protected JobProfitLossTotalsControl GlobalJobProfitLossTotalsControl;
		ZGrid GJCProfitLossGrid;
		protected ZLabel ChargeHidingMessageLabel;
		protected ZLabel SummaryChargeHidingMessageLabel;

		#endregion

		#region Profit and Loss

		internal void RefreshSelectedTab()
		{
			var jobProfitLoss = GetJobProfitLossItem();
			if (jobProfitLoss != null)
			{
				jobProfitLoss.ReloadCollectionsAndRelatedProperties(GetJobProfitLossItemType());
			}
		}

		void SetupProfitLossContextMenu()
		{
			ProfitLossGrid.DoubleClick += new EventHandler(ViewTransaction);

			ProfitLossGrid.ContextMenu.MenuItems.Add(0, new ZMenuItem("-"));

			var reverseWipAccrualMenuItem = new ZMenuItem(reverseWIPAccrualText, new EventHandler(ReverseWIPAccrual));
			ProfitLossGrid.ContextMenu.MenuItems.Add(0, reverseWipAccrualMenuItem);

			var viewMenuItem = new ZMenuItem(viewTransactionText, new EventHandler(ViewTransaction));
			ProfitLossGrid.ContextMenu.MenuItems.Add(1, viewMenuItem);

			var accountingJournalMenuItem = new ZMenuItem(printCFXJournalText, new EventHandler(HandlePrintAccountingJournal));
			ProfitLossGrid.ContextMenu.MenuItems.Add(2, accountingJournalMenuItem);

			var auditTransactionMenuItem = new ZMenuItem(AccountingConstants.AuditAndCashActionText.AuditTransactionText, (sender, e) => AccountingAuditHelper.HandleAuditTransaction(this, new AuditAndCashEventArgs(AuditSecurityCheckpoint, SelectedTransactions)));
			ProfitLossGrid.ContextMenu.MenuItems.Add(3, auditTransactionMenuItem);

			var undoAuditTransactionMenuItem = new ZMenuItem(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText, (sender, e) => AccountingAuditHelper.HandleUndoAuditTransaction(this, new AuditAndCashEventArgs(UndoAuditSecurityCheckpoint, SelectedTransactions)));
			ProfitLossGrid.ContextMenu.MenuItems.Add(4, undoAuditTransactionMenuItem);

			RegenerateJournalEntriesHelper.AddRegenerateJournalEntriesMenuItemIfAllowed(ProfitLossGrid.ContextMenu.MenuItems, HandleRegenerateJournalEntries, 5);

			if (!DesignModeFinder.IsDesigning)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
				{
					var accountingVoucherMenuItem = new ZMenuItem(printAccountingVoucherText, new EventHandler(HandlePrintAccountingVoucher));
					ProfitLossGrid.ContextMenu.MenuItems.Add(6, accountingVoucherMenuItem);
				}
			}

			ProfitLossGrid.ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
			ProfitLossSummaryGrid.ContextMenu.Popup += new EventHandler(SummaryContextMenu_Popup);
		}

		void HandleRegenerateJournalEntries(object sender, EventArgs e)
		{
			if (ProfitLossGrid.GetSelectedElements<ProfitLossDetailView>().Select(x => x.ProfitLossDetail).All(p => IsCFXJournal(p) && !p.ZY_Calc_AH.IsEmpty) && SelectedCFXJournalBusinessObjects.Any())
			{
				RegenerateJournalEntriesHelper.RegenerateJournalEntries(SelectedCFXJournalBusinessObjects);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("AFFCFFD3-F022-4F50-95E2-0830B633C122", "Journal Entries can only be regenerated if all selected Transactions are JC JNL's (CFX Journals)"));
			}
		}

		BusinessObject[] SelectedCFXJournalBusinessObjects
		{
			get
			{
				var selectedProfitLossDetais = ProfitLossGrid.GetSelectedElements<ProfitLossDetailView>().Select(x => x.ProfitLossDetail);
				var query = new ZDBOnlyQuery(typeof(JCJournalHeader));
				query.AddToFilter(AccTransactionHeaderSchema.PK, selectedProfitLossDetais.Select(x => x.ZY_Calc_AH));
				var journals = Factory.Load<JCJournalHeader>(query);
				return journals;
			}
		}

		bool IsCFXJournal(ProfitLossDetail detail)
		{
			if (detail != null && detail.ZY_Calc_TransactionType == TransactionTypes.Journal && detail.ZY_Calc_Ledger == LedgerTypes.JobCosting)
			{
				return true;
			}

			return false;
		}

		readonly MultilingualString reverseWIPAccrualText = ResString.GetMultilingualString("Accounting.JobInvoicing.ReverseWIPAccrual", "Reverse WIP/Accrual");

		readonly MultilingualString printCFXJournalText = ResString.GetMultilingualString("Accounting.JobInvoicing.AccountingJournal", "Print CFX Accounting Journal");

		readonly MultilingualString viewTransactionText = ResString.GetMultilingualString("Accounting.JobInvoicing.ViewTransaction", "View Transaction");

		readonly MultilingualString printAccountingVoucherText = ResString.GetMultilingualString("Accounting.JobInvoicing.AccountingVoucher", "Print Accounting Voucher");

		SecurityCheckpoint AuditSecurityCheckpoint => Env.Security.CFXAuditTransaction;

		SecurityCheckpoint UndoAuditSecurityCheckpoint => Env.Security.CFXUndoAuditTransaction;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected AccTransactionHeader[] SelectedTransactions
		{
			get
			{
				var selectedProfitLossDetais = ProfitLossGrid.GetSelectedElements<ProfitLossDetailView>().Select(x => x.ProfitLossDetail);
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JCJournalHeader));
				query.AddToFilter(AccTransactionHeaderSchema.PK, selectedProfitLossDetais.Select(x => x.ZY_Calc_AH));
				var selectedTransactions = Factory.Load<AccTransactionHeader>(query);
				return selectedTransactions;
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			if (CurrentProfitLossDetail != null)
			{
				ProfitLossGrid.ContextMenu.MenuItems[1].Enabled = CurrentProfitLossDetail.ZY_Calc_LineType == TransactionLineTypes.WIP || CurrentProfitLossDetail.ZY_Calc_LineType == TransactionLineTypes.Accrual;

				var isCFXJournal = IsCFXJournal(CurrentProfitLossDetail);
				ProfitLossGrid.ContextMenu.MenuItems[2].Enabled = isCFXJournal;
				ProfitLossGrid.ContextMenu.MenuItems[3].Enabled = isCFXJournal;
				ProfitLossGrid.ContextMenu.MenuItems[4].Enabled = isCFXJournal;
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
				{
					ProfitLossGrid.ContextMenu.MenuItems[6].Enabled = isCFXJournal;
				}
			}

			RemoveColourGridMenuItem(ProfitLossGrid);
		}

		void SummaryContextMenu_Popup(object sender, EventArgs e)
		{
			RemoveColourGridMenuItem(ProfitLossSummaryGrid);
		}

		void RemoveColourGridMenuItem(ZGrid grid)
		{
			if (grid.ContextMenu.MenuItems["GridColors"] != null)
			{
				grid.ContextMenu.MenuItems.Remove(grid.ContextMenu.MenuItems["GridColors"]);
			}
		}

		void HandlePrintAccountingJournal(object sender, EventArgs e)
		{
			if (IsCFXJournal(CurrentProfitLossDetail))
			{
				var journal = Factory.Load<JCJournalHeader>(CurrentProfitLossDetail.ZY_Calc_AH);
				AccountingJournalPrintHelper.PrintAccountingJournal(new List<TransactionHeader>() { journal });
			}
			else
			{
				Globals.Message.Show(Res.GetString("d0af9365-6874-44c1-90b2-ec0c632e0a93", "Please select a CFX Journal to print."));
			}
		}

		void HandlePrintAccountingVoucher(object sender, EventArgs e)
		{
			if (IsCFXJournal(CurrentProfitLossDetail))
			{
				var accountingVoucherPrintHelper = new AccountingVoucherPrintHelper();
				var transaction = Factory.Load<TransactionHeader>(CurrentProfitLossDetail.ZY_Calc_AH);
				var task = accountingVoucherPrintHelper.GetAccountingVoucherPrintTask(new TransactionHeader[] { transaction });
				if (task != null)
				{
					task.Run(Env.Security.None);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("49E2E037-FDAE-40f3-AB67-8F8E31FD5916", "Please select a CFX Journal to print."));
			}
		}

		void ViewTransaction(object sender, EventArgs e)
		{
			Job job = CurrentDataItem as Job ?? CreateJobFromProfitLossCollection();

			if (job != null && !job.JobHasParent())
			{
				Globals.Message.ShowError(Job.GetJobDoesNotHaveParentMessage());
				return;
			}

			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ViewTransaction))
			{
				SecurityHelper.ShowError(SecurityCore.ViewTransaction);
			}
			else
			{
				if (CurrentProfitLossDetail != null)
				{
					string ledger = CurrentProfitLossDetail.ZY_Calc_Ledger;
					if (string.IsNullOrEmpty(ledger))
					{
						Globals.Message.ShowError(Res.GetString("1a8d51ab-1a1f-406e-b933-1fa3ad346637", @"There is no transaction to view. This line has been prepared on the Job Billing tab, but not yet recognized as an actual WIP, REV, ACR or CST accounting transaction.
Please go to the actual job invoicing screen for more detail on this charge."), Res.GetString("bc187eeb-5fa4-49ca-9d6c-da97f240fe7a", "View Transaction"));
					}
					else
					{
						string transactionType = CurrentProfitLossDetail.ZY_Calc_TransactionType;

						if (transactionType == TransactionLineTypes.WIP)
						{
							WIPController.ShowViewForm(Factory.Load(typeof(WIP), CurrentProfitLossDetail.ZY_Calc_AL));
						}
						else if (transactionType == TransactionLineTypes.Accrual)
						{
							AccrualController.ShowViewForm(Factory.Load(typeof(Accrual), CurrentProfitLossDetail.ZY_Calc_AL));
						}
						else
						{
							bool isOperationInvalid = false;
							try
							{
								TransactionHeader transactionToView = Factory.Load<TransactionHeader>(CurrentProfitLossDetail.ZY_Calc_AH);
								ZController controller = AccountingControllerCreator.GetNewController(transactionToView);
								if (controller != null)
								{
									controller.SetFormsModalTo(PluggedIntoForm);
									controller.ShowViewForm(transactionToView);
								}
								else
								{
									throw new NotSupportedException(string.Format("Can't create controller for transaction with ledger '{0}' and type '{1}'.",
										transactionToView.AH_Ledger, transactionToView.AH_TransactionType));
								}
							}
							catch (ArgumentException ex)
							{
								isOperationInvalid = true;
								ErrorReporter.ReportOnce(ex.Message, "Not supported transaction in JobProfitLossControl.ViewTransactions", ex);
							}
							catch (NotSupportedException ex)
							{
								isOperationInvalid = true;
								ErrorReporter.ReportOnce(ex.Message, "Not supported controller in JobProfitLossControl.ViewTransactions", ex);
							}
							if (isOperationInvalid)
							{
								Globals.Message.ShowError(Res.GetString("f4cf4718-92cf-4048-8170-086cfa160c58", "The operation is not supported for the transaction."));
							}
						}
					}
				}
			}
		}

		void ReverseWIPAccrual(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ReverseWIPACR))
			{
				SecurityHelper.ShowError(SecurityCore.ReverseWIPACR);
			}
			else
			{
				if (CurrentProfitLossDetail != null)
				{
					string ledger = CurrentProfitLossDetail.ZY_Calc_Ledger;
					if (string.IsNullOrEmpty(ledger))
					{
						Globals.Message.ShowError(Res.GetString("ba976731-aab5-4cb8-bac7-7beee6cd946d", @"There is no WIP or Accrual to reverse. This line has been prepared on the Job Billing tab, but not yet recognized as an actual WIP or ACR accounting transaction.
Please go to the actual job invoicing screen to remove or amend this prepared charge."), Res.GetString("2535ba05-1a67-48f4-b6d4-5ad80b917935", "Reverse WIP/Accrual"));
					}
					else
					{
						string transactionType = CurrentProfitLossDetail.ZY_Calc_TransactionType;

						if (transactionType == TransactionLineTypes.WIP)
						{
							var wip = new BusinessObjectFactory().Load<WIP>(CurrentProfitLossDetail.ZY_Calc_AL);
							ShowWIPAccrualForReversal(wip, GetNewWIPController());
						}
						else if (transactionType == TransactionLineTypes.Accrual)
						{
							var accrual = new BusinessObjectFactory().Load<Accrual>(CurrentProfitLossDetail.ZY_Calc_AL);
							ShowWIPAccrualForReversal(accrual, GetNewAccrualController());
						}
					}
				}
			}
		}

		void ShowWIPAccrualForReversal(BaseWIPAccrual wIPAccrual, ZController controller)
		{
			if (wIPAccrual.IsReversed)
			{
				Globals.Message.ShowError(Res.GetString("cc0e6bfb-2fbb-4593-8fa0-4c1681b50cb2", "This transaction has already been reversed."), Res.GetString("2535ba05-1a67-48f4-b6d4-5ad80b917935", "Reverse WIP/Accrual"));
			}
			else if (wIPAccrual.IsApportioned)
			{
				ZString message = Res.GetString("184ff99c-6405-48d8-97bd-cf21838c431f", "This accrual is apportioned at Consol level.") + "\r\n";
				message += Res.GetString("631cf2fe-3df5-45e8-ba7f-8c088c128f1d", "Please go to the Costing Tab of Consol {0} to reverse this accrual.", wIPAccrual.JK_UniqueConsignRef);
				Globals.Message.ShowError(message, Res.GetString("2535ba05-1a67-48f4-b6d4-5ad80b917935", "Reverse WIP/Accrual"));
			}
			else if (!wIPAccrual.CanReverseWhenRelatedJobStatusIsJFC)
			{
				Globals.Message.ShowError(Res.GetString("82185CF8-82AA-45D8-80BE-73450674995B", "This transaction cannot be reversed as the related job has Ready For Financial Closure status."), Res.GetString("2535ba05-1a67-48f4-b6d4-5ad80b917935", "Reverse WIP/Accrual"));
			}
			else
			{
				var deleteForm = controller.ShowDeleteForm(wIPAccrual);
				if (deleteForm != null)
				{
					deleteForm.Closed += new EventHandler(LastShownForm_Closed);
				}
			}
		}

		void LastShownForm_Closed(object sender, EventArgs e)
		{
			var form = sender as IZForm;
			if (form != null)
			{
				form.Closed -= new EventHandler(LastShownForm_Closed);
			}
			FindButton.PerformClick();
		}

		IDisposable JobProfitLossRefreshBindingSuspender;

		void TabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			var jobProfitLoss = GetJobProfitLossItem();
			JobProfitLossRefreshBindingSuspender = jobProfitLoss?.RefreshBindingSuspender.GetSuspender();
		}

		void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			JobProfitLossRefreshBindingSuspender?.Dispose();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (isSummaryTabPageTabInitialized)
			{
				HandleSummaryChargeHidingMessageLabelVisibility();
			}
		}

		void SummaryTabPage_TabInitialized(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				HandleSummaryChargeHidingMessageLabelVisibility();
			}
			isSummaryTabPageTabInitialized = true;
		}

#if DEBUG
		protected virtual
#endif
		void DetailsTabPage_TabInitialized(object sender, EventArgs e)
		{
			var jobProfitLoss = GetJobProfitLossItem();
			if (jobProfitLoss != null && !ChargeHidingMessageLabel.Visible &&
				jobProfitLoss.ProfitLossFilteredDetails.Count < jobProfitLoss.ProfitLossDetails.Count)
			{
				ChargeHidingMessageLabel.Visible = true;
			}
		}

#if DEBUG
		protected virtual
#endif
		void HandleSummaryChargeHidingMessageLabelVisibility()
		{
			var jobProfitLoss = GetJobProfitLossItem();
			if (jobProfitLoss != null &&
				!SummaryChargeHidingMessageLabel.Visible &&
				jobProfitLoss.ProfitLossSummaryFilteredDetails != null &&
				jobProfitLoss.ProfitLossSummaryDetails != null &&
				jobProfitLoss.ProfitLossSummaryFilteredDetails.Count < jobProfitLoss.ProfitLossSummaryDetails.Count)
			{
				SummaryChargeHidingMessageLabel.Visible = true;
			}
		}

		bool isSummaryTabPageTabInitialized;

		#region Controllers

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;

		ZController fWIPController;

		ZController WIPController
		{
			get
			{
				if (fWIPController == null)
				{
					fWIPController = GetNewWIPController();
				}
				return fWIPController;
			}
		}

		ZController GetNewWIPController()
		{
			var wipController = ZControllerFactory.Create(ControllerIDs.WIP);
			if (PluggedIntoForm != null)
			{
				wipController.SetFormsModalTo(PluggedIntoForm);
			}
			return wipController;
		}

		ZController fAccrualController;
		ZController AccrualController
		{
			get
			{
				if (fAccrualController == null)
				{
					fAccrualController = GetNewAccrualController();
				}
				return fAccrualController;
			}
		}

		ZController GetNewAccrualController()
		{
			var accrualController = ZControllerFactory.Create(ControllerIDs.Accrual);
			if (PluggedIntoForm != null)
			{
				accrualController.SetFormsModalTo(PluggedIntoForm);
			}
			return accrualController;
		}

		Form PluggedIntoForm
		{
			get { return FindForm(); }
		}

		#endregion

		#endregion

		#region Find / Clear

		void FindButton_Click(object sender, EventArgs e)
		{
			RefreshSelectedTab();
		}

		JobProfitLoss GetJobProfitLossItem()
		{
			var jobProfitLoss = CurrentDataItem as JobProfitLoss;
			if (jobProfitLoss == null)
			{
				IJobCostingPlugIn plugin = CurrentDataItem as IJobCostingPlugIn;
				jobProfitLoss = GetJobProfitLossFromPlugin(plugin);
			}
			return jobProfitLoss;
		}

		JobProfitLoss.JobProfitLossType GetJobProfitLossItemType()
		{
			if (TabControl.SelectedTab == DetailsTabPage)
			{
				return JobProfitLoss.JobProfitLossType.Details;
			}
			else if (TabControl.SelectedTab == GlobalJobCostingTabPage)
			{
				return JobProfitLoss.JobProfitLossType.Global;
			}
			else
			{
				return JobProfitLoss.JobProfitLossType.Summary;
			}
		}

		JobProfitLoss GetJobProfitLossFromPlugin(IJobCostingPlugIn plugin)
		{
			if (plugin != null && plugin.ProfitLossContainer != null)
			{
				return plugin.ProfitLossContainer.FirstOrDefault() as JobProfitLoss;
			}

			return null;
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			var jobProfitLoss = GetJobProfitLossItem();
			if (jobProfitLoss != null)
			{
				jobProfitLoss.Filter.ClearFilterValues();
				jobProfitLoss.ReloadCollectionsAndRelatedProperties(GetJobProfitLossItemType());
			}
		}

		#endregion

		#region Current Items

		public ProfitLossDetail CurrentProfitLossDetail
		{
			get
			{
				var currentProfitLossDetailView = ProfitLossGrid.ListManager.GetCurrent() as ProfitLossDetailView;
				return currentProfitLossDetailView != null ? currentProfitLossDetailView.ProfitLossDetail : null;
			}
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				JobProfitLossRefreshBindingSuspender?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			JobProfitLoss jobProfitLoss = CurrentDataItem as JobProfitLoss;
			if (jobProfitLoss != null && jobProfitLoss.Parent != null)
			{
				if (jobProfitLoss.Parent is IJobInvoicingPlugIn plugInData)
				{
					bool isShipment = plugInData.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.Shipment;
					if (isShipment &&
						(
							CheckGlobalChargeAdministrationUser() ||
							CheckGlobalChargeConsolSendingAgent(plugInData) ||
							CheckGlobalChargeConsolReceivingAgent(plugInData) ||
							CheckGlobalChargeConsolSendingAgentSettlementGroup(plugInData) ||
							CheckGlobalChargeConsolReceivingAgentSettlementGroup(plugInData) ||
							CheckGlobalChargeControllingAgent(plugInData)
						)
						|| !isShipment && SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.GlobalChargeDetails))
					{
						GlobalJobCostingSecurityLabel.Visible = false;
					}
					else
					{
						GlobalJobCostingTabPanel.Visible = false;
						GlobalJobCostingSecurityLabel.Text = SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.GlobalChargeDetails);
					}
				}
			}
		}

		#region GlobalChargeDetails

		ZGuid CompanyProxyPk => GlbCompany.CurrentCompany.OrgProxy.PK;
		ZGuid? BranchProxyPk => GlbBranch.CurrentBranch.OrgProxy?.PK;

		bool CheckGlobalChargeAdministrationUser()
		{
			return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.GlobalChargeAdministrationUser);
		}

		bool CheckGlobalChargeConsolSendingAgent(IJobInvoicingPlugIn plugInData)
		{
			var sendingAgent = plugInData.InvoicingSupporter.SendingAgent;
			return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.GlobalChargeConsolSendingAgent) && (sendingAgent?.PK == CompanyProxyPk || BranchProxyPk != null && sendingAgent?.PK == BranchProxyPk);
		}

		bool CheckGlobalChargeConsolReceivingAgent(IJobInvoicingPlugIn plugInData)
		{
			var receivingAgent = plugInData.InvoicingSupporter.ReceivingAgent;
			return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.GlobalChargeConsolReceivingAgent) && (receivingAgent?.PK == CompanyProxyPk || BranchProxyPk != null && receivingAgent?.PK == BranchProxyPk);
		}

		bool CheckGlobalChargeConsolSendingAgentSettlementGroup(IJobInvoicingPlugIn plugInData)
		{
			var relatedPartyOfSendingAgent = plugInData.InvoicingSupporter.SendingAgent?.GetRelatedParty(RelatedPartyTypeList.Codes.ARSettlementGroup, LedgerTypes.AccountsReceivable);
			return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.GlobalChargeConsolSendingAgentSettlementGroup) && (relatedPartyOfSendingAgent?.PK == CompanyProxyPk || BranchProxyPk != null && relatedPartyOfSendingAgent?.PK == BranchProxyPk);
		}

		bool CheckGlobalChargeConsolReceivingAgentSettlementGroup(IJobInvoicingPlugIn plugInData)
		{
			var relatedPartyOfReceivingAgent = plugInData.InvoicingSupporter.ReceivingAgent?.GetRelatedParty(RelatedPartyTypeList.Codes.ARSettlementGroup, LedgerTypes.AccountsReceivable);
			return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.GlobalChargeConsolReceivingAgentSettlementGroup) && (relatedPartyOfReceivingAgent?.PK == CompanyProxyPk || BranchProxyPk != null && relatedPartyOfReceivingAgent?.PK == BranchProxyPk);
		}

		bool CheckGlobalChargeControllingAgent(IJobInvoicingPlugIn plugInData)
		{
			var relatedPartyOfConsignee = plugInData.InvoicingSupporter.Consignee?.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);
			var relatedPartyOfConsignor = plugInData.InvoicingSupporter.Consignor?.GetRelatedParty(RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyDirectionList.Codes.Sales);
			return SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.GlobalChargeControllingAgent) && (relatedPartyOfConsignee?.PK == CompanyProxyPk || BranchProxyPk != null && relatedPartyOfConsignee?.PK == BranchProxyPk) || (relatedPartyOfConsignor?.PK == CompanyProxyPk || BranchProxyPk != null && relatedPartyOfConsignor?.PK == BranchProxyPk);
		}

		#endregion

		protected void JobProfitReportButton_Click(object sender, EventArgs e)
		{
			PrintJobProfitDocument();
		}

		protected virtual void PrintJobProfitDocument()
		{
			Job job = CurrentDataItem as Job ?? CreateJobFromProfitLossCollection();

			if (job != null && !job.JobHasParent())
			{
				Globals.Message.ShowError(Job.GetJobDoesNotHaveParentMessage());
				return;
			}

			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PrintJob))
			{
				SecurityHelper.ShowError(SecurityCore.PrintJob);
			}
			else
			{
				if (job != null)
				{
					if (!job.HasChanges)
					{
						JobDocumentPrinter docPrinter = new JobDocumentPrinter(Factory);
						if (ZFormModaliser.ShowDialogAndDispose(new JobProfitDocumentPrintingForm(docPrinter)) == DialogResult.OK)
						{
							docPrinter.IsProfitLossDoc = true;
							JobDocumentPrintItem docPrintItem = new JobDocumentPrintItem(docPrinter, job, Factory);
							docPrinter.PrintJobProfitDocuments(Factory, docPrintItem);
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("fc16f5a0-0dee-40d5-97c8-6d208fafffab", "Please save this Invoicing Job before printing the Job Profit Document"));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("089fb171-7056-4f21-9aa5-447eb1cc4155", "There was an error preparing the Job Profit Document"));
				}
			}
		}

		Job CreateJobFromProfitLossCollection()
		{
			var profitLoss = CurrentDataItem as JobProfitLoss;
			var jobPK = (profitLoss != null && profitLoss.JobPKs.Length > 0) ? profitLoss.JobPKs[0] : ZGuid.Empty;
			var job = jobPK != Guid.Empty ? Factory.Load<Job>(jobPK) : null;
			return job;
		}

		#region Security

		public virtual SecurityCheckpoint PluginSecurity
		{
			get { return pluginSecurity; }
			set
			{
				pluginSecurity = value;
				ResetSecurityHelper();
			}
		}
		SecurityCheckpoint pluginSecurity;

		protected JobInvoicingSecurityHelper SecurityHelper
		{
			get { return securityHelper ?? (securityHelper = new JobInvoicingSecurityHelper(PluginSecurity, false)); }
		}
		JobInvoicingSecurityHelper securityHelper;

		void ResetSecurityHelper()
		{
			securityHelper = null;
		}

		#endregion

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return control == SummaryJobProfitLossTotalsControl && previousControl == JobProfitReportButton ||
				control == DetailsJobProfitLossTotalsControl && previousControl == DetailsJobProfitReportButton;
		}

		#endregion
	}
}

