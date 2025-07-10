using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public interface INctsHeaderWrapper
{
	int GetBindingItinerary();

	CarrierTypeDataProviderAbstractClass GetCarrier();

	int? GetContainerIndicator();

	IReadOnlyCollection<GoodsReferenceTypeDataProviderAbstractClass> GetGoodsReference(NctsDepartureHeaderContainer container);

	ITrader GetConsignee();
}
