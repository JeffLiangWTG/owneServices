using System;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class EditorPresenter : IEditorPresenter
	{
		public EditorPresenter(IDocument document, IDynamicContentLayoutElement content, IEventBroker broker)
		{
			this.document = document ?? throw new ArgumentNullException(nameof(document));
			this.content = content ?? throw new ArgumentNullException(nameof(content));
			this.broker = broker ?? throw new ArgumentNullException(nameof(broker));

			this.eventSubscriptions = new[]
			{
				broker.GetEvent<EndEditEvent>().Subscribe(OnEndEditEvent, document)
			};
		}

		readonly IDocument document;
		readonly IDynamicContentLayoutElement content;
		readonly IEventBroker broker;
		readonly IDisposable[] eventSubscriptions;
		IEditorView currentEditorView;

		public void Init(IEditorView editorView)
		{
			this.currentEditorView = editorView;
		}

		void OnEndEditEvent(EndEditEvent eventData)
		{
			currentEditorView?.EndEdit();
			broker.Publish(new ValueChangedEvent(document));
			Unsubscribe();
		}

		public void CommitChanges() => broker.Publish(new EndEditEvent(document, content));

		public void CancelChanges() => broker.Publish(new EndEditEvent(document, content));

		public void MoveToNextEditor()
		{
			broker.Publish(new EndEditEvent(document, content));
			broker.Publish(new MoveToNextEditableElementEvent(document, content, MoveToNextDirection.Forward));
		}

		public void MoveToPreviousEditor()
		{
			broker.Publish(new EndEditEvent(document, content));
			broker.Publish(new MoveToNextEditableElementEvent(document, content, MoveToNextDirection.Backward));
		}

		void Unsubscribe()
		{
			eventSubscriptions.ForEach(s => s.Dispose());
			currentEditorView = null;
		}

		public void Dispose() => Unsubscribe();
	}
}
