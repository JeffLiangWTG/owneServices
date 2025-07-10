using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DocumentSecurityServiceTest : TestCaseWithFactory
	{
		public void TestSecurityRights()
		{
			var menuItem1 = Factory.New<VisualizerMenuItem>();
			menuItem1.SU_BusinessContext = "Consol";
			menuItem1.SU_MenuName = "Form 1";

			var menuItem2 = Factory.New<VisualizerMenuItem>();
			menuItem2.SU_BusinessContext = "Consol";
			menuItem2.SU_MenuName = "Form 2";

			var menuItem3 = Factory.New<VisualizerMenuItem>();
			menuItem3.SU_BusinessContext = "Consol";
			menuItem3.SU_MenuName = "Form 3";

			using (var consolModule = ObjectFactory.Get<IModuleFactory>().Create(ModuleIDs.JobConsol))
			{
				var parentCheckpoint = Env.Security.FindOrCreateVisualizerFormsCheckpoint(ModuleIDs.JobConsol, consolModule.SecurityCheckpoint);

				var menuCheckpoint1 = Env.Security.FindOrCreateVisualizerFormCheckpoint(menuItem1.PK.ToGuid(), menuItem1.SU_MenuNameMultilingual, ModuleIDs.JobConsol, parentCheckpoint);
				var menuCheckpoint2 = Env.Security.FindOrCreateVisualizerFormCheckpoint(menuItem2.PK.ToGuid(), menuItem2.SU_MenuNameMultilingual, ModuleIDs.JobConsol, parentCheckpoint);

				menuCheckpoint1.IsAllowed = true;
				menuCheckpoint2.IsAllowed = false;

				Env.Security.FindOrCreateVisualizerFormModifyCheckpoint(menuItem1.PK.ToGuid(), menuItem1.SU_MenuNameMultilingual, ModuleIDs.JobConsol, menuCheckpoint1).IsAllowed = true;
				Env.Security.FindOrCreateVisualizerFormModifyCheckpoint(menuItem2.PK.ToGuid(), menuItem2.SU_MenuNameMultilingual, ModuleIDs.JobConsol, menuCheckpoint2).IsAllowed = false;

				Env.Security.FindOrCreateVisualizerFormDeliveryCheckpoint(menuItem1.PK.ToGuid(), menuItem1.SU_MenuNameMultilingual, ModuleIDs.JobConsol, menuCheckpoint1).IsAllowed = true;
				Env.Security.FindOrCreateVisualizerFormDeliveryCheckpoint(menuItem2.PK.ToGuid(), menuItem2.SU_MenuNameMultilingual, ModuleIDs.JobConsol, menuCheckpoint2).IsAllowed = false;

				Env.Security.FindOrCreateVisualizerFormSendMessageCheckpoint(menuItem1.PK.ToGuid(), menuItem1.SU_MenuNameMultilingual, ModuleIDs.JobConsol, menuCheckpoint1).IsAllowed = true;
				Env.Security.FindOrCreateVisualizerFormSendMessageCheckpoint(menuItem2.PK.ToGuid(), menuItem2.SU_MenuNameMultilingual, ModuleIDs.JobConsol, menuCheckpoint2).IsAllowed = false;

				var helper1 = new DocumentSecurityService(menuItem1, ModuleIDs.JobConsol);
				var helper2 = new DocumentSecurityService(menuItem2, ModuleIDs.JobConsol);
				var helper3 = new DocumentSecurityService(menuItem3, ModuleIDs.JobConsol);

				Assert("menuItem1 security checkpoint allowed.", helper1.CanView);
				Assert("menuItem2 security checkpoint disallowed.", !helper2.CanView);
				Assert("menuItem3 has not configured security checkpoint, allowed by default.", helper3.CanView);

				Assert("menuItem1 security checkpoint allowed.", helper1.CanModify);
				Assert("menuItem2 security checkpoint disallowed.", !helper2.CanModify);
				Assert("menuItem3 has not configured security checkpoint, allowed by default.", helper3.CanModify);

				Assert("menuItem1 security checkpoint allowed.", helper1.CanDeliver);
				Assert("menuItem2 security checkpoint disallowed.", !helper2.CanDeliver);
				Assert("menuItem3 has not configured security checkpoint, allowed by default.", helper3.CanDeliver);

				Assert("menuItem1 security checkpoint allowed.", helper1.CanSendMessage);
				Assert("menuItem2 security checkpoint disallowed.", !helper2.CanSendMessage);
				Assert("menuItem3 has not configured security checkpoint, allowed by default.", helper3.CanSendMessage);
			}
		}

		public void TestAllowToolsAccessCheckpoint()
		{
			Env.Security.AllowToolsAccess.IsAllowed = false;

			var menuItem = Factory.NewWithValidTestData<VisualizerMenuItem>();
			var securityService = new DocumentSecurityService(menuItem, ModuleIDs.JobConsol);
			Assert(!securityService.AllowToolsAccess);

			Env.Security.AllowToolsAccess.IsAllowed = true;
			Assert(securityService.AllowToolsAccess);
		}
	}
}
