using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer
{
	public class EUH7HVLVAsycudaPackedItemObjectReader : DataObjectReader<PackedItem, AsycudaPackedItem>
	{
		public EUH7HVLVAsycudaPackedItemObjectReader(PackedItem dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill bill, AsycudaPack pack, Shipment hvlvShipmentDataObject)
			: base(dataObject, logger, factory)
		{
			this.pack = Argument.NotNull(pack, nameof(pack));
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.hvlvShipmentDataObject = hvlvShipmentDataObject;
		}

		readonly AsycudaBill bill;
		readonly AsycudaPack pack;
		readonly Shipment hvlvShipmentDataObject;

		protected override AsycudaPackedItem GetExistingBusinessObject()
		{
			return pack.PackedItem;
		}

		protected override AsycudaPackedItem GetNewBusinessObject()
		{
			var packedItem = bill.PackedItems.AddNew();
			packedItem.PackagesPivot.AddPivotFor(pack);
			return packedItem;
		}

		protected override void PopulateBusinessObject(AsycudaPackedItem packedItem)
		{
			var packedItemRow = GetColumnIndexer(packedItem);
			var commercialInvoiceLine = dataObject.FindMatchingCommercialInvoiceLine(hvlvShipmentDataObject);

			var netWeight = commercialInvoiceLine.NetWeight ?? 0;
			var grossWeight = commercialInvoiceLine.Weight ?? 0;
			var weightUnit = commercialInvoiceLine.WeightUnit.GetCodeAsUpperCase();
			var convertToWeightUnit = (ZString)Weight.Kilograms;

			if (!weightUnit.Equals(convertToWeightUnit))
			{
				netWeight = Weight.ConvertSafe(netWeight, weightUnit, convertToWeightUnit);
				grossWeight = Weight.ConvertSafe(grossWeight, weightUnit, convertToWeightUnit);
			}

			SetValue(packedItemRow, AsycudaPackedItemSchema.API_NetWeight, netWeight);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GrossWeight, grossWeight);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_Tariff, commercialInvoiceLine.HarmonisedCode);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_RN_NKGoodsOrigin, commercialInvoiceLine.CountryOfOrigin);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsValue, commercialInvoiceLine.CustomsValue);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsQty, commercialInvoiceLine.CustomsQuantity);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GoodsDescription, dataObject.Description ?? ZString.Empty);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GoodsValue, dataObject.GoodsValue);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_RX_NKGoodsValueCurrency, hvlvShipmentDataObject.GoodsValueCurrency);
		}
	}
}
