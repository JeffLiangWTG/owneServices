using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.Testing;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.EConversation.ServiceTasks.Testing
{
	public abstract class NeoBusinessObjectEmailProcessorTest<T> : TestCaseWithFactory where T : BusinessObject, IAllowAttachEmailsToEDocs, IConversationProvider
	{
		public void TestEmailTypeName()
		{
			AssertEquals(ExpectedEmailTypeName, processor.EmailTypeName);
		}

		protected abstract string ExpectedEmailTypeName { get; }

		public void TestExpectedMailApplicationCode()
		{
			AssertEquals("NCV", processor.MailApplicationCode);
		}

		#region TestCreateAndProcessBusinessObjectEmail

		public void TestCreateAndProcessBusinessObjectEmail_FoundExistingBusinessObjectAndAttach()
		{
			using (GlowRegistry.Instance.NeoConversationsAttachmentDocType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ZZZ"))
			{
				AssertCreateAndProcessBusinessObjectEmail(true, EConversationUniqueIDUtil.GenerateElement(existingBizo), "ZZZ");
			}
		}

		public void TestCreateAndProcessBusinessObjectEmail_ElementDoesNotContainIdentifier_NotProcessed()
		{
			AssertNotProcessed($"<div name=\"{EConversationUniqueIDUtil.ElementName}\">Please don't edit the content of this email as your message may not be processed</div>");
			mockLogger.Verify(x => x.Log(LogType.Information, true, It.Is<string>(s => s.StartsWith("Business Object could not be loaded")), It.IsAny<object[]>()), Times.Once);
		}

		public void TestCreateAndProcessBusinessObjectEmail_EmptyIdentifier_NotProcessed()
		{
			AssertNotProcessed("<html><div>Some contents</div></html>");
			mockLogger.Verify(x => x.Log(LogType.Information, true, It.Is<string>(s => s.StartsWith("EConversation ID element not found")), It.IsAny<object[]>()), Times.Once);
		}

		public void TestCreateAndProcessBusinessObjectEmail_ExistingBusinessObjectNotFound_NotProcessed()
		{
			AssertNotProcessed($"<div name=\"{EConversationUniqueIDUtil.ElementName}\" id=\"JS|e7ed1c11-8624-4af6-b9b8-4848a7af27e5\">Some hackerman changed the pk</div>");
			mockLogger.Verify(x => x.Log(LogType.Information, true, It.Is<string>(s => s.StartsWith("Business Object could not be loaded")), It.IsAny<object[]>()), Times.Once);
		}

		void AssertNotProcessed(string htmlBody)
		{
			var email = new EmailBuilderForTesting()
				.Subject("Hello")
				.To("Someone", "test@wisetechglobal.com")
				.HtmlBody(htmlBody)
				.GetEmail();

			var query = new ZQuery(MailDBItemsSchema.MI_Application, processor.MailApplicationCode);

			AssertEquals("No MailItems created", 0, Factory.Load<MailItem>(query).Length);
			Assert("Should not be processed", !processor.CreateAndProcessMailItem(email, mockLogger.Object));
		}

		void AssertCreateAndProcessBusinessObjectEmail(bool shouldAttachToExistingBusinessObject, string htmlBody, string expectedDocType = null)
		{
			var email = new EmailBuilderForTesting()
				.From("From User", "fromuser@cargowise.com")
				.To("To User", "touser@cargowise.com")
				.Cc("Cc User", "ccuser@cargowise.com")
				.HtmlBody(htmlBody)
				.Subject($"RE: New messages in Shipment {existingBizo.ReferenceNumber}")
				.GetEmail();

			var query = new ZQuery(MailDBItemsSchema.MI_Application, processor.MailApplicationCode);
			AssertEquals("Precondition", 0, Factory.Load<MailItem>(query).Length);
			AssertEquals(0, existingBizo.DocManagerInfo.AllEDocs.Count);

			Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, mockLogger.Object));
			var mailItems = Factory.Load<MailItem>(query);
			AssertEquals(1, mailItems.Length);
			Assert(mailItems[0].IsInDatabase && !mailItems[0].HasChanges);
			AssertMailItemStatus(mailItems[0], shouldAttachToExistingBusinessObject);
			AssertEquals("From User <fromuser@cargowise.com>", mailItems[0].MI_From);
			AssertContains(htmlBody, mailItems[0].MI_Body);
			AssertEquals(email.Subject, mailItems[0].MI_Subject);
			AssertEquals(2, mailItems[0].MailRecipients.Count);

			IMailRecipient recTO, recCC;
			if (mailItems[0].MailRecipients[0].MR_RecipientType == nameof(MailRecipient.RecipientTypes.TO))
			{
				recTO = mailItems[0].MailRecipients[0];
				recCC = mailItems[0].MailRecipients[1];
			}
			else
			{
				recTO = mailItems[0].MailRecipients[1];
				recCC = mailItems[0].MailRecipients[0];
			}
			AssertEquals("\"To User\" <touser@cargowise.com>", recTO.EmailAddress);
			AssertEquals("\"Cc User\" <ccuser@cargowise.com>", recCC.EmailAddress);

			// Load in new Factory
			var bizO = new BusinessObjectFactory().Load<T>(existingBizo.PK);
			if (shouldAttachToExistingBusinessObject)
			{
				AssertEquals(1, bizO.DocManagerInfo.AllEDocs.Count);
				var eDoc = bizO.DocManagerInfo.AllEDocs[0];
				AssertEquals(expectedDocType, eDoc.DocType);
				AssertEquals(BusinessObjectEmailAttacher.GetEmailTextForOutlookExpressMessage(mailItems[0]), eDoc.GetImageDataReader().ConvertToAsciiStringAndCloseStream());
				mockLogger.Verify(x => x.Log(It.IsAny<LogType>(), It.IsAny<bool>(), It.Is<string>(s => s == "Email attached to: {0}"), It.Is<object[]>(args => args.Length == 1 && (string)args[0] == existingBizo.ReferenceNumber)), Times.Once);
			}
			else
			{
				AssertEquals(0, bizO.DocManagerInfo.AllEDocs.Count);
				mockLogger.Verify(x => x.Log(It.IsAny<LogType>(), It.IsAny<bool>(), It.Is<string>(s => s == "Email attached to: {0}"), It.IsAny<object[]>()), Times.Never);
			}
		}

		public void TestProcessFromEmailAddedToConversations()
		{
			var email1 = new EmailBuilderForTesting()
				.Subject($"New messages in Bizo {existingBizo.ReferenceNumber}")
				.To("Someone", "test@edi.com.au")
				.HtmlBody(EConversationUniqueIDUtil.GenerateElement(existingBizo))
				.GetEmail();
			var email2 = new EmailBuilderForTesting()
				.Subject($"New messages in Bizo {existingBizo.ReferenceNumber}")
				.To("Someone", "test@edi.com.au")
				.HtmlBody(EConversationUniqueIDUtil.GenerateElement(existingBizo))
				.GetEmail();

			processor.CreateAndProcessMailItem(email1, mockLogger.Object);
			processor.CreateAndProcessMailItem(email2, mockLogger.Object);

			var convo = existingBizo.eConversation;
			var messages = convo.Messages;

			Assert(convo.Messages.Any(x => x.JCM_Body.Contains(email1.LatestMessageInBody)));
			Assert(convo.Messages.Any(x => x.JCM_Body.Contains(email2.LatestMessageInBody)));
			AssertEquals(2, messages.Count);
		}

		public void TestProcessFromEmailAddedParticipants()
		{
			var email1 = new EmailBuilderForTesting()
				.Subject($"New messages in Bizo {existingBizo.ReferenceNumber}")
				.From("Queen Elizabeth", "queen.elizabeth@cargowise.com")
				.To("Someone", "test@edi.com.au")
				.HtmlBody(EConversationUniqueIDUtil.GenerateElement(existingBizo))
				.GetEmail();

			var email2 = new EmailBuilderForTesting()
				.Subject($"New messages in Bizo {existingBizo.ReferenceNumber}")
				.From("Somebody", "nonregistered@cargowise.com")
				.To("Someone", "test@edi.com.au")
				.HtmlBody(EConversationUniqueIDUtil.GenerateElement(existingBizo))
				.GetEmail();

			processor.CreateAndProcessMailItem(email1, mockLogger.Object);
			processor.CreateAndProcessMailItem(email2, mockLogger.Object);

			var convo = existingBizo.eConversation;

			Assert(convo.Participants.Any(x => x.EmailAddress.Contains("nonregistered@cargowise.com")));
			Assert(convo.Participants.Any(x => x.EmailAddress.Contains("queen.elizabeth@cargowise.com")));
		}

		public void TestProcessEmailWithAttachment()
		{
			using (GlowRegistry.Instance.NeoConversationsAttachmentDocType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ZZZ"))
			{
				var email1 = new EmailBuilderForTesting()
					.Subject($"New messages in Bizo {existingBizo.ReferenceNumber}")
					.From("Queen Elizabeth", "queen.elizabeth@cargowise.com")
					.To("Someone", "test@edi.com.au")
					.HtmlBody(EConversationUniqueIDUtil.GenerateElement(existingBizo))
					.WithAttachment("text.txt", "Hi, this is an attachment. Cheers")
					.GetEmail();

				processor.CreateAndProcessMailItem(email1, mockLogger.Object);

				var bizO = new BusinessObjectFactory().Load<T>(existingBizo.PK);
				AssertEquals(2, bizO.DocManagerInfo.AllEDocs.Count);
				AssertEquals("ZZZ", bizO.DocManagerInfo.AllEDocs[0].DocType);
				AssertEquals("ZZZ", bizO.DocManagerInfo.AllEDocs[1].DocType);
			}
		}

		public void TestIgnorePlainTextEmail_NotProcessed()
		{
			var email = new EmailBuilderForTesting()
				.Subject("Hello")
				.To("Someone", "test@wisetechglobal.com")
				.Body("there's no html to parse here")
				.GetEmail();

			var query = new ZQuery(MailDBItemsSchema.MI_Application, processor.MailApplicationCode);

			AssertEquals("No MailItems created", 0, Factory.Load<MailItem>(query).Length);
			Assert("Should not be processed", !processor.CreateAndProcessMailItem(email, mockLogger.Object));
		}

		void AssertMailItemStatus(MailItem mailItem, bool isMailAttached)
		{
			AssertEquals((isMailAttached) ? MailStatus.Processed : MailStatus.Unprocessed, mailItem.MI_Status);
		}

		#endregion

		protected override void SetUp()
		{
			mockLogger = new Mock<IEmailProcessorLogger>();
			processor = GetProcessor();

			existingBizo = GetExistingBusinessObject();
			Factory.Save();
		}

		protected abstract NeoBusinessObjectEmailProcessor<T> GetProcessor();

		protected abstract T GetExistingBusinessObject();

		Mock<IEmailProcessorLogger> mockLogger;
		NeoBusinessObjectEmailProcessor<T> processor;
		T existingBizo;
	}
}
