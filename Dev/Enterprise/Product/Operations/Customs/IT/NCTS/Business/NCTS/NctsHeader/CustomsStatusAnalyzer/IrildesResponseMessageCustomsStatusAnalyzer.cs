using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class IrildesResponseMessageCustomsStatusAnalyzer : ResponseMessageCustomsStatusAnalyzer<IrildesResponseMessage>
{
	public IrildesResponseMessageCustomsStatusAnalyzer(NctsDepartureMovementHeader movementHeader) : base(movementHeader, EDIMessageTypeList.Codes.IrildesResponse)
	{
	}

	protected override IrildesResponseMessage GetResponseMessage(EDIMessage response) => new IrildesResponseMessage(response.EM_MessageText);

	protected override bool IsValidResponseMessage(EDIMessage response) => response.EM_MessageType == MessageProcessorConstants.InterchangeTypes.IrildesResponseMessageType;

	protected override ZString GetStatusFromResponseMessage(IrildesResponseMessage responseMessage)
	{
		if (!responseMessage.IsPositive())
		{
			return ZString.Empty;
		}

		var (_, goodsWrittenOffClosedDate) = responseMessage.GetGoodsWrittenOffInfo();
		if (goodsWrittenOffClosedDate.IsEmpty || !goodsWrittenOffClosedDate.IsValid)
		{
			return ZString.Empty;
		}

		return NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
	}
}
