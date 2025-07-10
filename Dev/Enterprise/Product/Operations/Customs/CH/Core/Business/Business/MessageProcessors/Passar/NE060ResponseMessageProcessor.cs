using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class NE060ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INE060ResponseDetail>
{
	public NE060ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => [MessageTypeCodeList.Codes.MSG];

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportControlDecisionNotification };

	protected override string MessageFriendlyNameCore => (NoResString)"NE060 - Control Decision Notification";

	protected override INE060ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INE060ResponseDetail;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INE060ResponseDetail xmlObject) => FindLinkedObjectByGDRN(message, xmlObject);

	protected override void ProcessResponseMessage(CHEDIMessage message, INE060ResponseDetail customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = AdditionalCHEntryStatusList.Codes.DecisionToControl;
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, AdditionalCHEntryStatusList.Codes.DecisionToControl);
		}
	}
}
