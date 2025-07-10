using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderArrivalMessageSendingObjectCollection : NctsHeaderMessageSendingObjectCollection
{
	public NctsHeaderArrivalMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public new NctsHeaderArrivalMessageSendingObject this[int index] => (NctsHeaderArrivalMessageSendingObject)base[index];

	public new NctsHeaderArrivalMessageSendingObject AddNew() => (NctsHeaderArrivalMessageSendingObject)base.AddNew();

	protected override bool AllowNewCore => false;
}
