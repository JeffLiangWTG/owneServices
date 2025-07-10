using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class NE021ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INE021ResponseDetail>
{
	public NE021ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportPayloadRequestGoodsDeclarationResponse };

	protected override string MessageFriendlyNameCore => (NoResString)"NE021 - Payload request goods declaration response";

	protected override INE021ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INE021ResponseDetail;

	protected override void ProcessResponseMessage(CHEDIMessage message, INE021ResponseDetail customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Accepted;
			entryHeader.Logs.AddNew(Events.MessageStatusChange, CHLogicalStatusList.Codes.Accepted);
		}
	}
}
