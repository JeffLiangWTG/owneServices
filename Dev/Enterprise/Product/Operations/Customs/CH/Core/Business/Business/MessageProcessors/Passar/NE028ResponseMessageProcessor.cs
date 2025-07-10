using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class NE028ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INE028ResponseDetail>
{
	public NE028ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSG };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportDeclarationResponse };

	protected override string MessageFriendlyNameCore => (NoResString)"NE028 - Passar Export Declaration Response";

	protected override INE028ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INE028ResponseDetail;

	protected override void ProcessResponseMessage(CHEDIMessage message, INE028ResponseDetail customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			if (customsResponse.IsAccepted)
			{
				var entryNum = customsResponse.GDRN.AppendEntryNumVersion(customsResponse.GDRNVersion);
				var oldValue = entryHeader.MovementReferenceNumber;
				entryHeader.MovementReferenceNumberSetter(entryNum, issueDate: customsResponse.DecisionDateAndTime.UtcToLocalBranchTime(), expiryDate: customsResponse.ActivationDeadline);
				LogInformationIfValueUpdated(Res.GetString("7b83693a-3f12-4242-af96-1dd25386fb7e", "Movement Reference Number"), oldValue, entryNum, message.EM_MessageNum);

				entryHeader.CH_EntryStatus = Common.Shared.MessageStatusList.Codes.ClearReplace;
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Accepted;
			}
			else
			{
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Invalid;
				entryHeader.Logs.AddNew(Events.DeclarationRejected);
			}
		}
	}
}
