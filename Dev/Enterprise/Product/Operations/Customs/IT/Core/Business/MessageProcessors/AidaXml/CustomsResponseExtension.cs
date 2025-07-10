using CargoWise.Customs.IT.MessageDefinitions.NCTS.Departure.Irildes;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public static class CustomsResponseExtension
{
	public static bool IsPositive(this IResponseMessage responseMessage) => responseMessage.ResponseStatus is Ucc6AcknowledgementStatusList.Codes.ElaborationOkWithoutResult or Ucc6AcknowledgementStatusList.Codes.ElaborationOkWithResult;

	public static (ZString DepartureOffice, ZDateTime GoodsWrittenOffClosedDate) GetGoodsWrittenOffInfo(this IrildesResponseMessage responseMessage)
	{
		var irildesResponse = (IIrildesResponse)responseMessage.Data;
		return (irildesResponse.DepartureOffice, new ZDateTime(irildesResponse.GoodsWrittenOffClosedDate));
	}
}
