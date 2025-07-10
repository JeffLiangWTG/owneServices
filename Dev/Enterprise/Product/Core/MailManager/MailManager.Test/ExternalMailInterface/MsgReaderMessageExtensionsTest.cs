using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using MsgReader.Outlook;

namespace Enterprise.MailManager.ExternalMailInterface
{
	sealed class MsgReaderMessageExtensionsTest : TestCaseWithFactory
	{
		public void TestParseAttachments()
		{
			var messageWithMessagerfc822AttachmentData = resourceRetriever.GetBytes("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.messagerfc822.msg");
			using (var memoryStream = new MemoryStream(messageWithMessagerfc822AttachmentData, false))
			{
				using (var messageWithMessagerfc822Attachment = new Storage.Message(memoryStream))
				{
					var attachments1 = messageWithMessagerfc822Attachment.Attachments;
					AssertEquals("Attachments Count", 2, attachments1.Count);

					AssertEquals("Message1-Attachment1-FileName", "How do I check my super transactions.msg", ((Storage)attachments1[0]).GetName());
					var message1InnerMsgData = ((Storage)attachments1[0]).GetData();
					using (var message1InnerMsgStream = new MemoryStream(message1InnerMsgData, false))
					{
						using (var message1InnerMsg = new Storage.Message(message1InnerMsgStream))
						{
							AssertEquals("Message1-Attachment1-Email-Subject", "How do I check my super transactions?", message1InnerMsg.Subject);
						}
					}
					AssertEquals("Message1-Attachment2-FileName", "New Bitmap Image.jpg", ((Storage)attachments1[1]).GetName());
					AssertEquals("Message1-Attachment2-FileData", 51496, ((Storage)attachments1[1]).GetData().Length);
				}
			}

			var messageWithMultipleAttachmentTypesData = resourceRetriever.GetBytes("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.MultipleAttachmentTypes.msg");
			using (var memoryStream = new MemoryStream(messageWithMultipleAttachmentTypesData, false))
			{
				using (var messageWithMultipleAttachmentTypes = new Storage.Message(memoryStream))
				{
					var attachments2 = messageWithMultipleAttachmentTypes.Attachments;
					AssertEquals("Attachments Count", 5, attachments2.Count);

					AssertEquals("Message2-Attachment1-FileName", "image001.png", ((Storage)attachments2[0]).GetName());
					AssertEquals("Message2-Attachment1-FileData", 730, ((Storage)attachments2[0]).GetData().Length);
					AssertEquals("Message2-Attachment2-FileName", "attachmentImage.png", ((Storage)attachments2[1]).GetName());
					AssertEquals("Message2-Attachment2-FileData", 131, ((Storage)attachments2[1]).GetData().Length);
					AssertEquals("Message2-Attachment3-FileName", "attachmentTxt.txt", ((Storage)attachments2[2]).GetName());
					AssertEquals("Message2-Attachment3-FileData", 16, ((Storage)attachments2[2]).GetData().Length);
					AssertEquals("Message2-Attachment4-FileName", "attachmentEmail.eml", ((Storage)attachments2[3]).GetName());
					AssertEquals("Message2-Attachment4-FileData", 4196, ((Storage)attachments2[3]).GetData().Length);
					AssertEquals("Message2-Attachment5-FileName", "Empty Email.msg", ((Storage)attachments2.ElementAt(4)).GetName());
					var message2InnerMsgData = ((Storage)attachments2.ElementAt(4)).GetData();
					using (var message2InnerMsgStream = new MemoryStream(message2InnerMsgData, false))
					{
						using (var message2InnerMsg = new Storage.Message(message2InnerMsgStream))
						{
							AssertEquals("Message1-Attachment1-Email-Subject", "Empty Email", message2InnerMsg.Subject);
						}
					}
				}
			}
		}

		public void TestGetMessageData()
		{
			var messageData = resourceRetriever.GetBytes("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.messagerfc822.msg");
			using (var memoryStream = new MemoryStream(messageData, false))
			{
				using (var message = new Storage.Message(memoryStream))
				{
					AssertEquals("Get Message Data", messageData.Length, message.GetData().Length);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		EmbeddedResourceRetriever resourceRetriever;
	}
}
