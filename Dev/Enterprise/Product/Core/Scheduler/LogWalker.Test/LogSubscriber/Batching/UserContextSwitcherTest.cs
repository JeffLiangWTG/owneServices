using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Testing
{
	class UserContextSwitcherTest : TestCaseWithFactory
	{
		public void TestSetDoesNotReportErrors()
		{
			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (UserContextSwitcher.Build(
					Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, User.SupportUserName)),
					Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, Env.CurrentBranchPK)),
					Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, Env.CurrentDepartmentPK)),
					"DummyMessage").Set(null))
				{
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}
		}
	}
}
