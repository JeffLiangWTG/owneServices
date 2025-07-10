using System.Collections;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageHeaderLookups : EU.Business.CusTempStorage.TemporaryStorageHeaderLookups
	{
		public TemporaryStorageHeaderLookups(EU.Business.CusTempStorage.TemporaryStorageHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ManifestTypeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeaderLookups.ManifestTypeList", () =>
				{
					var applicationCodeList = new ImportDeclarationApplicationCodeList();
					applicationCodeList.RemoveCode(ImportDeclarationApplicationCodeList.Codes.Interfaced);
					return applicationCodeList;
				});
			}
		}

		public CodeDescriptionPairList RepresentativeStatusCodeList => Factory.GetCachedValue<RepresentativeStatusCodeList>();

		public CodeDescriptionPairList MeansIdentityTypeList => Factory.GetCachedValue<MeansOfTransportList>();

		protected override CodeDescriptionPairList TransportTypeListCore
		{
			get
			{
				var transportMode = Parent.AMA_TransportMode;
				return Factory.GetCachedValue("IE.TemporaryStorageHeaderLookups.TransportTypeList." + transportMode, () =>
				{
					CodeDescriptionPairList result;
					switch (transportMode)
					{
						case Customs.Business.TransportTypeList.Codes.Sea:
							result = new CodeDescriptionPairList();
							result.AddPair(MeansOfTransportList.Codes.ImoShipIdentificationNumber, MeansOfTransportList.Descriptions.ImoShipIdentificationNumber);
							result.AddPair(MeansOfTransportList.Codes.NameOfTheSeaGoingVessel, MeansOfTransportList.Descriptions.NameOfTheSeaGoingVessel);
							break;
						case Customs.Business.TransportTypeList.Codes.Air:
							result = new CodeDescriptionPairList();
							result.AddPair(MeansOfTransportList.Codes.IataFlightNumber, MeansOfTransportList.Descriptions.IataFlightNumber);
							result.AddPair(MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft, MeansOfTransportList.Descriptions.RegistrationNumberOfTheAircraft);
							break;
						case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
							result = new CodeDescriptionPairList();
							result.AddPair(MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode, MeansOfTransportList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
							result.AddPair(MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, MeansOfTransportList.Descriptions.NameOfTheInlandWaterwaysVessel);
							break;
						case Customs.Business.TransportTypeList.Codes.Rail:
							result = new CodeDescriptionPairList();
							result.AddPair(MeansOfTransportList.Codes.WagonNumber, MeansOfTransportList.Descriptions.WagonNumber);
							break;
						case Customs.Business.TransportTypeList.Codes.Road:
							result = new CodeDescriptionPairList();
							result.AddPair(MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle, MeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadVehicle);
							break;
						default:
							result = base.TransportTypeListCore;
							break;
					}
					return result;
				});
			}
		}

		protected override IList CustomsStatusListCore
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeaderLookups.CustomsStatusList", () =>
				{
					var applicationCodeList = new CodeDescriptionPairList();
					applicationCodeList.AddPair(AISEntryStatusList.Codes.Accepted, AISEntryStatusList.Descriptions.Accepted);
					applicationCodeList.AddPair(AISEntryStatusList.Codes.Rejected, AISEntryStatusList.Descriptions.Rejected);
					applicationCodeList.AddPair(AISEntryStatusList.Codes.Invalid, TempStorageInvalidEntryStatusDescription);
					applicationCodeList.AddPair(AISEntryStatusList.Codes.Registered, AISEntryStatusList.Descriptions.Registered);
					applicationCodeList.AddPair(AISEntryStatusList.Codes.AmendmentRequested, AISEntryStatusList.Descriptions.AmendmentRequested);
					return applicationCodeList;
				});
			}
		}

		string TempStorageInvalidEntryStatusDescription => Res.GetString("C0C52E52-6D45-46D1-9AFB-C73DBFFC6872", "TSD Invalidated");
	}
}
