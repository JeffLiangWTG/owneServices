using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT061ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT061ResponseDetail>
{
	public NT061ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT061 - Control Decision Notification Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarControlDecisionNotification };

	protected override string MovementType => NctsMovementType.Codes.Arrival;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INT061ResponseDetail xmlObject)
	{
		var applicationReference = xmlObject.OppositeInformationReferenceNumber;
		var nctsHeader =  string.IsNullOrEmpty(applicationReference) ? null : message.Factory.GetOutgoingMessageFromApplicationReference(ApplicationCode, applicationReference)?.EM_LinkedObject as NctsHeader;
		return nctsHeader ?? new NctsHeader.Loader(message.Factory).FindByArrivalReferenceNumber(xmlObject.ArrivalOperationReferenceNumber ?? ZString.Empty);
	}

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT061ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		if (customsResponse.IsSelectionNotificationInspectionDecisionClear)
		{
			if (customsResponse.IsSelectionNotificationStatusFinal && !nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.Any())
			{
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			}
			else
			{
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.CHClear;
			}
		}
		else
		{
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
		}
	}
}
