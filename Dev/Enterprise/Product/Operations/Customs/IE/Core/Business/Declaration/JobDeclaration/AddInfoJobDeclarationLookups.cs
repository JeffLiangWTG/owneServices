using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using TransportMeans = Enterprise.Customs.Business.TransportMeansList;
using TransportTypeListCodes = Enterprise.Customs.Business.TransportTypeList.Codes;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobDeclarationLookups
	{
		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public override CodeDescriptionPairList SpecificCircumstanceIndicatorList =>
				RefCusCodeListTypes.GetCachedList(
					Parent.Factory,
					Core.Constants.CountryCodes.Ireland,
					Constants.RefCusCodeListTypes.SpecificCircumstanceIndicatorType,
					ZDateTime.Today,
					includeParentDataGrouping: false
				);

		public override CodeDescriptionPairList TransportMeansList
		{
			get
			{
				CodeDescriptionPairList result;
				var declaration = Declaration;
				if (declaration.IsImport || declaration.IsMessageTypeExport)
				{
					result = GetTransportMeansListFiltered(declaration.JE_TransportModeInland);
				}
				else
				{
					result = new CodeDescriptionPairList();
				}
				return result;
			}
		}

		protected CodeDescriptionPairList GetTransportMeansListFiltered(ZString inlandTransportMode)
		{
			return Factory.GetCachedValue(string.Join("|", "IE.JobDeclarationLookups.TransportMeansList", inlandTransportMode), () =>
			{
				var result = new CodeDescriptionPairList();
				switch (inlandTransportMode)
				{
					case TransportTypeListCodes.Sea:
						result.AddPair(TransportMeans.Codes.ImoShipIdentificationNumber, TransportMeans.Descriptions.ImoShipIdentificationNumber);
						result.AddPair(TransportMeans.Codes.NameOfTheSeaGoingVessel, TransportMeans.Descriptions.NameOfTheSeaGoingVessel);
						result.DefaultCode = TransportMeans.Codes.ImoShipIdentificationNumber;
						break;
					case TransportTypeListCodes.Rail:
						result.AddPair(TransportMeans.Codes.WagonNumber, TransportMeans.Descriptions.WagonNumber);
						result.AddPair(TransportMeans.Codes.TrainNumber, TransportMeans.Descriptions.TrainNumber);
						result.DefaultCode = null;
						break;
					case TransportTypeListCodes.Road:
						result.AddPair(TransportMeans.Codes.RegistrationNumberOfTheRoadVehicle, TransportMeans.Descriptions.RegistrationNumberOfTheRoadVehicle);
						result.DefaultCode = TransportMeans.Codes.RegistrationNumberOfTheRoadVehicle;
						break;
					case TransportTypeListCodes.Air:
						result.AddPair(TransportMeans.Codes.IataFlightNumber, TransportMeans.Descriptions.IataFlightNumber);
						result.AddPair(TransportMeans.Codes.RegistrationNumberOfTheAircraft, TransportMeans.Descriptions.RegistrationNumberOfTheAircraft);
						result.DefaultCode = TransportMeans.Codes.IataFlightNumber;
						break;
					case TransportTypeListCodes.Mail:
					case TransportTypeListCodes.FixedTransportInstallations:
					case TransportTypeListCodes.OwnPropulsion:
						result.AddPair(TransportMeans.Codes.ImoShipIdentificationNumber, TransportMeans.Descriptions.ImoShipIdentificationNumber);
						result.AddPair(TransportMeans.Codes.NameOfTheSeaGoingVessel, TransportMeans.Descriptions.NameOfTheSeaGoingVessel);
						result.AddPair(TransportMeans.Codes.WagonNumber, TransportMeans.Descriptions.WagonNumber);
						result.AddPair(TransportMeans.Codes.TrainNumber, TransportMeans.Descriptions.TrainNumber);
						result.AddPair(TransportMeans.Codes.RegistrationNumberOfTheRoadVehicle, TransportMeans.Descriptions.RegistrationNumberOfTheRoadVehicle);
						result.AddPair(TransportMeans.Codes.RegistrationNumberOfTheRoadTrailer, TransportMeans.Descriptions.RegistrationNumberOfTheRoadTrailer);
						result.AddPair(TransportMeans.Codes.IataFlightNumber, TransportMeans.Descriptions.IataFlightNumber);
						result.AddPair(TransportMeans.Codes.RegistrationNumberOfTheAircraft, TransportMeans.Descriptions.RegistrationNumberOfTheAircraft);
						result.AddPair(TransportMeans.Codes.EuropeanVesselIdentificationNumberEniCode, TransportMeans.Descriptions.EuropeanVesselIdentificationNumberEniCode);
						result.AddPair(TransportMeans.Codes.NameOfTheInlandWaterwaysVessel, TransportMeans.Descriptions.NameOfTheInlandWaterwaysVessel);
						result.DefaultCode = null;
						break;
					case TransportTypeListCodes.InlandWaterwayTransport:
						result.AddPair(TransportMeans.Codes.EuropeanVesselIdentificationNumberEniCode, TransportMeans.Descriptions.EuropeanVesselIdentificationNumberEniCode);
						result.AddPair(TransportMeans.Codes.NameOfTheInlandWaterwaysVessel, TransportMeans.Descriptions.NameOfTheInlandWaterwaysVessel);
						result.DefaultCode = TransportMeans.Codes.EuropeanVesselIdentificationNumberEniCode;
						break;
					default:
						break;
				}

				return result;
			});
		}

		public override ICollection AgreedPlaceCodeList => Parent.AgreedPlaceUsesUNLOCO
				? new RefUNLOCOCollection(Factory)
				: new RefCountryCollection(Factory);

		protected override CodeDescriptionPairList RegionOfDestinationListCore => Factory.GetCachedValue<RegionOfDestinationList>();
	}
}
