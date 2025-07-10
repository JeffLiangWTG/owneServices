using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using FuncsHelper = Enterprise.Customs.CH.Business.FuncsHelper;

namespace Enterprise.Customs.CH.NCTS.Business;

public class ConsignmentDataProvider : IConsignment
{
	public static ConsignmentDataProvider New(NctsHeader nctsHeader) => nctsHeader?.MovementHeader == null ? null : new ConsignmentDataProvider(nctsHeader);

	ConsignmentDataProvider(NctsHeader nctsHeader)
	{
		this.nctsHeader = nctsHeader;
		movementHeader = nctsHeader.MovementHeader;
	}
	readonly NctsHeader nctsHeader;
	readonly NctsDepartureMovementHeader movementHeader;

	public bool ContainerIndicator => containerIndicator ??= GetContainerIndicator();
	bool? containerIndicator;

	bool GetContainerIndicator() => nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Any(c => c.BC_Mode == Constants.ContainerModes.Containerised)
			|| movementHeader.RelatedExportEntryHeaders.Cast<RelatedExportEntryHeaderGenPivot>().Select(x => x.EntryHeader.Declaration).Any(x => x.IsExportOrExportDeclarationActivation && JobDeclarationHelper.IsContainerised(x.JE_ContainerMode));

	public string CountryOfDestination => (countryOfDestination ??= FuncsHelper.IsCHNT015V4Active ? GetCountryOfDestinationV4() : GetCountryOfDestinationV5()).ReturnNullIfEmpty();
	ZString? countryOfDestination;

	ZString GetCountryOfDestinationV4() => NctsDataProviderHelper.IsSendDestinationCountryAtConsignmentLevelV4(nctsHeader) ? (nctsHeader.Bills.FirstOrDefault()?.GoodsItems.FirstOrDefault())?.EffectiveCountryCodeOfDestination ?? ZString.Empty : ZString.Empty;

	ZString GetCountryOfDestinationV5()
	{
		var country = ZString.Empty;
		if (!nctsHeader.IsLinkedOrRelatedExport)
		{
			country = nctsHeader.Bills.SelectMany(x => x.GoodsItems).SameOrDefault(x => x.EffectiveCountryCodeOfDestination);
		}
		return country;
	}

	public string CountryOfDispatch => (countryOfDispatch ??= FuncsHelper.IsCHNT015V4Active ? GetCountryOfDispatchV4() : GetCountryOfDispatchV5()).ReturnNullIfEmpty();
	ZString? countryOfDispatch;

	ZString GetCountryOfDispatchV4() => movementHeader.BM_RN_NKCountryOfDispatch.ReturnNullIfEmpty();

	ZString GetCountryOfDispatchV5()
	{
		ZString country;
		if (nctsHeader.IsLinkedOrRelatedExport)
		{
			country = nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch;
		}
		else
		{
			country = nctsHeader.Bills.SelectMany(x => x.GoodsItems).SameOrDefault(x => x.EffectiveCountryCodeOfDispatch);
		}
		return country;
	}

	public string ReferenceNumberUCR => movementHeader.BM_UniqueConsignmentReference.ReturnNullIfEmpty();

	public IConsignor Consignor => consignor ?? (consignor = ConsignorDataProvider.New(nctsHeader.Consignor, OrgConstants.ContactAllocationType.CUS));
	IConsignor consignor;

	public IConsignee Consignee => consignee ?? (consignee = ConsigneeDataProvider.New(nctsHeader.Consignee));
	IConsignee consignee;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = TransportEquipmentDataProvider.NewCollection(nctsHeader.DepartureHeaderContainers).ToArray());
	IReadOnlyCollection<ITransportEquipment> transportEquipments;

	public IReadOnlyCollection<IActiveBorderTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = ActiveBorderTransportMeansDataProvider.NewCollection(movementHeader)?.ToArray());
	IReadOnlyCollection<IActiveBorderTransportMeans> activeBorderTransportMeans;

	public IReadOnlyCollection<IHouseConsignment> HouseConsignments => houseConsignments ?? (houseConsignments = HouseConsignmentDataProvider.NewCollection(nctsHeader.Bills).ToArray());
	IReadOnlyCollection<IHouseConsignment> houseConsignments;

	public IPlaceOfLoadingOrUnloading PlaceOfLoading => placeOfLoading ?? (placeOfLoading = PlaceOfLoadingDataProvider.New(movementHeader));
	IPlaceOfLoadingOrUnloading placeOfLoading;

	public IPlaceOfLoadingOrUnloading PlaceOfUnloading => placeOfUnloading ??= movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON ? null : PlaceOfUnloadingDataProvider.New(movementHeader);
	IPlaceOfLoadingOrUnloading placeOfUnloading;

	public IReadOnlyCollection<ICountryOfRoutingOfConsignment> CountryOfRoutingOfConsignments => countryOfRoutingOfConsignments ?? (countryOfRoutingOfConsignments = CountryOfRoutingOfConsignmentProvider.NewCollection(nctsHeader.CountriesOfRouting).ToArray());
	IReadOnlyCollection<ICountryOfRoutingOfConsignment> countryOfRoutingOfConsignments;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = movementHeader.IsNationalTransitSwitzerland ? null : AdditionalSupplyChainActorDataProvider.NewCollection(movementHeader.CusSupplyChainActors).ToArray());
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

	public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = GetPreviousDocuments().ToArray());
	IReadOnlyCollection<IDocument> previousDocuments;

	IEnumerable<IDocument> GetPreviousDocuments()
	{
		var exportEntryDocuments = (ExportEntryDocumentDataProvider.NewCollection(movementHeader.RelatedExportEntryHeaders) ?? Enumerable.Empty<IDocument>()).ToArray();
		var sequenceNumberOffset = exportEntryDocuments.Length == 0 ? 0 : exportEntryDocuments.Last().SequenceNumber;
		var previousDocuments = DocumentDataProvider.NewCollection(nctsHeader.PreviousDocuments, sequenceNumberOffset) ?? Enumerable.Empty<IDocument>();
		return exportEntryDocuments.Concat(previousDocuments);
	}

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = DocumentDataProvider.NewCollection(nctsHeader.MovementHeader.SupportingDocuments).ToArray());
		IReadOnlyCollection<IDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ?? (transportDocuments = TransportDocumentDataProvider.NewCollection(nctsHeader.AdditionalDocuments).ToArray());
	IReadOnlyCollection<IDocument> transportDocuments;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= AdditionalInformationDataProvider.NewCollection(nctsHeader.AdditionalDocuments, AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ?? (additionalReferences = AdditionalReferenceDataProvider.NewCollection(nctsHeader.AdditionalDocuments).ToArray());
	IReadOnlyCollection<IAdditionalReference> additionalReferences;

	public bool? PartialDelivery => null;

	public bool Preference => false;

	public IExporter Exporter => null;

	public IReadOnlyCollection<ITransportCharges> TransportCharges => transportCharges ??= (moveHeaderTransportCharges ??= moveHeaderTransportCharges = TransportChargesDataProvider.New(movementHeader)) != null ? [moveHeaderTransportCharges] : [];
	ITransportCharges moveHeaderTransportCharges;
	ITransportCharges[] transportCharges;

	public IReadOnlyCollection<IConsignmentItem> ConsignmentItems => null;

	public IImporter Importer => null;

	public IPartialShipment PartialShipment => null;
}
