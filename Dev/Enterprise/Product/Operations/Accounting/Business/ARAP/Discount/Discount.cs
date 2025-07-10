using System.ComponentModel;
using System.Data;
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
	public abstract partial class Discount : TransactionHeader, IMatching, IMiscellaneousTransaction
	{
		public Discount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateInfo, BindableInvoiceAmountInfo, BindableOSAmountInfo, ExchangeRate as ZAccExchangeRate);
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_Desc = Enterprise.Accounting.Business.AccountingConstants.MatchingDefaultDesc.DiscountDesc;
		}

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			base.GenerateReverseTransactionCore(mustTransform);
			using (fReverseTransaction.AmountsCalculationsSuspender.GetSuspender())
			{
				fReverseTransaction.AH_OSExTaxAmount = -AH_OSExTaxAmount;
			}
		}

		[ReadOnly(true)]
		public override ZGuid AH_AG
		{
			get { return base.AH_AG; }
			set { base.AH_AG = value; }
		}

		#endregion

		#region Required To Implement Interface

		protected override ZString TransactionType
		{
			get { return Enterprise.ZArchitecture.Core.TransactionTypes.Discount; }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		#endregion

		#region IMatching Members

		ZString IMatching.RelatedTransactionDebtorsAsString { get { return ZString.Empty; } }
		ZString IMatching.CreatingUser { get { return ZString.Empty; } }
		ZString IMatching.PaymentCriticality { get { return ZString.Empty; } }
		ZString IMatching.RelatedDisbursementTransactions { get { return ZString.Empty; } }
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
			// set fields for this Discount
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

			revTrans.SetDescription(string.Format(AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.UnDiscountRelated,
									   Res.GetString("9839fa88-40cd-4aa6-bfb9-25115f492f92", "DISCOUNT RELATING TO UN_MATCH NO")) + " {0}", originalLink.AP_MatchGroupNum));
			revTrans.SetNumberOfSupportingDocuments(AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.UnDiscountRelated, 0));

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
			get { return OutstandingAmountMatching; }
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
