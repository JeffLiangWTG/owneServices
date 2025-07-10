using System;
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
	public abstract partial class TransferRow : TransactionHeader, IPayablesAndReceivables
	{
		public new abstract class Schema : TransactionHeader.Schema
		{
			public const string AH_BeforeTransfer = "AH_BeforeTransfer";
			public const string AH_AfterTransfer = "AH_AfterTransfer";
		}

		public TransferRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_GE = GlbDepartment.CurrentDepartment.PK;
			AH_GB = GlbBranch.CurrentBranch.PK;
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();

			AH_PostToGL = "Y";
		}

		#region Transaction Property Overrides

		protected override ZString TransactionType
		{
			get { return ZArchitecture.Core.TransactionTypes.Transfer; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		#endregion

		protected override TransactionHeaderNumberFountainUniqueIndexFailureHandler GetNewUniqueIndexFailureHandler()
		{
			return new TransactionHeaderNumberFountainUniqueIndexFailureHandlerForTransfer(this.ParentTransfer, this);
		}

		class TransactionHeaderNumberFountainUniqueIndexFailureHandlerForTransfer : TransactionHeaderNumberFountainUniqueIndexFailureHandler
		{
			public TransactionHeaderNumberFountainUniqueIndexFailureHandlerForTransfer(Transfer transfer, TransactionHeader header)
				: base(header)
			{
				this.transfer = transfer;
			}
			readonly Transfer transfer;

			protected override AccountingNumberFountainWrapper AccountingNumberFountainToFix
			{
				get { return transfer.TransferNumberFountain; }
			}
		}

		internal Transfer ParentTransfer { get; set; }

		#region SetOutstandingLocalAmountAfterLocalExTaxAmountSet

		protected override void SetOutstandingLocalAmountAfterLocalExTaxAmountSet()
		{
			SetOutstandingLocalAmountAfterLocalExTaxAmountSetCore();
		}

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
			get
			{
				if (fCurrentMatchGroup == null)
				{
					fCurrentMatchGroup = new TransactionMatchLinkGroup(Factory);
				}

				return fCurrentMatchGroup;
			}
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
				ZDecimal localAmountTemp = AH_OutstandingAmount;
				if (((IMatching)this).OSPartialPaymentAmount != OSOutstandingAmountMatching)
				{
					localAmountTemp = (ZDecimal)Env.CurrentCompany.ExchangeRate.ForeignToLocal(((IMatching)this).OSPartialPaymentAmount, AH_ExchangeRate);
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

		public ZPropertyInfo ChequeOrReferenceInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ChequeOrReference), x => AH_ChequeOrReferenceInfo); }
		}

		ZPropertyInfo IMatching.ChequeOrReferenceInfo
		{
			get { return ChequeOrReferenceInfo; }
		}

		bool IMatching.ChequeOrReference_ReadOnly { get; set; }

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

		#region Unmatching Members

		#region RelatedTransactions

		public override TransactionHeaderCollection RelatedTransactions
		{
			get
			{
				if (fRelatedTransactions == null)
				{
					ZQuery transferRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, AH_Ledger);
					transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
					transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, AH_TransactionNum);
					transferRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
					fRelatedTransactions = new TransactionHeaderCollection(Factory, transferRowFilter);
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

		protected abstract Type TypeOfTransferWrapper { get; }

		#endregion
	}
}
