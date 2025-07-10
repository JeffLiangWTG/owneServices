using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderDepartureMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<NctsHeaderDepartureMessageSendingObject>
{
	public NctsHeaderDepartureMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new System.NotImplementedException();
	}

	protected override bool AllowNewCore => false;
}
