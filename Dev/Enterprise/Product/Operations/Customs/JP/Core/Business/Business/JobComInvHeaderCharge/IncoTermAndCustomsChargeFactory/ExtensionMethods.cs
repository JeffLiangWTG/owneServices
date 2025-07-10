namespace Enterprise.Customs.JP.Business
{
	public static class ExtensionMethods
	{
		public static string GetCountryContext(this JobDeclaration declaration) => GetCountryContextCore(declaration.IsExport);

		public static string GetCountryContext(this JobComInvoiceHeader invoice) => GetCountryContextCore(invoice.IsExport);

		public static string GetCountryContext(this JobComInvoiceGroupHeader groupInvoice) => GetCountryContextCore(groupInvoice.JobDeclaration?.IsExport ?? false);

		static string GetCountryContextCore(bool isExport) => string.Concat(Core.Constants.CountryCodes.Japan, isExport ? Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export : Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import);
	}
}
