using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ZArchitecture.Web.Business.ServiceTasks.Testing
{
	[TestedType(typeof(ClearOutdatedLoginAttemptsServiceTask))]
	sealed class ClearOutdatedLoginAttemptsServiceTaskTest : ServiceTaskTestCase<ClearOutdatedLoginAttemptsServiceTask>
	{
		public void TestRunTask()
		{
			CreateStmLoginFailureLogForOrgContact();
			CreateStmLoginFailureLogForGlbStaff();
			Factory.Save();

			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 121);
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 122))
			{
				var serviceTask = new ClearOutdatedLoginAttemptsServiceTaskForTest();
				serviceTask.RunTask();

				var logs = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery());
				AssertEquals(2, logs.Length);
				AssertEquals("Should not be deleted", "ContactToDelete1", logs.First(o => o.SFL_TableCode == "OC").SFL_LoginName);
				AssertEquals("Should not be deleted", "StaffToDelete5", logs.First(o => o.SFL_TableCode == "GS").SFL_LoginName);

				AssertEquals(6, serviceTask.TestLogger.LogEntries.Count());
				AssertCollectionContains("Deleting records for 'OC'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Login lockout minutes '122'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleted 3 attempt logs", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleting records for 'GS'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Login lockout minutes '121'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleted 2 attempt logs", serviceTask.TestLogger.LogEntries);
			}
		}

		public void TestRunTask_With0Minutes()
		{
			CreateStmLoginFailureLogForOrgContact();
			CreateStmLoginFailureLogForGlbStaff();
			Factory.Save();

			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var serviceTask = new ClearOutdatedLoginAttemptsServiceTaskForTest();
				serviceTask.RunTask();

				var logs = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery());
				AssertEquals("No record should be deleted", 7, logs.Length);
				AssertEquals("No deleted log", 6, serviceTask.TestLogger.LogEntries.Count());
				AssertCollectionContains("Deleted 0 attempt logs", serviceTask.TestLogger.LogEntries);
			}
		}

		public void TestRunTask_Performance()
		{
			var insertQueryContact = "DECLARE @lockoutTime datetime = DATEADD(MINUTE, -120, GetUtcDate());" +
				"SET @lockoutTime = DATEADD(DAY, -7, @lockoutTime);" +
				"INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser)" +
				" VALUES (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'contact@wisetech.com', null, 0, 'OC', @lockoutTime, 'E', @lockoutTime, 'E');" +

				// Staff
				"INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser)" +
				" VALUES (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E')" +
				", (NEWID(), 'staffName', null, 0, 'GS', @lockoutTime, 'E', @lockoutTime, 'E');";

			for (int i = 0; i < 5000; i++)
			{
				using (var command = Db.Connection.Command(insertQueryContact)) // Memory usage optimization
				{
					command.ExecuteNonQuery();
				}
			}

			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var serviceTask = new ClearOutdatedLoginAttemptsServiceTaskForTest();

				var timer = new Stopwatch();

				timer.Start();
				serviceTask.RunTask();
				timer.Stop();

				var timeTaken = timer.Elapsed;
				var assertMessage = "Time taken: " + timeTaken.ToString(@"m\:ss\.fff");
				Assert(assertMessage, timeTaken.Seconds < 30);

				var logs = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery());
				AssertEquals(0, logs.Length);

				AssertEquals(6, serviceTask.TestLogger.LogEntries.Count());
				AssertCollectionContains("Deleting records for 'OC'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Login lockout minutes '10'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleted 50000 attempt logs", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleting records for 'GS'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Login lockout minutes '10'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleted 55000 attempt logs", serviceTask.TestLogger.LogEntries);
			}
		}

		public void TestRunTask_ShouldKeepRecordsForAnExtraWeek()
		{
			var loginFailureLogContact1 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLogContact1.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-4).AddDays(-7);
			loginFailureLogContact1.SFL_LoginName = "ContactToDelete1";
			loginFailureLogContact1.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			var loginFailureLogContact2 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLogContact2.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-4);
			loginFailureLogContact2.SFL_LoginName = "ContactToDelete2";
			loginFailureLogContact2.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			var loginFailureLogContact3 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLogContact3.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-8).AddDays(-7);
			loginFailureLogContact3.SFL_LoginName = "ContactToDelete3";
			loginFailureLogContact3.SFL_TableCode = OrgContactSchema.Constants.Prefix;

			var loginFailureLogStaff1 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLogStaff1.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-4).AddDays(-7);
			loginFailureLogStaff1.SFL_LoginName = "StaffToDelete1";
			loginFailureLogStaff1.SFL_TableCode = GlbStaffSchema.Constants.Prefix;
			var loginFailureLogStaff2 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLogStaff2.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-4);
			loginFailureLogStaff2.SFL_LoginName = "StaffToDelete2";
			loginFailureLogStaff2.SFL_TableCode = GlbStaffSchema.Constants.Prefix;

			Factory.Save();

			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 124);
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 125))
			{
				var serviceTask = new ClearOutdatedLoginAttemptsServiceTaskForTest();
				serviceTask.RunTask();

				var logs = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery());
				AssertEquals(2, logs.Length);
				AssertEquals("Should not be deleted", "ContactToDelete2", logs.First(o => o.SFL_TableCode == "OC").SFL_LoginName);
				AssertEquals("Should not be deleted", "StaffToDelete2", logs.First(o => o.SFL_TableCode == "GS").SFL_LoginName);

				AssertEquals(6, serviceTask.TestLogger.LogEntries.Count());
				AssertCollectionContains("Deleting records for 'OC'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Login lockout minutes '124'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleted 2 attempt logs", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleting records for 'GS'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Login lockout minutes '125'", serviceTask.TestLogger.LogEntries);
				AssertCollectionContains("Deleted 1 attempt logs", serviceTask.TestLogger.LogEntries);
			}
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(ClearOutdatedLoginAttemptsServiceTask).GetMethod(nameof(ClearOutdatedLoginAttemptsServiceTask.CheckShouldRun));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertEquals(string.Empty, ClearOutdatedLoginAttemptsServiceTask.CheckShouldRun());
			}

			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertEquals("The registry WebLoginLockoutMinutes or LoginLockoutMinutes need to have a value greater than 0", ClearOutdatedLoginAttemptsServiceTask.CheckShouldRun());
			}

			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				AssertEquals(string.Empty, ClearOutdatedLoginAttemptsServiceTask.CheckShouldRun());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void CreateStmLoginFailureLogForOrgContact()
		{
			var loginFailureLog1 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog1.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-1).AddDays(-7);
			loginFailureLog1.SFL_LoginName = "ContactToDelete1";
			loginFailureLog1.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			var loginFailureLog2 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog2.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-4).AddDays(-7);
			loginFailureLog2.SFL_LoginName = "ContactToDelete2";
			loginFailureLog2.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			var loginFailureLog3 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog3.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-8).AddDays(-7);
			loginFailureLog3.SFL_LoginName = "ContactToDelete3";
			loginFailureLog3.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			var loginFailureLog4 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog4.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddMonths(-2).AddDays(-7);
			loginFailureLog4.SFL_LoginName = "ContactToDelete4";
			loginFailureLog4.SFL_TableCode = OrgContactSchema.Constants.Prefix;
		}

		void CreateStmLoginFailureLogForGlbStaff()
		{
			var loginFailureLog1 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog1.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-1).AddDays(-7);
			loginFailureLog1.SFL_LoginName = "StaffToDelete5";
			loginFailureLog1.SFL_TableCode = GlbStaffSchema.Constants.Prefix;
			var loginFailureLog2 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog2.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-4).AddDays(-7);
			loginFailureLog2.SFL_LoginName = "StaffToDelete6";
			loginFailureLog2.SFL_TableCode = GlbStaffSchema.Constants.Prefix;
			var loginFailureLog3 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog3.SFL_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-8).AddDays(-7);
			loginFailureLog3.SFL_LoginName = "StaffToDelete7";
			loginFailureLog3.SFL_TableCode = GlbStaffSchema.Constants.Prefix;
		}

		class ClearOutdatedLoginAttemptsServiceTaskForTest : ClearOutdatedLoginAttemptsServiceTask
		{
			public ClearOutdatedLoginAttemptsServiceTaskForTest()
				: base()
			{
				ServiceLogger = TestLogger;
			}

			public LoggerForTest TestLogger
			{
				get { return testLogger ?? (testLogger = new LoggerForTest()); }
			}
			LoggerForTest testLogger;
		}
	}
}
