using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public class OrganisationConsigneePlugIn : ZPlugIn
	{
		public OrganisationConsigneePlugIn(OrgHeader organisation)
			: base(organisation)
		{
		}

		public override string Name => Res.GetString("1B47EBA1-2597-4B26-B448-D75B6EB161CE", "Bank Account");

		protected override ZBool HasUserControl => true;

		BROrgImpAddInfo AddInfo => addInfo ?? (addInfo = BROrgImpAddInfo.Get((OrgHeader)HostBusinessEntity));
		BROrgImpAddInfo addInfo;

		protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

		protected override ZArchitecture.GUI.ZTabPagePlugIn GetTabPage()
		{
			var result = base.GetTabPage();
			result.Added += delegate
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Brazil)
				{
					var parent = (TabControl)result.Parent;
					parent.SelectedTab = result;
				}
			};
			return result;
		}

		protected override Control GetNewUserControl() => new OrganisationConsigneePlugInUserControl();

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;
	}
}
