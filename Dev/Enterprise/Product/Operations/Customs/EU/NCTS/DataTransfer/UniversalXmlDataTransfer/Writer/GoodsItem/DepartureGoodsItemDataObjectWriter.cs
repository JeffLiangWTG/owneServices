using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public class DepartureGoodsItemDataObjectWriter : GoodsItemDataObjectWriter<NctsDepartureCargoDesc>
	{
		public DepartureGoodsItemDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, Shipment headerData, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceHeader commercialInvoiceHeaderData)
			: base(manager, helper, headerData, commercialInvoiceHeaderData)
		{
		}

		protected override UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceLine PopulateCommercialInvoiceLineData(NctsDepartureCargoDesc goodsItemBO)
		{
			var commercialInvoiceLineData = base.PopulateCommercialInvoiceLineData(goodsItemBO);
			commercialInvoiceLineData.HarmonisedCode = goodsItemBO.BY_HarmonisedTariff;
			commercialInvoiceLineData.CountryOfOrigin = new Country { Code = goodsItemBO.BY_RN_NKCountryOfOrigin, Name = helper.GetCountryDescriptionFromCode(goodsItemBO.BY_RN_NKCountryOfOrigin) };
			commercialInvoiceLineData.StateOfOrigin = State.NewOrEmpty(goodsItemBO.OriginState);
			commercialInvoiceLineData.CountryOfExport = new Country { Code = goodsItemBO.BY_RN_NKCountryOfDispatch, Name = helper.GetCountryDescriptionFromCode(goodsItemBO.BY_RN_NKCountryOfDispatch) };
			commercialInvoiceLineData.CustomsSecondQuantity = goodsItemBO.BY_CustomsSecondQuantity;
			commercialInvoiceLineData.CustomsSecondQuantityUnit = new CodeDescriptionPair6Char { Code = goodsItemBO.BY_CustomsSecondUnitQty, Description = goodsItemBO.Lookups.CustomsUnitOfQuantityList.GetDescriptionFromCode(goodsItemBO.BY_CustomsSecondUnitQty) };
			commercialInvoiceLineData.CustomsValue = goodsItemBO.BY_MonetaryValue;
			commercialInvoiceLineData.TaxType = new CodeDescriptionPair4Char { Code = goodsItemBO.BY_ZZF_NKTaxType, Description = goodsItemBO.Lookups.TaxOrFeeCodeList.GetDescriptionFromCode(goodsItemBO.BY_ZZF_NKTaxType) };
			commercialInvoiceLineData.Procedure = goodsItemBO.BY_Procedure;

			PopulateAddInfosData(goodsItemBO, commercialInvoiceLineData);
			PopulateJobDocAddresses(goodsItemBO, commercialInvoiceLineData);
			PopulateCustomsReferenceCollection(goodsItemBO, commercialInvoiceLineData);
			return commercialInvoiceLineData;
		}

		protected override void PopulatePackagesAndContainersData(NctsDepartureCargoDesc goodsItemBO, Shipment shipmentData, ZInt itemLineNo)
		{
			if (goodsItemBO.ContainersSelected.Count > 0)
			{
				AddContainerLink(goodsItemBO, shipmentData.PackingLineCollection, itemLineNo);
			}

			base.PopulatePackagesAndContainersData(goodsItemBO, shipmentData, itemLineNo);
		}

		void AddContainerLink(NctsDepartureCargoDesc goodsItemBO, DataObjectList<PackingLine> packingLineCollection, ZInt itemLineNo)
		{
			foreach (var containerNumberForThisGoodItem in goodsItemBO.ContainersSelected)
			{
				var packingLineData = new PackingLine(writeManager.WriterStrategy)
				{
					ContainerNumber = containerNumberForThisGoodItem,
					ItemNo = (ZShort)itemLineNo, //Item no this package belongs to
					EntryType = goodsItemBO.MoveHeader.BM_SubApplicationCode // Movement header type this package belongs to
				};
				AddCommercialInvoiceLineLink(packingLineData, itemLineNo);
				packingLineCollection.Add(packingLineData);
			}
		}

		void AddCommercialInvoiceLineLink(PackingLine packingLineData, ZInt? itemLineNo)
		{
			packingLineData.SetPackedItemCollection(() =>
			{
				var packedItemCollectionData = new List<PackedItem>();
				var packedItemData = new PackedItem
				{
					CommercialInvoiceLineLink = itemLineNo
				};
				packedItemCollectionData.Add(packedItemData);
				return packedItemCollectionData;
			});
		}

		void PopulateAddInfosData(NctsDepartureCargoDesc goodsItemBO, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceLine commercialInvoiceLineData)
		{
			var list = new List<AddInfo>();

			AddKeyValueItemToList(DataObjectWriterConstants.GoodsItem.AddInfo.DeclarationType, goodsItemBO.BY_Type, list);
			AddKeyValueItemToList(DataObjectWriterConstants.GoodsItem.AddInfo.CommercialReferenceNumber, goodsItemBO.BY_CommercialReferenceNumber, list);
			AddKeyValueItemToList(DataObjectWriterConstants.GoodsItem.AddInfo.TransportChargesMoP, goodsItemBO.BY_TransportChargesMethodOfPayment, list);
			AddKeyValueItemToList(DataObjectWriterConstants.GoodsItem.AddInfo.UNDangerousGoodsCode, goodsItemBO.UNDangerousGoodsCode, list);
			AddKeyValueItemToList(DataObjectWriterConstants.GoodsItem.AddInfo.UNDangerousGoodsStandard, goodsItemBO.UNDangerousGoodsStandard, list);
			AddKeyValueItemToList(DataObjectWriterConstants.GoodsItem.AddInfo.CountryOfDestination, goodsItemBO.BY_RN_NKCountryOfDestination, list);

			commercialInvoiceLineData.AddInfoCollection = list;

			if (commercialInvoiceLineData.AddInfoCollection.Count == 0)
			{
				commercialInvoiceLineData.AddInfoCollection = null;
			}
		}

		void AddKeyValueItemToList(ZString key, ZString value, List<AddInfo> list)
		{
			if (!value.IsEmpty && !value.IsDefault)
			{
				list.Add(new AddInfo()
				{
					Key = key,
					Value = value
				});
			}
		}

		void PopulateJobDocAddresses(NctsCommonCargoDesc goodsItemBO, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceLine commercialInvoiceLineData)
		{
			commercialInvoiceLineData.OrganizationAddressCollection = ProcessCollection(((IDocAddresses)goodsItemBO).DocAddresses, new JobDocAddressDataObjectWriter(writeManager));
		}

		void PopulateCustomsReferenceCollection(NctsDepartureCargoDesc goodsItemBO, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceLine commercialInvoiceLineData)
		{
			commercialInvoiceLineData.CustomsReferenceCollection = CustomsReferenceCollectionCreator.CreateCollection(helper, goodsItemBO, writeManager);
		}
	}
}
