using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoiceForm : BaseInvoicingForm
	{
		public ZCheckBox CashInvoiceOnCheckbox;
		#region Controls

		ZPanel ReceiptPaymentPanel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;
		#endregion

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public InvoiceForm()
		{
		}

		public InvoiceForm(InvoicingBase businessEntity)
			: base(businessEntity)
		{
			((Invoice)Invoice).IsInvoiceReceiptPaymentInfo.ValueChanged += UpdateControlsToMatchCashInvoiceOnCheckbox;
			PlugIns.Add(ControllerIDs.LinkedeNettEDIMessage);
			HookEvents();

			if (businessEntity.GetType() == typeof(APInvoice))
			{
				businessEntity.Factory.SetContext(BusinessContext.APInvoiceForm);
			}
		}

		void HookEvents()
		{
			if (BusinessEntity is ARInvoice)
			{
				ARInvoice invoice = BusinessEntity as ARInvoice;
				invoice.OnComplianceSequenceFailedToAssign += new EventHandler(ComplianceSequenceFailedToAssign);
				invoice.OnDigitalSignatureFailedToSign += new EventHandler<UserMessageEventArgs>(DigitalSignatureFailedToSign);
			}
			else if (BusinessEntity is APInvoice)
			{
				APInvoice invoice = BusinessEntity as APInvoice;
				invoice.DisplayHotCheques += new APInvoice.HotChequeSelectedHandler(Payment_DisplayHotCheques);
				invoice.NotifyUserPaymentUneditable += new APInvoice.PaymentFieldsUneditableHandler(Payment_NotifyUserPaymentUneditable);
			}

			if (BusinessEntity is Invoice)
			{
				Invoice invoice = BusinessEntity as Invoice;
				invoice.OnNegativeCompliancesFailedToCreate += new EventHandler(NegativeCompliancesFailedToCreate);
			}
		}

		void UnhookEvents()
		{
			if (BusinessEntity is ARInvoice)
			{
				ARInvoice invoice = BusinessEntity as ARInvoice;
				invoice.OnComplianceSequenceFailedToAssign -= new EventHandler(ComplianceSequenceFailedToAssign);
				invoice.OnDigitalSignatureFailedToSign -= new EventHandler<UserMessageEventArgs>(DigitalSignatureFailedToSign);
			}
			else if (BusinessEntity is APInvoice)
			{
				APInvoice invoice = BusinessEntity as APInvoice;
				invoice.DisplayHotCheques -= new APInvoice.HotChequeSelectedHandler(Payment_DisplayHotCheques);
				invoice.NotifyUserPaymentUneditable -= new APInvoice.PaymentFieldsUneditableHandler(Payment_NotifyUserPaymentUneditable);
			}

			if (BusinessEntity is Invoice)
			{
				Invoice invoice = BusinessEntity as Invoice;
				invoice.OnNegativeCompliancesFailedToCreate -= new EventHandler(NegativeCompliancesFailedToCreate);
			}
		}

		protected override bool ShouldShowConsolIDColumn
		{
			get { return BusinessEntity is APInvoice; }
		}

		protected InvoicingBase InvoicingBase
		{
			get { return BusinessEntity as InvoicingBase; }
		}

		#region Form Overrides

		protected override string IncompleteFormCaption
		{
			get { return BusinessEntity is APInvoice ? ResString.GetMultilingualString("InvoiceForm|67A5990F-06EC-44f6-99B1-72839B4934E6", "Incomplete AP Invoice") : ResString.GetMultilingualString("InvoiceForm|955142C0-725A-4f84-ACEC-BBD2C2D4D084", "Incomplete AR Invoice"); }
		}

		protected override string UnapprovedFormCaption
		{
			get { return BusinessEntity is APInvoice ? ResString.GetMultilingualString("InvoiceForm|7EEB31FB-4F2D-4E12-88E7-F07F0941E51D", "Unapproved AP Invoice") : ResString.GetMultilingualString("InvoiceForm|9A0D046D-D8E6-4911-82FC-C48740C24722", "Unapproved AR Invoice"); }
		}

		protected override string NormalFormCaption
		{
			get { return BusinessEntity is APInvoice ? ResString.GetMultilingualString("InvoiceForm|9715DA16-7092-4A12-BA0A-CD43B4CEF261", "AP Invoice") : ResString.GetMultilingualString("InvoiceForm|B2A23FB7-F234-4EEF-9C1B-25BF2480D75B", "AR Invoice"); }
		}

		protected override string CaptionForInsertingIntoLabels
		{
			get { return Res.GetString("InvoiceForm|1C2033D1-6154-471F-B7ED-07636EE23A44", "Invoice"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				if (ReceiptPaymentPanel != null)
				{
					SetupReceiptPaymentPanel();
				}
				else if (!isRunWhenTabInitializedCalledInSetDataBinding)
				{
					InvoiceDetailsTabPage.RunWhenTabInitialized((sender, args) =>
					{
						SetupReceiptPaymentPanel();
					});
					isRunWhenTabInitializedCalledInSetDataBinding = true;
				}
			}
		}

		bool isRunWhenTabInitializedCalledInSetDataBinding;

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			if (ARAPInvoice != null)
			{
				if (ARAPInvoice.GetType() == typeof(APInvoice))
				{
					ARAPInvoice.Factory.RemoveContext(BusinessContext.APInvoiceForm);
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!this.IsDesignMode())
			{
				ReceiptPaymentOuterPanel.Visible = !Invoice.IsPosted && DisplayMode != ODisplayMode.Delete && (Invoice.AH_TransactionType != TransactionTypes.UAInvoice) && !IsTransactionWithApprovalRequestEditing;
				UpdateControlsToMatchCashInvoiceOnCheckbox(this, EventArgs.Empty);
			}
		}

		protected override void Delete()
		{
			base.Delete();
			if (LastSaveSuccessful && !ARAPInvoice.IsDeleted && !CancelInsteadOfDelete)
			{
				ApprovalGUIProvider.PrintAPInvoiceAndCreditNote(ARAPInvoice);
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = MessageHelper.ShowMessageIfChequeBookUsesSamePrinterReturnsCancel(((Invoice)BusinessEntity).ChequeBook) ? ContinueWithSave.No : ContinueWithSave.Yes;

			if (result == ContinueWithSave.Yes)
			{
				var apInvoice = Invoice as APInvoice;
				if (apInvoice != null)
				{
					using (apInvoice.ClearLineAuthorisationCacheSuspender.GetSuspender()) // Suspend clearing cache because we have just loaded everything on ValidateAll
					{
						result = base.ShowPreSaveDialogs();
					}
				}
				else
				{
					result = base.ShowPreSaveDialogs();
				}
			}

			return result;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave continueWithSaveResult;

			bool shouldPromptToPrintInvoice = !Invoice.IsInDatabase || Invoice.AH_Ledger == LedgerTypes.IncompleteTransactions;

			continueWithSaveResult = base.ValidateAndSave();

			if (continueWithSaveResult == ContinueWithSave.Yes && shouldPromptToPrintInvoice && !IsINTransactionWithApprovalRequest)
			{
				ApprovalGUIProvider.PrintAPInvoiceAndCreditNote(ARAPInvoice);

				if (IsPaymentInvoice && ARAPInvoice.ReceiptPayment != null)
				{
					SetReceiptPaymentPanelOnContinueWithSave();

					PaymentPrintManager printManager = new PaymentPrintManager(ARAPInvoice.ReceiptPayment.PK.ToGuid(), TransactionTypes.Payment, ARAPInvoice.Factory);
					if (ARAPInvoice is APInvoice && ((IChequeNumberAutoAllocation)ARAPInvoice).ChequeIsAutoPrinted)
					{
						printManager.SetChequeIsAutoPrinted();
#if DEBUG
						Test_ChequeIsAutoPrintedSetOnPrintManager = ZBool.True;
#endif
					}
					printManager.Print();
				}

				Invoice.OnPromptAndPrintComplianceDocumentHandler?.Invoke(ConfirmToPrintDocument, ShowPrintingResult, new[] { Invoice.GetTransactionGeneratedComplianceDocument() }, string.Empty);
			}

			return continueWithSaveResult;
		}

		AccountingMessageHelper MessageHelper
		{
			get
			{
				if (fMessageHelper == null)
				{
					fMessageHelper = new AccountingMessageHelper();
				}
				return fMessageHelper;
			}
		}
		AccountingMessageHelper fMessageHelper;

		protected ITransactionParticipant[] GetAllTransactionParticipants(ITransactionParticipant[] factories1)
		{
			List<ITransactionParticipant> factories = new List<ITransactionParticipant>(factories1);
			if (ARAPInvoice.ReceiptPayment != null && ARAPInvoice.ReceiptPayment.BankAccount != null
				&& ARAPInvoice.ReceiptPayment.BankAccount.IsCreditCardOrLinkedAccount
				&& ARAPInvoice.ReceiptPaymentAH_ReceiptType == ReceiptTypes.eNettCreditCard)
			{
				factories.Add(new eNettPaymentTransactionParticipant(ARAPInvoice.ReceiptPayment));
			}
			return factories.ToArray();
		}

		protected override void SaveCore(ITransactionParticipant[] factories)
		{
			if (ARAPInvoice.IsInvoiceReceiptPayment && ARAPInvoice is IChequeNumberAutoAllocation && ((IChequeNumberAutoAllocation)ARAPInvoice).IsAutoAllocationEnabled)
			{
#if DEBUG
				if (Globals.IsTest && !Globals.GetIsUnitTestingProductionFunctionality())
				{
					Test_Allocator = new PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator((IChequeNumberAutoAllocation)ARAPInvoice, PaymentChequeNumberAllocator.PrintingMode.Invoice, ARAPInvoice.Factory);
					Test_Allocator.SetChequeBookToInactiveOnSaving = Test_DeactivateChequeBookOnAllocation;
					base.SaveCore(Test_Allocator.GetFactoriesForTest());
				}
				else
				{
#endif
					var allocator = new PaymentChequeNumberAllocator((IChequeNumberAutoAllocation)ARAPInvoice, PaymentChequeNumberAllocator.PrintingMode.Invoice, ARAPInvoice.Factory);
					base.SaveCore(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(factories));
#if DEBUG
				}
#endif
			}
			else
			{
				ARAPInvoice.HandleReceiptPayment();
				base.SaveCore(GetAllTransactionParticipants(factories));
			}

			if (ARAPInvoice.WasApprovingWithClaimInitialized)
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				APInvoice apInvoiceInNewFacotry = newFactory.Load<APInvoice>(ARAPInvoice.PK);

				APAccQueryClaim claim = apInvoiceInNewFacotry.CreateClaim(true) as APAccQueryClaim ?? apInvoiceInNewFacotry.CreateClaim(false) as APAccQueryClaim;

				ZController controller = ZControllerFactory.Create(ControllerIDs.APAccQueryClaim);
				controller.SetFormsModalTo(this);
				IZForm form = controller.ShowFormForNewEntity(claim);
				if (form != null)
				{
					form.DisplayMode = ODisplayMode.Edit;
				}
			}
		}

		protected override void HandleSaveException(Exception e)
		{
			if (e is AllocationSaveException allocationSaveEx)
			{
				Globals.Message.ShowError(allocationSaveEx.UserFriendlyMessage, Res.GetString("94bea668-734e-47eb-9f9c-41a39ee0135f", "Check Book Busy"));
			}
			else if (e is AllocationChequeBookException allocationChequeEx)
			{
				Globals.Message.ShowError(allocationChequeEx.UserFriendlyMessage, Res.GetString("6d93b753-5d57-4b91-a92d-b1317ad46adc", "Check Book Full"));
			}
			else if (e is ComplianceSequenceRelatedException complianceSequenceEx)
			{
				Globals.Message.ShowError(complianceSequenceEx.UserFriendlyMessage, Res.GetString("07ae6254-164e-4563-9562-1499af33f757", "Compliance sequence error"));
			}
			else if (e is ENettProcessCreditCardException eNettEx)
			{
				Globals.Message.ShowError(eNettEx.UserFriendlyMessage + "\r\n" + Res.GetString("66ceb64c-12d7-4991-ada3-e68cf7929e04", "ComPay Error: ({0}) {1}", eNettEx.eNettErrorCode, eNettEx.eNettErrorMessage), Res.GetString("a42550f3-ea84-492e-bb8e-5c87218dc70e", "ComPay Error"));
			}
			else
			{
				base.HandleSaveException(e);
			}
		}

		#region Handle Correct Controller ID

		protected override bool ShouldReopenWithCorrectControllerID(ControllerID correctID)
		{
			return ControllerID == ControllerIDs.APIncompleteInvoice && correctID == ControllerIDs.APInvoice;
		}

		#endregion

		#endregion

		#region Implementation

		protected Invoice fARAPInvoice;
		public Invoice ARAPInvoice
		{
			get
			{
				if (fARAPInvoice == null)
				{
					fARAPInvoice = (Invoice)BusinessEntity;
				}
				return fARAPInvoice;
			}
		}

		int originalMinimumHeight;

		void UpdateControlsToMatchCashInvoiceOnCheckbox(object sender, EventArgs e)
		{
			ReceiptPaymentPanel.Visible = CashInvoiceOnCheckbox.Checked;
			CashInvoiceOnCheckbox.Text = CashInvoiceOnCheckbox.Checked ?
Enterprise.Accounting.GUI.Res.GetString("InvoiceForm|491d5f4e-cfaa-4707-8e58-da48de8419d6", "Cash Invoice - Please complete the receipt / payment details if applicable or untick the box if not applicable.") :
Enterprise.Accounting.GUI.Res.GetString("InvoiceForm|07f65acc-15a4-41f1-a938-2de5c44ef863", "Cash Invoice - Please tick the box if you want to enter the receipt / payment details immediately.");

			if (originalMinimumHeight == 0)
			{
				originalMinimumHeight = MinimumSize.Height;
			}
			MinimumSize = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(MinimumSize.Width),
				ControlDpiScalingHelper.UnscaleFromCurrentDpiY(originalMinimumHeight + (CashInvoiceOnCheckbox.Checked ? ReceiptPaymentPanel.Height : 0)));
		}

		protected void SetupReceiptPaymentPanel()
		{
			foreach (Control control in ReceiptPaymentPanel.Controls)
			{
				if (control is InvoiceReceiptUserControl || control is InvoicePaymentUserControl)
				{
					ReceiptPaymentPanel.Controls.Remove(control);
					break;
				}
			}

			if (((InvoicingBase)DataSource).AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				Control invoiceReceiptUserControl = new InvoiceReceiptUserControl();
				ReceiptPaymentPanel.Controls.Add(invoiceReceiptUserControl);
				invoiceReceiptUserControl.Dock = DockStyle.Fill;
			}
			else
			{
				Control invoicePaymentControl = new InvoicePaymentUserControl();
				ReceiptPaymentPanel.Controls.Add(invoicePaymentControl);
				invoicePaymentControl.Dock = DockStyle.Fill;
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		void SetReceiptPaymentPanelOnContinueWithSave()
		{
			if (ReceiptPaymentPanel != null)
			{
				foreach (Control control in ReceiptPaymentPanel.Controls)
				{
					if (control is InvoicePaymentUserControl)
					{
						(control as InvoicePaymentUserControl).AddressWithContactControl.SetReadOnlyIncludingChildren();
						break;
					}
				}
			}
		}

		ZBool IsPaymentInvoice
		{
			get { return ARAPInvoice.IsInvoiceReceiptPayment && (ARAPInvoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable || ARAPInvoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions); }
		}

		void NegativeCompliancesFailedToCreate(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning(AccountingConstants.GetComplianceDocumentNegativeMessage());
		}

		void ComplianceSequenceFailedToAssign(object sender, EventArgs e)
		{
			var msg = InvoicingBase.GetMessageForComplianceSequenceErrors(e);
			Globals.Message.ShowWarning(msg);
			new ComplianceNumberAllocationFailureEmail(((InvoicingBase)sender)).Send();
		}

		void DigitalSignatureFailedToSign(object sender, UserMessageEventArgs e)
		{
			Globals.Message.ShowWarning(e.Message);
			new DigitalSignatureSigningFailureEmail(((InvoicingBase)sender), e.Message).Send();
		}

		#region APInvoice HotCheque

		void Payment_DisplayHotCheques(object sender, HotChequeLink chequeLink)
		{
			var chequeLinkForm = GetHotChequeLinkForm(chequeLink);
			chequeLinkForm.Closed += ChequeLinkForm_Closed;
			ZFormModaliser.ShowDialogAndDispose(chequeLinkForm);
		}

		protected virtual HotChequeLinkForm GetHotChequeLinkForm(HotChequeLink chequeLink)
		{
			return new HotChequeLinkForm(chequeLink);
		}

		void ChequeLinkForm_Closed(object sender, EventArgs e)
		{
			HotChequeLinkForm linkForm = sender as HotChequeLinkForm;
			if (linkForm != null && linkForm.SelectedHotCheque != null)
			{
				((APInvoice)BusinessEntity).ImportSelectedHotCheque(linkForm.SelectedHotCheque);
			}
		}

		void Payment_NotifyUserPaymentUneditable(object sender, string message)
		{
			Globals.Message.ShowInformation(message, Res.GetString("a31e8b07-e0df-46d5-bcb0-aa27b186162d", "Hot Check Imported"));
		}

		#endregion

		#endregion

		#region Dispose
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((Invoice)Invoice).IsInvoiceReceiptPaymentInfo.ValueChanged -= UpdateControlsToMatchCashInvoiceOnCheckbox;
				UnhookEvents();
				Invoice.UnlockAllSourceReferenceMutexes();
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}
		#endregion

		#region TestCase
#if DEBUG

		internal ZBool Test_DeactivateChequeBookOnAllocation;
		PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator Test_Allocator;
		internal ZBool Test_ChequeIsAutoPrintedSetOnPrintManager;

#endif
		#endregion

		void LineChargesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).BeginInit();
			this.LineChargesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineChargesGrid)).EndInit();
			this.LineChargesGrid.ResumeLayout(false);
			this.LineChargesGrid.PerformLayout();
		}
	}
}

