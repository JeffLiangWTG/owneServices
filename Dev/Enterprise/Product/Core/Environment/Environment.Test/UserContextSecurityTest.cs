using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment.Testing
{
	sealed class UserContextSecurityTest : TestCaseWithFactory
	{
		public void TestContextWillResetSecurity()
		{
			var staff1 = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
			var staff2 = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
			Factory.Save();

			var context1 = new UserContext((IUser)staff1, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			var context2 = new UserContext((IUser)staff2, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			var contextAndSecurity = new UserContextSecurity();
			contextAndSecurity.SetMasterUserContext(context1);

			AssertEquals("PRE: Should be using the staff1 context", staff1.PK.ToGuid(), contextAndSecurity.Security.UserPK);

			contextAndSecurity.SetCurrentThreadUserContext(context2, isRevert: false);
			AssertEquals("Should have the updated security PK", staff2.PK.ToGuid(), contextAndSecurity.Security.UserPK);
		}

		public void TestContextRevertedMoreTimesThenSetIsReported()
		{
			var context1 = new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			var contextAndSecurity = new UserContextSecurity();
			contextAndSecurity.SetCurrentThreadUserContext(context1, isRevert: false);
			contextAndSecurity.SetCurrentThreadUserContext(context1, isRevert: true);
			contextAndSecurity.SetCurrentThreadUserContext(context1, isRevert: true);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(contextAndSecurity.LastContextUpdateStackTrace.ToString(), ErrorReporter.LastMessageReported);
			AssertEquals($"Tried to revert UserContext more times than it was set.  CompanyCode: {Env.CurrentCompany.Code}, BranchCode: {Env.CurrentBranch.Code}, UserInitials: {Env.CurrentUser.Initials}.", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}
	}
}
