using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.StabilityChecker.Testing
{
	sealed class StabilityCheckerTest : TestCaseWithFactory
	{
		public void TestStabilityChecker()
		{
			var heathyAttribute = new StabilityCheckerAttribute("Desc", "Category", typeof(StabilityCheckerHealthy));
			var deadAttribute = new StabilityCheckerAttribute("Desc", "Category", typeof(StabilityCheckerException));
			var attributes = new StabilityCheckerAttribute[] { deadAttribute, heathyAttribute };
			StabilityResults results = StabilityChecker.CalculateStabilityResults(attributes);
			AssertEquals(2, results.Results.Count);

			const string descriptionStart = "The stability checker failed with an exception.\r\n\r\nSystem.ArgumentException: Something went wrong";
			Assert("Result description should start with error message '" + descriptionStart + "'\r\n\r\nActual Result:" + results.Results[0].Description, results.Results[0].Description.StartsWith(descriptionStart));
			const string stackTrace = "at Enterprise.StabilityChecker.Testing.StabilityCheckerException.Enterprise.StabilityChecker.IStabilityChecker.Check()";
			Assert("Result description should contain stack trace '" + stackTrace + "'\r\n\r\nActual Result:" + results.Results[0].Description, results.Results[0].Description.Contains(stackTrace));
			AssertEquals(StabilityResultLevel.Exception, results.Results[0].StabilityLevel);

			AssertEquals("All OK", results.Results[1].Description);
			AssertEquals(StabilityResultLevel.Healthy, results.Results[1].StabilityLevel);

			AssertEquals(ErrorReporter.LastMessageReported, "The stability checker failed with an exception.");
			ErrorReporter.Clear();
		}

		public void TestStoreStabilityResults()
		{
			DoTestStoreStabilityResults(true);
			DoTestStoreStabilityResults(false);
		}

		void DoTestStoreStabilityResults(bool isUserRelatedNotification)
		{
			var results = new StabilityResults();
			results.DateTimeCalculated = ZDateTime.UtcNow;
			results.Results.Add(new StabilityResult(StabilityResultLevel.Warning, "I'm sick", isUserRelatedNotification));
			StabilityChecker.StoreStabilityResults(results);
			var stored = StabilityChecker.GetStoredStabilityResults();
			AssertEquals(results, stored);
		}

		public void TestCreateNotifyEmailToPostMasterGroupIfSystemServiceTasksNotificationGroupIsEmpty()
		{
			AssertEquals("pre-condition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "A.G";
			group.GG_Desc = "Empty Group";

			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "test@edi.com";
			Factory.Save();

			NotificationDataRegistry.Instance.SystemServiceTasksNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var results = new StabilityResults();
			results.Results.Add(new StabilityResult(StabilityResultLevel.Warning, "I'm sick"));
			StabilityChecker.NotifyUsersOfStabilityResultsIfRequired(results);
			AssertEquals("EmailsCreated", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			var emailUtility = new EmailGroupUtility();
			var postMasterGroupEmails = emailUtility.GetGroupEmailCollection(Constants.Groups.PostMastersGroupPK, false);

			var emails = email.Recipients.Cast<RecipientDef>().Select(x => x.Email);
			AssertContainsExactElementsInAnyOrder(postMasterGroupEmails, emails);
			AssertContains("This email was intended for System Service Tasks Notification Group which is not set up in the registry item 'Notification > System Service Tasks Notification Group' or is empty.", email.Body);
		}

		public void TestUserRelatedNotificationEmail()
		{
			// set SystemServiceTasksNotificationGroup
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "A.G";
			group.GG_Desc = "8 days left";
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Xavier Admin";
			staff.GS_EmailAddress = "admin@cargowise.com";
			group.Staff.Add(staff);
			Factory.Save();
			NotificationDataRegistry.Instance.SystemServiceTasksNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			// set Post Master Group
			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var emailRecipient = postMasterGroup.Staff.AddNew();
			emailRecipient.GS_EmailAddress = "clinton@edi.com.au";
			emailRecipient.GS_LoginName = "TMP";
			emailRecipient.GS_Code = "ZAC";
			Factory.Save();

			var results = new StabilityResults();
			var healthyResult = "I'm all good";
			results.Results.Add(new StabilityResult(StabilityResultLevel.Healthy, healthyResult));
			var warningResult = "I'm sick";
			results.Results.Add(new StabilityResult(StabilityResultLevel.Warning, warningResult));
			results.Results.Add(new StabilityResult(StabilityResultLevel.Exception, "Exception Message"));
			var userNotification = "Please check disk space!";
			results.Results.Add(new StabilityResult(StabilityResultLevel.Warning, userNotification, isUserRelatedNotification: true));
			Assert(!results.Results[0].IsUserRelatedNotification);
			Assert(!results.Results[1].IsUserRelatedNotification);
			Assert(!results.Results[2].IsUserRelatedNotification);
			Assert(results.Results[3].IsUserRelatedNotification);
			StabilityChecker.NotifyUsersOfStabilityResultsIfRequired(results);

			AssertEquals("EmailsCreated", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			var normalEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, normalEmail.Recipients.Count);
			AssertEquals("admin@cargowise.com", normalEmail.Recipients[0].Email);
			AssertEmailBody(normalEmail.Body, healthyResult, warningResult);

			var userEemail = Env.OutgoingMailManager.EmailsCreated[1];
			var userEmailAddresses = new List<string>();

			foreach (RecipientDef recipient in userEemail.Recipients)
			{
				userEmailAddresses.Add(recipient.Email);
			}

			Assert(userEmailAddresses.Any(x => x.Equals("clinton@edi.com.au", StringComparison.OrdinalIgnoreCase)));
			AssertUserRelatedNotificationEmailBody(userEemail.Body, userNotification);
		}

		public void TestNotifyUsersOfStabilityResultsIfRequired()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "A.G";
			group.GG_Desc = "8 days left";

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Xavier Admin";
			staff.GS_EmailAddress = "admin@cargowise.com";

			group.Staff.Add(staff);
			Factory.Save();

			NotificationDataRegistry.Instance.SystemServiceTasksNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			StabilityResults results = new StabilityResults();

			StabilityChecker.NotifyUsersOfStabilityResultsIfRequired(results);
			AssertEquals("EmailsCreated", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			string healthyResult = "I'm all good";
			results.Results.Add(new StabilityResult(StabilityResultLevel.Healthy, healthyResult));
			StabilityChecker.NotifyUsersOfStabilityResultsIfRequired(results);
			AssertEquals("EmailsCreated", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			string warningResult = "I'm sick";
			results.Results.Add(new StabilityResult(StabilityResultLevel.Warning, warningResult));
			results.Results.Add(new StabilityResult(StabilityResultLevel.Exception, "Exception Message"));
			StabilityChecker.NotifyUsersOfStabilityResultsIfRequired(results);
			AssertEquals("EmailsCreated", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("admin@cargowise.com", email.Recipients[0].Email);
			AssertEmailBody(email.Body, healthyResult, warningResult);
		}

		void AssertEmailBody(string emailBody, string healthyResult, string warningResult)
		{
			string expectedBody = String.Format(@"System Stability Check

	License Code:		{0}
	DB Server Name:	{1}
	Database Name:	{2}

	Reporting Computer:	{3}
	Reporting Time:	01-Jan-00 00:00:00


Healthy: {4}

Warning: {5}",
				new EnterpriseInformationRetriever().LicenceCode,
				Db.ServerName,
				Db.DatabaseName,
				System.Environment.MachineName,
				healthyResult,
				warningResult);

			AssertEquals("Email Body", expectedBody, emailBody.Trim());
		}

		void AssertUserRelatedNotificationEmailBody(string emailBody, string userNotification)
		{
			var expectedBody = String.Format(@"System Stability Check

	License Code:		{0}
	DB Server Name:	{1}
	Database Name:	{2}

	Reporting Computer:	{3}
	Reporting Time:	01-Jan-00 00:00:00


Warning: {4}",
				new EnterpriseInformationRetriever().LicenceCode,
				Db.ServerName,
				Db.DatabaseName,
				System.Environment.MachineName,
				userNotification);

			AssertEquals("Email Body", expectedBody, emailBody.Trim());
		}
	}
}
