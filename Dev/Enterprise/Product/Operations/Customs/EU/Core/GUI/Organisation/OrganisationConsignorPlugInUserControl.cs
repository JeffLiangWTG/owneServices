using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class OrganisationConsignorPlugInUserControl : ZUserControl
	{
		public OrganisationConsignorPlugInUserControl()
		{
			InitializeComponent();
		}

		Type GetDataSourceType()
		{
			return new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
	}
}
