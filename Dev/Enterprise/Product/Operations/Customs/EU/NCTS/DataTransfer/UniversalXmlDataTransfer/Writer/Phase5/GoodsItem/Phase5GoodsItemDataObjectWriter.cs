using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase5
{
	class Phase5GoodsItemDataObjectWriter : DataObjectWriter<NctsDepartureCargoDesc, InBondMoveLineItem>
	{
		public Phase5GoodsItemDataObjectWriter(IDataWritingManager writeManager, UniversalDataObjectWriterHelper helper)
			: base(writeManager)
		{
			this.helper = Argument.NotNull(helper, nameof(helper));
		}
		protected readonly UniversalDataObjectWriterHelper helper;

		protected override InBondMoveLineItem PopulateDataObject(NctsDepartureCargoDesc goodsItemBo)
		{
			var inBondMoveLineItem = MapGoodsItemToInBondMoveLineItem(goodsItemBo);
			return inBondMoveLineItem;
		}

		InBondMoveLineItem MapGoodsItemToInBondMoveLineItem(NctsDepartureCargoDesc goodsItemBo)
		{
			var lineItem = new InBondMoveLineItem();
			lineItem.SetWriterStrategy(writeManager.WriterStrategy);
			lineItem.LineNumber = goodsItemBo.BY_LineNo;
			lineItem.DescriptionAndQuantityOfMerchandise = goodsItemBo.BY_Description;
			lineItem.Weight = goodsItemBo.BY_GrossWeight;
			lineItem.WeightUnit = new CodeDescriptionPair { Code = goodsItemBo.BY_GrossWeightUnit, Description = goodsItemBo.Lookups.WeightUnitList.GetDescriptionFromCode(goodsItemBo.BY_GrossWeightUnit) };
			lineItem.NetWeight = goodsItemBo.BY_NetWeight;
			lineItem.NetWeightUnit = new UnitOfWeight
			{
				Code = goodsItemBo.BY_NetWeightUnit,
				Description = goodsItemBo.Lookups.WeightUnitList.GetDescriptionFromCode(goodsItemBo.BY_NetWeightUnit),
			};
			lineItem.TariffCode = goodsItemBo.BY_FormattedHarmonisedTariff;
			lineItem.CustomsSecondQuantity = goodsItemBo.BY_CustomsSecondQuantity;
			lineItem.CustomsSecondQuantityUnit = new CodeDescriptionPair4Char
			{
				Code = goodsItemBo.BY_CustomsSecondUnitQty,
				Description = goodsItemBo.Lookups.CustomsUnitOfQuantityList.GetDescriptionFromCode(goodsItemBo.BY_CustomsSecondUnitQty)
			};
			lineItem.DeclarationType = new CodeDescriptionPair { Code = goodsItemBo.BY_Type, Description = goodsItemBo.Lookups.DeclarationTypeList.GetDescriptionFromCode(goodsItemBo.BY_Type) };
			lineItem.CountryOfDispatch = new CodeDescriptionPair2Char { Code = goodsItemBo.BY_RN_NKCountryOfDispatch, Description = goodsItemBo.Lookups.CountryOfDispatchList.GetDescriptionFromCode(goodsItemBo.BY_RN_NKCountryOfDispatch) };
			lineItem.CountryOfDestination = new CodeDescriptionPair2Char { Code = goodsItemBo.BY_RN_NKCountryOfDestination, Description = goodsItemBo.Lookups.CountryOfDestinationList.GetDescriptionFromCode(goodsItemBo.BY_RN_NKCountryOfDestination) };
			lineItem.CountryOfOrigin = new CodeDescriptionPair2Char { Code = goodsItemBo.BY_RN_NKCountryOfOrigin, Description = goodsItemBo.Lookups.CountryOfOriginList.GetDescriptionFromCode(goodsItemBo.BY_RN_NKCountryOfOrigin) };
			lineItem.ReferenceNumber = goodsItemBo.BY_CommercialReferenceNumber;
			lineItem.TransportPaymentMethod = new CodeDescriptionPair { Code = goodsItemBo.BY_TransportChargesMethodOfPayment, Description = goodsItemBo.Lookups.TransportChargesModeOfPaymentList.GetDescriptionFromCode(goodsItemBo.BY_TransportChargesMethodOfPayment) };
			lineItem.HazardousMaterial = new HazardousMaterial()
			{
				Code = goodsItemBo.BY_CusC4Number,
				CodeType = new CodeDescriptionPair5Char
				{
					Code = goodsItemBo.Lookups.CusCodeList?.Select(c => c).FirstOrDefault(x => x.ZZD_Code == goodsItemBo.BY_CusC4Number)?.ZZD_CodeType ?? EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS,
					Description = goodsItemBo.Lookups.CusCodeList?.Select(c => c).FirstOrDefault(x => x.ZZD_Code == goodsItemBo.BY_CusC4Number)?.ZZD_CodeTypeDesc
				},
			};

			lineItem.HazardousMaterial.UNDGCollection = ProcessCollection(goodsItemBo.UNDGs, new UNDGDataObjectWriter(writeManager));
			lineItem.Link = helper.GetGoodsItemLink(goodsItemBo);

			lineItem.CustomsFirstQuantity = goodsItemBo.CustomsFirstQuantityInKilograms;
			lineItem.CustomsFirstQuantityUnit = new UnitOfWeight
			{
				Code = goodsItemBo.CustomsFirstUnitQtyKilograms,
				Description = goodsItemBo.Lookups.CustomsUnitOfQuantityList.GetDescriptionFromCode(goodsItemBo.CustomsFirstUnitQtyKilograms) ??
					goodsItemBo.Lookups.WeightUnitList.GetDescriptionFromCode(goodsItemBo.CustomsFirstUnitQtyKilograms)
			};
			lineItem.CustomsThirdQuantity = goodsItemBo.BY_CustomsThirdQuantity;
			lineItem.CustomsThirdQuantityUnit = new CodeDescriptionPair4Char
			{
				Code = goodsItemBo.BY_CustomsThirdUnitQty,
				Description = goodsItemBo.Lookups.CustomsUnitOfQuantityList.GetDescriptionFromCode(goodsItemBo.BY_CustomsThirdUnitQty)
			};
			lineItem.CustomsFourthQuantity = goodsItemBo.BY_CustomsFourthQuantity;
			lineItem.CustomsFourthQuantityUnit = new CodeDescriptionPair4Char
			{
				Code = goodsItemBo.BY_CustomsFourthUnitQty,
				Description = goodsItemBo.Lookups.CustomsUnitOfQuantityList.GetDescriptionFromCode(goodsItemBo.BY_CustomsFourthUnitQty)
			};
			lineItem.MonetaryValue = goodsItemBo.BY_MonetaryValue;
			lineItem.MonetaryValueCurrency = new CodeDescriptionPair
			{
				Code = goodsItemBo.BY_RX_NKCurrency,
				Description = goodsItemBo.Lookups.Currencies?.Select(c => c).FirstOrDefault(x => x.Code == goodsItemBo.BY_RX_NKCurrency)?.RX_Desc
			};
			lineItem.TaxType = new CodeDescriptionPair
			{
				Code = goodsItemBo.BY_ZZF_NKTaxType,
				Description = goodsItemBo.Lookups.TaxOrFeeCodeList.GetDescriptionFromCode(goodsItemBo.BY_ZZF_NKTaxType)
			};
			lineItem.LinePrice = goodsItemBo.BY_LinePrice;
			lineItem.LinePriceCurrency = new CodeDescriptionPair
			{
				Code = goodsItemBo.BY_RX_NKLinePriceCurrency,
				Description = goodsItemBo.LinePriceCurrency?.RX_Desc
			};
			lineItem.SetCustomsSupportingInformationCollection(() => CustomsSupportingInformationCollectionCreator.CreateCollection(helper, goodsItemBo, writeManager, ZString.Empty, ((ICusSupportingInfoTypeSupporter)goodsItemBo).GetCusSupportingInfoTypes().Keys.ToArray()));
			lineItem.SetCustomsReferenceCollection(() => SetCustomsReferences(goodsItemBo));
			var organizationAddresses = new List<OrganizationAddress>();
			AddIfNotNull(goodsItemBo.Consignee, DocAddressType.ConsigneeAddress, organizationAddresses);
			lineItem.SetOrganizationAddressCollection(() => organizationAddresses);
			return lineItem;
		}

		void AddIfNotNull(JobDocAddress jobDocAddress, DocAddressType docAddressType, List<OrganizationAddress> organizationAddresses)
		{
			var jobDocAddressDataObjectWriter = new JobDocAddressDataObjectWriter(writeManager);
			var organizationAddress = jobDocAddressDataObjectWriter.GetDataObject(jobDocAddress);
			if (organizationAddress != null)
			{
				organizationAddress.AddressType = docAddressType.ToString();
				organizationAddresses.Add(organizationAddress);
			}
		}

		List<CustomsReference> SetCustomsReferences(NctsDepartureCargoDesc goodsItemBo)
		{
			var result = new List<CustomsReference>();
			var rangeToAdd = CustomsReferenceCollectionCreator.CreateCollection(helper, goodsItemBo, writeManager);
			if (rangeToAdd != null)
			{
				result.AddRange(rangeToAdd);
			}
			return result;
		}
	}
}
