using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions.Ccsuk
{
	public class DetachFromForwardingMethod : GbOperationalActionMethod
	{
		public DetachFromForwardingMethod()
			: base(new ZGuid("0F75FE66-9261-43D3-B483-49AFD3E306B5")) { }

		public override string Name
		{
			get { return "GB CCSUK function: Detach Awb From Forwarding"; }
		}

		public override string Description
		{
			get { return "Detach Awb link to Shipment or Consol"; }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new DetachFromForwardingApplicator();
		}
	}
}
