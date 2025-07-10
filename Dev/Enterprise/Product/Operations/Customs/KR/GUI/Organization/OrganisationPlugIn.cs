using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.KR.GUI
{
	public class OrganisationPlugIn : ZAlwaysLoadPlugIn
	{
		public OrganisationPlugIn(OrgHeader organisation)
			: base(organisation)
		{
			this.organisation = OrgHeaderWrapper.New(organisation);
		}

		readonly OrgHeaderWrapper organisation;

		public override string Name
		{
			get { return (NoResString)"Korea"; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new OrganisationPlugInUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return organisation;
		}
	}
}
