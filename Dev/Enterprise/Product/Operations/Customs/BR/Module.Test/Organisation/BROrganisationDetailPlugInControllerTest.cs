using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(BROrganisationDetailPlugInController))]
	class BROrganisationDetailPlugInControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.BR.OrganisationDetailsPlugIn;
		}

		public void TestSecurityCheckPoint()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var controller = new BROrganisationDetailPlugInController();
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForDelete(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForEdit(orgHeader));
			AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForNew(orgHeader));
			AssertEquals(Env.Security.OrgDetailsViewCountryDefaults, controller.GetCheckPointForView(orgHeader));
		}

		public void TestPlugIn()
		{
			var controller = new BROrganisationDetailPlugInController();
			using (var tabControl = new ZTabControl())
			{
				var plugIns = new PlugIns(Factory.New<OrgHeader>(), tabControl);
				plugIns.Add(controller.ID);
				using (var plugIn = plugIns.GetPlugIn(controller.ID))
				{
					AssertType<GUI.OrganisationDetailsPlugIn>(plugIn);
				}
			}
		}
	}
}
