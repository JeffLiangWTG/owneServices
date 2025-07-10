using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class PlaceOfLoadingWrapper : PlaceOfLoadingTypeDataProviderAbstractClass, IGeoLocationDetails
{
	PlaceOfLoadingWrapper(ZString bM_PortOfPresentationCode, ZString bM_PlaceOfLoading)
	{
		placeOfLoading = bM_PlaceOfLoading;
		lazyUNLOCode = new Lazy<string>(() => bM_PortOfPresentationCode.Length == Ucc6XmlConstants.GeoLocationDetails.UNLOCodeLength ? bM_PortOfPresentationCode : null);
		lazyCountryCode = new Lazy<string>(() => bM_PortOfPresentationCode.Length == Ucc6XmlConstants.GeoLocationDetails.CountryCodeLength ? bM_PortOfPresentationCode : null);
	}

	public static IGeoLocationDetails NewOrNull(NctsDepartureMovementHeader nctsDepartureMovementHeader)
		=> CreatePlaceOfLoadingWrapper(nctsDepartureMovementHeader);

	public static PlaceOfLoadingTypeDataProviderAbstractClass NewPlaceOfLoading(NctsDepartureMovementHeader nctsDepartureMovementHeader)
		=> CreatePlaceOfLoadingWrapper(nctsDepartureMovementHeader);

	static PlaceOfLoadingWrapper CreatePlaceOfLoadingWrapper(NctsDepartureMovementHeader nctsDepartureMovementHeader)
	{
		return
			nctsDepartureMovementHeader is NctsDepartureMovementHeader movementHeader
			&& (!movementHeader.BM_PlaceOfLoading.IsEmpty
				|| movementHeader.BM_PortOfPresentationCode.Length == Ucc6XmlConstants.GeoLocationDetails.UNLOCodeLength
				|| movementHeader.BM_PortOfPresentationCode.Length == Ucc6XmlConstants.GeoLocationDetails.CountryCodeLength
			)
				? new PlaceOfLoadingWrapper(movementHeader.BM_PortOfPresentationCode, movementHeader.BM_PlaceOfLoading)
				: null;
	}

	string IGeoLocationDetails.UNLOCode => lazyUNLOCode.Value?.Trim();
	readonly Lazy<string> lazyUNLOCode;

	string IGeoLocationDetails.CountryCode => lazyCountryCode.Value?.Trim();
	readonly Lazy<string> lazyCountryCode;

	string IGeoLocationDetails.Location => placeOfLoading.Trim();

	public override string UNLOCODE => lazyUNLOCode.Value?.Trim();

	public override string Country => lazyCountryCode.Value?.Trim();

	public override string Location => placeOfLoading.Trim();

	readonly ZString placeOfLoading;
}
