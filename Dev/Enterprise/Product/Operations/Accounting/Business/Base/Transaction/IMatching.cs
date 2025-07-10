using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public enum UnmatchingResult
	{
		Success,
		ContainsPayment,
		ContainsCashAdvanceARJournal,
		ContainsCashAdvanceAPJournal,
		CanNotUnmatch,

		DataError = 10,
		DataErrorInvoiceAmountAndMatchLinkAmountHasOppositeSigns,
		DataErrorAddingMatchAmountExceedOriginalInvoiceAmount,
		DataErrorAddingMatchAmountDecreaseAbsValueOfOutstandingAmount,
		DataErrorTransactionOrganisationIsInactive
	}

	[PropertyDescriptorCollection(typeof(BusinessObjectPropertyDescriptorCollection))]
	public interface IMatching : IBusiness
	{
		bool IsMatched { get; }
		bool IsAllPaidInTheSameCurrency(ZString currencyNK);
		ZDecimal CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK);
		void FullyPay(ZDateTime fullyPaidDate);
		void PartiallyPay();
		void GenerateMatchLinksCore();
		void GeneratePaymentApprovalItems(PaymentApprovalBase approval);
		UnmatchingResult CanUnmatch(ZDecimal matchLinkAmount);
		void Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount);
		void ChangeUnmatchDate(ZDateTime unmatchDate);
		TransactionMatchLinkGroup CurrentMatchGroup { get; }
		TransactionMatchLinkCollection Matchlinks { get; }
		PaymentApprovalItemCollection PaymentApprovalItems { get; }
		ZDecimal OSOutstandingAmount { get; }
		ZPropertyInfo OSOutstandingAmountInfo { get; }
		ZDecimal OutstandingAmount { get; }
		ZPropertyInfo OutstandingAmountInfo { get; }
		ZDecimal OriginalOutstandingAmount { get; }
		ZString TransactionType { get; }
		ZPropertyInfo TransactionTypeInfo { get; }
		[List("DisplayInvoiceAddressOverrides")]
		ZGuid DisplayInvoiceAddressOverride { get; }
		ZPropertyInfo DisplayInvoiceAddressOverrideInfo { get; }
		ZString TransactionCategory { get; }
		ZPropertyInfo TransactionCategoryInfo { get; }
		[List("Organisations")]
		ZGuid Organisation { get; }
		ZPropertyInfo OrganisationInfo { get; }
		[List("BranchCollection")]
		ZGuid BranchGuid { get; }
		ZPropertyInfo BranchGuidInfo { get; }
		[List("DepartmentCollection")]
		ZGuid DepartmentGuid { get; }
		ZPropertyInfo DepartmentGuidInfo { get; }
		ZString Ledger { get; }
		ZPropertyInfo LedgerInfo { get; }
		ZString TransactionNumber { get; }
		ZPropertyInfo TransactionNumberInfo { get; }
		ZString RelatedDisbursementTransactions { get; }
		ZString JobLocalReference { get; }
		ZPropertyInfo JobLocalReferenceInfo { get; }
		ZDecimal OSPartialPaymentAmount { get; set; }
		ZPropertyInfo OSPartialPaymentAmountInfo { get; }
		bool OSPartialPaymentAmount_ReadOnly { get; }
		ZDecimal LocalPartialPaymentAmount { get; }
		ZPropertyInfo LocalPartialPaymentAmountInfo { get; }
		[List("Currencies")]
		ZString CurrencyCode { get; }
		ZInt CurrencyDecimals { get; }
		ZPropertyInfo CurrencyDecimalsInfo { get; }
		ZInt LoginCompanyCurrencyDecimals { get; }
		ZPropertyInfo LoginCompanyCurrencyDecimalsInfo { get; }
		ZPropertyInfo PaymentCurrencyCodeInfo { get; }
		ZString Description { get; }
		ZPropertyInfo DescriptionInfo { get; }
		ZDateTime PostDate { get; set; }
		ZPropertyInfo PostDateInfo { get; }
		bool PostDate_ReadOnly { get; }
		ZString ChequeOrReference { get; set; }
		ZPropertyInfo ChequeOrReferenceInfo { get; }
		bool ChequeOrReference_ReadOnly { get; set; }
		ZDecimal ExchangeRateAmount { get; }
		ZPropertyInfo ExchangeRateAmountInfo { get; }
		ZString TransactionReference { get; }
		ZPropertyInfo TransactionReferenceInfo { get; }
		ZString ConsolidatedRef { get; }
		ZPropertyInfo ConsolidatedRefInfo { get; }
		ZString InvoiceBatchNumber { get; }
		ZPropertyInfo InvoiceBatchNumberInfo { get; }
		ZDateTime InvoiceDate { get; }
		ZPropertyInfo InvoiceDateInfo { get; }
		ZDateTime DueDate { get; }
		ZPropertyInfo DueDateInfo { get; }
		ZDateTime MatchDate { get; }
		ZPropertyInfo MatchDateInfo { get; }
		OrgHeaderCollection Organisations { get; }
		GlbBranchCollection BranchCollection { get; }
		GlbDepartmentCollection DepartmentCollection { get; }
		RefCurrencyCollection Currencies { get; }
		ZString RelatedTransactionDebtorsAsString { get; }
		ZString CreatingUser { get; }
		ZString PaymentCriticality { get; }
		ZDateTime PaymentRequestedDate { get; }
		ZString VoyageVesselOrFlightDate { get; }
		ZString ShipmentHouseBill { get; }
		ZString ShipmentMasterBill { get; }
		ZString RelatedClaimStatus { get; }
		ZString QueryNumber { get; }
		ZString InvoiceRemittanceReference { get; }
		[MaxLength(3)]
		[List("MatchStatusList")]
		ZString MatchStatus { get; set; }
		ZPropertyInfo MatchStatusInfo { get; }
		[MaxLength(3)]
		[List("MatchStatusReasonCodeList")]
		ZString MatchStatusReasonCode { get; set; }
		ReadOnlyCodeDescriptionPairList MatchStatusList { get; }
		ZPropertyInfo MatchStatusReasonCodeInfo { get; }
		ReadOnlyCodeDescriptionPairList MatchStatusReasonCodeList { get; }
		ZDecimal NotionalWHTTax { get; }
		ZPropertyInfo NotionalWHTTaxInfo { get; }
		ZDecimal RealizedWHTTax { get; }
		ZPropertyInfo RealizedWTHTaxInfo { get; }
		ZString InvoiceTransactionReference { get; }
	}

	public static class IMatchingExtensions
	{
		public static void GenerateMatchLinks(this IMatching matcher)
		{
			BusinessObject matcherAsBO = matcher as BusinessObject;
			if (matcherAsBO != null)
			{
				matcher.GenerateMatchLinksCore();
				matcherAsBO.RegisterEditableChildObject(matcher.CurrentMatchGroup);
			}
		}

		public static string GetIMatchingInfo(this IMatching header)
		{
			var result = new ZStringBuilder();
			var headerInfo = Invariant($"Ledger: {header.Ledger}, Transaction type: {header.TransactionType}, Payment Amount: {header.LocalPartialPaymentAmount}, Outstanding Amount: {header.OutstandingAmount}, OS Payment Amount: {header.OSPartialPaymentAmount}, OS Outstanding Amount: {header.OSOutstandingAmount}, Currency: {header.CurrencyCode}, Exchange Rate Amount: {header.ExchangeRateAmount}");
			result.Append(headerInfo);

			if (header.OSPartialPaymentAmount > header.OSOutstandingAmount && header is InvoicingBase invoice)
			{
				foreach (InvoicingLineBase line in invoice.Lines)
				{
					result.Append(System.Environment.NewLine + "\t" + line.GetTransactionLineInfo());
				}
			}

			return result.ToString();
		}
	}
}
