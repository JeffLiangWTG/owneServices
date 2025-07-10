using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT019ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT019ResponseDetail>
{
	public NT019ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT019 - Discrepancies at Destination Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarTransitDiscrepancies };

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT019ResponseDetail customsResponse, NctsHeader header)
	{
		header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination;
	}
}
