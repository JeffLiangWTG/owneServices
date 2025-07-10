using System.Linq;
using CargoWise.Customs.IN.MessageContracts;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.Messaging.Integration;
using MimeKit;

namespace Enterprise.Customs.IN.Business;

public class EmailInfo
{
	public EmailInfo()
	{ }

	public EmailInfo(MimeMessage emailContent)
	{
		Subject = emailContent.Subject ?? string.Empty;
		TextBody = emailContent.TextBody ?? string.Empty;
		HasAttachments = emailContent.Attachments.Any();

		if (HasAttachments)
		{
			var attachment = emailContent.Attachments.First();
			AttachmentName = attachment.GetSafeFileName();
			AttachmentText = MessageEncoding.UTF8WithoutBOM.GetString(attachment.GetData());
		}
	}

	public string Subject { get; set; }
	public string TextBody { get; set; }

	public bool HasAttachments { get; set; }
	public string AttachmentName { get; set; }
	public string AttachmentText { get; set; }

	public string MessageIdOnAttachment => messageIdOnAttachment ??= HasAttachments ? FlatFileMessage.ExtractMessageId(AttachmentText) : string.Empty;
	string messageIdOnAttachment;
}
