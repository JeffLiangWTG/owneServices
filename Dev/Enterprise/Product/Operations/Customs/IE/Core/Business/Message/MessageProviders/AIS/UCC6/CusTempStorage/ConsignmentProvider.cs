using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using AISInterfaces = CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class ConsignmentProvider : IConsignment
	{
		public static ConsignmentProvider New(TemporaryStorageHeader header)
			=> header == null ? null : new ConsignmentProvider(header, header.MasterBill);

		ConsignmentProvider(TemporaryStorageHeader header, TemporaryStorageBill bill)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.bill = Argument.NotNull(bill, nameof(bill));
			authorizationUsage = header.AuthorizationUsage as CusAuthorizationUsage;
		}
		readonly TemporaryStorageHeader header;
		readonly TemporaryStorageBill bill;
		readonly CusAuthorizationUsage authorizationUsage;

		public DateTime EstimatedDateAndTimeOfArrivalAtThePortOfUnloading => bill.ABL_A_ARV.IsValid ? bill.ABL_A_ARV.ToDateTime() : default;

		public decimal GrossMass => bill.ABL_GrossWeight;

		public string ReceptacleIdentificationNumber => null;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActorsCached ??= bill.SupplyChainActors.Select(chainActor => new AdditionalSupplyChainActorProvider(chainActor)).ToArray();
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActorsCached;

		public IIdType ArrivalTransportMeans => CachedValueHelper.GetValue(ref arrivalTransportMeansCached, () => ArrivalTransportMeansProvider.New(header.ArrivalTransportMeans));
		CachedValue<IIdType> arrivalTransportMeansCached;

		public IPerson Consignee => CachedValueHelper.GetValue(ref consigneeCached, () => PersonProvider.New(bill.Consignee, bill.ABL_ConsigneeRegNo, bill.ABL_ConsigneeRegNoType, bill.ABL_ConsigneePhone));
		CachedValue<IPerson> consigneeCached;

		public IPerson Consignor => CachedValueHelper.GetValue(ref consignorCached, () => PersonProvider.New(bill.Shipper, bill.ABL_ShipperRegNo, bill.ABL_ShipperRegNoType, bill.ABL_ShipperPhone));
		CachedValue<IPerson> consignorCached;

		public IGoodsLocation LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, () => LocationOfGoodsProvider.New((CusGoodsLocation)header.GoodsLocation));
		CachedValue<IGoodsLocation> locationOfGoods;

		public ILoadingUnloadingLocation LoadingLocation => CachedValueHelper.GetValue(ref loadingLocation, () => LoadingUnloadingLocationProvider.New(bill.PortOfLoading));
		CachedValue<ILoadingUnloadingLocation> loadingLocation;

		public IPerson NotifyParty => CachedValueHelper.GetValue(ref notifyPartyCached, () => PersonProvider.New(bill.NotifyParty, bill.ABL_NotifyPartyRegNo, bill.ABL_NotifyPartyRegNoType, bill.ABL_NotifyPartyPhone));
		CachedValue<IPerson> notifyPartyCached;

		public string UCR => bill.ABL_UCRNumber;

		public ILoadingUnloadingLocation PlaceOfUnloading => CachedValueHelper.GetValue(ref placeOfUnloading, () => LoadingUnloadingLocationProvider.New(bill.PortOfDischarge));
		CachedValue<ILoadingUnloadingLocation> placeOfUnloading;

		public IIdType Warehouse => CachedValueHelper.GetValue(ref warehouse, () => authorizationUsage != null ? IdTypeProvider.New(Constants.IDType.V, authorizationUsage.AGC_Number) : null);
		CachedValue<IIdType> warehouse;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ??= bill.PreviousDocuments.Select((element, index) => PreviousDocumentProvider.New(element, index)).ToArray();
		public IReadOnlyCollection<IPreviousDocument> previousDocuments;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ??= bill.SupportingDocuments.Select((element, index) => DocumentProvider.New(element, index)).ToArray();
		IReadOnlyCollection<IDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> TransportContractDocuments => transportContractDocuments ??= bill.AdditionalInfos.Where(x => x.IsATransportDocument).Select((element, index) => DocumentProvider.New(element, index)).ToArray();
		IReadOnlyCollection<IDocument> transportContractDocuments;

		public IReadOnlyCollection<AISInterfaces.ITransportEquipment> TransportEquipments => transportEquipments ??= bill.Header.Containers.Select(TransportEquipmentProvider.New).ToArray();
		IReadOnlyCollection<AISInterfaces.ITransportEquipment> transportEquipments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= bill.AdditionalInfos.Where(x => x.IsAnAdditionalInformation).Select(AdditionalInformationProvider.New).ToArray();
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IHouseConsignment> HouseConsignments => houseConsignmentsCached ??= bill.Header.HouseBills.Select(HouseConsignmentProvider.New).ToArray();
		IReadOnlyCollection<IHouseConsignment> houseConsignmentsCached;

		public IReadOnlyCollection<IConsignmentItem> ConsignmentItems => consignmentItemsCached ??= bill.PackedItems.Select(packedItem => new ConsignmentItemProvider(packedItem)).ToArray();
		IReadOnlyCollection<IConsignmentItem> consignmentItemsCached;

		public string InlandModeOfTransport => null;

		public string ModeOfTransportAtTheBorder => null;

		public string ReferenceNumberUCR => bill.ABL_UCRNumber;

		public int TotalPackageNumber => 0;

		public IMoney TransportAndInsuranceCostsToTheDestination => null;

		public IDocument TransportDocument => null;
	}
}
