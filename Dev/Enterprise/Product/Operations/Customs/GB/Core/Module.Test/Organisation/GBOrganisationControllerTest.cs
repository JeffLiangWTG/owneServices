using CargoWise.EntityFramework;
using Enterprise.Customs.GB.GUI.Organisation;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(GBOrganisationController))]
	sealed class GBOrganisationControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugIn()
		{
			var controller = new GBOrganisationControllerForTest();
			using var plugIn = controller.GetPlugInExposed(Factory.New<OrgHeader>());
			AssertType<OrganisationPlugIn>(plugIn);
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new GBOrganisationController();
			AssertEquals(controller.CheckPointForViewExposedForTest, Env.Security.OrganisationView);
			AssertEquals(controller.CheckPointForNewExposedForTest, Env.Security.OrganisationNew);
			AssertEquals(controller.CheckPointForEditExposedForTest, Env.Security.OrganisationModify);
			AssertEquals(controller.CheckPointForDeleteExposedForTest, Env.Security.OrganisationDelete);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.GB.OrganisationCustomsMessaging;

		class GBOrganisationControllerForTest : GBOrganisationController
		{
			public ZPlugIn GetPlugInExposed(IBusiness businessEntity) => GetPlugIn(businessEntity);
		}
	}
}
