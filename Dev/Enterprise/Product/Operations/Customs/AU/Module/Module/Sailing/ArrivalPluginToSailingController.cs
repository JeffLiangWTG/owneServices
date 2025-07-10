using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Sailing.GUI;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Module.Sailing
{
	public class ArrivalPluginToSailingController : Freight.Module.JobSailingController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override ZString TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.AU.ArrivalPluginToSailingController; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.AU.Module.Res.GetData("PlugInTabPage|ArrivalPluginToSailingController", "Arrival Reporting", "The Arrival Reporting tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ArrivalPluginToSailing(new CustomsJobVoyageWrapper((JobVoyage)businessEntity));
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
