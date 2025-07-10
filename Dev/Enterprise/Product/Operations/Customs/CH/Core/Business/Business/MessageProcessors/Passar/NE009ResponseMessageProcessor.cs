using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class NE009ResponseMessageProcessor : PassarDecisionGetMessageAcknowledgeMessageProcessor<INE009ResponseDetail>
{
	public NE009ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportWithdrawalResponseRejected };

	protected override string MessageFriendlyNameCore => (NoResString)"NE009 - Passar Export Declaration Withdrawal Response";

	protected override Event MessageRejectedEvent => Events.DeclarationCancellationRejected;

	protected override INE009ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INE009ResponseDetail;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INE009ResponseDetail xmlObject) => xmlObject.InitiatedByCustoms ? FindLinkedObjectByGDRN(message, xmlObject) : FindLinkedObjectByCorrelationIdentifier(message, xmlObject);

	protected override void ProcessResponseMessage(CHEDIMessage message, INE009ResponseDetail customsResponse)
	{
		base.ProcessResponseMessage(message, customsResponse);

		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			if (customsResponse.IsAccepted)
			{
				entryHeader.CH_EntryStatus = Common.Shared.EntryStatusList.Codes.Cancelled;
			}
		}
	}
}
