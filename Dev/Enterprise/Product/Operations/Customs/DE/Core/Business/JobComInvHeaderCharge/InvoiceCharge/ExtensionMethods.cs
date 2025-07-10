using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration.IsImport);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.IsImport);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.IsImport ?? false);

		public static bool HasChargesWithCode(this JobComInvoiceGroupHeader groupInvoice, ZString chargeCode) => groupInvoice.Charges.Cast<GroupInvoiceCharge>().Any(x => x.J7_ChargeType == chargeCode);

		public static bool HasChargesWithCode(this JobComInvoiceHeader invoice, ZString chargeCode) => invoice.Charges.Cast<InvoiceCharge>().Any(x => x.J7_ChargeType == chargeCode);

		public static bool HasChargesWithCode(this JobComInvoiceLine invoiceLine, ZString chargeCode) => invoiceLine.Charges.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == chargeCode);

		public static bool HasChargeCodeWithDifferentCurrency(this JobComInvoiceGroupHeader groupInvoice, ZString chargeCode, ZString currency)
			=> !chargeCode.IsEmpty && !currency.IsEmpty && groupInvoice.Charges.Cast<GroupInvoiceCharge>().Any(x => x.J7_ChargeType == chargeCode && !x.J7_RX_NKCurrency.IsEmpty && x.J7_RX_NKCurrency != currency);

		public static bool HasChargeCodeWithDifferentCurrency(this JobComInvoiceHeader invoice, ZString chargeCode, ZString currency)
			=> !chargeCode.IsEmpty && !currency.IsEmpty && invoice.Charges.Cast<InvoiceCharge>().Any(x => x.J7_ChargeType == chargeCode && !x.J7_RX_NKCurrency.IsEmpty && x.J7_RX_NKCurrency != currency);

		public static bool HasChargeCodeWithDifferentCurrency(this JobComInvoiceLine invoiceLine, ZString chargeCode, ZString currency)
			=> !chargeCode.IsEmpty && !currency.IsEmpty && invoiceLine.Charges.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == chargeCode && !x.J7_RX_NKCurrency.IsEmpty && x.J7_RX_NKCurrency != currency);

		public static bool SupportsIATA(this ZString chargeCode) => chargeCode == ImportChargeCodeList.Codes._010 || chargeCode == ImportChargeCodeList.Codes._011 || chargeCode == ImportChargeCodeList.Codes._014 || chargeCode == ImportChargeCodeList.Codes.AIR;

		static string GetCustomsChargeTypeListCacheKeyCore(bool isImport) => isImport ? Common.Shared.SharedJobMessageTypeList.Codes.Import : string.Empty;
	}
}
