using System.Collections.Specialized;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Mailer.Email.Testing
{
	sealed class EmailDocumentTest : TestCaseWithFactory
	{
		public void TestSubjectProperty()
		{
			string fSubjectTest = "subjective";
			EmailDocument fEmail = new EmailDocument();
			fEmail.Subject = fSubjectTest;
			AssertEquals("Subject accessible", fSubjectTest, fEmail.Subject);
		}

		public void TestBusinessEntityIDProperty()
		{
			ZGuid guid = ZGuid.NewZGuid();
			EmailDocument email = new EmailDocument();
			email.fEmailDef.SetupBusinessEntityInfo(guid, string.Empty, string.Empty);
			AssertEquals("BusinessEntityID can be accessed", guid, email.BusinessEntityID);
		}

		public void TestBusinessEntityTableCodeProperty()
		{
			string tableCode = "JS";
			EmailDocument email = new EmailDocument();
			email.fEmailDef.SetupBusinessEntityInfo(ZGuid.Empty, tableCode, string.Empty);
			AssertEquals("BusinessEntityTableCode can be accessed", tableCode, email.BusinessEntityTableCode);
		}

		public void TestBusinessEntityJobNumberProperty()
		{
			string jobNumber = "S00000001";
			EmailDocument email = new EmailDocument();
			email.fEmailDef.SetupBusinessEntityInfo(ZGuid.Empty, string.Empty, jobNumber);
			AssertEquals("BusinessEntityTableCode can be accessed", jobNumber, email.BusinessEntityJobNumber);
		}

		public void TestFromDisplayNameProperty()
		{
			string fFromDisplayNameTest = "Peter Drucker";
			EmailDocument fEmail = new EmailDocument();
			fEmail.FromDisplayName = fFromDisplayNameTest;
			AssertEquals("From display name accessible", fFromDisplayNameTest, fEmail.FromDisplayName);
		}

		public void TestRecipientsProperty()
		{
			StringCollection fRecipientstTest = new StringCollection();
			fRecipientstTest.Add("arnolds@lalaland.us");
			EmailDocument fEmail = new EmailDocument();
			fEmail.Recipients = fRecipientstTest;
			AssertEquals("Recipients accessible", fRecipientstTest[0], fEmail.Recipients[0]);
		}

		public void TestCCRecipientsProperty()
		{
			StringCollection fCCRecipientsTest = new StringCollection();
			fCCRecipientsTest.Add("incahoots@w.gov.us");
			EmailDocument fEmail = new EmailDocument();
			fEmail.CCRecipients = fCCRecipientsTest;
			AssertEquals("Recipients accessible", fCCRecipientsTest[0], fEmail.CCRecipients[0]);
		}

		public void TestBCCRecipientsProperty()
		{
			StringCollection blindCarbonCopyRecipients = new StringCollection();
			blindCarbonCopyRecipients.Add("incahoots@w.gov.us");
			EmailDocument emailDocument = new EmailDocument { BCCRecipients = blindCarbonCopyRecipients };
			AssertEquals("Recipients accessible", blindCarbonCopyRecipients[0], emailDocument.BCCRecipients[0]);
		}

		public void TestBodyProperty()
		{
			string fBodyTest = "Kylie's";
			EmailDocument fEmail = new EmailDocument();
			fEmail.Body = fBodyTest;
			AssertEquals("Body accessible", fBodyTest, fEmail.Body);
		}

		public void TestAttachmentsProperty()
		{
			EmailDocument fEmail = new EmailDocument();
			AttachmentDef fAttachmentDef = new AttachmentDef("", System.Array.Empty<byte>());
			fEmail.Attachments.Add(fAttachmentDef);
			AssertEquals("Attachment accessible", fAttachmentDef, fEmail.Attachments[0]);
		}

		public void TestExceptionThrown_PlainTextEmailNoAttachments()
		{
			var bodyTest = "Kylie's";
			var email = new EmailDocument();
			email.Body = bodyTest;
			email.Subject = bodyTest;
			var testMailRecipient1 = "hulk@edi.com.au";
			var mailRecipients = new StringCollection();
			mailRecipients.Add(testMailRecipient1);
			email.Recipients = mailRecipients;
			AssertExceptionThrown(typeof(EmailDocument.MailerEmailException), () => email.Send());
			email.ContentType = EmailContentTypes.HTML;
			AssertNoExceptionThrown(() => email.Send());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBatchEmail()
		{
			TestCaseHelper.ClearTable(MailDBAttachmentsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(MailDBRecipientsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			string testMailRecipient1 = "hulk@edi.com.au";
			string testMailRecipient2 = "davidj@edi.com.au";
			StringCollection mailRecipients = new StringCollection();
			mailRecipients.Add(testMailRecipient1);
			mailRecipients.Add(testMailRecipient2);
			AttachmentDef mailAttachment = new AttachmentDef(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName);
			AttachmentDefCollection fAttachmentList = new AttachmentDefCollection();
			fAttachmentList.Add(mailAttachment);

			EmailDocument postEmailToMailDB = new EmailDocument(fAttachmentList);
			postEmailToMailDB.Recipients = mailRecipients;
			postEmailToMailDB.Subject = "Print processor test";
			postEmailToMailDB.Send();

			AssertEquals("Outbound mail exists", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Outbound mail with recipients", 2, email.Recipients.Count);
			AssertEquals("Outbound mail with attachment", 1, email.Attachments.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestSendEmaiNow()
		{
			string testMailRecipient = "user@example";
			StringCollection mailRecipients = new StringCollection();
			mailRecipients.Add(testMailRecipient);
			AttachmentDef mailAttachment = new AttachmentDef(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName);
			AttachmentDefCollection fAttachmentList = new AttachmentDefCollection();
			fAttachmentList.Add(mailAttachment);
			EmailDocument meEmail = new EmailDocument(fAttachmentList);
			meEmail.Recipients = mailRecipients;
			meEmail.Subject = "Print processor test";
			meEmail.Send();
		}

		public void TestGetEmailSender()
		{
			EmailDocument meEmail = new EmailDocument();
			AssertNotNull("Email from", meEmail.SendingEmailAddress);
		}

		public void TestIsValidEmailAddress()
		{
			string[] valid =
			{
					"davidj@edi.com.au",
					"David.James@edi.com.au",
					"DavidJames@x.org",
					"a@example.org",
					"test@tv.tv",
					"tv@tv.tv"
				};

			string result = "";

			foreach (string validString in valid)
			{
				if (!EmailDocument.IsValidEmailAddress(validString))
				{
					result += validString + " should be valid" + System.Environment.NewLine;
				}
			}

			AssertEquals("Should be no errors, but these were found:", "", result);
		}
	}
}
