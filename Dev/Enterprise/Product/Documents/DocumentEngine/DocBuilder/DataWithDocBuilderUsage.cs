namespace Enterprise.DocumentEngine.DocBuilder
{
	public class DataWithDocBuilderUsage<T> : DataWithDocumentMacroUsage<T>
	{
		public DataWithDocBuilderUsage(T data)
			: base(data)
		{
		}

		public override DocumentMacroUsageCollection InitializeDocumentMacroUsageCollection()
		{
			return new DocBuilderUsageCollection();
		}
	}
}
