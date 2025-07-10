using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class OrganisationConsigneePlugInUserControl : ZUserControl
	{
		public OrganisationConsigneePlugInUserControl()
		{
			InitializeComponent();
		}

		Type GetDataSourceType()
		{
			return new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
	}
}
