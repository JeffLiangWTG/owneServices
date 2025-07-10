using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class CusSeaManOBLDetailCusUnderbondPluginController : VoyageManifestController
	{
		public override ControllerID ID => ControllerIDs.Customs.AU.CusSeaManOBLDetailCusUnderbondPluginController;

		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) =>
			new GUI.CMRCusUnderbondPlugin((IAUCusUnderbondUnionCollectionParent)businessEntity, ZString.Empty);

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|CusSeaManOBLDetailCusUnderbond", "Customs Underbond Movement");
	}
}
