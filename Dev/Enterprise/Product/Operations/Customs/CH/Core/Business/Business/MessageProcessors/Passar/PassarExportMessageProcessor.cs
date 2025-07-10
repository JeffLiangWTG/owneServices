using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public class PassarExportMessageProcessor : BaseMessageProcessor
{
	public PassarExportMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.Export };

	protected override string MessageFriendlyNameCore => Res.GetString("A5EE2CAB-7FB6-4182-95B3-A4A54B684946", "Passar Export Message Processor");

	protected override void ProcessMessageCore(CHEDIMessage message)
	{
		message.EM_LinkedObject = FindLinkedObjectByOutgoingSessionID(message);

		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			switch (message.EM_MessageSubType)
			{
				case MessageSubTypeCodeList.Codes.Acknowledged:
					entryHeader.CH_Status = EDIMessageStatusList.Codes.Acknowledged;
					break;
				case MessageSubTypeCodeList.Codes.Rejected:
					entryHeader.CH_Status = EDIMessageStatusList.Codes.Failed;
					break;
				default:
					message.EM_Status = EDIMessage.Status.Discarded;
					Logger.LogWarning($"Unexpected message sub type \"{message.EM_MessageSubType}\"");
					break;
			}
		}
	}
}
