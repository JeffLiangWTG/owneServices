using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CNOrgAdditionalCustomsDefaultsPlugInTest : ZPlugInGenericTest
	{
		public void TestHasUserControl()
		{
			using (var plugin = GetNewPlugIn(Organization))
			{
				AssertNotNull(plugin.UserControl);
			}
		}

		public new void TestBashUserControlOfPlugIn() => Assert(true);

		protected override ZPlugIn GetPlugInToTest() => GetNewPlugIn(Organization);

		protected CNOrgAdditionalCustomsDefaultsPlugIn GetNewPlugIn(OrgHeader organization)
		{
			return new CNOrgAdditionalCustomsDefaultsPlugIn(organization);
		}

		OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					organization = Factory.NewWithValidTestData<OrgHeader>();
					var link = organization.SupplierLinks.AddNew();
					link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
				}

				return organization;
			}
		}

		OrgHeader organization;
	}
}
