using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentSendingActionCollection : NonPersistentBusinessObjectCollection<DocumentSendingAction>
	{
		public DocumentSendingActionCollection(DocumentSendingActionParent sendingParent) : base(sendingParent.Factory)
		{
			Parent = sendingParent;
		}

		DocumentSendingActionParent Parent { get; }

		protected sealed override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Message Sending Action Collection should not support adding.");
		}

		protected sealed override bool AllowNewCore => false;

		protected sealed override bool AllowRemoveCore => false;

		internal void AddSendingAction()
		{
			var sendingAction = (DocumentSendingAction)Activator.CreateInstance(typeof(DocumentSendingAction), Parent.Header);
			sendingAction.MessageType = Parent.MessageType;
			Add(sendingAction);
		}
	}
}
