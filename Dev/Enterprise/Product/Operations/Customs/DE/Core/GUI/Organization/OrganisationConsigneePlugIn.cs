using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.DE.GUI
{
	public class OrganisationConsigneePlugIn : ZPlugIn
	{
		public OrganisationConsigneePlugIn(OrgHeader organisation)
			: base(organisation)
		{
		}

		public override string Name => Res.GetString("c42f4286-0ba1-4dea-9389-33ad903675d4", "DE Deferral");

		protected override ZBool HasUserControl => true;

		DEOrgImpAddInfo AddInfo => addInfo ?? (addInfo = DEOrgImpAddInfo.Get((OrgHeader)HostBusinessEntity));
		DEOrgImpAddInfo addInfo;

		protected override ZArchitecture.GUI.ZTabPagePlugIn GetTabPage()
		{
			var result = base.GetTabPage();
			result.Added += delegate
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Germany)
				{
					var parent = (TabControl)result.Parent;
					parent.SelectedTab = result;
				}
			};
			return result;
		}

		protected override Control GetNewUserControl() => new OrganisationConsigneePlugInUserControl();

		protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;
	}
}
