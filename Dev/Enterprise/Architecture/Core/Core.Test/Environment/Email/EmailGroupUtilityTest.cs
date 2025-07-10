using System;
using System.Collections.Specialized;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class EmailGroupUtilityTest : EmailTest
	{
		[ExpectNoExceptions()]
		public void TestNotificationNotSetDoesNotRaiseException()
		{
			var util = new EmailGroupUtility();
			DeleteStmDataNotificationRow();
			var thisShouldNotFail = util.GetCompanyNotificationGroupEmails();
			string groupDetail;
			thisShouldNotFail = util.GetCompanyNotificationGroupEmails(out groupDetail);
		}

		[ExpectNoExceptions]
		public void TestGetNotificationGroup()
		{
			Guid notifGroup = new EmailGroupUtility().GetNotificationGroup(Guid.Empty, Guid.Empty);
		}

		[ExpectException(typeof(EmailSendFailedException))]
		public void TestGetNotificationMailList()
		{
			Db.Connection.ExecuteNonQuery("update " + GlbStaffSchema.Constants.SqlSchemaName + "." + GlbStaffSchema.Constants.TableName + " set " + GlbStaffSchema.Constants.GS_EmailAddress + " = '', GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E'");
			StringCollection notifGroup = new EmailGroupUtility().GetNotificationMailList(Guid.Empty, Guid.Empty);
		}

		public void TestNotificationIsGroup()
		{
			DeleteStmDataNotificationRow();
			Guid groupPK = Guid.NewGuid();

			InsertStmDataNotificationRow(groupPK);

			InsertGroup(groupPK);

			Guid staff1PK = Guid.NewGuid();
			string testEmail1 = "test1@hotmail.com";
			InsertUser(staff1PK, testEmail1);

			Guid staff2PK = Guid.NewGuid();
			string testEmail2 = "test2@hotmail.com";
			InsertUser(staff2PK, testEmail2);

			InsertGroupLink(staff1PK, groupPK);
			InsertGroupLink(staff2PK, groupPK);

			// check that users that shouldn't be returned, aren't
			Guid unusedGroupPK = Guid.NewGuid();
			InsertGroup(unusedGroupPK);
			Guid unusedStaffPK = Guid.NewGuid();
			InsertUser(unusedStaffPK, "unused@hotmail.com");
			InsertGroupLink(unusedStaffPK, unusedGroupPK);

			var notificationEmails = new EmailGroupUtility().GetCompanyNotificationGroupEmails();
			AssertEquals(2, notificationEmails.Count);
			if (notificationEmails[0] == testEmail1)
			{
				AssertEquals(notificationEmails[1], testEmail2);
			}
			else
			{
				AssertEquals(notificationEmails[0], testEmail2);
				AssertEquals(notificationEmails[1], testEmail1);
			}

			string groupDetail;
			notificationEmails = new EmailGroupUtility().GetCompanyNotificationGroupEmails(out groupDetail);
			AssertEquals(2, notificationEmails.Count);
			if (notificationEmails[0] == testEmail1)
			{
				AssertEquals(notificationEmails[1], testEmail2);
			}
			else
			{
				AssertEquals(notificationEmails[0], testEmail2);
				AssertEquals(notificationEmails[1], testEmail1);
			}
			AssertEquals("Group Detail", "'T$1 - Test'", groupDetail);
		}

		void InsertEmailToGroupAll(string email, Guid groupAllPK, bool isActive = true)
		{
			var staffPK = Guid.NewGuid();
			InsertUser(staffPK, email, isActive);
			InsertGroupLink(staffPK, groupAllPK);
		}

		public void TestGetGroupAllEmailCollection()
		{
			var groupAllPK = Constants.Groups.AllPK;
			var emails1 = new EmailGroupUtility().GetGroupEmailCollection(groupAllPK, false);

			InsertEmailToGroupAll(EnvProxy.Instance.Registry.HostedNotificationsEmailOverride, groupAllPK);
			InsertEmailToGroupAll("Hosting.Notifications@cargowise.com", groupAllPK);
			InsertEmailToGroupAll("test3@hotmail.com", groupAllPK);
			InsertEmailToGroupAll("test4@hotmail.com", groupAllPK, false);

			var emails2 = new EmailGroupUtility().GetGroupEmailCollection(groupAllPK, false);

			AssertEquals(1, emails2.Count - emails1.Count);

			Assert(!emails2.Contains(EnvProxy.Instance.Registry.HostedNotificationsEmailOverride));
			Assert(!emails2.Contains("Hosting.Notifications@cargowise.com"));
			Assert(emails2.Contains("test3@hotmail.com"));
			Assert(!emails2.Contains("test4@hotmail.com"));
		}

		public void TestGetStaffEmailCollection()
		{
			var testEmail1 = "test1@hotmail.com";
			var testEmail11 = "test11@hotmail.com";
			InsertUser(Guid.NewGuid(), testEmail1);
			InsertUser(Guid.NewGuid(), testEmail11);

			var emailPMG = "pmg@pmg.ru";
			var pm = Guid.NewGuid();
			InsertUser(pm, emailPMG);
			InsertGroupLink(pm, EnvProxy.Instance.Registry.PostMasterGroup);

			var testEmail2 = "test2@hotmail.com";
			var testEmail22 = "test22@hotmail.com";
			InsertAdminUser(Guid.NewGuid(), testEmail2);
			InsertAdminUser(Guid.NewGuid(), testEmail22);

			var hostingNotificationEmail = EnvProxy.Instance.Registry.HostedNotificationsEmailOverride;
			InsertAdminUser(Guid.NewGuid(), hostingNotificationEmail);

			var hostingNotificationEmail2 = "hosting.notifications@cargowise.com";
			InsertAdminUser(Guid.NewGuid(), hostingNotificationEmail2);

			var hostingNotificationEmail3 = "hosting.notifications@corporate.cargowise.com";
			InsertAdminUser(Guid.NewGuid(), hostingNotificationEmail3);

			var emailGroupUtility = new EmailGroupUtility();
			var emailCollection = emailGroupUtility.GetStaffEmailCollection(true);

			AssertCollectionContains("email for Admin staff", "test2@hotmail.com", emailCollection);
			AssertCollectionContains("email for Admin staff", "test22@hotmail.com", emailCollection);
			AssertCollectionNotContains("email for non Admin staff", "test1@hotmail.com", emailCollection);
			AssertCollectionNotContains("email for non Admin staff", "test11@hotmail.com", emailCollection);
			AssertCollectionNotContains("email for Post Master staff", emailPMG, emailCollection);
			AssertCollectionContains("email " + hostingNotificationEmail, hostingNotificationEmail, emailCollection);
			AssertCollectionContains("email " + hostingNotificationEmail2, hostingNotificationEmail2, emailCollection);
			AssertCollectionContains("email " + hostingNotificationEmail3, hostingNotificationEmail3, emailCollection);

			foreach (var email in emailCollection)
			{
				AssertCollectionContains("email for Admin staff", email, emailGroupUtility.AdminStaff);
			}

			emailCollection.Clear();

			emailCollection = emailGroupUtility.GetStaffEmailCollection(false);
			AssertCollectionContains("email for Admin staff", "test2@hotmail.com", emailCollection);
			AssertCollectionContains("email for Admin staff", "test22@hotmail.com", emailCollection);
			AssertCollectionContains("email for non Admin staff", "test1@hotmail.com", emailCollection);
			AssertCollectionContains("email for non Admin staff", "test11@hotmail.com", emailCollection);
			AssertCollectionContains("email for Post Master staff", emailPMG, emailCollection);
			AssertCollectionNotContains("email " + hostingNotificationEmail, hostingNotificationEmail, emailCollection);
			AssertCollectionNotContains("email " + hostingNotificationEmail2, hostingNotificationEmail2, emailCollection);
			AssertCollectionNotContains("email " + hostingNotificationEmail3, hostingNotificationEmail3, emailCollection);

			foreach (var email in emailCollection)
			{
				AssertCollectionContains("email for All staff", email, emailGroupUtility.AllStaff);
			}

			emailCollection.Clear();

			emailCollection = emailGroupUtility.PostMasters;
			AssertCollectionNotContains("email for Admin staff", "test2@hotmail.com", emailCollection);
			AssertCollectionNotContains("email for Admin staff", "test22@hotmail.com", emailCollection);
			AssertCollectionNotContains("email for non Admin staff", "test1@hotmail.com", emailCollection);
			AssertCollectionNotContains("email for non Admin staff", "test11@hotmail.com", emailCollection);
			AssertCollectionContains("email for Post Master staff", emailPMG, emailCollection);
		}

		public void TestShouldUseDbCorrectly()
		{
			var lastError = string.Empty;

			var thread = new Thread(() =>
			{
				ErrorReporter.Clear();

				var utility = new EmailGroupUtility();
				utility.GetGroupEmailCollection(Guid.Empty, false);
				utility.GetStaffEmailCollection(false);
				utility.GetNotificationGroup(Guid.Empty, Guid.Empty);

				lastError = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});
			thread.Start();
			thread.Join(1000);

			// Should not be "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()"
			AssertEquals(string.Empty, lastError);
		}

		void InsertAdminUser(Guid staffPK, string emailAddress)
		{
			UserCount++;
			if (UserCount > 9)
			{
				UserCount = 0;
			}

			string sql = "insert into dbo.GlbStaff (GS_PK, GS_EmailAddress, GS_Code, GS_LoginName, GS_IsActive, GS_IsController, GS_IsDeveloper, GS_IsSystemAccount, GS_FullName, GS_UserAddress1, GS_City, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (";
			sql += "'" + staffPK + "', '" + emailAddress + "', 'T" + UserCount.ToString() + "', 'Test" + UserCount.ToString() + "', 1, 1, 0, 0, 'Some Test User', 'Address', 'Kiev', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
