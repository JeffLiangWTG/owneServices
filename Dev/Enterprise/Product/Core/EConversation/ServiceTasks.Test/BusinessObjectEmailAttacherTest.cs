using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using MailManager;
using MimeKit;
using static Enterprise.EConversation.Testing.JobConversationTest;

namespace Enterprise.EConversation.Testing
{
	sealed class BusinessObjectEmailAttacherTest : TestCaseWithFactory
	{
		public void TestAttachEmail()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var mailItem = new BusinessObjectFactory().NewWithValidTestData<MailItem>();
			mailItem.MI_Header = "MAIL HEADER";
			mailItem.MI_Body = "MAIL BODY";
			mailItem.MI_Subject = "Enquiry\\/";
			mailItem.MI_Status = MailStatus.Unprocessed;
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			mailItem.MI_POP3UIDL = Guid.NewGuid().ToString();

			AssertEquals(0, dummy.DocManagerInfo.AllEDocs.Count);
			BusinessObjectEmailAttacher.AttachEmail(dummy, Core.Constants.RefDocTypes.MiscellaneousDocument, mailItem);
			AssertEquals("Should be marked as Processed", MailStatus.Processed, mailItem.MI_Status);
			Assert("Not yet saved", !mailItem.IsInDatabase);

			Factory.Save();
			Assert("Should be saved", mailItem.IsInDatabase);
			AssertEquals(1, dummy.DocManagerInfo.AllEDocs.Count);
			var eDoc = dummy.DocManagerInfo.AllEDocs[0];
			const string expected =
@"MAIL HEADERSubject: Enquiry\/


MAIL BODY";
			AssertEquals(expected, eDoc.GetImageDataReader().ConvertToAsciiStringAndCloseStream());
			AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, eDoc.DocType);
			AssertEquals("Enquiry.eml", eDoc.FileName);

			var noSubjectMailItem = new BusinessObjectFactory().NewWithValidTestData<MailItem>();
			noSubjectMailItem.MI_Body = "NO SUBJECT";
			noSubjectMailItem.MI_Subject = "";
			noSubjectMailItem.MI_Status = MailStatus.Unprocessed;
			noSubjectMailItem.MI_Direction = DirectionList.Codes.Receive;
			noSubjectMailItem.MI_POP3UIDL = Guid.NewGuid().ToString();
			BusinessObjectEmailAttacher.AttachEmail(dummy, Core.Constants.RefDocTypes.MiscellaneousDocument, noSubjectMailItem);
			AssertEquals("Should be marked as Processed", MailStatus.Processed, noSubjectMailItem.MI_Status);
			Assert("Not yet saved", !noSubjectMailItem.IsInDatabase);

			Factory.Save();
			Assert("Should be saved", noSubjectMailItem.IsInDatabase);
			AssertEquals(2, dummy.DocManagerInfo.AllEDocs.Count);
			var eDoc2 = dummy.DocManagerInfo.AllEDocs[1];
			AssertEquals("\r\n\r\nNO SUBJECT", eDoc2.GetImageDataReader().ConvertToAsciiStringAndCloseStream());
			AssertEquals(Core.Constants.RefDocTypes.MiscellaneousDocument, eDoc2.DocType);
			AssertEquals("Email.eml", eDoc2.FileName);
		}

		public void TestAttachEmail_SavedEventHandlerShouldOnlyBeFiredOnce()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();

			var mailFactorySavedCount = 0;
			var mailFactory = new BusinessObjectFactory();
			var mailItem = mailFactory.NewWithValidTestData<MailItem>();
			mailItem.MI_Direction = DirectionList.Codes.Transmit;
			mailItem.MI_Status = MailStatus.Unprocessed;
			mailItem.MI_Subject = "Mail 1";
			mailItem.MI_POP3UIDL = Guid.NewGuid().ToString();
			mailFactory.Saved += delegate { mailFactorySavedCount++; };

			var anotherMailFactorySavedCount = 0;
			var anotherMailFactory = new BusinessObjectFactory();
			var anotherMailItem = anotherMailFactory.NewWithValidTestData<MailItem>();
			anotherMailItem.MI_Status = MailStatus.Unprocessed;
			anotherMailItem.MI_Subject = "Mail 2";
			anotherMailItem.MI_Direction = DirectionList.Codes.Receive;
			anotherMailItem.MI_POP3UIDL = Guid.NewGuid().ToString();
			anotherMailFactory.Saved += delegate { anotherMailFactorySavedCount++; };

			BusinessObjectEmailAttacher.AttachEmail(dummy, Core.Constants.RefDocTypes.MiscellaneousDocument, mailItem, anotherMailItem);
			dummy.Factory.Save();
			AssertEquals(2, dummy.DocManagerInfo.AllEDocs.Count);
			AssertContains("Mail 1", dummy.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains("Mail 2", dummy.DocManagerInfo.AllEDocs[1].FileName);
			AssertEquals(MailStatus.Processed, mailItem.MI_Status);
			AssertEquals(MailStatus.Processed, anotherMailItem.MI_Status);
			Assert(mailItem.IsInDatabase);
			Assert(anotherMailItem.IsInDatabase);
			AssertEquals(1, mailFactorySavedCount);
			AssertEquals(1, anotherMailFactorySavedCount);

			dummy.Z0_Description = "Changing the description";
			dummy.Factory.Save();
			AssertEquals("Factory.Saved event handler should have been unhooked", 1, mailFactorySavedCount);
			AssertEquals("Factory.Saved event handler should have been unhooked", 1, anotherMailFactorySavedCount);
		}

		public void TestGetContentsForOutlookExpressMessage()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_Header = "MAIL HEADER";
			mailItem.MI_Body = "MAIL BODY";
			const string expected =
@"MAIL HEADER

MAIL BODY";
			AssertEquals(expected, BusinessObjectEmailAttacher.GetEmailTextForOutlookExpressMessage(mailItem));

			var contentAsBytes = BusinessObjectEmailAttacher.GetContentsForOutlookExpressMessage(mailItem);
			AssertEquals(expected, Encoding.ASCII.GetString(contentAsBytes));
		}

		public void TestProcessAttachmentsShouldNotReattachAlreadyAttachedInlineImage()
		{
			var dummy1 = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var attachmentBytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.inlineImage.jpg");
			const string cid = "01@mobile";
			var email1 = new EmailBuilderForTesting()
				.Subject("fix this " + dummy1.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", attachmentBytes)
				.HtmlBody(FormattableString.Invariant($@"Hi, this is an email with [image: 123.jpg] subject. [cid:{cid}] additional info"))
				.GetEmail();
			var attachments1 = email1.Message.GetFullAttachments();
			AssertEquals("Precondition", 1, attachments1.Count());
			attachments1.ElementAt(0).ContentId = cid;

			var email2 = new EmailBuilderForTesting()
				.Subject("fix this " + dummy1.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("456.jpg", attachmentBytes)
				.HtmlBody(FormattableString.Invariant($@"Hi, this is an email with [image: 456.jpg] subject. [cid:{cid}] additional info"))
				.GetEmail();
			var attachments2 = email2.Message.GetFullAttachments();
			attachments2.ElementAt(0).ContentId = cid;
			AssertEquals("Precondition", 1, attachments2.Count());

			var originalEDocCount = dummy1.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher1 = new BusinessObjectEConversationAttacher();
			var eConversationAttacher2 = new BusinessObjectEConversationAttacher();
			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy1, email1, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher1);
			var bodyForConversation = convMessages.EmailBodyMessage;
			AssertEquals("Should add an EDoc for inline image", originalEDocCount + 1, dummy1.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Should set Document type on EDoc", Core.Constants.RefDocTypes.MiscellaneousDocument, dummy1.DocManagerInfo.AllEDocs[0].DocType);
			AssertContains("Should contain filename", "Hi, this is an email with [123.jpg]", bodyForConversation);

			var edoc = dummy1.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("123.jpg"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[123.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);

			convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy1, email2, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher2);
			bodyForConversation = convMessages.EmailBodyMessage;
			AssertEquals("Should not add an EDoc for inline image which has already been added", originalEDocCount + 1, dummy1.DocManagerInfo.AllEDocs.Count);
			AssertContains("Should change filename to match original eDoc filename", "Hi, this is an email with [123.jpg]", bodyForConversation);

			attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals(string.Empty, attachmentMessage);
		}

		public void TestProcessAttachmentsSrcAltFormatShouldAttachInlineImagesAsEDoc()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			const string cid = "01@mobile";
			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.inlineImage.jpg"))
				.HtmlBody(FormattableString.Invariant($@"Hi, this is an email with subject. [EDI logo] additional info --- forwarded message --- <p class=""MsoNormal""><span style=""font-size:9.0pt;font-family:&quot;Arial&quot;,sans-serif;color:black""><img border=""0"" width=""600"" height=""80"" style=""width:6.25in;height:.8333in"" id=""Picture_x0020_1"" src=""cid:{cid}"" alt=""EDI logo""><o:p></o:p></span></p>"))
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 1, attachments.Count());
			attachments.ElementAt(0).ContentId = cid;

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();

			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			var bodyForConversation = convMessages.EmailBodyMessage;
			AssertEquals("Should have added an EDoc for inline image", originalEDocCount + 1, dummy.DocManagerInfo.AllEDocs.Count);
			AssertContains("Should have changed format for inline image in bodyForConversation", "Hi, this is an email with subject. [123.jpg] additional info", bodyForConversation);

			var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("123.jpg"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[123.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);
		}

		public void TestProcessAttachmentsCIDFormatShouldAttachInlineImagesAsEDoc()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			const string cid = "01@mobile";
			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.inlineImage.jpg"))
				.HtmlBody(FormattableString.Invariant($@"Hi, this is an email with [image: 123.jpg] subject. [cid:{cid}] additional info"))
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			attachments.ElementAt(0).ContentId = cid;
			AssertEquals("Precondition", 1, attachments.Count());

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();

			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			var bodyForConversation = convMessages.EmailBodyMessage;

			AssertEquals("Should have added an EDoc for inline image", originalEDocCount + 1, dummy.DocManagerInfo.AllEDocs.Count);
			AssertContains("Should have changed format for inline image in bodyForConversation", "Hi, this is an email with [123.jpg] subject. [123.jpg] additional info", bodyForConversation);

			var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("123.jpg"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[123.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);
		}

		public void TestSkipsImagesSmallerThanSize_Inline()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			const string cid1 = "01@mobile";
			const string cid2 = "02@mobile";
			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.logo1.jpg"))
				.WithAttachment("456.jpg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.image1000.jpg"))
				.HtmlBody(FormattableString.Invariant($@"Hi, this is an email with [image: 123.jpg] subject. [cid:{cid1}] additional info [image: 456.jpg] [cid:{cid2}]"))
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			attachments.ElementAt(0).ContentId = cid1;
			attachments.ElementAt(1).ContentId = cid2;
			AssertEquals("Precondition", 2, attachments.Count());

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();

			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			var bodyForConversation = convMessages.EmailBodyMessage;
			AssertEquals("Should not have added an EDoc for a small inline image, only for the large one", originalEDocCount + 1, dummy.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Should have added a larger image", "456.jpg", dummy.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains("Should skip the image and leave as placeholder", "Hi, this is an email with  subject.  additional info [456.jpg] [456.jpg]", bodyForConversation);

			var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("456.jpg"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[456.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);
		}

		public void TestSkipsImagesSmallerThanSize_Attachments()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.logo1.jpg"))
				.WithAttachment("456.jpg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.image1000.jpg"))
				.HtmlBody(@"Hi, this is an email with subject.")
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 2, attachments.Count());

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();
			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			AssertEquals("Should not have added an EDoc for a small inline image, only for the large one", originalEDocCount + 1, dummy.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Should have added a larger image", "456.jpg", dummy.DocManagerInfo.AllEDocs[0].FileName);

			var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("456.jpg"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[456.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);
		}

		public void TestSkipsImagesSmallerThanSize_NonImage()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("text.txt", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.text.txt"))
				.HtmlBody(@"Hi, this is an email with subject.")
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 1, attachments.Count());

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();
			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			AssertEquals("Should have added an EDoc for a small text file", originalEDocCount + 1, dummy.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Should have added a text file", "text.txt", dummy.DocManagerInfo.AllEDocs[0].FileName);

			var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("text.txt"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[text.txt]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);
		}

		public void TestDuplicatesInSameReply_Attachments()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var attachmentBytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.inlineImage.jpg");
			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", attachmentBytes)
				.WithAttachment("456.jpg", attachmentBytes)
				.HtmlBody(@"Hi, this is an email with subject.")
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 2, attachments.Count());

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();

			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			var bodyForConversation = convMessages.EmailBodyMessage;
			AssertEquals("Should add just one of the images", originalEDocCount + 1, dummy.DocManagerInfo.AllEDocs.Count);
			AssertContains("Should append image at the end", "Hi, this is an email with subject.\r\nAttachment 1: [123.jpg]\r\nAttachment 2: [123.jpg]", bodyForConversation);

			var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("123.jpg"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[123.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);
		}

		public void TestDuplicatesInSameReply_Inline()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var attachmentBytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.inlineImage.jpg");
			const string cid1 = "01@mobile";
			const string cid2 = "02@mobile";
			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", attachmentBytes)
				.WithAttachment("456.jpg", attachmentBytes)
				.HtmlBody(FormattableString.Invariant($@"Hi, this is an email with [image: 123.jpg] subject. [cid:{cid1}] additional info [image: 456.jpg] [cid:{cid2}]"))
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			attachments.ElementAt(0).ContentId = cid1;
			attachments.ElementAt(1).ContentId = cid2;
			AssertEquals("Precondition", 2, attachments.Count());

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();

			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			var bodyForConversation = convMessages.EmailBodyMessage;
			AssertEquals("Should add just one of the images", originalEDocCount + 1, dummy.DocManagerInfo.AllEDocs.Count);
			AssertEquals("The second image should have the same filename", "Hi, this is an email with [123.jpg] subject. [123.jpg] additional info [123.jpg] [123.jpg]", bodyForConversation);

			var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("123.jpg"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[123.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);
		}

		public void TestDuplicatesInDummy()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.inlineImage.jpg"))
				.HtmlBody(@"Hi, this is an email with subject.")
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 1, attachments.Count());

			var edoc = dummy.DocManagerInfo.AddFileOrDocument(attachments.ElementAt(0).GetData(), "xxx.jpg", Core.Constants.RefDocTypes.MiscellaneousDocument);

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();

			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			var bodyForConversation = convMessages.EmailBodyMessage;
			AssertEquals("Should add just one of the images", originalEDocCount, dummy.DocManagerInfo.AllEDocs.Count);
			AssertContains("Should append image at the end", "Hi, this is an email with subject.\r\nAttachment 1: [xxx.jpg]", bodyForConversation);

			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals(string.Empty, attachmentMessage);
		}

		public void TestDuplicatesIndummy_Msg()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.msg", new byte[] { 1, 2, 3 })
				.HtmlBody(@"Hi, this is an email with subject.")
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 1, attachments.Count());
			dummy.DocManagerInfo.AddFileOrDocument(attachments.ElementAt(0).GetData(), "xxx.msg", Core.Constants.RefDocTypes.MiscellaneousDocument);

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();
			BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			AssertEquals("Should add just one of the msg files", originalEDocCount, dummy.DocManagerInfo.AllEDocs.Count);
		}

		public void TestProcessAttachments_MSGAttachments()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();
			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.msg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.msgAttachment.msg"))
				.HtmlBody(@"Hi, this is an email with subject.")
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 1, attachments.Count());

			var eConversationAttacher = new BusinessObjectEConversationAttacher();
			BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			AssertEquals("Should add the msg file", 1, dummy.DocManagerInfo.AllEDocs.Count);
		}

		public void TestNoFileExtension()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("msg-no-extension", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.test.eml"), new ContentType("message", "rfc822"))
				.HtmlBody(@"Hi, this is an email with subject.")
				.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 1, attachments.Count());

			var eConversationAttacher = new BusinessObjectEConversationAttacher();
			BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			AssertEquals("Should set extension based on type", "EML", dummy.DocManagerInfo.AllEDocs[0].DataType);
		}

		public void TestNoFileName()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var emailBuilderForTesting = new EmailBuilderForTesting();
			var attachmentBytes = resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.test.eml");
			var messageBytes = Encoding.UTF8.GetBytes(string.Format($@"From: Rick Yuan <Rick.Yuan@wisetechglobal.com>
To: Someone <test@edi.com.au>
Subject: fix this {dummy.ReferenceNumber}
Date: Fri, 3 Mar 2023 09:48:52 +0000
Message-ID:
	<ABCDEFGHIJK@LMNOPQ.ausprd01.prod.outlook.com>
Accept-Language: en-US
Content-Language: zh-CN
Content-Type: multipart/mixed;
	boundary=""_004_ABCDEFGHIJKLMNOPQausp_""
MIME-Version: 1.0

--_004_ABCDEFGHIJKLMNOPQausp_
Content-Type: multipart/alternative;
	boundary=""_000_ABCDEFGHIJKLMNOPQausp_""

--_000_ABCDEFGHIJKLMNOPQausp_
Content-Type: text/plain; charset=""us-ascii""

Hi, this is an email with subject.

--_000_ABCDEFGHIJKLMNOPQausp_
Content-Type: text/html; charset=""us-ascii""

Hi, this is an email with subject.

--_000_ABCDEFGHIJKLMNOPQausp_--

--_004_ABCDEFGHIJKLMNOPQausp_
Content-Type: message/rfc822; name=""""
Content-Disposition: attachment; filename="""";
	creation-date=""Fri, 03 Mar 2023 09:48:04 GMT"";
	modification-date=""Fri, 03 Mar 2023 09:48:53 GMT""
Content-Transfer-Encoding: base64

{Convert.ToBase64String(attachmentBytes)}

--_004_ABCDEFGHIJKLMNOPQausp_--
"));
			var message = MimeMessageExtensions.CreateMessageFromEml(messageBytes);
			emailBuilderForTesting.Message = message;
			var email = emailBuilderForTesting.GetEmail();
			var attachments = email.Message.GetFullAttachments();
			AssertEquals("Precondition", 1, attachments.Count());
			var eConversationAttacher = new BusinessObjectEConversationAttacher();
			BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			AssertEquals("Should set extension based on type", "EML", dummy.DocManagerInfo.AllEDocs[0].DataType);
		}

		public void TestProcessAttachmentsShouldAttachNonInlineImagesAsEDoc()
		{
			var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
			Factory.Save();

			var email = new EmailBuilderForTesting()
				.Subject("fix this " + dummy.ReferenceNumber)
				.To("Someone", "test@edi.com.au")
				.WithAttachment("123.jpg", resourceRetriever.Value.GetBytes("Enterprise.EConversation.ServiceTasks.Test.inlineImage.jpg"))
				.HtmlBody(@"Hi, this is an email with subject.")
				.GetEmail();

			var originalEDocCount = dummy.DocManagerInfo.AllEDocs.Count;
			var eConversationAttacher = new BusinessObjectEConversationAttacher();

			var convMessages = BusinessObjectEmailAttacher.ProcessAttachments(dummy, email, Core.Constants.RefDocTypes.MiscellaneousDocument, eConversationAttacher);
			var bodyForConversation = convMessages.EmailBodyMessage;
			AssertEquals("Should have added an EDoc for inline image", originalEDocCount + 1, dummy.DocManagerInfo.AllEDocs.Count);
			AssertContains("Should append image to the end", "Hi, this is an email with subject.\r\nAttachment 1: [123.jpg]", bodyForConversation);

			var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("123.jpg"));
			var attachmentMessage = convMessages.AttachmentMessage;
			AssertEquals($"Email Attachments\r\n[123.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})", attachmentMessage);
		}

		public sealed class DummyAllowAttachEmailsToEDocs : DummyBusinessObject, IAllowAttachEmailsToEDocs, IConversationProvider
		{
			public DummyAllowAttachEmailsToEDocs(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string ReferenceNumber => Z0_Description;

			public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Shipment));

			DocManagerInfo docManagerInfo;

			public JobConversation eConversation
			{
				get
				{
					if (conversation == null)
					{
						var newFactory = new BusinessObjectFactory();
						var conversationInNewFactory = newFactory.New<JobConversationForTest>();
						conversationInNewFactory.JCC_ParentID = PK;
						conversationInNewFactory.JCC_ParentTableCode = TablePrefix;
						newFactory.Save();

						conversation = Factory.Load<JobConversationForTest>(conversationInNewFactory.PK);
						RegisterEditableChildObject(conversation);
					}

					return conversation;
				}
			}
			JobConversation conversation;

			public ModuleIdentifier ParentModule => throw new NotImplementedException();

			public ControllerID ParentController => throw new NotImplementedException();

			public IEnumerable<Business.RelatedParty> AdditionalParticipants => throw new NotImplementedException();

			public bool SendEmailNotificationsOnSave => throw new NotImplementedException();

			public string EmailSubjectContentOverride => throw new NotImplementedException();

			public string FromAddressOverride => throw new NotImplementedException();

			public void RunConversationUpdateActionBeforeSaving() => throw new NotImplementedException();

			public NotificationEmailTemplate NotificationEmailTemplateOverride => throw new NotImplementedException();
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
