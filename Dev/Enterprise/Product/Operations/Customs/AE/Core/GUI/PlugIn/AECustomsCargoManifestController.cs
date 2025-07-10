using CargoWise.EntityFramework;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AE.GUI.PlugIn
{
	public class AECustomsCargoManifestController : CustomsCargoManifestPluginToConsolController
	{
		public AECustomsCargoManifestController()
		{
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new AECustomsCargoManifestPlugin((ForwardingConsol)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
