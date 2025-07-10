using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class CusSCAHouseCusUnderbondPluginController : SeaCargo.AUCustomsSeaCargoController
	{
		public override ControllerID ID
		{
			get
			{
				return ControllerIDs.Customs.AU.CusSCAHouseCusUnderbondPluginController;
			}
		}
		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			if (businessEntity is ForwardingShipment)
			{
				businessEntity = CusSCAHouse.LoadFromShipment(businessEntity as ForwardingShipment);
			}
			if (businessEntity is CusSCAHouse)
			{
				return new GUI.CMRCusUnderbondPlugin((IAUCusUnderbondUnionCollectionParent)businessEntity, ZString.Empty);
			}
			else
			{
				return null;
			}
		}
	}
}
