using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(OrganisationDetailsController))]
	class OrganisationDetailsControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var controller = new OrganisationDetailsController();
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForDelete(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForEdit(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForNew(orgHeader));
			AssertEquals(Env.Security.OrgDetailsViewCountryDefaults, controller.GetCheckPointForView(orgHeader));
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CN.OrganisationDetailsPlugIn;
	}
}
