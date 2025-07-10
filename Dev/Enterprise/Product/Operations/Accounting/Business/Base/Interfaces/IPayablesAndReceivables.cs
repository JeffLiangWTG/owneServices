
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IPayablesAndReceivables : IReversing, IMatching
	{
	}
}

#region Test
#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	public class TestIPayablesAndReceivables : TestIReverseTransaction, IPayablesAndReceivablesForTests
	{
		#region IMatching Members

		protected bool fIsMatched;
		public bool IsMatched
		{
			get { return fIsMatched; }
		}

		public void SetIsMatched(bool value)
		{
			fIsMatched = value;
		}

		bool IMatching.IsAllPaidInTheSameCurrency(ZString currencyNK)
		{
			return false;
		}

		ZDecimal IMatching.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK)
		{
			return 0;
		}

		ZString IMatching.RelatedDisbursementTransactions { get { return ZString.Empty; } }
		ZString IMatching.RelatedTransactionDebtorsAsString { get { return ZString.Empty; } }
		ZString IMatching.CreatingUser { get { return ZString.Empty; } }
		ZString IMatching.PaymentCriticality { get { return ZString.Empty; } }
		ZDateTime IMatching.PaymentRequestedDate { get { return ZDateTime.Empty; } }
		ZString IMatching.VoyageVesselOrFlightDate { get { return ZString.Empty; } }
		ZString IMatching.ShipmentHouseBill { get { return ZString.Empty; } }
		ZString IMatching.ShipmentMasterBill { get { return ZString.Empty; } }
		ZString IMatching.RelatedClaimStatus { get { return ZString.Empty; } }
		ZString IMatching.QueryNumber { get { return ZString.Empty; } }
		ZString IMatching.InvoiceRemittanceReference { get { return ZString.Empty; } }

		void IMatching.FullyPay(ZDateTime fullyPaidDate)
		{
			this.FullyPaidDate = fullyPaidDate;
		}

		void IMatching.PartiallyPay()
		{
		}

		void IMatching.GenerateMatchLinksCore()
		{
		}

		void IMatching.GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
		}

		UnmatchingResult IMatching.CanUnmatch(ZDecimal matchLinkAmount)
		{
			return UnmatchingResult.CanNotUnmatch;
		}

		void IMatching.Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
		}

		void IMatching.ChangeUnmatchDate(ZDateTime unmatchDate)
		{
		}

		TransactionMatchLinkGroup IMatching.CurrentMatchGroup
		{
			get
			{
				return fMatchLinksSetToReturn ?? (fMatchLinksSetToReturn = new TransactionMatchLinkGroup(Factory));
			}
		}

		public TransactionMatchLinkCollection Matchlinks
		{
			get
			{
				return new TransactionMatchLinkCollection(Factory);
			}
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

		ZPropertyInfo IMatching.LedgerInfo
		{
			get { return null; }
		}

		ZDecimal IMatching.OSOutstandingAmount
		{
			get { return 0.0M; }
		}

		ZPropertyInfo IMatching.OSOutstandingAmountInfo
		{
			get { return null; }
		}

		ZDecimal IMatching.OutstandingAmount
		{
			get { return 0.0M; }
		}

		ZPropertyInfo IMatching.OutstandingAmountInfo
		{
			get { return null; }
		}

		ZDecimal IMatching.OriginalOutstandingAmount
		{
			get { return 0.0M; }
		}

		ZGuid IMatching.Organisation
		{
			get { return CargoWise.Types.ZGuid.Empty; }
		}

		ZPropertyInfo IMatching.OrganisationInfo
		{
			get { return null; }
		}

		OrgHeaderCollection IMatching.Organisations
		{
			get { return null; }
		}

		ZString IMatching.TransactionNumber
		{
			get { return null; }
		}

		ZPropertyInfo IMatching.TransactionNumberInfo
		{
			get { return null; }
		}

		ZString IMatching.JobLocalReference
		{
			get { return null; }
		}

		ZPropertyInfo IMatching.JobLocalReferenceInfo
		{
			get { return null; }
		}

		ZDecimal IMatching.OSPartialPaymentAmount
		{
			get { return fOSPartialPaymentAmount; }
			set { fOSPartialPaymentAmount = value; }
		}
		ZDecimal fOSPartialPaymentAmount;

		ZPropertyInfo IMatching.OSPartialPaymentAmountInfo
		{
			get { return null; }
		}

		bool IMatching.OSPartialPaymentAmount_ReadOnly
		{
			get { return true; }
		}

		ZDecimal IMatching.LocalPartialPaymentAmount
		{
			get { return 0.0M; }
		}

		ZPropertyInfo IMatching.LocalPartialPaymentAmountInfo
		{
			get { return null; }
		}

		ZString IMatching.TransactionType
		{
			get { return null; }
		}

		ZPropertyInfo IMatching.TransactionTypeInfo
		{
			get { return null; }
		}

		ZString IMatching.TransactionCategory
		{
			get { return null; }
		}

		ZPropertyInfo IMatching.TransactionCategoryInfo
		{
			get { return null; }
		}

		ZGuid IMatching.BranchGuid
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IMatching.DepartmentGuid
		{
			get { return ZGuid.Empty; }
		}

		ZString IMatching.CurrencyCode
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.PaymentCurrencyCodeInfo
		{
			get { return null; }
		}

		ZInt IMatching.CurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		ZPropertyInfo IMatching.CurrencyDecimalsInfo
		{
			get { return null; }
		}

		ZInt IMatching.LoginCompanyCurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		ZPropertyInfo IMatching.LoginCompanyCurrencyDecimalsInfo
		{
			get { return null; }
		}

		ZString IMatching.Description
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.DescriptionInfo
		{
			get { return null; }
		}

		ZDateTime IMatching.PostDate
		{
			get { return ZDateTime.Empty; }
			set { }
		}

		ZPropertyInfo IMatching.PostDateInfo
		{
			get { return null; }
		}

		bool IMatching.PostDate_ReadOnly
		{
			get { return true; }
		}

		ZString IMatching.ChequeOrReference
		{
			get { return fChequeOrReference; }
			set { fChequeOrReference = value; }
		}
		ZString fChequeOrReference;

		ZPropertyInfo IMatching.ChequeOrReferenceInfo
		{
			get { return null; }
		}

		bool IMatching.ChequeOrReference_ReadOnly { get; set; }

		ZDecimal IMatching.ExchangeRateAmount
		{
			get { return 0.0M; }
		}

		ZPropertyInfo IMatching.ExchangeRateAmountInfo
		{
			get { return null; }
		}

		ZString IMatching.TransactionReference
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.TransactionReferenceInfo
		{
			get { return null; }
		}

		ZString IMatching.ConsolidatedRef
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.ConsolidatedRefInfo
		{
			get { return null; }
		}

		ZString IMatching.InvoiceBatchNumber
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.InvoiceBatchNumberInfo
		{
			get { return null; }
		}

		ZDateTime IMatching.InvoiceDate
		{
			get { return ZDateTime.Empty; }
		}

		ZPropertyInfo IMatching.InvoiceDateInfo
		{
			get { return null; }
		}

		ZDateTime IMatching.DueDate
		{
			get { return ZDateTime.Empty; }
		}

		ZPropertyInfo IMatching.DueDateInfo
		{
			get { return null; }
		}

		ZString IMatching.MatchStatus { get; set; }

		ZPropertyInfo IMatching.MatchStatusInfo
		{
			get { return null; }
		}

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusList { get { return null; } }

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusReasonCodeList { get { return null; } }

		ZString IMatching.MatchStatusReasonCode { get; set; }

		ZPropertyInfo IMatching.MatchStatusReasonCodeInfo
		{
			get { return null; }
		}

		ZDateTime IMatching.MatchDate
		{
			get { return ZDateTime.Empty; }
		}

		ZPropertyInfo IMatching.MatchDateInfo
		{
			get { return null; }
		}

		ZPropertyInfo IMatching.BranchGuidInfo
		{
			get { return null; }
		}

		GlbBranchCollection IMatching.BranchCollection
		{
			get { return null; }
		}

		ZPropertyInfo IMatching.DepartmentGuidInfo
		{
			get { return null; }
		}

		GlbDepartmentCollection IMatching.DepartmentCollection
		{
			get { return null; }
		}

		RefCurrencyCollection IMatching.Currencies
		{
			get { return null; }
		}

		TransactionMatchLinkGroup fMatchLinksSetToReturn;
		public void SetMatchLinksToBeGenerated(TransactionMatchLinkGroup matchLinksToSet)
		{
			fMatchLinksSetToReturn = matchLinksToSet;
		}

		ZGuid IMatching.DisplayInvoiceAddressOverride { get { return ZGuid.Empty; } }
		ZPropertyInfo IMatching.DisplayInvoiceAddressOverrideInfo { get { return null; } }

		ZDecimal IMatching.NotionalWHTTax => 0M;
		ZPropertyInfo IMatching.NotionalWHTTaxInfo => null;
		ZDecimal IMatching.RealizedWHTTax => 0M;
		ZPropertyInfo IMatching.RealizedWTHTaxInfo => null;

		ZString IMatching.InvoiceTransactionReference => ZString.Empty;

		#endregion
	}

	public interface IPayablesAndReceivablesForTests : IPayablesAndReceivables, IReversingForTests
	{
		void SetIsMatched(bool value);
		void SetMatchLinksToBeGenerated(TransactionMatchLinkGroup matchLinksToSet);
	}
}
#endif
#endregion
