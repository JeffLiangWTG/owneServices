using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class PlaceOfUnloadingWrapper : PlaceOfUnloadingTypeDataProviderAbstractClass, IGeoLocationDetails
{
	PlaceOfUnloadingWrapper(ZString foreignDestPortKCode, ZString placeOfUnloading)
	{
		this.placeOfUnloading = placeOfUnloading;
		lazyUNLOCode = new Lazy<string>(() => foreignDestPortKCode.Length == Ucc6XmlConstants.GeoLocationDetails.UNLOCodeLength ? foreignDestPortKCode : null);
		lazyCountryCode = new Lazy<string>(() => foreignDestPortKCode.Length == Ucc6XmlConstants.GeoLocationDetails.CountryCodeLength ? foreignDestPortKCode : null);
	}

	public static IGeoLocationDetails NewOrNull(NctsDepartureMovementHeader nctsDepartureMovementHeader)
	{
		return GetPlaceOfUnloadingWrapper(nctsDepartureMovementHeader);
	}

	public static PlaceOfUnloadingTypeDataProviderAbstractClass NewPlaceOfUnloading(NctsDepartureMovementHeader nctsDepartureMovementHeader)
	{
		return GetPlaceOfUnloadingWrapper(nctsDepartureMovementHeader);
	}

	static PlaceOfUnloadingWrapper GetPlaceOfUnloadingWrapper(NctsDepartureMovementHeader nctsDepartureMovementHeader)
	{
		return
			nctsDepartureMovementHeader is NctsDepartureMovementHeader movementHeader
			&& !IsSecurityTypeNotApplicable()
			&& (!movementHeader.BM_PlaceOfUnloading.IsEmpty
				|| movementHeader.BM_ForeignDestPortKCode.Length == Ucc6XmlConstants.GeoLocationDetails.UNLOCodeLength
				|| movementHeader.BM_ForeignDestPortKCode.Length == Ucc6XmlConstants.GeoLocationDetails.CountryCodeLength
			)
				? new PlaceOfUnloadingWrapper(movementHeader.BM_ForeignDestPortKCode, movementHeader.BM_PlaceOfUnloading)
		: null;

		bool IsSecurityTypeNotApplicable() => !movementHeader.IsInPhase5TransitionPeriod && movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON;
	}

	string IGeoLocationDetails.UNLOCode => lazyUNLOCode.Value?.Trim();
	readonly Lazy<string> lazyUNLOCode;

	string IGeoLocationDetails.CountryCode => lazyCountryCode.Value?.Trim();
	readonly Lazy<string> lazyCountryCode;

	string IGeoLocationDetails.Location => placeOfUnloading.Trim();

	public override string UNLOCODE => lazyUNLOCode.Value?.Trim();

	public override string Country => lazyCountryCode.Value?.Trim();

	public override string Location => placeOfUnloading.Trim();

	readonly ZString placeOfUnloading;
}
