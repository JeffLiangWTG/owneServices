using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.Core.Testing
{
	public class SecurityCheckpointWebUserAllowedTest : TestCaseWithFactory
	{
		public void TestIsAllowedDoesntCrashWhenSecurityInitialisedForGroup()
		{
			RunIsAllowedTest<GlbGroup>();
		}

		public void TestIsAllowedDoesntCrashWhenSecurityInitialisedForStaff()
		{
			RunIsAllowedTest<GlbStaff>();
		}

		public void TestWebUserAllowed()
		{
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				SecurityCore security = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				foreach (SecurityCheckpoint checkpoint in security.AllLoadedCheckPoints)
				{
					if ((checkpoint is SecurityCheckpointWebUserAllowed))
					{
						AssertEquals(checkpoint.ToString() + " should be allowed.", true, checkpoint.IsAllowed);
					}
				}

				//Now demonstrate that when we check securities on an unrelated user who is not WebUser, all of their securities are correctly denied.

				AssertEquals("security.MaintainShipmentShipments should be allowed.", true, security.MaintainShipmentShipments.IsAllowed);

				var unrelatedStaff = Factory.NewWithValidTestData<GlbStaff>();
				Factory.Save();

				security = new SecurityCore(null, unrelatedStaff.PK.ToGuid(), EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				Assert("CurrentUser is still webuser", EnvProxy.Instance.CurrentUser.IsWebUser);
				AssertEquals("security.MaintainShipmentShipments should not be allowed.", false, security.MaintainShipmentShipments.IsAllowed);
			}
		}

		void RunIsAllowedTest<TStaffOrGroup>() where TStaffOrGroup : BusinessObject
		{
			var staffOrGroup = Factory.NewWithValidTestData<TStaffOrGroup>();
			Factory.Save();

			var security = new SecurityCore(null, staffOrGroup, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

			var checkpoint = new SecurityCheckpointWebUserAllowed(null, null, null, security);

			AssertNoExceptionThrown(() => Assert(checkpoint.IsAllowed));
		}
	}
}
