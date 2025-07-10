namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IEDocsInstructions
	{
		bool SaveCopyToEDocs { get; }
		object Parent { get; }
	}
}