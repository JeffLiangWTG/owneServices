using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.EU.GUI
{
	public class OrganisationConsigneePlugIn : ZPlugIn
	{
		public OrganisationConsigneePlugIn(OrgHeader organisation)
			: base(organisation)
		{
		}

		public override string Name => (NoResString)"EU Deferral";

		protected override CargoWise.Types.ZBool HasUserControl => true;

		EUOrgImpAddInfo AddInfo => addInfo ?? (addInfo = EUOrgImpAddInfo.Get((OrgHeader)HostBusinessEntity, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		EUOrgImpAddInfo addInfo;

		protected override IBusiness GetBusinessEntityForPlugIn() => AddInfo;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override ZTabPagePlugIn GetTabPage()
		{
			var result = base.GetTabPage();
			result.Added += delegate
			{
				if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
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
