using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.NCTS.Business;

namespace Enterprise.Customs.IE.Business
{
	internal class QueryOnGuaranteeSendingActionCollection : NonPersistentBusinessObjectCollection<QueryOnGuaranteeSendingAction>
	{
		public QueryOnGuaranteeSendingActionCollection(QueryOnGuaranteeSendingActionParent sendingParent) : base(sendingParent.TopLevelBusinessObject.Factory)
		{
			Parent = sendingParent;
			AddSendingAction();
		}

		void AddSendingAction()
		{
			var sendingAction = new QueryOnGuaranteeSendingAction(Parent.Header);
			sendingAction.MessageType = Parent.MessageType;
			Add(sendingAction);
		}

		protected QueryOnGuaranteeSendingActionParent Parent { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Message Sending Action Collection should not support adding.");
		}

		protected sealed override bool AllowNewCore => false;

		protected sealed override bool AllowRemoveCore => false;
	}
}
