using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class AirCargoDeclarationCusUnderbondController : AirCargo.AUCustomsAirCargoController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.AirCargoDeclarationCusUnderbondController; }
		}

		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;

			// TODO: Fix this hack by adding in an interface on Shipment and Consol that returns the ICusUnderbondParent
			if (businessEntity is ForwardingShipment shipment)
			{
				businessEntity = CusHAWB.Load(shipment);
			}
			else if (businessEntity is ForwardingConsol consol)
			{
				businessEntity = CusMAWB.Load(consol);
			}

			if (businessEntity is ICusUnderbondParent underbondParent)
			{
				result = new CMRCusUnderbondPlugin(underbondParent, ZString.Empty, PluginName, typeof(CusHAWB));
			}

			return result;
		}

		const string PluginName = "House Underbond Movement";
	}
}
