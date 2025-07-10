using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Sailing.GUI;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module
{
	public class ManifestPluginToSailingController : Freight.Module.JobSailingController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override ZString TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.ManifestPluginToSailingController; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ManifestPluginToSailing((JobVoyage)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
