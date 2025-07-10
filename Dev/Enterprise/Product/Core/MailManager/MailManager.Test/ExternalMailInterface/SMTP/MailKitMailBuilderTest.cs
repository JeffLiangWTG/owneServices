using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.ZArchitecture.Environment;
using MailManager;
using MimeKit;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailKitMailBuilderTest : TestCaseWithFactory
	{
		public void TestBuildMessage()
		{
			AssertNotNull("Simple message", MailTestHelpers.GetMailItemWithoutAttachments(Factory).BuildMimeMessage());

			var mi = Factory.New<MailItem>();
			mi.RawMIMEString = resourceRetriever.Value.GetString("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.TestMail1.eml");
			AssertNotNull("From Mime", mi.BuildMimeMessage());
		}

		[TestTimeZoneUNLOCO("UAIEV")]
		[TestUtcOffset(2, 0, 0)]
		[TestDate]
		public void TestBuildMessage2()
		{
			//1) Set utc time to 10/01/2014 03:01:08 AM UTC and specify UtcOffest for Kiev as +2hours
			TestDateAttribute.Date = new DateTime(2014, 1, 10, 3, 1, 8, DateTimeKind.Utc);

			AssertEquals(new DateTime(2014, 1, 10, 3, 1, 8, DateTimeKind.Utc), ZDateTime.UtcNow);
			AssertEquals(new DateTime(2014, 1, 10, 5, 1, 8, DateTimeKind.Local), ZDateTime.Now);

			ZGuid bizOPK = ZGuid.NewZGuid();
			ZGuid staffPK = ZGuid.NewZGuid();

			//2) Create Mail Item
			var mi = Factory.New<MailItem>();
			mi.MI_Body = "Body " + (char)0x92;
			mi.MI_Subject = "Non-Ascii character: " + (char)0x92;
			mi.MI_From = "From@host.com";
			mi.MI_ReplyTo = "ReplyTo@host.com";
			mi.AddRecipientForUserCommunication("To@host.com", Business.MailRecipient.RecipientTypes.TO);
			mi.AddRecipientForUserCommunication("Cc@host.com", Business.MailRecipient.RecipientTypes.CC);
			mi.AddRecipientForUserCommunication("Bcc@host.com", Business.MailRecipient.RecipientTypes.BCC);
			mi.MI_ReceivedDateTime = ZDateTime.UtcNow;
			mi.MI_BusinessEntityID = bizOPK.ToString();
			mi.MI_BusinessEntityTableCode = ZArchitecture.Schema.GlbCompanyCampaignItemSchema.Constants.Prefix;
			mi.MI_BusinessEntityjobNumber = "S00000001";
			mi.MI_DocumentName = "Arrival Notice";
			mi.MI_Direction = DirectionList.Codes.Receive;
			mi.MI_SenderStaffID = staffPK.ToString();
			mi.MI_ListUnsubscribe = "<http://www.cw1.com/u?1234>";

			Factory.Save();

			var message = mi.BuildMimeMessage();

			AssertEquals("Body", mi.MI_Body, message.TextBody);
			AssertNotEquals("ContentTransferEncoding", ContentEncoding.SevenBit, ((MimePart)message.Body).ContentTransferEncoding);
			AssertEquals("Subject", mi.MI_Subject, message.Subject);
			AssertEquals("From@host.com", "From@host.com", message.From[0].ToString());
			AssertEquals("To", "To@host.com", message.To[0].ToString());
			AssertEquals("Cc", "Cc@host.com", message.Cc[0].ToString());
			AssertEquals("Bcc", "Bcc@host.com", message.Bcc[0].ToString());
			AssertEquals("ReplyTo", "ReplyTo@host.com", message.ReplyTo[0].ToString());
			AssertEquals("Date", new DateTime(2014, 1, 10, 3, 1, 8, DateTimeKind.Utc), message.Date.DateTime.ToUniversalTime());
			Assert("Body format should be a plain text", string.IsNullOrEmpty(message.HtmlBody));
			AssertEquals("BusinessEntityID", bizOPK.ToString(), message.Body.Headers["X-BusinessEntityID"]);
			AssertEquals("BusinessEntityTableCode", ZArchitecture.Schema.GlbCompanyCampaignItemSchema.Constants.Prefix, message.Body.Headers["X-BusinessEntityTableCode"]);
			AssertEquals("BusinessEntityjobNumber", "S00000001", message.Body.Headers["X-BusinessEntityjobNumber"]);
			AssertEquals("DocumentName", MasterFiles.Business.BounceEmailParser.EncodeDocumentName("Arrival Notice"), message.Body.Headers["X-DocumentName"]);
			AssertEquals("SenderStaffID", staffPK.ToString(), message.Body.Headers["X-SenderStaffID"]);
			AssertEquals("ListUnsubscribe", "<http://www.cw1.com/u?1234>", message.Body.Headers["List-Unsubscribe"]);
			AssertNullOrEmpty(message.Sender?.Address);

			// Non US-ASCII characters in document name
			mi.MI_DocumentName = "Fällige Rechnungen";
			message = mi.BuildMimeMessage();
			AssertEquals("DocumentName", MasterFiles.Business.BounceEmailParser.EncodeDocumentName("Fällige Rechnungen"), message.Body.Headers["X-DocumentName"]);

			mi = Factory.New<MailItem>();
			mi.MI_From = "undisclosed-recipients:";
			message = mi.BuildMimeMessage();
			AssertEquals("Mail group should be parsed correctly", 1, message.From.Count);
		}

		public void TestBuildMessage_HTML()
		{
			var mi1 = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi1.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			mi1.MI_Body = "<html />";
			var message1 = mi1.BuildMimeMessage();
			Assert("Should be html email", string.IsNullOrEmpty(message1.TextBody));
			Assert("Should be html email", !string.IsNullOrEmpty(message1.HtmlBody));

			var mi2 = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi2.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			mi2.MI_Body = "plain text";
			var message2 = mi2.BuildMimeMessage();
			Assert("Should NOT be html email", !string.IsNullOrEmpty(message2.TextBody));
			Assert("Should NOT be html email", string.IsNullOrEmpty(message2.HtmlBody));
		}

		public void TestWithHtmlEmailRelatedData()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi.MailAttachments.AddNew();
			mi.MailAttachments[0].MA_FileName = "file.jpg";
			mi.MailAttachments[0].MA_Data = new byte[] { 1 };
			mi.MI_Body = "<img src=\"cid:file.jpg\" alt=\"Footer Image\" />";
			mi.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			var message = mi.BuildMimeMessage();

			using (var stream = new MemoryStream())
			{
				message.WriteTo(stream);
				stream.Position = 0;
				var messageString = Encoding.UTF8.GetString(stream.ToByteArray());
				Assert("Email must contain mime-version 1.0 (for compatibility with Outlook)", messageString.ToLower().Contains("mime-version: 1.0"));
				Assert("content-location: file.jpg", messageString.ToLower().Contains("filename=file.jp"));
				Assert("content-id: <file.jpg>", messageString.ToLower().Contains("content-id: <file.jpg>"));
			}

			AssertEquals(1, message.BodyParts.Count(x => !(x is TextPart)));
			var mimePart = message.BodyParts.OfType<MimePart>().First(x => !(x is TextPart));
			AssertEquals("file.jpg", mimePart.FileName);
			AssertEquals("Image data should be base64 encoded", ContentEncoding.Base64, mimePart.ContentTransferEncoding);
			AssertEquals((byte)1, mimePart.Content.Stream.ToByteArray()[0]);
		}

		public void TestBuildMessageUseAlternativeViewWithVCalendarMessage()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi.MI_ContentType = EmailContentTypes.Calendar.ContentTypeCode;
			mi.MI_Body =
				@"BEGIN:VCALENDAR
VERSION:2.0
METHOD:REQUEST
PRODID:aa
BEGIN:VEVENT
UID:d8caee40-a028-41dd-b42c-894ce14c609f
CLASS:PUBLIC
STATUS:CONFIRMED
DTSTAMP:20180515T044117Z
DTSTART:20180516T144117
DTEND:20180516T154117
ORGANIZER;CN=sc:MAILTO:sam@wisetechglobal.com
DESCRIPTION:oops
ATTENDEE;CN=wc;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE:MAILTO:
 wilson.chen@wisetechglobal.com
END:VEVENT
END:VCALENDAR
";
			var message = mi.BuildMimeMessage();

			AssertEquals(1, message.BodyParts.Count());
			var calPart = message.BodyParts.OfType<TextPart>().First();
			AssertEquals(mi.MI_Body, Encoding.UTF8.GetString(calPart.Content.Stream.ToByteArray()));
			AssertNull(calPart.ContentDisposition);
			AssertEquals("calendar", calPart.ContentType.MediaSubtype);
		}

		public void TestBuildMessageUseAlternativeViewWithCancellationVCalendarMessage()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi.MI_ContentType = EmailContentTypes.Calendar.ContentTypeCode;
			mi.MI_Body =
				@"BEGIN:VCALENDAR
VERSION:2.0
METHOD:CANCEL
PRODID:aa
BEGIN:VEVENT
UID:d8caee40-a028-41dd-b42c-894ce14c609f
CLASS:PUBLIC
STATUS:CONFIRMED
DTSTAMP:20180515T044117Z
DTSTART:20180516T144117
DTEND:20180516T154117
ORGANIZER;CN=sc:MAILTO:sam@wisetechglobal.com
DESCRIPTION:oops
ATTENDEE;CN=wc;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE:MAILTO:
 wilson.chen@wisetechglobal.com
STATUS:CANCELLED
END:VEVENT
END:VCALENDAR
";
			var message = mi.BuildMimeMessage();

			AssertEquals(1, message.BodyParts.Count());
			var calPart = message.BodyParts.OfType<TextPart>().First();
			AssertEquals("CANCEL", calPart.ContentType.Parameters.First(x => x.Name == "method").Value);
		}

		[ExpectNoExceptions]
		public void TestBuildMessageUseAlternativeViewWithVCalendarMessage_InvalidTime()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi.MI_ContentType = EmailContentTypes.Calendar.ContentTypeCode;
			mi.MI_Body =
				@"BEGIN:VCALENDAR
VERSION:2.0
METHOD:REQUEST
PRODID:aa
BEGIN:VEVENT
UID:d8caee40-a028-41dd-b42c-894ce14c609f
CLASS:PUBLIC
STATUS:CONFIRMED
DTSTAMP:20180515T044117Z
DTSTART:<Invalid>
DTEND:<Invalid>
ORGANIZER;CN=sc:MAILTO:sam@wisetechglobal.com
DESCRIPTION:oops
ATTENDEE;CN=wc;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE:MAILTO:
 wilson.chen@wisetechglobal.com
END:VEVENT
END:VCALENDAR
";
			var message = mi.BuildMimeMessage();
			var calPart = message.BodyParts.OfType<TextPart>().First();
			AssertEquals(mi.MI_Body, Encoding.UTF8.GetString(calPart.Content.Stream.ToByteArray()));
		}

		#region Recipients Tests

		public void TestBuildMessage_Recipients_QueuedEmail()
		{
			var mailItem = GetMailItemForRecipientTests();
			mailItem.MI_Status = MailStatus.Queued;
			AssertBuildMessage_Recipients(mailItem);
		}

		public void TestBuildMessage_Recipients_DeliveredEmail()
		{
			var mailItem = GetMailItemForRecipientTests();
			mailItem.MI_Status = MailStatus.Sent;
			foreach (IMailRecipient recipient in mailItem.MailRecipients)
			{
				recipient.MR_DeliveredTime = ZDateTime.Now;
			}
			AssertBuildMessage_Recipients(mailItem);
		}

		public void TestBuildMessage_Recipients_FailedEmail()
		{
			var mailItem = GetMailItemForRecipientTests();
			mailItem.MI_Status = MailStatus.Failed;
			foreach (IMailRecipient recipient in mailItem.MailRecipients)
			{
				recipient.MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			}
			AssertBuildMessage_Recipients(mailItem);
		}

		public void TestBuildMessage_Recipients_WaitingForAcknowledgementEmail()
		{
			var mailItem = GetMailItemForRecipientTests();
			mailItem.MI_Status = MailStatus.QueuedWithAck;
			foreach (IMailRecipient recipient in mailItem.MailRecipients)
			{
				recipient.MR_LastAttempt = ZDateTime.Now.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			}
			AssertBuildMessage_Recipients(mailItem);
		}

		void AssertBuildMessage_Recipients(MailItem mailItem)
		{
			var message = mailItem.BuildMimeMessage();
			AssertEquals(3, message.To.Count);
			AssertEquals("To, Recipient|to@host.com", message.To[0].Name + "|" + ((MailboxAddress)message.To[0]).Address);
			AssertEquals("|didier.drogba@cargowise.com", message.To[1].Name + "|" + ((MailboxAddress)message.To[1]).Address);
			AssertEquals("Wayne Rooney|wayne.rooney@cargowise.com", message.To[2].Name + "|" + ((MailboxAddress)message.To[2]).Address);
			AssertEquals(2, message.Cc.Count);
			AssertEquals("Cc, Recipient|cc@host", message.Cc[0].Name + "|" + ((MailboxAddress)message.Cc[0]).Address);
			AssertEquals("Terry, John|john.terry@cargowise.com", message.Cc[1].Name + "|" + ((MailboxAddress)message.Cc[1]).Address);
			AssertEquals(2, message.Bcc.Count);
			AssertEquals("bcc, Recipient|bcc@host", message.Bcc[0].Name + "|" + ((MailboxAddress)message.Bcc[0]).Address);
			AssertEquals("Lampard, Frank|flampard@cargowise.com", message.Bcc[1].Name + "|" + ((MailboxAddress)message.Bcc[1]).Address);
		}

		MailItem GetMailItemForRecipientTests()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.AddRecipientForUserCommunication("\"To, Recipient\" <to@host.com>,didier.drogba@cargowise.com, Wayne Rooney <wayne.rooney@cargowise.com>", Business.MailRecipient.RecipientTypes.TO);
			mailItem.AddRecipientForUserCommunication("\"Cc, Recipient\" <cc@host>, \"Terry, John\"<john.terry@cargowise.com>", Business.MailRecipient.RecipientTypes.CC);
			mailItem.AddRecipientForUserCommunication("\"bcc, Recipient\" <bcc@host>, \"Lampard, Frank\" <flampard@cargowise.com>", Business.MailRecipient.RecipientTypes.BCC);

			return mailItem;
		}

		#endregion

		public void TestBuildMessageWithXmlAttachment()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi.MailAttachments.AddNew();
			mi.MailAttachments[0].MA_FileName = "Attachement.xml";
			byte[] xmlData = Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><SomeElement><SomeData>foo</SomeData><SomeData>bar</SomeData></SomeElement>");
			mi.MailAttachments[0].MA_Data = xmlData;
			mi.MI_Body = "";
			mi.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			var message = mi.BuildMimeMessage();
			AssertEquals(1, message.Attachments.Count());
			var attachment = message.Attachments.First() as TextPart;
			AssertEquals("Attachement.xml", attachment.FileName);
			AssertEquals(xmlData, attachment.Content.Stream.ToByteArray());
			AssertEquals("Xml attachments should be base64 encoded", ContentEncoding.Base64, attachment.ContentTransferEncoding);
			AssertContentDispositionParameterEncodingMethod(attachment);
		}

		public void TestBuildMessageWithPdfAttachment()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi.MailAttachments.AddNew();
			mi.MailAttachments[0].MA_FileName = "Attachement.pdf";
			mi.MI_Body = "";
			mi.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			var message = mi.BuildMimeMessage();
			AssertEquals(1, message.Attachments.Count());
			var attachment = message.Attachments.OfType<MimePart>().First();
			AssertEquals("Attachement.pdf", attachment.FileName);
			AssertEquals("application/pdf", attachment.ContentType.MimeType);
			AssertContentDispositionParameterEncodingMethod(attachment);
		}

		public void TestBuildMessageWithEmlAttachment()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			var attachment = mi.MailAttachments.AddNew();
			attachment.MA_FileName = "Attachment.eml";
			attachment.MA_Data = GreetingCardSpamEmail;

			var attachment2 = mi.MailAttachments.AddNew();
			attachment2.MA_FileName = "Attachment.xls";

			mi.MI_Body = "This is a default email";
			mi.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			var message = mi.BuildMimeMessage();

			AssertEquals(2, message.Attachments.Count());
			var emailPart = message.Attachments.OfType<MessagePart>().First();
			var excelPart = message.Attachments.OfType<MimePart>().Last();

			AssertEquals("Attachment.eml", emailPart.ContentType.Name);
			AssertEquals("message/rfc822", emailPart.ContentType.MimeType);
			AssertContentDispositionParameterEncodingMethod(emailPart);

			AssertEquals("Attachment.xls", excelPart.FileName);
			AssertEquals("application/vnd.ms-excel", excelPart.ContentType.MimeType);
			AssertEquals(ContentEncoding.Base64, excelPart.ContentTransferEncoding);
			AssertContentDispositionParameterEncodingMethod(excelPart);
		}

		public void TestBuildMessageWithEmlAttachmentWithInvalidCharacters()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			var attachment = mi.MailAttachments.AddNew();
			attachment.MA_FileName = "Attachment\\/:*<>|.eml";
			attachment.MA_Data = GreetingCardSpamEmail;

			var attachment2 = mi.MailAttachments.AddNew();
			attachment2.MA_FileName = "Attachment.xls";

			mi.MI_Body = "This is a default email";
			mi.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			var message = mi.BuildMimeMessage();

			AssertEquals(2, message.Attachments.Count());
			var emailPart = message.Attachments.OfType<MessagePart>().First();
			var excelPart = message.Attachments.OfType<MimePart>().Last();

			AssertEquals("Attachment.eml", emailPart.ContentType.Name);
			AssertEquals("message/rfc822", emailPart.ContentType.MimeType);
			AssertContentDispositionParameterEncodingMethod(emailPart);

			AssertEquals("Attachment.xls", excelPart.FileName);
			AssertEquals("application/vnd.ms-excel", excelPart.ContentType.MimeType);
			AssertEquals(ContentEncoding.Base64, excelPart.ContentTransferEncoding);
			AssertContentDispositionParameterEncodingMethod(excelPart);
		}

		public void TestBuildMessage_ReturnPathShouldBeSetAsRegistryValue()
		{
			using (RawDataRegistry.Instance.AllowEmailsToBeSentFromUsersAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "registry@abc.com"))
			{
				var mail1 = Factory.NewWithValidTestData<MailItem>();
				mail1.MI_From = "user <user@wise.com>";
				mail1.MI_BusinessEntityID = Guid.NewGuid().ToString();
				var mimeMessage = mail1.BuildMimeMessage();
				AssertEquals("<registry@abc.com>", mimeMessage.Headers["Return-Path"]);
			}

			using (RawDataRegistry.Instance.AllowEmailsToBeSentFromUsersAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "registry@abc.com"))
			{
				using (RawDataRegistry.Instance.SelectNDRPath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.SelectNDRPath.Codes.MB))
				{
					var mail = Factory.NewWithValidTestData<MailItem>();
					mail.MI_BusinessEntityID = Guid.NewGuid().ToString();
					var mimeMessage = mail.BuildMimeMessage();

					AssertEquals("<registry@abc.com>", mimeMessage.Headers["Return-Path"]);
				}

				using (RawDataRegistry.Instance.SelectNDRPath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.SelectNDRPath.Codes.CP))
				{
					var mail1 = Factory.NewWithValidTestData<MailItem>();
					mail1.MI_From = "staff1 <staff1@abcdef.com>";
					mail1.MI_BusinessEntityID = Guid.NewGuid().ToString();
					var mimeMessage1 = mail1.BuildMimeMessage();
					AssertEquals("<staff1@abcdef.com>", mimeMessage1.Headers["Return-Path"]);

					var mail2 = Factory.NewWithValidTestData<MailItem>();
					mail2.MI_From = "staff2 <staff2@abc.com>";
					mail2.MI_BusinessEntityID = Guid.NewGuid().ToString();
					var mimeMessage2 = mail2.BuildMimeMessage();
					AssertEquals("<registry@abc.com>", mimeMessage2.Headers["Return-Path"]);
				}

				using (RawDataRegistry.Instance.SelectNDRPath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.SelectNDRPath.Codes.SU))
				{
					var mail = Factory.NewWithValidTestData<MailItem>();
					mail.MI_From = "staff3 <staff3@abcdefghi.com>";
					mail.MI_BusinessEntityID = Guid.NewGuid().ToString();
					var mimeMessage = mail.BuildMimeMessage();
					AssertEquals("<staff3@abcdefghi.com>", mimeMessage.Headers["Return-Path"]);
				}
			}
		}
		public void TestFillAttachments_AttachmentEncodingShouldBeSetAsRegistryValue()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi.MailAttachments.AddNew();
			mi.MailAttachments[0].MA_FileName = "Attachement.pdf";
			mi.MI_Body = "";
			mi.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;

			var message = mi.BuildMimeMessage();
			AssertEquals(1, message.Attachments.Count());

			var attachment = message.Attachments.OfType<MimePart>().First();
			AssertEquals("Attachement.pdf", attachment.FileName);
			AssertEquals("application/pdf", attachment.ContentType.MimeType);
			AssertContentDispositionParameterEncodingMethod(attachment);

			using (RawDataRegistry.Instance.AttachmentEncodingFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "RFC2231"))
			{
				message = mi.BuildMimeMessage();
				AssertEquals(1, message.Attachments.Count());

				attachment = message.Attachments.OfType<MimePart>().First();
				AssertContentDispositionParameterEncodingMethod(attachment, ParameterEncodingMethod.Rfc2231);
			}
		}

		public void TestFillAttachments_AttachmentEncodingWillBeSetToRfc2047IfErrorDetectedForRegistryValue()
		{
			var mi = MailTestHelpers.GetMailItemWithoutAttachments(Factory);
			mi.MailAttachments.AddNew();
			mi.MailAttachments[0].MA_FileName = "Attachement.pdf";
			mi.MI_Body = "";
			mi.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;

			using (RawDataRegistry.Instance.AttachmentEncodingFormat.DataType.SuspendValidation())
			using (RawDataRegistry.Instance.AttachmentEncodingFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "RFC2298"))
			{
				ErrorReporter.Clear();
				var message = mi.BuildMimeMessage();
				AssertEquals(1, message.Attachments.Count());

				var attachment = message.Attachments.OfType<MimePart>().First();
				AssertEquals("Attachement.pdf", attachment.FileName);
				AssertEquals("application/pdf", attachment.ContentType.MimeType);
				AssertContentDispositionParameterEncodingMethod(attachment);
				AssertEquals("AttachmentEncodingParseError", ErrorReporter.LastKeyReported);
				AssertEquals("The entered attachment encoding format 'RFC2298' cannot be parsed into a valid ParameterEncodingMethod enum value.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		byte[] GreetingCardSpamEmail => resourceRetriever.Value.GetBytes("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.GreetingCardSpamEmail.eml");

		void AssertContentDispositionParameterEncodingMethod(MimeEntity mimeEntity, ParameterEncodingMethod encoding = ParameterEncodingMethod.Rfc2047)
		{
			foreach (var parameter in mimeEntity.ContentDisposition.Parameters)
			{
				AssertEquals(encoding, parameter.EncodingMethod);
			}
		}
	}
}
