using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IPageView
	{
		IPageViewPresenter Presenter { get; }

		IEnumerable<ILayoutElement> Elements { get; set; }

		void Refresh();

		void Scale(float scale);

		void ShowEditor(IEditorView editorView);
	}
}