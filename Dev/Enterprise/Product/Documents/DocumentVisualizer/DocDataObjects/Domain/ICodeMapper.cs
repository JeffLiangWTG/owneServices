namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface ICodeMapper
	{
		bool ShowForeignCode { get; }
		string GetForeignCode(string localCode);
		string GetLocalCode(string foreignCode);
	}
}
