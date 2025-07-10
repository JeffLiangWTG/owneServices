using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CN.GUI
{
	public class OrganisationDetailsPlugIn : ZPlugIn
	{
		public OrganisationDetailsPlugIn(OrgHeader orgHeader) : base(orgHeader)
		{
			OrgHeader = orgHeader;
		}

		CNOrgImpAddInfo fAddInfo;
		CNOrgImpAddInfo AddInfo => fAddInfo ?? (fAddInfo = CNOrgImpAddInfo.Get(OrgHeader));

		OrgHeader OrgHeader { get; }

		public override string Name => Res.GetString("8C8C45CE-F8CD-46B1-9AB0-4153CFD8BA77", "China");

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

		protected override Control GetNewUserControl() => new OrganisationDetailsUserControl();
	}
}
