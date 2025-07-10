using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Testing;

public class BEJobDeclarationMessageSendingObjectCollectionForTesting : BEJobDeclarationMessageSendingObjectCollection<BEJobDeclarationMessageSendingObjectForTesting>
{
	public BEJobDeclarationMessageSendingObjectCollectionForTesting(IEnumerable<BusinessObject> messagingObjects, BusinessObjectFactory factory) : base(messagingObjects, factory) { }
}