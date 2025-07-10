using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Sailing.GUI
{
	public class ManifestPluginToSailing : ZPlugIn
	{
		public ManifestPluginToSailing(JobVoyage voyage) : base(voyage)
		{
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			if (!voyageWrapper.Voyage.IsSea)
			{
				ErrorReporter.ReportOnce("Enterprise.Customs.AU.Sailing.GUI.ManifestPluginToSailing.NonSeaVoyage", "This plugin can only be used on a sea voyage!");
			}
		}

		public override string Name
		{
			get { return "Customs Manifest"; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return voyageWrapper;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new ManifestPluginMenu(voyageWrapper);
		}

		readonly CustomsJobVoyageWrapper voyageWrapper;
	}
}
