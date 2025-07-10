using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.DE.NCTS.Business.NctsDeclarationTypeList.Codes;
using static Enterprise.Customs.EU.NCTS.Business.NctsTypeOfSecurityList.Codes;
using CusGoodsLocationQualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;

namespace Enterprise.Customs.DE.NCTS.Business
{
	sealed class DEPDATHeaderProvider : NCTSHeaderProvider, IDEPDATHeader
	{
		public DEPDATHeaderProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
			movementHeader = nctsHeader.MovementHeader;
		}

		public string LRN => movementHeader.BM_PaperlessInbondNum.ValueOrNullIfEmpty();

		public string DeclarationType => movementHeader.BM_InBondEntryType;

		public string TransitDeclarationType => CachedValueHelper.GetValue(ref transitDeclarationType, () =>
			!movementHeader.IsSimplifiedNctsProcedure
			? "00"
			: nctsHeader.MovementHeader.CusAuthorizationUsages.Any(e => e.AGC_Code == "SSE")
				? "11"
				: "10");
		CachedValue<string> transitDeclarationType;

		public string TIRCarnetNumber =>
			movementHeader.BM_InBondEntryType == TIR ? nctsHeader.MovementHeader.TirCarnetNumber.ValueOrNullIfEmpty()
				: null;

		public DateTime? LimitDate => CachedValueHelper.GetValue(ref limitDate, () =>
			movementHeader.IsSimplifiedNctsProcedure
			? movementHeader.BM_ExportDate.ToNullableDateTime()
			: null);
		CachedValue<DateTime?> limitDate;

		public string Security => NCTSProviderHelpers.GetSecurity(movementHeader.BM_TypeOfSecurity);

		public string ReducesDataSet => movementHeader.BM_ReducedDatasetIndicator ? "1" : "0";

		public string SpecificCircumstance => movementHeader.BM_SpecificCircumstance;

		public string BindingItinerary => CachedValueHelper.GetValue(ref bidingItinerary, () =>
			movementHeader.BM_TypeOfSecurity == NON && nctsHeader.CountriesOfRouting.Cast<CountryOfRouting>().Any(e => !e.CY_Data.IsEmpty) ? "1" : "0");
		CachedValue<string> bidingItinerary;

		public IReadOnlyCollection<INCTSAuthorisation> Authorisations =>
			authorisations ?? (authorisations = nctsHeader.MovementHeader.CusAuthorizationUsages
				.Select(NCTSAuthorisationProvider.NewOrNull)
				.ToArray());
		IReadOnlyCollection<INCTSAuthorisation> authorisations;

		public string DepartureOffice => CachedValueHelper.GetValue(ref depatureOffice, () =>
			movementHeader.CustomsOffices
				.SingleOrDefault<NctsEuOfficeCode>(e => e.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture)?.CY_Data);
		CachedValue<string> depatureOffice;

		public string DestinationOffice => CachedValueHelper.GetValue(ref destinationOffice, () =>
			movementHeader.CustomsOffices
				.SingleOrDefault<NctsEuOfficeCode>(e => e.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination)?.CY_Data);
		CachedValue<string> destinationOffice;

		public IReadOnlyCollection<INCTSTransitOffice> TransitOffices =>
			transitOffices ?? (transitOffices = movementHeader.BM_InBondEntryType != TIR
				? movementHeader.TransitCustomsOfficeCodeList.Cast<NctsEuOfficeCode>()
					.Select<NctsEuOfficeCode, INCTSTransitOffice>(NCTSTransitOfficeProvider.NewOrNull)
					.ToArray()
				: Array.Empty<INCTSTransitOffice>());
		IReadOnlyCollection<INCTSTransitOffice> transitOffices;

		public IReadOnlyCollection<string> ExitOffices =>
			exitOffices ?? (exitOffices = movementHeader.BM_TypeOfSecurity.ValueOrNullIfEmpty().In(BTH, EXI)
				? movementHeader.CustomsOffices.Where(e => e.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit)
					.Select(e => e.CY_Data.ValueOrNullIfEmpty())
					.ToArray()
				: Array.Empty<string>());
		IReadOnlyCollection<string> exitOffices;

		public INCTSPartyIDAddressContact HolderOfTransitProcedure => CachedValueHelper.GetValue(ref holderOfTransitProcedure, GetHolderOfTransitProcedureProvider);

		INCTSPartyIDAddressContact GetHolderOfTransitProcedureProvider()
		{
			var contact = nctsHeader.Principal.E2_Contact;

			if (!contact.IsEmpty)
			{
				return NCTSPartyIDAddressContactProvider.NewOrNull(nctsHeader.Principal, fallback: false);
			}
			else
			{
				return NCTSPartyIDAddressContactProvider.NewOrNull(nctsHeader.Principal, GlbStaff.CurrentUser, MovementHeaderRepresentative == null);
			}
		}

		CachedValue<INCTSPartyIDAddressContact> holderOfTransitProcedure;

		public string HolderOfTransitProcedureTIRIdentification => CachedValueHelper.GetValue(ref holderOfTransitProcedureTIRIdentification, () =>
			movementHeader.BM_InBondEntryType == TIR
				? nctsHeader.Principal.Organisation.GetCustomsRegNo(TIR).ValueOrNullIfEmpty()
				: null);
		CachedValue<string> holderOfTransitProcedureTIRIdentification;

		public INCTSPartyIDContact Representative => CachedValueHelper.GetValue(ref representative, () => NCTSPartyIDContactProvider.NewOrNull(MovementHeaderRepresentative, GlbStaff.CurrentUser));
		CachedValue<INCTSPartyIDContact> representative;

		JobDocAddress MovementHeaderRepresentative => CachedValueHelper.GetValue(ref movementHeaderRepresentative, () => movementHeader.DocAddresses.Cast<JobDocAddress>()
			.FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.Representative && x.Organisation.HasEoriOrTcu()));
		CachedValue<JobDocAddress> movementHeaderRepresentative;

		public IReadOnlyCollection<INCTSGuarantee> Guarantees => guarantees ??= GetGuarantees();
		IReadOnlyCollection<INCTSGuarantee> guarantees;

		IReadOnlyCollection<INCTSGuarantee> GetGuarantees()
		{
			var result = new List<INCTSGuarantee>();
			var guarantees = nctsHeader.MovementHeader.Guarantees;
			result.AddRange(guarantees.Where(x => bondTypesMultipleReferences.Contains(x.PW_BondType))
				.GroupBy(x => x.PW_BondType)
				.Select(g => new NCTSGuaranteeProvider(g.Key, g.ToArray())));

			result.AddRange(guarantees.Where(x => x.PW_BondType == NctsGuaranteeTypeList.Codes._3).Select(NCTSGuaranteeProvider.NewOrNull));

			result.AddRange(guarantees.Where(x => bondTypesNoReferences.Contains(x.PW_BondType))
				.GroupBy(x => x.PW_BondType)
				.Select(g => NCTSGuaranteeProvider.NewOrNull(g.First())));

			return result;
		}

		public string CountryOfDispatch => CachedValueHelper.GetValue(ref countryOfDispatch, () => AllLines.SameOrDefault(i => i.EffectiveCountryOfDispatch));
		CachedValue<string> countryOfDispatch;

		public string CountryOfDestination => CachedValueHelper.GetValue(ref countryOfDestination, () => AllLines.SameOrDefault(i => i.EffectiveCountryOfDestination));
		CachedValue<string> countryOfDestination;

		public bool ContainerIndicator => nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Any(x => x.BC_Mode == Core.Constants.ContainerModes.Containerised);

		public string InlandModeOfTransport => movementHeader.BM_InlandTransportMode.ValueOrNullIfEmpty();

		public string ModeOfTransportAtBorder => movementHeader.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.NON
			|| movementHeader.HasTransitOffice() ? (string)movementHeader.BM_ExportTransportMode : null;

		public decimal GrossMass => movementHeader.BM_GrossWeight.Round(3).Normalize();

		public string ReferenceNumberUCR => CachedValueHelper.GetValue(ref referenceNumberUCR, () => AllLines.SameOrDefault(i => i.EffectiveReferenceNumberUCR));
		CachedValue<string> referenceNumberUCR;

		public INCTSPartyIDContact Carrier => CachedValueHelper.GetValue(ref carrier, () => NCTSPartyIDContactProvider.NewOrNull(movementHeader.DocAddresses.Cast<JobDocAddress>()
			.FirstOrDefault(x => x.E2_AddressType == DocAddressTypes.Codes.Carrier && x.Organisation.HasEoriOrTcu())));
		CachedValue<INCTSPartyIDContact> carrier;

		public INCTSPartyIDAddressContact Consignor => CachedValueHelper.GetValue(ref consignor,
				() => nctsHeader.Bills.Cast<NctsBill>().SameOrDefault(x => NCTSPartyIDAddressContactProvider.NewOrNull(x.EffectiveConsignor, fallback: true)));
		CachedValue<INCTSPartyIDAddressContact> consignor;

		public INCTSPartyIDAddressContact Consignee => CachedValueHelper.GetValue(ref consignee,
			() => ShouldPopulateConsignee ? AllLines.SameOrDefault(i => NCTSPartyIDAddressContactProvider.NewOrNull(i.EffectiveConsignee, fallback: true)) : null);
		CachedValue<INCTSPartyIDAddressContact> consignee;

		public bool ShouldPopulateConsignee => CachedValueHelper.GetValue(ref shouldPopulateConsignee, () => !movementHeader.IsCombinedWithExit || !movementHeader.Has30600AdditionalInformation);

		public bool IsSimplifiedProcedure => movementHeader.IsSimplifiedNctsProcedure;

		CachedValue<bool> shouldPopulateConsignee;

		public IReadOnlyCollection<INCTSActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= movementHeader.CusSupplyChainActors.Select(x => NCTSActorProvider.NewOrNull(x)).ToArray();
		IReadOnlyCollection<INCTSActor> additionalSupplyChainActors;

		public IReadOnlyCollection<CargoWise.Customs.DE.MessageContracts.NCTS.INCTSTransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = nctsHeader.DepartureHeaderContainers
				.Where(x => movementHeader.IsSimplifiedNctsProcedure || x.BC_Mode == Core.Constants.ContainerModes.Containerised)
				.Select(x => NCTSTransportEquipmentProvider.NewOrNull(x, TransitDeclarationType)).ToArray());
		IReadOnlyCollection<CargoWise.Customs.DE.MessageContracts.NCTS.INCTSTransportEquipment> transportEquipments;

		public string LocationOfGoodsType => movementHeader.IsSimplifiedNctsProcedure ? "B" : "A";

		public string LocationOfGoodsQualifierIfIdentification => movementHeader.IsSimplifiedNctsProcedure ? CusGoodsLocationQualifierList.Codes.AuthorizationNumber : CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

		public string LocationOfGoodsAdditionalIdentifier => movementHeader.GoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber ? movementHeader.GoodsLocation.CGL_AdditionalIdentifier.ValueOrNullIfEmpty() : null;

		public INCTSContactPerson LocationOfGoodsContact => movementHeader.GoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
			? (locationOfGoodsContact ?? (locationOfGoodsContact = NCTSContactPersonProvider.NewOrNull(movementHeader.GoodsLocation.Address))) : null;
		INCTSContactPerson locationOfGoodsContact;

		public IReadOnlyCollection<string> CountriesOfRoutingOfConsignment => countriesOfRoutingOfConsignment
			?? (countriesOfRoutingOfConsignment = nctsHeader.CountriesOfRouting.Cast<CountryOfRouting>().OrderBy(x => x.CY_Order).Select(x => (string)x.CY_Data).ToArray());
		IReadOnlyCollection<string> countriesOfRoutingOfConsignment;

		public IReadOnlyCollection<INCTSTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
		IReadOnlyCollection<INCTSTransportMeans> departureTransportMeans;

		IReadOnlyCollection<INCTSTransportMeans> GetDepartureTransportMeans()
		{
			var result = new List<INCTSTransportMeans>();

			var inlandTransportMode = movementHeader.BM_InlandTransportMode;
			var transportAtDepartureType = movementHeader.BM_TransportAtDepartureType;
			var transportAtDeparture = movementHeader.BM_TransportAtDeparture;

			if (!transportAtDepartureType.IsEmpty)
			{
				switch (inlandTransportMode)
				{
					case EU.Business.ModeOfTransportList.Codes._1_SeaTransport:
					case EU.Business.ModeOfTransportList.Codes._4_AirTransport:
					case EU.Business.ModeOfTransportList.Codes._8_InlandWaterwayTransport:
					case EU.Business.ModeOfTransportList.Codes._9_OwnPropulsion:
						if (!transportAtDeparture.IsEmpty)
						{
							result.Add(new NCTSTransportMeansProvider(transportAtDepartureType, transportAtDeparture, movementHeader.BM_RN_NKTransportAtDepartureCountry));
						}
						break;

					case EU.Business.ModeOfTransportList.Codes._2_RailTransport:
						AddIfNeeded(result, transportAtDepartureType, transportAtDeparture, movementHeader.BM_RN_NKTransportAtDepartureCountry);
						AddIfNeeded(result, NctsTransportTypeOfIdList.Codes._20, movementHeader.BM_TransportAtDepartureTrailer1RegNo, movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality);

						movementHeader.InlandTransportList.Cast<EU.NCTS.Business.InlandTransport>().ForEach(
							x => result.Add(new NCTSTransportMeansProvider(NctsTransportTypeOfIdList.Codes._20, x.CY_Data, x.CY_Code)));
						break;

					case EU.Business.ModeOfTransportList.Codes._3_RoadTransport:
						AddIfNeeded(result, transportAtDepartureType, transportAtDeparture, movementHeader.BM_RN_NKTransportAtDepartureCountry);
						AddIfNeeded(result, NctsTransportTypeOfIdList.Codes._31, movementHeader.BM_TransportAtDepartureTrailer1RegNo, movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality);
						AddIfNeeded(result, NctsTransportTypeOfIdList.Codes._31, movementHeader.BM_TransportAtDepartureTrailer2RegNo, movementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality);
						break;
				}
			}

			return result.ToArray();
		}

		static void AddIfNeeded(List<INCTSTransportMeans> result, ZString typeOfIdentification, ZString identificationNumber, ZString nationality)
		{
			if (!identificationNumber.IsEmpty)
			{
				result.Add(new NCTSTransportMeansProvider(typeOfIdentification, identificationNumber, nationality));
			}
		}

		public IReadOnlyCollection<INCTSBorderTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = GetActiveBorderTransportMeans());
		IReadOnlyCollection<INCTSBorderTransportMeans> activeBorderTransportMeans;

		IReadOnlyCollection<INCTSBorderTransportMeans> GetActiveBorderTransportMeans()
		{
			var result = new List<INCTSBorderTransportMeans>();

			if (!movementHeader.BM_TOLCarrierID.IsEmpty || !movementHeader.BM_AircraftIDAtBorder.IsEmpty)
			{
				result.Add(NCTSBorderTransportMeansProvider.NewOrNull(movementHeader));
			}
			movementHeader.AdditionalTransportAtBorderList.Cast<DepartureCusTransportMeans>().ForEach(x => result.Add(NCTSBorderTransportMeansProvider.NewOrNull(x)));

			return result.ToArray();
		}

		public string PlaceOfLoadingCountry => movementHeader.BM_PortOfPresentationCode.LeftOrNull(2);
		public string PlaceOfLoadingLocation => movementHeader.BM_PlaceOfLoading.ValueOrNullIfEmpty();
		public string PlaceOfUnloadingCountry => movementHeader.BM_ForeignDestPortKCode.LeftOrNull(2);
		public string PlaceOfUnloadingLocation => movementHeader.BM_PlaceOfUnloading.ValueOrNullIfEmpty();

		public IReadOnlyCollection<INCTSDocument> PreviousDocuments =>
			previousDocuments ??
			(previousDocuments = nctsHeader.PreviousDocuments.Select(NCTSDocumentProvider.NewOrNull)
				.ToArray());
		IReadOnlyCollection<INCTSDocument> previousDocuments;

		public IReadOnlyCollection<INCTSDocument> SupportingDocuments =>
			supportingDocuments ??
			(supportingDocuments = nctsHeader.MovementHeader.SupportingDocuments.Select(NCTSDocumentProvider.NewOrNull)
				.ToArray());
		IReadOnlyCollection<INCTSDocument> supportingDocuments;

		public IReadOnlyCollection<INCTSDocument> TransportDocuments =>
			transportDocuments ??
			(transportDocuments = nctsHeader.AdditionalDocuments
				.Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.TransportDocuments)
				.Select(NCTSDocumentProvider.NewOrNull)
				.ToArray());
		IReadOnlyCollection<INCTSDocument> transportDocuments;

		public IReadOnlyCollection<INCTSDocument> AdditionalReferences =>
			additionalReferences ??
			(additionalReferences = nctsHeader.AdditionalDocuments
				.Where(x => x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalReference)
				.Select(NCTSDocumentProvider.NewOrNull)
				.ToArray());
		IReadOnlyCollection<INCTSDocument> additionalReferences;

		public IReadOnlyCollection<INCTSAdditionalInformation> AdditionalInformation =>
			additionalInformation ??
			(additionalInformation = nctsHeader.AdditionalDocuments.Where(x =>
					x.CSI_SubType == AdditionalDocTypeList.Codes.AdditionalInformation)
				.Select(NCTSAdditionalInformationProvider.NewOrNull)
				.ToArray());
		IReadOnlyCollection<INCTSAdditionalInformation> additionalInformation;

		public IReadOnlyCollection<IDEPDATHouseConsignment> HouseConsignments => houseConsignments ?? (houseConsignments = nctsHeader.Bills.Select(x => DEPDATHouseConsignmentProvider.NewOrNull(x, this)).ToArray());
		IReadOnlyCollection<IDEPDATHouseConsignment> houseConsignments;

		public string MethodOfPayment => CachedValueHelper.GetValue(ref methodOfPayment, () => AllLines.SameOrDefault(g => g.EffectiveMethodOfPayment));
		CachedValue<string> methodOfPayment;

		readonly NctsDepartureMovementHeader movementHeader;

		readonly static ImmutableHashSet<string> bondTypesMultipleReferences = ImmutableHashSet.Create(
			NctsGuaranteeTypeList.Codes._0,
			NctsGuaranteeTypeList.Codes._1,
			NctsGuaranteeTypeList.Codes._2,
			NctsGuaranteeTypeList.Codes._4);

		readonly static ImmutableHashSet<string> bondTypesNoReferences = ImmutableHashSet.Create(
			NctsGuaranteeTypeList.Codes._8,
			NctsGuaranteeTypeList.Codes.B,
			NctsGuaranteeTypeList.Codes.R);

		IEnumerable<NctsDepartureCargoDesc> AllLines => allLines ?? (allLines = nctsHeader.Bills.Cast<NctsBill>().SelectMany(x => x.GoodsItems.Cast<NctsDepartureCargoDesc>()));
		IEnumerable<NctsDepartureCargoDesc> allLines;
	}
}
