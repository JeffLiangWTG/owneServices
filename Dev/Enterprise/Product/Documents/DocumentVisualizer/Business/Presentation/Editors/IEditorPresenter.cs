using System;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IEditorPresenter : IDisposable
	{
		void Init(IEditorView editorView);

		void CommitChanges();
		void CancelChanges();

		void MoveToNextEditor();
		void MoveToPreviousEditor();
	}
}