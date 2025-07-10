using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	[PropertyDescriptorCollection(typeof(TransactionPropertyDescriptorCollection))]
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	[CodeProperty("DepositBatchCode")]
	[DescriptionProperty("HumanReadableName")]
	public class DepositBatchParent : NonPersistentBusinessObjectWithLogsAndNotes, IReversing, IObsoleteValidation
	{
		public DepositBatchParent(BusinessObjectFactory factory, DepositBatch existingDepositBatchToBeLoaded)
			: this(factory)
		{
			this.fExistingDepositBatchToBeLoaded = existingDepositBatchToBeLoaded;
			if (existingDepositBatchToBeLoaded != null)
			{
				batchNumber = existingDepositBatchToBeLoaded.AH_TransactionNum;
				depositDate = existingDepositBatchToBeLoaded.AH_InvoiceDate;
				depositPostDate = existingDepositBatchToBeLoaded.AH_PostDate;
				reversingReason = existingDepositBatchToBeLoaded.AH_Desc;
			}
		}

		public DepositBatchParent(BusinessObjectFactory factory)
			: base(factory)
		{
			filterByBranch = true;
			filterByAllCurrencies = true;
			branchFilterPK = GlbBranch.CurrentBranch.PK;
			if (fExistingDepositBatchToBeLoaded == null)
			{
				depositPostDate = ZDate.Today.ToDateTime();
				depositDate = ZDate.Today.ToDateTime();
			}
		}

		#region ReadOnly

		protected virtual bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = false;
			if (UseEditableFieldsForReadOnly && property.HasSetter())
			{
				result = !WritableProperties.Contains(property.Name);
			}
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		List<string> WritableProperties
		{
			get
			{
				if (writableProperties == null)
				{
					writableProperties = new List<string>();
				}
				return writableProperties;
			}
		}

		public void AddWritableProperties(string[] list)
		{
			foreach (string line in list)
			{
				WritableProperties.Add(line);
			}
			UseEditableFieldsForReadOnly = true;
			RefreshBinding();
		}

		List<string> writableProperties;

		bool UseEditableFieldsForReadOnly;

		#endregion

		#region Base Override

		public override bool IsInDatabase
		{
			get { return IsExistingBatch && fExistingDepositBatchToBeLoaded.IsInDatabase; }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("9F841CBB-C3F3-4701-9353-A3E0D472A278", "Deposit Batch");

		#endregion

		#region Saving

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (DepositBatchLines.Count > 0)
				{
					fExistingDepositBatchToBeLoaded = DepositBatchLines[0];
				}
			}
		}

		#endregion

		#region ITransaction Members

		ZString ITransaction.Ledger
		{
			get { return LedgerTypes.CashBook; }
		}

		ZPropertyInfo ITransaction.LedgerInfo
		{
			get { return GetZPropertyInfo("Ledger"); }
		}

		ZString ITransaction.CurrencyCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsExistingBatch)
				{
					result = fExistingDepositBatchToBeLoaded.CurrencyCode;
				}
				return result;
			}
		}

		ZPropertyInfo ITransaction.CurrencyCodeInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo("CurrencyCode");
				if (IsExistingBatch)
				{
					result = fExistingDepositBatchToBeLoaded.CurrencyCodeInfo;
				}
				return result;
			}
		}

		ZDecimal ITransaction.OverseasTotalAmount
		{
			get
			{
				ZDecimal result = 0m;
				if (IsExistingBatch)
				{
					result = fExistingDepositBatchToBeLoaded.AH_OSTotal;
				}
				return result;
			}
		}

		ZPropertyInfo ITransaction.OverseasTotalAmountInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo("OverseasTotalAmount");
				if (IsExistingBatch)
				{
					result = fExistingDepositBatchToBeLoaded.AH_OSTotalInfo;
				}
				return result;
			}
		}

		ZDateTime ITransaction.PostDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsExistingBatch)
				{
					result = fExistingDepositBatchToBeLoaded.AH_PostDate;
				}
				return result;
			}
			set
			{
				if (IsExistingBatch)
				{
					fExistingDepositBatchToBeLoaded.AH_PostDate = value;
				}
			}
		}

		ZPropertyInfo ITransaction.PostDateInfo
		{
			get { return DepositPostDateInfo; }
		}

		[BusinessObjectTestExclude]
		public ZDateTime TransactionDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsExistingBatch)
				{
					result = fExistingDepositBatchToBeLoaded.AH_InvoiceDate;
				}
				return result;
			}
			set
			{
				if (IsExistingBatch)
				{
					fExistingDepositBatchToBeLoaded.AH_InvoiceDate = value;
				}
			}
		}

		ZPropertyInfo ITransaction.TransactionDateInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(TransactionDate));
				if (IsExistingBatch)
				{
					result = fExistingDepositBatchToBeLoaded.AH_InvoiceDateInfo;
				}
				return result;
			}
		}

		[MaxLength(1)]
		public ZString TransactionNumber
		{
			get
			{
				ZString result = TransactionNumber_toPassBizoTest;
				if (IsExistingBatch)
				{
					result = fExistingDepositBatchToBeLoaded.AH_TransactionNum;
				}
				return result;
			}
			set
			{
				SetNonPersistentPropertyValue(((ITransaction)this).TransactionNumberInfo, ref TransactionNumber_toPassBizoTest, value);
				if (IsExistingBatch)
				{
					fExistingDepositBatchToBeLoaded.AH_TransactionNum = value;
				}
			}
		}
		ZString TransactionNumber_toPassBizoTest;

		public ZPropertyInfo TransactionNumberInfo
		{
			get
			{
				return IsExistingBatch ? fExistingDepositBatchToBeLoaded.AH_TransactionNumInfo : GetZPropertyInfo(nameof(TransactionNumber));
			}
		}

		ZString ITransaction.TransactionType
		{
			get { return TransactionTypes.ReceiptBatch; }
		}

		ZPropertyInfo ITransaction.TransactionTypeInfo
		{
			get { return GetZPropertyInfo("TransactionType"); }
		}

		ZGuid ITransaction.Organization
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZPropertyInfo ITransaction.OrganizationInfo
		{
			get { return GetZPropertyInfo("Organization"); }
		}

		public ZString OriginalTransactionNumber
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionNumber)); }
		}

		public bool OriginalTransactionNumber_ReadOnly { get { return true; } }

		public ZString OriginalTransactionType
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionType)); }
		}

		public bool OriginalTransactionType_ReadOnly { get { return true; } }

		[BusinessObjectTestExclude]
		public ZString SupportingDocumentNumber
		{
			get { return ZString.Empty; }
			set { }
		}

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SupportingDocumentNumber)); }
		}

		OrgHeaderCollection ITransaction.Headers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public bool UserAllowedToBackPost
		{
			get { return Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed; }
		}

		public ZDateTime UnmatchDate
		{
			get { return unmatchDate; }
			set { SetNonPersistentPropertyValue(UnmatchDateInfo, ref unmatchDate, value); }
		}
		ZDateTime unmatchDate;

		public bool UnmatchDate_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return GetZPropertyInfo(nameof(UnmatchDate)); }
		}

		#region ReversalStatusCode

		[BusinessObjectTestExclude]
		public ZString ReversalStatusCode
		{
			get { return ZString.Empty; }
			set { }
		}

		public ZPropertyInfo ReversalStatusCodeInfo => GetZPropertyInfo(nameof(ReversalStatusCode));

		bool ITransaction.ReversalStatusCode_ReadOnly => true;

		ReadOnlyCodeDescriptionPairList ITransaction.ReversalStatusCodeList => null;

		#endregion

		#endregion

		#region IReversing Members

		public bool IsReversing
		{
			get { return fIsReversing; }
		}
		protected bool fIsReversing;

		public bool IsReversed
		{
			get { return IsExistingBatch && fExistingDepositBatchToBeLoaded.AH_IsCancelled; }
		}

		public void GenerateReverseTransaction(bool mustTransform)
		{
			if (IsExistingBatch)
			{
				fExistingDepositBatchToBeLoaded.IsReverseTransaction = true;

				fExistingDepositBatchToBeLoaded.AH_ReceiptBatchNo = ZString.Empty;
				fExistingDepositBatchToBeLoaded.AH_IsCancelled = true;
				fExistingDepositBatchToBeLoaded.AH_OSTotal = 0m;
				fExistingDepositBatchToBeLoaded.AH_InvoiceAmount = 0m;
				fExistingDepositBatchToBeLoaded.AH_GSTAmount = 0m;

				foreach (DepositBatchTransactionLine line in fExistingDepositBatchToBeLoaded.Transactions)
				{
					line.AH_ReceiptBatchNo = ZString.Empty;
				}
			}
		}

		public IReversing ReverseTransaction
		{
			get { return fExistingDepositBatchToBeLoaded; }
		}

		public bool IsReverseTransaction
		{
			get { return IsReversing; }
		}

		public void SetCancellationFlag(bool cancel)
		{
			fExistingDepositBatchToBeLoaded.AH_IsCancelled = cancel;
		}

		public void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue)
		{
		}

		public void SetDescription(ZString descriptionToSet)
		{
			if (IsExistingBatch)
			{
				fExistingDepositBatchToBeLoaded.AH_Desc = descriptionToSet;
			}
		}
		public void SetNumberOfSupportingDocuments(ZByte numberOfSupportingDocumentsToSet)
		{
			if (IsExistingBatch)
			{
				fExistingDepositBatchToBeLoaded.AH_NumberOfSupportingDocuments = numberOfSupportingDocumentsToSet;
			}
		}

		public void ApplyWorkflowTemplatesOnReverseTransaction()
		{
		}

		public ZString ReversingCode
		{
			get { return reversingCode; }
			set { reversingCode = value; }
		}

		public ZString ReversingReason
		{
			get { return reversingReason; }
			set
			{
				if (IsExistingBatch)
				{
					fExistingDepositBatchToBeLoaded.AH_Desc = new ZString(fExistingDepositBatchToBeLoaded.AH_Desc + " " + value).Left(fExistingDepositBatchToBeLoaded.AH_DescInfo.MaxLength);
					reversingReason = value;
				}
			}
		}

		public bool IsClearedInCashbook
		{
			get
			{
				return IsExistingBatch && !fExistingDepositBatchToBeLoaded.AH_DateClearedInCashbook.IsEmpty;
			}
		}

		public bool IsPeriodSubLedgerClosed
		{
			get
			{
				bool result = false;

				if (IsExistingBatch)
				{
					int postedPeriod = PeriodCalculator.GetPeriodFromDate(fExistingDepositBatchToBeLoaded.AH_PostDate);
					result = PeriodCalculator.IsPeriodSubLedgerClosed(postedPeriod);
				}
				return result;
			}
		}

		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}

				return fPeriodCalculator;
			}
		}
		AccountingPeriodCalculator fPeriodCalculator;

		public string[] MultipleReversingErrors
		{
			get { return MultipleReversingErrors_innerValue; }
			set { MultipleReversingErrors_innerValue = value; }
		}
		string[] MultipleReversingErrors_innerValue = Array.Empty<string>();

		#endregion

		#region Collections

		public GlbBranchCollection Branches
		{
			get
			{
				if (fDepositBatchLookups == null)
				{
					DepositBatch batch = Factory.New<DepositBatch>();
					fDepositBatchLookups = new DepositBatchLookups(batch);
				}
				return fDepositBatchLookups.Branches;
			}
		}
		DepositBatchLookups fDepositBatchLookups;

		public DepositBatchCollection DepositBatchLines
		{
			get
			{
				if (fDepositBatchLines == null)
				{
					fDepositBatchLines = new DepositBatchCollection(Factory);
					RegisterEditableChildObject(fDepositBatchLines);

					if (fExistingDepositBatchToBeLoaded == null)
					{
						fDepositBatchLines.CreateDepositBatches(this);
					}
					else
					{
						fDepositBatchLines.Add(fExistingDepositBatchToBeLoaded);
					}
				}
				return fDepositBatchLines;
			}
		}

		#endregion

		#region Properties

		#region DepositDate

		[BusinessObjectTestExclude]
		public ZDateTime DepositDate
		{
			get { return depositDate; }
			set
			{
				SetNonPersistentPropertyValue(DepositDateInfo, ref depositDate, value);
				foreach (DepositBatch depositBatch in DepositBatchLines)
				{
					depositBatch.AH_InvoiceDate = DepositDate;
				}
			}
		}

		public ZPropertyInfo DepositDateInfo
		{
			get { return GetZPropertyInfo(nameof(DepositDate)); }
		}

		protected bool DepositDate_ReadOnly
		{
			get { return fExistingDepositBatchToBeLoaded != null; }
		}

		#endregion

		#region DepositPostDate

		[BusinessObjectTestExclude]
		public ZDateTime DepositPostDate
		{
			get { return depositPostDate; }
			set
			{
				SetNonPersistentPropertyValue(DepositPostDateInfo, ref depositPostDate, value);
				foreach (DepositBatch depositBatch in DepositBatchLines)
				{
					depositBatch.AH_PostDate = depositPostDate;
				}
				if (!IsValidationSuspended)
				{
					ValidateDepositPostDate();
				}
			}
		}

		public ZPropertyInfo DepositPostDateInfo
		{
			get { return GetZPropertyInfo(nameof(DepositPostDate)); }
		}

		protected bool DepositPostDate_ReadOnly
		{
			get { return fExistingDepositBatchToBeLoaded != null; }
		}

		#endregion

		#region BatchNumber

		[ReadOnly(true)]
		[MaxLength(AccTransactionHeader.Schema.AH_TransactionNumMaxLength)]
		public ZString BatchNumber
		{
			get { return batchNumber; }
			set
			{
				CheckMaximumLength(BatchNumberInfo, value);
				SetNonPersistentPropertyValue(BatchNumberInfo, ref batchNumber, value);
			}
		}

		public ZPropertyInfo BatchNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BatchNumber)); }
		}

		#endregion

		public ZBool IsExistingBatch
		{
			get { return fExistingDepositBatchToBeLoaded != null; }
		}

		public ZString DepositBatchCode => fExistingDepositBatchToBeLoaded?.AH_TransactionNum ?? ZString.Empty;

		#region FilterByAllCurrencies

		[ReadOnlyMember(nameof(IsExistingBatch))]
		public ZBool FilterByAllCurrencies
		{
			get { return filterByAllCurrencies; }
			set
			{
				bool hasChanges = filterByAllCurrencies != value;
				SetNonPersistentPropertyValue(FilterByAllCurrenciesInfo, ref filterByAllCurrencies, value);
				if (value && hasChanges)
				{
					LoadDepositBatchLines();
				}
			}
		}

		public ZPropertyInfo FilterByAllCurrenciesInfo
		{
			get { return GetZPropertyInfo(nameof(FilterByAllCurrencies)); }
		}

		#endregion

		#region FilterByForeignCurrency

		[ReadOnlyMember(nameof(IsExistingBatch))]
		public ZBool FilterByForeignCurrency
		{
			get { return filterByForeignCurrency; }
			set
			{
				bool hasChanges = filterByForeignCurrency != value;
				SetNonPersistentPropertyValue(FilterByForeignCurrencyInfo, ref filterByForeignCurrency, value);
				if (value && hasChanges)
				{
					LoadDepositBatchLines();
				}
			}
		}

		public ZPropertyInfo FilterByForeignCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(FilterByForeignCurrency)); }
		}

		#endregion

		#region FilterByLocalCurrency

		[ReadOnlyMember(nameof(IsExistingBatch))]
		public ZBool FilterByLocalCurrency
		{
			get { return filterByLocalCurrency; }
			set
			{
				bool hasChanges = filterByLocalCurrency != value;
				SetNonPersistentPropertyValue(FilterByLocalCurrencyInfo, ref filterByLocalCurrency, value);
				if (value && hasChanges)
				{
					LoadDepositBatchLines();
				}
			}
		}

		public ZPropertyInfo FilterByLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(FilterByLocalCurrency)); }
		}

		#endregion

		#region BranchFilterPK

		[List("Branches")]
		public ZGuid BranchFilterPK
		{
			get { return branchFilterPK; }
			set
			{
				bool hasChanges = branchFilterPK != value;
				SetNonPersistentPropertyValue(BranchFilterPKInfo, ref branchFilterPK, value);
				if (hasChanges)
				{
					ValidateBranch();
					LoadDepositBatchLines();
				}
			}
		}

		public ZPropertyInfo BranchFilterPKInfo
		{
			get { return GetZPropertyInfo(nameof(BranchFilterPK)); }
		}

		protected bool BranchFilterPK_ReadOnly
		{
			get { return !FilterByBranch || IsExistingBatch; }
		}

		void ValidateBranch()
		{
			if (!IsValidationSuspended)
			{
				BranchFilterPKInfo.ClearAllNotifications();
				if (!BranchFilterPK.IsValid && !BranchFilterPK.IsEmpty)
				{
					BranchFilterPKInfo.AddError(Res.GetString("a230c49e-1911-4a24-8ecc-3c4bf90b175a", "This branch is not valid."));
				}
			}
		}

		#endregion

		#region FilterByBranch

		[ReadOnlyMember(nameof(IsExistingBatch))]
		public ZBool FilterByBranch
		{
			get { return filterByBranch; }
			set
			{
				SetNonPersistentPropertyValue(FilterByBranchInfo, ref filterByBranch, value);
				filterByCompany = !value;
				if (branchFilterPK == ZGuid.Empty)
				{
					branchFilterPK = GlbBranch.CurrentBranch.PK;
				}
				LoadDepositBatchLines();
				BranchFilterPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FilterByBranchInfo
		{
			get { return GetZPropertyInfo(nameof(FilterByBranch)); }
		}

		#endregion

		#region FilterByCompany

		[ReadOnlyMember(nameof(IsExistingBatch))]
		public ZBool FilterByCompany
		{
			get { return filterByCompany; }
			set
			{
				SetNonPersistentPropertyValue(FilterByCompanyInfo, ref filterByCompany, value);
				filterByBranch = !value;
				if (branchFilterPK == ZGuid.Empty)
				{
					branchFilterPK = GlbBranch.CurrentBranch.PK;
				}
				LoadDepositBatchLines();
				BranchFilterPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FilterByCompanyInfo
		{
			get { return GetZPropertyInfo(nameof(FilterByCompany)); }
		}

		#endregion

		#region TotalNoOfTransactionsSelected

		public ZInt TotalNoOfTransactionsSelected
		{
			get
			{
				int result = 0;

				foreach (DepositBatch depositBatch in DepositBatchLines)
				{
					foreach (DepositBatchTransactionLine line in depositBatch.Transactions)
					{
						if (line.IsSelected)
						{
							result++;
						}
					}
				}

				return result;
			}
		}

		public ZPropertyInfo TotalNoOfTransactionsSelectedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalNoOfTransactionsSelected)); }
		}

		#endregion

		public void SelectAllTransactions(bool select)
		{
			if (!IsExistingBatch)
			{
				foreach (DepositBatch line in DepositBatchLines)
				{
					line.IsSelected = select;
				}
			}
		}

		public void SelectAllTransactionsForBankAccount(string bankCode, bool select)
		{
			if (!IsExistingBatch)
			{
				foreach (DepositBatch line in DepositBatchLines)
				{
					if (line.BankCode == bankCode)
					{
						line.IsSelected = select;
					}
				}
			}
		}

		public bool HasDirectCreditReceipt
		{
			get
			{
				bool result = false;
				foreach (DepositBatch line in DepositBatchLines)
				{
					foreach (DepositBatchTransactionLine tranLine in line.Transactions)
					{
						if (tranLine.AH_ReceiptType == ReceiptTypes.DirectCredit)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public bool HasCancelledReceipt
		{
			get
			{
				bool result = false;
				foreach (DepositBatch line in DepositBatchLines)
				{
					foreach (DepositBatchTransactionLine tranLine in line.Transactions)
					{
						if (tranLine.AH_IsCancelled)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		void LoadDepositBatchLines()
		{
			if (!IsInDatabase && fDepositBatchLines != null)
			{
				fDepositBatchLines.CreateDepositBatches(this);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDepositPostDate();
		}

		protected void CheckDepositPostDateNotInFuture()
		{
			if (!DepositPostDateInfo.HasErrors())
			{
				if (DepositPostDate.Date > ZDateTime.Today)
				{
					if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
					{
						DepositPostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
					}
					else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
					{
						DepositPostDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
					}
				}
			}
		}

		public void ValidateDepositPostDate()
		{
			DepositPostDateInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(DepositPostDateInfo);
			var periodValidation = new PeriodValidationProvider(Factory);
			periodValidation.CheckDateFallsIntoValidPeriod(DepositPostDateInfo);
			CheckDepositPostDateNotInFuture();

			ZDateTime latestTransactionPostDate = DateTime.MinValue;
			foreach (DepositBatch depositBatch in DepositBatchLines)
			{
				foreach (DepositBatchTransactionLine transaction in depositBatch.Transactions)
				{
					if (transaction.IsSelected && transaction.AH_PostDate.CompareTo(latestTransactionPostDate) > 0)
					{
						latestTransactionPostDate = transaction.AH_PostDate;
					}
				}
			}

			if (depositPostDate.Date.CompareTo(latestTransactionPostDate.Date) < 0)
			{
				DepositPostDateInfo.AddError(Res.GetString("e2c2a9da-6e20-4bcd-b41d-109c3597c47a", "Deposit Post Date must be equal to or later than each transaction's Post Date."));
			}
		}

		ZString reversingCode;
		ZString reversingReason;
		ZString batchNumber;
		ZDateTime depositDate;
		ZDateTime depositPostDate;
		ZBool filterByAllCurrencies;
		ZBool filterByForeignCurrency;
		ZBool filterByLocalCurrency;
		ZGuid branchFilterPK;
		ZBool filterByBranch;
		ZBool filterByCompany;
		protected DepositBatch fExistingDepositBatchToBeLoaded;
		DepositBatchCollection fDepositBatchLines;
		public DepositBatch DepositBatch => fExistingDepositBatchToBeLoaded;

		#endregion

		protected override BusinessObject LogsAndNotesTarget => fExistingDepositBatchToBeLoaded ?? Factory.New<DepositBatch>();
	}
}
