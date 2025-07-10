using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer
{
	public class EUH7HVLVAsycudaPackDataObjectReader : DataObjectReader<PackingLine, AsycudaPack>
	{
		public EUH7HVLVAsycudaPackDataObjectReader(PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill bill, Shipment hvlvShipmentDataObject) : base(dataObject, logger, factory)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.hvlvShipmentDataObject = hvlvShipmentDataObject;
		}

		readonly AsycudaBill bill;
		readonly Shipment hvlvShipmentDataObject;

		protected override AsycudaPack GetExistingBusinessObject()
		{
			return null;
		}

		protected override AsycudaPack GetNewBusinessObject()
		{
			return bill.Packs.AddNew();
		}

		protected override void PopulateBusinessObject(AsycudaPack pack)
		{
			FillAsycudaPack(pack);
			FillPackedItem(pack);
		}

		void FillAsycudaPack(AsycudaPack pack)
		{
			var packingLineRow = GetColumnIndexer(pack);
			var unitOfWeight = hvlvShipmentDataObject.TotalWeightUnit.GetCodeAsUpperCase();

			SetValue(packingLineRow, AsycudaPackSchema.APA_PackQty, (ZInt)1);
			SetValue(packingLineRow, AsycudaPackSchema.APA_PackUQ, ZString.Empty);
			SetValue(packingLineRow, AsycudaPackSchema.APA_GoodsDescription, dataObject.GoodsDescription ?? ZString.Empty);
			SetValue(packingLineRow, AsycudaPackSchema.APA_Weight, dataObject.Weight ?? dataObject.ManifestedWeight);
			SetValue(packingLineRow, AsycudaPackSchema.APA_WeightUQ, (ZString?)unitOfWeight ?? ZString.Empty);

			var packedItemCollection = dataObject.PackedItemCollection;
			if (packedItemCollection != null)
			{
				var linePrice = packedItemCollection.Sum(packedItem =>
				{
					var commercialInvoiceLine = packedItem.FindMatchingCommercialInvoiceLine(hvlvShipmentDataObject);
					return commercialInvoiceLine?.LinePrice ?? ZDecimal.Zero;
				});
				var linePriceDetail = new GenAddOnDetail() { GenAddOnColumnName = ASYCUDA.Business.AsycudaPack.Schema.LinePrice, PropertyName = ASYCUDA.Business.AsycudaPack.Schema.LinePrice, Value = (ZDecimal)linePrice };
				linePriceDetail.ReadIntoBusinessObject(IsDefaultingEnabled, pack);

				if (hvlvShipmentDataObject.GoodsValueCurrency != null)
				{
					var linePriceCurrency = hvlvShipmentDataObject.GoodsValueCurrency.GetCodeAsUpperCase();
					var linePriceCurrencyDetail = new GenAddOnDetail() { GenAddOnColumnName = ASYCUDA.Business.AsycudaPack.Schema.LinePriceCurrency, PropertyName = ASYCUDA.Business.AsycudaPack.Schema.LinePriceCurrency, Value = linePriceCurrency };
					linePriceCurrencyDetail.ReadIntoBusinessObject(IsDefaultingEnabled, pack);
				}
			}
		}

		void FillPackedItem(AsycudaPack pack)
		{
			if (dataObject.PackedItemCollection != null)
			{
				foreach (var packedItem in dataObject.PackedItemCollection)
				{
					if (ShouldFillPackedItem(packedItem))
					{
						new EUH7HVLVAsycudaPackedItemObjectReader(packedItem, logger, factory, bill, pack, hvlvShipmentDataObject).ReadIntoBusinessObject();
					}
				}
			}
		}

		bool ShouldFillPackedItem(PackedItem packedItem)
		{
			var commercialInvoiceLine = packedItem.FindMatchingCommercialInvoiceLine(hvlvShipmentDataObject);

			return !string.IsNullOrEmpty(packedItem.Description)
				|| !string.IsNullOrEmpty(commercialInvoiceLine.HarmonisedCode)
				|| !string.IsNullOrEmpty(commercialInvoiceLine.CountryOfOrigin.Code)
				|| !string.IsNullOrEmpty(hvlvShipmentDataObject.GoodsValueCurrency.Code)
				|| (packedItem.GoodsValue.HasValue && packedItem.GoodsValue.Value > 0)
				|| (commercialInvoiceLine.NetWeight.HasValue && commercialInvoiceLine.NetWeight.Value > 0)
				|| (commercialInvoiceLine.Weight.HasValue && commercialInvoiceLine.Weight.Value > 0)
				|| (commercialInvoiceLine.CustomsValue.HasValue && commercialInvoiceLine.CustomsValue.Value > 0)
				|| (commercialInvoiceLine.CustomsQuantity.HasValue && commercialInvoiceLine.CustomsQuantity.Value > 0);
		}
	}
}
