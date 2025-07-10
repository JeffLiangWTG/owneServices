using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D23A.Messages.CONTRL;

namespace Enterprise.Customs.AE.Business;

public class CONTRLMessageBuilder : EDIFACTMessageBuilder<ICONTRLMessageProvider, CONTRLMessage, AEEDIMessage>
{
	public CONTRLMessageBuilder(ICONTRLMessageProvider data) : base(data, MessageSubTypes.Undefined, AECharacterSet.New())
	{
	}

	protected override void PopulateEdifactMessage()
	{
		CommonMessageBuilder.PopulateUNHSegment(edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection(), new CONTRLMessageHeaderProvider());
		PopulateUCISegment();
		PopulateUNTSegment();
	}

	protected override AEEDIMessage PopulateMessagesReturningResult()
	{
		PopulateEdifactMessage();
		var messageToSend = data.AddNewEDIMessage();
		messageToSend.EM_MessageType = AEConstants.Messaging.MessageTypes.CONTRL;
		messageToSend.EM_MessageText = Utils.GetFormattedEDIFactText(edifactMessage.ToString(AECharacterSet.New()));
		messageToSend.EM_EM_RequestMessage = data.RequestMessage.PK;
		return messageToSend;
	}

	void PopulateUCISegment()
	{
		var uci = edifactMessage.UCI.InstantiateAChildAndAddItToChildrenCollection();
		uci.InterchangeControlReference = data.InterchangeControlReference;
		uci.ActionCoded = data.ActionCoded;
	}

	void PopulateUNTSegment()
	{
		var uNTSegment = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
		uNTSegment.NumberOfSegmentsInAMessage = edifactMessage.CountIncludingUNT.ToString();
		uNTSegment.MessageReferenceNumber = edifactMessage.UNH[0].MessageReferenceNumber;
	}
}


