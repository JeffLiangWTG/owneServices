using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class OrganisationDetailsPlugInTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest() => new OrganisationDetailsPlugIn(Organisation);

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.FillWithValidTestData();
				}

				return fOrganisation;
			}
		}

		OrgHeader fOrganisation;
	}
}
