using System;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IPageViewPresenter : IDisposable
	{
		bool IsDocumentTranslatable { get; }

		void Init(IPageView pageView);

		void NotifyShowingContextMenu();
		void BeginEdit(IDynamicContentLayoutElement element, EditTrigger trigger);

		void CancelOverride(IDynamicContentLayoutElement element);
	}
}
