using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.GUI.PlugIn
{
	public class AUCustomsExit2PluginToConsolController : CustomsCargoManifestPluginToConsolController
	{
		public AUCustomsExit2PluginToConsolController()
		{
		}

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|CargoManifestPlugInForConsol", "Customs Export Manifest");

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new AUCustomsExit2PluginToConsol((ForwardingConsol)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.AUCustomsEX2;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.AUCustomsEX2Modify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.AUCustomsEX2Modify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.AUCustomsEX2Modify;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");
	}
}
