using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaPackedItemObjectReader : DataObjectReader<PackingLine, AsycudaPackedItem>
	{
		public AsycudaPackedItemObjectReader(PackingLine dataObject, List<AddInfo> packedItemAddInfoCollection, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaPack pack, AsycudaManifestDataObjectReaderHelper helper)
			: base(dataObject, logger, factory)
		{
			this.pack = Argument.NotNull(pack, "pack");
			this.helper = Argument.NotNull(helper, "helper");
			this.packedItemAddInfoCollection = packedItemAddInfoCollection;
		}
		readonly AsycudaPack pack;
		readonly AsycudaManifestDataObjectReaderHelper helper;
		readonly List<AddInfo> packedItemAddInfoCollection;

		protected override AsycudaPackedItem GetExistingBusinessObject()
		{
			return pack.IsOnePackedItemRelationship ? pack.PackedItem : null;
		}

		protected override AsycudaPackedItem GetNewBusinessObject()
		{
			return pack.PackedItems.AddNewPackedItem();
		}

		protected sealed override void PopulateBusinessObject(AsycudaPackedItem packedItem)
		{
			var packedItemRow = GetColumnIndexer(packedItem);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_RN_NKGoodsOrigin, dataObject.CountryOfOrigin);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_Tariff, dataObject.HarmonisedCode);
			SetValue(packedItemRow, AsycudaPackedItemSchema.API_GoodsDescription, dataObject.GetCleanSingleLineGoodsDescription());

			PopulatePackedItemForSpecificRules(packedItem);
			ReadAddInfo(packedItem);
		}

		protected virtual void PopulatePackedItemForSpecificRules(AsycudaPackedItem packedItem)
		{
		}

		void ReadAddInfo(AsycudaPackedItem packedItem)
		{
			if (packedItemAddInfoCollection != null)
			{
				var packedItemRow = GetColumnIndexer(packedItem);
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsValue, packedItemAddInfoCollection.GetZDecimalValue(AddInfoConstants.PackedItem.CustomsValue, logger));
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_DutyAmount, packedItemAddInfoCollection.GetZDecimalValue(AddInfoConstants.PackedItem.DutyValue, logger));
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_TaxAmount, packedItemAddInfoCollection.GetZDecimalValue(AddInfoConstants.PackedItem.TaxValue, logger));
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsQty, packedItemAddInfoCollection.GetZDecimalValue(AddInfoConstants.PackedItem.CustomsQty, logger));
				SetValue(packedItemRow, AsycudaPackedItemSchema.API_CustomsUQ, packedItemAddInfoCollection.GetZStringValue(AddInfoConstants.PackedItem.CustomsUQ, logger));
				new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(packedItemAddInfoCollection, helper.GetAsycudaPackedItemGenAddOnColumnList(packedItem), packedItem);
				PopulateCusEntryNumber(packedItem);
			}
		}

		void PopulateCusEntryNumber(AsycudaPackedItem packedItem)
		{
			var countryCode = helper.CountryCode;
			foreach (var pair in helper.GetPackedItemEntryNumberMapping())
			{
				var number = packedItemAddInfoCollection.GetZStringValue(pair.Key, logger);
				if (number.HasValue)
				{
					var entryNumberDataObject = new UniversalCustoms.EntryNumber()
					{
						Type = new EntryType() { Code = pair.Value },
						Number = number.Value,
						EntryIsSystemGenerated = false,
					};
					new CustomsEntryNumberDataObjectReader<AsycudaPackedItem>(entryNumberDataObject, logger, factory, packedItem, countryCode).ReadIntoBusinessObject();
				}
			}
		}
	}
}
