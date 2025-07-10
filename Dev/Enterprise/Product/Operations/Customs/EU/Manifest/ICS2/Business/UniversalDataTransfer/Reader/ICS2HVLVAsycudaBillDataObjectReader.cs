using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class ICS2HVLVAsycudaBillDataObjectReader : ICS2AsycudaBillDataObjectReader
	{
		public ICS2HVLVAsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
			this.isUpdateEnabled = isUpdateEnabled;
		}

		protected override void PopulateBillForSpecificRules(ASYCUDA.Business.AsycudaBill bill)
		{
			base.PopulateBillForSpecificRules(bill);
			FillBillQuantity((AsycudaBill)bill);
		}

		public void FillBillQuantity(AsycudaBill bill)
		{
			var billRow = GetColumnIndexer(bill);
			var packedItemQuantity = 0;
			var packingLines = dataObject.PackingLineCollection;

			if (packingLines != null)
			{
				foreach (var packingLine in packingLines)
				{
					var packedItemCount = packingLine.PackedItemCollection?.Count ?? 0;
					packedItemQuantity += packedItemCount;
				}
			}

			SetValue(billRow, AsycudaBillSchema.ABL_ManifestQty, (ZInt)packedItemQuantity);
		}

		protected override void FillPackingLines(ASYCUDA.Business.AsycudaBill bill)
		{
			if ((header.FeatureProvider?.SupportsAsycudaPacks ?? false) && dataObject.PackingLineCollection != null)
			{
				helper.PacksReaderHelper.MarkUnprocessedExistingObjectFor(factory, bill);
				foreach (var packingLineDataObject in dataObject.PackingLineCollection)
				{
					var packedItemCollection = packingLineDataObject.PackedItemCollection;
					if (packedItemCollection != null)
					{
						foreach (var packedItemDataObject in packedItemCollection)
						{
							var pack = new ICS2HVLVAsycudaPackDataObjectReader(packingLineDataObject, packedItemDataObject, logger, factory, bill, helper, isUpdateEnabled, dataObject).ReadIntoBusinessObject();
							helper.PacksReaderHelper.MarkProcessed(pack);
						}
					}
				}
				helper.PacksReaderHelper.DeleteUnprocessedObjectsFor(bill, logger);
			}
		}

		readonly bool isUpdateEnabled;
	}
}
