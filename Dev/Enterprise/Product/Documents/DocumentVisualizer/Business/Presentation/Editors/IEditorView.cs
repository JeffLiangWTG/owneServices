using System;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IEditorView : IDisposable
	{
		IDynamicContentLayoutElement Content { get; }

		object Control { get; }

		void BeginEdit();
		void EndEdit();
		void CancelEdit();

		bool WaitForUserInput { get; }

		bool IsPopup { get; }
	}
}