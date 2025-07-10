using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI
{
#if DEBUG
	// This ZForm serves as base class only. It's not abstract so that it can be open in designer tool. Hence it has TestExcludeZWinFormsAllHaveFormBashers attribute applied.
	[TestExcludeZWinFormsAllHaveFormBashers()]
	[TestExcludeZWinFormHasTypedConstructor()]
#endif

	public partial class AccountingZForm : ZForm, IButtonDeleteTextOverride, IButtonPostTextOverride, IButtonApplyTextOverride
	{
		public AccountingZForm(BusinessObject businessEntity)
			: base(businessEntity)
		{
			var dataExportBatchSource = businessEntity as IDataExportBatchSource;
			if (dataExportBatchSource != null && dataExportBatchSource.IsDataExportBatchSupported)
			{
				PlugIns.Add(ControllerIDs.DataExportBatchPlugin);
			}

			if (ShowAuditTab)
			{
				PlugIns.Add(ControllerIDs.Audit);
			}
		}

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public AccountingZForm()
		{
		}

		protected virtual bool ShowAuditTab
		{
			get { return false; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			LastSaveSuccessful = true;
		}

		protected override void OnLoad(EventArgs e)
		{
			SetAutoAddPreviousNextButtons();
			base.OnLoad(e);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			MakeRequiredFieldsEditable();
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			IReversing header = BusinessEntity as IReversing;

			if (header == null || DisplayMode != ODisplayMode.Delete || !header.IsReverseTransaction || BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation))
			{
				base.SetReadOnlyIncludingChildren();
			}
		}

		protected bool LastSaveSuccessful { get; set; }

		protected sealed override void Save(ITransactionParticipant[] factories)
		{
			LastSaveSuccessful = true;
			SaveCore(factories);
		}

		protected virtual void SaveCore(ITransactionParticipant[] factories)
		{
			List<IDisposable> suspenders = new List<IDisposable>();
			foreach (ITransactionParticipant participant in factories)
			{
				BusinessObjectFactory factory = participant as BusinessObjectFactory;
				if (factory != null)
				{
					suspenders.Add(CreditChecker.GetCreditLimitValidationSuspender(factory));
				}
			}

			try
			{
				LastSaveSuccessful = false;
				base.Save(factories);
				LastSaveSuccessful = true;
			}
			catch (JobCreationException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			finally
			{
				foreach (IDisposable suspender in suspenders)
				{
					suspender.Dispose();
				}

				suspenders.Clear();
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();
			if (Transaction != null && Transaction.HasErrors)
			{
				result = ContinueWithSave.No;
			}

			if (!LastSaveSuccessful)
			{
				result = ContinueWithSave.No;
			}

			if (Transaction != null && !(Transaction is InvoiceBulkBatch) && !Transaction.IsInDatabase && !IsINTransactionWithApprovalRequest && !IsARCreditNoteForAmendingWithApprovalRequest && result == ContinueWithSave.Yes)
			{
				string message = string.Format((NoResString)"Attempt of ValidateAndSave to return Yes for unsaved transaction"); // Error message reported to Developer
				ExceptionReporter.Instance.ReportDeveloperException(message, message, new Exception(message + System.Environment.NewLine + (new System.Diagnostics.StackTrace().ToString())));
				result = ContinueWithSave.No;
			}

			return result;
		}

		protected override void SaveToRecentItems()
		{
			var transaction = BusinessEntity as InvoicingBase;
			if (!IsINTransactionWithApprovalRequest && !IsINTransaction && (transaction == null || !transaction.IsAllocatingInvoice))
			{
				base.SaveToRecentItems();
#if DEBUG
				SaveToRecentItemsCounterForTest++;
#endif
			}
		}

#if DEBUG
		public int SaveToRecentItemsCounterForTest;
#endif

		protected virtual void SetAutoAddPreviousNextButtons()
		{
			AutoAddPreviousNextButtons = false;
		}

		protected override void DeleteCore()
		{
			if (IsUATransaction || IsINTransaction)
			{
				base.DeleteCore();
			}
		}

		#region ShowPreDeleteDialogs

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			ContinueWithDelete result = ContinueWithDelete.No;

			if (ReverseTransaction != null && !IsINTransaction)
			{
				if (!IsUATransaction)
				{
					ReverseTransaction.RunPreSaveValidation();
				}
				if (ReverseTransaction.HasErrors())
				{
					ShowErrorsDialog();
				}
				else
				{
					result = GetReversingReasonAndCode() && CheckConsolidatedInvoiceReferenceNotInDatabaseWhenReversing() ? ContinueWithDelete.Yes : ContinueWithDelete.No;
				}
			}
			else
			{
				result = base.ShowPreDeleteDialogs();
			}

			return result;
		}

		#endregion

		bool CheckConsolidatedInvoiceReferenceNotInDatabaseWhenReversing()
		{
			var result = true;
			if (BusinessEntity.Factory.HasContext(BusinessContext.MaximumJobInvoiceNumberError))
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("F6FD4057-B4FE-4A23-A719-62A439753B1C", "The maximum number of invoices for a job is {0}. You cannot post any more invoices for this job.", AccountingConfigurationRegistry.Instance.MaximumNumberOfInvoicesAllowedOnJob.Value));
			}
			else
			{
				var accTransactionHeader = BusinessEntity as AccTransactionHeader;
				if (accTransactionHeader != null && !accTransactionHeader.IsInDatabase && !accTransactionHeader.AH_ConsolidatedInvoiceRef.IsEmpty && IsARInvoiceOrCreditNote(accTransactionHeader))
				{
					var zdbOnlyQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
					zdbOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, accTransactionHeader.AH_ConsolidatedInvoiceRef);
					zdbOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
					zdbOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
					zdbOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });
					result = new BusinessObjectFactory().LoadTop1<AccTransactionHeader>(zdbOnlyQuery) == null;
					if (!result)
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("27122BF1-94FE-4303-B7D9-813BAFDFA000", "While you were working, the automatically assigned job invoice number was used by another user.\r\nPlease close the form and try again."));
					}
				}
			}

			return result;
		}

		bool IsARInvoiceOrCreditNote(AccTransactionHeader transaction)
		{
			return transaction.AH_Ledger == LedgerTypes.AccountsReceivable && (transaction.AH_TransactionType == TransactionTypes.Invoice || transaction.AH_TransactionType == TransactionTypes.CreditNote);
		}

		#region GetReversingReason

		bool GetReversingReasonAndCode()
		{
			if (ReverseTransaction == null)
			{
				return false;
			}

			PopulateReversingReasonAndCode();

			if (!SupportingDocumentNumber.IsEmpty)
			{
				ReverseTransaction.SupportingDocumentNumber = SupportingDocumentNumber;
			}

			bool result = !ReversingReason.IsEmpty && !ReversingCode.IsEmpty;

			if (result)
			{
				ReverseTransaction.ReversingReason = ReversingReason;
				ReverseTransaction.ReversingCode = ReversingCode;
			}
			return result;
		}

		#endregion

		#region Button Text Override

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get
			{
				IBadDebtWritingOff badDebt = BusinessEntity as IBadDebtWritingOff;
				return badDebt != null && badDebt.IsWritingOff ? Res.GetString("AccountingZForm|PostingButton|WriteOff", "&Write Off") :
					(IsUATransaction || IsINTransaction ? Res.GetString("AccountingZForm|PostingButton|Delete", "&Delete") : Res.GetString("AccountingZForm|PostingButton|Reverse", "&Reverse"));
			}
		}

		string IButtonPostTextOverride.PostButtonText
		{
			get
			{
				return IsThisSavingAndNotPosting ? Res.GetString("b186f444-e99e-4ee5-9f2f-54f5ec7e78da", "S&ave && Close") : Res.GetString("Posting.Buttons.PostClose", "P&ost && Close");
			}
		}

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get
			{
				return IsThisSavingAndNotPosting ? Res.GetString("cb9d23f8-7b37-4fbf-a168-3522968999c8", "&Save") : Res.GetString("Posting.Buttons.Post", "&Post");
			}
		}

		bool IsThisSavingAndNotPosting
		{
			get
			{
				return IsTransactionWithApprovalRequestEditing ||
						(
							BusinessEntity != null && ((BusinessObject)BusinessEntity).IsInDatabase
							&& !IsINTransaction && !IsINTransactionWithApprovalRequest && !IsTransactionForApproval
						);
			}
		}

		#endregion

		public override string FormVerb
		{
			get
			{
				string result;
				if (DisplayMode == ODisplayMode.Delete)
				{
					IBadDebtWritingOff badDebt = BusinessEntity as IBadDebtWritingOff;
					if (badDebt != null && badDebt.IsWritingOff)
					{
						result = Res.GetString("AccountingZForm|FormVerb|WriteOff", "Write Off");
					}
					else if (ReverseTransaction != null)
					{
						if (IsINTransaction)
						{
							result = CancelInsteadOfDelete ? Res.GetString("AccountingZForm|FormVerb|Cancel", "Cancel") : FormVerbs.Delete;
						}
						else
						{
							result = IsUATransaction ? Res.GetString("AccountingZForm|FormVerb|Cancel", "Cancel") : Res.GetString("AccountingZForm|FormVerb|Reverse", "Reverse");
						}
					}
					else
					{
						result = base.FormVerb;
					}
				}
				else if (BusinessEntityForHasChanges is BusinessObject && ((BusinessObject)BusinessEntityForHasChanges).IsInDatabase)
				{
					result = Res.GetString("FormVerb|View", "View");
				}
				else
				{
					result = base.FormVerb;
				}
				return result;
			}
		}

		protected virtual bool IsReversingMode
		{
			get { return DisplayMode == ODisplayMode.Delete && ReverseTransaction != null; }
		}

		protected IReversing ReverseTransaction
		{
			get { return BusinessEntity as IReversing; }
		}

		protected virtual string ReversingReasonInputBoxText
		{
			get
			{
				IBadDebtWritingOff badDebt = BusinessEntity as IBadDebtWritingOff;
				return Res.GetString("AccountingZForm|PleaseEnterTheReasonForThisTransaction", "Please enter the reason for {0} this transaction",
					badDebt != null && badDebt.IsWritingOff ? Res.GetString("AccountingZForm|ReasonForTransaction|WritingOff", "Writing off") : Res.GetString("AccountingZForm|ReasonForTransaction|Reversing", "Reversing"));
			}
		}

		protected virtual string ReversingReasonCaptionText
		{
			get
			{
				IBadDebtWritingOff badDebt = BusinessEntity as IBadDebtWritingOff;
				return badDebt != null && badDebt.IsWritingOff ? Res.GetString("AccountingZForm|WritingOffReason", "Writing Off Reason") : Res.GetString("AccountingZForm|ReversingReason", "Reversing Reason");
			}
		}

		protected void PopulateReversingReasonAndCode()
		{
			TransactionReasonHolder reversingHolder = new TransactionReasonHolder((IReversing)BusinessEntity);

			if (!Globals.IsTest && fReversingReason == ZString.Empty)
			{
				ZFormModaliser.ShowDialogAndDispose(new TransactionReasonForm(reversingHolder, ReversingReasonInputBoxText, ReversingReasonCaptionText));
			}

			if (!string.IsNullOrEmpty(reversingHolder.Reason))
			{
				var loginName = GlbStaff.CurrentUser == null ? ZString.Empty : GlbStaff.CurrentUser.GS_LoginName;
				fReversingReason = Res.GetString("f186dbf5-9d56-480a-bac9-68d9095e7427", "- {0} - {1} Entered By {2}", reversingHolder.Code, reversingHolder.Reason, loginName);
			}

			if (!string.IsNullOrEmpty(reversingHolder.Code))
			{
				fReversingCode = reversingHolder.Code;
			}

			if (!string.IsNullOrEmpty(reversingHolder.SupportingDocumentNumber))
			{
				fSupportingDocumentNumber = reversingHolder.SupportingDocumentNumber;
			}
		}

		protected ZString ReversingReason
		{
			get { return fReversingReason; }
		}
		protected ZString fReversingReason;

		protected ZString ReversingCode
		{
			get { return fReversingCode; }
		}
		protected ZString fReversingCode;

		protected ZString SupportingDocumentNumber
		{
			get { return fSupportingDocumentNumber; }
		}
		protected ZString fSupportingDocumentNumber;

		public bool CancelInsteadOfDelete { get; set; }

		protected override DialogResult ShowConfirmationForDelete()
		{
			if (CancelInsteadOfDelete && BusinessEntity is ICancellable)
			{
				string message = Res.GetString("c6965d53-f4a7-4df9-bb5f-e1453690eddd", "You are about to cancel this transaction. Do you want to proceed?");
				string caption = Res.GetString("22d54628-4b42-4d18-8cad-486834e3706d", "Cancel Confirmation");
				return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
			}
			else
			{
				return base.ShowConfirmationForDelete();
			}
		}

		protected override void Delete()
		{
			if (CancelInsteadOfDelete && BusinessEntity is ICancellable)
			{
				((ICancellable)BusinessEntity).IsCancelled = true;
				SaveInternal();
			}
			else
			{
				base.Delete();
			}
		}

		protected override void HandleSaveException(Exception ex)
		{
			if (BusinessEntity != null && BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation))
			{
				LastSaveSuccessful = false;
				DisplayMode = ODisplayMode.ReadOnly;
				SetReadOnlyIncludingChildren();
			}

			base.HandleSaveException(ex);
		}

		#region MakeRequiredFieldsEditable

		protected void MakeRequiredFieldsEditable()
		{
			if (IsReversingMode)
			{
				MakeRequiredFieldsEditableForReversing();
			}
		}

		protected virtual void MakeRequiredFieldsEditableForReversing()
		{
			MakeRequiredFieldsEditableForReversing(BusinessEntity as BusinessObject);
		}

		protected void MakeRequiredFieldsEditableForReversing(BusinessObject header)
		{
			var editableFields = GetEditableFields(header);

			if (editableFields.Count > 0)
			{
				header.ReadOnly = false;
				var method = header.GetType().GetMethod("AddWritableProperties");
				if (method != null)
				{
					method.Invoke(header, new object[] { editableFields.ToArray() });
				}
			}

			if (header is InvoicingBase || header is Payment)
			{
				((TransactionHeader)header).AH_OA_InvoiceAddressOverrideInfo.RefreshBinding();
			}
		}

		protected virtual List<string> GetEditableFields(BusinessObject header)
		{
			var editableFields = new List<string>();
			if (header is InvoicingBase invoicingBase)
			{
				if (invoicingBase.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
				{
					editableFields.Add(TransactionHeader.Schema.AH_TransactionNum);
				}

				if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType
					&& (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.China
						&& (invoicingBase.AH_Ledger == LedgerTypes.AccountsPayable || invoicingBase.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || invoicingBase.AH_Ledger == LedgerTypes.IncompleteTransactions))
					|| (invoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable
						&& ((ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(invoicingBase.Company.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceSubTypeEditableProvider>)?.Get().CheckComplianceSubTypeIsEditable(ReverseTransaction != null) ?? false)))
				{
					editableFields.Add(TransactionHeader.Schema.AH_ComplianceSubType);
				}

				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.VietNam && header is ARCreditNote)
				{
					editableFields.Add(ReverseTransaction.SupportingDocumentNumberInfo.Name);
				}

				editableFields.Add(InvoicingBase.Schema.AH_Calc_AmendStatusCode);
				editableFields.Add(InvoicingBase.Schema.ReversalStatusCode);
			}

			if (header is ITransaction iTransactionHeader && AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value)
			{
				if (iTransactionHeader.UserAllowedToBackPost)
				{
					editableFields.Add(TransactionHeader.Schema.AH_PostDate);
				}
			}

			if (header != null && header is TransactionHeader transactionHeader)
			{
				if (transactionHeader.UserAllowedToModifyInvoiceDateWhenReversing)
				{
					editableFields.Add(TransactionHeader.Schema.AH_InvoiceDate);
				}
			}

			editableFields.Add(ReverseTransaction.UnmatchDateInfo.Name);

			return editableFields;
		}

		protected bool IsUATransaction
		{
			get { return GetIsUATransaction(BusinessEntity); }
		}

		protected bool GetIsUATransaction(IBusiness businessEntity)
		{
			InvoicingBase transaction = businessEntity as InvoicingBase;
			return transaction != null && !transaction.IsDeleted && transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions;
		}

		protected bool IsINTransaction
		{
			get
			{
				InvoicingBase transaction = BusinessEntity as InvoicingBase;
				return transaction != null && !transaction.IsDeleted && transaction.AH_Ledger == LedgerTypes.IncompleteTransactions && !transaction.HasApprovalRequest;
			}
		}

		protected bool IsARCreditNoteForAmendingWithApprovalRequest
		{
			get
			{
				InvoicingBase transaction = BusinessEntity as InvoicingBase;
				return transaction != null && transaction.IsAmendingWithARCreditNote && transaction.HasApprovalRequest;
			}
		}

		protected bool IsINTransactionWithApprovalRequest
		{
			get
			{
				var transaction = BusinessEntity as InvoicingBase;
				return transaction != null && !transaction.IsDeleted && transaction.AH_Ledger == LedgerTypes.IncompleteTransactions && IsTransactionWithApprovalRequest;
			}
		}

		protected bool IsTransactionWithApprovalRequest
		{
			get
			{
				var transaction = BusinessEntity as InvoicingBase;
				return transaction != null && (IsTransactionWithApprovalRequestPosting || IsTransactionWithApprovalRequestEditing || transaction.HasApprovalRequest);
			}
		}

		protected bool IsTransactionWithApprovalRequestPosting
		{
			get
			{
				var transaction = BusinessEntity as InvoicingBase;
				return transaction != null && transaction.HasContext(APInvoiceChargesApprovalRequest.Context.Posting);
			}
		}

		protected bool IsTransactionWithApprovalRequestEditing
		{
			get
			{
				var transaction = BusinessEntity as InvoicingBase;
				return transaction != null && transaction.HasContext(APInvoiceChargesApprovalRequest.Context.Editing);
			}
		}

		bool IsTransactionForApproval
		{
			get
			{
				InvoicingBase transaction = BusinessEntity as InvoicingBase;
				return transaction != null && transaction.IsConvertedUAInvoiceOrCRD;
			}
		}

		AccTransactionHeader Transaction
		{
			get { return BusinessEntity as AccTransactionHeader; }
		}

		#endregion
	}
}

