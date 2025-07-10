using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;

[assembly: UniversalCustomsEDIMessagePacker(ApplicationCodeList.Codes.INCustoms, typeof(Enterprise.Customs.IN.Manifest.Business.INMessagePacker))]

namespace Enterprise.Customs.IN.Manifest.Business;

public class INMessagePacker : IUniversalCustomsEDIMessagePacker
{
	public bool AllowEmptyMessageBody => false;

	public ZString Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		var errorBuilder = new ZStringBuilder();

		var senderMailId = INMessageHelper.GetMessageSenderEmailId(message);
		var recipientMailId = INMessageHelper.GetMessageRecipientEmailId(message);

		if (string.IsNullOrEmpty(recipientMailId) || (string.IsNullOrEmpty(senderMailId) && !(message.UserWhoQueuedThisRecord?.IsSupportUser ?? false)))
		{
			var missingParty = string.IsNullOrEmpty(senderMailId) ? "Sender" : "Recipient";
			errorBuilder.Append($"Customs Interchange {missingParty} email id is not set up.");
		}
		else
		{
			var messageText = message.EM_MessageText;
			var sender = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			var recipient = INMessageHelper.GetMessageRecipient(message);
			var messageType = message.EM_MessageType;
			var messageNum = message.EM_MessageNum;
			var mailSubject = messageType + " " + messageNum;
			var fileName = messageNum + "." + messageType;
			var copyToEmailId = INMessageHelper.GetMessageCopyToEmailId(message);

			EDIMessagePackerUtils.PopulateInterchange(interchange, message.EM_ApplicationCode, messageType,
				sender, recipient, message.EM_GB, message.EM_GP, ZGuid.NewZGuid());
			interchange.EI_BodyText = messageText;
			interchange.SetHeaderTextWithAttributeDictionary(new Dictionary<string, string>
				{
					{ Constants.Messaging.InterchangeHeaderAttributes.SenderMailBoxKey, senderMailId },
					{ Constants.Messaging.InterchangeHeaderAttributes.DestinationMailBoxKey, recipientMailId },
					{ Constants.Messaging.InterchangeHeaderAttributes.SubjectKey, mailSubject },
					{ Constants.Messaging.InterchangeHeaderAttributes.FileNameKey, fileName },
					{ Constants.Messaging.InterchangeHeaderAttributes.CopyToMailBoxKey, copyToEmailId },
				});
		}

		return errorBuilder.ToString();
	}
}
