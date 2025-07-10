namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class TableStrategyExtractContainerisedORGTest : TableStrategyExtractBaseTest
	{
		#region Quotation with similar rateEntries

		//	ContainerisedTableStrategy is only for Freight charges, not Origin and Destination i.e. OriginDestinationAndChargeableTableRows, OriginDestinationAndContainerTableRows
		protected override string TestExtract_Quotation_SimilarRateEntries_ContractNumberExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_PaymentTermExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_CommodityExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_TransitTimeExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_MatchContainerRateClassExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_ServiceLevelExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_CarrierServiceLevelExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_CartageDeliveryAddressPostCodeExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_CartagePickupAddressPostCodeExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_ViaExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_SupplierExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_TransportProviderExpectedResult => "";
		protected override string TestExtract_Quotation_SimilarRateEntries_ControllingCustomerExpectedResult => "";

		#endregion

		#region Quotation and ClientRate with similar RateEntries

		// ChargeableTableStrategy is used in ChargeableTableRows that is only for Freight Charges (not Origin/Destination Charges)

		#region RateEntry field trigger new page

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierOrganization_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']

[Page 1]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_Commodity_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-HAZ-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']

[Page 1]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ServiceLevel_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry DEF-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']

[Page 1]

  [Row 0]
    [Entry DIR-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']";

		#endregion

		//TODO: 120 is missing
		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_MatchContainerRateClass_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '140.00 Flat']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ClientContractNumber_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT1]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT2]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_Frequency_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_FrequencyUnit_ExpectedResults =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: 1 per Fortnight]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult => TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult;

		#endregion

		protected override string TestExtract_MatchContainerRateClassComplexExpectedResult => "";

		protected override string TestExtract_ContainerExpectedResult => "";

		protected override string Category => Rating.Business.RatingConstants.RateCategory.ORG;
		protected override string Mode => Core.Constants.RateMode.FCL;
		protected override string ChargeCode => "ODOC";

		protected override BaseTableStrategy Strategy => strategy ?? (strategy = new ContainerisedTableStrategy(Factory));
		ContainerisedTableStrategy strategy;
	}
}
