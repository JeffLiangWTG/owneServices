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

public class NE131ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INE131ResponseDetail>
{
	public NE131ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSG };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportDeclarationActivationResponse };

	protected override string MessageFriendlyNameCore => (NoResString)"NE131 - Export Declaration Activation Response";

	protected override INE131ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INE131ResponseDetail;

	protected override string GetCustomsEntryNumber(INE131ResponseDetail customsResponse) => customsResponse.GDRN.AppendEntryNumVersion(customsResponse.GDRNVersion);

	protected override BusinessObject FindLinkedObject(EDIMessage message, INE131ResponseDetail xmlObject) => FindLinkedObjectByEntryNum(message, xmlObject);

	protected override void ProcessResponseMessage(CHEDIMessage message, INE131ResponseDetail customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			if (customsResponse.IsAccepted)
			{
				entryHeader.CH_EntryStatus = customsResponse.GDRNState;
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Accepted;

				entryHeader.Logs.AddNew(Events.CustomsEntryStatus, customsResponse.GDRNState);
			}
			else if (customsResponse.IsRejected)
			{
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Invalid;

				entryHeader.Logs.AddNew(Events.DeclarationActivationRejected);
			}
			else
			{
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Invalid;
			}
		}
	}
}
