using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DummyPageView : IPageView
	{
		public IPageViewPresenter Presenter => null;

		public IEnumerable<ILayoutElement> Elements { get; set; }

		public void Refresh()
		{
		}

		public void Scale(float scale)
		{
		}

		public void ShowEditor(IEditorView editorView)
		{
		}
	}
}