using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class NctsHeaderMessageSendingObjectCollection : EU.NCTS.Business.NctsHeaderMessageSendingObjectCollection
{
	public NctsHeaderMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public new NctsHeaderMessageSendingObject this[int index] => (NctsHeaderMessageSendingObject)base[index];

	public new NctsHeaderMessageSendingObject AddNew() => (NctsHeaderMessageSendingObject)base.AddNew();
}
