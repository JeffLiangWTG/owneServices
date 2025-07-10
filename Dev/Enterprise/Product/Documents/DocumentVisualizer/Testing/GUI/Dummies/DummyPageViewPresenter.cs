using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DummyPageViewPresenter : IPageViewPresenter
	{
		public bool IsDocumentTranslatable { get; set; }

		public void Init(IPageView pageView)
		{
		}

		public void NotifyShowingContextMenu()
		{
		}

		public void BeginEdit(IDynamicContentLayoutElement element, EditTrigger trigger)
		{
		}

		public void CancelOverride(IDynamicContentLayoutElement element)
		{
		}

		public void Dispose()
		{
		}
	}
}
