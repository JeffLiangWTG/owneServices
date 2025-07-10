using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class CUSCARMessageSender
{
	public int SendBillMessage(ASYCUDA.Business.AsycudaManifestHeader header, ASYCUDA.Business.MessageChooser messageChooser)
	{
		var selectedItems = messageChooser.GetSelectedMessageChooserItems().Cast<MessageChooserItem>();
		var successCreateMessageCount = 0;

		foreach (var item in selectedItems)
		{
			var provider = new CUSCARMessageProvider(item);
			var messageBulider = new CUSCARMessageBuilder(provider).PopulateMessages();
			if (messageBulider.IsSuccess)
			{
				var message = messageBulider.GetBuilderResults().Single().Message;
				SendCUSCARMessage(message, item, header);
				successCreateMessageCount++;
				var bill = item.Bill;
				((IStatusSupporter)bill).LogEventsOnParent(Events.CustomsManifestStatus, EDIMessage.Status.Sent);
			}
		}
		return successCreateMessageCount;
	}

	void SendCUSCARMessage(EDIMessage message, MessageChooserItem item, ASYCUDA.Business.AsycudaManifestHeader header)
	{
		message.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;
		message.EM_MessageSubType = GetMessageSubType(item.EntryType);
		message.EM_SendWithMessageErrors = item.HasMessageErrors || header.HasMessageErrors;
	}

	ZString GetMessageSubType(ZString entryType)
	{
		var result = ZString.Empty;
		switch (entryType)
		{
			case EntryTypes.Codes.Cancellation:
				result = Common.Shared.MessageSubTypeCodes.Codes.Cancellation;
				break;
			case EntryTypes.Codes.Change:
				result = Common.Shared.MessageSubTypeCodes.Codes.Change;
				break;
			case EntryTypes.Codes.Original:
				result = Common.Shared.MessageSubTypeCodes.Codes.Original;
				break;
			default:
				break;
		}
		return result;
	}
}
