using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(KROrganisationController))]
	sealed class KROrganisationControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.KR.OrganisationDetailsPlugIn;
		}

		public void TestSecurityCheckPoint()
		{
			var controller = new KROrganisationController();
			AssertEquals(controller.CheckPointForDeleteExposedForTest, Env.Security.OrgConfigModifyCountryDefaults);
			AssertEquals(controller.CheckPointForEditExposedForTest, Env.Security.OrgConfigModifyCountryDefaults);
			AssertEquals(controller.CheckPointForNewExposedForTest, Env.Security.OrgConfigModifyCountryDefaults);
			AssertEquals(controller.CheckPointForViewExposedForTest, Env.Security.OrgDetailsViewCountryDefaults);
		}

		public void TestPlugIn()
		{
			var controller = new KROrganisationController();
			using (var tabControl = new ZTabControl())
			{
				var plugIns = new PlugIns(Factory.New<OrgHeader>(), tabControl);
				plugIns.Add(controller.ID);
				using (var plugIn = plugIns.GetPlugIn(controller.ID))
				{
					AssertType<GUI.OrganisationPlugIn>(plugIn);
				}
			}
		}
	}
}
