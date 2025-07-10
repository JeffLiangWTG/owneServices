using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public delegate void DisplayCollectionNotesFormEventHandler();
	public partial class ARReceipt : Receipt, IDocManagerSupport, IEDocsParsingSupport
	{
		public ARReceipt(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("653d3d7e-16d1-49c4-9ade-7249fcd3b006", "Accounts Receivable Receipt");

		protected override ZString Ledger => ZArchitecture.Core.LedgerTypes.AccountsReceivable;

		protected override void SetDefaultBankAccount()
		{
			if (!fSuspendSettingDefaultBankForOrganisation)
			{
				SetDefaultBankAccountAR();
			}
		}

		public override ZString ExchangeRateType => Constants.ExchangeRateTypes.Code.SellRate;

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				ZGuid originalValue = AH_OHCore;

				base.AH_OHCore = value;

				if (value.IsValid && value != originalValue
						&& On_DisplayCollectionNotesForm != null
						&& Env.Security.ReceivablesCollectionCallsEdit.IsAllowed)
				{
					ZQuery collectionNotesFilter = new ZQuery(OrgCollectionNoteSchema.PN_OB, Header.CompanyData.PK);
					collectionNotesFilter.AddToFilter(OrgCollectionNoteSchema.PN_Status, SQLComparisonOperator.NotEqual, CollectionNoteStatusList.Codes.Closed);
					OrgCollectionNote firstRelatedNote = Factory.LoadTop1<OrgCollectionNote>(collectionNotesFilter);

					if (firstRelatedNote != null)
					{
						On_DisplayCollectionNotesForm();
					}
				}
			}
		}

		public override ZString AH_ReceiptType
		{
			get
			{
				return base.AH_ReceiptType;
			}
			set
			{
				base.AH_ReceiptType = value;
				IncludeInDepositBatch = (CanNotBeIncludedInDepositBatch) ? ZBool.False : ZBool.True;
				if (ParentCollection != null)
				{
					ParentCollection.RaiseOnReceiptTypeOnChildChanged();
				}
			}
		}

		public override ZDecimal AH_OSExTaxAmount
		{
			get
			{
				return base.AH_OSExTaxAmount;
			}
			set
			{
				base.AH_OSExTaxAmount = value;
				if (ParentCollection != null)
				{
					ParentCollection.RaiseOnAmountOnChildChanged();
				}
			}
		}

		public override ZDecimal AH_InvoiceAmount
		{
			get
			{
				return base.AH_InvoiceAmount;
			}
			set
			{
				var oldValue = AH_InvoiceAmount;

				base.AH_InvoiceAmount = value;
				if (ParentCollection != null)
				{
					ParentCollection.RaiseOnAmountOnChildChanged();
				}

				if (IsInDatabase && HasChanges && oldValue != value)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderInvoiceAmountWasModifiedAfterBeingSaved,
						() => $@"Invoice Amount was changed from {oldValue} to {value}.
Call stack:
{System.Environment.StackTrace}"
					);
				}
			}
		}

		#endregion

		#region Implementation

		ARReceiptCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections != null && ((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return ((IBusinessObjectInternals)this).ParentCollections[0] as ARReceiptCollection;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Public Members

		public void SuspendSettingDefaultBankForOrganisation(ZBool value) => fSuspendSettingDefaultBankForOrganisation = value;

		ZBool fSuspendSettingDefaultBankForOrganisation;

		public ZBool IncludeInDepositBatch
		{
			get
			{
				return fIncludeInDepositBatch;
			}
			set
			{
				fIncludeInDepositBatch = value;
				IncludeInDepositBatchInfo.RefreshBinding();
			}
		}
		ZBool fIncludeInDepositBatch;

		public ZPropertyInfo IncludeInDepositBatchInfo => GetZPropertyInfo(nameof(IncludeInDepositBatch));

		protected bool IncludeInDepositBatch_ReadOnly => CanNotBeIncludedInDepositBatch;

		ZBool CanNotBeIncludedInDepositBatch =>
			!(AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash ||
			AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque ||
			AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.CreditCard) ||
			!DefaultIncludeInDepositBatch;

		public void SetDefaultIncludeInDepositBatch(ZBool value) => DefaultIncludeInDepositBatch = value;

		ZBool DefaultIncludeInDepositBatch;

		#endregion

		#region DisplayCollectionNotesForm

		public event DisplayCollectionNotesFormEventHandler On_DisplayCollectionNotesForm;

		#endregion

		#region Override Readonly Properties

		public bool IsPopulatedFromARReceipt { get; set; }

		protected override bool AH_InvoiceDate_ReadOnly => base.AH_InvoiceDate_ReadOnly || IsPopulatedFromARReceipt;

		protected override bool AH_PostDate_ReadOnly => base.AH_PostDate_ReadOnly || IsPopulatedFromARReceipt;

		protected bool AH_ReceiptType_ReadOnly => IsPopulatedFromARReceipt && IsCashReceiptType && (BankAccount?.IsCashAccount ?? false);

		protected bool AH_AB_ReadOnly => IsPopulatedFromARReceipt;

		protected override bool AH_ExchangeRate_ReadOnly => base.AH_ExchangeRate_ReadOnly || IsPopulatedFromARReceipt;

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get { return base.AH_RX_NKTransactionCurrency_ReadOnly || IsPopulatedFromARReceipt; }
			set { base.AH_RX_NKTransactionCurrency_ReadOnly = value; }
		}

		#endregion

		#region IDocManagerSupport Members

		public new DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.ReceivableReceipt));

		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region RelevantCAPJournals

		public List<Journal.Journal> RelatedCARJOurnals { get; private set; }

		public void LoadCARJournals()
		{
			var cashAdvanceRequestHeaderInfoLoader = new CashAdvanceRequestInfoByPaymentOrReceipt(this);
			RelatedCARJOurnals = cashAdvanceRequestHeaderInfoLoader.CashAdvanceJournals;
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();

			if (checker.IsReceivablesCashAdvanceFunctionalityEnabled &&
				!checker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed &&
				(RelatedCARJOurnals?.Any() ?? false) &&
				ReverseTransaction is TransactionHeader &&
				IsReversed)
			{
				var cahJournalReverser = new PaidCashAdvanceRequestReverser(this);
				foreach (var cahUpdaterJournal in RelatedCARJOurnals.OfType<IJournalAssociatedToCashAdvanceRequest>())
				{
					cahUpdaterJournal.Accept(cahJournalReverser);
				}
			}

			base.OnFactorySavingBeforeTransactionCore();
		}
	}
}
