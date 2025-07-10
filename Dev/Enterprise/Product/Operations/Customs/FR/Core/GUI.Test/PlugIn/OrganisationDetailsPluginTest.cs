using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	class OrganisationDetailsPlugInTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest() => new OrganisationDetailsPlugIn(Factory.NewWithValidTestData<OrgHeader>());
	}
}
