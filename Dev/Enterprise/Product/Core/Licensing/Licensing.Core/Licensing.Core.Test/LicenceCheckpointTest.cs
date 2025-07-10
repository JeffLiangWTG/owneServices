using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class LicenceCheckpointTest : TransactionedTestCase
	{
		public void TestGetDefaultLicenceValue()
		{
			var licences = new Licences();
			var checkpoint = new LicenceCheckpoint("Name1", "DisplayName1", licences, licences.Core, LicenceModuleCategories.Codes.None);
			AssertEquals("normal checkpoint", ModuleLicenceType.ODM, checkpoint.GetDefaultLicenceValue());

			checkpoint.IsDefaultTransactional = true;
			AssertEquals("transactional checkpoint", ModuleLicenceType.CPT, checkpoint.GetDefaultLicenceValue());
		}

		[TestDate(2016, 1, 1)]
		[TestUtcOffset(0, 0, 0)]
		public void TestLogin_ShouldCreateConsumptionLog()
		{
			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			productRegistrationKey.CurrentBillingTimeZoneUtcOffsetForTest = 0.0d;
			productRegistrationKey.NextBillingTimeZoneUtcOffsetForTest = 11.0d;
			productRegistrationKey.NextUtcOffsetEffectiveTimeUtcForTest = new DateTime(2016, 1, 31, 13, 0, 0);

			TestDateAttribute.Date = new DateTime(2016, 1, 1);

			var licences = CreateLicencesForUser("Z3");
			var checkpointWithLog = new LicenceCheckpoint("Name1", "DisplayName1", licences, licences.Core, LicenceModuleCategories.Codes.None, shouldCreateConsumptionLogOnLogin: true);
			var checkpointWithoutLog = new LicenceCheckpoint("Name2", "DisplayName2", licences, licences.Core, LicenceModuleCategories.Codes.None, shouldCreateConsumptionLogOnLogin: false);
			checkpointWithLog.LicenceType = ModuleLicenceType.ODM;
			checkpointWithoutLog.LicenceType = ModuleLicenceType.ODM;

			AssertEquals(LicenceLoginResponse.Granted, checkpointWithLog.Login(new TestLicensedComponent()));
			AssertEquals("ConsumptionLogCreated", true, checkpointWithLog.ConsumptionLogCreated);
			checkpointWithLog.ConsumptionLogCreated = false;
			AssertEquals(LicenceLoginResponse.Granted, checkpointWithLog.Login(new TestLicensedComponent()));
			AssertEquals("No more consumption log is created", false, checkpointWithLog.ConsumptionLogCreated);

			AssertEquals(LicenceLoginResponse.Granted, checkpointWithoutLog.Login(new TestLicensedComponent()));
			AssertEquals("ConsumptionLogCreated", false, checkpointWithoutLog.ConsumptionLogCreated);

			checkpointWithLog.ConsumptionLogCreated = false;
			TestDateAttribute.Date = new DateTime(2016, 1, 31, 12, 59, 59);
			checkpointWithLog.Login(new TestLicensedComponent());
			AssertEquals("Consumption log is created", true, checkpointWithLog.ConsumptionLogCreated);

			checkpointWithLog.ConsumptionLogCreated = false;
			TestDateAttribute.Date = new DateTime(2016, 1, 31, 13, 0, 0);
			checkpointWithLog.Login(new TestLicensedComponent());
			AssertEquals("Consumption log is created - billing time zone offset applies", true, checkpointWithLog.ConsumptionLogCreated);

			checkpointWithLog.ConsumptionLogCreated = false;
			TestDateAttribute.Date = new DateTime(2016, 2, 1, 0, 0, 0);
			checkpointWithLog.Login(new TestLicensedComponent());
			AssertEquals("Consumption log is not created - day in billing time zone hasn't changed", false, checkpointWithLog.ConsumptionLogCreated);

			checkpointWithLog.ConsumptionLogCreated = false;
			TestDateAttribute.Date = new DateTime(2016, 2, 1, 13, 0, 0);
			checkpointWithLog.Login(new TestLicensedComponent());
			AssertEquals("Consumption log is created - day in billing time zone has changed", true, checkpointWithLog.ConsumptionLogCreated);

			productRegistrationKey.CurrentBillingTimeZoneUtcOffsetForTest = 11.0d;
			productRegistrationKey.NextBillingTimeZoneUtcOffsetForTest = 10.0d;
			productRegistrationKey.NextUtcOffsetEffectiveTimeUtcForTest = new DateTime(2016, 4, 2, 16, 0, 0);

			checkpointWithLog.ConsumptionLogCreated = false;
			TestDateAttribute.Date = new DateTime(2016, 4, 2, 13, 0, 0);
			checkpointWithLog.Login(new TestLicensedComponent());
			AssertEquals("Consumption log is created", true, checkpointWithLog.ConsumptionLogCreated);

			checkpointWithLog.ConsumptionLogCreated = false;
			TestDateAttribute.Date = new DateTime(2016, 4, 3, 13, 0, 0);
			checkpointWithLog.Login(new TestLicensedComponent());
			AssertEquals("Consumption log is not created - new offset applies and day in billing time zone hasn't changed", false, checkpointWithLog.ConsumptionLogCreated);

			checkpointWithLog.ConsumptionLogCreated = false;
			TestDateAttribute.Date = new DateTime(2016, 4, 3, 14, 0, 0);
			checkpointWithLog.Login(new TestLicensedComponent());
			AssertEquals("Consumption log is created", true, checkpointWithLog.ConsumptionLogCreated);
		}

		public void TestLogin_LicenceTypeCPT()
		{
			SetDatabaseType(DatabaseTypes.Codes.Production);
			ILicensedComponent licensedComponent = new TestLicensedComponent();
			var licences = CreateLicencesForUser("Z3");
			var checkpoint = new LicenceCheckpoint("Name1", "DisplayName1", licences, licences.Core, LicenceModuleCategories.Codes.None, shouldCreateConsumptionLogOnLogin: true) { IsDefaultTransactional = true };

			AssertEquals("Login allowed despite 0 user count", LicenceLoginResponse.Granted, checkpoint.Login(licensedComponent));

			AssertEquals("Another login allowed despite zero user limit", LicenceLoginResponse.Granted, checkpoint.Login(licensedComponent));
		}

		#region System Expiry

		[TestDate(2005, 12, 03, 18, 0, 0)] // Before date and time of expiry
		public void TestSystemExpirationWeekends_NoExpiry()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetSystemExpiryDate(new DateTime(2005, 12, 3, 0, 0, 0));

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
			AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
		}

		public static void SetSystemExpiryDate(DateTime expiry)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.SystemExpiryDateForTest = expiry;
		}

		public static void SetDatabaseType(string type)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = type;
		}

		[TestDate(2005, 12, 05, 10, 0, 0)] // Before date and time of expiry
		public void TestSystemExpirationWeekendsMondayMorning_NoExpiry()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetSystemExpiryDate(new DateTime(2005, 12, 3, 0, 0, 0));

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
			AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
		}

		[TestDate(2005, 12, 05, 16, 00, 00)] // Default date and time of expiry
		public void TestSystemExpirationWeekends_ExpiresOn()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetSystemExpiryDate(new DateTime(2005, 12, 3, 0, 0, 0));

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("Core Login()", LicenceLoginResponse.Denied, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", false, licences1.Core.IsLoggedIn);
			Assert("LastReason", licences1.Core.LastReasonForNotAllowing.IndexOf("All users will be locked out of your system until this is resolved") > -1);
		}

		[TestDate(2005, 12, 05, 16, 00, 00)] // Default date and time of expiry
		public void TestSystemStabilityErrorMessage()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetSystemExpiryDate(new DateTime(2005, 12, 3, 0, 0, 0));

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("Core Login()", LicenceLoginResponse.Denied, licences1.Core.Login(licensedComponent));

			AssertEquals(string.Format(@"Your database is no longer in sync with WiseTech Global. All users will be locked out of your system until this is resolved. Database Name: {0}, Server Name: {1}
This indicates that your Process Controller is not running or that it is blocked from reaching the WiseTech Global registration web service.
You can login to diagnose this using a login with Non-Operational permissions. Please access Maintain > System > Service Tasks to check the Process Controller(s). Please access Help > Register/Unregister Product to check the registration.",
					Db.DatabaseName, Db.ServerName), licences1.Core.LastReasonForNotAllowing);
		}

		[TestDate(2005, 12, 05, 15, 59, 59)] // Before date and time of expiry
		public void TestSystemExpiration_NoExpiry()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();
			licences1.Core.LicenceType = ModuleLicenceType.ODM;

			AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
			AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
		}

		[TestDate(2005, 12, 05, 16, 00, 00)] // Default date and time of expiry
		public void TestSystemExpiration_ExpiresOn()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();
			licences1.Core.LicenceType = ModuleLicenceType.ODM;

			AssertEquals("Core Login()", LicenceLoginResponse.Denied, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", false, licences1.Core.IsLoggedIn);
			Assert("LastReason", licences1.Core.LastReasonForNotAllowing.IndexOf("All users will be locked out of your system until this is resolved") > -1);
		}

		[TestDate(2005, 12, 15)] // After Default date of expiry
		public void TestSystemExpiration_ExpiresAfter()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();
			licences1.Core.LicenceType = ModuleLicenceType.ODM;

			AssertEquals("Core Login()", LicenceLoginResponse.Denied, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", false, licences1.Core.IsLoggedIn);
			Assert("LastReason", licences1.Core.LastReasonForNotAllowing.IndexOf("All users will be locked out of your system until this is resolved") > -1);
		}

		[TestDate(2005, 12, 05)] // Default date of expiry
		public void TestSystemExpiration_BatchProcessorUser()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.ServiceUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				ILicensedComponent licensedComponent = new TestLicensedComponent();
				Licences licences1 = new Licences();
				licences1.Core.LicenceType = ModuleLicenceType.ODM;

				AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
				AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
				AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
			}
		}

		[TestDate(2005, 12, 05)] // Default date of expiry
		public void TestSystemExpiration_PostMasterUser()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.PostMasterUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				ILicensedComponent licensedComponent = new TestLicensedComponent();
				Licences licences1 = new Licences();
				licences1.Core.LicenceType = ModuleLicenceType.ODM;

				AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
				AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
				AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
			}
		}

		[TestDate(2005, 12, 05)] // Default date of expiry
		public void TestSystemExpiration_EducationLicence()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetDatabaseType(DatabaseTypes.Codes.Education);

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
			AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
		}

		[TestDate(2005, 12, 05)] // Default date of expiry
		public void TestSystemExpiration_TrainingLicence()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetDatabaseType(DatabaseTypes.Codes.Training);

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
			AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
		}

		[TestDate(2005, 12, 05)] // Default date of expiry
		public void TestSystemExpiration_DemoLicence()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetDatabaseType(DatabaseTypes.Codes.Demo);

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
			AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
		}

		[TestDate(2005, 12, 05, 16, 0, 0)] // Default date of expiry
		public void TestSystemExpiration_ProductionLicence()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetDatabaseType(DatabaseTypes.Codes.Production);

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("Core Login()", LicenceLoginResponse.Denied, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", false, licences1.Core.IsLoggedIn);
			Assert("LastReason", licences1.Core.LastReasonForNotAllowing.IndexOf("All users will be locked out of your system until this is resolved") > -1);
		}

		[TestDate(2005, 12, 05, 16, 0, 0)] // Default date of expiry
		public void TestSystemExpiration_TestLicence()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetDatabaseType(DatabaseTypes.Codes.Test);

			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();
			licences1.Core.LicenceType = ModuleLicenceType.ODM;

			AssertEquals("Core Login()", LicenceLoginResponse.Denied, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", false, licences1.Core.IsLoggedIn);
			Assert("LastReason", licences1.Core.LastReasonForNotAllowing.IndexOf("All users will be locked out of your system until this is resolved") > -1);
		}

		[TestDate(2005, 12, 15)] // After Default date of expiry
		public void TestSystemExpiration_TestStatic()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = false;
			ILicensedComponent licensedComponent = new TestLicensedComponent();
			Licences licences1 = new Licences();
			licences1.Core.LicenceType = ModuleLicenceType.ODM;

			AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
			AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
			AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
		}

		[TestDate(2005, 12, 05)] // Default date of expiry
		public void TestSystemExpiration_DemoCompanyAllowedLogin()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			Guid demoBranchPK = new Guid("2FDBA7FB-60BA-4A03-8336-0DEFAC4F9673");

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, demoBranchPK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				ILicensedComponent licensedComponent = new TestLicensedComponent();
				Licences licences1 = new Licences();
				licences1.Core.LicenceType = ModuleLicenceType.ODM;

				AssertEquals("Core Login()", LicenceLoginResponse.Granted, licences1.Core.Login(licensedComponent));
				AssertEquals("IsLoggedIn", true, licences1.Core.IsLoggedIn);
				AssertEquals("LastReason", "", licences1.Core.LastReasonForNotAllowing);
			}
		}

		#endregion

		#region Unique Module Codes

		public void TestAllModuleCodesAreUnique()
		{
			LicenceCheckpoint[] checkpoints = ((Licences)EnvProxy.Instance.Licence).GetAllCheckpoints();

			StringBuilder builder = new StringBuilder();
			foreach (LicenceCheckpoint outerCheckpoint in checkpoints)
			{
				foreach (LicenceCheckpoint innerCheckpoint in checkpoints)
				{
					if (outerCheckpoint.Name == innerCheckpoint.Name && !outerCheckpoint.Equals(innerCheckpoint))
					{
						builder.Append(String.Format("The Module Code {0} is being used by two LicenceCheckpoints \"{1}\" and \"{2}\"\n", innerCheckpoint.Name, innerCheckpoint.DisplayName, outerCheckpoint.DisplayName));
					}
				}
			}

			AssertEquals("There should NOT be any duplicate modules", "", builder.ToString());
		}

		#endregion

		#region Login

		public void TestLicenceCheckpointLoggedInGetsSet()
		{
			TestLicensedComponent module1 = new TestLicensedComponent();
			Licences licences1 = new Licences();

			AssertEquals("LicenceCheckpointLoggedIn", 0, module1.LicensedComponentManager.CheckpointCount);
			licences1.Core.Login(module1);
			AssertEquals("LicenceCheckpointLoggedIn", 1, module1.LicensedComponentManager.CheckpointCount);
			Assert("LicenceCheckpointLoggedIn contains Core", module1.LicensedComponentManager.ContainsCheckpoint(licences1.Core));

			licences1.Core.Logout(module1);
			AssertEquals("LicenceCheckpointLoggedIn", 0, module1.LicensedComponentManager.CheckpointCount);
		}

		public void TestLoginUsingDifferentCompanies()
		{
			ILicensedComponent module1 = new TestLicensedComponent();

			var user1 = NewUser("Z1");
			Licences licences1 = new Licences(user1);
			licences1.Core.LicenceType = ModuleLicenceType.ODM;

			var user2 = NewUser("Z2");
			Licences licences2 = new Licences(user2);
			licences2.Core.LicenceType = ModuleLicenceType.ODM;

			try
			{
				AssertEquals("Login in User1", LicenceLoginResponse.Granted, licences1.Core.Login(module1));
				AssertEquals("Login in User2 should still be allowed because User2 is logging into another company", LicenceLoginResponse.Granted, licences2.Core.Login(module1));
			}
			finally
			{
				licences1.Core.Logout(module1);
				licences2.Core.Logout(module1);
			}
		}

		public void TestLoginOfLicensedComponent()
		{
			var module1 = new TestLicensedComponent();
			var module2 = new TestLicensedComponent();
			var module3 = new TestLicensedComponent();
			var module4 = new TestLicensedComponent();

			Licences licences1 = CreateLicencesForUser("U1");
			Licences licences2 = CreateLicencesForUser("U2");

			var core1 = licences1.Core;
			var core2 = licences2.Core;
			var nonCore1 = licences1.Accountant;
			var nonCore2 = licences2.Accountant;

			AssertEquals("LicenceCheckpointLoggedIn", 0, module1.LicensedComponentManager.CheckpointCount);
			AssertEquals(LicenceLoginResponse.Granted, core1.Login(module1));
			AssertEquals("LicenceCheckpointLoggedIn", 1, module1.LicensedComponentManager.CheckpointCount);

			AssertEquals(LicenceLoginResponse.Granted, core2.Login(module1));
			AssertEquals("LicenceCheckpointLoggedIn", 2, module1.LicensedComponentManager.CheckpointCount);

			core1.Logout(module1);
			core2.Logout(module1);

			AssertEquals("LicenceCheckpointLoggedIn", 0, module1.LicensedComponentManager.CheckpointCount);

			AssertEquals(LicenceLoginResponse.Granted, nonCore1.Login(module1));
			AssertEquals("LicenceCheckpointLoggedIn", 1, module1.LicensedComponentManager.CheckpointCount);

			AssertEquals(LicenceLoginResponse.Granted, nonCore2.Login(module1));
			AssertEquals("LicenceCheckpointLoggedIn", 2, module1.LicensedComponentManager.CheckpointCount);

			nonCore1.Logout(module1);
			nonCore2.Logout(module1);
			AssertEquals("LicenceCheckpointLoggedIn", 0, module1.LicensedComponentManager.CheckpointCount);

			AssertEquals("Login for User1 module2", LicenceLoginResponse.Granted, core1.Login(module2));
			AssertEquals("Login for User2", LicenceLoginResponse.Granted, core2.Login(module2));

			AssertEquals("Login for User1 module1", LicenceLoginResponse.Granted, core1.Login(module1));
			AssertEquals("Login for User1 module1", LicenceLoginResponse.Granted, core1.Login(module1)); //login twice shouldn't cause problem
			AssertEquals("Login for User2", LicenceLoginResponse.Granted, core2.Login(module2));

			AssertEquals("Login for User1 module3", LicenceLoginResponse.Granted, nonCore1.Login(module3));
			AssertEquals("Login for User2", LicenceLoginResponse.Granted, nonCore2.Login(module3));

			core1.Logout(module1);
			core1.Logout(module2);
			core1.Logout(module2); // logout twice shouldn't cause problem
			AssertEquals("Login for User2", LicenceLoginResponse.Granted, core2.Login(module4));

			core1.Logout(module2);
			core1.Logout(module1);
			core1.Logout(module3);
			core2.Logout(module2);
			core2.Logout(module4);

			nonCore2.Logout(module3);
			nonCore2.Logout(module4);
		}

		public void TestAlwaysAllow()
		{
			Licences licences1 = new Licences();
			AssertEquals("AlwaysAllow", LicenceLoginResponse.Granted, licences1.AlwaysAllow.Login(new TestLicensedComponent()));
		}

		public void TestLoginShouldNotContinueToHoldTheComponentReference()
		{
			var checkpoint = new Licences().Accountant;

			var componentReference2 = CallInItsOwnScope(() =>
			{
				ILicensedComponent licenceComponent = new TestLicensedComponent();
				var componentReference = new WeakReference(licenceComponent, true);

				AssertEquals("Login", LicenceLoginResponse.Granted, checkpoint.Login(licenceComponent));

				licenceComponent = null;
				return componentReference;
			});

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertEquals("Is component still referenced?", false, componentReference2.IsAlive);
		}

		public void TestLoginFailureShouldNotContinueToHoldTheComponentReference()
		{
			var checkpoint = new Licences().Accountant;
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			SetSystemExpiryDate(new DateTime(2005, 12, 3, 0, 0, 0));

			var componentReference2 = CallInItsOwnScope(() =>
			{
				ILicensedComponent licenceComponent = new TestLicensedComponent();
				var componentReference = new WeakReference(licenceComponent, true);

				AssertEquals("Login", LicenceLoginResponse.Denied, checkpoint.Login(licenceComponent));
				AssertEquals("Is component referenced?", true, componentReference.IsAlive);

				licenceComponent = null;
				return componentReference;
			});

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertEquals("Is component still referenced?", false, componentReference2.IsAlive);
		}

		T CallInItsOwnScope<T>(Func<T> getter)
		{
			return getter();
		}

		[TestDate(2005, 12, 06)]
		public void TestControllerLoginHasExpired()
		{
			var user1 = NewUser("U1", true, "zz", true);
			Licences licences1 = new Licences(user1);
			AssertEquals("Pre", true, LicenceExpiryCheck.Create().SystemHasExpired);
			ILicensedComponent licensedComponent = new TestLicensedComponent();
			licences1.Core.Login(licensedComponent);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Checkpoints

		public void TestGetAllCheckpointsReturnsLicences()
		{
			Licences licences1 = new Licences();
			LicenceCheckpoint[] checkpoints = licences1.GetAllCheckpoints();
			AssertNotNull("Licence checkpoints returned", checkpoints);
		}

		#endregion

		public void TestModuleLicenceTypeDescription()
		{
			var user = NewUser("Z3");
			Licences licences = new Licences(user);

			LicenceTypes typeList = new LicenceTypes();
			foreach (string licenceType in Enum.GetNames(typeof(ModuleLicenceType)))
			{
				licences.Core.LicenceType = (ModuleLicenceType)Enum.Parse(typeof(ModuleLicenceType), licenceType);
				AssertEquals(typeList.GetDescriptionFromCode(licenceType), licences.Core.ModuleLicenceTypeDescription);
			}

			licences.Core.LicenceType = (ModuleLicenceType)(-298238); // arbitrary number not assigned to the enum members
			AssertEquals("Unknown", licences.Core.ModuleLicenceTypeDescription);
		}

		#region Implementation

		#region Test Classes

		internal class TestLicensedComponent : ILicensedComponent
		{
			#region ILicensedComponent Members

			public LicensedComponentManager LicensedComponentManager
			{
				get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
			}

			IDisposable ILicensedComponent.LicensedComponentManager
			{
				get
				{
					if (fLicensedComponentManager == null)
					{
						fLicensedComponentManager = new LicensedComponentManager(this);
					}
					return fLicensedComponentManager;
				}
			}
			LicensedComponentManager fLicensedComponentManager;

			#endregion
		}

		#endregion

		#region Helper Methods/Properties

		Licences CreateLicencesForUser(string code, bool isSystemUser = false)
		{
			var licences = new Licences(NewUser(code, isSystemUser));
			return licences;
		}

		IUser NewUser(string code)
		{
			return NewUser(code, false);
		}

		IUser NewUser(string code, bool isSystemUser)
		{
			return NewUser(code, isSystemUser, code);
		}

		IUser NewUser(string code, bool isSystemUser, string loginName)
		{
			return NewUser(code, isSystemUser, loginName, false);
		}

		IUser LoadUserWithLoginName(string loginName)
		{
			return (IUser)Factory.LoadTop1(ObjectFactory.GetType<IGlbStaff>(), new ZQuery(GlbStaffSchema.GS_LoginName, loginName));
		}

		IUser NewUser(string code, bool isSystemUser, string loginName, bool isController)
		{
			var user = LoadUserWithLoginName(loginName);
			if (user == null)
			{
				var staff = (IGlbStaff)Factory.New(ObjectFactory.GetType<IGlbStaff>());
				staff.GS_Code = code;
				staff.GS_LoginName = loginName;
				staff.GS_IsSystemAccount = isSystemUser;
				staff.GS_IsController = isController;
				user = staff;
			}

			return user;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion

		#region Setup

		protected override void TearDown()
		{
			LicenceCheckpoint.ShouldCheckForSystemExpiry = false;
			base.TearDown();
		}

		#endregion

		#endregion
	}
}
