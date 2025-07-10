using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public class MessageSendingActionCollection : NonPersistentBusinessObjectCollection<MessageSendingAction>
	{
		public MessageSendingActionCollection(IEnumerable<BusinessObject> messagingObjects, Func<BusinessObject, ZString> getDetails, BusinessObjectFactory factory) : base(factory)
		{
			this.messagingObjects = messagingObjects;
			this.getDetails = getDetails;
		}

		readonly IEnumerable<BusinessObject> messagingObjects;
		readonly Func<BusinessObject, ZString> getDetails;

		internal void PopulateElements()
		{
			foreach (BusinessObject messagingObject in messagingObjects)
			{
				Add(GetSendingAction(messagingObject));
			}
		}

		protected virtual MessageSendingAction GetSendingAction(BusinessObject messagingObject) => new MessageSendingAction(messagingObject, x => getDetails(x));

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException("Users cannot add a new member");

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
