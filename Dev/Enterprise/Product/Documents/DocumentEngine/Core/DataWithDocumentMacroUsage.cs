namespace Enterprise.DocumentEngine
{
	public abstract class DataWithDocumentMacroUsage<T>
	{
		protected DataWithDocumentMacroUsage(T data)
		{
			this.Data = data;
		}

		public T Data { get; set; }

		public DocumentMacroUsageCollection DocBuilderUsages
		{
			get
			{
				if (docBuilderUsages == null)
				{
					docBuilderUsages = InitializeDocumentMacroUsageCollection();
				}
				return docBuilderUsages;
			}
		}
		DocumentMacroUsageCollection docBuilderUsages;

		public abstract DocumentMacroUsageCollection InitializeDocumentMacroUsageCollection();
	}
}
