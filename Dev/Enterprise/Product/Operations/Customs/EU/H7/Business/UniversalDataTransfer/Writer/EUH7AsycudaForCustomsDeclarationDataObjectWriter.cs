using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Common.AsycudaCustoms;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer
{
	public class EUH7AsycudaForCustomsDeclarationDataObjectWriter : AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>
	{
		public EUH7AsycudaForCustomsDeclarationDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override void PopulateDataObject(AsycudaBill sourceBill, Shipment uxml)
		{
			base.PopulateDataObject(sourceBill, uxml);
			PopulatePack(sourceBill, uxml);
			PopulateEntryHeaderCollection(uxml);
			uxml.ShipmentIncoTerm = ListHelper.GetWithDescription<UniversalIncoTerm>(sourceBill.ABL_Incoterm, sourceBill.Lookups.IncotermList);
		}

		protected override void PopulateWeight(AsycudaBill sourceBill, Shipment uxml)
		{
			uxml.TotalWeight = sourceBill.ABL_GrossWeight;
			uxml.TotalWeightUnit = new UnitOfWeight
			{
				Code = sourceBill.ABL_GrossWeightUQ
			};
		}

		protected override void PopulateVolume(AsycudaBill sourceBill, Shipment uxml)
		{
			uxml.TotalVolume = sourceBill.ABL_Volume;
			uxml.TotalVolumeUnit = new UnitOfVolume
			{
				Code = sourceBill.ABL_VolumeUQ
			};
		}

		protected override void PopulateCommercialData(AsycudaBill sourceBill, Shipment uxml)
		{
			var invoiceHeader = PopulateCommercialInvoice(sourceBill);
			var invoiceLines = new DataObjectList<CommercialInvoiceLine>();
			sourceBill.PackedItems.ForEach(item => invoiceLines.Add(PopulateCommercialInvoiceLine(item)));
			invoiceHeader.SetCommercialInvoiceLineCollection(() => invoiceLines);
			uxml.CommercialInfo = new CommercialInfo();
			uxml.CommercialInfo.SetWriterStrategy(writeManager.WriterStrategy);
			uxml.CommercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { invoiceHeader });
		}

		protected override void PopulateMessageType(AsycudaBill sourceBill, Shipment uxml)
		{
			uxml.MessageType = new CodeDescriptionPair { Code = AsycudaJobMessageTypeList.Codes.Import };
		}

		CommercialInvoiceHeader PopulateCommercialInvoice(AsycudaBill sourceBill)
		{
			var firstLineGrossWeightUQ = FirstPackedItemGrossWeightUQ(sourceBill) ?? FirstPackedItemNetWeightUQ(sourceBill) ?? sourceBill.ABL_GrossWeightUQ;
			var firstLineNetWeightUQ = FirstPackedItemNetWeightUQ(sourceBill) ?? FirstPackedItemGrossWeightUQ(sourceBill) ?? sourceBill.ABL_GrossWeightUQ;

			return new CommercialInvoiceHeader(writeManager.WriterStrategy)
			{
				InvoiceAmount = sourceBill.ABL_GoodsValue,
				InvoiceCurrency = new Currency
				{
					Code = sourceBill.ABL_RX_NKGoodsValueCurrency
				},
				Weight = sourceBill.PackedItems.Sum(item => Weight.Convert(item.API_GrossWeight, item.API_GrossWeightUQ, firstLineGrossWeightUQ)),
				WeightUnit = new UnitOfWeight
				{
					Code = firstLineGrossWeightUQ
				},
				NetWeight = sourceBill.PackedItems.Sum(item => Weight.Convert(item.API_NetWeight, item.API_NetWeightUQ, firstLineNetWeightUQ)),
				NetWeightUQ = new UnitOfWeight
				{
					Code = firstLineNetWeightUQ
				}
			};
		}

		CommercialInvoiceLine PopulateCommercialInvoiceLine(AsycudaPackedItem item)
		{
			return new CommercialInvoiceLine(writeManager.WriterStrategy)
			{
				HarmonisedCode = item.API_FormattedTariff,
				Description = item.API_GoodsDescription,
				CountryOfOrigin = new Country() { Code = item.API_RN_NKGoodsOrigin },
				InvoiceQuantity = item.API_CustomsQty,
				Weight = item.API_GrossWeight,
				WeightUnit = new UnitOfWeight
				{
					Code = item.API_GrossWeightUQ,
				},
				NetWeight = item.API_NetWeight,
				NetWeightUnit = new UnitOfWeight
				{
					Code = item.API_NetWeightUQ,
				},
				LinePrice = ConvertCommercialInvoiceLineLinePrice(item),
				InvoiceQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(item.API_CustomsUQ, item.Lookups.PackTypeList),
			};
		}

		void PopulatePack(AsycudaBill sourceBill, Shipment uxml)
		{
			uxml.OuterPacks = sourceBill.ABL_ManifestQty;
			uxml.OuterPacksPackageType = new PackageType
			{
				Code = sourceBill.Packs[0]?.APA_PackUQ
			};
		}

		void PopulateEntryHeaderCollection(Shipment uxml)
		{
			uxml.SetEntryHeaderCollection(() => new List<EntryHeader>());
		}

		ZString? FirstPackedItemGrossWeightUQ(AsycudaBill sourceBill)
		{
			ZString? packedItemGrossWeightUQs = sourceBill.PackedItems
				.Select(item => item.API_GrossWeightUQ)
				.FirstOrDefault(item => !item.IsEmpty);

			return string.IsNullOrEmpty(packedItemGrossWeightUQs) ? null : packedItemGrossWeightUQs;
		}

		ZString? FirstPackedItemNetWeightUQ(AsycudaBill sourceBill)
		{
			ZString? packedItemNetWeightUQs = sourceBill.PackedItems
				.Select(item => item.API_NetWeightUQ)
				.FirstOrDefault(item => !item.IsEmpty);

			return string.IsNullOrEmpty(packedItemNetWeightUQs) ? null : packedItemNetWeightUQs;
		}

		ZDecimal? ConvertCommercialInvoiceLineLinePrice(AsycudaPackedItem item)
		{
			ZDecimal? customsValue = null;
			if (!item.API_CustomsValue.IsEmpty)
			{
				customsValue = item.API_CustomsValue;
				if (!item.API_RX_NKGoodsValueCurrency.IsEmpty && item.API_RX_NKGoodsValueCurrency != item.Bill.ABL_RX_NKGoodsValueCurrency)
				{
					var converter = CurrencyConverter.New(item.Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, true);
					var originalAmount = new Money(item.API_CustomsValue, item.GoodsValueCurrency);
					var convertedValue = converter.ConvertExact(originalAmount, item.Bill.GoodsValueCurrency);
					customsValue = convertedValue.Amount;
				}
			}

			return customsValue;
		}
	}
}
