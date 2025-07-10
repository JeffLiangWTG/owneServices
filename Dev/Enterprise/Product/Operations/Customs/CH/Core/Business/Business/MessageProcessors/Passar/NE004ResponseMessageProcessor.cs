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

public class NE004ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INxx04ResponseDetail>
{
	public NE004ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSG };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarExportDeclarationAmendmentResponse };

	protected override string MessageFriendlyNameCore => (NoResString)"NE004 - Passar Export Declaration Amendment Response";

	protected override INxx04ResponseDetail DeserializeResponse(CHEDIMessage message) => message.MessageAnalyzer.MessageDetail as INxx04ResponseDetail;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INxx04ResponseDetail xmlObject) => xmlObject.InitiatedByCustoms ? FindLinkedObjectByGDRN(message, xmlObject) : base.FindLinkedObject(message, xmlObject);

	protected override void ProcessResponseMessage(CHEDIMessage message, INxx04ResponseDetail customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			entryHeader.CH_Status = customsResponse.IsAccepted ? CHLogicalStatusList.Codes.Accepted : CHLogicalStatusList.Codes.Invalid;

			if (customsResponse.IsAccepted)
			{
				entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Declaration;
				entryHeader.MovementReferenceNumberSetter(customsResponse.GDRN.AppendEntryNumVersion(customsResponse.GDRNVersion), issueDate: customsResponse.DecisionDateAndTime.UtcToLocalBranchTime(), expiryDate: customsResponse.ActivationDeadline);
			}
			else if (customsResponse.IsRejected)
			{
				entryHeader.Logs.AddNew(Events.DeclarationAmendmentRejected);
			}
		}
	}
}
