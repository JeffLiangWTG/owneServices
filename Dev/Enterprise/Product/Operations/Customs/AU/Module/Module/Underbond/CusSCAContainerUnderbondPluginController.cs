using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module.SeaCargo;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class CusSCAContainerUnderbondPluginController : AUCustomsSeaCargoController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.CusSCAContainerUnderbondPluginController; }
		}

		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			if (businessEntity is ForwardingConsol)
			{
				businessEntity = CusSCAOceanBill.Load((ForwardingConsol)businessEntity);
			}
			if (businessEntity is CusSCAOceanBill)
			{
				CusUnderbondPlugin plugin = new GUI.CMRCusUnderbondPlugin((IAUCusUnderbondUnionCollectionParent)businessEntity, ZString.Empty);
				return plugin;
			}
			else
			{
				return null;
			}
		}
	}
}
