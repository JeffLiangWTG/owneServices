
namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IVisualizableDocumentCommand
	{
		string Name { get; }
		bool IsApplicable { get; }
		void Execute();
	}
}