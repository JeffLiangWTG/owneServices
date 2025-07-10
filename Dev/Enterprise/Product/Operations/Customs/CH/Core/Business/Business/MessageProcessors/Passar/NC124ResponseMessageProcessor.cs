using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public abstract class NC124ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INC124ResponseDetail>
{
	public NC124ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NC124 - Activation Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarActivationResponse };

	protected sealed override void ProcessResponseMessage(CHEDIMessage message, INC124ResponseDetail customsResponse) => ProcessResponseMessageCore(message, customsResponse);

	protected virtual bool ProcessResponseMessageCore(CHEDIMessage message, INC124ResponseDetail customsResponse)
	{
		var result = false;
		if (message.EM_LinkedObject is CusEntryHeader cusEntryHeader)
		{
			if (customsResponse.IsAccepted)
			{
				cusEntryHeader.CH_Status = CHLogicalStatusList.Codes.Accepted;
				cusEntryHeader.CH_EntryStatus = AdditionalCHEntryStatusList.Codes.Active;
			}
			else if (customsResponse.IsRejected)
			{
				cusEntryHeader.CH_Status = CHLogicalStatusList.Codes.Invalid;
				cusEntryHeader.Logs.AddNew(Events.DeclarationActivationRejected);
			}
			result = true;
		}
		return result;
	}
}
