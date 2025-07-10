using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration.IsImport);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.IsImport);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.IsImport ?? false);

		static string GetCustomsChargeTypeListCacheKeyCore(bool isImport) => isImport ? "IMP" : "";

		static IEnumerable<JobComInvCharge> FindRoyaltyCharges(this IChargeHolder chargeHolder) => chargeHolder.Charges.Cast<JobComInvCharge>().Where(charge => charge.J7_ChargeType == CustomsChargeTypeList.Codes.Royalty);

		public static IEnumerable<JobComInvCharge> FindRoyaltyChargesOnInvoiceOrGroup(this JobComInvoiceHeader invoice)
		{
			var charges = invoice.FindRoyaltyCharges();
			var invoiceGroup = invoice.GroupHeader;
			if (invoiceGroup != null)
			{
				charges = charges.Union(invoiceGroup.FindRoyaltyCharges());
			}
			return charges;
		}
	}
}
