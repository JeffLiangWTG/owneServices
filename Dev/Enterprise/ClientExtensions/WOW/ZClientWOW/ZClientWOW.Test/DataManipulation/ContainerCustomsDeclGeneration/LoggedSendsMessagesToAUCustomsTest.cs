using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	public class LoggedSendsMessagesToAUCustomsTest : TransactionedTestCase
	{
		public void TestLogErrors()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobDeclaration decl = factory.New<JobDeclaration>();
			NotificationBuffer buffer = new NotificationBuffer(null);
			LoggedSendsMessagesToAUCustoms log = new LoggedSendsMessagesToAUCustoms(decl, buffer);
			AssertEquals(false, buffer.HasErrors);
			decl.MessageInitiator = log;
			decl.DoMerge();
			AssertEquals(true, buffer.HasErrors);
		}

		[GuiTest]
		public void TestAskUserToContinueWithAction()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			JobDeclaration decl = factory.New<JobDeclaration>();
			NotificationBuffer buffer = new NotificationBuffer(null);
			LoggedSendsMessagesToAUCustoms log = new LoggedSendsMessagesToAUCustoms(decl, buffer);
			bool oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
			bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
			var oldControl = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				Env.Security.SupervisorOverrides.IsAllowed = false;
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
				Env.Security.AllowMessageErrors.IsAllowed = false;
				GlbStaff.CurrentUser.GS_IsController = false;
				Assert(log.AskUserToContinueWithAction("TestMessage", "TestTitle", decl));
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				Assert("No staff nor group has security to send with error", log.AskUserToContinueWithAction("TestMessage", "TestTitle", decl));
				factory.Save();
				factory = new BusinessObjectFactory();
				decl = factory.Load<JobDeclaration>(decl.PK);
				log = new LoggedSendsMessagesToAUCustoms(decl, buffer);
				var staff = factory.New<GlbStaff>();
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_IsActive = true;
				staff.GS_Code = "GS1";
				var se = factory.New<GlbSecurity>();
				se.GU_SecurityRight = Env.Security.AllowMessageErrors.Code;
				se.GU_SecurityItemIsAllowed = true;
				se.GU_GS = staff.PK;
				se.GU_GB = GlbBranch.CurrentBranch.PK;
				factory.Save();
				decl.JE_MessageType = "ZZZ";
				decl.Validation.ValidateAll();
				Assert(!log.AskUserToContinueWithAction("TestMessage", "TestTitle", decl));
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
				GlbStaff.CurrentUser.GS_IsController = oldControl;
				Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
				Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
			}
		}

		public void TestYesNoCancelQuery()
		{
			var factory = new BusinessObjectFactory();
			var decl = factory.New<JobDeclaration>();
			var buffer = new NotificationBuffer(null);
			var log = new LoggedSendsMessagesToAUCustoms(decl, buffer);
			var result = log.YesNoCancelQuery("Is this an amazing unit test or what?", "Unit Test");
			AssertEquals("Cancel by default", Customs.Business.YesNoCancel.Cancel, result);
			AssertEquals("1 error Log", true, buffer.HasErrors);
		}
	}
}
