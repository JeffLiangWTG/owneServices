using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class ICS2HVLVAsycudaPackDataObjectReader : AsycudaPackDataObjectReader
	{
		public ICS2HVLVAsycudaPackDataObjectReader(PackingLine dataObject, PackedItem packedItemDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled, Shipment hvlvShipmentDataObject)
			: base(dataObject, logger, factory, bill, helper, isUpdateEnabled)
		{
			this.hvlvShipmentDataObject = hvlvShipmentDataObject;
			this.packedItemDataObject = packedItemDataObject;
		}

		readonly Shipment hvlvShipmentDataObject;
		readonly PackedItem packedItemDataObject;

		protected override void FillPackedItem(ASYCUDA.Business.AsycudaPack packingLine)
		{
			FillPackInfo(packingLine, packedItemDataObject);
			new ICS2HVLVAsycudaPackedItemObjectReader(dataObject, packedItemDataObject, null, logger, factory, packingLine, helper, hvlvShipmentDataObject).ReadIntoBusinessObject();
		}

		void FillPackInfo(ASYCUDA.Business.AsycudaPack packingLine, PackedItem packedItemDataObject)
		{
			var packingLineRow = GetColumnIndexer(packingLine);
			if (hvlvShipmentDataObject.GoodsValueCurrency != null)
			{
				var commercialInvoiceLine = packedItemDataObject.FindMatchingCommercialInvoiceLine(hvlvShipmentDataObject);

				var linePriceCurrency = hvlvShipmentDataObject.GoodsValueCurrency.GetCodeAsUpperCase();
				var linePriceCurrencyDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaPack.Schema.LinePriceCurrency, PropertyName = AsycudaPack.Schema.LinePriceCurrency, Value = linePriceCurrency };
				linePriceCurrencyDetail.ReadIntoBusinessObject(IsDefaultingEnabled, packingLine);

				var linePrice = packingLine.LinePrice + commercialInvoiceLine.LinePrice ?? 0m;
				var linePriceDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaPack.Schema.LinePrice, PropertyName = AsycudaPack.Schema.LinePrice, Value = (ZDecimal)linePrice };
				linePriceDetail.ReadIntoBusinessObject(IsDefaultingEnabled, packingLine);
			}

			var unitOfQuantity = packedItemDataObject.UnitOfQuantity.GetCodeAsUpperCase();
			var unitOfWeight = packedItemDataObject.GrossWeightUnit.GetCodeAsUpperCase();

			SetValue(packingLineRow, AsycudaPackSchema.APA_CommodityCode, packedItemDataObject.Product.Code ?? ZString.Empty);
			SetValue(packingLineRow, AsycudaPackSchema.APA_GoodsDescription, packedItemDataObject.Description ?? ZString.Empty);
			SetValue(packingLineRow, AsycudaPackSchema.APA_Volume, ZDecimal.Zero);
			SetValue(packingLineRow, AsycudaPackSchema.APA_VolumeUQ, ZString.Empty);
			SetValue(packingLineRow, AsycudaPackSchema.APA_PackQty, (ZInt?)packedItemDataObject.PackedQuantity ?? ZInt.Zero);
			SetValue(packingLineRow, AsycudaPackSchema.APA_PackUQ, unitOfQuantity.IsEmpty ? (ZString)PkgUnit.Piece : unitOfQuantity);
			SetValue(packingLineRow, AsycudaPackSchema.APA_Weight, packedItemDataObject.GrossWeight ?? ZDecimal.Zero);
			SetValue(packingLineRow, AsycudaPackSchema.APA_WeightUQ, unitOfWeight.IsEmpty ? (ZString)Weight.Kilograms : unitOfWeight);
		}
	}
}

