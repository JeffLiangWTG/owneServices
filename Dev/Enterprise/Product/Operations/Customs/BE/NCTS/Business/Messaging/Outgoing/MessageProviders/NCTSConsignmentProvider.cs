using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BE.NCTS.Business.Messaging.MessageProviders.NCTS.Outgoing
{
	public class NCTSConsignmentProvider : IConsignmentType20
	{
		protected readonly NctsHeader nctsHeader;
		protected readonly NctsDepartureMovementHeader depHeader;

		public NCTSConsignmentProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			depHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(depHeader));
		}

		public virtual string CountryOfDispatch => depHeader.BM_RN_NKCountryOfDispatch;

		public string CountryOfDestination => depHeader.BM_RL_NKDestinationPort;

		public bool ContainerIndicator => nctsHeader.DepartureHeaderContainers.Count > 0;

		public int? InlandModeOfTransport => NctsMessageProviderHelper.ConvertStringToNullableInt(depHeader.BM_InlandTransportMode);

		public int? ModeOfTransportAtTheBorder => NctsMessageProviderHelper.ConvertStringToNullableInt(depHeader.BM_ExportTransportMode);

		public decimal GrossMass => depHeader.BM_GrossWeight.Normalize();

		public virtual string ReferenceNumberUCR => depHeader.BM_UniqueConsignmentReference;

		public IParty Carrier => CarrierAddress != null ? CachedValueHelper.GetValue(ref carrier, GetCarrierCore) : null;
		CachedValue<IParty> carrier;

		protected virtual IParty GetCarrierCore() => CarrierAddress != null ? new CarrierProvider(CarrierAddress) : null;

		protected JobDocAddress CarrierAddress => carrierAddress ?? (carrierAddress = JobDocAddress.Load(depHeader, DocAddressType.Carrier));
		JobDocAddress carrierAddress;

		public IParty Consignee => ConsigneeAddress != null ? CachedValueHelper.GetValue(ref consignee, GetConsigneeCore) : null;
		CachedValue<IParty> consignee;

		protected virtual IParty GetConsigneeCore() => ConsigneeAddress != null ? new ConsigneeProvider(ConsigneeAddress) : null;

		protected JobDocAddress ConsigneeAddress => consigneeAddress ?? (consigneeAddress = JobDocAddress.Load(nctsHeader, DocAddressType.ConsigneeAddress));
		JobDocAddress consigneeAddress;

		public IParty Consignor => ConsignorAddress != null ? CachedValueHelper.GetValue(ref consignor, GetConsignorCore) : null;
		CachedValue<IParty> consignor;

		protected virtual IParty GetConsignorCore() => ConsignorAddress != null ? new PartyProvider(ConsignorAddress) : null;

		protected JobDocAddress ConsignorAddress => consignorAddress ?? (consignorAddress = JobDocAddress.Load(nctsHeader, DocAddressType.ConsignorDocumentaryAddress));
		JobDocAddress consignorAddress;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = GetAdditionalSupplyChainActorsCore());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		protected virtual IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActorsCore() => NctsDataRetrieveMethods.GetCusReferences(nctsHeader.Factory, nctsHeader.PK).Select((cr, index) => new AdditionalSupplyChainActorProvider(cr, index + 1)).ToArray();

		public IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = GetTransportEquipmentsCore());
		IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> transportEquipments;

		protected virtual IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> GetTransportEquipmentsCore()
		{
			return nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Select((ctr, index) => new TransportEquipmentsForNCTSHeaderContainerProvider(ctr, index + 1)).ToArray<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment>();
		}

		public ILocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, GetLocationOfGoodsCore);
		CachedValue<ILocationOfGoods> locationOfGoods;

		protected virtual ILocationOfGoods GetLocationOfGoodsCore() => new LocationOfGoodsProvider(nctsHeader);

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeansCore());
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		protected virtual IReadOnlyCollection<IDepartureTransportMeans> GetDepartureTransportMeansCore() => GetDepartureTransportMeans().ToArray();

		public IReadOnlyCollection<ICountryOfRoutingOfConsignment> CountryOfRoutingOfConsignments => countryOfRoutingOfConsignments ?? (countryOfRoutingOfConsignments = GetCountryOfRoutingOfConsignmentsCore());
		ICountryOfRoutingOfConsignment[] countryOfRoutingOfConsignments;

		protected virtual ICountryOfRoutingOfConsignment[] GetCountryOfRoutingOfConsignmentsCore() => nctsHeader.CountriesOfRouting.Cast<CountryOfRouting>()
			.Select(it => new CountryOfRoutingOfConsignmentsProvider(it.CY_Data, it.CY_Order)).ToArray<ICountryOfRoutingOfConsignment>();

		public IReadOnlyCollection<IActiveBorderTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ??= new ActiveBorderTransportMeansCollectionProvider(depHeader);
		IReadOnlyCollection<IActiveBorderTransportMeans> activeBorderTransportMeans;

		public IPlace PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, GetPlaceOfLoadingCore);
		CachedValue<IPlace> placeOfLoading;

		protected virtual IPlace GetPlaceOfLoadingCore() => new PlaceOfLoadingProvider(depHeader);

		public IPlace PlaceOfUnloading => placeOfUnloading ?? (placeOfUnloading = new PlaceOfUnloadingProvider(depHeader));
		IPlace placeOfUnloading;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = GetPreviousDocumentsCore());
		IReadOnlyCollection<IPreviousDocument> previousDocuments;

		protected virtual IReadOnlyCollection<IPreviousDocument> GetPreviousDocumentsCore() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(nctsHeader.Factory, nctsHeader.PK, Constants.CusSupportingInfoTypes.PreviousDocument)
			.Select(csi => new PreviousDocumentProvider(csi)).ToArray<IPreviousDocument>();

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = GetSupportingDocumentsCore());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		protected virtual IReadOnlyCollection<ISupportingDocument> GetSupportingDocumentsCore() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(nctsHeader.Factory, depHeader.PK, Constants.CusSupportingInfoTypes.SupportingDocument)
			.Select(csi => new SupportingDocumentProvider(csi)).ToArray<ISupportingDocument>();

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ?? (transportDocuments = GetTransportDocumentsCore());
		IReadOnlyCollection<ITransportDocument> transportDocuments;

		protected virtual IReadOnlyCollection<ITransportDocument> GetTransportDocumentsCore() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(nctsHeader.Factory, nctsHeader.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.TransportDocument)
			.Select(csi => new TransportDocumentProvider(csi)).ToArray<ITransportDocument>();

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = GetAdditionalReferencesCore());
		IReadOnlyCollection<IDocument> additionalReferences;

		protected virtual IReadOnlyCollection<IDocument> GetAdditionalReferencesCore() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(nctsHeader.Factory, nctsHeader.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalReference)
			.Select(csi => new AdditionalReferenceProvider(csi)).ToArray<IDocument>();

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = GetAdditionalInformationCore());
		IReadOnlyCollection<IAdditionalInformation> additionalInformation;

		protected virtual IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformationCore() => NctsDataRetrieveMethods.GetCusSupportingInfo(nctsHeader.Factory, nctsHeader.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalInformation).Select(csi => new AdditionalInformationProvider(csi)).ToArray<IAdditionalInformation>();

		public IReadOnlyCollection<IHouseConsignmentType10> HouseConsignments => houseConsignments ?? (houseConsignments = nctsHeader.Bills.Cast<NctsBill>().Select((bill, index) => new NCTSHouseConsignmentProvider(bill, index + 1)).ToArray<IHouseConsignmentType10>());
		IHouseConsignmentType10[] houseConsignments;

		public virtual string TransportChargesMethodOfPayment => depHeader.BM_MethodOfPayment;

		IEnumerable<DepartureTransportMeansProvider> GetDepartureTransportMeans()
		{
			var sequence = 1;
			if (!depHeader.BM_TransportAtDeparture.IsEmpty)
			{
				yield return new DepartureTransportMeansTransportAtDepartureProvider(depHeader, sequence++);
			}
			if (!depHeader.BM_TransportAtDepartureTrailer1RegNo.IsEmpty)
			{
				yield return new DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider(depHeader, sequence++);
			}
			if (!depHeader.BM_TransportAtDepartureTrailer2RegNo.IsEmpty)
			{
				yield return new DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider(depHeader, sequence++);
			}
			if (!depHeader.BM_AircraftIDAtDeparture.IsEmpty)
			{
				yield return new DepartureTransportMeansAircraftIDAtDepartureProvider(depHeader, sequence);
			}
		}
	}
}
