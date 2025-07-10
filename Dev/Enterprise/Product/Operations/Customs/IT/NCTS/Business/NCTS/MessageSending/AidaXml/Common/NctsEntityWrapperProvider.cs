using CargoWise.Common;
using Enterprise.Customs.IT.NCTS.Business.NCTS.MessageSending.AidaXml;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsEntityWrapperProvider : INctsEntityWrapperProvider
{
	INctsDepartureMovementHeaderWrapper INctsEntityWrapperProvider.GetNctsDepartureMovementHeaderWrapper(NctsDepartureMovementHeader movementHeader)
	{
		Argument.NotNull(movementHeader, nameof(movementHeader));
		return new NctsDepartureMovementHeaderWrapper(movementHeader);
	}

	INctsHeaderWrapper INctsEntityWrapperProvider.GetNctsHeaderWrapper(NctsHeader header)
	{
		Argument.NotNull(header, nameof(header));
		return new NctsHeaderWrapper(header);
	}

	IHouseConsignmentCustomsMessageWrapper INctsEntityWrapperProvider.GetHouseConsignmentWrapper(NctsBill bill)
	{
		Argument.NotNull(bill, nameof(bill));
		return new HouseConsignmentCustomsMessageWrapper(bill);
	}
}
