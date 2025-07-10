using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MailManager.Business.Testing
{
	[TestedType(typeof(MailItemCopySender))]
	sealed class MailItemCopySenderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSuccessfullySendCopyTo()
		{
			ZDateTime testStarted = ZDateTime.UtcNow;

			MailItem item1 = GetNewMailItem();
			item1.RunPreSaveValidation();
			Assert("Save without Error", !item1.HasNotifications());

			MailItem item2 = GetNewMailItem();
			item2.RunPreSaveValidation();
			Assert("Save without Error", !item2.HasNotifications());

			MailItem item3 = GetNewMailItem();
			item3.MI_Header = "To: bad@example\r\nCc: bad@example\r\nReceived: from foo by bar\r\nTue, 6 Dec 2005 10:10:10 +1100\r\nReceived: from foo by bar\r\nTue, 6 Dec 2005 10:10:11 +1100\r\nX-Junk: foobar\r\n";
			item3.MI_Direction = MailDirection.Receive;
			item3.MI_Status = MailStatus.Failed;
			item3.MI_Subject = "Subject";
			item3.MI_Body = "Message Body";
			item3.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			item3.RunPreSaveValidation();
			Assert("Save without Error", !item3.HasNotifications());

			Factory.Save();

			MailItemCopySender copySender = new MailItemCopySender(new MailItem[] { item1, item2, item3 });
			copySender.MailAddressToSendCopyTo = "copy@edi.com";
			copySender.SendCopyTo();
			AssertEquals("Should affect three Items", 3, copySender.SentItemPKs.Length);

			AssertEquals("Should send copy without errors", "", copySender.Errors);

			MailItem sentItem1 = Factory.Load<MailItem>(copySender.SentItemPKs[0]);
			AssertEquals("Send", MailDirection.Transmit, sentItem1.MI_Direction);
			AssertEquals("Queued", MailStatus.Queued, sentItem1.MI_Status);
			AssertEquals("One Recipient", 1, sentItem1.MailRecipients.Count);
			AssertEquals("Recipient Address", "<copy@edi.com>", sentItem1.MailRecipients[0].EmailAddress);
			Assert("SendDateTime is in correct range", sentItem1.MI_SendDateTime >= testStarted && sentItem1.MI_SendDateTime <= ZDateTime.UtcNow);

			MailItem sentItem2 = Factory.Load<MailItem>(copySender.SentItemPKs[1]);
			AssertEquals("Send", MailDirection.Transmit, sentItem2.MI_Direction);
			AssertEquals("Queued", MailStatus.Queued, sentItem2.MI_Status);
			AssertEquals("One Recipient", 1, sentItem2.MailRecipients.Count);
			AssertEquals("Recipient Address", "<copy@edi.com>", sentItem2.MailRecipients[0].EmailAddress);
			Assert("SendDateTime is in correct range", sentItem2.MI_SendDateTime >= testStarted && sentItem2.MI_SendDateTime <= ZDateTime.UtcNow);

			MailItem sentItem3 = Factory.Load<MailItem>(copySender.SentItemPKs[2]);
			AssertEquals("Send", MailDirection.Transmit, sentItem3.MI_Direction);
			AssertEquals("Queued", MailStatus.Queued, sentItem3.MI_Status);
			AssertEquals("One Recipient", 1, sentItem3.MailRecipients.Count);
			AssertEquals("Recipient Address", "<copy@edi.com>", sentItem3.MailRecipients[0].EmailAddress);
			Assert("SendDateTime is in correct range", sentItem3.MI_SendDateTime >= testStarted && sentItem3.MI_SendDateTime <= ZDateTime.UtcNow);
			Assert("Should clear out existing to and cc headers", !sentItem3.MI_Header.Contains("bad@example"));
			Assert("Should clear out existing received headers, because they can cause mail servers to delay or block delivery. A mail server might assume a message has looped, when in fact the copy we're sending is a new message.", !sentItem3.MI_Header.Contains("received:"));
			Assert("Should leave other header", sentItem3.MI_Header.ToLower().Contains("x-junk: foobar"));

			AssertEquals("From", string.Format("{0} <{1}>", Env.Registry.MailboxDisplayName, Env.Registry.MailboxEmailAddress), sentItem3.MI_From);
			AssertEquals("Subject", "FW: " + item3.MI_Subject, sentItem3.MI_Subject);
			AssertEquals("Body", item3.MI_Body, sentItem3.MI_Body);
			AssertEquals("Content Type", item3.MI_ContentType, sentItem3.MI_ContentType);
		}

		public void TestSendCopyTo_ProcessesMultipleRecipients()
		{
			MailItem item1 = GetNewMailItem();
			item1.RunPreSaveValidation();
			Assert("Save without Error", !item1.HasNotifications());

			Factory.Save();

			MailItemCopySender copySender = new MailItemCopySender(new MailItem[] { item1 });
			copySender.MailAddressToSendCopyTo = "copy1@edi.com; copy2@edi.com; copy3@edi.com";
			copySender.SendCopyTo();
			AssertEquals("Should affect one Item", 1, copySender.SentItemPKs.Length);

			AssertEquals("Should send copy without errors", "", copySender.Errors);

			MailItem sentItem1 = Factory.Load<MailItem>(copySender.SentItemPKs[0]);
			AssertEquals("Send", MailDirection.Transmit, sentItem1.MI_Direction);
			AssertEquals("Queued", MailStatus.Queued, sentItem1.MI_Status);
			AssertEquals("Three Recipients", 3, sentItem1.MailRecipients.Count);
			AssertEquals("First Recipient Address", "<copy1@edi.com>", sentItem1.MailRecipients[0].EmailAddress);
			AssertEquals("Second Recipient Address", "<copy2@edi.com>", sentItem1.MailRecipients[1].EmailAddress);
			AssertEquals("Third Recipient Address", "<copy3@edi.com>", sentItem1.MailRecipients[2].EmailAddress);
		}

		public void TestSendMailCopyToWithAttachment()
		{
			MailItem item = GetNewMailItem();
			MailAttachment attachment = item.MailAttachments.AddNew();
			attachment.MA_ContentType = "XML";
			attachment.MA_FileName = "Document.xml";
			attachment.MA_Data = Encoding.UTF8.GetBytes("<document><element>foo</element></document>");
			Factory.Save();

			MailItemCopySender copySender = new MailItemCopySender(new MailItem[] { item });
			copySender.MailAddressToSendCopyTo = "copy@edi.com";
			copySender.SendCopyTo();

			AssertEquals("Should affect three Items", 1, copySender.SentItemPKs.Length);
			AssertEquals("Should send copy without errors", "", copySender.Errors);
			MailItem sentItem = Factory.Load<MailItem>(copySender.SentItemPKs[0]);
			AssertEquals("Send", MailDirection.Transmit, sentItem.MI_Direction);
			AssertEquals("Queued", MailStatus.Queued, sentItem.MI_Status);
			AssertEquals("One Recipient", 1, sentItem.MailRecipients.Count);
			AssertEquals("Recipient Address", "<copy@edi.com>", sentItem.MailRecipients[0].EmailAddress);
			AssertEquals("Attachments", 1, sentItem.MailAttachments.Count);
			AssertEquals(attachment.MA_ContentType, sentItem.MailAttachments[0].MA_ContentType);
			AssertEquals(attachment.MA_FileName, sentItem.MailAttachments[0].MA_FileName);
			AssertEquals(attachment.MA_Data, sentItem.MailAttachments[0].MA_Data);
		}

		public void TestSendCopyToIsSentWithOutboundFromRegistry()
		{
			var item = GetNewMailItem();
			AssertEquals(item.MI_From, "sender@domain.com");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var copySender = new MailItemCopySender(new MailItem[] { item })
				{
					MailAddressToSendCopyTo = "copy@edi.com"
				};
				copySender.Factory.Save();

				copySender.SendCopyTo();
				var sentItem = Factory.Load<MailItem>(copySender.SentItemPKs[0]);
				AssertEquals(string.Format("{0} <{1}>", Env.Registry.MailboxDisplayName, Env.Registry.MailboxEmailAddress), sentItem.MI_From);
			}
		}

		public void TestSendCopyToIsSentWithOutboundFromCurrentUser()
		{
			var item = GetNewMailItem();
			AssertEquals(item.MI_From, "sender@domain.com");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Env.Registry.MailboxEmailAddress = "";
				var copySender = new MailItemCopySender(new MailItem[] { item })
				{
					MailAddressToSendCopyTo = "copy@edi.com"
				};
				copySender.Factory.Save();

				copySender.SendCopyTo();
				var sentItem = Factory.Load<MailItem>(copySender.SentItemPKs[0]);
				AssertEquals("Copy should be send with old inbound mail address", string.Format("{0} <{1}>", Env.CurrentUser.FullName, Env.CurrentUser.EmailAddress), sentItem.MI_From);
			}
		}

		public void TestSendCopyToIsSentWithOutboundFromOldInbound()
		{
			var item = GetNewMailItem();
			AssertEquals(item.MI_From, "sender@domain.com");

			using (Env.SetTemporaryUserContext(null, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Env.Registry.MailboxEmailAddress = "";
				var copySender = new MailItemCopySender(new MailItem[] { item })
				{
					MailAddressToSendCopyTo = "copy@edi.com"
				};
				copySender.Factory.Save();

				copySender.SendCopyTo();
				var sentItem = Factory.Load<MailItem>(copySender.SentItemPKs[0]);
				AssertEquals("Copy should be send with old inbound mail address", item.MI_From, sentItem.MI_From);
			}
		}

		public void TestSendMailCopyToWithMultibyteCharacters()
		{
			var item = GetNewMailItem();
			item.MI_Body = "Multibyte characters: “ῦ–ῤ”";

			var copySender = new MailItemCopySender(new MailItem[] { item });
			copySender.MailAddressToSendCopyTo = "copy@edi.com";
			copySender.SendCopyTo();
			var sentItem = Factory.Load<MailItem>(copySender.SentItemPKs[0]);

			AssertEquals("Multibyte characters: “ῦ–ῤ”", sentItem.MI_Body);
		}

		MailItem GetNewMailItem()
		{
			MailItem item = Factory.New<MailItem>();
			item.MI_Direction = MailDirection.Transmit;
			item.MI_Status = MailStatus.Sent;
			item.MI_SendDateTime = ZDateTime.UtcNow;
			item.MI_ReceivedDateTime = ZDateTime.UtcNow;
			item.MI_From = "sender@domain.com";
			item.AddRecipientForUserCommunication("original@recipient", MailRecipient.RecipientTypes.TO);
			return item;
		}
	}
}
