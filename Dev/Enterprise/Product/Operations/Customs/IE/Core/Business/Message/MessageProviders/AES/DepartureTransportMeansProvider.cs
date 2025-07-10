using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	public class DepartureTransportMeansProvider : ITransportMeans
	{
		public static IReadOnlyCollection<ITransportMeans> CreateCollection(JobDeclaration declaration)
		{
			var result = new List<ITransportMeans>();
			var typeOfIdentification = declaration.JE_TransportMeans;
			var transportModeInland = declaration.JE_TransportModeInland;
			var transportIDInland = declaration.JE_TransportIDInland;
			var transportNationalityInland = declaration.JE_RN_NKTransportNationalityInland;

			if (!(transportModeInland.IsEmpty || transportModeInland == Core.Constants.TransportModes.Road && typeOfIdentification == Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle && transportIDInland.IsEmpty && transportNationalityInland.IsEmpty))
			{
				result.Add(new DepartureTransportMeansProvider { TypeOfIdentification = typeOfIdentification, IdentificationNumber = transportIDInland, Nationality = transportNationalityInland });
			}

			if (transportModeInland == Core.Constants.TransportModes.Road)
			{
				if (!declaration.JE_Trailer1RegNo.IsEmpty)
				{
					result.Add(new DepartureTransportMeansProvider { TypeOfIdentification = Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, IdentificationNumber = declaration.JE_Trailer1RegNo, Nationality = declaration.JE_RN_NKTrailer1Nationality });
				}
				if (!declaration.JE_Trailer2RegNo.IsEmpty)
				{
					result.Add(new DepartureTransportMeansProvider { TypeOfIdentification = Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, IdentificationNumber = declaration.JE_Trailer2RegNo, Nationality = declaration.JE_RN_NKTrailer2Nationality });
				}
			}

			return result.ToArray();
		}

		public string TypeOfIdentification { get; set; }
		public string IdentificationNumber { get; set; }
		public string Nationality { get; set; }
	}
}
