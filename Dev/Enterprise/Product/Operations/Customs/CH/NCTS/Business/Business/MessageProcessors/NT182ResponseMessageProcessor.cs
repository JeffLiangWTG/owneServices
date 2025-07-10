using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT182ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT182ResponseDetail>
{
	public NT182ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarEventDuringTheJourney };

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override string MessageFriendlyNameCore => (NoResString)"NT182 - Transit forwarded incident notification to ed Response Message Processor";

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT182ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;
	}
}
