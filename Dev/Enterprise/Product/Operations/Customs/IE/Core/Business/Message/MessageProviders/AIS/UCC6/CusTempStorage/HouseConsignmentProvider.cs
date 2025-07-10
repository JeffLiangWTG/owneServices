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
	public class HouseConsignmentProvider : IHouseConsignment
	{
		public static HouseConsignmentProvider New(TemporaryStorageBill bill) => new HouseConsignmentProvider(bill);

		public HouseConsignmentProvider(TemporaryStorageBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}
		readonly TemporaryStorageBill bill;

		public decimal TotalGrossMass => Core.Constants.Weight.ConvertSafe(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ, Core.Constants.Weight.Kilograms);

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActorsCached ?? (additionalSupplyChainActorsCached = bill.SupplyChainActors.Select(chainActor => new AdditionalSupplyChainActorProvider(chainActor)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActorsCached;

		public IPerson Consignee => CachedValueHelper.GetValue(ref consigneeCached, () => PersonProvider.New(bill.Consignee, bill.ABL_ConsigneeRegNo, bill.ABL_ConsigneeRegNoType, bill.ABL_ConsigneePhone));
		CachedValue<IPerson> consigneeCached;

		public IPerson Consignor => CachedValueHelper.GetValue(ref consignorCached, () => PersonProvider.New(bill.Shipper, bill.ABL_ShipperRegNo, bill.ABL_ShipperRegNoType, bill.ABL_ShipperPhone));
		CachedValue<IPerson> consignorCached;

		public IPerson NotifyParty => CachedValueHelper.GetValue(ref notifyPartyCached, () => PersonProvider.New(bill.NotifyParty, bill.ABL_NotifyPartyRegNo, bill.ABL_NotifyPartyRegNoType, bill.ABL_NotifyPartyPhone));
		CachedValue<IPerson> notifyPartyCached;

		public string UCR => bill.ABL_UCRNumber;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ?? (previousDocuments = bill.PreviousDocuments.Select((element, index) => PreviousDocumentProvider.New(element, index)).ToArray());
		public IReadOnlyCollection<IPreviousDocument> previousDocuments;

		public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = bill.SupportingDocuments.Select((element, index) => DocumentProvider.New(element, index)).ToArray());
		IReadOnlyCollection<IDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> TransportContractDocuments => transportContractDocuments ?? (transportContractDocuments = bill.AdditionalInfos.Where(x => x.IsATransportDocument).Select((element, index) => DocumentProvider.New(element, index)).ToArray());
		IReadOnlyCollection<IDocument> transportContractDocuments;

		IReadOnlyCollection<AISInterfaces.ITransportEquipment> transportEquipments;
		public IReadOnlyCollection<AISInterfaces.ITransportEquipment> TransportEquipments => transportEquipments
			?? (transportEquipments = bill.PackedItems.SelectMany(item => item.TemporaryStorageLinkPackages).Select(linkPackage => linkPackage.Package?.Container).WhereNotNull().Select(TransportEquipmentProvider.New).ToArray());

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences = bill.AdditionalInfos.Where(x => x.IsAnAdditionalReference).Select((element, index) => DocumentProvider.New(element, index)).ToArray());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations = bill.AdditionalInfos.Where(x => x.IsAnAdditionalInformation).Select(AdditionalInformationProvider.New).ToArray());
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IConsignmentItem> ConsignmentItems => consignmentItemsCached ?? (consignmentItemsCached = bill.PackedItems.Select(ConsignmentItemProvider.New).ToArray());
		IReadOnlyCollection<IConsignmentItem> consignmentItemsCached;
	}
}
