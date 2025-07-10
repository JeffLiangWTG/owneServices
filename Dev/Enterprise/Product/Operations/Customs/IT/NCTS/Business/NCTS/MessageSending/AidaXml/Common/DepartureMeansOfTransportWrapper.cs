using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class DepartureMeansOfTransportWrapper : DepartureTransportMeansTypeDataProviderAbstractClass, IMeansOfTransport
{
	DepartureMeansOfTransportWrapper(int typeOfIdentification, string identificationNumber, string nationality)
	{
		TypeOfIdentification = typeOfIdentification;
		IdentificationNumber = identificationNumber;
		Nationality = nationality;
	}

	public override int SequenceNumber => default;

	public override int? TypeOfIdentification { get; }

	public override string IdentificationNumber { get; }

	public override string Nationality { get; }

	int IMeansOfTransport.TypeOfIdentification => TypeOfIdentification!.Value;

	internal static IReadOnlyCollection<DepartureMeansOfTransportWrapper> CollectFromMovementHeader(NctsDepartureMovementHeader movementHeader)
		=> From(movementHeader).WhereNotNull().ToCollection();

	internal static IReadOnlyCollection<DepartureMeansOfTransportWrapper> CollectFromBill(NctsBill bill)
		=> From(bill).WhereNotNull().ToCollection();

	internal static DepartureMeansOfTransportWrapper NewOrNull(string typeOfIdentification, string identificationNumber, string nationality)
	{
		var theTypeOfIdentification = int.TryParse(typeOfIdentification, out var result)
			? result
			: -1;

		if (!identificationNumber.IsEmpty()
			|| !nationality.IsEmpty()
			|| theTypeOfIdentification != -1)
		{
			return new DepartureMeansOfTransportWrapper(theTypeOfIdentification, identificationNumber.Trim(), nationality);
		}
		return null;
	}

	internal static DepartureMeansOfTransportWrapper NewOrNullForTrailer(string identificationNumber, string nationality)
	{
		if (identificationNumber.IsEmpty()
			&& nationality.IsEmpty())
		{
			return null;
		}
		return NewOrNull(NctsTransportTypeOfIdList.Codes._31, identificationNumber, nationality);
	}

	#region Implementation

	static IEnumerable<DepartureMeansOfTransportWrapper> From(NctsDepartureMovementHeader movementHeader)
	{
		switch (movementHeader.BM_InlandTransportMode)
		{
			case ModeOfTransportList.Codes._1_SeaTransport:
			case ModeOfTransportList.Codes._4_AirTransport:
			case ModeOfTransportList.Codes._7_FixedTransportInstallations:
			case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
			case ModeOfTransportList.Codes._9_OwnPropulsion:
				yield return NewOrNull(
					movementHeader.BM_TransportAtDepartureType,
					movementHeader.BM_TransportAtDeparture,
					movementHeader.BM_RN_NKTransportAtDepartureCountry
					);
				break;
			case ModeOfTransportList.Codes._2_RailTransport:
				var isInPhase5TransitionPeriod = movementHeader.IsInPhase5TransitionPeriod;
				yield return NewOrNull(
					movementHeader.BM_TransportAtDepartureType,
					movementHeader.BM_TransportAtDeparture,
					nationality: isInPhase5TransitionPeriod
						? null
						: movementHeader.BM_RN_NKTransportAtDepartureCountry
				);
				if (movementHeader.BM_TransportAtDeparture.IsEmpty || isInPhase5TransitionPeriod)
				{
					break;
				}
				foreach (var additionalWagon in movementHeader.AdditionalWagons.Where(w => !w.WagonNumber.IsEmpty))
				{
					yield return NewOrNull(
						NctsTransportTypeOfIdList.Codes._20,
						additionalWagon.WagonNumber,
						additionalWagon.WagonNationality
					);
				}
				break;
			case ModeOfTransportList.Codes._3_RoadTransport:
				yield return NewOrNull(
					movementHeader.BM_TransportAtDepartureType,
					movementHeader.BM_TransportAtDeparture,
					movementHeader.BM_RN_NKTransportAtDepartureCountry
					);
				yield return NewOrNullForTrailer(
					movementHeader.Trailer1IDAtDeparture,
					movementHeader.Trailer1NationalityAtDeparture
					);
				yield return NewOrNullForTrailer(
					movementHeader.Trailer2IDAtDeparture,
					movementHeader.Trailer2NationalityAtDeparture
					);
				break;
			default:
				yield return null;
				break;
		}
	}

	static IEnumerable<DepartureMeansOfTransportWrapper> From(NctsBill bill)
	{
		switch (bill.InlandTransportModeAtDeparture)
		{
			case ModeOfTransportList.Codes._1_SeaTransport:
				yield return NewOrNull(
					bill.TransportTypeAtDeparture,
					bill.VesselNameAtDeparture,
					bill.VesselCountryAtDeparture);
				break;
			case ModeOfTransportList.Codes._2_RailTransport:
				yield return NewOrNull(
					bill.TransportTypeAtDeparture,
					bill.TransportAtDeparture,
					bill.TransportCountryAtDeparture);

				if (bill.TransportAtDeparture.IsEmpty)
				{
					break;
				}

				foreach (var additionalWagon in bill.AdditionalWagons.Where(w => !w.WagonNumber.IsEmpty))
				{
					yield return NewOrNull(
						NctsTransportTypeOfIdList.Codes._20,
						additionalWagon.WagonNumber,
						additionalWagon.WagonNationality
					);
				}
				break;
			case ModeOfTransportList.Codes._4_AirTransport:
			case ModeOfTransportList.Codes._7_FixedTransportInstallations:
			case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
			case ModeOfTransportList.Codes._9_OwnPropulsion:
				yield return NewOrNull(
					bill.TransportTypeAtDeparture,
					bill.TransportAtDeparture,
					bill.TransportCountryAtDeparture);
				break;
			case ModeOfTransportList.Codes._3_RoadTransport:
				yield return NewOrNull(
					bill.TransportTypeAtDeparture,
					bill.TransportAtDeparture,
					bill.TransportCountryAtDeparture);
				yield return NewOrNullForTrailer(
					bill.Trailer1IDAtDeparture,
					bill.Trailer1NationalityAtDeparture);
				yield return NewOrNullForTrailer(
					bill.Trailer2IDAtDeparture,
					bill.Trailer2NationalityAtDeparture);
				break;
			default:
				yield return null;
				break;
		}
	}

	#endregion
}

sealed class DepartureMeansOfTransportHCWrapper : DepartureTransportMeansHCTypeDataProviderAbstractClass
{
	DepartureMeansOfTransportHCWrapper(DepartureMeansOfTransportWrapper wrapper)
	{
		TypeOfIdentification = wrapper.TypeOfIdentification!.Value;
		IdentificationNumber = wrapper.IdentificationNumber;
		Nationality = wrapper.Nationality;
	}

	public override int SequenceNumber => default;

	public override int TypeOfIdentification { get; }

	public override string IdentificationNumber { get; }

	public override string Nationality { get; }

	internal static IReadOnlyCollection<DepartureMeansOfTransportHCWrapper> CollectFromBill(NctsBill bill)
		=> bill.Header is NctsHeader header && !header.IsInPhase5TransitionPeriod
		? SharedValueMapResolverProvider.GetDepartureTransportMeansMapResolver().GetValueForLine(bill)?.Select(x => new DepartureMeansOfTransportHCWrapper(x)).ToCollection() ?? []
		: DepartureMeansOfTransportWrapper.CollectFromBill(bill).Select(x => new DepartureMeansOfTransportHCWrapper(x)).ToCollection();
}
