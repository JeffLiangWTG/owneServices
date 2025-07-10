using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.NCTS.MessageSending.AidaXml;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsDepartureMovementHeaderWrapper : INctsDepartureMovementHeaderWrapper
{
	public NctsDepartureMovementHeaderWrapper(NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
	}
	readonly NctsDepartureMovementHeader movementHeader;

	int INctsDepartureMovementHeaderWrapper.GetSecurityType()
	{
		switch (movementHeader.BM_TypeOfSecurity)
		{
			case NctsTypeOfSecurityList.Codes.ENT:
				return 1;

			case NctsTypeOfSecurityList.Codes.EXI:
				return 2;

			case NctsTypeOfSecurityList.Codes.BTH:
				return 3;

			case NctsTypeOfSecurityList.Codes.NON:
			default:
				return 0;
		}
	}

	int INctsDepartureMovementHeaderWrapper.GetReducedDatasetIndicator()
		=> movementHeader.BM_ReducedDatasetIndicator ? 1 : 0;

	string INctsDepartureMovementHeaderWrapper.GetSpecificCircumstanceIndicator()
		=> movementHeader.BM_SpecificCircumstance;

	DateTime? INctsDepartureMovementHeaderWrapper.GetLimitDate()
	{
		var exportDate = movementHeader.BM_ExportDate;
		if (exportDate.IsEmpty || !exportDate.IsValid)
		{
			return null;
		}
		return exportDate.Date.ToDateTime();
	}

	PlaceOfLoadingTypeDataProviderAbstractClass INctsDepartureMovementHeaderWrapper.GetPlaceOfLoading() => PlaceOfLoadingWrapper.NewPlaceOfLoading(movementHeader);

	PlaceOfUnloadingTypeDataProviderAbstractClass INctsDepartureMovementHeaderWrapper.GetPlaceOfUnloading() => PlaceOfUnloadingWrapper.NewPlaceOfUnloading(movementHeader);

	LocationOfGoodsTypeDataProviderAbstractClass INctsDepartureMovementHeaderWrapper.GetLocationOfGoods()
	{
		var goodsLocation = movementHeader.GoodsLocation;
		return goodsLocation.CGL_Qualifier.IsEmpty
			? null
			: new LocationOfGoodsWrapper(goodsLocation);
	}

	IReadOnlyCollection<DepartureTransportMeansTypeDataProviderAbstractClass> INctsDepartureMovementHeaderWrapper.GetDepartureTransportMeans()
		=> movementHeader.Header is NctsHeader header && !header.IsInPhase5TransitionPeriod
		? SharedValueMapResolverProvider.GetDepartureTransportMeansMapResolver().GetValueForHeader(header)
		: DepartureMeansOfTransportWrapper.CollectFromMovementHeader(movementHeader);

	IReadOnlyCollection<ActiveBorderTransportMeansTypeDataProviderAbstractClass> INctsDepartureMovementHeaderWrapper.GetActiveBorderTransportMeans()
		=> GetActiveBoderTransportMeansCore()
			.WhereNotNull()
			.ToCollection();

	IEnumerable<ActiveBorderTransportMeansTypeDataProviderAbstractClass> GetActiveBoderTransportMeansCore()
	{
		yield return ActiveBorderMeansOfTransportWrapper.NewOrNullActiveBorderTransportMeansType(movementHeader);

		foreach (var departureCusTransportMeans in movementHeader.AdditionalTransportAtBorderList.Cast<DepartureCusTransportMeans>())
		{
			yield return ActiveBorderMeansOfTransportWrapper.NewOrNullActiveBorderTransportMeansType(departureCusTransportMeans);
		}
	}
}
