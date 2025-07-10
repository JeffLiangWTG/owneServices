using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Diagnostics
{
	[TestedType(typeof(EmailDiagnostics))]
	class EmailDiagnosticsTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			this.EmailDiagnostics = new EmailDiagnostics();
		}

		EmailDiagnostics EmailDiagnostics;

		public void TestTo()
		{
			AssertEquals("Default", "EmailTest@cargowise.com", EmailDiagnostics.To);

			EmailDiagnostics.To = "example@example";
			AssertEquals("example@example", EmailDiagnostics.To);
			Assert(!EmailDiagnostics.ToInfo.HasErrors());

			EmailDiagnostics.To = "";
			Assert(EmailDiagnostics.ToInfo.HasErrors());
		}

		public void TestSend()
		{
			EnvProxy.Instance.Registry.SMTPDefaultReturnEmailAddress = "system@example.com";
			CargoWise.Data.Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'user@example.com', GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E'"); // Need to update all GlbStaff records at once
			TestCaseHelper.ClearTable(MailRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(MailAttachment.Schema.TableName);
			TestCaseHelper.ClearTable(MailItem.Schema.TableName);
			EmailDiagnostics.Send();
			AssertEquals("Mail count", 1, Factory.Load<MailItem>(new ZQuery()).Length);
			AssertEquals("Mail recipient count", 1, Factory.Load<MailRecipient>(new ZQuery()).Length);
			Assert("Mail sender", Factory.LoadTop1<MailItem>(new ZQuery()).MI_From.Contains(EnvProxy.Instance.Registry.SMTPDefaultReturnEmailAddress));
		}

		public void TestReceivedSubject()
		{
			EmailDiagnostics.ReceivedSubject = "hi";
			AssertEquals("hi", EmailDiagnostics.ReceivedSubject);
		}

		public void TestReceivedDateTime()
		{
			EmailDiagnostics.ReceivedDateTime = new ZDateTime(2001, 11, 24, 1, 2, 3);
			AssertEquals(new ZDateTime(2001, 11, 24, 1, 2, 3), EmailDiagnostics.ReceivedDateTime);
		}

		public void TestCheckMail()
		{
			TestCaseHelper.ClearTable(MailRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(MailAttachment.Schema.TableName);
			TestCaseHelper.ClearTable(MailItem.Schema.TableName);

			EmailDiagnostics.CheckMail();
			AssertEquals("no emails", EmailDiagnostics.NoMessages, EmailDiagnostics.ReceivedSubject);
			Assert("no emails", EmailDiagnostics.ReceivedDateTime.IsEmpty);

			MailItem item = Factory.New(typeof(MailItem)) as MailItem;

			item.MI_Direction = MailDirection.Receive;
			item.MI_Subject = "This subject has nothing to do with anything. Don't treat it as a test message.";
			item.MI_ReceivedDateTime = new ZDateTime(2003, 2, 17, 8, 3, 0);
			item.MI_SendDateTime = new ZDateTime(2000, 1, 1, 0, 0, 0);
			Factory.Save();
			EmailDiagnostics.CheckMail();
			AssertEquals("non-test email", EmailDiagnostics.NoMessages, EmailDiagnostics.ReceivedSubject);
			Assert("non-test email", EmailDiagnostics.ReceivedDateTime.IsEmpty);

			item.MI_Subject = "blahblah" + EmailDiagnostics.SubjectIdentifier + "blah!";
			Factory.Save();
			EmailDiagnostics.CheckMail();
			AssertEquals("valid email", item.MI_Subject, EmailDiagnostics.ReceivedSubject);
			AssertEquals("valid email", item.MI_ReceivedDateTime, EmailDiagnostics.ReceivedDateTime);
		}
	}
}
