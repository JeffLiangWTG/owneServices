using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.NCTS.Business.NCTS.MessageSending.AidaXml;
using Argument = CargoWise.Common.Argument;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;
using EUAdditionalInfo = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo;
using EuCommonPreviousDocument = Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument;
using IAdditionalInformation = CargoWise.Customs.IT.MessageContracts.IAdditionalInformation;
using IGuarantee = CargoWise.Customs.IT.MessageContracts.NCTS.Departure.IGuarantee;
using ILocationOfGoods = CargoWise.Customs.IT.MessageContracts.NCTS.Departure.ILocationOfGoods;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class D1ConsignmentWrapper : ID1Consignment
{
	public D1ConsignmentWrapper(NctsHeader header, IMessageSendingWrapperFactory messageSendingWrapperFactory, INctsAmendment amendment = null)
	{
		this.header = Argument.NotNull(header, nameof(header));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		movementHeader = Argument.NotNull(header.MovementHeader, nameof(header.MovementHeader));
		this.amendment = amendment;

		InitializeLazy(messageSendingWrapperFactory);
	}

	#region ID1Consignment

	string ID1Consignment.DeclarationCustomsOffice => lazyDeclarationCustomsOffice.Value;
	Lazy<string> lazyDeclarationCustomsOffice;

	string ID1Consignment.DeclarationType => lazyDeclarationType.Value;
	Lazy<string> lazyDeclarationType;

	INctsAmendment ID1Consignment.Amendment => amendment;

	string ID1Consignment.AdditionalDeclarationType => lazyAdditionalDeclarationType.Value;
	Lazy<string> lazyAdditionalDeclarationType;

	int ID1Consignment.TypeOfSecurity => lazyTypeOfSecurity.Value;
	Lazy<int> lazyTypeOfSecurity;

	bool ID1Consignment.ReducedDatasetIndicator => lazyReducedDatasetIndicator.Value;
	Lazy<bool> lazyReducedDatasetIndicator;

	string ID1Consignment.SpecificCircumstanceIndicator => lazySpecificCircumstanceIndicator.Value;
	Lazy<string> lazySpecificCircumstanceIndicator;

	DateTime? ID1Consignment.LimitDate => lazyLimitDate.Value;
	Lazy<DateTime?> lazyLimitDate;

	string ID1Consignment.TirCarnetNumber => lazyTirCarnetNumber.Value;
	Lazy<string> lazyTirCarnetNumber;

	string ID1Consignment.Lrn => IT.Business.ITEDIMessage.ITMessageNumberPlaceholder;

	IReadOnlyCollection<IAuthorization> ID1Consignment.Authorizations => lazyAuthorizations.Value;
	Lazy<IReadOnlyCollection<IAuthorization>> lazyAuthorizations;

	IRepresentative ID1Consignment.Representative => lazyRepresentative.Value;
	Lazy<IRepresentative> lazyRepresentative;

	IHolderOfTransitProcedure ID1Consignment.HolderOfTransitProcedure => lazyHolderOfTransitProcedure.Value;
	Lazy<IHolderOfTransitProcedure> lazyHolderOfTransitProcedure;

	DateTime? ID1Consignment.GoodsPresentationDateTime => lazyGoodsPresentationDateTime.Value;
	Lazy<DateTime?> lazyGoodsPresentationDateTime;

	bool ID1Consignment.IsBindingItinerary => lazyIsBindingItinerary.Value;
	Lazy<bool> lazyIsBindingItinerary;

	string ID1Consignment.DepartureCustomsOffice => lazyDepartureCustomsOffice.Value;
	Lazy<string> lazyDepartureCustomsOffice;

	IReadOnlyCollection<ICustomsOfficeOfTransit> ID1Consignment.CustomsOfficesOfTransit => lazyCustomsOfficesOfTransit.Value;
	Lazy<IReadOnlyCollection<ICustomsOfficeOfTransit>> lazyCustomsOfficesOfTransit;

	string ID1Consignment.DestinationCustomsOffice => lazyDestinationCustomsOffice.Value;
	Lazy<string> lazyDestinationCustomsOffice;

	IReadOnlyCollection<string> ID1Consignment.CustomsOfficesOfExitTransit => lazyCustomsOfficesOfExitTransit.Value;
	Lazy<IReadOnlyCollection<string>> lazyCustomsOfficesOfExitTransit;

	IReadOnlyCollection<IGuarantee> ID1Consignment.Guarantees => lazyGuarantees.Value;
	Lazy<IReadOnlyCollection<IGuarantee>> lazyGuarantees;

	IReadOnlyCollection<IPreviousDocument> ID1Consignment.PreviousDocuments => lazyPreviousDocuments.Value;
	Lazy<IReadOnlyCollection<IPreviousDocument>> lazyPreviousDocuments;

	IReadOnlyCollection<IAdditionalInformation> ID1Consignment.AdditionalInformation => lazyAdditionalInformation.Value;

	Lazy<IReadOnlyCollection<IAdditionalInformation>> lazyAdditionalInformation;

	IReadOnlyCollection<ISupportingDocument> ID1Consignment.SupportingDocuments => lazySupportingDocuments.Value;
	Lazy<IReadOnlyCollection<ISupportingDocument>> lazySupportingDocuments;

	IReadOnlyCollection<IAdditionalReference> ID1Consignment.AdditionalReferences => lazyAdditionalReferences.Value;
	Lazy<IReadOnlyCollection<IAdditionalReference>> lazyAdditionalReferences;

	IReadOnlyCollection<ITransportDocument> ID1Consignment.TransportDocuments => lazyTransportDocuments.Value;
	Lazy<IReadOnlyCollection<ITransportDocument>> lazyTransportDocuments;

	string ID1Consignment.Ucr => lazyUcr.Value;
	Lazy<string> lazyUcr;

	ICarrier ID1Consignment.Carrier => lazyCarrier.Value;
	Lazy<ICarrier> lazyCarrier;

	ITrader ID1Consignment.Consignor => lazyConsignor.Value;
	Lazy<ITrader> lazyConsignor;

	ITrader ID1Consignment.Consignee => lazyConsignee.Value;
	Lazy<ITrader> lazyConsignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> ID1Consignment.AdditionalSupplyChainActors => lazyAdditionalSupplyChainActors.Value;
	Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>> lazyAdditionalSupplyChainActors;

	string ID1Consignment.TransportChargesMethodOfPayment => lazyTransportChargesMethodOfPayment.Value;
	Lazy<string> lazyTransportChargesMethodOfPayment;

	string ID1Consignment.CountryOfDestination => lazyCountryOfDestination.Value;
	Lazy<string> lazyCountryOfDestination;

	ILocationOfGoods ID1Consignment.LocationOfGoods => lazyLocationOfGoods.Value;
	Lazy<ILocationOfGoods> lazyLocationOfGoods;

	string ID1Consignment.CountryOfDispatch => lazyCountryOfDispatch.Value;
	Lazy<string> lazyCountryOfDispatch;

	IReadOnlyCollection<IConsignmentCountryRouting> ID1Consignment.ConsignmentRoutings => lazyConsignmentRoutings.Value;
	Lazy<IReadOnlyCollection<IConsignmentCountryRouting>> lazyConsignmentRoutings;

	IGeoLocationDetails ID1Consignment.PlaceOfLoading => lazyPlaceOfLoading.Value;
	Lazy<IGeoLocationDetails> lazyPlaceOfLoading;

	IGeoLocationDetails ID1Consignment.PlaceOfUnloading => lazyPlaceOfUnloading.Value;
	Lazy<IGeoLocationDetails> lazyPlaceOfUnloading;

	decimal ID1Consignment.GrossMass => lazyGrossMass.Value;
	Lazy<decimal> lazyGrossMass;

	bool? ID1Consignment.IsContainerizedTransport => lazyIsContainerizedTransport.Value;
	Lazy<bool?> lazyIsContainerizedTransport;

	int? ID1Consignment.BorderMeansOfTransportMode => lazyBorderMeansOfTransportMode.Value;
	Lazy<int?> lazyBorderMeansOfTransportMode;

	int? ID1Consignment.InlandTransportMode => lazyInlandTransportMode.Value;
	Lazy<int?> lazyInlandTransportMode;

	IReadOnlyCollection<IMeansOfTransport> ID1Consignment.DepartureMeansOfTransports => lazyDepartureMeansOfTransports.Value;
	Lazy<IReadOnlyCollection<IMeansOfTransport>> lazyDepartureMeansOfTransports;

	IReadOnlyCollection<ITransportEquipment> ID1Consignment.TransportEquipment => lazyTransportEquipment.Value;
	Lazy<IReadOnlyCollection<ITransportEquipment>> lazyTransportEquipment;

	IReadOnlyCollection<IActiveBorderMeansOfTransport> ID1Consignment.ActiveBorderTransportMeans => lazyActiveBorderTransportMeans.Value;
	Lazy<IReadOnlyCollection<IActiveBorderMeansOfTransport>> lazyActiveBorderTransportMeans;

	#endregion

	#region Implementation

	void InitializeLazy(IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		INctsEntityWrapperProvider entityWrapperProvider = new NctsEntityWrapperProvider();
		nctsHeaderWrapper = new Lazy<INctsHeaderWrapper>(() => messageSendingWrapperFactory.GetNewNctsHeaderWrapper(header));
		moveHeaderWrapper = new Lazy<INctsDepartureMovementHeaderWrapper>(() => entityWrapperProvider.GetNctsDepartureMovementHeaderWrapper(movementHeader));

		lazyActiveBorderTransportMeans = new Lazy<IReadOnlyCollection<IActiveBorderMeansOfTransport>>(GetActiveBorderMeansOfTransport);
		lazyAdditionalDeclarationType = new Lazy<string>(() => movementHeader.BM_AdditionalDeclarationType);
		lazyAdditionalInformation = new Lazy<IReadOnlyCollection<IAdditionalInformation>>(GetAdditionalInformation);
		lazyAdditionalReferences = new Lazy<IReadOnlyCollection<IAdditionalReference>>(GetAdditionalReference);
		lazyAdditionalSupplyChainActors = new Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>>(GetAdditionalSupplyChainActors);
		lazyAuthorizations = new Lazy<IReadOnlyCollection<IAuthorization>>(GetAuthorizations);
		lazyBorderMeansOfTransportMode = new Lazy<int?>(GetBorderMeansOfTransportMode);
		lazyConsignee = new Lazy<ITrader>(NctsHeaderWrapper.GetConsignee);
		lazyConsignmentRoutings = new Lazy<IReadOnlyCollection<IConsignmentCountryRouting>>(GetConsignmentRoutings);
		lazyConsignor = new Lazy<ITrader>(GetConsignor);
		lazyCountryOfDestination = new Lazy<string>(GetCountryOfDestination);
		lazyCountryOfDispatch = new Lazy<string>(GetCountryOfDispatch);
		lazyCustomsOfficesOfExitTransit = new Lazy<IReadOnlyCollection<string>>(GetCustomsOfficesOfExitTransit);
		lazyCustomsOfficesOfTransit = new Lazy<IReadOnlyCollection<ICustomsOfficeOfTransit>>(GetCustomsOfficesOfTransit);
		lazyCarrier = new Lazy<ICarrier>(() => CarrierWrapper.NewOrNull(movementHeader.Carrier?.Organisation, header.Principal?.Organisation));
		lazyDeclarationCustomsOffice = new Lazy<string>(GetDeclarationCustomsOffice);
		lazyDeclarationType = new Lazy<string>(() => movementHeader.BM_InBondEntryType);
		lazyDepartureCustomsOffice = new Lazy<string>(GetDepartureCustomsOffice);
		lazyDestinationCustomsOffice = new Lazy<string>(GetDestinationCustomsOffice);
		lazyGoodsPresentationDateTime = new Lazy<DateTime?>(GetGoodsPresentationDateTime);
		lazyGrossMass = new Lazy<decimal>(() => movementHeader.BM_GrossWeight);
		lazyGuarantees = new Lazy<IReadOnlyCollection<IGuarantee>>(GetGuarantees);
		lazyHolderOfTransitProcedure = new Lazy<IHolderOfTransitProcedure>(GetHolderOfTransitProcedure);
		lazyInlandTransportMode = new Lazy<int?>(GetInlandTransportMode);
		lazyIsBindingItinerary = new Lazy<bool>(() => header.CountriesOfRouting.Any());
		lazyIsContainerizedTransport = new Lazy<bool?>(GetIsContainerizedTransport);
		lazyLimitDate = new Lazy<DateTime?>(() => MoveHeaderWrapper.GetLimitDate());
		lazyPlaceOfLoading = new Lazy<IGeoLocationDetails>(() => PlaceOfLoadingWrapper.NewOrNull(movementHeader));
		lazyPlaceOfUnloading = new Lazy<IGeoLocationDetails>(() => PlaceOfUnloadingWrapper.NewOrNull(movementHeader));
		lazyPreviousDocuments = new Lazy<IReadOnlyCollection<IPreviousDocument>>(GetPreviousDocument);
		lazyReducedDatasetIndicator = new Lazy<bool>(() => movementHeader.BM_ReducedDatasetIndicator);
		lazyRepresentative = new Lazy<IRepresentative>(GetRepresentative);
		lazySpecificCircumstanceIndicator = new Lazy<string>(GetSpecificCircumstanceIndicator);
		lazySupportingDocuments = new Lazy<IReadOnlyCollection<ISupportingDocument>>(GetSupportingDocuments);
		lazyTirCarnetNumber = new Lazy<string>(GetTirCarnetNumber);
		lazyTransportChargesMethodOfPayment = new Lazy<string>(GetTransportChargesMethodOfPayment);
		lazyTransportDocuments = new Lazy<IReadOnlyCollection<ITransportDocument>>(GetTransportDocuments);
		lazyTransportEquipment = new Lazy<IReadOnlyCollection<ITransportEquipment>>(GetTransportEquipment);
		lazyTypeOfSecurity = new Lazy<int>(GetTypeOfSecurity);
		lazyUcr = new Lazy<string>(GetUcr);
		lazyLocationOfGoods = new Lazy<ILocationOfGoods>(GetLocationOfGoods);
		lazyDepartureMeansOfTransports = new Lazy<IReadOnlyCollection<IMeansOfTransport>>(()
			=> !header.IsInPhase5TransitionPeriod
			? (SharedValueMapResolverProvider.GetDepartureTransportMeansMapResolver().GetValueForHeader(header) ?? [])
			: DepartureMeansOfTransportWrapper.CollectFromMovementHeader(movementHeader));
	}

	string GetCountryOfDestination()
	{
		if (movementHeader.Header.IsInPhase5TransitionPeriod)
		{
			return SharedValueMapResolverProvider.GetCountryOfDestinationMapResolver()
				.GetValueForHeader(movementHeader);
		}
		return SharedValueMapResolverProvider.GetCountryOfDestinationMapHeaderResolver()
			.GetValueForHeader(movementHeader);
	}

	ILocationOfGoods GetLocationOfGoods()
	{
		var goodsLocation = movementHeader.GoodsLocation;
		return goodsLocation.CGL_Qualifier.IsEmpty
			? null
			: new LocationOfGoodsWrapper(goodsLocation);
	}

	int GetTypeOfSecurity()
	{
		switch (movementHeader.BM_TypeOfSecurity)
		{
			case NctsTypeOfSecurityList.Codes.ENT:
				return 1;

			case NctsTypeOfSecurityList.Codes.EXI:
				return 2;

			case NctsTypeOfSecurityList.Codes.BTH:
				return 3;

			case NctsTypeOfSecurityList.Codes.NON:
			default:
				return 0;
		}
	}

	IReadOnlyCollection<ITransportEquipment> GetTransportEquipment()
	{
		return header
			.DepartureHeaderContainers
			.Cast<NctsDepartureHeaderContainer>()
			.Where(hc => hc.IsContainerised || hc.TotalSealCount > 0)
			.Select(hc => new TransportEquipmentWrapper(hc))
			.ToCollection();
	}

	IReadOnlyCollection<ITransportDocument> GetTransportDocuments()
	{
		return header
			.AdditionalDocuments
			.Where(ad => ad.IsATransportDocument)
			.Cast<EUAdditionalInfo>()
			.Select(ad => new TransportDocumentWrapper(ad))
			.ToCollection();
	}

	string GetTransportChargesMethodOfPayment()
	{
		if (movementHeader.Header.IsInPhase5TransitionPeriod)
		{
			var resolver = SharedValueMapResolverProvider.GetTransportChargesMethodOfPaymentMapResolver();
			return resolver.GetValueForHeader(movementHeader);
		}
		else
		{
			var resolver = SharedValueMapResolverProvider.GetTransportBillMethodOfPaymentMapResolver();
			return resolver.GetValueForHeader(movementHeader);
		}
	}

	string GetTirCarnetNumber()
	{
		return movementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR
			? movementHeader.TirCarnetNumber
			: null;
	}

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocuments()
	{
		return header
			.MovementHeader
			.SupportingDocuments
			.Cast<NctsSupportingDocument>()
			.Select(sd => new SupportingDocumentWrapper(sd))
			.ToCollection();
	}

	string GetSpecificCircumstanceIndicator() => movementHeader.BM_SpecificCircumstance;

	IRepresentative GetRepresentative()
	{
		var organisation = movementHeader.Representative?.Organisation;

		return organisation != null
			? new RepresentativeWrapper(organisation)
			: null;
	}

	IReadOnlyCollection<IPreviousDocument> GetPreviousDocument()
	{
		return header
			.PreviousDocuments
			.Cast<EuCommonPreviousDocument>()
			.Select(pd => new PreviousDocumentWrapper(pd))
			.ToCollection();
	}

	bool? GetIsContainerizedTransport()
	{
		return header.DepartureHeaderContainers
		.Cast<NctsDepartureHeaderContainer>()
		.Any(hc => hc.BC_Mode == ContainerModes.Containerised);
	}

	int? GetInlandTransportMode()
	{
		if (int.TryParse(movementHeader.BM_InlandTransportMode, out var inlandTransportMode))
		{
			return inlandTransportMode;
		}
		return null;
	}

	IHolderOfTransitProcedure GetHolderOfTransitProcedure()
	{
		var principal = header.Principal;

		return !principal.IsEmpty
			? new HolderOfTransitProcedureWrapper(principal, movementHeader.BM_InBondEntryType)
			: null;
	}

	IReadOnlyCollection<IGuarantee> GetGuarantees()
	{
		var guaranteeWrapperFactory = new GuaranteeWrapperFactory();

		return header.MovementHeader.Guarantees
			.Select(guarantee => guaranteeWrapperFactory.GetNewGuaranteeWrapper(guarantee))
			.ToCollection();
	}

	DateTime? GetGoodsPresentationDateTime()
	{
		var presentationDateTime = movementHeader.BM_PresentationDateTime;
		if (presentationDateTime.IsEmpty || !presentationDateTime.IsValid)
		{
			return null;
		}
		return presentationDateTime.ToZDateTime().ToDateTime();
	}

	string GetDestinationCustomsOffice()
	{
		var customsOffices = header.IsPhase5 ? header.CommonMovementHeader.CustomsOffices : header.CustomsOffices;
		var officeOfDestination = customsOffices
			.Cast<NctsEuOfficeCode>()
			.FirstOrDefault(customsOffice => customsOffice.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
		return officeOfDestination == null ? null : officeOfDestination.CY_Data;
	}

	string GetDepartureCustomsOffice()
	{
		var officeOfDeparture = GetOfficeOfDeparture();
		return officeOfDeparture == null ? null : officeOfDeparture.CY_Data;
	}

	string GetDeclarationCustomsOffice()
	{
		var officeOfDeparture = GetOfficeOfDeparture();

		return officeOfDeparture == null ? null : officeOfDeparture.CY_Data.SubstringSafe(2);
	}

	NctsEuOfficeCode GetOfficeOfDeparture()
	{
		var customsOffices = header.IsPhase5 ? header.CommonMovementHeader.CustomsOffices : header.CustomsOffices;
		return customsOffices
			.Cast<NctsEuOfficeCode>()
			.FirstOrDefault(customsOffice => customsOffice.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
	}

	IReadOnlyCollection<ICustomsOfficeOfTransit> GetCustomsOfficesOfTransit()
	{
		var customsOffices = header.IsPhase5 ? header.CommonMovementHeader.CustomsOffices : header.CustomsOffices;
		return customsOffices
			.Where(customsOffice => customsOffice.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfTransit)
			.Select(customOfficesOfTransit => new CustomsOfficeOfTransitWrapper(customOfficesOfTransit))
			.ToCollection();
	}

	string GetCountryOfDispatch()
	{
		return SharedValueMapResolverProvider
			.GetCountryOfDispatchMapHeaderResolver()
			.GetValueForHeader(movementHeader);
	}

	IReadOnlyCollection<string> GetCustomsOfficesOfExitTransit()
	{
		var customsOffices = header.IsPhase5 ? header.CommonMovementHeader.CustomsOffices : header.CustomsOffices;
		return customsOffices
			.Where(customsOffice => customsOffice.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit)
			.Select(officesOfExitTransit => (string)officesOfExitTransit.CY_Data)
			.ToCollection();
	}

	ITrader GetConsignor()
	{
		var jobDocAddress = header.Consignor;

		return !jobDocAddress.IsEmpty
			? new EoriOrTcuTraderWrapper(jobDocAddress)
			: null;
	}

	IReadOnlyCollection<IConsignmentCountryRouting> GetConsignmentRoutings()
	{
		return header.CountriesOfRouting
			.Where(countryOfRouting => !countryOfRouting.CY_Data.IsEmpty)
			.Select(countryOfRouting => new ConsignmentCountryRoutingWrapper(countryOfRouting))
			.ToCollection();
	}

	int? GetBorderMeansOfTransportMode()
	{
		var exportTransportMode = movementHeader.BM_ExportTransportMode;
		if (int.TryParse(exportTransportMode, out var result))
		{
			return result;
		}
		return null;
	}

	IReadOnlyCollection<IAuthorization> GetAuthorizations()
	{
		return header.MovementHeader.CusAuthorizationUsages
			.Select(authorizationUsages => new CustomsCodeAuthorizationWrapper(authorizationUsages))
			.ToCollection();
	}

	IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors()
	{
		return header.MovementHeader.CusSupplyChainActors
			.Where(supplyChainActor => supplyChainActor.CFR_Type == CusReferenceTypeList.Codes.SupplyChainActor)
			.Select(supplyChainActor => new AdditionalSupplyChainActorWrapper(supplyChainActor))
			.ToCollection();
	}

	IReadOnlyCollection<IAdditionalReference> GetAdditionalReference()
	{
		return header.AdditionalDocuments
			.Where(additionalDocument => additionalDocument.IsAnAdditionalReference)
			.Select(additionalDocument => new AdditionalReferenceWrapper(additionalDocument))
			.ToCollection();
	}

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation()
	{
		return header.AdditionalDocuments
			.Where(additionalDocument => additionalDocument.IsAnAdditionalInformation)
			.Select(additionalDocument => new AdditionalInformationWrapper(additionalDocument))
			.ToCollection();
	}

	IReadOnlyCollection<IActiveBorderMeansOfTransport> GetActiveBorderMeansOfTransport()
	{
		return GetActiveBorderMeansOfTransportList()
			.WhereNotNull()
			.ToCollection();
	}

	IEnumerable<IActiveBorderMeansOfTransport> GetActiveBorderMeansOfTransportList()
	{
		yield return ActiveBorderMeansOfTransportWrapper.NewOrNull(movementHeader);

		foreach (var departureCusTransportMeans in movementHeader.AdditionalTransportAtBorderList.Cast<DepartureCusTransportMeans>())
		{
			yield return ActiveBorderMeansOfTransportWrapper.NewOrNull(departureCusTransportMeans);
		}
	}

	string GetUcr()
	{
		var header = movementHeader.Header;
		if (header.IsInPhase5TransitionPeriod)
		{
			return movementHeader.BM_UniqueConsignmentReference;
		}
		return header.ResolveUcr();
	}

	#endregion

	readonly NctsHeader header;
	readonly NctsDepartureMovementHeader movementHeader;
	readonly INctsAmendment amendment;

	INctsHeaderWrapper NctsHeaderWrapper => nctsHeaderWrapper.Value;
	Lazy<INctsHeaderWrapper> nctsHeaderWrapper;

	INctsDepartureMovementHeaderWrapper MoveHeaderWrapper => moveHeaderWrapper.Value;
	Lazy<INctsDepartureMovementHeaderWrapper> moveHeaderWrapper;
}
