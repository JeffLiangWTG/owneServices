using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class ExitControlMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<ExitControlMessageSendingObject>
	{
		public ExitControlMessageSendingObjectCollection(IEnumerable<CusExitReport> messagingObjects, BusinessObjectFactory factory) : base(factory)
		{
			PopulateElements(messagingObjects);
		}

		protected virtual ExitControlMessageSendingObject GetSendingAction(CusExitReport messagingObject)
		{
			return new ExitControlMessageSendingObject(messagingObject);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Users cannot add a new member");
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		void PopulateElements(IEnumerable<CusExitReport> messagingObjects)
		{
			foreach (var messagingObject in messagingObjects)
			{
				Add(GetSendingAction(messagingObject));
			}
		}
	}
}
