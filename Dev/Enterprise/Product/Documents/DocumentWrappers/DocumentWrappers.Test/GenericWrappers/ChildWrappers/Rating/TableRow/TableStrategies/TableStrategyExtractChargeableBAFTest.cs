namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class TableStrategyExtractChargeableBAFTest : TableStrategyExtractBaseTest
	{
		#region Quotation with similar rateEntries

		protected override string TestExtract_Quotation_SimilarRateEntries_ContractNumberExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '201.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '401.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT2]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '202.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '402.00']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_PaymentTermExpectedResult => TestExtract_RateEntryCommonExpectedResult;

		protected override string TestExtract_Quotation_SimilarRateEntries_CommodityExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '201.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '401.00']

[Row 1]
  [Entry [Empty Service Level]-HAZ-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '202.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '402.00']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_TransitTimeExpectedResult => TestExtract_RateEntryCommonExpectedResult;

		protected override string TestExtract_Quotation_SimilarRateEntries_MatchContainerRateClassExpectedResult => TestExtract_RateEntryCommonExpectedResult;

		protected override string TestExtract_Quotation_SimilarRateEntries_ServiceLevelExpectedResult =>
@"
[Row 0]
  [Entry STD-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '201.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '401.00']

[Row 1]
  [Entry EXP-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '202.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '402.00']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_CarrierServiceLevelExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_CartageDeliveryAddressPostCodeExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_CartagePickupAddressPostCodeExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_ViaExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_SupplierExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_TransportProviderExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_ControllingCustomerExpectedResult => TestExtract_RateEntryCommonExpectedResult;

		string TestExtract_RateEntryCommonExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '201.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '401.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '202.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '402.00']
";

		#endregion

		#region Quotation and ClientRate with similar RateEntries

		#region RateEntry field trigger new page

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierOrganization_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '20.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '40.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '120.00']
    [SubRow 3, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '140.00']

[Page 1]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '21.00']
    [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '121.00']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_Commodity_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-HAZ-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '20.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '40.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '120.00']
    [SubRow 3, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '140.00']

[Page 1]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '21.00']
    [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '121.00']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ServiceLevel_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry DEF-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '20.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '40.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '120.00']
    [SubRow 3, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '140.00']

[Page 1]

  [Row 0]
    [Entry DIR-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '21.00']
    [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '121.00']";

		#endregion

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_MatchContainerRateClass_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '20.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '40.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '120.00']
    [SubRow 3, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '140.00']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '21.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '22.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '121.00']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ClientContractNumber_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT1]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '20.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '40.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '120.00']
    [SubRow 3, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '140.00']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT2]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '21.00']
    [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '121.00']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_Frequency_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '20.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '40.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '120.00']
    [SubRow 3, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '140.00']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '21.00']
    [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '121.00']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_FrequencyUnit_ExpectedResults =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '20.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '40.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '120.00']
    [SubRow 3, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '140.00']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: 1 per Fortnight]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '21.00']
    [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '121.00']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult
			=> TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult;

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '20.00']
    [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '40.00']
    [SubRow 2, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '120.00']
    [SubRow 3, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '140.00']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
      [Caption Group 1]
      [Column 'Flat' = '21.00']
    [SubRow 1, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Caption Group 1]
      [Column 'Flat' = '121.00']";

		#endregion

		protected override string TestExtract_MatchContainerRateClassComplexExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '11.00']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '12.00']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT2]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '21.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '22.00']

[Row 3]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT3]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '31.00']

[Row 4]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT3]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '32.00']
";

		protected override string TestExtract_ContainerExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '10.00']
  [SubRow 1, 'AUD - Australian Dollar', 'BAF - Bunker Adjustment Factor']
    [Caption Group 1]
    [Column 'Flat' = '20.00']
";

		protected override string Category => Rating.Business.RatingConstants.RateCategory.FCL;
		protected override string Mode => Core.Constants.RateMode.SEA;
		protected override string ChargeCode => "BAF";

		protected override BaseTableStrategy Strategy => strategy ?? (strategy = new ChargeableTableStrategy(Factory));
		ChargeableTableStrategy strategy;
	}
}
