using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class OrganisationConsigneePlugInTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest() => new OrganisationConsigneePlugIn(organisation);

		public new void TestBashUserControlOfPlugIn()
		{
			Assert(true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
		}
		OrgHeader organisation;
	}
}
