using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument
{
	public interface IIDDItem<TIDDLiquidationWrapper, TIDDDutiesAndTaxesWrapper>
		where TIDDLiquidationWrapper : DocBaseWrapper, IIDDLiquidation<TIDDDutiesAndTaxesWrapper>
		where TIDDDutiesAndTaxesWrapper : DocBaseWrapper, IIDDDutiesAndTax
	{
		ZString Procedure { get; }
		ZString ItemAmountInvoiced { get; }
		ZString SupplementaryUnits { get; }
		ZString GrossMass { get; }
		ZString NetMass { get; }
		ZString NatureOfTransaction { get; }
		ZString AdditionsAndDeductions { get; }
		ZString TariffCode { get; }
		ZString CusCode { get; }
		ZString GoodsDescription { get; }
		ZString TaricAdditionalCode { get; }
		ZString NationalAdditionalCode { get; }
		ZString CountryOfOrigin { get; }
		ZString CountryOfDestination { get; }
		ZString CountryOfDispatch { get; }
		ZString CountryOfPrefentialOrigin { get; }
		ZString Preference { get; }
		ZString CustomsValue { get; }
		ZString StatisticalValue { get; }
		ZString VatBase { get; }
		ZString ExporterName { get; }
		ZString ExporterEORI { get; }
		ZString ExporterAddress { get; }
		ZString ExporterPostCode { get; }
		ZString ExporterCity { get; }
		ZString ExporterCountry { get; }
		ZString BuyerName { get; }
		ZString BuyerCode { get; }
		ZString BuyerEORI { get; }
		ZString BuyerAddress { get; }
		ZString BuyerPostCode { get; }
		ZString BuyerCity { get; }
		ZString BuyerCountry { get; }
		ZString ConsigneeName { get; }
		ZString ConsigneeEORI { get; }
		ZString ConsigneeAddress { get; }
		ZString ConsigneePostCode { get; }
		ZString ConsigneeCity { get; }
		ZString ConsigneeCountry { get; }
		ZString SellerName { get; }
		ZString SellerEORI { get; }
		ZString SellerAddress { get; }
		ZString SellerPostCode { get; }
		ZString SellerCity { get; }
		ZString SellerCountry { get; }
		ZString SupplyChainActorName { get; }
		ZString SupplyChainActorEORI { get; }
		ZString SupplyChainActorRole { get; }
		ZString AuthorizationType { get; }
		ZString AuthorizationNumber { get; }
		ZString AuthorizationHolder { get; }
		ZString SpecialMentions { get; }
		ZString FiscalReferences { get; }
		ZString AdditionalReferences { get; }
		ZString PreviousDocuments { get; }
		ZString SupportingDocuments { get; }
		ZString TransportDocuments { get; }
		ZString TypeOfPackage { get; }
		ZString NumberOfPagkage { get; }
		ZString ShippingMarks { get; }
		ZString ItemNumber { get; }
		ZString UnguaranteedAmount { get; }
		ZString GuaranteedAmount { get; }
		ZString AmountToBeCovered { get; }
		ZString TotalPayableAmount { get; }
		ZString PayableTaxAmount { get; }
		TIDDLiquidationWrapper Liquidation { get; }
	}
}
