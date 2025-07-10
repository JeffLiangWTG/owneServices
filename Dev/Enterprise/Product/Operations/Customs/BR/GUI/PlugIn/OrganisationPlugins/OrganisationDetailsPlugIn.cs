using Enterprise.Customs.BR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.BR.GUI
{
	public class OrganisationDetailsPlugIn : ZPlugIn
	{
		public OrganisationDetailsPlugIn(OrgHeader orgHeader) : base(orgHeader)
		{
			wrapper = new OrgHeaderWrapper(orgHeader);
		}

		readonly OrgHeaderWrapper wrapper;

		protected override CargoWise.Types.ZBool HasUserControl => true;

		protected override System.Windows.Forms.Control GetNewUserControl() => new OrganisationDetailPlugInUserControl();

		protected override CargoWise.EntityFramework.IBusiness GetBusinessEntityForPlugIn() => wrapper;

		protected override Licensing.LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		public override string Name => Res.GetString("71231821-a603-4106-b529-b69086798097", "Brazil");
	}
}
