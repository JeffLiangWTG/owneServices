using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class DepartureTransportMeansProvider : IDepartureTransportMeans
	{
		public string TypeOfIdentification { get; private set; }

		public string IdentificationNumber { get; private set; }

		public string Nationality { get; private set; }

		public static IReadOnlyCollection<DepartureTransportMeansProvider> CreateCollection(JobDeclaration declaration)
		{
			if (declaration.CustomsEntryInstructions.Any(cei => cei.Style4thDigitIs9()))
			{
				return Array.Empty<DepartureTransportMeansProvider>();
			}

			var result = new List<DepartureTransportMeansProvider>();
			switch (declaration.JE_TransportModeInland)
			{
				case Core.Constants.TransportModes.Air:
				case Core.Constants.TransportModes.Sea:
				case Core.Constants.TransportModes.InlandWaterwayTransport:
				case Core.Constants.TransportModes.OwnPropulsion:
				case Core.Constants.TransportModes.Mail when !declaration.JE_TransportMeans.IsEmpty:
				case Core.Constants.TransportModes.FixedTransportInstallations when !declaration.JE_TransportMeans.IsEmpty:
					AddTransportIDTransportMeans(result, declaration);
					break;
				case Core.Constants.TransportModes.Road:
					AddTransportIDTransportMeans(result, declaration);
					AddTrailerTransportMeans(result, declaration.JE_Trailer1RegNo, declaration.JE_RN_NKTrailer1Nationality);
					AddTrailerTransportMeans(result, declaration.JE_Trailer2RegNo, declaration.JE_RN_NKTrailer2Nationality);
					break;
				case Core.Constants.TransportModes.Rail:
					AddTransportIDTransportMeans(result, declaration);
					AddAdditionalWagonTransportMeans(result, declaration.InlandTransports);
					break;
			}

			return result;
		}

		static void AddAdditionalWagonTransportMeans(ICollection<DepartureTransportMeansProvider> result, InlandTransportCollection additionalWagons)
		{
			foreach (var additionalWagon in additionalWagons)
			{
				result.Add(new DepartureTransportMeansProvider
				{
					IdentificationNumber = additionalWagon.CY_Data,
					TypeOfIdentification = TransportMeansList.Codes.WagonNumber,
					Nationality = additionalWagon.Nationality
				});
			}
		}

		static void AddTransportIDTransportMeans(ICollection<DepartureTransportMeansProvider> result, JobDeclaration declaration)
		{
			result.Add(new DepartureTransportMeansProvider
			{
				IdentificationNumber = declaration.JE_TransportIDInland,
				TypeOfIdentification = declaration.JE_TransportMeans,
				Nationality = declaration.JE_RN_NKTransportNationalityInland
			});
		}

		static void AddTrailerTransportMeans(ICollection<DepartureTransportMeansProvider> result, ZString trailerRegNo, ZString trailerNationality)
		{
			if (trailerRegNo.IsEmpty)
			{
				return;
			}

			result.Add(new DepartureTransportMeansProvider
			{
				IdentificationNumber = trailerRegNo,
				TypeOfIdentification = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer,
				Nationality = trailerNationality
			});
		}
	}
}
