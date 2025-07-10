using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;

namespace Enterprise.Customs.IT.Business;

public class ITEDIMessageCollection : Enterprise.Messaging.Business.EDIMessageCollection
{
	public ITEDIMessageCollection(BusinessObject master)
		: base(master)
	{
	}

	public new ITEDIMessage this[int index] => (ITEDIMessage)base[index];

	public new ITEDIMessage AddNew() => (ITEDIMessage)base.AddNew();

	public ITEDIMessage GetLastSuccessfullySentIdoc() => SentOrderedMessageList.LastOrDefault(x => x.EM_MessageType == SADConstants.CustomsInterchangeType.IdocR);

	public ITEDIMessage GetLastSuccessfullySentMessageBySubType(ZString subType) => SentOrderedMessageList.LastOrDefault(x => x.EM_MessageSubType == subType);

	public ITEDIMessage GetLastSuccessfullySentMessageForDepositedStatus()
		=> SentOrderedMessageList.LastOrDefault(x => x.EM_MessageType == EDIMessageTypeList.Codes.NewDeclaration
		&& (x.EM_MessageSubType.StartsWith(MessageProcessorConstants.MessageSubTypes.HCategory) || x.EM_MessageSubType.StartsWith(MessageProcessorConstants.MessageSubTypes.BCategory)));

	public ITEDIMessage GetLastMessageByType(ZString messageType) => OrderedMessageList.LastOrDefault(message => message.EM_MessageType == messageType);

	public ITEDIMessage GetIdocMessage(ZString messageNum) => OrderedMessageList.LastOrDefault(x => x.EM_MessageNum == messageNum && x.EM_MessageType == SADConstants.CustomsInterchangeType.IdocR);

	public ITEDIMessage GetFirstAcknowledgmentByUniqueTransactionID(ZString uniqueTransactionID)
	{
		if (uniqueTransactionID.IsEmpty)
		{
			return null;
		}

		return OrderedMessageList
			.FirstOrDefault(message => IsAcknowledgmentAndMatchesUniqueTransactionID(message));

		bool IsAcknowledgmentAndMatchesUniqueTransactionID(ITEDIMessage message)
		{
			return message.EM_MessageType == EDIMessageTypeList.Codes.Acknowledgment
				&& UniqueTransactionIdentifierTextExtractor.ExtractUniqueTransactionIdentifier(message.EM_MessageText) == uniqueTransactionID;
		}
	}

	public ITEDIMessage GetFirstSentMessageBySessionGuid(ZGuid interchangeSessionGuid)
	{
		if (interchangeSessionGuid.IsEmpty)
		{
			return null;
		}

		return SentOrderedMessageList
			.FirstOrDefault(x => x.Interchange != null && x.Interchange.EI_SessionGUID == interchangeSessionGuid);
	}

	#region Implementation

	IEnumerable<ITEDIMessage> OrderedMessageList => Elements.Cast<ITEDIMessage>().OrderBy(x => x.EM_SystemCreateTimeUtc);

	IEnumerable<ITEDIMessage> SentOrderedMessageList => OrderedMessageList.Where(x => x.IsTransmitMessage && (x.EM_Status == EDIMessageStatusList.Codes.Sent || x.EM_Status == EDIMessageStatusList.Codes.Manual));

	#endregion
}
