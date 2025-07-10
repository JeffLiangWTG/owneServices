using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.Testing.SecurityOverride
{
	sealed class ReopenPeriodsSecurityOverriderProviderTest : TestCaseWithFactory
	{
		public void TestPromptForTemporaryAccessCore_NoOneInTheAuthorizedUsersOfReopenClosedPeriods()
		{
			SecurityTestObject.CreateTestUser(true, Env.Security.PeriodManagementReOpenPeriodLevel3.Code, "USR", loginUserName, loginPassword);
			ExecuteRequestLoginCredentials(loginUserName, loginPassword, new List<GlbStaff>());

			var loginForm = (LoginForm)ZFormModaliser.LastFormShownDialogForTest;
			AssertEquals("LastFormShownDialogForTest", "Security Override Login", loginForm.Text);
			AssertContains(@"You do not have the required security right to reopen period.
To override this security, a user with security right to [Reopen Period Third Level] must login. Please enter username and password details below.
The following users have the required security right. CargoWise has authorized the following people to reopen periods:
", loginForm.Message);

			AssertEquals("Error message will pop up here because the approved user are not in the list of authorized users", @"You do not have the appropriate security right to run this function.
If you require access to this function, ask your system administration to change your Staff or Group Security Rights to allow access to:
Manage > General Ledger > Period Management > Reopen Period", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPromptForTemporaryAccessCore_loginNameIsNotInUsersAuthorizedToReopenClosedPeriods()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "other user";

			var collection = new UsersAuthorizedToReopenClosedPeriodsCollection();
			collection.AddNew();
			collection[0].StaffPK = staff.PK;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			SecurityTestObject.CreateTestUser(true, Env.Security.None.Code, "USR", loginUserName, loginPassword);

			ExecuteRequestLoginCredentials(loginUserName, loginPassword, new List<GlbStaff> { staff });
			AssertEquals("An error message will pop up here because the authorized user does not include test user.", @"You do not have the appropriate security right to run this function.
If you require access to this function, ask your system administration to change your Staff or Group Security Rights to allow access to:
Manage > General Ledger > Period Management > Reopen Period", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPromptForTemporaryAccessCore_LoginAuthenticationFailed()
		{
			var errorPassword = "errorPass";

			SecurityTestObject.CreateTestUser(true, Env.Security.PeriodManagementReOpenPeriodLevel3.Code, "USR", loginUserName, loginPassword);
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "USR"));
			staff.GS_FullName = "test user";

			var collection = new UsersAuthorizedToReopenClosedPeriodsCollection();
			collection.AddNew();
			collection[0].StaffPK = staff.PK;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			ExecuteRequestLoginCredentials(loginUserName, errorPassword, new List<GlbStaff> { staff });
			AssertEquals("The expected error is the error username or password.", "The user name (or password) entered is incorrect or password is expired. Please re-enter the correct username and password.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPromptForTemporaryAccessCore_WithInactiveUser()
		{
			SecurityTestObject.CreateTestUser(true, Env.Security.PeriodManagementReOpenPeriodLevel3.Code, "USR", loginUserName, loginPassword);
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "USR"));
			staff.GS_FullName = "test user";
			staff.GS_IsActive = false;

			var collection = new UsersAuthorizedToReopenClosedPeriodsCollection();
			collection.AddNew();
			collection[0].StaffPK = staff.PK;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			ExecuteRequestLoginCredentials(loginUserName, loginPassword, new List<GlbStaff> { staff });
			AssertEquals("The expected error is an invalid user.", "Your login has been disabled. Please see your system administrator to re-activate your login.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPromptForTemporaryAccessCore_WithNoErrorMessages()
		{
			SecurityTestObject.CreateTestUser(true, Env.Security.PeriodManagementReOpenPeriodLevel3.Code, "USR", loginUserName, loginPassword);
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "USR"));
			staff.GS_FullName = "test user";

			var collection = new UsersAuthorizedToReopenClosedPeriodsCollection();
			collection.AddNew();
			collection[0].StaffPK = staff.PK;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			ExecuteRequestLoginCredentials(loginUserName, loginPassword, new List<GlbStaff> { staff });
			AssertEquals("No error message will pop up here because the authorized user includes test user.", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPromptForTemporaryAccessCore_UserIsNotIncludedInAuthorizedUsers()
		{
			SecurityTestObject.CreateTestUser(true, Env.Security.PeriodManagementReOpenPeriodLevel3.Code, "USR", loginUserName, loginPassword);
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "USR"));
			staff.GS_FullName = "test user";

			var collection = new UsersAuthorizedToReopenClosedPeriodsCollection();
			collection.AddNew();
			collection[0].StaffPK = staff.PK;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			ExecuteRequestLoginCredentials(loginUserName, loginPassword, new List<GlbStaff> { staff });
			var loginForm = (LoginForm)ZFormModaliser.LastFormShownDialogForTest;
			AssertEquals("LastFormShownDialogForTest", "Security Override Login", loginForm.Text);
			AssertEquals(@"You do not have the required security right to reopen period.
To override this security, a user with security right to [Reopen Period Third Level] must login. Please enter username and password details below.
The following users have the required security right. CargoWise has authorized the following people to reopen periods:
test user", loginForm.Message);
		}

		void ExecuteRequestLoginCredentials(string userName, string password, List<GlbStaff> staffs)
		{
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				((LoginForm)form).DoLoginForTest(userName, password);
				ZFormModaliser.ResultToReturnFromShowDialog = ZFormModaliser.ResultToReturnFromShowDialog != DialogResult.OK ? DialogResult.OK : DialogResult.Cancel;
			});
			var reopenPeriodsSecurityOverriderProvider = new ReopenPeriodsSecurityOverriderProvider(staffs);
			reopenPeriodsSecurityOverriderProvider.PromptForTemporaryAccessCore(Env.Security.PeriodManagementReOpenPeriodLevel3);
		}

		const string loginUserName = "test user";
		const string loginPassword = "pass";
	}
}
