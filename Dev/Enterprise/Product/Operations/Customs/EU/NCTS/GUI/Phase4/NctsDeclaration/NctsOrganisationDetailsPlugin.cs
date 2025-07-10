using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class NctsOrganisationDetailsPlugin : ZPlugIn
	{
		public NctsOrganisationDetailsPlugin(OrgHeader organisation)
			: base(organisation)
		{
			this.wrapper = OrgHeaderWrapper.New(organisation);
		}

		readonly OrgHeaderWrapper wrapper;

		public override string Name
		{
			get { return "NCTS"; }
		}

		protected override CargoWise.Types.ZBool HasUserControl
		{
			get { return true; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return wrapper;
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return new NctsOrganisationPluginUserControl();
		}
	}
}
