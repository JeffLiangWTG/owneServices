using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public abstract class NC909ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INC909ResponseDetail>
{
	public NC909ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NC909 - Technical Error Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarTechnicalError };

	protected sealed override void ProcessResponseMessage(CHEDIMessage message, INC909ResponseDetail customsResponse) => ProcessResponseMessageCore(message, customsResponse);

	protected virtual bool ProcessResponseMessageCore(CHEDIMessage message, INC909ResponseDetail customsResponse)
	{
		var result = false;
		if (message.EM_LinkedObject is CusEntryHeader cusEntryHeader)
		{
			UpdateExportStatus(cusEntryHeader);
			result = true;
		}
		return result;
	}

	void UpdateExportStatus(CusEntryHeader header)
	{
		switch (header.CH_PhaseStatus)
		{
			case PassarDeclarationPhaseList.Codes.Declaration:
				header.Logs.AddNew(Events.DeclarationRejected);
				break;
			case PassarDeclarationPhaseList.Codes.Amendment:
				header.Logs.AddNew(Events.DeclarationAmendmentRejected);
				break;
			case PassarDeclarationPhaseList.Codes.Cancellation:
				header.Logs.AddNew(Events.DeclarationCancellationRejected);
				break;
			case PassarDeclarationPhaseList.Codes.Activation:
				header.Logs.AddNew(Events.DeclarationActivationRejected);
				break;
		}
		header.CH_Status = CHLogicalStatusList.Codes.Invalid;
	}
}
