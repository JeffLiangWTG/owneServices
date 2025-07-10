using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using TransportTypeListCodes = Enterprise.Customs.Business.TransportTypeList.Codes;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InlandTransportCodeDescriptionPairListBuilder
	{
		public InlandTransportCodeDescriptionPairListBuilder(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		protected readonly JobDeclaration declaration;

		public CodeDescriptionPairList GetList()
		{
			var result = new CodeDescriptionPairList();

			switch (declaration.TransportModeValueForTransportMeans)
			{
				case TransportTypeListCodes.Air:
					AddAirPairs(result);
					break;

				case TransportTypeListCodes.FixedTransportInstallations:
					AddFixedTransportInstallationsPairs(result);
					break;

				case TransportTypeListCodes.InlandWaterwayTransport:
					AddInlandWaterwayTransportPairs(result);
					break;

				case TransportTypeListCodes.OwnPropulsion:
					AddOwnPropulsionPairs(result);
					break;

				case TransportTypeListCodes.Mail:
					AddMailPairs(result);
					break;

				case TransportTypeListCodes.Rail:
					AddRailPairs(result);
					break;

				case TransportTypeListCodes.Road:
					AddRoadPairs(result);
					break;

				case TransportTypeListCodes.Sea:
					AddSeaPairs(result);
					break;

				default:
					result.AddRange(GetDefaultList());
					break;
			}

			result.Sort();
			return result;
		}

		protected virtual void AddAirPairs(CodeDescriptionPairList result)
		{
			result.AddPair(MeansOfTransportList.Codes.IataFlightNumber, MeansOfTransportList.Descriptions.IataFlightNumber);
			result.AddPair(MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft, MeansOfTransportList.Descriptions.RegistrationNumberOfTheAircraft);
			result.DefaultCode = MeansOfTransportList.Codes.IataFlightNumber;
		}

		protected virtual void AddInlandWaterwayTransportPairs(CodeDescriptionPairList result)
		{
			result.AddPair(MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode, MeansOfTransportList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
			result.AddPair(MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, MeansOfTransportList.Descriptions.NameOfTheInlandWaterwaysVessel);
			result.DefaultCode = MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel;
		}

		protected virtual void AddRailPairs(CodeDescriptionPairList result)
		{
			result.AddPair(MeansOfTransportList.Codes.WagonNumber, MeansOfTransportList.Descriptions.WagonNumber);
			result.AddPair(InlandMeansOfTransportList.Codes.TrainNumber, InlandMeansOfTransportList.Descriptions.TrainNumber);
			result.DefaultCode = MeansOfTransportList.Codes.WagonNumber;
		}

		protected virtual void AddRoadPairs(CodeDescriptionPairList result)
		{
			result.AddPair(MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle, MeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadVehicle);
			result.AddPair(InlandMeansOfTransportList.Codes.RegistrationNumberOfTheRoadTrailer, InlandMeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadTrailer);
			result.DefaultCode = MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle;
		}

		protected virtual void AddSeaPairs(CodeDescriptionPairList result)
		{
			result.AddPair(MeansOfTransportList.Codes.ImoShipIdentificationNumber, MeansOfTransportList.Descriptions.ImoShipIdentificationNumber);
			result.AddPair(MeansOfTransportList.Codes.NameOfTheSeaGoingVessel, MeansOfTransportList.Descriptions.NameOfTheSeaGoingVessel);
			result.DefaultCode = MeansOfTransportList.Codes.NameOfTheSeaGoingVessel;
		}

		protected virtual void AddFixedTransportInstallationsPairs(CodeDescriptionPairList result)
		{
			result.AddRange(GetDefaultList());
		}

		protected virtual void AddOwnPropulsionPairs(CodeDescriptionPairList result)
		{
			result.AddRange(GetDefaultList());
		}

		protected virtual void AddMailPairs(CodeDescriptionPairList result)
		{
			result.AddRange(GetDefaultList());
		}

		protected virtual CodeDescriptionPairList GetDefaultList() => declaration.IsExport ? new InlandMeansOfTransportList() : new MeansOfTransportList();
	}
}
