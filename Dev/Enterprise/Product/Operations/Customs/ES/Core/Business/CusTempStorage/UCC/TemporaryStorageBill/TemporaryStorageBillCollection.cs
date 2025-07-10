using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageBillCollection<TBill, THeader> : EU.Business.CusTempStorage.TemporaryStorageBillCollection<TBill, THeader> where TBill : TemporaryStorageBill where THeader : TemporaryStorageHeader
{
	public TemporaryStorageBillCollection(THeader header) : base(header)
	{
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var temporaryStorageBill = (TemporaryStorageBill)child;
		if (Master.MovementOfContainersOnly)
		{
			temporaryStorageBill.ABL_BolType = TemporaryStorageBill.ChildMocCode;
		}
	}
}
