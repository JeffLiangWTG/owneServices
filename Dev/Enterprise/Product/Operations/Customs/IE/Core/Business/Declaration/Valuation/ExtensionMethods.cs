using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration.IsExport, declaration.IsAir);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(
			invoice.IsExport,
			invoice.JobDeclaration?.IsAir ?? false
		);

		public static string GetIncoTermChargeFactoryCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(
			groupInvoice.JobDeclaration?.IsExport ?? false,
			groupInvoice.JobDeclaration?.IsAir ?? false
		);

		static string GetCustomsChargeTypeListCacheKeyCore(bool isExport, bool isAir) =>
			$"{(isExport ? IEJobMessageTypeList.Codes.Export : IEJobMessageTypeList.Codes.Import)}{(!isExport && isAir ? Core.Constants.TransportModes.Air : string.Empty)}";
	}
}
