using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.FR.GUI
{
	public class OrganisationConsigneePlugIn : ZPlugIn
	{
		public OrganisationConsigneePlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		public override string Name => Enterprise.Customs.FR.GUI.Res.GetString("118B5592-65F5-4ADC-9DE2-3ADB3971739E", "FR Deferral");

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		FROrgImpAddInfo AddInfo => addInfo ?? (addInfo = FROrgImpAddInfo.Get((OrgHeader)HostBusinessEntity));
		FROrgImpAddInfo addInfo;

		protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

		protected override Control GetNewUserControl() => new OrganisationConsigneePlugInUserControl();

		protected override ZTabPagePlugIn GetTabPage()
		{
			var result = base.GetTabPage();
			result.Added += delegate
			{
				if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.France)
				{
					var parent = (TabControl)result.Parent;
					parent.SelectedTab = result;
				}
			};
			return result;
		}
	}
}
