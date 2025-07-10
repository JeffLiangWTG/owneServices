using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class AirCargoCusUnderbondPluginController : AirCargo.AUCustomsAirCargoController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.AirCargoCusUnderbondPluginController; }
		}

		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;

			if (businessEntity is ForwardingConsol consol)
			{
				businessEntity = CusMAWB.Load(consol);
			}

			if (businessEntity is ICusUnderbondParent underbondParent)
			{
				result = new GUI.CMRCusUnderbondPlugin(underbondParent, ZString.Empty, PluginName);
			}

			return result;
		}

		const string PluginName = "Master Underbond Movement";
	}
}
