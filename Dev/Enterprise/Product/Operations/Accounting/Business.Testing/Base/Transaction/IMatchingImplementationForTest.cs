using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	class IMatchingImplementationForTest : DummyBusinessObject, IMatching
	{
		public IMatchingImplementationForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IMatching Implementation

		bool IMatching.ChequeOrReference_ReadOnly { get; set; }

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
		public TransactionMatchLinkCollection Matchlinks { get; set; }
		public TransactionMatchLinkGroup CurrentMatchGroup { get; set; }
		void IMatching.GenerateMatchLinksCore() { GenerateMatchLinksCoreCallCount++; }
		public int GenerateMatchLinksCoreCallCount;

		bool IMatching.IsMatched { get { return false; } }
		void IMatching.FullyPay(ZDateTime fullyPaidDate) { }
		void IMatching.PartiallyPay() { }
		void IMatching.GeneratePaymentApprovalItems(PaymentApprovalBase approval) { }
		UnmatchingResult IMatching.CanUnmatch(ZDecimal matchLinkAmount) { return UnmatchingResult.CanNotUnmatch; }
		void IMatching.Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount) { }
		void IMatching.ChangeUnmatchDate(ZDateTime unmatchDate) { }
		PaymentApprovalItemCollection IMatching.PaymentApprovalItems { get { return null; } }
		ZDecimal IMatching.OSOutstandingAmount { get { return 0; } }
		ZPropertyInfo IMatching.OSOutstandingAmountInfo { get { return null; } }
		ZDecimal IMatching.OutstandingAmount { get { return ZDecimal.Zero; } }
		ZPropertyInfo IMatching.OutstandingAmountInfo { get { return null; } }
		ZDecimal IMatching.OriginalOutstandingAmount { get { return ZDecimal.Zero; } }
		ZString IMatching.TransactionType { get { return null; } }
		ZPropertyInfo IMatching.TransactionTypeInfo { get { return null; } }
		ZString IMatching.TransactionCategory { get { return null; } }
		ZPropertyInfo IMatching.TransactionCategoryInfo { get { return null; } }
		ZGuid IMatching.Organisation { get { return ZGuid.Empty; } }
		ZPropertyInfo IMatching.OrganisationInfo { get { return null; } }
		ZGuid IMatching.BranchGuid { get { return ZGuid.Empty; } }
		ZPropertyInfo IMatching.BranchGuidInfo { get { return null; } }
		ZGuid IMatching.DepartmentGuid { get { return ZGuid.Empty; } }
		ZPropertyInfo IMatching.DepartmentGuidInfo { get { return null; } }
		ZString IMatching.Ledger { get { return null; } }
		ZPropertyInfo IMatching.LedgerInfo { get { return null; } }
		ZString IMatching.TransactionNumber { get { return null; } }
		ZPropertyInfo IMatching.TransactionNumberInfo { get { return null; } }
		ZString IMatching.JobLocalReference { get { return null; } }
		ZPropertyInfo IMatching.JobLocalReferenceInfo { get { return null; } }
		ZDecimal IMatching.OSPartialPaymentAmount { get; set; }
		bool IMatching.OSPartialPaymentAmount_ReadOnly { get { return false; } }
		ZPropertyInfo IMatching.OSPartialPaymentAmountInfo { get { return null; } }
		ZDecimal IMatching.LocalPartialPaymentAmount { get { return ZDecimal.Zero; } }
		ZPropertyInfo IMatching.LocalPartialPaymentAmountInfo { get { return null; } }
		ZString IMatching.CurrencyCode { get { return ZString.Empty; } }
		ZPropertyInfo IMatching.PaymentCurrencyCodeInfo { get { return null; } }
		ZInt IMatching.CurrencyDecimals { get { return 2; } }
		ZPropertyInfo IMatching.CurrencyDecimalsInfo { get { return null; } }
		ZInt IMatching.LoginCompanyCurrencyDecimals { get { return 2; } }
		ZPropertyInfo IMatching.LoginCompanyCurrencyDecimalsInfo { get { return null; } }
		ZString IMatching.Description { get { return null; } }
		ZPropertyInfo IMatching.DescriptionInfo { get { return null; } }
		ZDateTime IMatching.PostDate { get; set; }
		ZPropertyInfo IMatching.PostDateInfo { get { return null; } }
		bool IMatching.PostDate_ReadOnly { get { return false; } }
		ZString IMatching.ChequeOrReference { get; set; }
		ZPropertyInfo IMatching.ChequeOrReferenceInfo { get { return null; } }
		ZDecimal IMatching.ExchangeRateAmount { get { return ZDecimal.Zero; } }
		ZPropertyInfo IMatching.ExchangeRateAmountInfo { get { return null; } }
		ZString IMatching.TransactionReference { get { return null; } }
		ZPropertyInfo IMatching.TransactionReferenceInfo { get { return null; } }
		ZString IMatching.ConsolidatedRef { get { return null; } }
		ZPropertyInfo IMatching.ConsolidatedRefInfo { get { return null; } }
		ZString IMatching.InvoiceBatchNumber { get { return null; } }
		ZPropertyInfo IMatching.InvoiceBatchNumberInfo { get { return null; } }
		ZDateTime IMatching.InvoiceDate { get { return ZDateTime.Now; } }
		ZPropertyInfo IMatching.InvoiceDateInfo { get { return null; } }
		ZDateTime IMatching.DueDate { get { return ZDateTime.Now; } }
		ZPropertyInfo IMatching.DueDateInfo { get { return null; } }
		ZString IMatching.MatchStatus { get; set; }
		ZPropertyInfo IMatching.MatchStatusInfo { get { return null; } }
		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusList { get { return null; } }
		ZString IMatching.MatchStatusReasonCode { get; set; }
		ZPropertyInfo IMatching.MatchStatusReasonCodeInfo { get { return null; } }
		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusReasonCodeList { get { return null; } }
		ZDateTime IMatching.MatchDate { get { return ZDateTime.Now; } }
		ZPropertyInfo IMatching.MatchDateInfo { get { return null; } }
		OrgHeaderCollection IMatching.Organisations { get { return null; } }
		GlbBranchCollection IMatching.BranchCollection { get { return null; } }
		GlbDepartmentCollection IMatching.DepartmentCollection { get { return null; } }
		RefCurrencyCollection IMatching.Currencies { get { return null; } }

		ZGuid IMatching.DisplayInvoiceAddressOverride { get { return ZGuid.Empty; } }
		ZPropertyInfo IMatching.DisplayInvoiceAddressOverrideInfo { get { return null; } }

		ZDecimal IMatching.NotionalWHTTax => 0M;
		ZPropertyInfo IMatching.NotionalWHTTaxInfo => null;
		ZDecimal IMatching.RealizedWHTTax => 0M;
		ZPropertyInfo IMatching.RealizedWTHTaxInfo => null;
		ZString IMatching.InvoiceTransactionReference => null;
		#endregion
	}
}
