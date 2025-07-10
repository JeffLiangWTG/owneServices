using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business
{
	public class FinalSumAWithAPreliminaryMessageSendingActionCollection : NonPersistentBusinessObjectCollection<FinalSumAWithAPreliminaryMessageSendingAction>
	{
		public FinalSumAWithAPreliminaryMessageSendingActionCollection(IEnumerable<CusTempStorageLine> messagingObjects, BusinessObjectFactory factory)
			: base(factory)
		{
			this.messagingObjects = messagingObjects;
		}
		readonly IEnumerable<CusTempStorageLine> messagingObjects;

		public void PopulateElements()
		{
			foreach (var messagingObject in messagingObjects)
			{
				Add(GetSendingAction(messagingObject));
			}
		}

		protected FinalSumAWithAPreliminaryMessageSendingAction GetSendingAction(CusTempStorageLine messagingObject) => new FinalSumAWithAPreliminaryMessageSendingAction(messagingObject);

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Users cannot add a new member");
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
