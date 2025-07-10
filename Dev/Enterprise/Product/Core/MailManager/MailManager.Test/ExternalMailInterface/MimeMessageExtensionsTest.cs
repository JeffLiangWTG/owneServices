using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using MimeKit;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MimeMessageExtensionsTest : TestCaseWithFactory
	{
		public void TestParseAttachments()
		{
			var messageWithMessagerfc822Attachment = MimeMessageExtensions.CreateMessageFromEml(resourceRetriever.Value.GetString("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.messagerfc822.txt"));
			var attachments1 = messageWithMessagerfc822Attachment.GetFullAttachments();
			AssertEquals("Attachments Count", 2, attachments1.Count());
			AssertEquals("NonVisualAttachments Count", 2, messageWithMessagerfc822Attachment.GetNonVisualAttachments().Count());
			AssertEquals("VisualAttachments Count", 0, messageWithMessagerfc822Attachment.GetVisualAttachments().Count());

			AssertEquals("Message1-Attachment1-FileName", string.Empty, attachments1.ElementAt(0).GetName());
			var message1InnerMsg = MimeMessageExtensions.CreateMessageFromEml(attachments1.ElementAt(0).GetData());
			AssertEquals("Message1-Attachment1-Email-Subject", "How do I check my super transactions?", message1InnerMsg.Subject);
			AssertEquals("Message1-Attachment2-FileName", "New Bitmap Image.jpg", attachments1.ElementAt(1).GetName());
			AssertEquals("Message1-Attachment2-FileData", 51496, attachments1.ElementAt(1).GetData().Length);

			var messageWithMultipleAttachmentTypes = MimeMessageExtensions.CreateMessageFromEml(resourceRetriever.Value.GetString("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.MultipleAttachmentTypes.eml"));
			var attachments2 = messageWithMultipleAttachmentTypes.GetFullAttachments();
			AssertEquals("Attachments Count", 5, attachments2.Count());
			AssertEquals("NonVisualAttachments Count", 4, messageWithMultipleAttachmentTypes.GetNonVisualAttachments().Count());
			AssertEquals("VisualAttachments Count", 1, messageWithMultipleAttachmentTypes.GetVisualAttachments().Count());

			AssertEquals("Message2-Attachment1-FileName", "image001.png", attachments2.ElementAt(0).GetName());
			AssertEquals("Message2-Attachment1-FileData", 730, attachments2.ElementAt(0).GetData().Length);
			AssertEquals("Message2-Attachment2-FileName", "attachmentImage.png", attachments2.ElementAt(1).GetName());
			AssertEquals("Message2-Attachment2-FileData", 131, attachments2.ElementAt(1).GetData().Length);
			AssertEquals("Message2-Attachment3-FileName", "attachmentTxt.txt", attachments2.ElementAt(2).GetName());
			AssertEquals("Message2-Attachment3-FileData", 16, attachments2.ElementAt(2).GetData().Length);
			AssertEquals("Message2-Attachment4-FileName", "attachmentEmail.eml", attachments2.ElementAt(3).GetName());
			AssertEquals("Message2-Attachment4-FileData", 4196, attachments2.ElementAt(3).GetData().Length);
			AssertEquals("Message2-Attachment5-FileName", string.Empty, attachments2.ElementAt(4).GetName());
			var message2InnerMsg = MimeMessageExtensions.CreateMessageFromEml(attachments2.ElementAt(4).GetData());
			AssertEquals("Message1-Attachment1-Email-Subject", "Empty Email", message2InnerMsg.Subject);
		}

		public void TestMimeEntityGetDataIsNull()
		{
			var mimePart = new MimePart();
			AssertEquals(0, mimePart.GetData().Length);

			var messagePart = new MessagePart();
			AssertEquals(0, messagePart.GetData().Length);
		}

		public void TestGetMessageData()
		{
			var message = MimeMessageExtensions.CreateMessageFromEml(resourceRetriever.Value.GetString("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.messagerfc822.txt"));
			var readMessaage = MimeMessageExtensions.CreateMessageFromEml(message.GetData());
			AssertEquals("Message Subject", "SDubj", readMessaage.Subject);
		}

		public void TestRenderAddressWithBrackets()
		{
			var mailboxAddress1 = new MailboxAddress(null, "a@b.com");
			var mailboxAddress2 = new MailboxAddress("user", "a@b.com");

			AssertEquals("<a@b.com>", mailboxAddress1.RenderAddressWithBrackets());
			AssertEquals("\"user\" <a@b.com>", mailboxAddress2.RenderAddressWithBrackets());
		}

		public void TestGetSafeFileName()
		{
			AssertMatch(new Regex(@"att_(.+)\.dat"), MimeMessageExtensions.GetSafeFileName("", ""));
			AssertMatch(new Regex(@"att_(.+)\.json"), MimeMessageExtensions.GetSafeFileName("", "application/json"));
			AssertEquals("test", MimeMessageExtensions.GetSafeFileName("test", "application/json"));
			AssertEquals("test.", MimeMessageExtensions.GetSafeFileName("test.", "application/json"));
			AssertEquals("test.json", MimeMessageExtensions.GetSafeFileName("test.json", "application/json"));
			AssertEquals("test_%____.json", MimeMessageExtensions.GetSafeFileName(@"test?%|<>"".json", "application/json"));
			AssertEquals("test_test.json", MimeMessageExtensions.GetSafeFileName(@"test:test.json", "application/json"));
		}

		public void TestGetSenderOrFrom()
		{
			var mailboxAddress1 = new MailboxAddress("sender", "sender@test.com");
			var mailboxAddress2 = new MailboxAddress("from", "from@test.com");

			var message1 = new MimeMessage();
			message1.Sender = mailboxAddress1;
			message1.From.Add(mailboxAddress2);
			Assert(message1.GetSenderOrFrom().Equals(mailboxAddress1));

			var message2 = new MimeMessage();
			message2.From.Add(mailboxAddress2);
			Assert(message2.GetSenderOrFrom().Equals(mailboxAddress2));

			var message3 = new MimeMessage();
			AssertNull(message3.GetSenderOrFrom());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
