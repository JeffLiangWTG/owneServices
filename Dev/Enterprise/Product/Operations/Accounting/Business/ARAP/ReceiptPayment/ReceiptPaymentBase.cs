using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
	public abstract partial class ReceiptPaymentBase : TransactionHeader, IPayablesAndReceivables, IeNettPayment, IUnmatchOnReversing
	{
		public new abstract class Schema : TransactionHeader.Schema
		{
			public const string DisplayCashFlowCategoryOverride = "DisplayCashFlowCategoryOverride";
		}

		#region Type Decider

		public new static readonly TypeDecider TypeDecider = new TransactionHeaderTypeDecider();

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "To fix this we need more investigation as this is exisitng since 2008.")]
		protected ReceiptPaymentBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			LocalForeignDataEntryCalculator = new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateInfo, AH_LocalExTaxAmountInfo, AH_OSExTaxAmountInfo, ExchangeRate as ZAccExchangeRate);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_TransactionCategory), ConcurrencyPolicy.Strict);
		}
		protected readonly LocalForeignDataEntry LocalForeignDataEntryCalculator;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_ReceiptType = this is Receipt ? AccountingConfigurationRegistry.Instance.DefaultReceiptType.Value : AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value;
		}

		protected abstract void SetDefaultBankAccount();

		#endregion

		#region Can Apply Data Refresh

		protected override ZPropertyInfo[] GetPropertiesWithStrictConcurrency()
		{
			var additionalPropertiesWithStrictConcurrency = new ZPropertyInfo[] { AH_TransactionCategoryInfo };
			return base.GetPropertiesWithStrictConcurrency().Concat(additionalPropertiesWithStrictConcurrency).ToArray();
		}

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				RefreshBinding();
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (IsInDatabase)
			{
				if ((ZString)AH_TransactionCategoryInfo.OriginalValue != AH_TransactionCategory)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("Cash Flow Category Modified. Previous value: '[{0}]', New value: '({1})'", AH_TransactionCategoryInfo.OriginalValue, AH_TransactionCategory));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
			else if (WorkflowDescriptors.Instance.TryGetValueSafe(GetWorkflowType()) != null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}
		}

		protected override void DeleteCore()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.DeleteCore();
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (!IsInDatabase && AH_TransactionCategory.IsEmpty)
			{
				AH_TransactionCategory = DefaultCashFlowCategory;
			}

			if (!IsInDatabase && !IsInMatchingContext)
			{
				AH_MatchStatus = AccountingConstants.MatchStatusTypes.Unallocated.Code;
				AH_MatchStatusReasonCode = AccountingConstants.MatchStatusReasonCodeTypes.InAdvance.Code;
			}
		}

		internal bool IsLoadedFromGUI
		{
			get { return fIsLoadedFromGUI; }
			set
			{
				if (fIsLoadedFromGUI != value)
				{
					fIsLoadedFromGUI = value;
					fMatchingBaseObject = null;
				}
			}
		}
		bool fIsLoadedFromGUI = true;

		public void UnhookLocalForeignAmountRecalculation()
		{
			LocalForeignDataEntryCalculator.UnhookEvents();
		}

		public void AddTransactionThatExcludedFromUnmatchedList(IMatching transaction)
		{
			TransactionsCanNotBeMatched.Add(transaction);
		}

		public abstract ZString ExchangeRateType { get; }

		public bool IsBankCurrencyLocal => BankAccount != null && BankAccount.AB_RX_NKAccountCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		#region Properties

		#region Overriden Readonly Properties

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get { return !IsBankCurrencyLocal; }
			set { base.AH_RX_NKTransactionCurrency_ReadOnly = value; }
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_PostDate_ReadOnly
		{
			get { return false; }
		}

		#endregion

		#region SetOutstandingLocalAmountAfterLocalExTaxAmountSet

		protected override void SetOutstandingLocalAmountAfterLocalExTaxAmountSet()
		{
			SetOutstandingLocalAmountAfterLocalExTaxAmountSetCore();
		}

		#endregion

		#region AH_OH

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				var hasChanges = base.AH_OHCore != value;
				base.AH_OHCore = value;
				if (hasChanges && !IsInDatabase && Header != null && Header.CompanyData != null)
				{
					SetDefaultBankAccount();
				}
			}
		}

		#endregion

		#region AH_AB

		[List("BankAccounts")]
		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				base.AH_AB = value;
				if (BankAccount != null)
				{
					ExchangeRate.Currency = BankAccount.AB_RX_NKAccountCurrency;

					if (ExchangeRate.Currency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						ExchangeRate.RefetchExchangeRate();
					}

					if (AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
					{
						AH_ChequeOrReference = ZString.Empty;
					}
				}
				AH_ABInfo.RefreshBinding();
			}
		}

		#endregion

		#region AH_ExchangeRate

		protected virtual bool AH_ExchangeRate_ReadOnly
		{
			get
			{
				bool result = false;
				if (!inAH_ExchangeRate_ReadOnly)
				{
					inAH_ExchangeRate_ReadOnly = true;
					result = ExchangeRate.RateInfo.ReadOnly;
					inAH_ExchangeRate_ReadOnly = false;
				}
				return result;
			}
		}
		bool inAH_ExchangeRate_ReadOnly;

		#endregion

		#region AH_RX_NKTransactionCurrency

		public override ZString AH_RX_NKTransactionCurrency
		{
			get { return base.AH_RX_NKTransactionCurrency; }
			set
			{
				if (base.AH_RX_NKTransactionCurrency != value)
				{
					var oldOSCurrenctDecimal = OSCurrencyDecimals;
					base.AH_RX_NKTransactionCurrency = value;
					var newOSCurrencyDecimal = OSCurrencyDecimals;
					if (oldOSCurrenctDecimal != newOSCurrencyDecimal)
					{
						AH_OSExTaxAmount = RoundAmountToCurrencyDecimals(AH_OSExTaxAmount);
						AH_OSTaxAmount = RoundAmountToCurrencyDecimals(AH_OSTaxAmount);
					}
				}
			}
		}

		#endregion

		#region AH_ReceiptType

		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set
			{
				base.AH_ReceiptType = value;
				SetReceiptType();
			}
		}

		#endregion

		#region AH_Calc_ChequeReferenceLabel

		public ZString AH_Calc_ChequeReferenceLabel
		{
			get { return AH_ReceiptType == ReceiptTypes.Cheque ? (NoResString)"Check Number" : (NoResString)"Reference Number"; }
		}

		public ZPropertyInfo AH_Calc_ChequeReferenceLabelInfo
		{
			get { return GetZPropertyInfo(nameof(AH_Calc_ChequeReferenceLabel)); }
		}

		#endregion

		#region Debit

		public override ZDecimal Debit
		{
			get { return DebitForNormalReceiptPayment; }
		}

		public override ZDecimal Credit
		{
			get { return CreditForNormalReceiptPayment; }
		}

		#endregion

		#region DisplayCashFlowCategoryOverride

		[List("DisplayCashFlowCategoryOverrides")]
		public ZString DisplayCashFlowCategoryOverride
		{
			get { return AH_TransactionCategory; }
			set { AH_TransactionCategory = value; }
		}

		public ZPropertyInfo DisplayCashFlowCategoryOverrideInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.DisplayCashFlowCategoryOverride, x => AH_TransactionCategoryInfo); }
		}

		public bool DisplayCashFlowCategoryOverride_ReadOnly
		{
			get { return AH_TransactionCategoryInfo.ReadOnly; }
		}

		public CodeDescriptionPairList DisplayCashFlowCategoryOverrides
		{
			get
			{
				if (fCashFlowTypeList == null)
				{
					fCashFlowTypeList = new CodeDescriptionPairList();
					foreach (CashFlowActivityConfiguration cashFlowActivity in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
					{
						if (cashFlowActivity.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Undefined &&
							cashFlowActivity.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Cash &&
							cashFlowActivity.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.NonCash &&
							cashFlowActivity.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Exchange)
						{
							fCashFlowTypeList.AddPair(cashFlowActivity.Code, cashFlowActivity.Description);
						}
					}
				}

				return fCashFlowTypeList;
			}
		}
		CodeDescriptionPairList fCashFlowTypeList;

		protected ZString DefaultCashFlowCategory
		{
			get
			{
				ZString result = ZString.Empty;
				if (AH_OH.IsValid)
				{
					var org = Factory.Load<OrgHeader>(AH_OH);
					if (org != null && org.CompanyData != null)
					{
						if (AH_Ledger == LedgerTypes.AccountsReceivable)
						{
							ZGuid arDebtorGroupPK = org.CompanyData.OB_OJ_ARDebtorGroup;
							foreach (CashFlowCategoryBasedOnDebtorGroup x in AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.Value)
							{
								if (x.OrgGroupPK == arDebtorGroupPK)
								{
									result = x.CashFlowCategory;
									break;
								}
							}
						}
						else if (AH_Ledger == LedgerTypes.AccountsPayable)
						{
							ZGuid apCreditorGroupPK = org.CompanyData.OB_OG_APCreditorGroup;
							foreach (CashFlowCategoryBasedOnCreditorGroup x in AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.Value)
							{
								if (x.OrgGroupPK == apCreditorGroupPK)
								{
									result = x.CashFlowCategory;
									break;
								}
							}
						}
					}
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Lookups

		#region Bank Accounts

		public AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					ZQuery branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, AH_GB);
					branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

					ZQuery bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, AH_GC);
					bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					bankFilter.AddToFilter(branchFilter, JoinCondition.And);

					fBankAccounts = new AccBankAccountCollection(Factory, bankFilter);
				}
				return fBankAccounts;
			}
		}

		AccBankAccountCollection fBankAccounts;

		#endregion

		#endregion

		#region MatchingBaseObject

		public MatchingBase MatchingBaseObject
		{
			get
			{
				if (fMatchingBaseObject == null)
				{
					if (Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
					{
						fMatchingBaseObject = new APMatchingBase(Factory, this, IsLoadedFromGUI);
					}
					else if (Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
					{
						fMatchingBaseObject = new ARMatchingBase(Factory, this, IsLoadedFromGUI);
					}
				}
				PrepareReceiptPaymentForMatching();
				if (TransactionsCanNotBeMatched.Count > 0)
				{
					fMatchingBaseObject.ExcludeTransactionsFromUnmatchedList(TransactionsCanNotBeMatched);
				}
				return fMatchingBaseObject;
			}
		}
		MatchingBase fMatchingBaseObject;

		#endregion

		#region MatchingDate

		//Date of current matching proccess
		public ZDateTime CurrentMatchingDate
		{
			get { return fCurrentMatchingDate; }
			set { fCurrentMatchingDate = value; }
		}
		ZDateTime fCurrentMatchingDate;

		#endregion

		#region Validation

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new ReceiptPaymentBaseValidation(this);
		}

		#endregion

		#region Implementation

		#region AccValidationHelper

		protected AccValidationHelper AccValidationHelper
		{
			get
			{
				if (fAccValidationHelper == null)
				{
					fAccValidationHelper = new AccValidationHelper();
				}
				return fAccValidationHelper;
			}
		}
		AccValidationHelper fAccValidationHelper;

		#endregion

		#region GenerateReverseTransaction

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);

			fReverseTransaction.AH_ChequeOrReference = this.AH_ChequeOrReference;   // must be set after AH_ReceiptType
		}

		#endregion

		#region ApplyWorkflowTemplatesOnReverseTransactionCore

		protected override void ApplyWorkflowTemplatesOnReverseTransactionCore()
		{
			base.ApplyWorkflowTemplatesOnReverseTransactionCore();
			var reverseReceiptPaymentBase = fReverseTransaction as ReceiptPaymentBase;
			if (reverseReceiptPaymentBase != null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(reverseReceiptPaymentBase);
			}
		}

		#endregion

		#region SetReceiptType

		protected virtual void SetReceiptType()
		{
			SetReferenceNumber();
		}

		void SetReferenceNumber()
		{
			var defaultReferenceNumber = AccountingConfigurationRegistry.Instance.GetReferenceNumberFromType(AH_ReceiptType);
			if (defaultReferenceNumber.IsEmpty)
			{
				SetReferenceNumberCore();
			}
			else
			{
				AH_ChequeOrReference = defaultReferenceNumber;
			}
		}

		protected virtual void SetReferenceNumberCore()
		{
			if (AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash)
			{
				AH_ChequeOrReference = ZArchitecture.Core.ReceiptTypes.Cash;
			}
			else if (AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
			{
				AH_ChequeOrReference = ZString.Empty;
			}
			else if (AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.CreditCard)
			{
				AH_ChequeOrReference = ZString.Empty;
			}
		}

		#endregion

		#region SetDefaultBankAccountAR

		protected void SetDefaultBankAccountAR()
		{
			ZGuid defaultBank = new ReceiptPaymentDefaultsAR(this).GetDefaultBankAccount();
			if (defaultBank.IsValid)
			{
				AH_AB = defaultBank;
			}
		}

		#endregion

		#region SetDefaultBankAccountAP

		protected void SetDefaultBankAccountAP()
		{
			ZGuid defaultBank = new ReceiptPaymentDefaultsAP(this).GetDefaultBankAccount();
			if (defaultBank.IsValid)
			{
				AH_AB = defaultBank;
			}
		}

		#endregion

		protected virtual void PrepareReceiptPaymentForMatching()
		{
			IsMatching = true;
			((IMatching)this).OSPartialPaymentAmountInfo.RefreshBinding();
		}

		#region TransactionsCanNotBeMatched

		IMatchingCollection TransactionsCanNotBeMatched
		{
			get
			{
				if (fTransactionsCanNotBeMatched == null)
				{
					fTransactionsCanNotBeMatched = new IMatchingCollection(Factory);
				}

				return fTransactionsCanNotBeMatched;
			}
		}

		IMatchingCollection fTransactionsCanNotBeMatched;

		#endregion

		#endregion

		#region IMatching Members

		ZString IMatching.RelatedDisbursementTransactions { get { return ZString.Empty; } }
		ZString IMatching.RelatedTransactionDebtorsAsString { get { return ZString.Empty; } }
		ZString IMatching.CreatingUser { get { return ZString.Empty; } }
		ZString IMatching.PaymentCriticality { get { return ZString.Empty; } }
		ZDateTime IMatching.PaymentRequestedDate { get { return ZDateTime.Empty; } }
		ZString IMatching.VoyageVesselOrFlightDate { get { return ZString.Empty; } }
		ZString IMatching.ShipmentHouseBill { get { return ZString.Empty; } }
		ZString IMatching.ShipmentMasterBill { get { return ZString.Empty; } }
		ZInt IMatching.CurrencyDecimals { get { return AH_Calc_RXDecimals; } }
		ZPropertyInfo IMatching.CurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo("CurrencyDecimals"); }
		}

		ZInt IMatching.LoginCompanyCurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.GetLocalDecimals(); }
		}

		ZPropertyInfo IMatching.LoginCompanyCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo("LoginCompanyCurrencyDecimals"); }
		}

		bool IMatching.IsMatched
		{
			get
			{
				return AH_LocalOutstandingAmount != AH_LocalTotalAmount;
			}
		}

		bool IMatching.IsAllPaidInTheSameCurrency(ZString currencyNK)
		{
			return currencyNK == AH_RX_NKTransactionCurrency;
		}

		ZDecimal IMatching.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK)
		{
			return currencyNK == AH_RX_NKTransactionCurrency ? ((IMatching)this).OSPartialPaymentAmount : 0;
		}

		void IMatching.FullyPay(ZDateTime fullyPaidDate)
		{
			MatchingMonitor.FullyPay(fullyPaidDate);
		}

		void IMatching.PartiallyPay()
		{
			MatchingMonitor.PartiallyPay();
		}

		void IMatching.GenerateMatchLinksCore()
		{
			MatchingMonitor.GenerateMatchLinks();
		}

		void IMatching.GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
			MatchingMonitor.GeneratePaymentApprovalItems(approval);
		}

		PaymentApprovalItemCollection IMatching.PaymentApprovalItems
		{
			get
			{
				if (fPaymentApprovalItems == null)
				{
					fPaymentApprovalItems = new PaymentApprovalItemCollection(Factory);
				}

				return fPaymentApprovalItems;
			}
		}

		PaymentApprovalItemCollection fPaymentApprovalItems;
		
		UnmatchingResult IMatching.CanUnmatch(ZDecimal matchLinkAmount)
		{
			return CanUnmatchMatchLink(matchLinkAmount);
		}

		void IMatching.Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			UnmatchCore(matchLinkAmount, matchLinkOSAmount);
		}

		protected virtual void UnmatchCore(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			AH_FullyPaidDate = ZDateTime.Empty;
			TransactionHeaderOSOutstandingAmountProvider.ForceToSetOutstandingAmounts(this, matchLinkAmount, matchLinkOSAmount, true);
		}

		void IMatching.ChangeUnmatchDate(ZDateTime unmatchDate)
		{
		}

		TransactionMatchLinkGroup IMatching.CurrentMatchGroup
		{
			get { return fCurrentMatchGroup ?? (fCurrentMatchGroup = new TransactionMatchLinkGroup(Factory)); }
		}
		protected TransactionMatchLinkGroup fCurrentMatchGroup;

		TransactionMatchLinkCollection IMatching.Matchlinks
		{
			get
			{
				return matchlinks ?? (matchlinks = new TransactionMatchLinkCollection(Factory, new ZQuery(AccTransactionMatchLinkSchema.AP_AH, PK)));
			}
		}
		TransactionMatchLinkCollection matchlinks;

		ZDecimal IMatching.OSOutstandingAmount
		{
			get { return OSOutstandingAmountMatching; }
		}

		ZPropertyInfo IMatching.OSOutstandingAmountInfo
		{
			get { return GetZPropertyInfo("OSOutstandingAmount"); }
		}

		ZDecimal IMatching.OutstandingAmount
		{
			get { return OutstandingAmountMatching; }
		}

		ZPropertyInfo IMatching.OutstandingAmountInfo
		{
			get { return GetZPropertyInfo("OutstandingAmount"); }
		}

		ZDecimal IMatching.OriginalOutstandingAmount
		{
			get { return AH_OutstandingAmount; }
		}

		[List("Organisations")]
		ZGuid IMatching.Organisation
		{
			get { return AH_OH; }
		}

		ZPropertyInfo IMatching.OrganisationInfo
		{
			get { return GetZPropertyInfo("Organisation"); }
		}

		ZString IMatching.Ledger
		{
			get { return AH_Ledger; }
		}

		ZPropertyInfo IMatching.LedgerInfo
		{
			get { return GetZPropertyInfo("Ledger"); }
		}

		ZString IMatching.TransactionNumber
		{
			get { return AH_TransactionNum; }
		}

		ZPropertyInfo IMatching.TransactionNumberInfo
		{
			get { return GetZPropertyInfo("TransactionNumber"); }
		}

		[ReadOnlyMember(nameof(IsMatching))]
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal OSPartialPaymentAmount
		{
			get { return fOSPartialPaymentAmount; }
			set
			{
				fOSPartialPaymentAmount = value;
				((IMatching)this).OSPartialPaymentAmountInfo.RefreshBinding();
				((IMatching)this).LocalPartialPaymentAmountInfo.RefreshBinding();
				if (!IsValidationSuspended && Validation is MatchingValidation)
				{
					((MatchingValidation)Validation).ValidateOSPartialPaymentAmount();
				}
				CollectInfoIfOSPartialPaymentAmountIsGreaterThanOustandingAmount();
			}
		}

		void CollectInfoIfOSPartialPaymentAmountIsGreaterThanOustandingAmount()
		{
			if (Ledger == LedgerTypes.AccountsPayable && TransactionType == TransactionTypes.Payment && OSPartialPaymentAmount > OSOutstandingAmountMatching)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK,
					CriticalValidationInfoCollectorServiceKeyType.OSPartialPaymentAmountIsGreaterThanOutstandingAmount,
					() => FormattableString.Invariant($@"
OSPartialPaymentAmount: {OSPartialPaymentAmount}
OSOutstandingAmountMatching: {OSOutstandingAmountMatching}
AH_Calc_OSOutstandingAmount: {AH_Calc_OSOutstandingAmount}
LocalPartialPaymentAmount: {((IMatching)this).LocalPartialPaymentAmount}
OutstandingAmountMatching: {OutstandingAmountMatching}
AH_OutstandingAmount: {AH_OutstandingAmount}
AH_RX_NKTransactionCurrency: {AH_RX_NKTransactionCurrency}
AH_ExchangeRate: {AH_ExchangeRate}
ValidationType: {Validation.GetType()}
IsValidationSuspended: {IsValidationSuspended.ToYesNoString()},
Stack Trace: {System.Environment.StackTrace}"));
			}
		}

		ZDecimal fOSPartialPaymentAmount;

		ZPropertyInfo IMatching.OSPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSPartialPaymentAmount)); }
		}

		protected bool IsMatching { get; set; }

		ZDecimal IMatching.LocalPartialPaymentAmount
		{
			get
			{
				ZDecimal localAmountTemp = OutstandingAmountMatching;
				if (((IMatching)this).OSPartialPaymentAmount != OSOutstandingAmountMatching)
				{
					localAmountTemp = (ZDecimal)Env.CurrentCompany.ExchangeRate.ForeignToLocal(((IMatching)this).OSPartialPaymentAmount, GetHighPrecisionExchangeRate());
				}

				return localAmountTemp;
			}
		}

		ZPropertyInfo IMatching.LocalPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo("LocalPartialPaymentAmount"); }
		}

		ZString IMatching.TransactionType
		{
			get { return AH_TransactionType; }
		}

		ZPropertyInfo IMatching.TransactionTypeInfo
		{
			get { return GetZPropertyInfo("TransactionType"); }
		}

		ZGuid IMatching.BranchGuid
		{
			get { return AH_GB; }
		}

		ZPropertyInfo IMatching.BranchGuidInfo
		{
			get { return GetZPropertyInfo("BranchGuid"); }
		}

		ZGuid IMatching.DepartmentGuid
		{
			get { return AH_GE; }
		}

		ZPropertyInfo IMatching.DepartmentGuidInfo
		{
			get { return GetZPropertyInfo("DepartmentGuid"); }
		}

		ZPropertyInfo IMatching.PaymentCurrencyCodeInfo
		{
			get { return CurrencyCodeInfo; }
		}

		[MaxLength(3)]
		[List("Lookups.TransactionCurrencies")]
		public ZString CurrencyCode
		{
			get { return AH_RX_NKTransactionCurrency; }
		}

		public ZPropertyInfo CurrencyCodeInfo => GetZPropertyInfo(nameof(AH_RX_NKTransactionCurrency));

		ZString IMatching.Description
		{
			get { return AH_Desc; }
		}

		ZPropertyInfo IMatching.DescriptionInfo
		{
			get { return GetZPropertyInfo("Description"); }
		}

		ZDateTime IMatching.PostDate
		{
			get { return AH_PostDate; }
			set { AH_PostDate = value; }
		}

		ZPropertyInfo IMatching.PostDateInfo
		{
			get { return GetZPropertyInfo("PostDate"); }
		}

		bool IMatching.PostDate_ReadOnly
		{
			get { return true; }
		}

		public ZString ChequeOrReference
		{
			get { return AH_ChequeOrReference; }
			set { AH_ChequeOrReference = value; }
		}

		ZString IMatching.ChequeOrReference
		{
			get { return ChequeOrReference; }
			set { ChequeOrReference = value; }
		}

		bool IMatching.ChequeOrReference_ReadOnly { get; set; }

		public ZPropertyInfo ChequeOrReferenceInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ChequeOrReference), x => AH_ChequeOrReferenceInfo); }
		}

		ZPropertyInfo IMatching.ChequeOrReferenceInfo
		{
			get { return ChequeOrReferenceInfo; }
		}

		ZDecimal IMatching.ExchangeRateAmount
		{
			get { return AH_ExchangeRate; }
		}

		ZPropertyInfo IMatching.ExchangeRateAmountInfo
		{
			get { return GetZPropertyInfo("ExchangeRateAmount"); }
		}

		ZString IMatching.TransactionReference
		{
			get { return AH_TransactionReference; }
		}

		ZPropertyInfo IMatching.TransactionReferenceInfo
		{
			get { return GetZPropertyInfo("TransactionReference"); }
		}

		ZString IMatching.ConsolidatedRef
		{
			get { return AH_ConsolidatedInvoiceRef; }
		}

		ZPropertyInfo IMatching.ConsolidatedRefInfo
		{
			get { return GetZPropertyInfo("ConsolidatedRef"); }
		}

		ZDateTime IMatching.InvoiceDate
		{
			get { return AH_InvoiceDate; }
		}

		ZPropertyInfo IMatching.InvoiceDateInfo
		{
			get { return GetZPropertyInfo("InvoiceDate"); }
		}

		ZDateTime IMatching.DueDate
		{
			get { return AH_DueDate; }
		}

		ZPropertyInfo IMatching.DueDateInfo
		{
			get { return GetZPropertyInfo("DueDate"); }
		}

		ZString IMatching.MatchStatus
		{
			get { return AH_MatchStatus; }
			set { AH_MatchStatus = value; }
		}

		ZPropertyInfo IMatching.MatchStatusInfo => GetZPropertyInfo("MatchStatus");

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusList => AccountingUtils.GetMatchStatusList();

		ZString IMatching.MatchStatusReasonCode
		{
			get { return AH_MatchStatusReasonCode; }
			set { AH_MatchStatusReasonCode = value; }
		}

		ZPropertyInfo IMatching.MatchStatusReasonCodeInfo => GetZPropertyInfo("MatchStatusReasonCode");

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusReasonCodeList => AccountingUtils.GetMatchStatusReasonCodeList();

		ZDateTime IMatching.MatchDate
		{
			get
			{
				ZDateTime dateToReturn = ZDateTime.Empty;
				if (LatestMatchLink != null)
				{
					dateToReturn = LatestMatchLink.AP_MatchDate;
				}

				return dateToReturn;
			}
		}

		ZPropertyInfo IMatching.MatchDateInfo
		{
			get { return GetZPropertyInfo("MatchDate"); }
		}

		OrgHeaderCollection IMatching.Organisations
		{
			get { return Lookups.Headers; }
		}

		GlbBranchCollection IMatching.BranchCollection
		{
			get { return Lookups.Branches; }
		}

		GlbDepartmentCollection IMatching.DepartmentCollection
		{
			get { return Lookups.Departments; }
		}

		RefCurrencyCollection IMatching.Currencies
		{
			get { return Lookups.TransactionCurrencies; }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal IMatching.NotionalWHTTax => AH_NotionalWHTTax;
		ZPropertyInfo IMatching.NotionalWHTTaxInfo => AH_NotionalWHTTaxInfo;
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal IMatching.RealizedWHTTax => AH_RealizedWHTTax;
		ZPropertyInfo IMatching.RealizedWTHTaxInfo => AH_RealizedWHTTaxInfo;
		#endregion

		#region CardSecurityCode

		public ZString CardSecurityCode
		{
			get;
			set;
		}

		#endregion

		#region IeNettPayment Members

		public ZString PaymentType
		{
			get { return AH_ReceiptType; }
		}

		public eNettWebServiceResult PayWithCreditCardViaENett()
		{
			IeNettWebServiceWrapper wrapper = ObjectFactory.Get<IeNettWebServiceWrapper>();
			return wrapper.ProcessCreditCard(this, CardSecurityCode);
		}

		public void PayWithEnettDirectDebitFX()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUnmatchOnReversing Members

		public IUnmatchingData UnmatchingData
		{
			get { return unmatchingDataProvider ?? (unmatchingDataProvider = new UnmatchingDataProvider(this)); }
		}
		IUnmatchingData unmatchingDataProvider;

		#endregion
	}
}
