using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class DocumentsSendingActionCollection : NonPersistentBusinessObjectCollection<DocumentsSendingAction>
	{
		public DocumentsSendingActionCollection(DocumentsSendingActionParent sendingParent) : base(sendingParent.TopLevelBusinessObject.Factory)
		{
			Parent = sendingParent;
		}

		DocumentsSendingActionParent Parent { get; }

		protected sealed override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Message Sending Action Collection should not support adding.");
		}

		protected sealed override bool AllowNewCore => false;

		protected sealed override bool AllowRemoveCore => false;

		internal void PopulateElements()
		{
			Add(CreateElementCore(Parent.CusExitReport));
		}

		DocumentsSendingAction CreateElementCore(CusExitReport exitReport) => (DocumentsSendingAction)Activator.CreateInstance(typeof(DocumentsSendingAction), exitReport);

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((DocumentsSendingAction)bizOAdded).MessageType = Parent.MessageType;
		}
	}
}
