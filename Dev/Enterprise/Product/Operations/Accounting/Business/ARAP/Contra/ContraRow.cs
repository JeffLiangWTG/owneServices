using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP
{
	[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
	public abstract partial class ContraRow : TransactionHeader, IPayablesAndReceivables
	{
		#region Schema

		public new abstract class Schema : TransactionHeader.Schema
		{
			public const string AH_BeforeContra = "AH_BeforeContra";
			public const string AH_AfterContra = "AH_AfterContra";
		}

		#endregion

		public ContraRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Contra ParentContra;

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_GE = GlbDepartment.CurrentDepartment.PK;
			AH_GB = GlbBranch.CurrentBranch.PK;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (ParentContra != null && saveSucceeded)
			{
				ParentContra.AH_TransactionNumInfo.RefreshBinding();
				ParentContra.ReadOnly = true;
			}
		}

		#endregion

		protected override ZString TransactionType
		{
			get { return ZArchitecture.Core.TransactionTypes.Contra; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		protected override TransactionHeaderNumberFountainUniqueIndexFailureHandler GetNewUniqueIndexFailureHandler()
		{
			return new TransactionHeaderNumberFountainUniqueIndexFailureHandlerForContra(this.ParentContra, this);
		}

		class TransactionHeaderNumberFountainUniqueIndexFailureHandlerForContra : TransactionHeaderNumberFountainUniqueIndexFailureHandler
		{
			public TransactionHeaderNumberFountainUniqueIndexFailureHandlerForContra(Contra contra, TransactionHeader header)
				: base(header)
			{
				this.contra = contra;
			}
			readonly Contra contra;

			protected override AccountingNumberFountainWrapper AccountingNumberFountainToFix
			{
				get
				{
					return contra.NumberFountain;
				}
			}
		}

		#region Properties

		#region SetOutstandingLocalAmountAfterLocalExTaxAmountSet

		protected override void SetOutstandingLocalAmountAfterLocalExTaxAmountSet()
		{
			SetOutstandingLocalAmountAfterLocalExTaxAmountSetCore();
		}

		#endregion

		#region AH_BeforeContra

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_BeforeContra
		{
			get
			{
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.NotEqual, true);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.Equal, AH_OH);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, AH_Ledger);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
				filter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.Equal, AH_GC);

				TransactionHeader[] transactions = (TransactionHeader[])Factory.Load(typeof(TransactionHeader), filter);
				decimal beforeContra = 0;
				foreach (TransactionHeader header in transactions)
				{
					beforeContra += header.AH_OutstandingAmount;
				}

				return new ZDecimal(-beforeContra);
			}
		}

		public ZPropertyInfo AH_BeforeContraInfo
		{
			get { return GetZPropertyInfo(Schema.AH_BeforeContra); }
		}

		#endregion

		#region AH_AfterContra

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_AfterContra
		{
			get { return fAH_AfterContra; }
			set { SetNonPersistentPropertyValue(AH_AfterContraInfo, ref fAH_AfterContra, value); }
		}

		ZDecimal fAH_AfterContra;

		public ZPropertyInfo AH_AfterContraInfo
		{
			get { return GetZPropertyInfo(Schema.AH_AfterContra); }
		}

		#endregion

		#region SetAfterContraValues

		public void SetAfterContraValues()
		{
			AH_AfterContra = AH_BeforeContra - AH_LocalExTaxAmount;
		}

		#endregion

		#region AH_InvoiceDate

		public override ZDateTime AH_InvoiceDate
		{
			get { return base.AH_InvoiceDate; }
			set
			{
				base.AH_InvoiceDate = value;
				AH_DueDate = value;
			}
		}

		#endregion

		#endregion

		#region Unmatching Members

		#region RelatedTransactions

		public override TransactionHeaderCollection RelatedTransactions
		{
			get
			{
				if (fRelatedTransactions == null)
				{
					ZQuery contraRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Contra);
					contraRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, AH_TransactionNum);
					contraRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
					fRelatedTransactions = new TransactionHeaderCollection(Factory, contraRowFilter);
					fRelatedTransactions.Load();
				}

				return fRelatedTransactions;
			}
		}

		#endregion

		#region AreRelatedTransactionsCreatedByMatching

		public override ZBool AreRelatedTransactionsCreatedByMatching
		{
			get
			{
				ZBool createdByMatching = true;
				if (AH_TransactionCreatedByMatching)
				{
					foreach (TransactionHeader header in RelatedTransactions)
					{
						if (!header.AH_TransactionCreatedByMatching)
						{
							createdByMatching = false;
						}
					}
				}
				else
				{
					createdByMatching = false;
				}

				return createdByMatching;
			}
		}

		#endregion

		#endregion

		#region IMatching Members

		ZString IMatching.RelatedDisbursementTransactions { get { return ZString.Empty; } }
		ZString IMatching.RelatedTransactionDebtorsAsString { get { return ZString.Empty; } }
		ZString IMatching.CreatingUser { get { return ZString.Empty; } }
		ZString IMatching.PaymentCriticality { get { return ZString.Empty; } }
		ZDateTime IMatching.PaymentRequestedDate { get { return ZDateTime.Empty; } }

		bool IMatching.IsMatched
		{
			get { return AH_LocalTotalAmount != AH_LocalOutstandingAmount; }
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

		ZDecimal IMatching.OSPartialPaymentAmount
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
			}
		}

		ZDecimal fOSPartialPaymentAmount;

		ZPropertyInfo IMatching.OSPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo("OSPartialPaymentAmount"); }
		}

		ZDecimal IMatching.LocalPartialPaymentAmount
		{
			get
			{
				ZDecimal tempLocalAmount = AH_OutstandingAmount;
				if (((IMatching)this).OSPartialPaymentAmount != OSOutstandingAmountMatching)
				{
					tempLocalAmount = (ZDecimal)Env.CurrentCompany.ExchangeRate.ForeignToLocal(((IMatching)this).OSPartialPaymentAmount, AH_ExchangeRate);
				}
				return tempLocalAmount;
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
			get { return GetZPropertyInfo(nameof(TransactionType)); }
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

		bool IMatching.PostDate_ReadOnly
		{
			get { return true; }
		}

		ZPropertyInfo IMatching.PostDateInfo
		{
			get { return GetZPropertyInfo("PostDate"); }
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
				if (LatestMatchLink != null)
				{
					return LatestMatchLink.AP_MatchDate;
				}
				else
				{
					return ZDateTime.Empty;
				}
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
	}
}
