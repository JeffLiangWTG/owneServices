using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class ActiveBorderTransportMeansProvider : IActiveBorderTransportMeans
{
	readonly AsycudaManifestHeader manifestHeader;

	ActiveBorderTransportMeansProvider(AsycudaManifestHeader manifestHeader)
	{
		this.manifestHeader = manifestHeader;
	}

	public static ActiveBorderTransportMeansProvider NewOrNull(AsycudaManifestHeader header) =>
		header is null ? null : new(header);

	public string IdentificationNumber => manifestHeader.AMA_VehicleRegistration.IsEmpty ? manifestHeader.AMA_LloydsNumber : manifestHeader.AMA_VehicleRegistration;

	public string TypeOfIdentification => manifestHeader.MOTIdentifierType;

	public string TypeOfMeansOfTransport => manifestHeader.AMA_TransportMeans;

	public int ModeOfTransport => manifestHeader.AMA_TransportMode.ToString() switch
	{
		TransportTypeList.Codes.Sea => 1,
		TransportTypeList.Codes.Rail => 2,
		TransportTypeList.Codes.Road => 3,
		TransportTypeList.Codes.Air => 4,
		TransportTypeList.Codes.InlandWaterwayTransport => 8,
		_ => 0,
	};

	public string Nationality => manifestHeader.AMA_RN_NKConveyanceNationality;

	public DateTime? ActualDepartureDate => manifestHeader.AMA_A_DEP.ToNullableDateTime();

	public DateTime? EstimatedDepartureDate => manifestHeader.AMA_E_DEP.ToNullableDateTime();

	public DateTime EstimatedArrivalDate => manifestHeader.AMA_E_ARV.UtcDateTime();

	public string ConveyanceReferenceNumber => manifestHeader.MOTIdentifier;

	public IReadOnlyCollection<IItinerary> CountriesOfRouting => countriesOfRouting ??= manifestHeader.Itinerary.Cast<RouteEntry>().ToArray(i => new ItineraryProvider(i));
	IReadOnlyCollection<IItinerary> countriesOfRouting;
}
