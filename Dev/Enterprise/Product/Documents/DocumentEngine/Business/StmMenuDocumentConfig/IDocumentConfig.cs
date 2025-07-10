namespace Enterprise.DocumentEngine.Business
{
	public interface IDocumentConfig
	{
		IDocumentConfigItemCollection ConfigItems { get; }
		string PageStyle { get; }
		string OverrideDataContext { get; }
		string DocumentTitle { get; }
		string DocumentType { get; }
		bool IsTemplate { get; }
	}
}