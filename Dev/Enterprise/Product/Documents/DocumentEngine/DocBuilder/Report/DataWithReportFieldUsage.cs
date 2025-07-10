namespace Enterprise.DocumentEngine.DocBuilder
{
	public class DataWithReportFieldUsage<T> : DataWithDocumentMacroUsage<T>
	{
		public DataWithReportFieldUsage(T data)
			: base(data)
		{
		}

		public override DocumentMacroUsageCollection InitializeDocumentMacroUsageCollection()
		{
			return new ReportFieldUsageCollection();
		}
	}
}
