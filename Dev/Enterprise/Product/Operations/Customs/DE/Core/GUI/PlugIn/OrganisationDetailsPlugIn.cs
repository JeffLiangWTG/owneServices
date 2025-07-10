using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.DE.GUI
{
	public class OrganisationDetailsPlugIn : ZPlugIn
	{
		readonly OrgHeader organisation;

		public OrganisationDetailsPlugIn(OrgHeader organisation)
			: base(organisation)
		{
			this.organisation = organisation;
		}

		public override string Name => (NoResString)"Germany"; // Germany specific name.
		protected override CargoWise.Types.ZBool HasUserControl => true;
		protected override CargoWise.EntityFramework.IBusiness GetBusinessEntityForPlugIn() => organisation;
		protected override Licensing.LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;
		protected override System.Windows.Forms.Control GetNewUserControl() => new OrganisationDetailsUserControl();
	}
}
