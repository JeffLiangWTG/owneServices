using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;

namespace Enterprise.Customs.IE.H7.Business
{
	public class GoodsShipmentItemProvider : IGoodsShipmentItem
	{
		public GoodsShipmentItemProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
			pack = packedItem.Pack;
		}

		readonly AsycudaPack pack;
		readonly AsycudaPackedItem packedItem;

		public string DeclarationGoodsItemNumber => null;

		public decimal StatisticalValue => 0;

		public string NatureOfTransaction => null;

		public string ReferenceNumberUCR => null;

		public DateTime DateOfAcceptance => DateTime.MinValue;

		public IReadOnlyCollection<IAuthorisation> Authorisations => Array.Empty<IAuthorisation>();

		public IProcedure Procedure => CachedValueHelper.GetValue(ref procedure, () => new ProcedureProvider(packedItem));
		CachedValue<IProcedure> procedure;

		public IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IAdditionalSupplyChainActor> AdditionalSupplyChainActors => Array.Empty<CargoWise.Customs.IE.MessageContracts.Interfaces.IAdditionalSupplyChainActor>();

		public IParty Buyer => null;

		public IParty Seller => null;

		public IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () => MessageProviderHelper.GetExporter(pack));
		CachedValue<IParty> exporterCached;

		public IOrigin Origin => null;

		public string CountryOfDispatch => null;

		public string MemberStateTerritory => null;

		public IDestination Destination => null;

		public IMCommodity Commodity => CachedValueHelper.GetValue(ref commodityCached, () => new MCommodityProvider(packedItem));
		CachedValue<IMCommodity> commodityCached;

		public IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IPackaging> Packages => packages ?? (packages = new SequencedPackagingProvider[] { new SequencedPackagingProvider(pack) });
		IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IPackaging> packages;

		public IReadOnlyCollection<IPreviousDocumentGoodsShipmentItem> PreviousDocuments => previousDocuments ?? (previousDocuments =
			packedItem.PreviousDocuments.Select(x => new PreviousDocumentGoodsShipmentItemProvider(x)).ToArray<IPreviousDocumentGoodsShipmentItem>());
		IReadOnlyCollection<IPreviousDocumentGoodsShipmentItem> previousDocuments;

		public IReadOnlyCollection<ISupportingDocumentGoodsShipmentItem> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
			packedItem.SupportingDocuments.Select(x => new SupportingDocumentGoodsShipmentItemProvider(x)).ToArray<ISupportingDocumentGoodsShipmentItem>());
		IReadOnlyCollection<ISupportingDocumentGoodsShipmentItem> supportingDocuments;

		public IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
			packedItem.AdditionalDocuments.Where(x => x.IsATransportDocument)
			.Select(x => new CcQualifierDocumentProvider(x)).ToArray<ICcQualifierDocument>());
		IReadOnlyCollection<CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument> transportDocuments;

		public IReadOnlyCollection<ICcQualifierDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
			packedItem.AdditionalDocuments.Where(x => x.IsAnAdditionalReference)
			.Select(x => new CcQualifierDocumentProvider(x)).ToArray<ICcQualifierDocument>());
		IReadOnlyCollection<ICcQualifierDocument> additionalReferences;

		public IReadOnlyCollection<ICcQualifierAdditionalInformation> AdditionalInformations => additionalInformations ?? (additionalInformations =
			packedItem.AdditionalDocuments.Where(x => x.IsAnAdditionalInformation)
			.Select(x => new CcQualifierAdditionalInformationProvider(x)).ToArray<ICcQualifierAdditionalInformation>());
		IReadOnlyCollection<ICcQualifierAdditionalInformation> additionalInformations;

		public ICustomsValuation CustomsValuation => null;

		public string ValuationAdjustment => null;

		public IReadOnlyCollection<IAdditionalFiscalReference> AdditionalFiscalReferences =>
			pack?.Bill is AsycudaBill packBill && !packBill.ABL_SellerRegNo.IsEmpty
			? additionalFiscalReferences ??= new AdditionalFiscalReferenceProvider[] { new(pack.Bill.ABL_SellerRegNo) }
			: Array.Empty<IAdditionalFiscalReference>();

		IReadOnlyCollection<IAdditionalFiscalReference> additionalFiscalReferences;

		public ITransportCosts TransportAndInsuranceCostsToTheDestination => CachedValueHelper.GetValue(ref transportCostsCached, () => TransportCostsProvider.NewOrNullPerPackedItem(packedItem.Bill));
		CachedValue<ITransportCosts> transportCostsCached;

		public IReadOnlyCollection<string> ContainerIds => Array.Empty<string>();
	}
}
