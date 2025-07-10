using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobInvoicePrintingControl : ZUserControl
	{
		public JobInvoicePrintingControl()
		{
			InitializeComponent();

			InvoicesGrid.ContextMenu.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("9184AAF8-8A4A-41A5-86C7-1756C6D21DF2", "&View"), HandleViewClick));
			InvoicesGrid.ContextMenu.MenuItems.Add(1, new ZMenuItem(ResString.GetMultilingualString("925ADA2E-9975-4977-9C13-F879B59E11E1", "&Edit"), HandleEditClick));
			InvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("B385B80B-4FF4-4209-8820-F4EE30A77DAD", "&Print"), PrintInvoices));

			InvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("MenuItem.AmendWithCreditNote", "Amend with Credit Note"), AmendWithCreditNote));
			InvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("MenuItem.AmendWithInvoice", "Amend with Invoice"), AmendWithInvoice));

			if (!DesignModeFinder.IsDesigning)
			{
				if ((GlbCompany.CurrentCompany.Country.SupportComplianceSubType && GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.China))
				{
					InvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.PrintTaxInvoice", "Print Compliance Document"), new EventHandler(PrintClassAInvoices)));
				}

				if (GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
				{
					InvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.UpdateTaxInvoiceNumber", "Update Compliance Sub Type and/or Number"), new EventHandler(UpdateClassAInvoices)));
					if (!AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
					{
						if (GlbCompany.CurrentCompany.Country.HasAccComplianceSequence)
						{
							InvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.JobInvoicing.UpdateComplianceNumber", "Allocate Compliance Number"), new EventHandler(UpdateComplianceNumber)));
						}
					}
				}
				else
				{
					RemoveGridColumn(AccTransactionHeader.Schema.AH_TransactionReference);
				}

				if (!GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
				{
					RemoveGridColumn(AccTransactionHeader.Schema.AH_ComplianceSubType);
				}

				if (!(CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.Country.Code) as IComplianceDocumentInfo)?.ShouldDisplayComplianceDocumentDate() ?? true)
				{
					RemoveGridColumn(AccTransactionHeader.Schema.AH_ComplianceDocumentDate);
				}

				if (!JobInvoicePrintingControlPresentationProvider.IsEInvoicingColumnsAvailable(ledgerType))
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

				var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
				var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();

				if (!(amendStatusCodeProvider?.ShouldShowAmendStatusCode() ?? false))
				{
					RemoveGridColumn(nameof(TransactionHeader.AmendStatusCodeAndDescription));
				}

				if (!JobInvoicePrintingControlPresentationProvider.IsTaxBranchColumnAvailable())
				{
					RemoveGridColumn(nameof(TransactionHeader.Schema.AH_GB_TaxBranch));
				}

				var guiHelper = new EInvoicingGUIActionHelper(() => InvoicesGrid.SelectedElements.OfType<TransactionHeader>());
				InvoicesGrid.ContextMenu.MenuItems.AddRange(guiHelper.GetActionMenuItems());
			}
			InvoicesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.ARTransaction.OverrideTransactionDescription", "Override Transaction Description"), HandleOverrideTransactionDescription));
		}

		protected virtual ZString ledgerType
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ResizeControls();

			if (!DesignModeFinder.IsDesigning)
			{
				FetchButton.Visible = AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Value;
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (this.InvoiceFilterObject != null && this.InvoiceFilterObject.FilteredTransactions.Count < this.InvoiceFilterObject.Transactions.Count)
			{
				this.InvoiceHidingMessageLabel.Visible = true;
			}
		}

		void RemoveGridColumn(ZString columnName)
		{
			foreach (Core.Forms.ZGridColumnInfo columnInfo in InvoicesGrid.ColumnStyles)
			{
				if (columnInfo.ColumnName == columnName)
				{
					InvoicesGrid.ColumnStyles.Remove(columnInfo);
					break;
				}
			}
		}

		ZButton FetchButton;

		JobARInvoicePrintingFilter InvoiceFilterObject;

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			ResizeControls();
		}

		void ResizeControls()
		{
			ControlDpiScalingHelper.SetHeight(ref InvoicesGrid, PrintButton.Top - InvoicesGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(8), false);
			ControlDpiScalingHelper.SetWidth(ref InvoicesGrid, ClientSize.Width - InvoicesGrid.Left * 2, false);
		}

		#region Event Handlers

		void FetchButton_Click(object sender, EventArgs e)
		{
			BusinessObject[] invoices = InvoicesGrid.SelectedElements;
			if (invoices.Length > 0)
			{
				foreach (BusinessObject businessObject in invoices)
				{
					TransactionHeader transaction = businessObject as TransactionHeader;
					if (transaction != null)
					{
						try
						{
							transaction.PopulateExtraProperties();
							transaction.OutstandingAmountBindableInfo.RefreshBinding();
							transaction.PaymentStatusInfo.RefreshBinding();
							transaction.FullyPaidDateBindableInfo.RefreshBinding();
						}
						catch (TransactionNotFoundException exception)
						{
							Globals.Message.ShowError(exception.Message, Res.GetString("2adf6df5-7992-4a78-a3bc-91105ff82284", "Error"));
						}
						catch (InvalidOperationException ex)
						{
							string message = Res.GetString("c1b6eeb5-8fe0-4757-8d27-664c93ec084a", @"An error occurred when retrieving outstanding transaction details from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {0} System Registry or a problem with an external system.", Core.Constants.ProductName) + "\r\n\r\n";

							if (ex.InnerException != null)
							{
								message += Res.GetString("49b3868c-92a2-4972-9789-98a2a6337f39", "The error message provided is: '{0}'.", ex.InnerException.Message) + "\r\n\r\n";
							}

							if (!DesignModeFinder.IsDesigning)
							{
								message += Res.GetString("44aa4aa1-a2c9-453f-ad98-05818eb85dbb", @"{0} tried to retrieve these details from the external system defined in the {0} System Registry:
Registry Location: Accounting > Transaction Payment Status -> Transaction Payment Status Web Service URL.
External System Web Service URL: {1}", Core.Constants.ProductName, AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUrl.Value);
							}

							Globals.Message.ShowError(message, Res.GetString("2adf6df5-7992-4a78-a3bc-91105ff82284", "Error"));
						}
					}
				}
			}
			else
			{
				string message = Res.GetString("70d4218e-fccd-4ca4-9248-58be604ca4cf", "Please select an invoice or invoices.");
				string caption = Res.GetString("af928414-6a96-4624-9078-7f6fc31eb1b6", "Select an Invoice");
				Globals.Message.ShowInformation(message, caption);
			}
		}

		InvoicePrintTask GetInvoicePrintTask(TransactionHeader[] headers = null)
		{
			IJobHeaderParent jobParent = null;
			if (InvoiceFilterObject != null)
			{
				jobParent = InvoiceFilterObject.HostBusinessObject as IJobHeaderParent;
			}
			return new InvoicePrintTask(new InvoicePrintTask.Configuration(headers ?? InvoicesGrid.GetSelectedElements<TransactionHeader>()) { JobParent = jobParent });
		}

		void PrintInvoices(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PrintInvoice))
			{
				SecurityHelper.ShowError(SecurityCore.PrintInvoice);
			}
			else
			{
				if (InvoicesGrid.SelectedElements.Length > 0)
				{
					var allSelectedInvoiceBase = InvoicesGrid.GetSelectedElements<TransactionHeader>();
					var transactionsEligibleToPrint = InvoicePrintHelper.GetEligibleForPrintingTransactions(allSelectedInvoiceBase);

					if (transactionsEligibleToPrint.Any())
					{
						Print(transactionsEligibleToPrint);
					}
				}
				else
				{
					string message = Res.GetString("183e8d22-5698-4688-84bf-32e9df9ab260", "Please select an invoice or invoices before printing.");
					string caption = Res.GetString("af928414-6a96-4624-9078-7f6fc31eb1b6", "Select an Invoice");
					Globals.Message.ShowInformation(message, caption);
				}
			}

			void Print(IEnumerable<TransactionHeader> headers)
			{
				try
				{
					GetInvoicePrintTask(headers.ToArray()).Run();
				}
				catch (UnableToFindInvoiceDocumentCommandException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void PrintClassAInvoices(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PrintGovtTax))
			{
				SecurityHelper.ShowError(SecurityCore.PrintGovtTax);
			}
			else
			{
				TransactionHeader[] invoicesToPrint = InvoicesGrid.SelectedElements.Where(x => x is TransactionHeader).Cast<TransactionHeader>().ToArray();

				if (invoicesToPrint.Length == 0)
				{
					string caption = Res.GetString("3cabb8b7-8c46-4301-ba61-3abf136024fa", "Print Govt Tax Invoice");
					string message = Res.GetString("d65a5bb5-3dbd-4de7-9649-312a1174a8de", "Please select an invoice or invoices to print.\r\n\r\n - You can select multiple invoices by clicking while holding down Ctrl or Shift Key.");
					Globals.Message.ShowInformation(message, caption);
				}
				else
				{
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
					{
						if (invoicesToPrint.Where(invoice => invoice.Header != null && invoice.Header.UNLOCO != null).Any(invoice => invoice.Header.UNLOCO.RL_RN_NKCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
						{
							string caption = Res.GetString("3cabb8b7-8c46-4301-ba61-3abf136024fa", "Print Govt Tax Invoice");
							string message = Res.GetString("65bae2ea-d51a-4ae8-9342-9390e3727e95", "All the selected invoice(s) must have a debtor in the this country/region ({0}).", GlbCompany.CurrentCompany.Country.Description);
							Globals.Message.ShowInformation(message, caption);
							return;
						}
					}

					if (AccountingUtils.IsVietnamCompanyEInvoicingEnabled)
					{
						invoicesToPrint = invoicesToPrint.Where(x => (x as InvoicingBase).IsValidTransactionToPrintGovtTaxInvoiceInVietnam).ToArray();

						if (invoicesToPrint.Length == 0)
						{
							Globals.Message.ShowError(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint);
							return;
						}

						if (invoicesToPrint.Length < InvoicesGrid.SelectedElements.Length)
						{
							Globals.Message.ShowWarning(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint);
						}
					}

					if (!DesignModeFinder.IsDesigning)
					{
						new GovtTaxInvoicePrinter(AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value).PrintGovtTaxInvoices(invoicesToPrint);
					}
				}
			}
		}

		void AmendWithCreditNote(object sender, EventArgs e)
		{
			AmendTransaction(TransactionTypes.CreditNote);
		}

		void AmendWithInvoice(object sender, EventArgs e)
		{
			AmendTransaction(TransactionTypes.Invoice);
		}

#if DEBUG
		public ZForm AmendingForm_ForTestOnly;
#endif

		void AmendTransaction(ZString transactionType)
		{
			if (SelectedTransactionInInvoicesGrid == null)
			{
				InvoicePrintingUserControlHelper.ShowNoSelectedMessage();
				return;
			}

			BusinessObjectFactory factory = new BusinessObjectFactory();
			InvoicingBase originalTransaction = factory.Load<InvoicingBase>(SelectedTransactionInInvoicesGrid.PK);

			var amending = CreditNoteAmendingHelper.AmendARTransaction(transactionType, originalTransaction, SecurityHelper, (message, caption) => SecurityHelper.ShowError(message),
				(message, caption) => Globals.Message.ShowInformation(message, caption));

			if (amending != null)
			{
				var transactionHeader = (InvoicingBase)amending;
				var controller = AccountingControllerCreator.GetNewController(transactionHeader);
				var amendingHolder = new TransactionReasonHolder(amending, TransactionReasonCategory.AmendmentReason);

				if (PopulateAmendingReasonAndCode(amending, amendingHolder))
				{
					transactionHeader.SetContext(BusinessContext.AmendingInvoice);

					if (amendingHolder.IsAmendInFull)
					{
						new CFXJournalReverser().ReverseJournal(originalTransaction);
						transactionHeader.SetReadOnlyIncludingChildren(true);
						transactionHeader.IsAmendInFull = true;
					}

					IZForm amendingForm = controller.ShowFormForNewEntity(transactionHeader);

					if (amendingForm != null)      //This check occurs due to security checking in ShowFormForNewEntity, which may return null if permissions are not valid.
					{
						amendingForm.Closed += new EventHandler(amendingForm_Closed);
					}
#if DEBUG
					AmendingForm_ForTestOnly = (ZForm)amendingForm;
#endif
				}
			}
		}

		void amendingForm_Closed(object sender, EventArgs e)
		{
			ZForm form = sender as ZForm;
			if (form != null && form.BusinessEntity.IsInDatabaseIncludingChildren)
			{
				ZForm parentForm = ParentForm as ZForm;
				IAmending amending = form.BusinessEntity as IAmending;
				if (amending != null && amending.IsAmendingTransaction &&
					parentForm != null && parentForm.BusinessEntity != null)
				{
					InvoicingBase amendingTransaction = parentForm.BusinessEntity.Factory.Load<InvoicingBase>(amending.PK);
					amendingTransaction.RemoveContext(BusinessContext.AmendingInvoice);
					RefreshAfterAmending(amendingTransaction);
				}

				form.Closed -= new EventHandler(amendingForm_Closed);
			}
		}

		void RefreshAfterAmending(InvoicingBase transaction)
		{
			if (transaction != null)
			{
				List<Job> jobsToRefresh = new List<Job>();

				if (transaction.Job != null)
				{
					jobsToRefresh.Add((Job)transaction.Job);
				}

				jobsToRefresh.AddRange(from InvoiceDependentJob invoiceDependentJob in transaction.InvoiceDependentJobs where transaction.Job == null || invoiceDependentJob.Job.PK != transaction.Job.PK select invoiceDependentJob.Job);

				foreach (Job job in jobsToRefresh)
				{
					job.RefreshCharges();
				}
			}

			InvoiceFilterObject.RefreshInvoiceList();
		}

		protected bool PopulateAmendingReasonAndCode(IAmending amending, TransactionReasonHolder amendingHolder)
		{
			DialogResult result = DialogResult.Cancel;

			if (amending.AmendingReason == ZString.Empty)
			{
				result = ZFormModaliser.ShowDialogAndDispose(new TransactionReasonForm(amendingHolder, Res.GetString("c8908a50-ac16-44cc-9b69-d491b23dfe08", "Please enter the reason for Amending this transaction"), Res.GetString("0dc59417-b4b9-47ca-b9c0-50d2da6eecad", "Amending Reason")));
			}

			if (result == DialogResult.OK)
			{
				if (!string.IsNullOrEmpty(amendingHolder.Reason))
				{
					amending.AmendingReason = Res.GetString("bceb0395-2610-4c97-8feb-35b30a3ace2c", "- {0} - {1} Entered By {2}", amendingHolder.Code, amendingHolder.Reason, GlbStaff.CurrentUser.GS_LoginName);
				}

				if (!string.IsNullOrEmpty(amendingHolder.Code))
				{
					amending.AmendingReasonCode = amendingHolder.Code;
				}

				if (!string.IsNullOrEmpty(amendingHolder.SupportingDocumentNumber))
				{
					amending.SupportingDocumentNumber = amendingHolder.SupportingDocumentNumber;
				}
			}

			return result == DialogResult.OK;
		}

		void UpdateComplianceNumber(object sender, EventArgs e)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ARUpdateComplianceNumber))
			{
				SecurityHelper.ShowError(SecurityCore.ARUpdateComplianceNumber);
			}
			else
			{
				List<TransactionHeader> transactions = new List<TransactionHeader>();
				BusinessObject[] bizOs = InvoicesGrid.SelectedElements;
				foreach (BusinessObject bizO in bizOs)
				{
					TransactionHeader transaction = bizO as TransactionHeader;
					if (transaction != null && transaction.AH_TransactionReference.IsEmpty && !transaction.AH_ComplianceSubType.IsEmpty)
					{
						transactions.Add(transaction);
					}
				}

				if (AccountingUtils.IsVietnamCompanyEInvoicingEnabled)
				{
					transactions = transactions.Where(x => (x as InvoicingBase).IsValidTransactionToAllocateInVietnam).ToList();

					if (transactions.Count == 0)
					{
						Globals.Message.ShowError(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToAllocate);
						return;
					}

					if (transactions.Count < bizOs.Length)
					{
						Globals.Message.ShowWarning(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToAllocate);
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
					Globals.Message.ShowWarning(Res.GetString("36088cdb-d0d1-4613-9230-10dd0902ded0", "This function will only update transactions that have a Compliance Sub Type and do not already have a Compliance Number populated."));
				}
			}
		}

		void UpdateClassAInvoices(object sender, EventArgs e)
		{
			var securityCheckpoint = Env.Security.ReceivablesModifyComplianceSubTypeOrNumber;
			if (!securityCheckpoint.IsAllowed)
			{
				securityCheckpoint.ShowError();
				return;
			}
			else
			{
				BusinessObject[] invoicesToUpdate = InvoicesGrid.SelectedElements;
				UpdateGovtTaxInvoices(invoicesToUpdate);
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
				var errorMessageForARComplianceSubTypeAndNumberUpdate = ElectronicInvoicingUpdateActionPermissions.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(invoicesToUpdate.Cast<AccTransactionHeader>().ToArray());

				if (!errorMessageForARComplianceSubTypeAndNumberUpdate.IsEmpty)
				{
					Globals.Message.ShowError(errorMessageForARComplianceSubTypeAndNumberUpdate);
					return;
				}

				var transactionHeader = invoicesToUpdate[0] as AccTransactionHeader;
				var messageForUpdateActionPermissions = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberManualUpdateToAnyValue(transactionHeader.AH_Ledger, transactionHeader.Company);

				if (!messageForUpdateActionPermissions.IsEmpty)
				{
					Globals.Message.ShowInformation(messageForUpdateActionPermissions);
					return;
				}

				List<ZGuid> classAInvoiceGuids = new List<ZGuid>();
				foreach (BusinessObject bizO in invoicesToUpdate)
				{
					classAInvoiceGuids.Add(bizO.PK);
				}
				ZPKCollection results = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.InvoicePrinting);
				results.Rebuild(classAInvoiceGuids);
				if (invoicesToUpdate[0] is AccTransactionHeader)
				{
					AccTransactionHeader initialInvoice = invoicesToUpdate[0] as AccTransactionHeader;
					ZController classAInvController = ZControllerFactory.Create(ControllerIDs.InvoicePrinting);
					classAInvController.ShowEditForm(initialInvoice);
#if DEBUG
					LastShownUpdateComplianceForm = (ZForm)ZControllerFactory.Create(ControllerIDs.InvoicePrinting).ShowEditForm(initialInvoice);
#else
					ZControllerFactory.Create(ControllerIDs.InvoicePrinting).ShowEditForm(initialInvoice);
#endif
				}
			}
		}

#if DEBUG
		internal ZForm LastShownUpdateComplianceForm;
#endif

		void HandleViewClick(object sender, EventArgs e)
		{
			if (SelectedTransactionInInvoicesGrid != null)
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
			if (SelectedTransactionInInvoicesGrid != null)
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

		void HandleOverrideTransactionDescription(object sender, EventArgs e)
		{
			if (Env.Security.AROverrideTransactionDescription.IsAllowed)
			{
				InvoicePrintingUserControlHelper.OverrideTransactionDescription(InvoicesGrid);
			}
			else
			{
				Env.Security.AROverrideTransactionDescription.ShowError();
			}
		}

		#endregion

		#region Implementation

		TransactionHeader SelectedTransactionInInvoicesGrid
		{
			get
			{
				bool hasCurrent = (InvoicesGrid != null && InvoicesGrid.ListManager != null && InvoicesGrid.ListManager.Position >= 0);
				return hasCurrent ? InvoicesGrid.ListManager.GetCurrent() as TransactionHeader : null;
			}
		}

		void ShowViewForm()
		{
			AccountingControllerCreator.GetNewController(SelectedTransactionInInvoicesGrid).ShowViewForm(SelectedTransactionInInvoicesGrid);
		}

#if DEBUG
		public ZForm EditForm_ForTestOnly;
#endif

		void ShowEditForm()
		{
			var editForm = AccountingControllerCreator.GetNewController(SelectedTransactionInInvoicesGrid).ShowEditForm(SelectedTransactionInInvoicesGrid);

#if DEBUG
			EditForm_ForTestOnly = (ZForm)editForm;
#endif
		}

		#endregion

		#region Binding

		bool IsAlreadyBound;
		public void Bind(JobARInvoicePrintingFilter invoiceFilterObject)
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
			ControlDpiScalingHelper.SetTop(FindButton, TransactionTypeDropEdit.Top, false);
			ControlDpiScalingHelper.SetTop(ClearButton, TransactionTypeDropEdit.Top, false);
			ControlDpiScalingHelper.SetTop(FetchButton, TransactionTypeDropEdit.Top, false);
			ControlDpiScalingHelper.SetHeight(FilterPanel, FilterPanel.Height - JobHeaderFindBox.Height, false);
			InvoiceHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// Do Nothing. Handling manually.
		}

		#endregion

		#region Security

		public virtual SecurityCheckpoint PluginSecurity
		{
			get { return fPluginSecurity; }
			set { fPluginSecurity = value; }
		}
		SecurityCheckpoint fPluginSecurity;

		//SecurityCheckpoint GetInvSecurity(string Name)
		//{
		//    return Env.Security.GetInvoicingSecurityCheckPoint(PluginSecurity, Name);
		//}

		JobInvoicingSecurityHelper SecurityHelper
		{
			get
			{
				if (fSecurityHelper == null)
				{
					fSecurityHelper = new JobInvoicingSecurityHelper(PluginSecurity);
				}
				return fSecurityHelper;
			}
		}
		JobInvoicingSecurityHelper fSecurityHelper;

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
			}
			base.Dispose(disposing);
		}

		#endregion

		IJobInvoicePrintingControlPresentationProvider JobInvoicePrintingControlPresentationProvider => jobInvoicePrintingControlPresentationProvider ?? (jobInvoicePrintingControlPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetJobInvoicePrintingControlPresentationProvider());
		IJobInvoicePrintingControlPresentationProvider jobInvoicePrintingControlPresentationProvider;
	}
}

