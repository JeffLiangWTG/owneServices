using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class OrganisationConsigneePlugInTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest() => new OrganisationConsigneePlugIn(Factory.NewWithValidTestData<OrgHeader>());
	}
}
