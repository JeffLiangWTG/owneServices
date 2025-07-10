using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT055ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT055ResponseDetail>
{
	public NT055ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT055 - Invalid Guarantee Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarInvalidGuarantee };

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT055ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		if (nctsHeader.MovementHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.DecisionToControl)
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.CHControlForInvalidGuarantee;
		}
		else
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
		}
	}
}
