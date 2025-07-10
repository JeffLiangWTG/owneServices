using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ES.GUI
{
	public class OrganisationConsigneePlugIn : ZPlugIn
	{
		public OrganisationConsigneePlugIn(OrgHeader organisation)
			: base(organisation)
		{
		}

		public override string Name => Res.GetString("F66E0957-74CD-426B-BBA9-CAE579C8BFA4", "ES Deferral");

		protected override CargoWise.Types.ZBool HasUserControl => true;

		ESOrgImpAddInfo AddInfo => addInfo ?? (addInfo = ESOrgImpAddInfo.Get((OrgHeader)HostBusinessEntity));
		ESOrgImpAddInfo addInfo;

		protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override ZTabPagePlugIn GetTabPage()
		{
			var result = base.GetTabPage();
			result.Added += delegate
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Spain)
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
