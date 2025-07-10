using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public abstract class CusEntryHeaderMessageSendingActionCollection<TSendingAction> : NonPersistentBusinessObjectCollection<TSendingAction> where TSendingAction : CusEntryHeaderMessageSendingAction
	{
		protected CusEntryHeaderMessageSendingActionCollection(CusEntryHeaderMessageSendingActionParent<TSendingAction> sendingParent) : base(sendingParent.TopLevelBusinessObject.Factory)
		{
			Parent = sendingParent;
		}

		protected CusEntryHeaderMessageSendingActionParent<TSendingAction> Parent { get; }

		protected sealed override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Message Sending Action Collection should not support adding.");
		}

		protected sealed override bool AllowNewCore => false;

		protected sealed override bool AllowRemoveCore => false;

		internal void PopulateElements()
		{
			foreach (CusEntryHeader entryHeader in AllEntries)
			{
				Add(CreateElementCore(entryHeader));
			}
		}

		protected virtual IEnumerable<CusEntryHeader> AllEntries => Parent.JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>();

		protected virtual TSendingAction CreateElementCore(CusEntryHeader entryHeader) => (TSendingAction)Activator.CreateInstance(typeof(TSendingAction), entryHeader);
	}
}
