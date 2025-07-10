namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IDocumentDescriptor
	{
		string Name { get; }
		string DataContext { get; }
		bool EnableTranslation { get; }
		string DocumentType { get; }
		string MenuName { get; }
		bool IsSystemDefined { get; }

		IVisualizerDocumentData DocumentData { get; }
		IPrintInstructions PrintInstructions { get; }
		IMessageInstructions MessageInstructions { get; }
		IDisplayInstructions DisplayInstructions { get; }
		IEDocsInstructions EDocsInstructions { get; }
	}
}
