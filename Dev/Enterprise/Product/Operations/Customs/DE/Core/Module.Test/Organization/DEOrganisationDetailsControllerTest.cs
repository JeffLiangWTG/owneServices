using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(DEOrganisationDetailsController))]
	class DEOrganisationDetailsControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var controller = new DEOrganisationDetailsController();
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForDelete(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForEdit(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForNew(orgHeader));
			AssertEquals(Env.Security.OrgDetailsViewCountryDefaults, controller.GetCheckPointForView(orgHeader));
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.DE.OrganisationConsigneePlugIn;
	}
}
