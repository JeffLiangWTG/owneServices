using CargoWise.Common;
using CargoWise.Customs.AE.MessageContracts;
using Enterprise.Customs.AE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class DocMessageBuilder
{
	public DocMessageBuilder(MPCIAttachmentMessageBuilder messageBuilder, SupportingDocSendingObject sendingObject)
	{
		MessageBuilder = Argument.NotNull(messageBuilder, nameof(messageBuilder));
		SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}
	MPCIAttachmentMessageBuilder MessageBuilder { get; }
	SupportingDocSendingObject SendingObject { get; }

	public AEEDIMessage PopulateMessage()
	{
		if (SendingObject.CUSCARMessage?.EM_LinkedObject is AsycudaBill bill)
		{
			var message = (AEEDIMessage)bill.Messages.AddNew(typeof(AEEDIMessage));
			message.EM_MessageType = AEConstants.Messaging.MessageTypes.DOCSUC;
			message.EM_MessageSubType = AEConstants.Messaging.MessageTypes.DOCXXX;
			message.EM_MessageText = MessageBuilder.GetMessageText();
			message.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			return message;
		}
		return null;
	}
}
