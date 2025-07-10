using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;

namespace Enterprise.Customs.IE.H7.Business
{
	public class GoodsShipmentProvider : IGoodsShipment
	{
		public GoodsShipmentProvider(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		public string NatureOfTransaction => null;

		public decimal TotalAmountInvoiced => bill.ABL_GoodsValue;

		public string InvoiceCurrency => bill.ABL_RX_NKCustomsValueCurrency;

		public DateTime DateOfAcceptance => DateTime.MinValue;

		public decimal ExchangeRate => 0;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => Array.Empty<IAdditionalSupplyChainActor>();

		public CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty Buyer => null;

		public CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty Seller => null;

		public CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () => MessageProviderHelper.GetExporter(bill));
		CachedValue<CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty> exporterCached;

		public IDeliveryTerms DeliveryTerms => null;

		public string CountryOfDispatch => null;

		public string MemberStateTerritory => null;

		public IDestination Destination => null;

		public IIdType Warehouse => null;

		public IReadOnlyCollection<IMPreviousDocument> PreviousDocuments => previousDocuments ?? (previousDocuments =
			bill.PreviousDocuments.Select(x => new MPreviousDocumentProvider(x)).ToArray<IMPreviousDocument>());
		IReadOnlyCollection<IMPreviousDocument> previousDocuments;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
			bill.SupportingDocuments.Select(x => new SupportingDocumentProvider(x)).ToArray<ISupportingDocument>());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<ICcQualifierDocument> AdditionalReferences => additionalReferencesCached ?? (additionalReferencesCached =
			bill.AdditionalDocuments.Where(x => x.IsAnAdditionalReference)
			.Select(x => new CcQualifierDocumentProvider(x)).ToArray<ICcQualifierDocument>());
		IReadOnlyCollection<ICcQualifierDocument> additionalReferencesCached;

		public IReadOnlyCollection<ICcQualifierAdditionalInformation> AdditionalInformations => additionalInformationsCached ?? (additionalInformationsCached =
			bill.AdditionalDocuments.Where(x => x.IsAnAdditionalInformation)
			.Select(x => new CcQualifierAdditionalInformationProvider(x)).ToArray<ICcQualifierAdditionalInformation>());
		IReadOnlyCollection<ICcQualifierAdditionalInformation> additionalInformationsCached;

		public IReadOnlyCollection<IAdditionsAndDeductions> AdditionsAndDeductions => Array.Empty<IAdditionsAndDeductions>();

		public IReadOnlyCollection<IAdditionalFiscalReference> AdditionalFiscalReferences =>
			bill.ABL_SellerRegNo.IsEmpty ? Array.Empty<IAdditionalFiscalReference>() : additionalFiscalReferences ?? (additionalFiscalReferences = new AdditionalFiscalReferenceProvider[] { new (bill.ABL_SellerRegNo) });
		IReadOnlyCollection<IAdditionalFiscalReference> additionalFiscalReferences;

		public IMoney PostalCharges => null;

		public IMConsignment04 Consignment => CachedValueHelper.GetValue(ref consignmentCached, () => new MConsignment04Provider(bill));
		CachedValue<IMConsignment04> consignmentCached;

		public IReadOnlyCollection<IGoodsShipmentItem> GoodsShipmentItems => goodsShipmentItem ?? (goodsShipmentItem =
			bill.PackedItems.Cast<AsycudaPackedItem>().Select(x => new GoodsShipmentItemProvider(x)).ToArray());
		IReadOnlyCollection<IGoodsShipmentItem> goodsShipmentItem;
	}
}
