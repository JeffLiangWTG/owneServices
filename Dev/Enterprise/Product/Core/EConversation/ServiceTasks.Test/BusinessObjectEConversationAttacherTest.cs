using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Integration;
using static Enterprise.EConversation.Testing.BusinessObjectEmailAttacherTest;

namespace Enterprise.EConversation.Testing
{
	sealed class BusinessObjectEConversationAttacherTest : TestCaseWithFactory
	{
		public void TestAttachEmailToJobConversation()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var dummy = Factory.New<DummyAllowAttachEmailsToEDocs>();
				Factory.Save();

				const string cid = "01@mobile";
				var email = new EmailBuilderForTesting()
					.Subject("fix this " + dummy.ReferenceNumber)
					.To("Someone", "test@edi.com.au")
					.WithAttachment("123.jpg", resourceRetriever.GetBytes("Enterprise.EConversation.ServiceTasks.Test.inlineImage.jpg"))
					.HtmlBody(FormattableString.Invariant($@"Hi, this is an email with [image: 123.jpg] subject. [cid:{cid}] additional info"))
					.GetEmail();
				var attachments = email.Message.GetFullAttachments();
				AssertEquals("Precondition", 1, attachments.Count());
				attachments.ElementAt(0).ContentId = cid;

				var eConversationAttacher = new BusinessObjectEConversationAttacher();
				eConversationAttacher.AttachEmailToJobConversation(email, Core.Constants.RefDocTypes.MiscellaneousDocument, dummy);

				var convMessages = dummy.eConversation.GetTimeOrderedMessages().ToArray();
				AssertEquals(2, convMessages.Length);

				var edoc = dummy.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("123.jpg"));
				var emailAttachmentMessageBody = $"Email Attachments\r\n[123.jpg]({ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc)})";
				var emailAttachmentMessage = convMessages.FirstOrDefault(x => x.Body == emailAttachmentMessageBody);
				AssertNotNull(emailAttachmentMessage);
				AssertEquals(MessageType.LocalInternal, emailAttachmentMessage.MessageType);

				var emailBody = "Hi, this is an email with [123.jpg] subject. [123.jpg] additional info";
				var emailBodyMessage = convMessages.FirstOrDefault(x => x.Body == emailBody);
				AssertNotNull(emailBodyMessage);
				AssertEquals(MessageType.Remote, emailBodyMessage.MessageType);
			}
		}
	}
}
