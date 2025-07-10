using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.BR.GUI.Testing
{
	sealed class OrganisationDetailsPlugInTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			return new OrganisationDetailsPlugIn(organisation);
		}
	}
}
