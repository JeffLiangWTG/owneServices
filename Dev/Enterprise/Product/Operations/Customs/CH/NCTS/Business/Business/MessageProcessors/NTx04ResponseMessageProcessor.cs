using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

sealed class NTx04ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INxx04ResponseDetail>
{
	public NTx04ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT004/NT504 - Departure Amendment Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarDepartureAmendmentResponse, MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationAmendmentResponse };

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool ShouldFindLinkedObjectByMRN(INxx04ResponseDetail customsResponse) => customsResponse.InitiatedByCustoms;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INxx04ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		var movementHeader = nctsHeader.MovementHeader;

		if (customsResponse.IsAccepted)
		{
			movementHeader.BM_Phase = NCTS5DeparturePhaseList.Codes.Declaration;
			movementHeader.BM_EntryDate = customsResponse.DecisionDateAndTime.UtcToLocalBranchTime();
			nctsHeader.MovementReferenceNumberSetter(customsResponse.MRN.AppendEntryNumVersion(customsResponse.MRNVersion), expiryDate: customsResponse.ActivationDeadline);
			nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
		}
		else if (customsResponse.IsReceived)
		{
			nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
			nctsHeader.Logs.AddNew(Events.DeclarationAmendmentQueued);
		}
		else if (customsResponse.IsRejected)
		{
			nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
			nctsHeader.Logs.AddNew(Events.DeclarationAmendmentRejected);
		}
	}
}
