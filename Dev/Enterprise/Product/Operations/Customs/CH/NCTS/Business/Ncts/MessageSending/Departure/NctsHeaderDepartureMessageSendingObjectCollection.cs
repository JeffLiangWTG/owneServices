using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderDepartureMessageSendingObjectCollection : NctsHeaderMessageSendingObjectCollection
{
	public NctsHeaderDepartureMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public new NctsHeaderDepartureMessageSendingObject this[int index] => (NctsHeaderDepartureMessageSendingObject)base[index];

	public new NctsHeaderDepartureMessageSendingObject AddNew() => (NctsHeaderDepartureMessageSendingObject)base.AddNew();

	protected override bool AllowNewCore => false;
}
