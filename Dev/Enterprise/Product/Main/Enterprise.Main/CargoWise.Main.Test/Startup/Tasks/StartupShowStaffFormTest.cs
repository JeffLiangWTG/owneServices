using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class StartupShowStaffFormTest : TransactionedTestCase
	{
		public void TestTestShowStaffFormEveryXDaysWhenFrequencyIsVeryBig()
		{
			Env.Registry.StaffDetailsUpdateFrequency = 3650000;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			GlbStaff staffWithPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithPermissions.GS_ChangePasswordAtNextLogin = false;
			staffWithPermissions.GS_IsActive = true;
			AddSecurityToStaff(staffWithPermissions, Env.Security.StaffOwnDetails.Code, true);

			Factory.Save();

			var initialUserContext = Env.CurrentUserContext;
			using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissions.GS_LoginName, initialUserContext.Branch.PK, initialUserContext.Department.PK)))
			{
				AssertEquals("Staff With Permissions be logged in", Env.CurrentUser.PK, staffWithPermissions.PK);

				TimeSpan twentyDays = new TimeSpan(20, 0, 0, 0);

				DateTime twentyDaysEarlier = Env.Time.CurrentLocalDate.Subtract(twentyDays);

				Env.Registry.DateTimeStaffFormWasLastShown = twentyDaysEarlier;

				new StartupShowStaffForm().Execute();
				AssertEquals("Staff form should NOT have been shown today", twentyDaysEarlier, Env.Registry.DateTimeStaffFormWasLastShown);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestShowStaffFormEveryXDays()
		{
			var initialUserContext = Env.CurrentUserContext;

			Env.Registry.StaffDetailsUpdateFrequency = 30;
			Env.Security.StaffEdit.IsAllowed = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			GlbStaff staffWithPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithPermissions.GS_ChangePasswordAtNextLogin = false;
			staffWithPermissions.GS_IsActive = true;
			AddSecurityToStaff(staffWithPermissions, Env.Security.StaffOwnDetails.Code, true);
			AddSecurityToStaff(staffWithPermissions, Env.Security.StaffView.Code, true);

			Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.Staff).IsAllowed = true;

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissions.GS_LoginName, initialUserContext.Branch.PK, initialUserContext.Department.PK)))
			{
				AssertEquals("Staff With Permissions be logged in", Env.CurrentUser.PK, staffWithPermissions.PK);

				new StartupShowStaffForm().Execute();
				AssertStaffFormShownVeryRecentlyAndDispose();

				TimeSpan twentyDays = new TimeSpan(20, 0, 0, 0);
				TimeSpan thirtyDays = new TimeSpan(30, 0, 0, 0);

				DateTime twentyDaysEarlier = Env.Time.CurrentLocalDate.Subtract(twentyDays);
				DateTime thirtyDaysEarlier = Env.Time.CurrentLocalDate.Subtract(thirtyDays);

				Env.Registry.DateTimeStaffFormWasLastShown = twentyDaysEarlier;

				new StartupShowStaffForm().Execute();
				AssertEquals("Staff form should NOT have been shown today", twentyDaysEarlier, Env.Registry.DateTimeStaffFormWasLastShown);

				Env.Registry.DateTimeStaffFormWasLastShown = thirtyDaysEarlier;
				Env.Registry.StaffDetailsUpdateFrequency = 0;

				new StartupShowStaffForm().Execute();
				AssertEquals("Staff update frequency is 0, so form should not have been shown", thirtyDaysEarlier, Env.Registry.DateTimeStaffFormWasLastShown);

				Env.Registry.StaffDetailsUpdateFrequency = 30;

				new StartupShowStaffForm().Execute();
				AssertStaffFormShownVeryRecentlyAndDispose();
			}

			GlbStaff staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPermissions.GS_ChangePasswordAtNextLogin = false;
			staffWithoutPermissions.GS_IsActive = true;
			AddSecurityToStaff(staffWithoutPermissions, Env.Security.StaffOwnDetails.Code, false);
			AddSecurityToStaff(staffWithoutPermissions, Env.Security.StaffDetails.Code, false);
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, initialUserContext.Branch.PK, initialUserContext.Department.PK)))
			{
				AssertEquals("Staff Without Permissions be logged in", Env.CurrentUser.PK, staffWithoutPermissions.PK);

				new StartupShowStaffForm().Execute();
				AssertEquals("Staff has no permissions, form should NEVER have been shown", DateTime.MinValue, Env.Registry.DateTimeStaffFormWasLastShown);
			}
		}

		public void TestStaffFormIsNotShownForSystemAccountsEveryXDays()
		{
			var initialUserContext = Env.CurrentUserContext;

			// Arrange
			Env.Registry.StaffDetailsUpdateFrequency = 30;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			GlbStaff staffSystemAccount = Factory.NewWithValidTestData<GlbStaff>();
			staffSystemAccount.GS_ChangePasswordAtNextLogin = false;
			staffSystemAccount.GS_IsActive = true;
			staffSystemAccount.GS_IsSystemAccount = true;
			AddSecurityToStaff(staffSystemAccount, Env.Security.StaffOwnDetails.Code, true);
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staffSystemAccount.GS_LoginName, initialUserContext.Branch.PK, initialUserContext.Department.PK)))
			{
				AssertEquals("New User should be logged in", Env.CurrentUser.PK, staffSystemAccount.PK);

				new StartupShowStaffForm().Execute();
				AssertEquals("Staff is System Account, form should NEVER have been shown", DateTime.MinValue, Env.Registry.DateTimeStaffFormWasLastShown);
			}
		}

		public void TestStaffFormIsNotShownForRoboticAccountsEveryXDays()
		{
			// Arrange
			var initialUserContext = Env.CurrentUserContext;

			Env.Registry.StaffDetailsUpdateFrequency = 30;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			GlbStaff botAccount = Factory.NewWithValidTestData<GlbStaff>();
			botAccount.GS_ChangePasswordAtNextLogin = false;
			botAccount.GS_IsActive = true;
			botAccount.GS_IsRobot = true;
			AddSecurityToStaff(botAccount, Env.Security.StaffOwnDetails.Code, true);
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(botAccount.GS_LoginName, initialUserContext.Branch.PK, initialUserContext.Department.PK)))
			{
				// Act
				new StartupShowStaffForm().Execute();

				// Assert
				AssertEquals("Staff is Robotic Account, form should NEVER have been shown", DateTime.MinValue, Env.Registry.DateTimeStaffFormWasLastShown);
			}
		}

		void AssertStaffFormShownVeryRecentlyAndDispose()
		{
			TimeSpan formShownTime = Env.Time.CurrentLocalDateTime - Env.Registry.DateTimeStaffFormWasLastShown;
			Assert("Form should have been shown in last two minutes", formShownTime < new TimeSpan(0, 2, 0));
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Your staff profile is displayed every"));

			using (var lastForm = ZApplication.GetOpenForms().OfType<GlbStaffForm>().FirstOrDefault())
			{
				AssertNotNull(lastForm);
				AssertEquals("Last shown form should be of type GlbStaffForm", typeof(GlbStaffForm), lastForm.GetType());
				AssertEquals("Form should not have new button", ODisplayMode.NewSaved, lastForm.DisplayMode);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		void AddSecurityToStaff(GlbStaff staff, ZString securityRight, bool isAllowed)
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_GS = staff.PK;
			security.GU_SecurityRight = securityRight;
			security.GU_SecurityItemIsAllowed = isAllowed;
			staff.StaffSecurityPermissionsCollection.Add(security);
		}

		protected override void SetUp()
		{
			Factory = new BusinessObjectFactory();
		}

		BusinessObjectFactory Factory;
	}
}
