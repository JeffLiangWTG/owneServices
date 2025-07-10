using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT029ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT029ResponseDetail>
{
	public NT029ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT029 - Release for Transit Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarTransitReleased };

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT029ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		var entryNum = nctsHeader.MovementReferenceEntryNumber;
		entryNum.CE_IssueDate = customsResponse.PreparationDateAndTime.UtcToLocalBranchTime();
	}
}
