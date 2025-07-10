using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration.IsImport);

		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.IsImport);

		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.IsImport ?? false);

		static string GetCustomsChargeTypeListCacheKeyCore(bool isImport) => isImport ? "IMP" : "";
	}
}
