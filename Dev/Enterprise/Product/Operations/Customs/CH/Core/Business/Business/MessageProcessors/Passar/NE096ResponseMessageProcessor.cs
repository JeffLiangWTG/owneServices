using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class NE096ResponseMessageProcessor : PassarDecisionGetMessageAcknowledgeMessageProcessor<INE096ResponseDetail>
{
	public NE096ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportDecisionRectification };

	protected override string MessageFriendlyNameCore => (NoResString)"NE096 - Passar Export Declaration Rectification";

	protected override Event MessageRejectedEvent => Events.DeclarationAmendmentRejected;

	protected override INE096ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INE096ResponseDetail;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INE096ResponseDetail xmlObject) => xmlObject.InitiatedByCustoms ? FindLinkedObjectByGDRN(message, xmlObject) : FindLinkedObjectByCorrelationIdentifier(message, xmlObject);
}
