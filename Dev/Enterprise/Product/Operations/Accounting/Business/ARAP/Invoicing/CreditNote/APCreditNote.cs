using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[CodeProperty(AccTransactionHeaderSchema.Constants.AH_ConsolidatedInvoiceRef)]
	[RestrictedFilteredItem()]
	public class APCreditNote : CreditNote, IDocManagerSupport, IAmending, IEDocsParsingSupport, ISupportAccProcessLogging
	{
		public APCreditNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("785adb90-801a-485c-99fd-e42a4e9778a4", "Accounts Payable Credit Note"); }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(APCreditNoteLine); }
		}

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new APCreditNoteLineCollection(this);
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForInternalRef
		{
			get { return AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.Value.Value ? AccountingNumberFountainWrapperFactory.Instance.APInvoiceInternalRef : AccountingNumberFountainWrapperFactory.Instance.APCreditNoteInternalRef; }
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(APInvoice); }
		}

		protected override Type TypeOfTransaction
		{
			get { return typeof(APCreditNote); }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			if (Factory.HasContext(BusinessContext.SavingIncompleteTransaction))
			{
				return new IncompleteInvoicingBaseValidation(this);
			}

			return new APCreditNoteValidation(this);
		}

		protected override ZString InvoiceApprovedLogReference
		{
			get { return (NoResString)"AP|CRD|Approved and Posted"; }
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			AddInvoiceApprovalLog();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && !Factory.HasContext(BusinessContext.IncompleteInvoiceSaving))
			{
				ClearApportionmentJobMutexes();
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				this.RemoveContext(BusinessContext.PostUnapprovedCreditNoteForApproveClaim);
			}
		}

		public override void ReleaseAllMutexOnInvoice()
		{
			base.ReleaseAllMutexOnInvoice();
			ClearApportionmentJobMutexes();
		}

		protected override bool ValidateExpectedInvoiceTotalSecurityIsAllowed
		{
			get
			{
				return Env.Security.AllowAPCreditNoteChangeDefaultExpectedTotalValue.IsAllowed;
			}
		}

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting; }
		}

		#region Invoice Amount Levels

		protected override AuthorizationModeAndSettingsRegistryItem AuthorizationModeAndSettingsRegistry => null;

		protected override SecurityCheckpoint RetrieveLevelApprovalCheckPoint(string levelCode) => null;

		#endregion

		#region IDocManagerSupport Members

		protected override InvoicingDocManagerInfo GetNewDocManagerInfo()
		{
			return docManagerInfo ?? (docManagerInfo = new APInvoiceDocManagerInfo(this, Core.Constants.DocManagerCodes.PayableCreditNote));
		}
		InvoicingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region IAmending Members
		// Note: All the following codes should be moved to the parent class CreditNote once all the interface methods
		// are implemented in AP CRD, i.e. AR CRD and AP CRD should share the same piece of code.

		IAmending IAmending.GenerateAmendingTransaction(string transactionType)
		{
			throw new NotImplementedException();
		}

		ZString IAmending.AmendingReason
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}
		ZString IAmending.AmendingReasonCode
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		void IAmending.FlagAsCreatedAmending() => hasBeenCreatedAsAmending = true;

		bool IAmending.IsAmendingTransaction => this.CheckIsAmendingTransaction(hasBeenCreatedAsAmending);

		bool IAmending.IsOriginalTransaction => !AH_TransactionBelongsToGroup.IsValid && !IsAmendingTransaction_SoftReference;

		ITransaction IAmending.OriginalTransaction
		{
			get { return Factory.Load<InvoicingBase>(AH_TransactionBelongsToGroup); }
		}

		ZGuid[] IAmending.OriginalTransactionJobPKs => this.GetOriginalTransactionJobPKs();

		ZGuid IAmending.OriginalTransactionAccountPK => this.GetOriginalTransactionAccountPK();

		#endregion

		#region Events

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				if (RelatedDraftInvoice != null)
				{
					result.Add(RelatedDraftInvoice);
				}

				return result.ToArray();
			}
		}

		#endregion

		protected override void SetHeaderDetail(InvoicingBase transaction)
		{
			base.SetHeaderDetail(transaction);
			this.IsSelfBillingInvoice = transaction.IsSelfBillingInvoice;
			this.UseJobExchangeRate = transaction.UseJobExchangeRate;

			if (IsAmendingCreditNote)
			{
				this.AH_GB = transaction.AH_GB;
				this.AH_GE = transaction.AH_GE;
			}
		}

		protected override bool CanCopyExRateForAmendingCore
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(this.GetExRateLedger(), IsLocalCurrencyTransaction, AH_GC) == AccountingConstants.InvoicePostingExchangeRateOption.Default.Code;
			}
		}

		protected override void CopyTransactionLine(InvoicingLineBase fromLine, InvoicingLineBase toLine, bool populateAmount = true)
		{
			base.CopyTransactionLine(fromLine, toLine, populateAmount);
			toLine.AL_Sequence = fromLine.AL_Sequence;
			toLine.AL_InputGSTVATRecoverable = fromLine.AL_InputGSTVATRecoverable;
		}

		protected override bool OriginalTransactionReference_ReadOnlyCore()
		{
			return (IsAmendingTransaction_StrongReference && hasBeenCreatedAsAmending) || AreOriginalReferenceFieldsReadOnly;
		}

		#region ISupportAccProcessLogging

		ZGuid ISupportAccProcessLogging.ParentId => RelatedDraftInvoice?.PK ?? ZGuid.Empty;

		string ISupportAccProcessLogging.ParentTableCode => RelatedDraftInvoice != null ? AccDraftInvoiceHeaderSchema.Constants.Prefix : string.Empty;

		bool ISupportAccProcessLogging.ShouldLog => RelatedDraftInvoice != null;

		IAccProcessLog[] ISupportAccProcessLogging.Logs => throw new NotImplementedException(); //Will be implemented in a future WI

		IAccProcessLogger ISupportAccProcessLogging.Logger => logger ?? (logger = (RelatedDraftInvoice != null ? new AccDraftInvoiceProcessingErrorLogger(Factory) : throw new NotImplementedException()));
		IAccProcessLogger logger;

		AccDraftInvoiceHeader RelatedDraftInvoice => Factory.LoadTop1<AccDraftInvoiceHeader>(new ZQuery(AccDraftInvoiceHeaderSchema.AIH_AH_PostedTransactionHeader, PK));

		#endregion

#if DEBUG
		// this property should be removed once AP CRD implements the GenerateAmendingTransaction in another WI
		public override bool HasImplementedGenerateAmendingTransaction => false;

#endif
	}
}
