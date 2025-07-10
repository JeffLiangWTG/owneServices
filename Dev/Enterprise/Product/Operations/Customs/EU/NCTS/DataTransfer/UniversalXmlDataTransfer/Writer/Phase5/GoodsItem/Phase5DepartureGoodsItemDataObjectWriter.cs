using CargoWise.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5
{
	class Phase5DepartureGoodsItemDataObjectWriter : DataObjectWriter<NctsDepartureCargoDesc, CommercialInvoiceHeader>
	{
		public Phase5DepartureGoodsItemDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, CommercialInvoiceHeader commercialInvoiceHeaderData)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, nameof(helper));
			this.commercialInvoiceHeaderData = commercialInvoiceHeaderData;
		}
		protected readonly UniversalDataObjectWriterHelper helper;
		readonly CommercialInvoiceHeader commercialInvoiceHeaderData;

		protected sealed override CommercialInvoiceHeader PopulateDataObject(NctsDepartureCargoDesc goodsItemBo)
		{
			var commercialInvoiceLineData = PopulateCommercialInvoiceLineData(goodsItemBo);
			commercialInvoiceHeaderData.CommercialInvoiceLineCollection.Add(commercialInvoiceLineData);
			return commercialInvoiceHeaderData;
		}

		protected CommercialInvoiceLine PopulateCommercialInvoiceLineData(NctsDepartureCargoDesc goodsItemBo)
		{
			var line = new CommercialInvoiceLine(writeManager.WriterStrategy)
			{
				LineNo = goodsItemBo.BY_LineNo,
				Description = goodsItemBo.BY_Description,
				Weight = goodsItemBo.BY_GrossWeight,
				WeightUnit = new UnitOfWeight
				{
					Code = goodsItemBo.BY_GrossWeightUnit,
					Description = goodsItemBo.Lookups.WeightUnitList.GetDescriptionFromCode(goodsItemBo.BY_GrossWeightUnit),
				},
				NetWeight = goodsItemBo.BY_NetWeight,
				NetWeightUnit = new UnitOfWeight
				{
					Code = goodsItemBo.BY_NetWeightUnit,
					Description = goodsItemBo.Lookups.WeightUnitList.GetDescriptionFromCode(goodsItemBo.BY_NetWeightUnit),
				},
				HarmonisedCode = goodsItemBo.BY_HarmonisedTariff,
				CountryOfOrigin = new Country { Code = goodsItemBo.BY_RN_NKCountryOfOrigin, Name = helper.GetCountryDescriptionFromCode(goodsItemBo.BY_RN_NKCountryOfOrigin) },
				CountryOfExport = new Country { Code = goodsItemBo.BY_RN_NKCountryOfDispatch, Name = helper.GetCountryDescriptionFromCode(goodsItemBo.BY_RN_NKCountryOfDispatch) },

				CustomsSecondQuantity = goodsItemBo.BY_CustomsSecondQuantity,
				CustomsSecondQuantityUnit = new CodeDescriptionPair6Char { Code = goodsItemBo.BY_CustomsSecondUnitQty, Description = goodsItemBo.Lookups.CustomsUnitOfQuantityList.GetDescriptionFromCode(goodsItemBo.BY_CustomsSecondUnitQty) },
				CustomsValue = goodsItemBo.BY_MonetaryValue,
			};
			if (goodsItemBo is
				{
					BY_WarehouseEntryLineNo.IsEmpty: false,
					BY_WarehouseEntryNumber.IsEmpty: false,
					BY_BondedWhsQuantity.IsEmpty: false,
				})
			{
				line.BondedWarehouseQuantity = goodsItemBo.BY_BondedWhsQuantity;
				line.BondedWarehouseQuantityUnit = new CodeDescriptionPair() { Code = goodsItemBo.BY_BondedWhsUnitQty, Description = goodsItemBo.Lookups.BondedWhsUnitQtyList.GetDescriptionFromCode(goodsItemBo.BY_BondedWhsUnitQty) };
				line.PartNo = goodsItemBo.Part?.OP_PartNum;
				line.PreviousEntryNumber = goodsItemBo.BY_WarehouseEntryNumber;
				line.PreviousEntryLineNumber = goodsItemBo.BY_WarehouseEntryLineNo;
				line.EntryLineNumber = goodsItemBo.BY_LineNo;
				line.EntryNumber = goodsItemBo.BY_CommercialReferenceNumber;
			}
			return line;
		}
	}
}
