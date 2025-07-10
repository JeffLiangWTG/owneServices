namespace Enterprise.Customs.FR.Business.Declaration
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration.IsImport);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.IsImport);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.IsImport ?? false);

		static string GetCustomsChargeTypeListCacheKeyCore(bool isImport) => isImport ? string.Empty : Common.Shared.SharedJobMessageTypeList.Codes.Export;
	}
}
