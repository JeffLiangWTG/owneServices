using System;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class EmailSenderTest : TransactionedTestCase
	{
		const string toAddress = "billy.jean@neverland.com";
		const string fromAddress = "mj@neverland.com";
		const string subject = "Nah nah nah";
		const string body = "The kid is not my son";

		public void TestSendEmailToEmptyGroupDoesntCreateMail()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var emptyGroup = factory.NewWithValidTestData<GlbGroup>();

			factory.Save();

			AssertEquals("PRE: The group is empty", 0, emptyGroup.Staff.Count);

			var emailsBefore = MailItemsCount;

			var emailSender = new EmailSender(TestConnection, "my@email.com");
			emailSender.SendEmailToGroup(emptyGroup.PK.ToGuid(), "Some stuff", "More stuff");

			AssertEquals("There was nobody to send to, so shouldnt have been created.", emailsBefore, MailItemsCount);
		}

		public void TestSendEmailToGroup()
		{
			SetEmail("PM", toAddress);

			var emailSender = new EmailSender(TestConnection, fromAddress);

			var recipientGroupPk = PostMastersGroupPk;
			emailSender.SendEmailToGroup(recipientGroupPk, subject, body);
			SendGroupEmailThroughEnterprise(recipientGroupPk, subject, body, fromAddress);

			var emails = GetMatchingEmails(toAddress, body);

			AssertEquals("Should be created the same way as it is in enterprise - Emails Created", 2, emails.Rows.Count);
			AssertEquals("Subject", emails.Rows[1][0].ToString(), emails.Rows[0][0].ToString());
			AssertEquals("From", RemoveCompanyName(emails.Rows[1][1].ToString()), RemoveCompanyName(emails.Rows[0][1].ToString()));
		}

		string RemoveCompanyName(string email)
		{
			return Regex.Replace(email, ".*?<(.*?)>", "$1");
		}

		void SendGroupEmailThroughEnterprise(Guid recipiantGroupPk, string subject, string body, string fromAddress)
		{
			var mail = new EmailDef { Subject = subject, Body = body, FromAddress = fromAddress };
			mail.Subject = subject;
			mail.Body = body;
			EnvProxy.Instance.OutgoingMailManager.CreateAndSave(mail, recipiantGroupPk, ZArchitecture.Core.GroupSourceLocator.GetFromRegistryItem(NotificationDataRegistry.Instance.WebPrintNotificationGroup));
		}

		public void TestSendEmailToUserWithNoEmail()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "COD";
			staff.GS_EmailAddress = string.Empty;

			factory.Save();

			var preCount = MailItemsCount;
			var emailSender = new EmailSender(TestConnection, fromAddress);

			Assert("We should fail to send an email", !emailSender.SendEmailToUser("COD", "Blah", "Blah"));
			AssertEquals("There was no email address for user, so no email should be created", preCount, MailItemsCount);
		}

		int MailItemsCount
		{
			get { return (int)TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.MailDBItems"); }
		}

		public void TestSendEmailsToUser()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "COD";
			staff.GS_EmailAddress = toAddress;

			factory.Save();

			var emailSender = new EmailSender(TestConnection, fromAddress);
			Assert("We should create the email successfully", emailSender.SendEmailToUser("COD", subject, body));

			var enterpriseEmail = new EmailDef { FromAddress = fromAddress, Subject = subject, Body = body };
			enterpriseEmail.AddRecipientForUserCommunication(toAddress);
			EnvProxy.Instance.OutgoingMailManager.CreateAndSave(enterpriseEmail);

			var emails = GetMatchingEmails(toAddress, body);

			AssertEquals("Should be created the same way as it is in enterprise - Emails Created", 2, emails.Rows.Count);
			AssertEquals("Subject", emails.Rows[1][0].ToString(), emails.Rows[0][0].ToString());
			AssertEquals("From", RemoveCompanyName(emails.Rows[1][1].ToString()), RemoveCompanyName(emails.Rows[0][1].ToString()));
		}

		Guid PostMastersGroupPk => new WebPrintNotificationGroupRegistryItem().LoadValue(TestConnection);

		DataTable GetMatchingEmails(string toAddress, string body)
		{
			var sqlText = string.Format(@"
				SELECT MI_Subject, MI_From FROM dbo.MailDBItems 
				WHERE MI_Body like '{0}'
				AND MI_Status = 'QUE'
				AND MI_Direction = 'TRX'
				AND MI_ContentType = 'PLN'
				AND MI_Encoding = ''
				AND MI_LastAttemptDateTime is null
				AND MI_PK in (SELECT MR_MI FROM dbo.MailDBRecipients WHERE MR_RecipientMailAddress = '{1}')
				ORDER BY MI_Subject",
				body,
				toAddress);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void SetEmail(string staffCode, string email)
		{
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbStaff SET GS_EmailAddress = '{0}', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_Code = 'PM'", toAddress));
		}

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}
	}
}
