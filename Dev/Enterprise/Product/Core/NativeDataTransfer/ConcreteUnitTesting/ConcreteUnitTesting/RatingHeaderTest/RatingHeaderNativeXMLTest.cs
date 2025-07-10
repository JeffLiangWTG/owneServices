using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	public class RatingHeaderNativeXMLTest : TestCaseWithFactory
	{
		[TestDate(2014, 08, 14)]
		public void TestGivenRateEntryAndRateLineWithTheSameDate_WhenNewStartDateInOriginalRangeAndNewEndDateIsGreaterThanOriginalRange_ThenOldRateEntryAndRateLineShouldBeSetToExpired()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <PK>9dcbc490-b33b-4aa1-acab-2ac6d2d2638d</PK>
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteDateTime>2013-09-04T00:00:00</QuoteDateTime>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate>2013-09-11T00:00:00</FollowUpDate>
        <Accepted>2013-11-14T00:00:00</Accepted>
        <IsLocked>false</IsLocked>
        <IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
        <RateType>SAL</RateType>
        <GlobalRateLevel>0</GlobalRateLevel>
        <GlobalRateDescription></GlobalRateDescription>
        <AirCFX>0.00</AirCFX>
        <SeaCFX>0.00</SeaCFX>
        <ExportAirCFX>0.00</ExportAirCFX>
        <ExportSeaCFX>0.00</ExportSeaCFX>
        <PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
        <PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
        <PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
        <PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
        <QuoteCancellationReason></QuoteCancellationReason>
        <RateEntryCollection>          
          <RateEntry Action=""MERGE"">
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-15T00:00:00</RateStartDate>
            <RateEndDate>2014-09-05T00:00:00</RateEndDate>
            <TransitTime></TransitTime>
            <Frequency>0</Frequency>
            <FrequencyUnit></FrequencyUnit>
            <CartagePickupAddressPostCode></CartagePickupAddressPostCode>
            <CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <PageHeading></PageHeading>
            <PageOpeningText></PageOpeningText>
            <PageClosingText></PageClosingText>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
            <BuyersConsolRateMode></BuyersConsolRateMode>
            <ContractNumber></ContractNumber>
            <FromSuburb TableName=""RefCityTown"" />
            <ToSuburb TableName=""RefCityTown"" />
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
                <LineOrder>0</LineOrder>
                <RateDesc></RateDesc>
                <ConversionFactor>0.000</ConversionFactor>
                <WeightVolume>KG</WeightVolume>
                <WeightVolumeMultiple>0.0</WeightVolumeMultiple>
                <RateCalculator>UNT</RateCalculator>
                <CompanyTariffLevel>0</CompanyTariffLevel>
                <Rounding>DEF</Rounding>
                <IsOnPallets>false</IsOnPallets>
                <IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
                <RoundingFactor>0.000</RoundingFactor>
                <ActualPercentage>0</ActualPercentage>
                <Condition></Condition>
                <RateLineItemsCollection>
                  <RateLineItems Action=""MERGE"">
                    <LineOrder>0</LineOrder>
                    <Type>UNT</Type>
                    <BreakMinimum>0.000</BreakMinimum>
                    <Break>0.000</Break>
                    <BreakWeightVolume></BreakWeightVolume>
                    <Value>40.0000</Value>
                    <AgentDeclaredRate>0.0000</AgentDeclaredRate>
                    <FlatAmount>0.0000</FlatAmount>
                    <Text></Text>
                    <CallForPricing>false</CallForPricing>
                    <UnitMultiple>1</UnitMultiple>
                  </RateLineItems>
                </RateLineItemsCollection>
                <Currency TableName=""RefCurrency"">
                  <Code>USD</Code>
                  <PK>60aae969-b80b-4a40-9b2d-810d3385c76e</PK>
                </Currency>
                <AccChargeCode>
                  <Code>FRT</Code>
                  <PK>3fe61667-eb35-4080-b869-cb585e62a90c</PK>
                  <GlbCompany>
                    <Code>EDI</Code>
                    <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
                  </GlbCompany>
                </AccChargeCode>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
              <PK>60aae969-b80b-4a40-9b2d-810d3385c76e</PK>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </Publisher>
          </RateEntry>
        </RateEntryCollection>
        <OrgHeader>
          <Code>AAAAA</Code>
          <PK>{0}</PK>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
          <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
        </GlbCompany>
      </RatingHeader>
    </Rate>
  </Body>
</Native>";

			#endregion

			#region Prepare Existing Rates

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAAAA";
			var ratingHeader = Factory.NewWithValidTestData<ClientRate>();
			ratingHeader.TH_OH = orgHeader.PK;
			ratingHeader.TH_QuoteNumber = string.Empty;
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

			var rateLine1 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine1.TL_RateStartDate = new ZDate(2014, 08, 1);
			rateLine1.TL_RateEndDate = new ZDate(2014, 08, 10);

			var rateLine2 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine2.TL_RateStartDate = new ZDate(2014, 08, 1);
			rateLine2.TL_RateEndDate = new ZDate(2014, 08, 20);

			var rateLine3 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine3.TL_RateStartDate = new ZDate(2014, 08, 10);
			rateLine3.TL_RateEndDate = new ZDate(2014, 08, 31);

			var rateLine4 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine4.TL_RateStartDate = new ZDate(2014, 08, 20);
			rateLine4.TL_RateEndDate = new ZDate(2014, 08, 31);

			var rateLine5 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine5.TL_RateEndDate = new ZDate(2014, 08, 20);

			var rateLine6 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");

			Factory.Save();

			#endregion

			var preparedXml = string.Format(xml, orgHeader.PK);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection
					.Cast<RateEntry>()
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals(2, loadedEntries.Count);

				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].TI_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 5), loadedEntries[1].TI_RateEndDate);

				AssertEquals(6, loadedEntries[0].RateLines.Count);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].RateLines[0].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 10), loadedEntries[0].RateLines[0].TL_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].RateLines[1].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].RateLines[1].TL_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 10), loadedEntries[0].RateLines[2].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].RateLines[2].TL_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 20), loadedEntries[0].RateLines[3].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[0].RateLines[3].TL_RateEndDate);
				AssertEquals(ZDateTime.Empty, loadedEntries[0].RateLines[4].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].RateLines[4].TL_RateEndDate);
				AssertEquals(ZDateTime.Empty, loadedEntries[0].RateLines[5].TL_RateStartDate);
				AssertEquals(ZDateTime.Empty, loadedEntries[0].RateLines[5].TL_RateEndDate);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestGivenRateEntryAndRateLineWithTheSameDate_WhenNewStartDateIsLessThenOriginalStartDateAndNewEndDateIsInOriginalRange_ThenOldRateEntryAndRateLineShouldBeSetToExpired()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <PK>9dcbc490-b33b-4aa1-acab-2ac6d2d2638d</PK>
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteDateTime>2013-09-04T00:00:00</QuoteDateTime>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate>2013-09-11T00:00:00</FollowUpDate>
        <Accepted>2013-11-14T00:00:00</Accepted>
        <IsLocked>false</IsLocked>
        <IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
        <RateType>SAL</RateType>
        <GlobalRateLevel>0</GlobalRateLevel>
        <GlobalRateDescription></GlobalRateDescription>
        <AirCFX>0.00</AirCFX>
        <SeaCFX>0.00</SeaCFX>
        <ExportAirCFX>0.00</ExportAirCFX>
        <ExportSeaCFX>0.00</ExportSeaCFX>
        <PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
        <PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
        <PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
        <PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
        <QuoteCancellationReason></QuoteCancellationReason>
        <RateEntryCollection>          
          <RateEntry Action=""MERGE"">
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-07-15T00:00:00</RateStartDate>
            <RateEndDate>2014-08-15T00:00:00</RateEndDate>
            <TransitTime></TransitTime>
            <Frequency>0</Frequency>
            <FrequencyUnit></FrequencyUnit>
            <CartagePickupAddressPostCode></CartagePickupAddressPostCode>
            <CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <PageHeading></PageHeading>
            <PageOpeningText></PageOpeningText>
            <PageClosingText></PageClosingText>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
            <BuyersConsolRateMode></BuyersConsolRateMode>
            <ContractNumber></ContractNumber>
            <FromSuburb TableName=""RefCityTown"" />
            <ToSuburb TableName=""RefCityTown"" />
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
                <LineOrder>0</LineOrder>
                <RateDesc></RateDesc>
                <ConversionFactor>0.000</ConversionFactor>
                <WeightVolume>KG</WeightVolume>
                <WeightVolumeMultiple>0.0</WeightVolumeMultiple>
                <RateCalculator>UNT</RateCalculator>
                <CompanyTariffLevel>0</CompanyTariffLevel>
                <Rounding>DEF</Rounding>
                <IsOnPallets>false</IsOnPallets>
                <IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
                <RoundingFactor>0.000</RoundingFactor>
                <ActualPercentage>0</ActualPercentage>
                <Condition></Condition>
                <RateLineItemsCollection>
                  <RateLineItems Action=""MERGE"">
                    <LineOrder>0</LineOrder>
                    <Type>UNT</Type>
                    <BreakMinimum>0.000</BreakMinimum>
                    <Break>0.000</Break>
                    <BreakWeightVolume></BreakWeightVolume>
                    <Value>40.0000</Value>
                    <AgentDeclaredRate>0.0000</AgentDeclaredRate>
                    <FlatAmount>0.0000</FlatAmount>
                    <Text></Text>
                    <CallForPricing>false</CallForPricing>
                    <UnitMultiple>1</UnitMultiple>
                  </RateLineItems>
                </RateLineItemsCollection>
                <Currency TableName=""RefCurrency"">
                  <Code>USD</Code>
                  <PK>60aae969-b80b-4a40-9b2d-810d3385c76e</PK>
                </Currency>
                <AccChargeCode>
                  <Code>FRT</Code>
                  <PK>3fe61667-eb35-4080-b869-cb585e62a90c</PK>
                  <GlbCompany>
                    <Code>EDI</Code>
                    <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
                  </GlbCompany>
                </AccChargeCode>
              </RateLines>
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
              <PK>60aae969-b80b-4a40-9b2d-810d3385c76e</PK>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </Publisher>
          </RateEntry>
        </RateEntryCollection>
        <OrgHeader>
          <Code>AAAAA</Code>
          <PK>{0}</PK>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
          <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
        </GlbCompany>
      </RatingHeader>
    </Rate>
  </Body>
</Native>";

			#endregion

			#region Prepare Existing Rates

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAAAA";
			var ratingHeader = Factory.NewWithValidTestData<ClientRate>();
			ratingHeader.TH_OH = orgHeader.PK;
			ratingHeader.TH_QuoteNumber = string.Empty;
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

			var rateLine1 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine1.TL_RateStartDate = new ZDate(2014, 08, 1);
			rateLine1.TL_RateEndDate = new ZDate(2014, 08, 10);

			var rateLine2 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine2.TL_RateStartDate = new ZDate(2014, 08, 1);
			rateLine2.TL_RateEndDate = new ZDate(2014, 08, 20);

			var rateLine3 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine3.TL_RateStartDate = new ZDate(2014, 08, 10);
			rateLine3.TL_RateEndDate = new ZDate(2014, 08, 31);

			var rateLine4 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine4.TL_RateStartDate = new ZDate(2014, 08, 20);
			rateLine4.TL_RateEndDate = new ZDate(2014, 08, 31);

			var rateLine5 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			rateLine5.TL_RateStartDate = new ZDate(2014, 08, 1);

			var rateLine6 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");

			Factory.Save();

			#endregion

			var preparedXml = string.Format(xml, orgHeader.PK);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection
					.Cast<RateEntry>()
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals(2, loadedEntries.Count);

				AssertEquals(new ZDateTime(2014, 07, 15), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[0].TI_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 16), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[1].TI_RateEndDate);

				AssertEquals(6, loadedEntries[1].RateLines.Count);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[1].RateLines[0].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 10), loadedEntries[1].RateLines[0].TL_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 16), loadedEntries[1].RateLines[1].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 20), loadedEntries[1].RateLines[1].TL_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 16), loadedEntries[1].RateLines[2].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[1].RateLines[2].TL_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 20), loadedEntries[1].RateLines[3].TL_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[1].RateLines[3].TL_RateEndDate);
				AssertEquals(new ZDateTime(2014, 08, 16), loadedEntries[1].RateLines[4].TL_RateStartDate);
				AssertEquals(ZDateTime.Empty, loadedEntries[1].RateLines[4].TL_RateEndDate);
				AssertEquals(ZDateTime.Empty, loadedEntries[1].RateLines[5].TL_RateStartDate);
				AssertEquals(ZDateTime.Empty, loadedEntries[1].RateLines[5].TL_RateEndDate);
			}
		}
	}
}
