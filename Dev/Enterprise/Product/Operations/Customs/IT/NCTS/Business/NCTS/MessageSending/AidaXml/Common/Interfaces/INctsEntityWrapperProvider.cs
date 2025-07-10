using Enterprise.Customs.IT.NCTS.Business.NCTS.MessageSending.AidaXml;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public interface INctsEntityWrapperProvider
{
	INctsDepartureMovementHeaderWrapper GetNctsDepartureMovementHeaderWrapper(NctsDepartureMovementHeader movementHeader);

	INctsHeaderWrapper GetNctsHeaderWrapper(NctsHeader header);

	IHouseConsignmentCustomsMessageWrapper GetHouseConsignmentWrapper(NctsBill nctsBill);
}
