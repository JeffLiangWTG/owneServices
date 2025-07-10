using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.GB.GUI
{
	public class OrganisationConsigneePlugIn : ZPlugIn
	{
		public OrganisationConsigneePlugIn(OrgHeader organisation)
			: base(organisation)
		{
		}

		public override string Name => "GB Deferral";

		protected override CargoWise.Types.ZBool HasUserControl => true;

		GBOrgImpAddInfo AddInfo => addInfo ?? (addInfo = GBOrgImpAddInfo.Get((OrgHeader)HostBusinessEntity));
		GBOrgImpAddInfo addInfo;

		protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override ZTabPagePlugIn GetTabPage()
		{
			var result = base.GetTabPage();
			result.Added += delegate
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == "GB")
				{
					var parent = (TabControl)result.Parent;
					parent.SelectedTab = result;
				}
			};
			return result;
		}

		protected override Control GetNewUserControl() => new OrganisationConsigneePlugInUserControl();
	}
}
