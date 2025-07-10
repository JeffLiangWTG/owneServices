using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class InlandTransportCollection : CusCodeDataCollection<InlandTransport>
{
	public InlandTransportCollection(BusinessObject master) : base(master, Constants.CusCodeDataTypes.TransportInland)
	{
	}

	public new IInlandTransportParent Master => (IInlandTransportParent)base.Master;

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var inlandTransport = (InlandTransport)child;
		Master.InlandTransportLineNumberGenerator.RecalculateWhenAdded(inlandTransport);
	}

	protected override void OnRemoved(BusinessObject bizO)
	{
		base.OnRemoved(bizO);

		Master.InlandTransportLineNumberGenerator.ReCalculateAll();
	}
}
