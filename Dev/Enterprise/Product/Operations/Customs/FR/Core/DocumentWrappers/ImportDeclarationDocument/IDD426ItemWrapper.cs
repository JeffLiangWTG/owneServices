using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDD426ItemWrapper : DocBaseWrapper, IIDDItem<IDD426LiquidationWrapper, IDD426DutiesAndTaxWrapper>
{
	public static IDD426ItemWrapper New(ZInt itemNumber, MGoodsShipmentItemType05FR goodsShipmentItem, GoodsShipmentItemType goodsShipmentItemFromDetailedTaxation, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factoryToWrap)
	{
		return new IDD426ItemWrapper(itemNumber, goodsShipmentItem, goodsShipmentItemFromDetailedTaxation, dutiesAndTaxesSummaries, factoryToWrap);
	}

	internal static IDD426ItemWrapper New(ZInt itemNumber, MGoodsShipmentItemType05FR goodsShipmentItem, BusinessObjectFactory factoryToWrap)
	{
		return New(itemNumber, goodsShipmentItem, new GoodsShipmentItemType(), new List<DutiesAndTaxesSummariesType>(), factoryToWrap);
	}

	public IDD426ItemWrapper(ZInt itemNumber, MGoodsShipmentItemType05FR goodsShipmentItem, GoodsShipmentItemType goodsShipmentItemFromDetailedTaxation, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factory) : base(goodsShipmentItem, factory)
	{
		this.goodsShipmentItem = Argument.NotNull(goodsShipmentItem, nameof(goodsShipmentItem));
		this.goodsShipmentItemFromDetailedTaxation = goodsShipmentItemFromDetailedTaxation;
		this.dutiesAndTaxesSummaries = dutiesAndTaxesSummaries;
		this.itemNumber = itemNumber;
	}

	internal IDD426ItemWrapper(ZInt itemNumber, MGoodsShipmentItemType05FR goodsShipmentItem, BusinessObjectFactory factory)
		: this(itemNumber, goodsShipmentItem, new GoodsShipmentItemType(), new List<DutiesAndTaxesSummariesType>(), factory)
	{
	}

	readonly MGoodsShipmentItemType05FR goodsShipmentItem;
	readonly GoodsShipmentItemType goodsShipmentItemFromDetailedTaxation;
	readonly ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries;
	readonly ZInt itemNumber;

	public ZString ItemAmountInvoiced => goodsShipmentItem.Commodity?.InvoiceLine?.ItemAmountInvoiced.ToString() ?? ZString.Empty;

	public ZString StatisticalValue => goodsShipmentItem.StatisticalValue.ToString();

	public IDD426LiquidationWrapper Liquidation => liquidation ?? (liquidation = GetLiquidation());
	IDD426LiquidationWrapper liquidation;

	IDD426LiquidationWrapper GetLiquidation() => IDD426LiquidationWrapper.New(goodsShipmentItemFromDetailedTaxation, dutiesAndTaxesSummaries, Factory);

	public ZString ItemNumber => itemNumber.ToString();

	public ZString Procedure => ZString.Empty;

	public ZString SupplementaryUnits => ZString.Empty;

	public ZString GrossMass => ZString.Empty;

	public ZString NetMass => ZString.Empty;

	public ZString NatureOfTransaction => ZString.Empty;

	public ZString AdditionsAndDeductions => ZString.Empty;

	public ZString TariffCode => ZString.Empty;

	public ZString CusCode => ZString.Empty;

	public ZString GoodsDescription => ZString.Empty;

	public ZString TaricAdditionalCode => ZString.Empty;

	public ZString NationalAdditionalCode => ZString.Empty;

	public ZString CountryOfOrigin => ZString.Empty;

	public ZString CountryOfPrefentialOrigin => ZString.Empty;

	public ZString CountryOfDestination => ZString.Empty;

	public ZString CountryOfDispatch => ZString.Empty;

	public ZString Preference => ZString.Empty;

	public ZString CustomsValue => ZString.Empty;

	public ZString VatBase => ZString.Empty;

	public ZString ExporterName => ZString.Empty;

	public ZString ExporterEORI => ZString.Empty;

	public ZString ExporterAddress => ZString.Empty;

	public ZString ExporterPostCode => ZString.Empty;

	public ZString ExporterCity => ZString.Empty;

	public ZString ExporterCountry => ZString.Empty;

	public ZString BuyerName => ZString.Empty;

	public ZString BuyerCode => ZString.Empty;

	public ZString BuyerEORI => ZString.Empty;

	public ZString BuyerAddress => ZString.Empty;

	public ZString BuyerPostCode => ZString.Empty;

	public ZString BuyerCity => ZString.Empty;

	public ZString BuyerCountry => ZString.Empty;

	public ZString ConsigneeName => ZString.Empty;

	public ZString ConsigneeEORI => ZString.Empty;

	public ZString ConsigneeAddress => ZString.Empty;

	public ZString ConsigneePostCode => ZString.Empty;

	public ZString ConsigneeCity => ZString.Empty;

	public ZString ConsigneeCountry => ZString.Empty;

	public ZString SellerName => ZString.Empty;

	public ZString SellerEORI => ZString.Empty;

	public ZString SellerAddress => ZString.Empty;

	public ZString SellerPostCode => ZString.Empty;

	public ZString SellerCity => ZString.Empty;

	public ZString SellerCountry => ZString.Empty;

	public ZString SupplyChainActorName => ZString.Empty;

	public ZString SupplyChainActorEORI => ZString.Empty;

	public ZString SupplyChainActorRole => ZString.Empty;

	public ZString AuthorizationType => ZString.Empty;

	public ZString AuthorizationNumber => ZString.Empty;

	public ZString AuthorizationHolder => ZString.Empty;

	public ZString SpecialMentions => ZString.Empty;

	public ZString FiscalReferences => ZString.Empty;

	public ZString AdditionalReferences => ZString.Empty;

	public ZString PreviousDocuments => ZString.Empty;

	public ZString SupportingDocuments => ZString.Empty;

	public ZString TransportDocuments => ZString.Empty;

	public ZString TypeOfPackage => ZString.Empty;

	public ZString NumberOfPagkage => ZString.Empty;

	public ZString ShippingMarks => ZString.Empty;

	public ZString UnguaranteedAmount => ZString.Empty;

	public ZString GuaranteedAmount => ZString.Empty;

	public ZString AmountToBeCovered => ZString.Empty;

	public ZString TotalPayableAmount => ZString.Empty;

	public ZString PayableTaxAmount => ZString.Empty;
}
