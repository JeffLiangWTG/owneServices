using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailItemExtensionsTest : TestCaseWithFactory
	{
		public void TestAttachmentIsReferredToInHTMLBody()
		{
			MailItem mi = Factory.New<MailItem>();
			MailAttachment attachment = Factory.New<MailAttachment>();
			mi.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			mi.MI_Body = "<img src=\"In.jpg\" alt=\"Footer Image\" />";
			attachment.MA_FileName = "NotIn.jpg";
			string dummy = null;
			AssertEquals(false, mi.AttachmentIsReferredToInHTMLBody(attachment, out dummy));
			attachment.MA_FileName = "In.jpg";
			AssertEquals(true, mi.AttachmentIsReferredToInHTMLBody(attachment, out dummy));
			mi.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			AssertEquals(false, mi.AttachmentIsReferredToInHTMLBody(attachment, out dummy));
		}

		public void TestAttachmentIsReferredToInHTMLBody_Unicode()
		{
			var mi = Factory.New<MailItem>();
			var attachment = Factory.New<MailAttachment>();
			mi.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			mi.MI_Body = "<img src=\"%E5%88%B0%E7%9D%80%E6%A1%88%E5%86%85%20_%20Arrival%20Notice%20-%20S00001017_1_image1.png\" alt=\"Footer Image\" />";
			attachment.MA_FileName = "NotIn.jpg";
			string dummy = null;
			AssertEquals(false, mi.AttachmentIsReferredToInHTMLBody(attachment, out dummy));
			attachment.MA_FileName = "到着案内 _ Arrival Notice - S00001017_1_image1.png";
			AssertEquals(true, mi.AttachmentIsReferredToInHTMLBody(attachment, out dummy));
			mi.MI_ContentType = EmailContentTypes.PlainText.ContentTypeCode;
			AssertEquals(false, mi.AttachmentIsReferredToInHTMLBody(attachment, out dummy));
		}

		public void TestAttachmentIsReferredToInHTMLBodyWithFullyQualifiedFileName()
		{
			MailItem mi = Factory.New<MailItem>();
			MailAttachment attachment = Factory.New<MailAttachment>();
			mi.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			mi.MI_Body = "<img src=\"In.jpg\" alt=\"Footer Image\" />";
			attachment.MA_FileName = @"\Rude\NotIn.jpg";
			string dummy = null;
			AssertEquals(false, mi.AttachmentIsReferredToInHTMLBody(attachment, out dummy));
		}

		public void TestUpdateMailItemStatus()
		{
			MailItem mailItemQue = Factory.New<MailItem>();
			mailItemQue.UpdateMailItemStatus();
			AssertEquals("QUE Email without recipients failed", MailStatus.Failed, mailItemQue.MI_Status);

			mailItemQue.AddRecipientForUserCommunication("To", MailRecipient.RecipientTypes.TO);
			mailItemQue.UpdateMailItemStatus();
			AssertEquals("QUE Email sent", MailStatus.Sent, mailItemQue.MI_Status);

			MailItem mailItemQwa1 = Factory.New<MailItem>();
			mailItemQwa1.MI_Status = MailStatus.QueuedWithAck;
			mailItemQwa1.AddRecipientForUserCommunication("To", MailRecipient.RecipientTypes.TO);
			mailItemQwa1.AddRecipientForUserCommunication("Cc", MailRecipient.RecipientTypes.CC);
			mailItemQwa1.MailRecipients[0].MR_DeliveredTime = ZDateTime.UtcNow;
			mailItemQwa1.MailRecipients[1].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			mailItemQwa1.UpdateMailItemStatus();
			AssertEquals("QWA Email keep the same status", MailStatus.QueuedWithAck, mailItemQwa1.MI_Status);

			mailItemQwa1.MailRecipients[1].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			mailItemQwa1.MailRecipients[1].MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			mailItemQwa1.UpdateMailItemStatus();
			AssertEquals("QWA Email failed", MailStatus.Failed, mailItemQwa1.MI_Status);

			mailItemQwa1.MI_Status = MailStatus.QueuedWithAck;
			mailItemQwa1.MailRecipients[1].MR_DeliveredTime = ZDateTime.UtcNow;
			mailItemQwa1.UpdateMailItemStatus();
			AssertEquals("QWA Email sent", MailStatus.Sent, mailItemQwa1.MI_Status);

			MailItem mailItemQwa2 = Factory.New<MailItem>();
			mailItemQwa2.MI_Status = MailStatus.QueuedWithAck;
			mailItemQwa2.AddRecipientForUserCommunication("To", MailRecipient.RecipientTypes.TO);
			mailItemQwa2.AddRecipientForUserCommunication("Cc", MailRecipient.RecipientTypes.CC);
			mailItemQwa2.MailRecipients[0].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			mailItemQwa2.MailRecipients[0].MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			mailItemQwa2.MailRecipients[1].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			mailItemQwa2.MailRecipients[1].MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			mailItemQwa2.UpdateMailItemStatus();
			AssertEquals("QWA Email totally failed", MailStatus.Failed, mailItemQwa2.MI_Status);
		}

		public void TestUpdateRecipientsStatus()
		{
			MailItem mailItem = Factory.New<MailItem>();
			mailItem.MI_Status = MailStatus.QueuedWithAck;
			mailItem.AddRecipientForUserCommunication("To@host", MailRecipient.RecipientTypes.TO);
			mailItem.AddRecipientForUserCommunication("Cc@host", MailRecipient.RecipientTypes.CC);
			mailItem.AddRecipientForUserCommunication("Bcc@host", MailRecipient.RecipientTypes.BCC);
			mailItem.MailRecipients[0].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			mailItem.MailRecipients[1].MR_AckAttempt = MailAcknowledgement.MaxAttempts;
			mailItem.MailRecipients[1].MR_LastAttempt = ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout - 1);
			mailItem.MailRecipients[2].MR_DeliveredTime = ZDateTime.UtcNow;
			mailItem.UpdateMailItemStatus();
			AssertEquals("First recipient sent", 1, (int)mailItem.MailRecipients[0].MR_AckAttempt);
			AssertEquals("Second recipient not sent", MailAcknowledgement.MaxAttempts, (int)mailItem.MailRecipients[1].MR_AckAttempt);
			AssertEquals("Third recipient not sent", 0, (int)mailItem.MailRecipients[2].MR_AckAttempt);
		}
	}
}
