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

public class NE083ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INE083ResponseDetail>
{
	public NE083ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSG };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportDeclarationIssuanceOfAssessmentDecision };

	protected override string MessageFriendlyNameCore => (NoResString)"NE083 - Export declaration issuance of assessment decisions";

	protected override INE083ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INE083ResponseDetail;

	protected override string GetCustomsEntryNumber(INE083ResponseDetail customsResponse) => customsResponse.GDRN;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INE083ResponseDetail xmlObject) => FindLinkedObjectByEntryNum(message, xmlObject, true);

	protected override void ProcessResponseMessage(CHEDIMessage message, INE083ResponseDetail customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = CHLogicalStatusList.Codes.EVV;
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, CHLogicalStatusList.Codes.EVV);
		}
	}
}
