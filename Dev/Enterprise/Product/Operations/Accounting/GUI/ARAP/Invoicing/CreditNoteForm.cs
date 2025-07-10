using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI
{
	public partial class CreditNoteForm : BaseInvoicingForm
	{
		#region Controls

		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;
		#endregion

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CreditNoteForm()
		{
		}

		public CreditNoteForm(InvoicingBase businessEntity)
			: base(businessEntity)
		{
			HookEvents();
			// TODO Check if it is realy needed
			//this.FileNewMenuItem.Click += new System.EventHandler(this.FileNewMenuItem_Click);
			//this.FileSaveMenuItem.Click += new System.EventHandler(this.FileSaveMenuItem_Click);
			//this.FileSaveAndCloseMenuItem.Click += new System.EventHandler(this.FileSaveAndCloseMenuItem_Click);
			//this.FileCloseMenuItem.Click += new System.EventHandler(this.FileCloseMenuItem_Click);
			//this.FileDeleteMenuItem.Click += new System.EventHandler(this.FileDeleteMenuItem_Click);

			if (businessEntity is APCreditNote)
			{
				businessEntity.Factory.SetContext(BusinessContext.APCreditNoteForm);
			}
		}

		void HookEvents()
		{
			if (BusinessEntity is ARCreditNote)
			{
				ARCreditNote creditNote = BusinessEntity as ARCreditNote;
				creditNote.OnComplianceSequenceFailedToAssign += new EventHandler(ComplianceSequenceFailedToAssign);
				creditNote.OnDigitalSignatureFailedToSign += new EventHandler<UserMessageEventArgs>(DigitalSignatureFailedToSign);
			}

			if (BusinessEntity is CreditNote)
			{
				CreditNote creditNote = BusinessEntity as CreditNote;
				creditNote.OnNegativeCompliancesFailedToCreate += new EventHandler(NegativeCompliancesFailedToCreate);
			}
		}

		void UnhookEvents()
		{
			if (BusinessEntity is ARCreditNote)
			{
				ARCreditNote creditNote = BusinessEntity as ARCreditNote;
				creditNote.OnComplianceSequenceFailedToAssign -= new EventHandler(ComplianceSequenceFailedToAssign);
				creditNote.OnDigitalSignatureFailedToSign -= new EventHandler<UserMessageEventArgs>(DigitalSignatureFailedToSign);
			}
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

		#region Form Overrides

		protected override bool ShouldShowConsolIDColumn
		{
			get { return true; }
		}

		protected override string IncompleteFormCaption
		{
			get { return BusinessEntity is APCreditNote ? ResString.GetMultilingualString("CreditNoteForm|085E42C5-5D48-4694-92DC-2EEE3B0244B2", "Incomplete AP Credit Note") : ResString.GetMultilingualString("CreditNoteForm|D4A1F88F-486E-4a14-9E4D-E2087E64B51E", "Incomplete AR Credit Note"); }
		}

		protected override string UnapprovedFormCaption
		{
			get { return BusinessEntity is APCreditNote ? ResString.GetMultilingualString("CreditNoteForm|58F57ACB-5C68-49C9-9A23-9F9318B08659", "Unapproved AP Credit Note") : ResString.GetMultilingualString("CreditNoteForm|61E568D7-C5AD-44CB-A347-8C435CE27299", "Unapproved AR Credit Note"); }
		}

		protected override string NormalFormCaption
		{
			get { return BusinessEntity is APCreditNote ? ResString.GetMultilingualString("CreditNoteForm|CC1812A5-645E-48B9-8F78-BE3EFBE5FDD3", "AP Credit Note") : ResString.GetMultilingualString("CreditNoteForm|4707EA97-3C04-4464-9F4D-7A3B82B9E5EB", "AR Credit Note"); }
		}

		protected override string CaptionForInsertingIntoLabels
		{
			get { return Res.GetString("CreditNoteForm|8B9C5B44-B0F7-418E-BC1C-2C5D5DF74AEC", "Credit Note"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool IsReversingMode
		{
			get
			{
				return base.IsReversingMode || (Invoice != null && Invoice.IsAmendInFull);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				Action action = () =>
					{
						if (Invoice != null && Invoice.IsReverseTransaction)
						{
							InvoiceDetails.ApportionChargesButton.ReadOnly = true;
						}
					};

				if (InvoiceDetails != null)
				{
					action();
				}
				else if (!isRunWhenTabInitializedCalledInSetDataBinding)
				{
					InvoiceDetailsTabPage.RunWhenTabInitialized((sender, args) =>
					{
						action();
					});
					isRunWhenTabInitializedCalledInSetDataBinding = true;
				}
			}
		}

		bool isRunWhenTabInitializedCalledInSetDataBinding;

		public override ODisplayMode DisplayMode
		{
			get { return base.DisplayMode; }
			set
			{
				base.DisplayMode = value;
				if (value == ODisplayMode.Delete || value == ODisplayMode.New)
				{
					if (InvoiceDetails != null)
					{
						InvoiceDetails.TransactionGuidFindBox.Visible = InvoiceDetails.TransactionGuidFindBox.Visible || IsOriginalReferenceVisible();
						InvoiceDetails.TransactionGuidFindBox.GetExtension<LabelCaptionRenderer>().Visible = InvoiceDetails.TransactionGuidFindBox.Visible;
					}
					else if (!isRunWhenTabInitializedCalledInDisplayMode)
					{
						InvoiceDetailsTabPage.RunWhenTabInitialized((sender, args) =>
						{
							InvoiceDetails.TransactionGuidFindBox.Visible = IsOriginalReferenceVisible();
							InvoiceDetails.TransactionGuidFindBox.GetExtension<LabelCaptionRenderer>().Visible = InvoiceDetails.TransactionGuidFindBox.Visible;
						});
						isRunWhenTabInitializedCalledInDisplayMode = true;
					}
				}
			}
		}

		bool isRunWhenTabInitializedCalledInDisplayMode;

		bool IsOriginalReferenceVisible()
		{
			return (Invoice != null && !Invoice.IsReversing && ShouldShowOriginalInvoiceReferenceFields);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			if (BusinessEntity is APCreditNote)
			{
				BusinessEntity.Factory.RemoveContext(BusinessContext.APCreditNoteForm);
			}
		}

		#region Handle Correct Controller ID

		protected override bool ShouldReopenWithCorrectControllerID(ControllerID correctID)
		{
			return ControllerID == ControllerIDs.APIncompleteCreditNote && correctID == ControllerIDs.APCreditNote;
		}

		#endregion

		#endregion

		#region Implementation

		#region Print Credit Note

		protected override void Delete()
		{
			base.Delete();

			if (LastSaveSuccessful && !Invoice.IsDeleted)
			{
				ApprovalGUIProvider.PrintAPInvoiceAndCreditNote(Invoice);
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			bool shouldBePrinted = !Invoice.IsInDatabase || Invoice.AH_Ledger == LedgerTypes.IncompleteTransactions;

			ContinueWithSave saveResult = base.ValidateAndSave();

			if (saveResult == ContinueWithSave.Yes && !IsARCreditNoteForAmendingWithApprovalRequest && shouldBePrinted && !IsINTransactionWithApprovalRequest)
			{
				ApprovalGUIProvider.PrintAPInvoiceAndCreditNote(Invoice);
			}

			Invoice.OnPromptAndPrintComplianceDocumentHandler?.Invoke(ConfirmToPrintDocument, ShowPrintingResult, new[] { Invoice.GetTransactionGeneratedComplianceDocument() }, string.Empty);

			return saveResult;
		}

		#endregion

		protected override void SetInvoiceHeaderDefaults()
		{
			base.SetInvoiceHeaderDefaults();

			if (Invoice.AH_Ledger == LedgerTypes.AccountsReceivable && Invoice.IsAmendingCreditNote && GlbCompany.CurrentCompany.Country.Code == CountryCodes.VietNam && !string.IsNullOrEmpty(Invoice.AH_ComplianceSubType))
			{
				InvoiceDetails.AH_ComplianceSubTypeDropEdit.ReadOnly = true;
			}
		}

		protected override void HandleSaveException(Exception e)
		{
			if (e is ComplianceSequenceRelatedException complianceSequenceEx)
			{
				Globals.Message.ShowError(complianceSequenceEx.UserFriendlyMessage, Res.GetString("0465FE24-3F58-4511-B00B-3F01D90E2480", "Compliance sequence error"));
			}
			else
			{
				base.HandleSaveException(e);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

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

