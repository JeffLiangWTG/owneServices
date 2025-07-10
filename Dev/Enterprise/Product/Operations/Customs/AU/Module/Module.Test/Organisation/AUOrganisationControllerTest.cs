using CargoWise.EntityFramework;
using Enterprise.Customs.AU.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AUOrganisationController))]
	sealed class AUOrganisationControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugIn()
		{
			var controller = new AUOrganisationControllerForTest();
			using (ZPlugIn plugIn = controller.GetPlugIn(Factory.New<OrgHeader>()))
			{
				AssertEquals(typeof(OrganisationPlugIn), plugIn.GetType());
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.AU.OrganisationCustomsMessaging;
		}

		sealed class AUOrganisationControllerForTest : AUOrganisationController
		{
			public new ZPlugIn GetPlugIn(IBusiness businessEntity) => base.GetPlugIn(businessEntity);
		}
	}
}
