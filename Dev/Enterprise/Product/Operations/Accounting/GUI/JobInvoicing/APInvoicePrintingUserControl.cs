using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class APInvoicePrintingUserControl : ZUserControl
	{
		IAPInvoicePrintingUserControlPresentationProvider APInvoicePrintingUserControlPresentationProvider => aPInvoicePrintingUserControlPresentationProvider ?? (aPInvoicePrintingUserControlPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetAPJobInvoicePrintingUserControlPresentationProvider());
		IAPInvoicePrintingUserControlPresentationProvider aPInvoicePrintingUserControlPresentationProvider;

		public APInvoicePrintingUserControl()
		{
			InitializeComponent();
			SetupAPInvoicesContextMenu();
			RemoveComplianceSubTypeColumnIfApplicable();
			RemoveTaxBranchColumnIfApplicable();
			RemoveEInvoicingColumnsIfApplicable();
		}

		#region Binding

		JobAPInvoicePrintingFilter InvoiceFilterObject;
		bool IsAlreadyBound;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (this.InvoiceFilterObject != null && this.InvoiceFilterObject.FilteredTransactions.Count < this.InvoiceFilterObject.Transactions.Count)
			{
				this.InvoiceHidingMessageLabel.Visible = true;
			}
		}

		public void Bind(JobAPInvoicePrintingFilter invoiceFilterObject)
		{
			this.InvoiceFilterObject = invoiceFilterObject;
			if (!IsAlreadyBound)
			{
				if (!invoiceFilterObject.IsFreightConsol)
				{
					SetupForJobShipment();
				}
				base.SetDataBinding(invoiceFilterObject, "");
				IsAlreadyBound = true;
			}
		}

		protected void SetupForJobShipment()
		{
			JobHeaderFindBox.Visible = false;
			ControlDpiScalingHelper.SetTop(ref FindButton, TransactionTypeDropEdit.Top, false);
			ControlDpiScalingHelper.SetTop(ref ClearButton, TransactionTypeDropEdit.Top, false);
			ControlDpiScalingHelper.SetHeight(FilterPanel, FilterPanel.Height - JobHeaderFindBox.Height, false);
			InvoiceHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 70, true);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// Do Nothing. Handling manually.
		}

		#endregion

		#region Implementation

		public static MultilingualString PrintMenuText
		{
			get { return ResString.GetMultilingualString("1b9c0b9a-12ee-4066-8cc5-7207d894ae78", "&Print"); }
		}
		protected static MultilingualString PrintMatchDocMenuText
		{
			get { return ResString.GetMultilingualString("7ca4621d-81bd-4e96-9a9d-a6e1fb392eab", "Print &Match Doc"); }
		}
		protected static MultilingualString PrintTransactionMenuText
		{
			get { return ResString.GetMultilingualString("4419e8a7-aad1-4c52-9d60-0236ea889d0c", "Print Transaction"); }
		}
		protected static MultilingualString PrintSelfBillingInvoiceMenuText
		{
			get { return ResString.GetMultilingualString("1d0dbf1c-9dbc-4175-859d-81e78c5684d1", "Print &Self Billing Invoice"); }
		}
		protected static MultilingualString ViewMenuItemText
		{
			get { return ResString.GetMultilingualString("373de15e-c208-423f-8c7d-be671b06d99d", "&View"); }
		}

		void RemoveComplianceSubTypeColumnIfApplicable()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
				{
					RemoveGridColumn(AccTransactionHeader.Schema.AH_ComplianceSubType);
					RemoveGridColumn(AccTransactionHeader.Schema.AH_TransactionReference);
				}
			}
		}

		void RemoveTaxBranchColumnIfApplicable()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!APInvoicePrintingUserControlPresentationProvider.IsTaxBranchColumnAvailable())
				{
					RemoveGridColumn(nameof(TransactionHeader.Schema.AH_GB_TaxBranch));
				}
			}
		}

		void RemoveEInvoicingColumnsIfApplicable()
		{
			if (JobInvoicePrintingControlPresentationProvider?.IsEInvoicingColumnsAvailable(LedgerTypes.AccountsPayable) == false)
			{
				RemoveGridColumn(TransactionHeader.Schema.EInvoicingBatchNumber);
				RemoveGridColumn(TransactionHeader.Schema.EInvoicingAuthorisationNumber);
				RemoveGridColumn(TransactionHeader.Schema.EInvoicingeHubAllocatedNumber);
				RemoveGridColumn(TransactionHeader.Schema.EInvoicingError);
				RemoveGridColumn(TransactionHeader.Schema.EInvoicingGovernmentAllocatedNumber);
				RemoveGridColumn(TransactionHeader.Schema.EInvoicingLastResponseReceivedUtc);
				RemoveGridColumn(TransactionHeader.Schema.EInvoicingLastSentTimeUtc);
				RemoveGridColumn(TransactionHeader.Schema.EInvoicingStatus);
			}
		}

		void RemoveGridColumn(ZString columnName)
		{
			foreach (Core.Forms.ZGridColumnInfo columnInfo in APInvoicesGrid.ColumnStyles)
			{
				if (columnInfo.ColumnName == columnName)
				{
					APInvoicesGrid.ColumnStyles.Remove(columnInfo);
					break;
				}
			}
		}

		void SetupAPInvoicesContextMenu()
		{
			APInvoicesGrid.ContextMenu.MenuItems.Add(0, new ZMenuItem(ViewMenuItemText, HandleViewClick));
			APInvoicesGrid.ContextMenu.MenuItems.Add(1, new ZMenuItem(ResString.GetMultilingualString("FD676CC9-FDC2-47E9-8623-2EC417A68A42", "&Edit"), HandleEditClick));

			MenuItem printMenuItem = new ZMenuItem(PrintMenuText);
			printMenuItem.MenuItems.Add(new ZMenuItem(PrintTransactionMenuText, HandlePrint));
			printMenuItem.MenuItems.Add(new ZMenuItem(PrintMatchDocMenuText, HandlePrintMatchingReport));
			printMenuItem.MenuItems.Add(new ZMenuItem(PrintSelfBillingInvoiceMenuText, HandlePrintSelfBillingInvoice));
			APInvoicesGrid.ContextMenu.MenuItems.Add(printMenuItem);

			APInvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("93D08156-D24D-4F2D-AAB6-0CE0DDB31367", "Amend with Credit Note"), AmendWithCreditNote));

			APInvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("8D865904-D664-488F-A435-F71BB2397DD1", "Edit Requisition Date and Status"), new EventHandler(HandleEditRequisition)));
			if (!DesignModeFinder.IsDesigning)
			{
				if (GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
				{
					APInvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.UpdateTaxInvoiceNumber_Payables", "Update Compliance Sub Type and/or Number"), new EventHandler(UpdateComplianceDetailsOnInvoices)));
					if (!AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
					{
						if (GlbCompany.CurrentCompany.Country.HasAccComplianceSequence)
						{
							APInvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.UpdateComplianceNumber_Payables", "Allocate Compliance Number"), new EventHandler(UpdateComplianceNumber)));
						}
					}
				}
			}

			APInvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("178831A9-9D55-468B-A920-9E125191176A", "Override Transaction Description"), HandleOverrideTransactionDescription));
		}

		void AmendWithCreditNote(object sender, EventArgs e)
		{
			if (SelectedTransactionInInvoicesGrid == null)
			{
				InvoicePrintingUserControlHelper.ShowNoSelectedMessage();
				return;
			}

			var factory = new BusinessObjectFactory();
			var originalTransaction = factory.Load<InvoicingBase>(SelectedTransactionInInvoicesGrid.PK);

			var amending = CreditNoteAmendingHelper.AmendAPTransaction(originalTransaction, SecurityHelper,
																		(message, caption) => SecurityHelper.ShowError(message),
																		(message, caption) => Globals.Message.ShowInformation(message, caption));

			if (amending != null)
			{
				var transactionHeader = (InvoicingBase)amending;
				var controller = AccountingControllerCreator.GetNewController(transactionHeader);

				transactionHeader.SetContext(BusinessContext.AmendingInvoice);
				var amendingForm = controller.ShowFormForNewEntity(transactionHeader);

				if (amendingForm != null)      //This check occurs due to security checking in ShowFormForNewEntity, which may return null if permissions are not valid.
				{
					amendingForm.Closed += new EventHandler(AmendingForm_Closed);
				}
#if DEBUG
				AmendingForm_ForTestOnly = (ZForm)amendingForm;
#endif
			}
		}

		void AmendingForm_Closed(object sender, EventArgs e)
		{
			var form = sender as ZForm;
			if (form != null && form.BusinessEntity.IsInDatabaseIncludingChildren)
			{
				var parentForm = ParentForm as ZForm;
				var amending = form.BusinessEntity as IAmending;
				if (amending != null && amending.IsAmendingTransaction &&
					parentForm != null && parentForm.BusinessEntity != null)
				{
					var amendingTransaction = parentForm.BusinessEntity.Factory.Load<InvoicingBase>(amending.PK);
					amendingTransaction.RemoveContext(BusinessContext.AmendingInvoice);
					RefreshAfterAmending(amendingTransaction);
				}

				form.Closed -= new EventHandler(AmendingForm_Closed);
			}
		}

		void RefreshAfterAmending(InvoicingBase transaction)
		{
			if (transaction != null)
			{
				var jobsToRefresh = new List<Job>();

				if (transaction.Job != null)
				{
					jobsToRefresh.Add((Job)transaction.Job);
				}

				jobsToRefresh.AddRange(from InvoiceDependentJob invoiceDependentJob
									   in transaction.InvoiceDependentJobs
									   where transaction.Job == null || invoiceDependentJob.Job.PK != transaction.Job.PK
									   select invoiceDependentJob.Job);

				foreach (var job in jobsToRefresh)
				{
					job.Charges.Load();
					job.UpdateTotals();
				}
			}

			InvoiceFilterObject.RefreshInvoiceList();
		}

		TransactionHeader SelectedTransactionInInvoicesGrid
		{
			get
			{
				bool hasCurrent = (APInvoicesGrid != null && APInvoicesGrid.ListManager != null && APInvoicesGrid.ListManager.Position >= 0);
				return hasCurrent ? APInvoicesGrid.ListManager.GetCurrent() as TransactionHeader : null;
			}
		}

		IJobInvoicePrintingControlPresentationProvider JobInvoicePrintingControlPresentationProvider => jobInvoicePrintingControlPresentationProvider ?? (jobInvoicePrintingControlPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetJobInvoicePrintingControlPresentationProvider());
		IJobInvoicePrintingControlPresentationProvider jobInvoicePrintingControlPresentationProvider;

		void UpdateComplianceNumber(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.APUpdateComplianceNumber))
			{
				SecurityHelper.ShowError(SecurityCore.APUpdateComplianceNumber);
			}
			else
			{
				List<TransactionHeader> transactions = new List<TransactionHeader>();
				BusinessObject[] bizOs = APInvoicesGrid.SelectedElements;
				foreach (BusinessObject bizO in bizOs)
				{
					TransactionHeader transaction = bizO as TransactionHeader;
					if (transaction != null && transaction.AH_TransactionReference.IsEmpty && !transaction.AH_ComplianceSubType.IsEmpty)
					{
						transactions.Add(transaction);
					}
				}

				if (transactions.Count > 0)
				{
					new GovtTaxInvoicePrinter(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly).PrintGovtTaxInvoices(transactions.ToArray());
				}
				else if (bizOs.Length == 0)
				{
					Globals.Message.ShowError(Res.GetString("86d31d6c-d32c-491d-9960-ac1d408e7ec6", "Please select a record in the grid."));
				}
				else if (transactions.Count < bizOs.Length)
				{
					Globals.Message.ShowWarning(Res.GetString("e986f772-e0c1-4cbe-ab84-1a76fb4ebb55", "This function will only update transactions that have a Compliance Sub Type and do not already have a Compliance Number populated."));
				}
			}
		}

		TransactionHeader SelectedTransactionInAPInvoicesGrid
		{
			get
			{
				bool hasCurrent = (APInvoicesGrid != null && APInvoicesGrid.ListManager != null && APInvoicesGrid.ListManager.Position >= 0);
				return hasCurrent ? APInvoicesGrid.ListManager.GetCurrent() as TransactionHeader : null;
			}
		}

		#endregion

		#region Security

		public virtual SecurityCheckpoint PluginSecurity
		{
			get { return fPluginSecurity; }
			set { fPluginSecurity = value; }
		}
		SecurityCheckpoint fPluginSecurity;

		JobInvoicingSecurityHelper fSecurityHelper;
		JobInvoicingSecurityHelper SecurityHelper
		{
			get { return fSecurityHelper ?? (fSecurityHelper = new JobInvoicingSecurityHelper(PluginSecurity)); }
		}

		#endregion

		void ShowViewForm()
		{
			AccountingControllerCreator.GetNewController(SelectedTransactionInAPInvoicesGrid).ShowViewForm(SelectedTransactionInAPInvoicesGrid);
		}

		void ShowEditForm()
		{
			var editForm = AccountingControllerCreator.GetNewController(SelectedTransactionInAPInvoicesGrid).ShowEditForm(SelectedTransactionInAPInvoicesGrid);

#if DEBUG
			EditForm_ForTestOnly = (ZForm)editForm;
#endif
		}

		#region Event Handlers

		void HandleEditRequisition(object sender, EventArgs e)
		{
			if (SelectedTransactionInAPInvoicesGrid != null)
			{
				new RequisitionEditHelper().HandleEditRequisition(APInvoicesGrid, SelectedTransactionInAPInvoicesGrid.Factory, SecurityHelper.GetInvSecurity(SecurityCore.EditRequisition));
			}
			else
			{
				InvoicePrintingUserControlHelper.ShowNoSelectedMessage();
			}
		}

		void UpdateComplianceDetailsOnInvoices(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.APUpdateComplianceSubTypeAndNumber))
			{
				SecurityHelper.ShowError(SecurityCore.APUpdateComplianceSubTypeAndNumber);
			}
			else
			{
				BusinessObject[] invoicesToUpdate = APInvoicesGrid.SelectedElements;
				UpdateGovtTaxInvoices(invoicesToUpdate);
			}
		}
		void HandleOverrideTransactionDescription(object sender, EventArgs e)
		{
			if (Env.Security.APOverrideTransactionDescription.IsAllowed)
			{
				InvoicePrintingUserControlHelper.OverrideTransactionDescription(APInvoicesGrid);
			}
			else
			{
				Env.Security.APOverrideTransactionDescription.ShowError();
			}
		}

		void UpdateGovtTaxInvoices(BusinessObject[] invoicesToUpdate)
		{
			if (invoicesToUpdate.Length == 0)
			{
				string caption = Res.GetString("f95dd146-9625-4ca0-b5a8-aa8f6f62006f", "Update Govt Tax Invoice");
				string message = Res.GetString("3b23c91e-5d35-4831-a15d-c61740bec12e", "Please select an invoice or invoices to update.\r\n\r\n - You can select multiple invoices by clicking while holding down Ctrl or Shift Key.");
				Globals.Message.ShowInformation(message, caption);
			}
			else
			{
				List<ZGuid> transactionPKs = new List<ZGuid>();
				foreach (BusinessObject bizO in invoicesToUpdate)
				{
					transactionPKs.Add(bizO.PK);
				}
				ZPKCollection results = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.InvoicePrinting);
				results.Rebuild(transactionPKs);
				if (invoicesToUpdate[0] is AccTransactionHeader)
				{
					AccTransactionHeader initialInvoice = invoicesToUpdate[0] as AccTransactionHeader;
					ZController classAInvController = ZControllerFactory.Create(ControllerIDs.InvoicePrinting);
					classAInvController.ShowEditForm(initialInvoice);
				}
			}
		}

		void HandlePrint(object sender, EventArgs e)
		{
			if (SelectedTransactionInAPInvoicesGrid != null)
			{
				if (SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PrintCostConfirmationDocument))
				{
					InvoicePrintHelper.PrintCostConfirmationDocument(SelectedTransactionInAPInvoicesGrid);
				}
				else
				{
					SecurityHelper.ShowError(SecurityCore.PrintCostConfirmationDocument);
				}
			}
			else
			{
				InvoicePrintingUserControlHelper.ShowNoSelectedMessage();
			}
		}

		void HandlePrintMatchingReport(object sender, EventArgs e)
		{
			if (SelectedTransactionInAPInvoicesGrid != null)
			{
				if (SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PrintMatchDocument))
				{
					InvoicePrintHelper.PrintMatchingReport(SelectedTransactionInAPInvoicesGrid);
				}
				else
				{
					SecurityHelper.ShowError(SecurityCore.PrintMatchDocument);
				}
			}
			else
			{
				InvoicePrintingUserControlHelper.ShowNoSelectedMessage();
			}
		}

		void HandlePrintSelfBillingInvoice(object sender, EventArgs e)
		{
			if (SelectedTransactionInAPInvoicesGrid != null)
			{
				if (SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PrintSelfBillingInvoiceDocument))
				{
					InvoicePrintHelper.PrintSelfBillingInvoice(SelectedTransactionInAPInvoicesGrid);
				}
				else
				{
					SecurityHelper.ShowError(SecurityCore.PrintSelfBillingInvoiceDocument);
				}
			}
			else
			{
				InvoicePrintingUserControlHelper.ShowNoSelectedMessage();
			}
		}

		void HandleViewClick(object sender, EventArgs e)
		{
			if (SelectedTransactionInAPInvoicesGrid != null)
			{
				ShowViewForm();
			}
			else
			{
				InvoicePrintingUserControlHelper.ShowNoSelectedMessage();
			}
		}

		void HandleEditClick(object sender, EventArgs e)
		{
			if (SelectedTransactionInAPInvoicesGrid != null)
			{
				ShowEditForm();
			}
			else
			{
				InvoicePrintingUserControlHelper.ShowNoSelectedMessage();
			}
		}

		void FindButton_Click(object sender, EventArgs e)
		{
			if (InvoiceFilterObject != null)
			{
				InvoiceFilterObject.RefreshInvoiceList();
			}
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			if (InvoiceFilterObject != null)
			{
				InvoiceFilterObject.ResetInvoiceList();
			}
		}

		#endregion
	}
}
