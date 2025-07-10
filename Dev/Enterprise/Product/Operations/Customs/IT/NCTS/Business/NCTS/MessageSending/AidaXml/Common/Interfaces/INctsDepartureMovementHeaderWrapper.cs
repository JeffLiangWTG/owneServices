using System;
using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.NCTS.MessageSending.AidaXml;

public interface INctsDepartureMovementHeaderWrapper
{
	int GetSecurityType();

	int GetReducedDatasetIndicator();

	string GetSpecificCircumstanceIndicator();

	DateTime? GetLimitDate();

	PlaceOfLoadingTypeDataProviderAbstractClass GetPlaceOfLoading();

	PlaceOfUnloadingTypeDataProviderAbstractClass GetPlaceOfUnloading();

	LocationOfGoodsTypeDataProviderAbstractClass GetLocationOfGoods();

	IReadOnlyCollection<DepartureTransportMeansTypeDataProviderAbstractClass> GetDepartureTransportMeans();

	IReadOnlyCollection<ActiveBorderTransportMeansTypeDataProviderAbstractClass> GetActiveBorderTransportMeans();
}
