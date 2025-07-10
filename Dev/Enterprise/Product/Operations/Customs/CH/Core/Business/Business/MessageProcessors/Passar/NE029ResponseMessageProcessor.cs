using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public sealed class NE029ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INE029ResponseDetail>
{
	public NE029ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSG };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportReleaseResponse };

	protected override string MessageFriendlyNameCore => (NoResString)"NE029 - Passar Export Release Response";

	protected override INE029ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INE029ResponseDetail;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INE029ResponseDetail xmlObject) => FindLinkedObjectByGDRN(message, xmlObject);

	protected override void ProcessResponseMessage(CHEDIMessage message, INE029ResponseDetail customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			if (entryHeader.CH_EntryStatus != AdditionalCHEntryStatusList.Codes.CustomsAssessmentDecision)
			{
				entryHeader.CH_EntryStatus = AdditionalCHEntryStatusList.Codes.ReleasedForExport;
			}
			entryHeader.CH_EntryReleaseDate = new ZDateTime(customsResponse.PreparationDateAndTime, System.DateTimeKind.Utc).ToLocalBranchTime();
		}
	}
}
