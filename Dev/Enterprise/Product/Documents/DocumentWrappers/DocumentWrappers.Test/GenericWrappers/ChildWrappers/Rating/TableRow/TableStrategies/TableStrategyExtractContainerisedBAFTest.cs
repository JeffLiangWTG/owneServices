namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class TableStrategyExtractContainerisedBAFTest : TableStrategyExtractBaseTest
	{
		#region Quotation with similar rateEntries

		protected override string TestExtract_Quotation_SimilarRateEntries_ContractNumberExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '201.00', '']
  [Line 1,3, '40GP', 'AUD', '401.00', '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT2]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '202.00', '']
  [Line 1,3, '40GP', 'AUD', '402.00', '']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_PaymentTermExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_CommodityExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '201.00', '']
  [Line 1,3, '40GP', 'AUD', '401.00', '']

[Row 1]
  [Entry [Empty Service Level]-HAZ-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '202.00', '']
  [Line 1,3, '40GP', 'AUD', '402.00', '']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_TransitTimeExpectedResult => TestExtract_RateEntryCommonExpectedResult;

		protected override string TestExtract_Quotation_SimilarRateEntries_MatchContainerRateClassExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '201.00', '']
  [Line 1,3, '40GP', 'AUD', '401.00', '']
  [Line 2,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 2,2, '20GP', 'AUD', '202.00', '']
  [Line 2,3, '40GP', 'AUD', '402.00', '']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_ServiceLevelExpectedResult =>
@"
[Row 0]
  [Entry STD-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '201.00', '']
  [Line 1,3, '40GP', 'AUD', '401.00', '']

[Row 1]
  [Entry EXP-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '202.00', '']
  [Line 1,3, '40GP', 'AUD', '402.00', '']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_CarrierServiceLevelExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP 
-  Carrier Service Level: STD', 'AUD', '201.00', '']
  [Line 1,3, '40GP 
-  Carrier Service Level: STD', 'AUD', '401.00', '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP 
-  Carrier Service Level: EXP', 'AUD', '202.00', '']
  [Line 1,3, '40GP 
-  Carrier Service Level: EXP', 'AUD', '402.00', '']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_CartageDeliveryAddressPostCodeExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP 
-  To Postcode 1111', 'AUD', '201.00', '']
  [Line 1,3, '40GP 
-  To Postcode 1111', 'AUD', '401.00', '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP 
-  To Postcode 2222', 'AUD', '202.00', '']
  [Line 1,3, '40GP 
-  To Postcode 2222', 'AUD', '402.00', '']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_CartagePickupAddressPostCodeExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP 
-  From Postcode 1111', 'AUD', '201.00', '']
  [Line 1,3, '40GP 
-  From Postcode 1111', 'AUD', '401.00', '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP 
-  From Postcode 2222', 'AUD', '202.00', '']
  [Line 1,3, '40GP 
-  From Postcode 2222', 'AUD', '402.00', '']
";

		protected override string TestExtract_Quotation_SimilarRateEntries_ViaExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_SupplierExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_TransportProviderExpectedResult => TestExtract_RateEntryCommonExpectedResult;
		protected override string TestExtract_Quotation_SimilarRateEntries_ControllingCustomerExpectedResult => TestExtract_RateEntryCommonExpectedResult;

		string TestExtract_RateEntryCommonExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '201.00', '']
  [Line 1,3, '40GP', 'AUD', '401.00', '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '202.00', '']
  [Line 1,3, '40GP', 'AUD', '402.00', '']
";

		#endregion

		#region Quotation and ClientRate with similar RateEntries

		#region RateEntry field trigger new page

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierOrganization_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '20.00', '']
    [Line 1,3, '40GP', 'AUD', '40.00', '']

[Page 1]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '21.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_Commodity_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-HAZ-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '20.00', '']
    [Line 1,3, '40GP', 'AUD', '40.00', '']

[Page 1]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '21.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ServiceLevel_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry DEF-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '20.00', '']
    [Line 1,3, '40GP', 'AUD', '40.00', '']

[Page 1]

  [Row 0]
    [Entry DIR-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '21.00', '']";

		#endregion

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ClientContractNumber_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT1]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '20.00', '']
    [Line 1,3, '40GP', 'AUD', '40.00', '']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONT2]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '21.00', '']";

		//TODO: 120 is missing and weird that 22 is shown
		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_MatchContainerRateClass_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '20.00', '']
    [Line 1,3, '40GP', 'AUD', '40.00', '']
    [Line 2,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 2,2, '20GP', 'AUD', '21.00', '']
    [Line 2,3, '20GP', 'AUD', '22.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_CarrierServiceLevel_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP 
-  Carrier Service Level: STD', 'AUD', '20.00', '']
    [Line 1,3, '40GP 
-  Carrier Service Level: STD', 'AUD', '40.00', '']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP 
-  Carrier Service Level: EXP', 'AUD', '21.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_DestinationPostCode_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP 
-  To Postcode 1111', 'AUD', '20.00', '']
    [Line 1,3, '40GP 
-  To Postcode 1111', 'AUD', '40.00', '']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP 
-  To Postcode 2222', 'AUD', '21.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_OriginPostCode_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP 
-  From Postcode 1111', 'AUD', '20.00', '']
    [Line 1,3, '40GP 
-  From Postcode 1111', 'AUD', '40.00', '']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP 
-  From Postcode 2222', 'AUD', '21.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_Frequency_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '20.00', '']
    [Line 1,3, '40GP', 'AUD', '40.00', '']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every 2 Days]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '21.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_FrequencyUnit_ExpectedResults =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: Every Day]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '20.00', '']
    [Line 1,3, '40GP', 'AUD', '40.00', '']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Frequency: 1 per Fortnight]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '21.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult =>
@"[Page 0]

  [Row 0]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '120.00 Flat']
      [Column '40GP' = '140.00 Flat']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '20.00', '']
    [Line 1,3, '40GP', 'AUD', '40.00', '']

  [Row 1]
    [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
    [SubRow 0, 'AUD - Australian Dollar', 'FRT - International Freight']
      [Column '20GP' = '121.00 Flat']
      [Column '40GP' = '']
    [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
    [Line 1,2, '20GP', 'AUD', '21.00', '']";

		protected override string TestExtract_QuotationAndClientRate_SimilarRateEntries_NonDistinguishingColumnsForDocumentGrouping_ExpectedResult => TestExtract_QuotationAndClientRate_SimilarRateEntries_ExpectedResult;

		#endregion

		protected override string TestExtract_MatchContainerRateClassComplexExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT1]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '11.00', '']
  [Line 2,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 2,2, '40GP', 'AUD', '12.00', '']

[Row 1]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT2]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '21.00', '']
  [Line 1,3, '40GP', 'AUD', '22.00', '']

[Row 2]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT3]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX, Contract Number: CONTRACT3]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '31.00', '']
  [Line 2,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 2,2, '40GP', 'AUD', '32.00', '']
";

		protected override string TestExtract_ContainerExpectedResult =>
@"
[Row 0]
  [Entry [Empty Service Level]-GEN-FCL AUSYD => USLAX]
  [SubRow 0, '[Empty Currency]', '[Empty Charge]']
    [Column '20GP' = '']
    [Column '40GP' = '']
  [Line 1,1, 'Bunker Adjustment Factor', '', '', '']
  [Line 1,2, '20GP', 'AUD', '10.00', '']
  [Line 1,3, '40GP', 'AUD', '20.00', '']
";

		protected override string Category => Rating.Business.RatingConstants.RateCategory.FCL;
		protected override string Mode => Core.Constants.RateMode.SEA;
		protected override string ChargeCode => "BAF";

		protected override BaseTableStrategy Strategy => strategy ?? (strategy = new ContainerisedTableStrategy(Factory));
		ContainerisedTableStrategy strategy;
	}
}
