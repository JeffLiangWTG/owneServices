using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class OrganisationPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestUserControl()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			using (OrganisationPlugIn plugIn = new OrganisationPlugIn(organisation))
			{
				AssertEquals(typeof(OrganisationPlugInUserControl), plugIn.UserControl.GetType());
				AssertEquals(typeof(OrgHeaderWrapper), plugIn.BusinessEntity.GetType());
				AssertEquals("Korea", plugIn.Name);
				Assert(!Env.Licence.ImportBroker.IsLoggedIn);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			return new OrganisationPlugIn(organisation);
		}
	}
}
