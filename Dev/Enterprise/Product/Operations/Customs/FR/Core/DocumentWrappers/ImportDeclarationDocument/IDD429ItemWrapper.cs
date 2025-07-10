using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDD429ItemWrapper : DocBaseWrapper, IIDDItem<IDD429LiquidationWrapper, IDD429DutiesAndTaxWrapper>
{
	public static IDD429ItemWrapper New(ZInt itemNumber, MGoodsShipmentItemType04FR goodsShipmentItem, GoodsShipmentItemType goodsShipmentItemFromDetailedTaxation, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factoryToWrap)
	{
		return new IDD429ItemWrapper(itemNumber, goodsShipmentItem, goodsShipmentItemFromDetailedTaxation, dutiesAndTaxesSummaries, factoryToWrap);
	}

	internal static IDD429ItemWrapper New(ZInt itemNumber, MGoodsShipmentItemType04FR goodsShipmentItem, BusinessObjectFactory factoryToWrap)
	{
		return New(itemNumber, goodsShipmentItem, new GoodsShipmentItemType(), new List<DutiesAndTaxesSummariesType>(), factoryToWrap);
	}

	public IDD429ItemWrapper(ZInt itemNumber, MGoodsShipmentItemType04FR goodsShipmentItem, GoodsShipmentItemType goodsShipmentItemFromDetailedTaxation, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factory) : base(goodsShipmentItem, factory)
	{
		this.goodsShipmentItem = Argument.NotNull(goodsShipmentItem, nameof(goodsShipmentItem));
		this.goodsShipmentItemFromDetailedTaxation = goodsShipmentItemFromDetailedTaxation;
		this.dutiesAndTaxesSummaries = dutiesAndTaxesSummaries;
		this.itemNumber = itemNumber;
	}

	internal IDD429ItemWrapper(ZInt itemNumber, MGoodsShipmentItemType04FR goodsShipmentItem, BusinessObjectFactory factory)
		: this(itemNumber, goodsShipmentItem, new GoodsShipmentItemType(), new List<DutiesAndTaxesSummariesType>(), factory)
	{
	}

	readonly MGoodsShipmentItemType04FR goodsShipmentItem;
	readonly GoodsShipmentItemType goodsShipmentItemFromDetailedTaxation;
	readonly ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries;
	readonly ZInt itemNumber;

	public ZString Procedure
	{
		get
		{
			var procedure = goodsShipmentItem.Procedure;
			var result = new[] { procedure.RequestedProcedure }.Concat(new[] { procedure.PreviousProcedure });
			if(procedure.AdditionalProcedure != null)
			{
				result = result.Concat(procedure.AdditionalProcedure.Select(p => p.AdditionalProcedure));
			}

			return string.Join(" ", result);
		}
	}

	public ZString ItemAmountInvoiced => goodsShipmentItem.Commodity.InvoiceLine?.ItemAmountInvoiced.ToString() ?? ZString.Empty; 

	public ZString SupplementaryUnits => goodsShipmentItem.Commodity.GoodsMeasure?.SupplementaryUnits.ToString() ?? ZString.Empty;

	public ZString GrossMass => goodsShipmentItem.Commodity.GoodsMeasure?.GrossMass.ToString() ?? ZString.Empty;

	public ZString NetMass => goodsShipmentItem.Commodity.GoodsMeasure?.NetMass.ToString() ?? ZString.Empty;

	public ZString NatureOfTransaction => goodsShipmentItem.NatureOfTransaction;

	public ZString AdditionsAndDeductions
	{
		get
		{
			var result = ZString.Empty;
			if (goodsShipmentItem.CustomsValuation?.AdditionsAndDeductions != null)
			{
				var additionsAndDeductionList = goodsShipmentItem.CustomsValuation.AdditionsAndDeductions.Select(deduction => $"{deduction.Code} {deduction.Amount} {deduction.Currency}").ToList();
				result = string.Join(", ", additionsAndDeductionList);
			}
			return result;
		}
	}

	public ZString TariffCode
	{
		get
		{
			var result = new ZString();
			var commodityCode = goodsShipmentItem.Commodity.CommodityCode;
			if (commodityCode != null)
			{
				result = commodityCode.HarmonizedSystemSubheadingCode + commodityCode.CombinedNomenclatureCode + commodityCode.TaricCode;
			}

			return result;
		}
	}

	public ZString CusCode => goodsShipmentItem.Commodity.CUSCode;

	public ZString GoodsDescription => goodsShipmentItem.Commodity.DescriptionOfGoods;

	public ZString TaricAdditionalCode => goodsShipmentItem.Commodity.CommodityCode?.TaricAdditionalCode == null ? ZString.Empty : string.Join(" - ", goodsShipmentItem.Commodity.CommodityCode.TaricAdditionalCode.Select(p => p.TaricAdditionalCode));

	public ZString NationalAdditionalCode => goodsShipmentItem.Commodity.CommodityCode?.NationalAdditionalCode == null ? ZString.Empty : string.Join(" - ", goodsShipmentItem.Commodity.CommodityCode.NationalAdditionalCode.Select(p => p.NationalAdditionalCode));

	public ZString CountryOfOrigin => goodsShipmentItem.Origin?.CountryOfOrigin ?? ZString.Empty;

	public ZString CountryOfPrefentialOrigin => goodsShipmentItem.Origin?.CountryOfPreferentialOrigin ?? ZString.Empty;

	public ZString CountryOfDestination => goodsShipmentItem.Destination?.CountryOfDestination ?? ZString.Empty;

	public ZString CountryOfDispatch => goodsShipmentItem?.CountryOfDispatch?.CountryOfDispatch ?? ZString.Empty;

	public ZString Preference => goodsShipmentItem.Commodity.CalculationOfTaxes?.Preference ?? ZString.Empty;

	public ZString StatisticalValue => goodsShipmentItem.StatisticalValue.ToString();

	public ZString ExporterName => goodsShipmentItem.Exporter?.Name ?? ZString.Empty;

	public ZString ExporterEORI => goodsShipmentItem.Exporter?.IdentificationNumber ?? ZString.Empty;

	public ZString ExporterAddress => goodsShipmentItem.Exporter?.Address?.StreetAndNumber ?? ZString.Empty;

	public ZString ExporterPostCode => goodsShipmentItem.Exporter?.Address?.Postcode ?? ZString.Empty;

	public ZString ExporterCity => goodsShipmentItem.Exporter?.Address?.City ?? ZString.Empty;

	public ZString ExporterCountry => goodsShipmentItem.Exporter?.Address?.Country ?? ZString.Empty;

	public ZString BuyerName => goodsShipmentItem.Buyer?.Name ?? ZString.Empty;

	public ZString BuyerCode => ZString.Empty;

	public ZString BuyerEORI => goodsShipmentItem.Buyer?.IdentificationNumber ?? ZString.Empty;

	public ZString BuyerAddress => goodsShipmentItem.Buyer?.Address?.StreetAndNumber ?? ZString.Empty;

	public ZString BuyerPostCode => goodsShipmentItem.Buyer?.Address?.Postcode ?? ZString.Empty;

	public ZString BuyerCity => goodsShipmentItem.Buyer?.Address?.City ?? ZString.Empty;

	public ZString BuyerCountry => goodsShipmentItem.Buyer?.Address?.Country ?? ZString.Empty;

	public ZString ConsigneeName => goodsShipmentItem.Consignee?.Name ?? ZString.Empty;

	public ZString ConsigneeEORI => goodsShipmentItem.Consignee?.IdentificationNumber ?? ZString.Empty;

	public ZString ConsigneeAddress => goodsShipmentItem.Consignee?.Address.StreetAndNumber ?? ZString.Empty;

	public ZString ConsigneePostCode => goodsShipmentItem.Consignee?.Address.Postcode ?? ZString.Empty;

	public ZString ConsigneeCity => goodsShipmentItem.Consignee?.Address?.City ?? ZString.Empty;

	public ZString ConsigneeCountry => goodsShipmentItem.Consignee?.Address?.Country ?? ZString.Empty;

	public ZString SellerName => goodsShipmentItem.Seller?.Name ?? ZString.Empty;

	public ZString SellerEORI => goodsShipmentItem.Seller?.IdentificationNumber ?? ZString.Empty;

	public ZString SellerAddress => goodsShipmentItem.Seller?.Address.StreetAndNumber ?? ZString.Empty;

	public ZString SellerPostCode => goodsShipmentItem.Seller?.Address.Postcode ?? ZString.Empty;

	public ZString SellerCity => goodsShipmentItem.Seller?.Address.City ?? ZString.Empty;

	public ZString SellerCountry => goodsShipmentItem.Seller?.Address.Country ?? ZString.Empty;

	public ZString SupplyChainActorEORI => goodsShipmentItem.AdditionalSupplyChainActor?.FirstOrDefault()?.IdentificationNumber ?? ZString.Empty;

	public ZString SupplyChainActorRole => goodsShipmentItem.AdditionalSupplyChainActor?.FirstOrDefault()?.Role ?? ZString.Empty;

	public ZString AuthorizationType => goodsShipmentItem.Authorisation?.FirstOrDefault()?.Type ?? ZString.Empty;

	public ZString AuthorizationNumber => goodsShipmentItem.Authorisation?.FirstOrDefault()?.ReferenceNumber ?? ZString.Empty;

	public ZString AuthorizationHolder => goodsShipmentItem.Authorisation?.FirstOrDefault()?.HolderOfTheAuthorisation ?? ZString.Empty;

	public ZString SpecialMentions => goodsShipmentItem.AdditionalInformation == null ? ZString.Empty : string.Join(" - ", goodsShipmentItem.AdditionalInformation.Select(p => p.Code));

	public ZString FiscalReferences => goodsShipmentItem.AdditionalFiscalReference == null ? ZString.Empty : string.Join(" - ", goodsShipmentItem.AdditionalFiscalReference.Select(p => p.FiscalReferenceIdentificationNumber));

	public ZString AdditionalReferences => goodsShipmentItem.AdditionalReference == null ? ZString.Empty : string.Join(" - ", goodsShipmentItem.AdditionalReference.Select(p => string.Join(" ", p.Type, p.ReferenceNumber)));

	public ZString PreviousDocuments => goodsShipmentItem.PreviousDocument == null ? ZString.Empty : string.Join(" - ", goodsShipmentItem.PreviousDocument.Select(p => string.Join(" ", p.Type, p.ReferenceNumber)));

	public ZString SupportingDocuments => goodsShipmentItem.SupportingDocument == null ? ZString.Empty : string.Join(" - ", goodsShipmentItem.SupportingDocument.Select(p => string.Join(" ", p.Type, p.ReferenceNumber)));

	public ZString TransportDocuments => goodsShipmentItem.TransportDocument == null ? ZString.Empty : string.Join(" - ", goodsShipmentItem.TransportDocument.Select(p => string.Join(" ", p.Type, p.ReferenceNumber)));

	public ZString TypeOfPackage => goodsShipmentItem.Packaging == null ? ZString.Empty : string.Join(" / ", goodsShipmentItem.Packaging.Select(p => p.TypeOfPackages));

	public ZString NumberOfPagkage => goodsShipmentItem.Packaging == null ? ZString.Empty : goodsShipmentItem.Packaging.Sum(p => ZInt.ParseSafe(p.NumberOfPackages, 0)).ToString();

	public ZString ShippingMarks => goodsShipmentItem.Packaging == null ? ZString.Empty : string.Join(" / ", goodsShipmentItem.Packaging.Select(p => p.ShippingMarks));

	public IDD429LiquidationWrapper Liquidation => liquidation ?? (liquidation = GetLiquidation());
	IDD429LiquidationWrapper liquidation;

	IDD429LiquidationWrapper GetLiquidation() => IDD429LiquidationWrapper.New(goodsShipmentItemFromDetailedTaxation, dutiesAndTaxesSummaries, Factory);

	public ZString ItemNumber => itemNumber.ToString();

	public ZString CustomsValue => ZString.Empty;
	public ZString VatBase => ZString.Empty;

	public ZString SupplyChainActorName => ZString.Empty;

	public ZString UnguaranteedAmount => ZString.Empty;

	public ZString GuaranteedAmount => ZString.Empty;

	public ZString AmountToBeCovered => ZString.Empty;

	public ZString TotalPayableAmount => ZString.Empty;

	public ZString PayableTaxAmount => goodsShipmentItem.Commodity?.CalculationOfTaxes?.TotalDutiesAndTaxesAmount.ToString() ?? ZString.Empty;
}
