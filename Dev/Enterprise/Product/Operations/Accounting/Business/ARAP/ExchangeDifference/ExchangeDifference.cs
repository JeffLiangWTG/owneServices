using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP
{
	[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
	public abstract partial class ExchangeDifference : TransactionHeader, IMatching, IReversing, IMiscellaneousTransaction
	{
		public ExchangeDifference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateInfo, BindableInvoiceAmountInfo, BindableOSAmountInfo, ExchangeRate as ZAccExchangeRate);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_Desc = AccountingConstants.MatchingDefaultDesc.ExchangeDifferenceDesc;

#if DEBUG
			AH_AG = AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value;
#endif
		}

		#region Required To Implement Interface

		protected override ZString TransactionType
		{
			get { return Enterprise.ZArchitecture.Core.TransactionTypes.ExchangeDifference; }
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		#endregion

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);
			using (fReverseTransaction.AmountsCalculationsSuspender.GetSuspender())
			{
				fReverseTransaction.AH_LocalExTaxAmount = -AH_LocalExTaxAmount;
			}
		}

		[ReadOnly(true)]
		public override ZGuid AH_AG
		{
			get { return base.AH_AG; }
			set { base.AH_AG = value; }
		}

		[ZUnbindableProperty()]
		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AH_OSTotal
		{
			get { return base.AH_OSTotal; }
			set
			{
				base.AH_OSTotal = value;

				if (value >= 0)
				{
					AH_AG = AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value;
				}
				else
				{
					AH_AG = AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount.Value;
				}
			}
		}

		#region IMatching Members

		ZString IMatching.RelatedDisbursementTransactions { get { return ZString.Empty; } }
		ZString IMatching.RelatedTransactionDebtorsAsString { get { return ZString.Empty; } }
		ZString IMatching.CreatingUser { get { return ZString.Empty; } }
		ZString IMatching.PaymentCriticality { get { return ZString.Empty; } }
		ZDateTime IMatching.PaymentRequestedDate { get { return ZDateTime.Empty; } }

		public bool IsMatched
		{
			get { return false; }
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
		}

		void IMatching.GenerateMatchLinksCore()
		{
			MatchingMonitor.GenerateMatchLinks();
		}

		void IMatching.GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
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
			return UnmatchingResult.Success;
		}

		void IMatching.Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			// set fields for this ExchangeDiff
			((IMatching)this).FullyPay(ZDateTime.Now);
			((IReversing)this).SetCancellationFlag(true);

			((IMatching)this).Matchlinks.Load();
			TransactionMatchLink originalLink = ((IMatching)this).Matchlinks[0];

			// Re-create original Matchlink because we cannot reuse it
			this.GenerateMatchLinks();
			TransactionMatchLinkOSAmountProvider.FillReversingMatchLinkAmounts(this, originalLink);

			// create the reversing Transaction, fully pay it, set TransactionBelongsToGroup and set cancellation
			((IReversing)this).GenerateReverseTransaction(true);
			((IReversing)this).SetTransactionBelongsToGroupField(ZGuid.NewZGuid());
			IReversing revTrans = ((IReversing)this).ReverseTransaction;

			var description = GetPaymentAndReceiptDescription(originalLink);

			revTrans.SetDescription(string.Format(CultureInfo.InvariantCulture, AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.UnExchangeDiff, Res.GetString("ba8cd348-ed09-49e4-87cd-5e009068b594", "EXCHANGE DIFFERENCE RELATING TO UN-MATCH NO")) + " {0}{1}", originalLink.AP_MatchGroupNum, description));

			revTrans.SetNumberOfSupportingDocuments(AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.UnExchangeDiff, 0));

			((IMatching)revTrans).FullyPay(ZDateTime.Now);
			revTrans.SetCancellationFlag(true);

			// generate the matchlink for reversing transaction
			((IMatching)revTrans).GenerateMatchLinks();
			((IMatching)this).CurrentMatchGroup.AddRange(((IMatching)revTrans).CurrentMatchGroup);
			((IMatching)revTrans).CurrentMatchGroup.RemoveAll();

			ZDateTime matchDate = ZDateTime.Now;
			foreach (TransactionMatchLink link in ((IMatching)this).CurrentMatchGroup)
			{
				link.AP_MatchDate = matchDate;
			}
		}

		protected override void SetDescriptionCore(ZString descriptionToSet)
		{
			AH_Desc = descriptionToSet.Left(AccTransactionHeader.Schema.AH_DescMaxLength);
		}

		string GetPaymentAndReceiptDescription(TransactionMatchLink originalLink)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionMatchLink), AccTransactionMatchLinkSchema.AP_AH);
			subQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, originalLink.AP_MatchGroupNum);
			subQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchDate, originalLink.AP_MatchDate);
			query.AddSubQuery(subQuery, JoinCondition.And);
			var transactionTypeQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
			transactionTypeQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			query.AddToFilter(transactionTypeQuery);
			query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_IsCancelled, 0);
			query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, AH_GC);
			query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, AH_Ledger);
			var transactions = Factory.Load<AccTransactionHeader>(query).OrderBy(x => x.AH_TransactionNum);

			return transactions.Any()
				? " [" + string.Join(", ", transactions.Select(x => x.AH_TransactionType + ":" + x.AH_TransactionNum)) + "]"
				: string.Empty;
		}

		void IMatching.ChangeUnmatchDate(ZDateTime unmatchDate)
		{
			if (ReverseTransaction != null)
			{
				ReverseTransaction.PostDate = unmatchDate;
				((IMatching)this).FullyPay(unmatchDate);
				((IMatching)ReverseTransaction).FullyPay(unmatchDate);
				foreach (TransactionMatchLink link in ((IMatching)this).CurrentMatchGroup)
				{
					link.AP_MatchDate = unmatchDate;
				}
			}
		}

		TransactionMatchLinkGroup IMatching.CurrentMatchGroup
		{
			get { return fCurrentMatchGroup ?? (fCurrentMatchGroup = new TransactionMatchLinkGroup(Factory)); }
		}
		TransactionMatchLinkGroup fCurrentMatchGroup;

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

		ZString IMatching.TransactionType
		{
			get { return AH_TransactionType; }
		}

		ZPropertyInfo IMatching.TransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionType)); }
		}

		ZGuid IMatching.Organisation
		{
			get { return AH_OH; }
		}

		ZPropertyInfo IMatching.OrganisationInfo
		{
			get { return GetZPropertyInfo("Organisation"); }
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

		[ReadOnlyMember(nameof(IsOSPartialPaymentAmountReadOnly))]
		ZDecimal IMatching.OSPartialPaymentAmount
		{
			get { return OSOutstandingAmountMatching; }
			set { ((IMatching)this).OSPartialPaymentAmountInfo.RefreshBinding(); }
		}

		ZPropertyInfo IMatching.OSPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo("OSPartialPaymentAmount"); }
		}

		public bool IsOSPartialPaymentAmountReadOnly { get; set; }

		ZDecimal IMatching.LocalPartialPaymentAmount
		{
			get { return AH_OutstandingAmount; }
		}

		ZPropertyInfo IMatching.LocalPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo("LocalPartialPaymentAmount"); }
		}

		[MaxLength(3)]
		ZString IMatching.CurrencyCode
		{
			get { return AH_RX_NKTransactionCurrency; }
		}

		ZPropertyInfo IMatching.PaymentCurrencyCodeInfo
		{
			get { return GetZPropertyInfo("CurrencyCode"); }
		}

		ZInt IMatching.CurrencyDecimals
		{
			get { return AH_Calc_RXDecimals; }
		}

		ZPropertyInfo IMatching.CurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo("CurrencyDecimals"); }
		}

		ZInt IMatching.LoginCompanyCurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		ZPropertyInfo IMatching.LoginCompanyCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo("LoginCompanyCurrencyDecimals"); }
		}

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

		ZString IMatching.VoyageVesselOrFlightDate { get { return ZString.Empty; } }
		ZString IMatching.ShipmentHouseBill { get { return ZString.Empty; } }
		ZString IMatching.ShipmentMasterBill { get { return ZString.Empty; } }

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal IMatching.NotionalWHTTax => AH_NotionalWHTTax;
		ZPropertyInfo IMatching.NotionalWHTTaxInfo => AH_NotionalWHTTaxInfo;
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal IMatching.RealizedWHTTax => AH_RealizedWHTTax;
		ZPropertyInfo IMatching.RealizedWTHTaxInfo => AH_RealizedWHTTaxInfo;
		#endregion

		#region IMiscellaneousTransaction Members

		MatchingBase IMiscellaneousTransaction.MatchingBizO
		{
			get { return fMatchingBizO; }
			set { fMatchingBizO = value; }
		}

		MatchingBase fMatchingBizO;

		SecurityCheckpoint IMiscellaneousTransaction.CheckpointForUnmatch => null;

		#endregion
	}
}
