using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	class UserContextScopeManagerTest : TestCaseWithFactory
	{
		public void TestEnterUserContextDoesNotReportErrors()
		{
			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (new UserContextScopeManager().EnterUserContext(new UserContext(User.SupportUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}
		}
	}
}
