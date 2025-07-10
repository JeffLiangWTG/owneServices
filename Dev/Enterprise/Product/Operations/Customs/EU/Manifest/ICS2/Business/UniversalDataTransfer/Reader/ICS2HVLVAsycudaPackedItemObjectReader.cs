using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2HVLVAsycudaPackedItemObjectReader : AsycudaPackedItemObjectReader
	{
		public ICS2HVLVAsycudaPackedItemObjectReader(PackingLine dataObject, PackedItem packedItemDataObject, List<AddInfo> packedItemAddInfoCollection, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaPack pack, AsycudaManifestDataObjectReaderHelper helper, Shipment hvlvShipment)
			: base(dataObject, packedItemAddInfoCollection, logger, factory, pack, helper)
		{
			this.hvlvShipment = hvlvShipment;
			this.packedItemDataObject = packedItemDataObject;
		}

		readonly PackedItem packedItemDataObject;
		readonly Shipment hvlvShipment;

		protected override void PopulatePackedItemForSpecificRules(ASYCUDA.Business.AsycudaPackedItem packedItem)
		{
			if (packedItemDataObject != null && hvlvShipment != null)
			{
				var packedItemDataRow = GetColumnIndexer(packedItem);
				SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_GoodsDescription, packedItemDataObject.Description);

				var commercialInvoiceLine = packedItemDataObject.FindMatchingCommercialInvoiceLine(hvlvShipment);
				if (commercialInvoiceLine != null)
				{
					SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_RN_NKGoodsOrigin, commercialInvoiceLine.CountryOfOrigin);
					SetValue(packedItemDataRow, AsycudaPackedItemSchema.API_Tariff, commercialInvoiceLine.HarmonisedCode);
				}
			}
		}
	}
}
