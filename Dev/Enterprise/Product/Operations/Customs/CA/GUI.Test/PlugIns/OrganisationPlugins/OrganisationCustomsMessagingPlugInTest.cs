using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class OrganisationCustomsMessagingPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestUserControl()
		{
			using (var plugIn = GetPlugInToTest())
			{
				AssertEquals(typeof(OrganisationCustomsMessagingPlugInUserControl), plugIn.UserControl.GetType());
				AssertEquals(typeof(OrgHeaderTCPMessageWrapper), plugIn.BusinessEntity.GetType());
				AssertEquals(typeof(OrganisationCustomsMessagingPlugInMenu), plugIn.TopLevelMenu.GetType());
				AssertEquals("Customs Messaging", plugIn.Name);
				Assert(!Env.Licence.ImportBroker.IsLoggedIn);
			}
		}

		public void TestChangeTheVisibility()
		{
			using (var plugIn = new OrganisationCustomsMessagingPlugIn(new CustomsMessagingPlugInSupportOrgHeaderWrapper(Factory.NewWithValidTestData<OrgHeader>())))
			{
				AssertEquals("Visibility", plugIn.Enabled, plugIn.PlugInSupport.PlugInVisible);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
			=> new OrganisationCustomsMessagingPlugIn(new CustomsMessagingPlugInSupportOrgHeaderWrapper(Factory.NewWithValidTestData<OrgHeader>()));
	}
}
