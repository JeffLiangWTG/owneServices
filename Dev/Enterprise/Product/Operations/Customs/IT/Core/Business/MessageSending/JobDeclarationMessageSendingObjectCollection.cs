using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public class JobDeclarationMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject>
{
	public JobDeclarationMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new System.NotImplementedException();
	}

	protected override bool AllowNewCore => false;
}
