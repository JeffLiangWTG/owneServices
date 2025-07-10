using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public abstract class GoodsItemDataObjectWriter<T> : DataObjectWriter<T, UniversalCustoms.CommercialInvoiceHeader>
		where T : NctsCommonCargoDesc
	{
		protected GoodsItemDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, Shipment headerData, UniversalCustoms.CommercialInvoiceHeader commercialInvoiceHeaderData)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "UniversalDataObjectWriterHelper helper");
			this.headerData = headerData;
			this.commercialInvoiceHeaderData = commercialInvoiceHeaderData;
		}
		protected readonly UniversalDataObjectWriterHelper helper;
		readonly UniversalCustoms.CommercialInvoiceHeader commercialInvoiceHeaderData;
		readonly Shipment headerData;

		protected sealed override UniversalCustoms.CommercialInvoiceHeader PopulateDataObject(T goodsIteBO)
		{
			var commercialInvoiceLineCollection = commercialInvoiceHeaderData.CommercialInvoiceLineCollection;
			if (commercialInvoiceLineCollection != null)
			{
				var commercialInvoiceLineData = PopulateCommercialInvoiceLineData(goodsIteBO);
				commercialInvoiceLineCollection.Add(commercialInvoiceLineData);
			}
			return commercialInvoiceHeaderData;
		}

		protected virtual UniversalCustoms.CommercialInvoiceLine PopulateCommercialInvoiceLineData(T goodsIteBO)
		{
			var commercialInvoiceLineData = new UniversalCustoms.CommercialInvoiceLine(writeManager.WriterStrategy)
			{
				LineNo = goodsIteBO.BY_LineNo,
				Description = goodsIteBO.BY_Description,
				Weight = goodsIteBO.BY_GrossWeight,
				WeightUnit = new UnitOfWeight
				{
					Code = goodsIteBO.BY_GrossWeightUnit,
					Description = goodsIteBO.Lookups.WeightUnitList.GetDescriptionFromCode(goodsIteBO.BY_GrossWeightUnit)
				},
				NetWeight = goodsIteBO.BY_NetWeight,
				NetWeightUnit = new UnitOfWeight
				{
					Code = goodsIteBO.BY_NetWeightUnit,
					Description = goodsIteBO.Lookups.WeightUnitList.GetDescriptionFromCode(goodsIteBO.BY_NetWeightUnit)
				},
				HarmonisedCode = goodsIteBO.BY_HarmonisedTariff
			};
			PopulatePackagesAndContainersData(goodsIteBO, headerData, goodsIteBO.BY_LineNo);

			commercialInvoiceLineData.CustomsSupportingInformationCollection = GetGoodsItemCustomsReferenceCollectionData(goodsIteBO); // Supporting Documents, Additional Infos, Previous Documents
			return commercialInvoiceLineData;
		}

		List<UniversalCustoms.CustomsSupportingInformation> GetGoodsItemCustomsReferenceCollectionData(T goodsItemBO) => goodsItemBO is Integration.Customs.ICusSupportingInfoTypeSupporter ? GetInvoiceCustomsSupportingInformationCollectionCore(goodsItemBO) : null;

		protected virtual List<UniversalCustoms.CustomsSupportingInformation> GetInvoiceCustomsSupportingInformationCollectionCore(T goodsItemBO) => CustomsSupportingInformationCollectionCreator.CreateCollection(helper, goodsItemBO, writeManager);

		protected virtual void PopulatePackagesAndContainersData(T goodsItemBO, Shipment shipmentData, ZInt itemLineNo)
		{
			foreach (NctsPackage packageBO in goodsItemBO.Packages)
			{
				var packingLineData = new PackingLine(writeManager.WriterStrategy)
				{
					PackType = ListHelper.GetWithDescription<PackageType>(packageBO.B5_UnitType, packageBO.Lookups.UnitTypeList),
					PackQty = packageBO.B5_UnitCount,
					MarksAndNos = packageBO.B5_MarksAndNumbers,
					ItemNo = (ZShort)itemLineNo, //Item no this package belongs to
					EntryType = goodsItemBO.MoveHeader.BM_SubApplicationCode // Movement header type this package belongs to
				};
				AddCommercialInvoiceLineLink(packingLineData, itemLineNo);
				shipmentData.PackingLineCollection.Add(packingLineData);
			}
		}

		static void AddCommercialInvoiceLineLink(PackingLine packingLineData, ZInt? itemLineNo)
		{
			packingLineData.SetPackedItemCollection(() =>
			{
				var packedItemCollectionData = new List<PackedItem>();
				var packedItemData = new PackedItem()
				{
					CommercialInvoiceLineLink = itemLineNo
				};
				packedItemCollectionData.Add(packedItemData);
				return packedItemCollectionData;
			});
		}
	}
}
