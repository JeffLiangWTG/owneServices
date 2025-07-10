namespace Enterprise.Customs.IN.Business;

public static class ExtensionMethods
{
	public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration?.IsExport ?? false);

	public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration?.IsExport ?? false);

	public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice?.IsExport ?? false);

	static string GetCustomsChargeTypeListCacheKeyCore(bool isExport) => isExport ? Common.Shared.SharedJobMessageTypeList.Codes.Export : "";
}
