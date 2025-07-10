using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using TransportMeans = Enterprise.Customs.Business.TransportMeansList;
using TransportTypeListCodes = Enterprise.Customs.Business.TransportTypeList.Codes;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public partial class JobDeclarationLookups : EU.Business.Declaration.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList PaymentPartyList
		{
			get
			{
				var representationType = Parent.JE_DeclarantType;

				return Factory.GetCachedValue("33D19691-1A58-4ED9-8241-489BDF699188|DutyPaymentPartyList|" + representationType, () =>
				{
					var result = new CodeDescriptionPairList();
					switch (representationType)
					{
						case RepresentationTypeList.Codes._1Self:
							result.AddPair(DeferralPaymentPartyList.Codes.Declarant, DeferralPaymentPartyList.Descriptions.Declarant);
							break;
						case RepresentationTypeList.Codes._3Indirect:
							result.AddPair(DeferralPaymentPartyList.Codes.Declarant, DeferralPaymentPartyList.Descriptions.Declarant);
							result.AddPair(DeferralPaymentPartyList.Codes.DefermentParty, DeferralPaymentPartyList.Descriptions.DefermentParty);
							result.AddPair(DeferralPaymentPartyList.Codes.RepresentedParty, DeferralPaymentPartyList.Descriptions.RepresentedParty);
							break;
						default:
							result.AddPair(DeferralPaymentPartyList.Codes.Declarant, DeferralPaymentPartyList.Descriptions.Declarant);
							result.AddPair(DeferralPaymentPartyList.Codes.DefermentParty, DeferralPaymentPartyList.Descriptions.DefermentParty);
							result.AddPair(DeferralPaymentPartyList.Codes.Representative, DeferralPaymentPartyList.Descriptions.Representative);
							break;
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList DefermentAccountNumberList => DeferralPaymentPartyList.GetDeferralAccountNumberList(Parent.JE_PaymentMethod, Parent);

		public OrganisationsFindBoxCollection BuyingAgentAddressList => new OrganisationsFindBoxCollection(Factory);

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public OrganisationsFindBoxCollection AcquirerCollection => new OrganisationsFindBoxCollection(Factory);

		public CodeDescriptionPairList StatisticStatusCodeList => Factory.GetCachedValue<StatisticStatusCodeList>();

		public override CodeDescriptionPairList IncoTermList => Parent.IsImport ? Factory.GetCachedValue<IncotermA1840CodeList>() : base.IncoTermList;

		public override ICodeDescriptionPairList DeclarantTypeList
		{
			get
			{
				var isINDNotAllowed = Parent.JE_MessageType == DEJobMessageTypeList.Codes.WarehouseAdjustment;
				return Factory.GetCachedValue(string.Join("_", "DEJobDeclarationLookups.DeclarantTypeList", isINDNotAllowed), () =>
				{
					var result = new RepresentationTypeList();
					if (isINDNotAllowed)
					{
						result.RemoveCode(RepresentationTypeList.Codes._3Indirect);
					}
					return result;
				});
			}
		}

		public override CodeDescriptionPairList TransportMeansList => Parent.IsExport
			? GetInlandTransportMeansListForExport(Parent.JE_TransportModeInland)
			: base.TransportMeansList;

		CodeDescriptionPairList GetInlandTransportMeansListForExport(ZString inlandTransportMode)
		{
			return Factory.GetCachedValue($"DE.JobDeclarationLookups.TransportMeansList|{inlandTransportMode}", () =>
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
						break;
					case TransportTypeListCodes.InlandWaterwayTransport:
						result.AddPair(TransportMeans.Codes.EuropeanVesselIdentificationNumberEniCode, TransportMeans.Descriptions.EuropeanVesselIdentificationNumberEniCode);
						result.AddPair(TransportMeans.Codes.NameOfTheInlandWaterwaysVessel, TransportMeans.Descriptions.NameOfTheInlandWaterwaysVessel);
						result.DefaultCode = TransportMeans.Codes.EuropeanVesselIdentificationNumberEniCode;
						break;
				}

				return result;
			});
		}

		public CodeDescriptionPairList VATClaimBackList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();
	}
}
