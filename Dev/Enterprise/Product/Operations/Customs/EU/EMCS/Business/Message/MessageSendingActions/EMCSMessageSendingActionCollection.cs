using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public abstract class EMCSMessageSendingActionCollection<TSendingAction> : NonPersistentBusinessObjectCollection<TSendingAction> where TSendingAction : EMCSMessageSendingAction
	{
		protected EMCSMessageSendingActionCollection(EMCSMessageSendingActionParent<TSendingAction> sendingParent) : base(sendingParent.TopLevelBusinessObject.Factory)
		{
			Parent = sendingParent;
		}

		internal void PopulateElements()
		{
			Add(CreateElementCore(Parent.JobDeclaration));
		}

		protected EMCSMessageSendingActionParent<TSendingAction> Parent { get; }

		protected sealed override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Message Sending Action Collection should not support adding.");
		}

		protected sealed override bool AllowNewCore => false;

		protected sealed override bool AllowRemoveCore => false;

		protected virtual TSendingAction CreateElementCore(EMCSJobDeclaration jobDeclaration) => (TSendingAction)Activator.CreateInstance(typeof(TSendingAction), jobDeclaration);
	}
}
