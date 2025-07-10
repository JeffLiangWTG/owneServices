using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ExportJobDeclarationLookups : JobDeclarationLookups
{
	public ExportJobDeclarationLookups(JobDeclaration parent) : base(parent)
	{
	}

	public override ZZRefCusCodeListCombinedCollection JE_CustomsOfficeList => Parent.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Builtin
		? ZZRefCusCodeListCombinedCollection.GetCachedCollection(
			Factory,
			Core.Constants.CountryCodes.Belgium,
			new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice },
			ZDateTime.Today,
			new[]
			{
				new RefCusCodeListAttributeFilter(UniversalReferenceConstants.Role, SQLComparisonOperator.Equal, UniversalReferenceConstants.Export)
			})
		: base.JE_CustomsOfficeList;

	public override CodeDescriptionPairList TransportMeansList
	{
		get
		{
			switch (Parent.JE_TransportModeInland)
			{
				case Customs.Business.TransportTypeList.Codes.Sea:
					return Factory.GetCachedValue("BE.JobDeclarationLookups.JE_TransportMeansList_SEA", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Customs.Business.TransportMeansList.Codes.ImoShipIdentificationNumber, Customs.Business.TransportMeansList.Descriptions.ImoShipIdentificationNumber);
						result.AddPair(Customs.Business.TransportMeansList.Codes.NameOfTheSeaGoingVessel, Customs.Business.TransportMeansList.Descriptions.NameOfTheSeaGoingVessel);
						result.DefaultCode = Customs.Business.TransportMeansList.Codes.ImoShipIdentificationNumber;
						return result;
					});
				case Customs.Business.TransportTypeList.Codes.Rail:
					return Factory.GetCachedValue("BE.JobDeclarationLookups.JE_TransportMeansList_RAI", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Customs.Business.TransportMeansList.Codes.WagonNumber, Customs.Business.TransportMeansList.Descriptions.WagonNumber);
						result.AddPair(Customs.Business.TransportMeansList.Codes.TrainNumber, Customs.Business.TransportMeansList.Descriptions.TrainNumber);
						result.DefaultCode = null;
						return result;
					});
				case Customs.Business.TransportTypeList.Codes.Road:
					return Factory.GetCachedValue("BE.JobDeclarationLookups.JE_TransportMeansList_ROA", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, Customs.Business.TransportMeansList.Descriptions.RegistrationNumberOfTheRoadVehicle);
						result.DefaultCode = Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle;
						return result;
					});
				case Customs.Business.TransportTypeList.Codes.Air:
					return Factory.GetCachedValue("BE.JobDeclarationLookups.JE_TransportMeansList_AIR", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Customs.Business.TransportMeansList.Codes.IataFlightNumber, Customs.Business.TransportMeansList.Descriptions.IataFlightNumber);
						result.AddPair(Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheAircraft, Customs.Business.TransportMeansList.Descriptions.RegistrationNumberOfTheAircraft);
						result.DefaultCode = Customs.Business.TransportMeansList.Codes.IataFlightNumber;
						return result;
					});
				case Customs.Business.TransportTypeList.Codes.Mail:
				case Customs.Business.TransportTypeList.Codes.FixedTransportInstallations:
				case Customs.Business.TransportTypeList.Codes.OwnPropulsion:
					return Factory.GetCachedValue("BE.JobDeclarationLookups.JE_TransportMeansList_MAI-FIX-OWN", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Customs.Business.TransportMeansList.Codes.ImoShipIdentificationNumber, Customs.Business.TransportMeansList.Descriptions.ImoShipIdentificationNumber);
						result.AddPair(Customs.Business.TransportMeansList.Codes.NameOfTheSeaGoingVessel, Customs.Business.TransportMeansList.Descriptions.NameOfTheSeaGoingVessel);
						result.AddPair(Customs.Business.TransportMeansList.Codes.WagonNumber, Customs.Business.TransportMeansList.Descriptions.WagonNumber);
						result.AddPair(Customs.Business.TransportMeansList.Codes.TrainNumber, Customs.Business.TransportMeansList.Descriptions.TrainNumber);
						result.AddPair(Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, Customs.Business.TransportMeansList.Descriptions.RegistrationNumberOfTheRoadVehicle);
						result.AddPair(Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, Customs.Business.TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer);
						result.AddPair(Customs.Business.TransportMeansList.Codes.IataFlightNumber, Customs.Business.TransportMeansList.Descriptions.IataFlightNumber);
						result.AddPair(Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheAircraft, Customs.Business.TransportMeansList.Descriptions.RegistrationNumberOfTheAircraft);
						result.AddPair(Customs.Business.TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, Customs.Business.TransportMeansList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
						result.AddPair(Customs.Business.TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, Customs.Business.TransportMeansList.Descriptions.NameOfTheInlandWaterwaysVessel);
						result.DefaultCode = null;
						return result;
					});
				case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
					return Factory.GetCachedValue("BE.JobDeclarationLookups.JE_TransportMeansList_IWT", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Customs.Business.TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, Customs.Business.TransportMeansList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
						result.AddPair(Customs.Business.TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, Customs.Business.TransportMeansList.Descriptions.NameOfTheInlandWaterwaysVessel);
						result.DefaultCode = Customs.Business.TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode;
						return result;
					});
				default:
					return base.TransportMeansList;
			}
		}
	}
}
