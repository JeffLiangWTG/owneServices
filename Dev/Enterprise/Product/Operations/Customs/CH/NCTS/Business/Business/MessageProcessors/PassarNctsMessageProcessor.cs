using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.NCTS.Business;

public class PassarNctsMessageProcessor : CH.Business.BaseMessageProcessor
{
	public PassarNctsMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.PassarNcts };

	protected override string MessageFriendlyNameCore => Res.GetString("0A6C1F29-5409-4B47-BD9F-071D41E2D36B", "Passar NCTS Message Processor");

	protected override void ProcessMessageCore(CHEDIMessage message)
	{
		message.EM_LinkedObject = FindLinkedObjectByOutgoingSessionID(message);

		if (NctsHeader.GetLinkedNctsHeader(message.EM_LinkedObject) is NctsHeader nctsHeader)
		{
			switch (message.EM_MessageSubType)
			{
				case MessageSubTypeCodeList.Codes.Acknowledged:
					nctsHeader.EffectiveMessageStatus = EDIMessageStatusList.Codes.Acknowledged;
					break;
				case MessageSubTypeCodeList.Codes.Rejected:
					nctsHeader.EffectiveMessageStatus = EDIMessageStatusList.Codes.Failed;
					break;
				default:
					message.EM_Status = EDIMessage.Status.Discarded;
					Logger.LogWarning($"Unexpected message sub type \"{message.EM_MessageSubType}\"");
					break;
			}
		}
	}
}
