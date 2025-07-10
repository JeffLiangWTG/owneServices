using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions.Ccsuk
{
	public class QueryWithUpdateMethod : GbOperationalActionMethod
	{
		public QueryWithUpdateMethod()
			: base(new ZGuid("ECC2AA81-2B02-44BF-B3D9-BE1C31C0CD6D"))
		{ }

		public override string Name
		{
			get { return "GB CCSUK function: query with update (send FSR)"; }
		}

		public override string Description
		{
			get { return "Send an FSR message with the 'update' flag set, and synchronise the local record with the CCS-UK 'gospel' data"; }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new QueryWithUpdateApplicator();
		}
	}
}
