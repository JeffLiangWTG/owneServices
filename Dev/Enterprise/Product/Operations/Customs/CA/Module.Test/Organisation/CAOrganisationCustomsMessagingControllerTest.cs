using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAOrganisationCustomsMessagingController))]
	sealed class CAOrganisationCustomsMessagingControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugIn()
		{
			var controller = new CAOrganisationCustomsMessagingController();
			using (var plugIn = controller.GetPlugInInternal(Factory.New<OrgHeader>()))
			{
				AssertType<GUI.OrganisationCustomsMessagingPlugIn>(plugIn);
			}
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new CAOrganisationCustomsMessagingController();
			AssertEquals(controller.CheckPointForDeleteExposedForTest, Env.Security.QueryMessages);
			AssertEquals(controller.CheckPointForEditExposedForTest, Env.Security.QueryMessages);
			AssertEquals(controller.CheckPointForNewExposedForTest, Env.Security.QueryMessages);
			AssertEquals(controller.CheckPointForViewExposedForTest, Env.Security.QueryMessagesView);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.OrganisationCustomsMessaging;
	}
}
