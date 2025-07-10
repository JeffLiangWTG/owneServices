using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.DocumentEngine.Scheduler.Business.ReportSerializationInfo;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	public class TestUserSwitcher : TestCaseWithFactory
	{
		public void TestUserSwitching()
		{
			var originalUser = Env.CurrentUser.LoginName;
			var originalBranch = GlbBranch.CurrentBranch.PK;
			var originalDepartment = GlbDepartment.CurrentDepartment.PK;

			var nonCurrentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, originalBranch));
			AssertNotNull("Precondition: There must be a branch", nonCurrentBranch);

			var nonCurrentDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, originalDepartment));
			AssertNotNull("Precondition: There must be a department", nonCurrentDepartment);

			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, originalUser));
			AssertNotNull("Precondition: There must be a staff", staff);

			var info = new ReportSerializationInfo(staff.GS_LoginName.ToString(), nonCurrentBranch.PK, nonCurrentDepartment.PK);
			using (new UserSwitcher(info, staff.GS_Code))
			{
				AssertEquals("Should change user", staff.GS_LoginName, Env.CurrentUser.LoginName);
				AssertEquals("Should not change current branch", originalBranch, GlbBranch.CurrentBranch.PK);
				AssertEquals("Should change department", nonCurrentDepartment.PK, GlbDepartment.CurrentDepartment.PK);
			}

			var nonExistingDepartment = ZGuid.NewZGuid();
			AssertNull("Precondition: No department should exist for this PK", Factory.Load<GlbDepartment>(nonExistingDepartment));

			info = new ReportSerializationInfo(staff.GS_LoginName.ToString(), nonCurrentBranch.PK, nonExistingDepartment);
			using (new UserSwitcher(info, staff.GS_Code))
			{
				AssertEquals("Should change user", staff.GS_LoginName, Env.CurrentUser.LoginName);
				AssertEquals("Should not change current branch", originalBranch, GlbBranch.CurrentBranch.PK);
				AssertEquals("Should fall back to current department", originalDepartment, GlbDepartment.CurrentDepartment.PK);
			}
		}

		public void TestExceptionOnWrongUser()
		{
			const string user = "_This_User_Should_Not_Exist_";
			const string code = "_This_Code_Should_Not_Exist_";
			var info = new ReportSerializationInfo(user, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			try
			{
				using (new UserSwitcher(info, code))
				{ }
				Fail("InvalidPrintUserException should have been thrown here.");
			}
			catch (ReportSerializationInfo.InvalidPrintUserException ex)
			{
				AssertEquals("_This_Code_Should_Not_Exist_ or _This_User_Should_Not_Exist_", ex.User);
				AssertEquals(false, ex.IsInactive);
			}
		}

		public void TestExceptionOnInactiveUser()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "INA";
			user.GS_LoginName = "Inactive staff";
			Factory.Save();
			var info = new ReportSerializationInfo(user.GS_LoginName, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			try
			{
				user.GS_IsActive = false;
				Factory.Save();
				using (new UserSwitcher(info, user.GS_Code))
				{ }
				Fail("InvalidPrintUserException should have been thrown here.");
			}
			catch (ReportSerializationInfo.InvalidPrintUserException ex)
			{
				AssertEquals(user.GS_LoginName, ex.User);
				AssertEquals(true, ex.IsInactive);
			}
		}
	}
}
