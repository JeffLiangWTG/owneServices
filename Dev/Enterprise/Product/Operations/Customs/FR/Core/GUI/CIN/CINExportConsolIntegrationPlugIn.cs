using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessagesWrappers.CIN;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;

namespace Enterprise.Customs.FR.GUI.CIN
{
	public class CINExportConsolIntegrationPlugIn : CustomsManifestPlugIn
	{
		public CINExportConsolIntegrationPlugIn(ForwardingConsol hostEntity)
		: base(hostEntity)
		{
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new ExportConsolIntegrationMenu(CINExportConsolIntegrationWrapper);
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return new CINExportConsolntegrationWrapper();
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ExportBroker; }
		}
		public override string Name
		{
			get { return "CINExportConsolIntegration"; }
		}

		CINExportConsolntegrationWrapper CINExportConsolIntegrationWrapper
		{
			get
			{
				if (cinExportConsolIntegrationWrapper == null && Consol != null)
				{
					cinExportConsolIntegrationWrapper = new CINExportConsolntegrationWrapper(Consol);
				}
				return cinExportConsolIntegrationWrapper;
			}
		}
		ForwardingConsol Consol
		{
			get { return (ForwardingConsol)ManifestProvider; }
		}

		protected override void ChangeTheVisibilityCore()
		{
			this.Enabled = CINExportConsolIntegrationWrapper?.IsCINExportMenuEnabled ?? false;

			(this.TopLevelMenu as ExportConsolIntegrationMenu)?.RefreshMenu();
		}

		CINExportConsolntegrationWrapper cinExportConsolIntegrationWrapper;
	}
}
