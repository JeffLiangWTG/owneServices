using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.Module.Testing
{
	[TestedType(typeof(MailItemTemplateController))]
	public class MailItemTemplateControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MailItemTemplate;
		}

		public void TestModuleId()
		{
			AssertEquals(ModuleIDs.MailItemTemplate, new MailItemTemplateController().ModuleID);
		}

		public void TestGetCheckPointForEdit()
		{
			var controller = new MailItemTemplateController();
			var template = Factory.New<MailItemTemplate>();
			Assert(controller.GetCheckPointForEdit(template).IsAllowed);
			Env.Security.EditAllEmailTemplates.IsAllowed = false;
			Assert(controller.GetCheckPointForEdit(template).IsAllowed);
			template.MIT_GC_Company = ZGuid.NewZGuid();
			Assert(!controller.GetCheckPointForEdit(template).IsAllowed);
			Env.Security.EditAllEmailTemplates.IsAllowed = true;
			Assert(controller.GetCheckPointForEdit(template).IsAllowed);
			Env.Security.EditAllEmailTemplates.IsAllowed = false;
			template.MIT_GC_Company = GlbCompany.CurrentCompany.PK;
			Assert(controller.GetCheckPointForEdit(template).IsAllowed);
			template.MIT_GB_Branch = GlbBranch.CurrentBranch.PK;
			Assert(controller.GetCheckPointForEdit(template).IsAllowed);
			template.MIT_GE_Department = ZGuid.NewZGuid();
			Assert(!controller.GetCheckPointForEdit(template).IsAllowed);
		}
	}
}
