using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IEditorBuildService
	{
		IEditorView Build(IDynamicContentLayoutElement element, IEditorPresenter editorPresenter, IMacroBusinessObjectProperty[] properties, float scale);
	}
}