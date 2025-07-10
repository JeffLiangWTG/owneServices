using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.EU.GUI
{
	public class OrganisationConsignorPlugIn : ZPlugIn
	{
		public OrganisationConsignorPlugIn(OrgHeader organisation)
			: base(organisation)
		{
		}

		public override string Name => (NoResString)"Customs Defaults";

		protected override CargoWise.Types.ZBool HasUserControl => true;

		EUOrgImpAddInfo AddInfo => addInfo ?? (addInfo = EUOrgImpAddInfo.Get((OrgHeader)HostBusinessEntity, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		EUOrgImpAddInfo addInfo;

		protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override Control GetNewUserControl() => new OrganisationConsignorPlugInUserControl();
	}
}
