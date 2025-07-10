using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DepartureTransportMeansProvider : ITransportMeans
	{
		public static ITransportMeans GetTransportMean(EnRouteIncident incident) => new DepartureTransportMeansProvider(incident.BN_TransportAtDepartureType, incident.BN_TransportAtDepartureID, incident.BN_RN_NKTransportAtDepartureIDNationality);

		public static IReadOnlyCollection<ITransportMeans> GetTransportMeans(IDepartureTransportMeansProvider businessObjProvideDepartureTransportMeans)
		{
			var results = new List<ITransportMeans>();
			if (businessObjProvideDepartureTransportMeans != null)
			{
				switch (businessObjProvideDepartureTransportMeans.InlandTransportModeAtDeparture)
				{
					case ModeOfTransportList.Codes._1_SeaTransport:
						results.Add(new DepartureTransportMeansProvider(businessObjProvideDepartureTransportMeans.TransportTypeAtDeparture, businessObjProvideDepartureTransportMeans.VesselNameAtDeparture, businessObjProvideDepartureTransportMeans.VesselCountryAtDeparture));
						break;
					case ModeOfTransportList.Codes._2_RailTransport:
						if (!businessObjProvideDepartureTransportMeans.Trailer1IDAtDeparture.IsEmpty)
						{
							results.Add(new DepartureTransportMeansProvider(NctsTransportTypeOfIdList.Codes._20, businessObjProvideDepartureTransportMeans.Trailer1IDAtDeparture, businessObjProvideDepartureTransportMeans.Trailer1NationalityAtDeparture));
						}
						else if (!businessObjProvideDepartureTransportMeans.TransportAtDeparture.IsEmpty)
						{
							results.Add(new DepartureTransportMeansProvider(NctsTransportTypeOfIdList.Codes._21, businessObjProvideDepartureTransportMeans.TransportAtDeparture, businessObjProvideDepartureTransportMeans.TransportCountryAtDeparture));
						}
						break;
					case ModeOfTransportList.Codes._3_RoadTransport:
						if (!businessObjProvideDepartureTransportMeans.TransportAtDeparture.IsEmpty)
						{
							results.Add(new DepartureTransportMeansProvider(NctsTransportTypeOfIdList.Codes._30, businessObjProvideDepartureTransportMeans.TransportAtDeparture, businessObjProvideDepartureTransportMeans.TransportCountryAtDeparture));
						}
						if (!businessObjProvideDepartureTransportMeans.Trailer1IDAtDeparture.IsEmpty)
						{
							results.Add(new DepartureTransportMeansProvider(NctsTransportTypeOfIdList.Codes._31, businessObjProvideDepartureTransportMeans.Trailer1IDAtDeparture, businessObjProvideDepartureTransportMeans.Trailer1NationalityAtDeparture));
						}
						if (!businessObjProvideDepartureTransportMeans.Trailer2IDAtDeparture.IsEmpty)
						{
							results.Add(new DepartureTransportMeansProvider(NctsTransportTypeOfIdList.Codes._31, businessObjProvideDepartureTransportMeans.Trailer2IDAtDeparture, businessObjProvideDepartureTransportMeans.Trailer2NationalityAtDeparture));
						}
						break;
					case ModeOfTransportList.Codes._4_AirTransport:
						if (!businessObjProvideDepartureTransportMeans.AircraftIDAtDeparture.IsEmpty)
						{
							results.Add(new DepartureTransportMeansProvider(NctsTransportTypeOfIdList.Codes._41, businessObjProvideDepartureTransportMeans.AircraftIDAtDeparture, businessObjProvideDepartureTransportMeans.TransportCountryAtDeparture));
						}
						else if (!businessObjProvideDepartureTransportMeans.TransportAtDeparture.IsEmpty)
						{
							results.Add(new DepartureTransportMeansProvider(businessObjProvideDepartureTransportMeans.TransportTypeAtDeparture, businessObjProvideDepartureTransportMeans.TransportAtDeparture, businessObjProvideDepartureTransportMeans.TransportCountryAtDeparture));
						}
						break;
					case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
						if (!businessObjProvideDepartureTransportMeans.VesselNameAtDeparture.IsEmpty)
						{
							results.Add(new DepartureTransportMeansProvider(businessObjProvideDepartureTransportMeans.TransportTypeAtDeparture, businessObjProvideDepartureTransportMeans.VesselNameAtDeparture, businessObjProvideDepartureTransportMeans.VesselCountryAtDeparture));
						}
						break;
					case ModeOfTransportList.Codes._9_OwnPropulsion:
						results.Add(new DepartureTransportMeansProvider(businessObjProvideDepartureTransportMeans.TransportTypeAtDeparture, businessObjProvideDepartureTransportMeans.TransportAtDeparture, businessObjProvideDepartureTransportMeans.TransportCountryAtDeparture));
						break;
				}
			}
			return results.ToArray();
		}

		DepartureTransportMeansProvider(ZString typeOfIdentification, ZString identificationNumber, ZString nationality)
		{
			this.typeOfIdentification = typeOfIdentification;
			this.identificationNumber = identificationNumber;
			this.nationality = identificationNumber.IsEmpty ? ZString.Empty : nationality;
		}

		public string TypeOfIdentification => typeOfIdentification;

		readonly string typeOfIdentification;
		public string IdentificationNumber => identificationNumber;
		readonly string identificationNumber;

		public string Nationality => nationality;
		readonly string nationality;
	}
}
