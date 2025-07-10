using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public abstract class BEJobDeclarationMessageSendingObjectCollection<TSendingAction> : JobDeclarationMessageSendingObjectCollection<TSendingAction>
	where TSendingAction : BEJobDeclarationMessageSendingObject
{
	public BEJobDeclarationMessageSendingObjectCollection(IEnumerable<BusinessObject> messagingObjects, BusinessObjectFactory factory) : base(factory)
	{
		this.messagingObjects = messagingObjects;
	}

	readonly IEnumerable<BusinessObject> messagingObjects;

	public void PopulateElements()
	{
		foreach (BusinessObject messagingObject in messagingObjects)
		{
			Add(GetSendingAction(messagingObject));
		}
	}

	protected virtual BEJobDeclarationMessageSendingObject GetSendingAction(BusinessObject messagingObject)
	{
		return (BEJobDeclarationMessageSendingObject)Activator.CreateInstance(typeof(TSendingAction), messagingObject);
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;
}
