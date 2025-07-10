using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class CusHAWBCusUnderbondPluginController : AirCargo.AUCustomsHouseAirCargoController
	{
		public override ControllerID ID
		{
			get
			{
				return ControllerIDs.Customs.AU.CusHAWBCusUnderbondPluginController;
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GUI.CMRCusUnderbondPlugin((IAUCusUnderbondUnionCollectionParent)businessEntity, "HouseBills.");
		}
	}
}
