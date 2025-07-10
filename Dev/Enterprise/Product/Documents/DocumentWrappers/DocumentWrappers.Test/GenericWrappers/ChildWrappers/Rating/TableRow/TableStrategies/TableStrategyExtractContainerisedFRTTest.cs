namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class TableStrategyExtractContainerisedFRTTest : TableStrategyExtractBaseTest
	{
		#region Quotation with similar rateEntries

		protected override string TestExtract_Quotation_SimilarRateEntries_ContractNumberExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '201.00 Flat']
    [Column '40GP' = '401.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT2]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '202.00 Flat']
    [Column '40GP' = '402.00 Flat']
";
		protected override string TestExtract_Quotation_SimilarRateEntries_PaymentTermExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_CommodityExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '201.00 Flat']
    [Column '40GP' = '401.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-HAZ-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '202.00 Flat']
    [Column '40GP' = '402.00 Flat']
";
		protected override string TestExtract_Quotation_SimilarRateEntries_TransitTimeExpectedResult => TestExtract_RateEntryCommonExpectedResult;

		//TODO: 201 and 401 should be shown
		protected override string TestExtract_Quotation_SimilarRateEntries_MatchContainerRateClassExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '202.00 Flat']
    [Column '40GP' = '402.00 Flat']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_ServiceLevelExpectedResult =>
@"
[Row 0]
  [Entry STD-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '201.00 Flat']
    [Column '40GP' = '401.00 Flat']

[Row 1]
  [Entry EXP-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '202.00 Flat']
    [Column '40GP' = '402.00 Flat']
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
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '201.00 Flat']
    [Column '40GP' = '401.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '202.00 Flat']
    [Column '40GP' = '402.00 Flat']
";

		#endregion

		#region Quotation and ClientRate with similar RateEntries

		// Quote FRT charges are the same as ClientRate FRT charges hence Quote FRT charges are prioritized and ClientRate FRT charges are not shown.

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

		//TODO: missing 120
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

		protected override string TestExtract_MatchContainerRateClassComplexExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '11.00 Flat']
    [Column '40GP' = '12.00 Flat']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT2]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '21.00 Flat']
    [Column '40GP' = '22.00 Flat']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT3]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT3]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '31.00 Flat']
    [Column '40GP' = '32.00 Flat']
";

		protected override string TestExtract_ContainerExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
    [Column '20GP' = '10.00 Flat']
    [Column '40GP' = '20.00 Flat']
";

		protected override string Category => Rating.Business.RatingConstants.RateCategory.FCL;
		protected override string Mode => Core.Constants.RateMode.SEA;
		protected override string ChargeCode => "FRT";

		protected override BaseTableStrategy Strategy => strategy ?? (strategy = new ContainerisedTableStrategy(Factory));
		ContainerisedTableStrategy strategy;
	}
}
