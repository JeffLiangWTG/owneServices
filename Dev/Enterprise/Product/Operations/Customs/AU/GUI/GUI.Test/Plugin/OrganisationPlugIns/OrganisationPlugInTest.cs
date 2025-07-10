using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.GUI.Testing
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
				AssertEquals(typeof(OrganisationPlugInMenu), plugIn.TopLevelMenu.GetType());
				AssertEquals("Customs Messaging", plugIn.Name);
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
