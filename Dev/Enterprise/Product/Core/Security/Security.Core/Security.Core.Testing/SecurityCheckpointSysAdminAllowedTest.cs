using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.Core.Testing
{
	public class SecurityCheckpointSysAdminAllowedTest : TestCaseWithFactory
	{
		public void TestIsAllowedDoesntCrashWhenSecurityInitialisedForGroup()
		{
			RunIsAllowedTest<GlbGroup>();
		}

		public void TestIsAllowedDoesntCrashWhenSecurityInitialisedForStaff()
		{
			RunIsAllowedTest<GlbStaff>();
		}

		public void TestSysAdminProhibitions()
		{
			var controller = Factory.New<GlbStaff>();
			controller.GS_Code = "ES9";
			controller.GS_LoginName = "ES9.LoginName";
			controller.GS_IsController = true;
			controller.GS_IsOperational = false;
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(controller.GS_LoginName))
			{
				SecurityCore security = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				foreach (SecurityCheckpoint checkpoint in security.AllLoadedCheckPoints)
				{
					if ((checkpoint.Parent != null) && (checkpoint.Parent.Parent != null))
					{
						if (security.System.IsAncestorOf(checkpoint) || security.UserAdmin.IsAncestorOf(checkpoint) ||
							// these 3 used to be under System and are allowed for non-operational
							security.PrintingSection.IsAncestorOf(checkpoint) || security.ReportSection.IsAncestorOf(checkpoint) || security.EmailSection.IsAncestorOf(checkpoint)
						)
						{
							AssertEquals(checkpoint.ToString() + " should be allowed.", true, checkpoint.IsAllowed);
						}
						else
						{
							AssertEquals(checkpoint.ToString() + " should not be allowed.", false, checkpoint.IsAllowed);
						}
					}
				}

				//Now demonstrate that when we check securities on an unrelated user who is not SysAdmin/NonOperationalOnly, all of their securities are correctly denied.

				AssertEquals("security.Staff should be allowed.", true, security.Staff.IsAllowed);

				var unrelatedStaff = Factory.NewWithValidTestData<GlbStaff>();
				Factory.Save();

				security = new SecurityCore(null, unrelatedStaff.PK.ToGuid(), EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				Assert("CurrentUser is still non-operational", !EnvProxy.Instance.CurrentUser.IsOperational);
				Assert("unrelatedStaff is not non-operational", unrelatedStaff.GS_IsOperational);
				AssertEquals("security.Staff should not be allowed.", false, security.Staff.IsAllowed);
			}
		}

		void RunIsAllowedTest<TStaffOrGroup>() where TStaffOrGroup : BusinessObject
		{
			var staffOrGroup = Factory.NewWithValidTestData<TStaffOrGroup>();
			Factory.Save();

			var security = new SecurityCore(null, staffOrGroup, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

			var checkpoint = new SecurityCheckpointNonOperationalAllowed(null, null, null, security);

			AssertNoExceptionThrown(() => Assert(checkpoint.IsAllowed));
		}
	}
}
