using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderCustomsStatusProvider : ISadCustomsStatusProvider
{
	ZString ISadCustomsStatusProvider.AwaitingMessageStatus => NctsMessageStatusList.Codes.DepartureDeclarationSent;

	ZString ISadCustomsStatusProvider.AcknowledgedMessageStatus => NctsMessageStatusList.Codes.Ok;

	ZString ISadCustomsStatusProvider.ClearedMessageStatus => NctsMessageStatusList.Codes.Ok;

	ZString ISadCustomsStatusProvider.ErrorMessageStatus => NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors;

	ZString ISadCustomsStatusProvider.RegisteredCustomsStatus => NctsTransitStatusList.Codes.DeclarationMrnAllocated;

	ZString ISadCustomsStatusProvider.UnderControlCustomsStatus => NctsTransitStatusList.Codes.GoodsNotReleasedForTransit;

	ZString ISadCustomsStatusProvider.ClearedCustomsStatus => NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;

	ZString ISadCustomsStatusProvider.NbRejectedCustomsStatus => NctsTransitStatusList.Codes.NbRejected;

	ZString ISadCustomsStatusProvider.ArrivalCustomsStatus => NctsTransitStatusList.Codes.GoodsWrittenOff;

	ImmutableArray<CustomsStatusOrder> ISadCustomsStatusProvider.StatusWithInformationOrderCollection
	{
		get
		{
			return ImmutableArray.Create(
				new CustomsStatusOrder(ZString.Empty, 0),
				new CustomsStatusOrder(NctsTransitStatusList.Codes.DeclarationMrnAllocated, 1),
				new CustomsStatusOrder(NctsTransitStatusList.Codes.NbRejected, 2),
				new CustomsStatusOrder(NctsTransitStatusList.Codes.GoodsNotReleasedForTransit, 3),
				new CustomsStatusOrder(NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, 4),
				new CustomsStatusOrder(NctsTransitStatusList.Codes.GoodsWrittenOff, 5));
		}
	}
}
