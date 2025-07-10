using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DbBackup.Engine;
using Enterprise.DbHealth.Check;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	[TestedType(typeof(DbHealthCheckServiceTask))]
	sealed class DbHealthCheckServiceTaskTest : ServiceTaskTestCase<DbHealthCheckServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		public void TestIsAcknowledgeableWarning_IsSameWarning()
		{
			AssertAcknowledgeableWarning(DatabaseWarning.FileLocationWarning, isAcknowledgedUpdated: true);
			AssertAcknowledgeableWarning(ServerWarning.SqlVersionWarning, isAcknowledgedUpdated: true);
			AssertAcknowledgeableWarning(DiskWarning.DiskSpaceWarning, isAcknowledgedUpdated: true);
			AssertAcknowledgeableWarning(DatabaseWarning.RestoreWarning, isAcknowledgedUpdated: false);

			void AssertAcknowledgeableWarning(string warningType, bool isAcknowledgedUpdated)
			{
				var dbHealthCheckTask = new DbHealthCheckServiceTaskForTesting();
				var warningList = new DbHealthWarningListForTesting();

				var warning1 = "On 03/14/2022";
				warningList.AddDummyWarning("Source", warningType, warning1, "");
				dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
				var registryWarningCollection = SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value;
				AssertEquals(1, registryWarningCollection.Count);
				AssertEquals("On 03/14/2022", registryWarningCollection[0].Description);

				AssertEquals("Precondition", expected: true, registryWarningCollection[0].IsAcknowledgeable);
				AssertEquals("Precondition", expected: false, registryWarningCollection[0].IsAcknowledged);

				registryWarningCollection[0].IsAcknowledged = true;
				warningList.ClearList();
				warningList.AddDummyWarning("Source", warningType, warning1, "");
				dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
				registryWarningCollection = SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value;
				AssertEquals("Count not changed", 1, registryWarningCollection.Count);
				AssertEquals("IsAcknowledged was set to true since IsSameWarning is true", expected: true, registryWarningCollection[0].IsAcknowledged);

				var warning2 = "On 03/15/2022";
				registryWarningCollection[0].IsAcknowledged = true;
				warningList.ClearList();
				warningList.AddDummyWarning("Source", warningType, warning2, "");

				dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
				registryWarningCollection = SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value;
				AssertEquals("Count not changed", 1, registryWarningCollection.Count);
				AssertEquals("On 03/15/2022", registryWarningCollection[0].Description);
				AssertEquals("IsAcknowledged was set according to IsSameWarning", isAcknowledgedUpdated, registryWarningCollection[0].IsAcknowledged);
			}
		}

		[TestDate(2012, 11, 1)]
		public void TestSendWarningEmailScenarios()
		{
			var dbHealthCheckTask = new DbHealthCheckServiceTaskForTesting();
			AssertEquals("Sent Email Count [Initial]", 0, dbHealthCheckTask.SentEmailCount);

			// Empty warning list => DO NOT send an email
			var warningList = new DbHealthWarningListForTesting();
			dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
			AssertEquals("Sent Email Count [Empty list]", 0, dbHealthCheckTask.SentEmailCount);

			// 1 warning => sends an email
			warningList.AddDummyWarning("Source", ServerWarning.SqlVersionWarning, "Acknowledgeable Warning", "");
			dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
			AssertEquals("Sent Email Count [List: 1 warning, 0 acknowledged]", 1, dbHealthCheckTask.SentEmailCount);

			// Warning acknowledged, but it's the first day of the month => sends another email
			var registryWarningColection = SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value;
			registryWarningColection[0].IsAcknowledged = ZBool.True;
			SystemDataRegistry.Instance.LastDbHealthCheckWarningList.SetValue(registryWarningColection);
			dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
			AssertEquals("Sent Email Count [List: 1 warning, 1 acknowledged]", 2, dbHealthCheckTask.SentEmailCount);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			// Warning acknowledged and it's NOT the first day of the month => DO NOT send an email
			dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
			AssertEquals("Sent Email Count [List: 1 warning, 1 acknowledged]", 2, dbHealthCheckTask.SentEmailCount);

			// Warning unacknowledged => sends another email
			registryWarningColection = SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value;
			registryWarningColection[0].IsAcknowledged = ZBool.False;
			SystemDataRegistry.Instance.LastDbHealthCheckWarningList.SetValue(registryWarningColection);
			dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
			AssertEquals("Sent Email Count [List: 1 warning, 0 acknowledged]", 3, dbHealthCheckTask.SentEmailCount);

			// Warning acknowledged again => DO NOT send an email
			registryWarningColection = SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value;
			registryWarningColection[0].IsAcknowledged = ZBool.True;
			SystemDataRegistry.Instance.LastDbHealthCheckWarningList.SetValue(registryWarningColection);
			dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
			AssertEquals("Sent Email Count [List: 1 warning, 1 acknowledged]", 3, dbHealthCheckTask.SentEmailCount);

			// Added 1 unacknowledged warning => sends another email
			warningList.AddDummyWarning("Source", "Type", "Unacknowledgeable Warning", "");
			dbHealthCheckTask.EmailWarningListIfChangedAndSaveLastWarningList_Exposed(warningList);
			AssertEquals("Sent Email Count [List: 2 warnings, 1 acknowledged]", 4, dbHealthCheckTask.SentEmailCount);
		}

		[ExpectNoExceptions]
		public void TestEmptyEmailingGroupDoesNotThowException()
		{
			var emailUntility = new EmailGroupUtility();

			var groupEmails = emailUntility.SendNotificationToDatabaseAdministrator(NotificationDataRegistry.Instance.DatabaseHealthCheckNotificationGroup.Value, throwExceptionIfEmptyGroup: false);

			AssertEquals("Preconidition: DatabaseHealthCheckNotificationGroup is empty", 0, groupEmails.Count);

			var companyNotificationEmails = emailUntility.GetCompanyNotificationGroupEmails();
			AssertEquals("Preconidition: CompanyNotificationGroupEmails is empty", 0, companyNotificationEmails.Count);

			var dbHealthCheckTask = new DbHealthCheckServiceTaskForTestingEmail();

			dbHealthCheckTask.ServiceLogger = new TestServiceLogger();
			Assert("Precondition: there is NO record about empty emailing groups in the log", !dbHealthCheckTask.ServiceLogger.ToString().Contains(EmailNotificationSender.EmailsGroupsEmptyWarningText));
			var originalIsInterative = Globals.IsUserInteractive;
			using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInterative))
			using (Globals.SetIsUnitTestingProductionFunctionality())
			{
				Globals.IsUserInteractive = false;
				dbHealthCheckTask.SendWarningEmailExposed("Tes subject", "Test Body");
				Assert("Log record about empty emailing groups has been added", dbHealthCheckTask.ServiceLogger.ToString().Contains(EmailNotificationSender.EmailsGroupsEmptyWarningText));
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		class DbHealthCheckServiceTaskForTesting : DbHealthCheckServiceTask
		{
			protected override void SendWarningEmail(string subject, string body)
			{
				SentEmailCount++;
			}

			public int SentEmailCount;

			public void EmailWarningListIfChangedAndSaveLastWarningList_Exposed(DbHealthWarningList healthWarnings)
			{
				EmailWarningListIfChangedAndSaveLastWarningList(healthWarnings, CancellationToken.None);
			}
		}

		class DbHealthCheckServiceTaskForTestingEmail : DbHealthCheckServiceTask
		{
			public void SendWarningEmailExposed(string subject, string body)
			{
				SendWarningEmail(subject, body);
			}
		}
	}
}
