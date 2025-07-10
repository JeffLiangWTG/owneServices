namespace Enterprise.Customs.BR.Business
{
	public static class ExtensionMethods
	{
		public static string GetIncoTermChargeFactoryCacheKey(this JobDeclaration declaration) => GetCustomsChargeTypeListCacheKeyCore(declaration);

		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceGroupHeader groupInvoice) => GetCustomsChargeTypeListCacheKeyCore(groupInvoice.JobDeclaration);

		public static string GetCustomsChargeTypeListCacheKey(this JobComInvoiceHeader invoice) => GetCustomsChargeTypeListCacheKeyCore(invoice.JobDeclaration, invoice.JZ_MessageType, invoice.IsImport, invoice.IsExport);

		static string GetCustomsChargeTypeListCacheKeyCore(JobDeclaration declaration, string messageType = "", bool isImport = false, bool isExport = false)
		{
			return GetCustomsChargeTypeListCacheKeyCore(declaration?.JE_MessageType ?? messageType, declaration?.IsImport ?? isImport, declaration?.IsExport ?? isExport);
		}

		static string GetCustomsChargeTypeListCacheKeyCore(string messageType, bool isImport, bool isExport)
		{
			if (messageType == Common.BR.BRJobMessageTypeList.Codes.ImportLicense)
			{
				return messageType;
			}
			else if (isImport)
			{
				return Common.BR.BRJobMessageTypeList.Codes.Import;
			}
			else if (isExport)
			{
				return Common.BR.BRJobMessageTypeList.Codes.Export;
			}
			return string.Empty;
		}
	}
}
