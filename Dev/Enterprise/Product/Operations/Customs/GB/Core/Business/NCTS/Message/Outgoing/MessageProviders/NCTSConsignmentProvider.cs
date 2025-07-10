using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Business.NCTS
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

		public string InlandModeOfTransport => GetModeStringOrNull(depHeader.BM_InlandTransportMode);

		public string ModeOfTransportAtTheBorder => GetModeStringOrNull(depHeader.BM_ExportTransportMode);

		string GetModeStringOrNull(string stringValue) => int.TryParse(stringValue, out var intValue) && intValue != 0 ? stringValue : null;

		public decimal GrossMass => WeightRounding.Round(IsInPhase5TransitionPeriod, depHeader.BM_GrossWeight.Normalize());

		public virtual string ReferenceNumberUCR => depHeader.BM_UniqueConsignmentReference;

		public IParty Carrier => CarrierAddress != null ? CachedValueHelper.GetValue(ref carrier, GetCarrier) : null;
		CachedValue<IParty> carrier;

		IParty GetCarrier() => CarrierAddress != null ? new CarrierProvider(CarrierAddress, IsInPhase5TransitionPeriod) : null;

		protected JobDocAddress CarrierAddress => carrierAddress ??= JobDocAddress.Load(depHeader, DocAddressType.Carrier);
		JobDocAddress carrierAddress;

		public IParty Consignee => ConsigneeAddress != null ? CachedValueHelper.GetValue(ref consignee, GetConsignee) : null;
		CachedValue<IParty> consignee;

		IParty GetConsignee()
		{
			var consignee = ConsigneeAddress != null ? new KnownEoriPartyProvider(ConsigneeAddress, true, IsInPhase5TransitionPeriod) : null;
			if (consignee != null)
			{
				var consigneePartyProvider = consignee;
				if (consigneePartyProvider.IsEmpty)
				{
					consignee = null;
				}
			}
			return consignee;
		}

		protected JobDocAddress ConsigneeAddress => consigneeAddress ??= JobDocAddress.Load(nctsHeader, DocAddressType.ConsigneeAddress);
		JobDocAddress consigneeAddress;

		public IParty Consignor => ConsignorAddress != null ? CachedValueHelper.GetValue(ref consignor, GetConsignor) : null;
		CachedValue<IParty> consignor;

		IParty GetConsignor()
		{
			var consignor = ConsignorAddress != null ? new KnownEoriPartyProvider(ConsignorAddress, true, IsInPhase5TransitionPeriod) : null;
			if (consignor != null)
			{
				var consignorPartyProvider = consignor;
				if (consignorPartyProvider.IsEmpty)
				{
					consignor = null;
				}
			}
			return consignor;
		}

		protected JobDocAddress ConsignorAddress => consignorAddress ??= JobDocAddress.Load(nctsHeader, DocAddressType.ConsignorDocumentaryAddress);
		JobDocAddress consignorAddress;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= GetAdditionalSupplyChainActors();
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors() => NctsDataRetrieveMethods
			.GetCusReferences(nctsHeader.Factory, nctsHeader.PK).Select((cr, index) => new AdditionalSupplyChainActorProvider(cr, index + 1)).ToArray();

		public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ??= GetTransportEquipments();
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		IReadOnlyCollection<ITransportEquipment> GetTransportEquipments() => nctsHeader.DepartureHeaderContainers
			.Cast<NctsDepartureHeaderContainer>().Select((ctr, index) => new TransportEquipmentsProvider(ctr, index + 1)).ToArray<ITransportEquipment>();

		public ILocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, GetLocationOfGoods);
		CachedValue<ILocationOfGoods> locationOfGoods;

		ILocationOfGoods GetLocationOfGoods() => !depHeader.GoodsLocation.CGL_Qualifier.IsEmpty ? new LocationOfGoodsProvider(nctsHeader) : null;

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??= GetDepartureTransportMeans().ToArray();
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		public IReadOnlyCollection<ICountryOfRoutingOfConsignment> CountryOfRoutingOfConsignments => countryOfRoutingOfConsignments ??= GetCountryOfRoutingOfConsignments();
		ICountryOfRoutingOfConsignment[] countryOfRoutingOfConsignments;

		ICountryOfRoutingOfConsignment[] GetCountryOfRoutingOfConsignments() => nctsHeader.CountriesOfRouting.Cast<CountryOfRouting>()
			.Select(it => new CountryOfRoutingOfConsignmentsProvider(it.CY_Data, it.CY_Order)).ToArray<ICountryOfRoutingOfConsignment>();

		public IReadOnlyCollection<IActiveBorderTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ??= new ActiveBorderTransportMeansCollectionProvider(depHeader);
		IReadOnlyCollection<IActiveBorderTransportMeans> activeBorderTransportMeans;

		public IPlace PlaceOfLoading => !depHeader.BM_PortOfPresentationCode.IsEmpty ? new PlaceProvider(depHeader.BM_PortOfPresentationCode, null) : null;

		public IPlace PlaceOfUnloading => !depHeader.BM_ForeignDestPortKCode.IsEmpty ? new PlaceProvider(depHeader.BM_ForeignDestPortKCode, null) : null;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ??= !IsInPhase5TransitionPeriod
			? GetPreviousDocuments()
			: Array.Empty<IPreviousDocument>();
		IReadOnlyCollection<IPreviousDocument> previousDocuments;

		IReadOnlyCollection<IPreviousDocument> GetPreviousDocuments() => NctsDataRetrieveMethods
				.GetCusSupportingInfo(nctsHeader.Factory, nctsHeader.PK, Constants.CusSupportingInfoTypes.PreviousDocument)
				.Select(csi => new PreviousDocumentProvider(csi, IsInPhase5TransitionPeriod)).ToArray<IPreviousDocument>();

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??= !IsInPhase5TransitionPeriod
			? GetSupportingDocuments()
			: Array.Empty<ISupportingDocument>();
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		IReadOnlyCollection<ISupportingDocument> GetSupportingDocuments() => NctsDataRetrieveMethods
				.GetCusSupportingInfo(nctsHeader.Factory, depHeader.PK, Constants.CusSupportingInfoTypes.SupportingDocument)
				.Select(csi => new SupportingDocumentProvider(csi, IsInPhase5TransitionPeriod)).ToArray<ISupportingDocument>();

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ??= !IsInPhase5TransitionPeriod
			? GetTransportDocuments()
			: Array.Empty<ITransportDocument>();
		IReadOnlyCollection<ITransportDocument> transportDocuments;

		IReadOnlyCollection<ITransportDocument> GetTransportDocuments() => NctsDataRetrieveMethods
				.GetCusSupportingInfo(nctsHeader.Factory, nctsHeader.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.TransportDocument)
				.Select(csi => new TransportDocumentProvider(csi, IsInPhase5TransitionPeriod)).ToArray<ITransportDocument>();

		public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ??= !IsInPhase5TransitionPeriod
			? GetAdditionalReferences()
			: Array.Empty<IAdditionalReference>();
		IReadOnlyCollection<IAdditionalReference> additionalReferences;

		IReadOnlyCollection<IAdditionalReference> GetAdditionalReferences() => NctsDataRetrieveMethods
				.GetCusSupportingInfo(nctsHeader.Factory, nctsHeader.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalReference)
				.Select(csi => new AdditionalReferenceProvider(csi, IsInPhase5TransitionPeriod)).ToArray<IAdditionalReference>();

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ??= !IsInPhase5TransitionPeriod
			? GetAdditionalInformation()
			: Array.Empty<IAdditionalInformation>();
		IReadOnlyCollection<IAdditionalInformation> additionalInformation;

		protected IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(nctsHeader.Factory, nctsHeader.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalInformation)
			.Select(csi => new AdditionalInformationProvider(csi)).ToArray<IAdditionalInformation>();

		public IReadOnlyCollection<IHouseConsignmentType10> HouseConsignments => houseConsignments ??= GetHouseConsignments();
		IReadOnlyCollection<IHouseConsignmentType10> houseConsignments;

		IReadOnlyCollection<IHouseConsignmentType10> GetHouseConsignments() => nctsHeader.Bills.Cast<NctsBill>()
			.OrderBy(bill => bill.SequenceNumber)
			.Select((bill, index) => new NCTSHouseConsignmentProvider(bill, index + 1))
			.ToArray<IHouseConsignmentType10>();

		public string TransportChargesMethodOfPayment => depHeader.BM_MethodOfPayment;

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

		bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => nctsHeader.IsInPhase5TransitionPeriod);
		CachedValue<bool> isInPhase5TransitionPeriod;
	}
}
