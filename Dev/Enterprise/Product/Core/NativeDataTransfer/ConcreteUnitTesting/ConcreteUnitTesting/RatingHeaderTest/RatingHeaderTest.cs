using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Common.GUI.Import;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	public class RatingHeaderTest : TestCaseWithFactory
	{
		[TestDate(2014, 5, 1)]
		public void TestImport()
		{
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(RatingHeaderXml)))
			{
				var orgPK = EDICodeMapper.GetDefaultOrgPK();
				var orgHeader = Factory.Load<OrgHeader>(orgPK);

				var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
				var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Germany));

				var patternMatchOverride1 = orgHeader.CreatePatternMatchOverrideForTest();
				patternMatchOverride1.OO_Relationship = "PTC";
				patternMatchOverride1.OO_OH = orgPK;
				patternMatchOverride1.OO_ForeignCode = "AAAAAA";
				patternMatchOverride1.OO_LocalGuid = port.PK;

				var patternMatchOverride2 = orgHeader.CreatePatternMatchOverrideForTest();
				patternMatchOverride2.OO_Relationship = "COU";
				patternMatchOverride2.OO_OH = orgPK;
				patternMatchOverride2.OO_ForeignCode = "ZZ";
				patternMatchOverride2.OO_LocalGuid = country.PK;

				Factory.Save();

				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
RateEntry: Code mapped from foreign code AAAAAA to local code AUSYD
RateEntry: Code mapped from foreign code ZZ to local code DE
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 2 inserts, 0 updates, 0 deletes
RateLines - 4 inserts, 0 updates, 0 deletes
RateLineItems - 4 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var factory = new BusinessObjectFactory();
				var ratingHeader = factory.LoadTop1<RatingHeader>(new ZQuery());
				var entry1 = ratingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection.Cast<RateEntry>().First();
				AssertEquals("AIR", entry1.TI_RateCategory);
				AssertEquals("AUSYD", entry1.TI_OriginLRC);
				AssertEquals("USLAX", entry1.TI_DestinationLRC);

				var entry2 = ratingHeader.EntryCollections[RatingConstants.RateCategory.ORG].LoadedCollection.Cast<RateEntry>().First();
				AssertEquals("ORG", entry2.TI_RateCategory);
				AssertEquals("AU", entry2.TI_OriginLRC);
				AssertEquals("DE", entry2.TI_DestinationLRC);
				AssertEquals("UA", entry2.TI_ViaLRC);
			}
		}

		public void TestExport_ClientRate()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Org";

			var helper = new TestHelper(Factory);
			var clientRate = helper.NewClientRate(orgHeader);

			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "DE");
			rateEntry.TI_ViaLRC = "UA";
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);

			Factory.Save();

			string actualXML = "";

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(clientRate))
			using (var reader = new StreamReader(dataStream))
			{
				actualXML = reader.ReadToEnd();
			}

			var element = XElement.Load(new StringReader(actualXML));

			var rateType = element.Descendants().First(e => e.Name.LocalName == "RateType").Value;
			var orgHeaderCode = element.Descendants().First(e => e.Name.LocalName == "OrgHeader").Descendants().First(e => e.Name.LocalName == "Code").Value;
			var rateEntryElement = element.Descendants().First(e => e.Name.LocalName == "RateEntry");
			var origin = rateEntryElement.Descendants().First(e => e.Name.LocalName == "OriginLRC");
			var destination = rateEntryElement.Descendants().First(e => e.Name.LocalName == "DestinationLRC");
			var via = rateEntryElement.Descendants().First(e => e.Name.LocalName == "ViaLRC");

			CombineAssertions(delegate
			{
				AssertEquals(RatingConstants.RatingHeaderTypes.ClientRate, rateType);
				AssertEquals("Org", orgHeaderCode);
				AssertEquals("PTC", origin.Attribute("Relationship").Value);
				AssertEquals("COU", destination.Attribute("Relationship").Value);
				AssertEquals("COU", via.Attribute("Relationship").Value);
			});
		}

		public void TestExport_Quote()
		{
			var helper = new TestHelper(Factory);
			var quote = helper.NewQuote(helper.NewOrgHeader());
			quote.TH_QuoteNumber = "AAABBB1";
			quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "DE");

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(quote))
			using (var reader = new StreamReader(dataStream))
			{
				var actualXML = reader.ReadToEnd();

				var element = XElement.Load(new StringReader(actualXML));

				var rateType = element.Descendants().First(e => e.Name.LocalName == "RateType").Value;
				var quoteNumber = element.Descendants().First(e => e.Name.LocalName == "QuoteNumber").Value;
				var rateEntryElement = element.Descendants().First(e => e.Name.LocalName == "RateEntry");
				var origin = rateEntryElement.Descendants().First(e => e.Name.LocalName == "OriginLRC");
				var destination = rateEntryElement.Descendants().First(e => e.Name.LocalName == "DestinationLRC");

				var orgHeader = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "RatingHeader")
					?.Descendants().FirstOrDefault(e => e.Name.LocalName == "JobDocAddress")
					?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Address")
					?.Descendants().FirstOrDefault(e => e.Name.LocalName == "OrgHeader");

				CombineAssertions(delegate
				{
					AssertEquals(RatingConstants.RatingHeaderTypes.Quote, rateType);
					AssertEquals("When you create a new quote for the same org withthe same number you'd expect it to append /a", "AAABBB1/A", quoteNumber);
					AssertEquals("PTC", origin.Attribute("Relationship").Value);
					AssertEquals("COU", destination.Attribute("Relationship").Value);
					AssertEquals("should has Quote OrgHeader information", quote.TH_ClientCode + quote.TH_OH.ToString(), orgHeader?.Value);
				});
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportClientRateWhenRecipientRoleClient()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>ClientRate</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~SAL</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
							<RateEntry Action=""MERGE"">
								<PK>c00ef7fe-0f49-48e6-bb5c-81f916c75b05</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-09-01T00:00:00</RateStartDate>
								<RateEndDate>2014-10-31T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC></OriginLRC>
								<DestinationLRC Relationship=""PTC"">AUBNE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-24T13:42:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>057877a4-df07-41f7-82ec-fe7edf917c0b</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>3f1e4225-087f-4354-ac49-72a279927fcd</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>150.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
							<RateEntry Action=""MERGE"">
								<PK>f3d74720-1b41-4d4c-b138-d352b0bb05e5</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-11-01T00:00:00</RateStartDate>
								<RateEndDate>2015-03-01T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC Relationship=""COU"">AU</OriginLRC>
								<DestinationLRC Relationship=""COU"">DE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>343226ff-8cf4-4ec5-a91f-173269b0acd4</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>123.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AAAAA</Code>
							<PK>00000000-0000-0000-0000-000000000000</PK>
						</OrgHeader>
						<GlbCompany>
							<Code>EDI</Code>
						</GlbCompany>
					</RatingHeader>
				</Rate>
			</Body>
		</Native>";

			#endregion

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: AAAAA
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 2 inserts, 0 updates, 0 deletes
RateLines - 2 inserts, 0 updates, 0 deletes
RateLineItems - 2 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				AssertEquals(RatingConstants.RatingHeaderTypes.Costing, loadedRatingHeader.TH_RateType);
				AssertEquals(2, loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection.Count);
				AssertEquals("EDICUS", loadedRatingHeader.Header.OH_Code);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportClientRateWhenRecipientRoleClient_DoesNotAffectExistingClientRate()
		{
			var helper = new TestHelper(Factory);
			var client = helper.CreateCreditor("CLIENT");
			var existingClientRate = helper.NewClientRateWithSingleRateLine(client, RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "DE", "FRT", 90m);
			Factory.Save();

			var updatedXML = "";
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			using (var dataStream = xmlSerializer.SerializeToStream(existingClientRate))
			using (var reader = new StreamReader(dataStream))
			{
				updatedXML = reader.ReadToEnd();
			}

			#region dataContext for XML Header

			var dataContext = @"
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>";

			#endregion

			updatedXML = updatedXML.Replace("<Header>", "<Header>" + dataContext); //Insert DataContext into the XML Header

			var clientRateQuery = new ZQuery(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.ClientRate);
			var costingQuery = new ZQuery(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Costing);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(updatedXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();

				var clientRates = newFactory.Load<RatingHeader>(clientRateQuery);
				var costings = newFactory.Load<RatingHeader>(costingQuery);

				var clientRate = clientRates.Single();
				var costing = costings.Single();

				var actualRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
				AssertContainsExactElementsInAnyOrder(new[] { clientRate, costing }, actualRatingHeaders);
				AssertEquals(existingClientRate.PK, clientRate.PK);
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK, costing.TH_OH);
				AssertEntryAndLine(costing);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: CLIENT
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			using (var stream = new MemoryStream(encoding.GetBytes(updatedXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();

				var clientRates = newFactory.Load<RatingHeader>(clientRateQuery);
				var costings = newFactory.Load<RatingHeader>(costingQuery);

				var clientRate = clientRates.Single();
				var costing = costings.Single();

				var actualRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
				AssertContainsExactElementsInAnyOrder(new[] { clientRate, costing }, actualRatingHeaders);
				AssertEquals(existingClientRate.PK, clientRate.PK);
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK, costing.TH_OH);
				AssertEntryAndLine(costing);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: CLIENT
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 0 updates, 0 deletes
RateEntry - 0 inserts, 0 updates, 0 deletes
RateLines - 0 inserts, 0 updates, 0 deletes
RateLineItems - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				costing.TH_OH = ZGuid.Empty;
				newFactory.Save();
			}

			using (var stream = new MemoryStream(encoding.GetBytes(updatedXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();

				var clientRates = newFactory.Load<RatingHeader>(clientRateQuery);
				var costings = newFactory.Load<RatingHeader>(costingQuery);

				AssertEquals(2, costings.Length);

				var clientRate = clientRates.Single();

				var standardCosting = costings.First(x => x.TH_OH.IsEmpty);
				var importedCosting = costings.First(x => x.TH_OH == GlbCompany.CurrentCompany.OrgProxy.PK);

				var actualRatingHeaders = newFactory.Load<RatingHeader>(new ZQuery());
				AssertContainsExactElementsInAnyOrder(new[] { clientRate, standardCosting, importedCosting }, actualRatingHeaders);
				AssertEntryAndLine(standardCosting);
				AssertEntryAndLine(importedCosting);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: CLIENT
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		static void AssertEntryAndLine(RatingHeader costing)
		{
			var costEntry = costing.AllEntries.Single();
			AssertEquals(RatingConstants.RateCategory.ORG, costEntry.TI_RateCategory);
			AssertEquals(Core.Constants.RateMode.LCL, costEntry.TI_Mode);
			AssertEquals("AU", costEntry.TI_OriginLRC);
			AssertEquals("DE", costEntry.TI_DestinationLRC);

			var costLine = costEntry.RateLines.Cast<RateLine>().Single();
			AssertEquals("FLT", costLine.TL_RateCalculator);
			AssertEquals("FRT", costLine.ChargeCode.AC_Code);
		}

		[TestDate(2014, 08, 14)]
		public void TestImportQuotationWhenRecipientRoleClient()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>Quotation</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~QTE</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AAAAA</Code>
							<PK>00000000-0000-0000-0000-000000000000</PK>
						</OrgHeader>
						<GlbCompany>
							<Code>EDI</Code>
						</GlbCompany>
					</RatingHeader>
				</Rate>
			</Body>
		</Native>";

			#endregion

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: AAAAA
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				AssertEquals(RatingConstants.RatingHeaderTypes.Costing, loadedRatingHeader.TH_RateType);
				AssertEquals("EDICUS", loadedRatingHeader.Header.OH_Code);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportQuotationWhenRecipientRoleClientAndOwnerIsNotKnown()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>MCLAREN</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>Quotation</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~QTE</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AAAAA</Code>
							<PK>00000000-0000-0000-0000-000000000000</PK>
						</OrgHeader>
						<GlbCompany>
							<Code>EDI</Code>
						</GlbCompany>
					</RatingHeader>
				</Rate>
			</Body>
		</Native>";

			#endregion

			var mappedOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			mappedOrganisation.OH_Code = "MANUTD";

			var mapping = OrgHeader.DefaultOrg.CreatePatternMatchOverrideForTest();
			mapping.OO_ForeignCode = "MCLAREN";
			mapping.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			mapping.OO_LocalGuid = mappedOrganisation.PK;

			mapping.Factory.Save();
			Factory.Save();

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Carrier sell rates have been transformed to client buy rates. Carrier: MANUTD, Client: AAAAA
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				AssertEquals(RatingConstants.RatingHeaderTypes.Costing, loadedRatingHeader.TH_RateType);
				AssertEquals("MANUTD", loadedRatingHeader.Header.OH_Code);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportClientRateWhenRecipientRoleOrgProxy()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>ClientRate</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~SAL</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>ORP</Code>
							<Description>Organization Proxy</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
							<RateEntry Action=""MERGE"">
								<PK>c00ef7fe-0f49-48e6-bb5c-81f916c75b05</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-09-01T00:00:00</RateStartDate>
								<RateEndDate>2014-10-31T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC></OriginLRC>
								<DestinationLRC Relationship=""PTC"">AUBNE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-24T13:42:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>057877a4-df07-41f7-82ec-fe7edf917c0b</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>3f1e4225-087f-4354-ac49-72a279927fcd</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>150.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
							<RateEntry Action=""MERGE"">
								<PK>f3d74720-1b41-4d4c-b138-d352b0bb05e5</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-11-01T00:00:00</RateStartDate>
								<RateEndDate>2015-03-01T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC Relationship=""COU"">AU</OriginLRC>
								<DestinationLRC Relationship=""COU"">DE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>343226ff-8cf4-4ec5-a91f-173269b0acd4</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>123.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
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

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAAAA";

			Factory.Save();

			var preparedXml = string.Format(xml, orgHeader.PK);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				AssertEquals(RatingConstants.RatingHeaderTypes.ClientRate, loadedRatingHeader.TH_RateType);
				AssertEquals(2, loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection.Count);
				AssertEquals("AAAAA", loadedRatingHeader.Header.OH_Code);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportClientRateWithTwoRecipientRoles()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>ClientRate</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~SAL</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
						<RecipientRole>
							<Code>ORP</Code>
							<Description>Organization Proxy</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
							<RateEntry Action=""MERGE"">
								<PK>c00ef7fe-0f49-48e6-bb5c-81f916c75b05</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-09-01T00:00:00</RateStartDate>
								<RateEndDate>2014-10-31T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC></OriginLRC>
								<DestinationLRC Relationship=""PTC"">AUBNE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-24T13:42:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>057877a4-df07-41f7-82ec-fe7edf917c0b</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>3f1e4225-087f-4354-ac49-72a279927fcd</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>150.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
							<RateEntry Action=""MERGE"">
								<PK>f3d74720-1b41-4d4c-b138-d352b0bb05e5</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-11-01T00:00:00</RateStartDate>
								<RateEndDate>2015-03-01T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC Relationship=""COU"">AU</OriginLRC>
								<DestinationLRC Relationship=""COU"">DE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>343226ff-8cf4-4ec5-a91f-173269b0acd4</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>123.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AAAAA</Code>
							<PK>00000000-0000-0000-0000-000000000000</PK>
						</OrgHeader>
						<GlbCompany>
							<Code>EDI</Code>
						</GlbCompany>
					</RatingHeader>
				</Rate>
			</Body>
		</Native>";

			#endregion

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				AssertEquals("Should still import as a cost", RatingConstants.RatingHeaderTypes.Costing, loadedRatingHeader.TH_RateType);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportClientRateWithExternalOrg()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>ClientRate</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~SAL</Key>
						</DataSource>
					</DataSourceCollection>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
							<RateEntry Action=""MERGE"">
								<PK>c00ef7fe-0f49-48e6-bb5c-81f916c75b05</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-09-01T00:00:00</RateStartDate>
								<RateEndDate>2014-10-31T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC></OriginLRC>
								<DestinationLRC Relationship=""PTC"">AUBNE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-24T13:42:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>057877a4-df07-41f7-82ec-fe7edf917c0b</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>3f1e4225-087f-4354-ac49-72a279927fcd</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>150.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
							<RateEntry Action=""MERGE"">
								<PK>f3d74720-1b41-4d4c-b138-d352b0bb05e5</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-11-01T00:00:00</RateStartDate>
								<RateEndDate>2015-03-01T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC Relationship=""COU"">AU</OriginLRC>
								<DestinationLRC Relationship=""COU"">DE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>343226ff-8cf4-4ec5-a91f-173269b0acd4</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>123.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AAAAA</Code>
							<PK>{0}</PK>
						</OrgHeader>
						<GlbCompany>
							<Code>EDI</Code>
						</GlbCompany>
					</RatingHeader>
				</Rate>
			</Body>
		</Native>";

			#endregion

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAAAA";

			Factory.Save();

			var preparedXml = string.Format(xml, orgHeader.PK);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				AssertEquals(RatingConstants.RatingHeaderTypes.ClientRate, loadedRatingHeader.TH_RateType);
				AssertEquals(2, loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection.Count);
				AssertEquals("AAAAA", loadedRatingHeader.Header.OH_Code);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportClientRateWhenRecipientRoleClient_GlobalClientRate()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>ClientRate</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~SAL</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
							<RateEntry Action=""MERGE"">
								<PK>f3d74720-1b41-4d4c-b138-d352b0bb05e5</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-11-01T00:00:00</RateStartDate>
								<RateEndDate>2015-03-01T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC Relationship=""COU"">AU</OriginLRC>
								<DestinationLRC Relationship=""COU"">DE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>343226ff-8cf4-4ec5-a91f-173269b0acd4</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>123.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"" />
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AASDRA</Code>
							<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
						</OrgHeader>
					</RatingHeader>
				</Rate>
			</Body>
		</Native>";

			#endregion

			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out _, out _, true, false, "FRT", "FRT");

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: AASDRA
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log reports imported rates", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());

				Assert("Global Client Rate should be imported as a Global Cost Rate", loadedRatingHeader.IsGlobalCostRate());
				AssertEquals(1, loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection.Count);
				AssertEquals("EDICUS", loadedRatingHeader.Header.OH_Code);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportClientRateWhenRecipientRoleClient_DoesNotDeleteExistingClientRate()
		{
			#region RatingHeaderXml

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>ClientRate</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~SAL</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>EDI</Code>
						<Country>
							<Code>AU</Code>
							<Name>Australia</Name>
						</Country>
						<Name>Eagle Datamation International</Name>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>BNE</Code>
						<Name>BN - AUBNE</Name>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
						<Name>Branch</Name>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
							<RateEntry Action=""MERGE"">
								<PK>c00ef7fe-0f49-48e6-bb5c-81f916c75b05</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-09-01T00:00:00</RateStartDate>
								<RateEndDate>2014-10-31T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC></OriginLRC>
								<DestinationLRC Relationship=""PTC"">AUBNE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-24T13:42:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>057877a4-df07-41f7-82ec-fe7edf917c0b</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>3f1e4225-087f-4354-ac49-72a279927fcd</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>150.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
							<RateEntry Action=""MERGE"">
								<PK>f3d74720-1b41-4d4c-b138-d352b0bb05e5</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-11-01T00:00:00</RateStartDate>
								<RateEndDate>2015-03-01T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC Relationship=""COU"">AU</OriginLRC>
								<DestinationLRC Relationship=""COU"">DE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>343226ff-8cf4-4ec5-a91f-173269b0acd4</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>123.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<PercentOf TableName=""AccChargeCode"" />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AAAAA</Code>
							<PK>00000000-0000-0000-0000-000000000000</PK>
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

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: AAAAA
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 1 inserts, 0 updates, 0 deletes
RateEntry - 2 inserts, 0 updates, 0 deletes
RateLines - 2 inserts, 0 updates, 0 deletes
RateLineItems - 2 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				AssertEquals(RatingConstants.RatingHeaderTypes.Costing, loadedRatingHeader.TH_RateType);
				AssertEquals(2, loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection.Count);
				AssertEquals("EDICUS", loadedRatingHeader.Header.OH_Code);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWithDuplicateRatesAndActionMerge()
		{
			#region RatingHeaderXml

			string rateEntryXml = @"
            <RateEntry Action=""MERGE"">
            <Mode>SEA</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>FCL</RateCategory>
            <RateStartDate>2018-1-1</RateStartDate><RateEndDate></RateEndDate>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40GP</Code>
			</RefContainer>
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<RateLineItemsCollection>
					<RateLineItems Action=""MERGE"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>";

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteDateTime>2019-09-04T00:00:00</QuoteDateTime>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate>2019-09-11T00:00:00</FollowUpDate>
        <Accepted>2019-11-14T00:00:00</Accepted>
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
        <RateEntryCollection>"
			+ rateEntryXml
			+ rateEntryXml
			+ @"
        </RateEntryCollection>
        <OrgHeader>
          <Code>AAAAA</Code>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
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

			Factory.Save();

			#endregion

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes"
				.Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.AllEntries
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals(1, loadedEntries.Count);

				AssertEquals("FCL", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2018, 01, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(true, loadedEntries[0].TI_RateEndDate.IsEmpty);
			}
		}

		#region TestImportWithDateRangeUpdateBehaviour

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewDateRangeGreaterThanOriginalDateRange()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-09-01T00:00:00</RateStartDate>
            <RateEndDate>2014-09-25T00:00:00</RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 09, 1), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 25), loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateEqualOriginalEndDate()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-31T00:00:00</RateStartDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

			Factory.Save();

			#endregion

			var preparedXml = string.Format(xml, orgHeader.PK);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateEndDate changed from value 31/08/2014 12:00:00 AM to value 30/08/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection
					.Cast<RateEntry>()
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 30), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 5), loadedEntries[1].TI_RateEndDate);

				AssertEquals(2, loadedEntries.Count);
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateInOriginalRangeAndNewEndDateIsGreaterThanOriginalRange()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 5), loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateEndDate changed from value 31/08/2014 12:00:00 AM to value 14/08/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateInOriginalRangeAndOriginalEndDateIsNotDefined()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = ZDate.Empty;

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 5), loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateEndDate changed from value Empty date to value 14/08/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2019, 9, 1)]
		public void TestImportContainingDuplicateRatesWithOverlappingDateRanges()
		{
			#region RatingHeaderXml

			string rateEntryXmlWithNoStartEnd = @"
            <RateEntry Action=""MERGE"">
            <Mode>SEA</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>FCL</RateCategory>
            <RateStartDate></RateStartDate><RateEndDate></RateEndDate>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40GP</Code>
			</RefContainer>
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<RateLineItemsCollection>
					<RateLineItems Action=""MERGE"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>";

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteDateTime>2019-09-04T00:00:00</QuoteDateTime>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate>2019-09-11T00:00:00</FollowUpDate>
        <Accepted>2019-11-14T00:00:00</Accepted>
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
        <RateEntryCollection>"
			+ rateEntryXmlWithNoStartEnd.Replace("<RateStartDate></RateStartDate><RateEndDate></RateEndDate>",
				"<RateStartDate>2018-1-1</RateStartDate><RateEndDate></RateEndDate>")
			+ rateEntryXmlWithNoStartEnd.Replace("<RateStartDate></RateStartDate><RateEndDate></RateEndDate>",
				"<RateStartDate>2019-1-1</RateStartDate><RateEndDate></RateEndDate>")
			+ rateEntryXmlWithNoStartEnd.Replace("<RateStartDate></RateStartDate><RateEndDate></RateEndDate>",
				"<RateStartDate>2019-1-1</RateStartDate><RateEndDate></RateEndDate>")
			+ @"
        </RateEntryCollection>
        <OrgHeader>
          <Code>AAAAA</Code>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
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

			Factory.Save();

			#endregion

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[SEA]
RateCategory[FCL]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateEndDate changed from value Empty date to value 31/12/2018 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 2 inserts, 0 updates, 0 deletes
RateLines - 2 inserts, 0 updates, 0 deletes
RateLineItems - 2 inserts, 0 updates, 0 deletes"
				.Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.AllEntries
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals(2, loadedEntries.Count);

				AssertEquals("FCL", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2018, 01, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2018, 12, 31), loadedEntries[0].TI_RateEndDate);

				AssertEquals("FCL", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2019, 1, 1), loadedEntries[1].TI_RateStartDate);
				AssertEquals(true, loadedEntries[1].TI_RateEndDate.IsEmpty);
			}
		}

		[TestDate(2020, 4, 1)]
		public void TestImport_DeleteEntryFollowedByInsertWithSameDates()
		{
			#region Prepare Existing Rates

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAAAA";
			var ratingHeader = Factory.NewWithValidTestData<ClientRate>();
			ratingHeader.TH_OH = orgHeader.PK;
			ratingHeader.TH_QuoteNumber = string.Empty;

			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RateStartDate = new ZDate(2020, 4, 1);
			entry.TI_RateEndDate = new ZDate(2020, 4, 30);

			Factory.Save();

			#endregion

			#region RatingHeaderXml

			string xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
		<PK>{ratingHeader.PK.ToString()}</PK>
        <RateType>SAL</RateType>
        <RateEntryCollection>          
          <RateEntry Action=""DELETE"">
			<PK>{entry.PK.ToString()}</PK>
          </RateEntry>        
          <RateEntry Action=""INSERT"">
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>{entry.TI_RateStartDate.ToISO8601ShortDateString()}</RateStartDate>
            <RateEndDate>{entry.TI_RateEndDate.ToISO8601ShortDateString()}</RateEndDate>
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
              <RateLines Action=""INSERT"">
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
                  <RateLineItems Action=""INSERT"">
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
                </Currency>
                <AccChargeCode>
                  <Code>FRT</Code>
                  <GlbCompany>
                    <Code>EDI</Code>
                  </GlbCompany>
                </AccChargeCode>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>
        </RateEntryCollection>
      </RatingHeader>
    </Rate>
  </Body>
</Native>";

			#endregion

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Processed: Rate
--- Import Process Finished -----------------------------------------------------------

RatingHeader - 0 inserts, 0 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 1 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes"
				.Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.AllEntries
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals(1, loadedEntries.Count);

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(entry.TI_RateStartDate, loadedEntries[0].TI_RateStartDate);
				AssertEquals(entry.TI_RateEndDate, loadedEntries[0].TI_RateEndDate);
			}
		}

		[TestDate(2019, 9, 1)]
		public void TestImportContainingDifferentRatesWithOverlappingDateRanges_DoesNotUpdateDate()
		{
			#region RatingHeaderXml

			string rateEntryXmlWithNoStartEndDestination = @"
            <RateEntry Action=""MERGE"">
            <Mode>SEA</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>FCL</RateCategory>
            <RateStartDate></RateStartDate><RateEndDate></RateEndDate><DestinationLRC></DestinationLRC>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40GP</Code>
			</RefContainer>
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<RateLineItemsCollection>
					<RateLineItems Action=""MERGE"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>";

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteDateTime>2019-09-04T00:00:00</QuoteDateTime>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate>2019-09-11T00:00:00</FollowUpDate>
        <Accepted>2019-11-14T00:00:00</Accepted>
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
        <RateEntryCollection>"
			+ rateEntryXmlWithNoStartEndDestination.Replace("<RateStartDate></RateStartDate><RateEndDate></RateEndDate><DestinationLRC></DestinationLRC>",
				"<RateStartDate>2018-1-1</RateStartDate><RateEndDate></RateEndDate><DestinationLRC>USLAX</DestinationLRC>")
			+ rateEntryXmlWithNoStartEndDestination.Replace("<RateStartDate></RateStartDate><RateEndDate></RateEndDate><DestinationLRC></DestinationLRC>",
				"<RateStartDate>2019-1-1</RateStartDate><RateEndDate></RateEndDate><DestinationLRC>NZAKL</DestinationLRC>")
			+ @"
        </RateEntryCollection>
        <OrgHeader>
          <Code>AAAAA</Code>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
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

			Factory.Save();

			#endregion

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 2 inserts, 0 updates, 0 deletes
RateLines - 2 inserts, 0 updates, 0 deletes
RateLineItems - 2 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.AllEntries
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals(2, loadedEntries.Count);

				AssertEquals("FCL", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2018, 01, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(true, loadedEntries[0].TI_RateEndDate.IsEmpty);

				AssertEquals("FCL", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("NZAKL", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2019, 1, 1), loadedEntries[1].TI_RateStartDate);
				AssertEquals(true, loadedEntries[1].TI_RateEndDate.IsEmpty);
			}
		}

		[TestDate(2019, 9, 1)]
		public void TestImportContainingRatesWithDifferentColumnsWithOverlappingDateRanges_DoesNotUpdateDate()
		{
			// Second rate is identical to first, except
			// - StartDate is later
			// - It has a ViaLRC element

			#region RatingHeaderXml

			string rateEntryXmlWithNoStartEnd = @"
            <RateEntry Action=""INSERT"">
            <Mode>SEA</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>FCL</RateCategory>
            <RateStartDate></RateStartDate><RateEndDate></RateEndDate>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40GP</Code>
			</RefContainer>
            <RateLinesCollection>
              <RateLines Action=""INSERT"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<RateLineItemsCollection>
					<RateLineItems Action=""INSERT"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>";

			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteDateTime>2019-09-04T00:00:00</QuoteDateTime>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate>2019-09-11T00:00:00</FollowUpDate>
        <Accepted>2019-11-14T00:00:00</Accepted>
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
        <RateEntryCollection>"
			+ rateEntryXmlWithNoStartEnd.Replace("<RateStartDate></RateStartDate><RateEndDate></RateEndDate>",
				"<RateStartDate>2018-1-1</RateStartDate><RateEndDate></RateEndDate>")
			+ rateEntryXmlWithNoStartEnd.Replace("<RateStartDate></RateStartDate><RateEndDate></RateEndDate>",
				"<RateStartDate>2019-1-1</RateStartDate><RateEndDate></RateEndDate><ViaLRC>NZAKL</ViaLRC>")
			+ @"
        </RateEntryCollection>
        <OrgHeader>
          <Code>AAAAA</Code>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
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

			Factory.Save();

			#endregion

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(xml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 2 inserts, 0 updates, 0 deletes
RateLines - 2 inserts, 0 updates, 0 deletes
RateLineItems - 2 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.AllEntries
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals(2, loadedEntries.Count);

				CombineAssertions(() =>
				{
					AssertEquals("[0].TI_RateCategory", "FCL", loadedEntries[0].TI_RateCategory);
					AssertEquals("[0].TI_OriginLRC", "AUSYD", loadedEntries[0].TI_OriginLRC);
					AssertEquals("[0].TI_DestinationLRC", "USLAX", loadedEntries[0].TI_DestinationLRC);
					AssertEquals("[0].TI_ViaLRC", "", loadedEntries[0].TI_ViaLRC);
					AssertEquals("[0].TI_RateStartDate", new ZDateTime(2018, 01, 1), loadedEntries[0].TI_RateStartDate);
					AssertEquals("[0].TI_RateEndDate.IsEmpty", true, loadedEntries[0].TI_RateEndDate.IsEmpty);

					AssertEquals("[1].TI_RateCategory", "FCL", loadedEntries[1].TI_RateCategory);
					AssertEquals("[1].TI_OriginLRC", "AUSYD", loadedEntries[1].TI_OriginLRC);
					AssertEquals("[1].TI_DestinationLRC", "USLAX", loadedEntries[1].TI_DestinationLRC);
					AssertEquals("[1].TI_ViaLRC", "NZAKL", loadedEntries[1].TI_ViaLRC);
					AssertEquals("[1].TI_RateStartDate", new ZDateTime(2019, 1, 1), loadedEntries[1].TI_RateStartDate);
					AssertEquals("[1].TI_RateEndDate.IsEmpty", true, loadedEntries[1].TI_RateEndDate.IsEmpty);
				});
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateEqualOriginalStartDateAndNewEndDateIsInOriginalRange()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-01T00:00:00</RateStartDate>
            <RateEndDate>2014-08-25T00:00:00</RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 25), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 26), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateStartDate changed from value 1/08/2014 12:00:00 AM to value 26/08/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[GuiTest, TestDate(2017, 04, 04)]
		public void TestImportRateEntryWithExistingDateRangeOverlap_DoesNotThrowUnhandledException()
		{
			#region RatingHeaderXml
			string xml = @"<?xml version=""1.0"" encoding =""utf-8"" ?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" version = ""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteCancellationReason></QuoteCancellationReason>
        <QuoteDate></QuoteDate>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate></FollowUpDate>
        <Accepted>2017-03-31T00:00:00</Accepted>
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
        <SystemLastEditTimeUtc>2017-03-31T04:12:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2017-03-31T03:24:00</SystemCreateTimeUtc>
        <RateEntryCollection>
          <RateEntry Action=""MERGE"">
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <ContractNumber></ContractNumber>
            <RateStartDate>2017-03-31T00:00:00</RateStartDate>
            <RateEndDate></RateEndDate>
            <TransitTime></TransitTime>
            <Frequency>0</Frequency>
            <FrequencyUnit></FrequencyUnit>
            <CartagePickupAddressPostCode></CartagePickupAddressPostCode>
            <CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
            <OriginLRC Relationship=""COU"">AU</OriginLRC>
            <DestinationLRC></DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <PageHeading></PageHeading>
            <PageOpeningText></PageOpeningText>
            <PageClosingText></PageClosingText>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
            <BuyersConsolRateMode></BuyersConsolRateMode>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <FromSuburb TableName=""RefCityTown"" />
            <ToSuburb TableName=""RefCityTown"" />
            <SystemCreateTimeUtc>2017-03-31T14:06:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2017-03-31T04:12:00</SystemLastEditTimeUtc>
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
                <LineOrder>0</LineOrder>
                <RateDesc></RateDesc>
                <RateDescLocal></RateDescLocal>
                <ConversionFactor>0.000</ConversionFactor>
                <WeightVolume></WeightVolume>
                <WeightVolumeMultiple>0.0</WeightVolumeMultiple>
                <RateCalculator>FLT</RateCalculator>
                <FeeChargeLevel></FeeChargeLevel>
                <CompanyTariffLevel>0</CompanyTariffLevel>
                <ActualPercentage>0</ActualPercentage>
                <Rounding>DEF</Rounding>
                <IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
                <IsOnPallets>false</IsOnPallets>
                <RoundingFactor>0.000</RoundingFactor>
                <Condition></Condition>
                <FeeChargeType></FeeChargeType>
                <FactorNumerator></FactorNumerator>
                <FactorDenominator></FactorDenominator>
                <IsTact>false</IsTact>
                <RateLineItemsCollection>
                  <RateLineItems Action=""MERGE"">
                    <LineOrder>0</LineOrder>
                    <Type>BAS</Type>
                    <UnitMultiple>1</UnitMultiple>
                    <BreakMinimum>0.000</BreakMinimum>
                    <Break>0.000</Break>
                    <CallForPricing>false</CallForPricing>
                    <Value>1337.0000</Value>
                    <AgentDeclaredRate>0.0000</AgentDeclaredRate>
                    <FlatAmount>0.0000</FlatAmount>
                    <Text></Text>
                    <BreakWeightVolume></BreakWeightVolume>
                    <PercentOf TableName=""AccChargeCode"" />
                    <DomesticZone TableName=""RateTransportZones"" />
                  </RateLineItems>
                </RateLineItemsCollection>
                <Currency TableName=""RefCurrency"">
                  <Code>AUD</Code>
                </Currency>
                <AccChargeCode>
                  <Code>FRT</Code>
                  <GlbCompany>
                    <Code>EDI</Code>
                  </GlbCompany>
                </AccChargeCode>
                <ProductNumber TableName=""OrgSupplierPart"" />
              </RateLines>
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>AUD</Code>
            </Currency>
            <Supplier TableName=""OrgHeader"" />
            <TransportProvider TableName=""OrgHeader"" />
            <Consignor TableName=""OrgHeader"" />
            <Consignee TableName=""OrgHeader"" />
            <CartagePickupAddressOverride TableName=""OrgAddress"" />
            <CartageDeliveryAddressOverride TableName=""OrgAddress"" />
            <ServiceLevel_NI TableName=""RefServiceLevel"" />
            <CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
            <ViaLRC />
            <AgentOverride TableName=""OrgHeader"" />
            <Warehouse TableName=""WhsWarehouse"" />
            <RefContainer />
            <CommodityCode TableName=""RefCommodityCode"">
              <Code>GEN</Code>
            </CommodityCode>
            <OriginZone TableName=""RateTransportZones"" />
            <DestinationZone TableName=""RateTransportZones"" />
            <ControllingCustomer TableName=""OrgHeader"" />
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </Publisher>
          </RateEntry>
        </RateEntryCollection>
        <FirstSignatory TableName=""GlbStaff"" />
        <SecondSignatory TableName=""GlbStaff"" />
        <OrgHeader>
          <Code>AAAAAA</Code>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
          <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
        </GlbCompany>
      </RatingHeader>
    </Rate>
  </Body>
</Native>
";
			#endregion

			#region Prepare Existing Rates

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAAAAA";

			var header = Factory.NewWithValidTestData<ClientRate>();
			header.TH_OH = orgHeader.PK;
			header.TH_QuoteNumber = string.Empty;

			var entry = header.AddRateEntry("AIR", "LSE", "AU", "");
			entry.TI_RateStartDate = new ZDate(2017, 03, 31);
			entry.TI_RateEndDate = new ZDate(2017, 09, 01);

			Factory.Save();

			#endregion

			var preparedXml = string.Format(xml, orgHeader.PK);

			var encoding = new UTF8Encoding();
			using (var newForm = new DataImportForm<XElement>())
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				newForm.ImportService = new ImportServiceManagerForTesting().ImportService;
				newForm.ImportService.Import(stream);

				var newFactory = new BusinessObjectFactory();
				var loadedHeader = newFactory.Load<RatingHeader>(header.PK);
				var loadedEntries = loadedHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection
					.Cast<RateEntry>()
					.ToList();

				AssertEquals(1, loadedEntries.Count);
				AssertEquals(new ZDateTime(2017, 03, 31), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2017, 09, 01), loadedEntries[0].TI_RateEndDate);

				#region Expected Log
				string expectedLog = @"
Start Import Process
-----------------------------------------------------------------
Record: 
<Rate version=""2.0"">
  <RatingHeader Action=""MERGE"">
    <IsCancelled>false</IsCancelled>
    <OneTimeQuote>false</OneTimeQuote>
    <QuoteNumber></QuoteNumber>
    <QuoteCancellationReason></QuoteCancellationReason>
    <QuoteDate></QuoteDate>
    <QuoteEndDate></QuoteEndDate>
    <FollowUpDate></FollowUpDate>
    <Accepted>2017-03-31T00:00:00</Accepted>
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
    <SystemLastEditTimeUtc>2017-03-31T04:12:00</SystemLastEditTimeUtc>
    <SystemCreateTimeUtc>2017-03-31T03:24:00</SystemCreateTimeUtc>
...

failed to Import:
Incoming RateEntry date range conflicts with existing RateEntry.
Existing RateEntry date range: 31-Mar-17 00:00:00 - 01-Sep-17 00:00:00.
Incoming RateEntry date range 31-Mar-17 00:00:00 - Empty date
Mode[LSE]
RateCategory[AIR]
OriginLRC[AU]
-----------------------------------------------------------------
Import Process Finished
-----------------------------------------------------------------
				".Trim();
				#endregion

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, newForm.ProgressTextBox.Text.Trim());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenOriginalRangeIsInNewDateRange()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-07-01T00:00:00</RateStartDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

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
					.ToList();

				AssertEquals(1, loadedEntries.Count);

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[0].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Rate failed to Import:
Incoming RateEntry date range conflicts with existing RateEntry.
Existing RateEntry date range: 01-Aug-14 00:00:00 - 31-Aug-14 00:00:00.
Incoming RateEntry date range 01-Jul-14 00:00:00 - 05-Sep-14 00:00:00
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewRangeIsInOriginalDateRange()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-15T00:00:00</RateStartDate>
            <RateEndDate>2014-08-31T00:00:00</RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 09, 30);

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateEndDate changed from value 30/09/2014 12:00:00 AM to value 14/08/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenEndDateEqualOriginalStartDate()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-01T00:00:00</RateStartDate>
            <RateEndDate>2014-08-31T00:00:00</RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 31);
			entry.TI_RateEndDate = new ZDate(2014, 09, 30);

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 09, 1), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 30), loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateStartDate changed from value 31/08/2014 12:00:00 AM to value 1/09/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateIsLessThenOriginalStartDateAndNewEndDateIsInOriginalRange()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 07, 15), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 16), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateStartDate changed from value 1/08/2014 12:00:00 AM to value 16/08/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateIsLessThenOriginalStartDateAndOriginalEndDateIsNotDefined()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = ZDate.Empty;

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 07, 15), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 16), loadedEntries[1].TI_RateStartDate);
				AssertEquals(ZDateTime.Empty, loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateStartDate changed from value 1/08/2014 12:00:00 AM to value 16/08/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewDateRangeIsLessThanOriginalDateRange()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-01T00:00:00</RateStartDate>
            <RateEndDate>2014-08-31T00:00:00</RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 09, 1);
			entry.TI_RateEndDate = new ZDate(2014, 09, 30);

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 31), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 09, 1), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 30), loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewDateRangeInterceptsTwoOriginalDateRanges()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-15T00:00:00</RateStartDate>
            <RateEndDate>2014-09-15T00:00:00</RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry1 = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1.TI_RH_NKCommodityCode = string.Empty;
			entry1.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry1.TI_RateEndDate = new ZDate(2014, 08, 31);

			var entry2 = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry2.TI_RH_NKCommodityCode = string.Empty;
			entry2.TI_RateStartDate = new ZDate(2014, 09, 1);
			entry2.TI_RateEndDate = new ZDate(2014, 09, 30);

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

				AssertEquals(3, loadedEntries.Count);

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 15), loadedEntries[1].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[2].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[2].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[2].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 09, 16), loadedEntries[2].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 30), loadedEntries[2].TI_RateEndDate);

				string expectedLog1 = @"
Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateStartDate changed from value 1/09/2014 12:00:00 AM to value 16/09/2014 12:00:00 AM
				".Trim();

				string expectedLog2 = @"
Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateEndDate changed from value 31/08/2014 12:00:00 AM to value 14/08/2014 12:00:00 AM
				".Trim();

				var logs = manager.GetLogs();
				Assert(logs.Contains(expectedLog1));
				Assert(logs.Contains(expectedLog2));
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewDateRangeInterceptsTwoOriginalDateRangesWithGapBetweenOriginalRanges()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-15T00:00:00</RateStartDate>
            <RateEndDate>2014-09-15T00:00:00</RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry1 = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1.TI_RH_NKCommodityCode = string.Empty;
			entry1.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry1.TI_RateEndDate = new ZDate(2014, 08, 25);

			var entry2 = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry2.TI_RH_NKCommodityCode = string.Empty;
			entry2.TI_RateStartDate = new ZDate(2014, 09, 5);
			entry2.TI_RateEndDate = new ZDate(2014, 09, 30);

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

				AssertEquals(3, loadedEntries.Count);

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 08, 14), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[1].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 15), loadedEntries[1].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[2].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[2].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[2].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 09, 16), loadedEntries[2].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 30), loadedEntries[2].TI_RateEndDate);

				string expectedLog1 = @"
Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateStartDate changed from value 5/09/2014 12:00:00 AM to value 16/09/2014 12:00:00 AM
				".Trim();

				string expectedLog2 = @"
Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateEndDate changed from value 25/08/2014 12:00:00 AM to value 14/08/2014 12:00:00 AM
				".Trim();

				var logs = manager.GetLogs();
				Assert(logs.Contains(expectedLog1));
				Assert(logs.Contains(expectedLog2));
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateInOriginalRangeAndNewEndDateIsNotDefined()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-15T00:00:00</RateStartDate>
            <RateEndDate></RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 08, 1);
			entry.TI_RateEndDate = new ZDate(2014, 08, 31);

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

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDate(2014, 08, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDate(2014, 08, 14), loadedEntries[0].TI_RateEndDate);

				AssertEquals("AIR", loadedEntries[1].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[1].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[1].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 08, 15), loadedEntries[1].TI_RateStartDate);
				AssertEquals(ZDateTime.Empty, loadedEntries[1].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------


Existing RateEntry was expired by incoming RateEntry
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
RateEndDate changed from value 31/08/2014 12:00:00 AM to value 14/08/2014 12:00:00 AM


Processed: Rate
--- Import Process Finished -----------------------------------------------------------
RatingHeader - 0 inserts, 1 updates, 0 deletes
RateEntry - 1 inserts, 0 updates, 0 deletes
RateLines - 1 inserts, 0 updates, 0 deletes
RateLineItems - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateLessThanOriginalRangeAndNewEndDateIsNotDefined()
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2014-08-15T00:00:00</RateStartDate>
            <RateEndDate></RateEndDate>
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
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
			var entry = ratingHeader.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_RH_NKCommodityCode = string.Empty;
			entry.TI_RateStartDate = new ZDate(2014, 09, 1);
			entry.TI_RateEndDate = new ZDate(2014, 09, 30);

			Factory.Save();

			#endregion

			var preparedXml = string.Format(xml, orgHeader.PK);

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				var currentThread = System.Threading.Thread.CurrentThread;
				var currentCulture = currentThread.CurrentCulture;

				try
				{
					currentThread.CurrentCulture = new CultureInfo("en-AU");
					manager.ImportService.Import(stream);
				}
				finally
				{
					currentThread.CurrentCulture = currentCulture;
				}

				var newFactory = new BusinessObjectFactory();
				var loadedRatingHeader = newFactory.LoadTop1<RatingHeader>(new ZQuery());
				var loadedEntries = loadedRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection
					.Cast<RateEntry>()
					.OrderBy(x => x.TI_RateStartDate)
					.ToList();

				AssertEquals(1, loadedEntries.Count);

				AssertEquals("AIR", loadedEntries[0].TI_RateCategory);
				AssertEquals("AUSYD", loadedEntries[0].TI_OriginLRC);
				AssertEquals("USLAX", loadedEntries[0].TI_DestinationLRC);
				AssertEquals(new ZDateTime(2014, 09, 1), loadedEntries[0].TI_RateStartDate);
				AssertEquals(new ZDateTime(2014, 09, 30), loadedEntries[0].TI_RateEndDate);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Rate failed to Import:
Incoming RateEntry date range conflicts with existing RateEntry.
Existing RateEntry date range: 01-Sep-14 00:00:00 - 30-Sep-14 00:00:00.
Incoming RateEntry date range 15-Aug-14 00:00:00 - Empty date
Mode[LSE]
RateCategory[AIR]
OriginLRC[AUSYD]
DestinationLRC[USLAX]
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		[TestDate(2014, 08, 14)]
		public void TestImportWhenNewStartDateIsEmpty()
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
      <RatingHeader Action=""INSERT"">
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
          <RateEntry Action=""INSERT"">
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate></RateStartDate>
            <RateEndDate></RateEndDate>
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
              <RateLines Action=""INSERT"">
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                  <RateLineItems Action=""INSERT"">
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
				AssertNull(loadedRatingHeader);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Rate failed to Import:
Validation errors found in Native XML:
RatingHeader.RateEntry.RateStartDate validation failed: Field cannot be empty/null
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}
		}

		#endregion

		#region RatingHeaderXml

		const string RatingHeaderXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
            <PK>67498628-3939-4f1a-bafd-f61fa2bb24b8</PK>
            <Mode>LSE</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>AIR</RateCategory>
            <RateStartDate>2013-01-01T00:00:00</RateStartDate>
            <RateEndDate></RateEndDate>
            <TransitTime></TransitTime>
            <Frequency>0</Frequency>
            <FrequencyUnit></FrequencyUnit>
            <CartagePickupAddressPostCode></CartagePickupAddressPostCode>
            <CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AAAAAA</OriginLRC>
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
            <SystemCreateTimeUtc>2016-08-22T23:22:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2016-09-09T03:29:00</SystemLastEditTimeUtc>
            <RateKey>1732228795</RateKey>
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
                <PK>584eddec-9409-4b7d-a6ea-785fe13c49f5</PK>
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
                    <PK>a3398ca0-6f22-4691-9948-85943a90a640</PK>
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
              <RateLines Action=""MERGE"">
                <PK>ba9857f6-84c7-4d52-ac29-f8898335af94</PK>
                <LineOrder>1</LineOrder>
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
                    <PK>dd23b6a1-13d0-43ec-8bb5-88d510bc7fe0</PK>
                    <LineOrder>0</LineOrder>
                    <Type>UNT</Type>
                    <BreakMinimum>0.000</BreakMinimum>
                    <Break>0.000</Break>
                    <BreakWeightVolume></BreakWeightVolume>
                    <Value>30.0000</Value>
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
                  <Code>CAF</Code>
                  <PK>c53a36a8-37d5-4e96-8315-ac3a57b5a225</PK>
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
          <RateEntry Action=""MERGE"">
            <PK>5d046016-78fa-4d83-9efb-d59966ed69f3</PK>
            <Mode>ALL</Mode>
            <LineOrder>1</LineOrder>
            <RateCategory>ORG</RateCategory>
            <RateStartDate>2013-11-06T00:00:00</RateStartDate>
            <RateEndDate></RateEndDate>
            <TransitTime></TransitTime>
            <Frequency>0</Frequency>
            <FrequencyUnit></FrequencyUnit>
            <CartagePickupAddressPostCode></CartagePickupAddressPostCode>
            <CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
            <WeightVolume></WeightVolume>
            <OriginLRC>AU</OriginLRC>
            <DestinationLRC Relationship=""COU"">ZZ</DestinationLRC>
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
                <PK>04b0f8f3-fa44-4598-ac62-80ca89cf17dd</PK>
                <LineOrder>0</LineOrder>
                <RateDesc></RateDesc>
                <ConversionFactor>0.000</ConversionFactor>
                <WeightVolume>CI</WeightVolume>
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
                    <PK>7af6a67f-605f-4f44-b69b-40169c5419c8</PK>
                    <LineOrder>0</LineOrder>
                    <Type>UNT</Type>
                    <BreakMinimum>0.000</BreakMinimum>
                    <Break>0.000</Break>
                    <BreakWeightVolume></BreakWeightVolume>
                    <Value>20.0000</Value>
                    <AgentDeclaredRate>0.0000</AgentDeclaredRate>
                    <FlatAmount>0.0000</FlatAmount>
                    <Text></Text>
                    <CallForPricing>false</CallForPricing>
                    <UnitMultiple>1</UnitMultiple>
                  </RateLineItems>
                </RateLineItemsCollection>
                <Currency TableName=""RefCurrency"">
                  <Code>AUD</Code>
                  <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
                </Currency>
                <AccChargeCode>
                  <Code>OCART</Code>
                  <PK>5289b6ba-1743-44a3-86c0-948f33197409</PK>
                  <GlbCompany>
                    <Code>EDI</Code>
                    <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
                  </GlbCompany>
                </AccChargeCode>
              </RateLines>
              <RateLines Action=""MERGE"">
                <PK>d22da787-e726-4b0e-b6f1-b1110f5e1da4</PK>
                <LineOrder>1</LineOrder>
                <RateDesc></RateDesc>
                <ConversionFactor>0.000</ConversionFactor>
                <WeightVolume></WeightVolume>
                <WeightVolumeMultiple>0.0</WeightVolumeMultiple>
                <RateCalculator>FLT</RateCalculator>
                <CompanyTariffLevel>0</CompanyTariffLevel>
                <Rounding>DEF</Rounding>
                <IsOnPallets>false</IsOnPallets>
                <IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
                <RoundingFactor>0.000</RoundingFactor>
                <ActualPercentage>0</ActualPercentage>
                <Condition></Condition>
                <RateLineItemsCollection>
                  <RateLineItems Action=""MERGE"">
                    <PK>e6c1ebdf-1626-4148-bfd2-43f52273e350</PK>
                    <LineOrder>0</LineOrder>
                    <Type>BAS</Type>
                    <BreakMinimum>0.000</BreakMinimum>
                    <Break>0.000</Break>
                    <BreakWeightVolume></BreakWeightVolume>
                    <Value>0.0000</Value>
                    <AgentDeclaredRate>0.0000</AgentDeclaredRate>
                    <FlatAmount>0.0000</FlatAmount>
                    <Text></Text>
                    <CallForPricing>false</CallForPricing>
                    <UnitMultiple>1</UnitMultiple>
                  </RateLineItems>
                </RateLineItemsCollection>
                <Currency TableName=""RefCurrency"">
                  <Code>AUD</Code>
                  <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
                </Currency>
                <AccChargeCode>
                  <Code>FRT</Code>
                  <PK>7a5f264b-b413-4b18-8df2-9002cda53b7c</PK>
                  <GlbCompany>
                    <Code>EDI</Code>
                    <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
                  </GlbCompany>
                </AccChargeCode>
              </RateLines>
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </Currency>
            <ServiceLevel_NI TableName=""RefServiceLevel"">
              <Code>D2D</Code>
              <PK>d1b6d8a0-5ca7-4cf5-8cd7-64e1d10fb87a</PK>
            </ServiceLevel_NI>
            <ViaLRC Relationship=""COU"">UA</ViaLRC>
            <CommodityCode TableName=""RefCommodityCode"">
              <Code>GEN</Code>
              <PK>e3ca23c3-2dbf-4660-bd2b-77573b26cfaa</PK>
            </CommodityCode>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </Publisher>
          </RateEntry>
        </RateEntryCollection>
        <OrgHeader>
          <Code>4BELEVSYD</Code>
          <PK>24181eea-d3e5-4afe-8892-8684ab555879</PK>
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

		#region Snail Tests for Performance

		[SnailTest]
		public void TestNativeXMLExportPerformanceDoNotExcessivelyDegrade()
		{
			var factory = new BusinessObjectFactory();
			var helper = new TestHelper(factory);
			var costing = helper.NewCosting(helper.NewOrgHeader());
			using (costing.GetValidationSuspender())
			{
				var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AUSYD", "GBLON");
				rateEntry.AddRateLine("FRT", CombinedCalculator.Code, "KG");
				AddRateEntries(rateEntry, 399, "A");
				factory.Save();

				var generator = new NativeXMLGenerator();
				generator.GenerateNativeXML(costing);

				var query = new ZQuery(RateEntrySchema.TI_TH, costing.PK);
				query.AddToFilter(RateEntrySchema.TI_RateCategory, RatingConstants.RateCategory.LCL);
				var countRateEntriesSmall = Factory.GetDatabaseCount(typeof(RateEntry), query);

				var smallExportRatio = generator.StopWatch.ElapsedTicks / countRateEntriesSmall;
				var smallExportSeconds = generator.StopWatch.Elapsed.TotalSeconds;
				var smallExportBytes = generator.BytesExported;

				AddRateEntries(rateEntry, 3599, "B");
				factory.Save();

				generator.GenerateNativeXML(costing);
				var countRateEntriesLarge = Factory.GetDatabaseCount(typeof(RateEntry), query);
				var largeExportRatio = generator.StopWatch.ElapsedTicks / countRateEntriesLarge;
				var largeExportSeconds = generator.StopWatch.Elapsed.TotalSeconds;
				var largeExportBytes = generator.BytesExported;

				Assert("Pre-condition: smallExportRatio != 0", smallExportRatio != 0);
				Assert("\r\nSmall Time Taken: " + smallExportSeconds + " seconds for " + smallExportBytes + " bytes - " + (smallExportBytes / smallExportSeconds) + " bytes per second."
					+ "\r\nLarge Time Taken: " + largeExportSeconds + " seconds for " + largeExportBytes + " bytes - " + (largeExportBytes / largeExportSeconds) + " bytes per second."
					+ $"\r\nTime per Rate Entry, Small Export: {smallExportRatio}, Large Export: {largeExportRatio}"
					, (largeExportRatio / smallExportRatio) < 3);
			}

			// BG 08-Nov-2016
			// We should be able to drop the "magic number" above back to 1 if we can make Native XML scale out properly. Right now it's sitting at around 1.1-1.2. 
			// Pushed out to 2 to make sure this does not become an intermittantly failing test. Make sure we don't blow stuff out. Times are at around 2.3 and 25 seconds. (small / large)
		}

		public void TestImport_InsertRateEntryDbHits()
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
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteDateTime>2019-09-04T00:00:00</QuoteDateTime>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate>2019-09-11T00:00:00</FollowUpDate>
        <Accepted>2019-11-14T00:00:00</Accepted>
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
          <RateEntry Action=""INSERT"">
            <Mode>SEA</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>FCL</RateCategory>
            <RateStartDate>2019-09-01T00:00:00</RateStartDate>
            <RateEndDate>2019-09-30T00:00:00</RateEndDate>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40GP</Code>
			</RefContainer>
            <RateLinesCollection>
              <RateLines Action=""INSERT"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<RateLineItemsCollection>
					<RateLineItems Action=""INSERT"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>
          <RateEntry Action=""INSERT"">
            <Mode>SEA</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>FCL</RateCategory>
            <RateStartDate>2019-09-01T00:00:00</RateStartDate>
            <RateEndDate>2019-09-30T00:00:00</RateEndDate>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40HC</Code>
			</RefContainer>
            <RateLinesCollection>
              <RateLines Action=""INSERT"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<RateLineItemsCollection>
					<RateLineItems Action=""INSERT"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>
        </RateEntryCollection>
        <OrgHeader>
          <Code>AAAAA</Code>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
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

			Factory.Save();

			#endregion

			var preparedXml = xml;

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				var importService = manager.ImportService;
				importService.TablesForHitQueryCollection = new[] { RateEntry.Schema.TableName };
				importService.Import(stream);

				AssertEquals("No errors", "", string.Join("\r\n", manager.Logs.Buffer.Errors));

				var newFactory = new BusinessObjectFactory();
				var loadedEntries = newFactory.Load<RateEntry>(new ZQuery());
				AssertEquals(2, loadedEntries.Length);

				var rowFactories = new[] { importService.LastRowFactoryForTest, importService.BehaviourRowFactoryForTest };
				AssertTableHits(rowFactories, RateEntry.Schema.TableName, 1);
			}
		}

		public void TestImport_MergeRateEntryDbHits()
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
        <IsCancelled>false</IsCancelled>
        <OneTimeQuote>false</OneTimeQuote>
        <QuoteNumber></QuoteNumber>
        <QuoteDateTime>2019-09-04T00:00:00</QuoteDateTime>
        <QuoteEndDate></QuoteEndDate>
        <FollowUpDate>2019-09-11T00:00:00</FollowUpDate>
        <Accepted>2019-11-14T00:00:00</Accepted>
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
            <Mode>SEA</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>FCL</RateCategory>
            <RateStartDate>2019-09-01T00:00:00</RateStartDate>
            <RateEndDate>2019-09-30T00:00:00</RateEndDate>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40GP</Code>
			</RefContainer>
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<RateLineItemsCollection>
					<RateLineItems Action=""MERGE"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>
          <RateEntry Action=""MERGE"">
            <Mode>SEA</Mode>
            <LineOrder>0</LineOrder>
            <RateCategory>FCL</RateCategory>
            <RateStartDate>2019-09-01T00:00:00</RateStartDate>
            <RateEndDate>2019-09-30T00:00:00</RateEndDate>
            <WeightVolume>KG</WeightVolume>
            <OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
            <DestinationLRC>USLAX</DestinationLRC>
            <IsCrossTrade>false</IsCrossTrade>
            <MatchContainerRateClass>false</MatchContainerRateClass>
            <QuotePageIncoTerm></QuotePageIncoTerm>
            <DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40HC</Code>
			</RefContainer>
            <RateLinesCollection>
              <RateLines Action=""MERGE"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<RateLineItemsCollection>
					<RateLineItems Action=""MERGE"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
              </RateLines>             
            </RateLinesCollection>
            <Currency TableName=""RefCurrency"">
              <Code>USD</Code>
            </Currency>
            <Publisher TableName=""GlbCompany"">
              <Code>EDI</Code>
            </Publisher>
          </RateEntry>
        </RateEntryCollection>
        <OrgHeader>
          <Code>AAAAA</Code>
        </OrgHeader>
        <GlbCompany>
          <Code>EDI</Code>
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

			Factory.Save();

			#endregion

			var preparedXml = xml;

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				var importService = manager.ImportService;
				importService.TablesForHitQueryCollection = new[] { RateEntry.Schema.TableName };
				importService.Import(stream);

				AssertEquals("No errors", "", string.Join("\r\n", manager.Logs.Buffer.Errors));

				var newFactory = new BusinessObjectFactory();
				var loadedEntries = newFactory.Load<RateEntry>(new ZQuery());
				AssertEquals(2, loadedEntries.Length);

				// Shold be zero queries to RowFactory since all the queries are direct SQL
				var rowFactory = importService.LastRowFactoryForTest;
				AssertTableHits(rowFactory, RateEntry.Schema.TableName, 0);
				AssertTableHits(rowFactory, RateLine.Schema.TableName, 0);
				AssertTableHits(rowFactory, RateLineItem.Schema.TableName, 0);
				AssertTableHits(rowFactory, StmNoteSchema.Constants.TableName, 0);
			}
		}

		public void TestImport_Logging()
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
		<IsCancelled>false</IsCancelled>
		<OneTimeQuote>false</OneTimeQuote>
		<QuoteNumber></QuoteNumber>
		<QuoteDateTime>2019-09-04T00:00:00</QuoteDateTime>
		<QuoteEndDate></QuoteEndDate>
		<FollowUpDate>2019-09-11T00:00:00</FollowUpDate>
		<Accepted>2019-11-14T00:00:00</Accepted>
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
			<Mode>SEA</Mode>
			<LineOrder>0</LineOrder>
			<RateCategory>FCL</RateCategory>
			<RateStartDate>2019-09-01T00:00:00</RateStartDate>
			<RateEndDate>2019-09-30T00:00:00</RateEndDate>
			<WeightVolume>KG</WeightVolume>
			<OriginLRC Relationship=""PTC"">AUSYD</OriginLRC>
			<DestinationLRC>USLAX</DestinationLRC>
			<IsCrossTrade>false</IsCrossTrade>
			<MatchContainerRateClass>false</MatchContainerRateClass>
			<QuotePageIncoTerm></QuotePageIncoTerm>
			<DataChecked>false</DataChecked>
			<RefContainer>
				<Code>40GP</Code>
			</RefContainer>
			<RateLinesCollection>
			  <RateLines Action=""MERGE"">
				<LineOrder>0</LineOrder>
				<ConversionFactor>0</ConversionFactor>
				<WeightVolume>CN</WeightVolume>
				<RateCalculator>UNT</RateCalculator>
				<CompanyTariffLevel>0</CompanyTariffLevel>
				<ActualPercentage>0</ActualPercentage>
				<Rounding>DEF</Rounding>
				<Currency>
					<Code>AUD</Code>
				</Currency>
				<AccChargeCode>
					<Code>FRT</Code>
					  <GlbCompany>
						<Code>EDI</Code>
					  </GlbCompany>
				</AccChargeCode>
				<StmNoteCollection>
				  <StmNote Action=""MERGE"">
					<Description>Trade Lane Charge Internal Note</Description>
					<IsCustomDescription>false</IsCustomDescription>
					<NoteText></NoteText>
					<NoteType>INT</NoteType>
					<NoteContext>CIA</NoteContext>
				  </StmNote>
				</StmNoteCollection>
				<RateLineItemsCollection>
					<RateLineItems Action=""MERGE"">
						<LineOrder>0</LineOrder>
						<Type>UNT</Type>
						<UnitMultiple>1</UnitMultiple>
						<Value>1075</Value>
					</RateLineItems>
				</RateLineItemsCollection>
			  </RateLines>
			</RateLinesCollection>
			<Currency TableName=""RefCurrency"">
			  <Code>USD</Code>
			</Currency>
			<Publisher TableName=""GlbCompany"">
			  <Code>EDI</Code>
			</Publisher>
		  </RateEntry>
		</RateEntryCollection>
		<OrgHeader>
		  <Code>AAAAA</Code>
		</OrgHeader>
		<GlbCompany>
		  <Code>EDI</Code>
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

			Factory.Save();

			var initialHeaderLogCount = new BusinessObjectFactory().GetDatabaseCount(typeof(StmALog), new ZQuery(StmALogSchema.SL_Table, RatingHeaderSchema.Constants.TableName));

			#endregion

			var preparedXml = xml;

			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(preparedXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				var importService = manager.ImportService;
				importService.TablesForHitQueryCollection = new[] { RateEntry.Schema.TableName };
				importService.Import(stream);

				AssertEquals("No errors", "", string.Join("\r\n", manager.Logs.Buffer.Errors));

				var newFactory = new BusinessObjectFactory();
				var finalHeaderLogCount = newFactory.GetDatabaseCount(typeof(StmALog), new ZQuery(StmALogSchema.SL_Table, RatingHeaderSchema.Constants.TableName));
				CombineAssertions(() =>
				{
					AssertEquals("should be 1 additional RatingHeader log (DIM)", 1, finalHeaderLogCount - initialHeaderLogCount);
					AssertEquals("should be no RateEntry logs", 0, newFactory.GetDatabaseCount(typeof(StmALog), new ZQuery(StmALogSchema.SL_Table, RateEntrySchema.Constants.TableName)));
					AssertEquals("should be no RateLine logs", 0, newFactory.GetDatabaseCount(typeof(StmALog), new ZQuery(StmALogSchema.SL_Table, RateLinesSchema.Constants.TableName)));
					AssertEquals("should be no RateLineItem logs", 0, newFactory.GetDatabaseCount(typeof(StmALog), new ZQuery(StmALogSchema.SL_Table, RateLineItemsSchema.Constants.TableName)));
					AssertEquals("should be no StmNote logs", 0, newFactory.GetDatabaseCount(typeof(StmALog), new ZQuery(StmALogSchema.SL_Table, StmNoteSchema.Constants.TableName)));

					AssertEquals("should be 1 RatingHeader", 1, newFactory.GetDatabaseCount(typeof(RatingHeader)));
					AssertEquals("should be 1 RateEntry", 1, newFactory.GetDatabaseCount(typeof(RateEntry)));
					AssertEquals("should be 1 RateLine", 1, newFactory.GetDatabaseCount(typeof(RateLine)));
					AssertEquals("should be 1 RateLineItem", 1, newFactory.GetDatabaseCount(typeof(RateLineItem)));
					AssertEquals("should be 1 SmtNote", 1, newFactory.GetDatabaseCount(typeof(StmNote), new ZQuery(StmNoteSchema.ST_Description, "Trade Lane Charge Internal Note")));
				});
			}
		}

		public void TestDeleteRateEntryByPKOnly_PKNotFound()
		{
			var globalFRTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalFRTChargeCode.AC_GC = ZGuid.Empty;
			globalFRTChargeCode.AC_Code = "FRT";
			globalFRTChargeCode.AC_Desc = "Global FRT";
			globalFRTChargeCode.AC_ChargeType = "MRG";

			var globalClientRate1 = Factory.New<ClientRate>();
			globalClientRate1.TH_GC = ZGuid.Empty;
			globalClientRate1.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			globalClientRate1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX")
				.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, "KG", "AUD");

			var rateEntry1 = globalClientRate1.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			rateEntry1.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, "KG", "AUD");

			globalClientRate1.AddRateEntry("FCL", "SEA", "AUSYD", "HKHKG")
				.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, "KG", "AUD");

			Guid somePk = Guid.NewGuid();

			Factory.Save();

			string ratingXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <PK>{globalClientRate1.PK.ToString()}</PK>
        <RateType>SAL</RateType>
        <RateEntryCollection>
          <RateEntry Action=""DELETE"">
            <PK>{somePk.ToString()}</PK>
          </RateEntry>
        </RateEntryCollection>
      </RatingHeader>
    </Rate>
  </Body>
</Native>";

			var manager = new ImportServiceManagerForTesting();

			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(ratingXml)))
			{
				var importService = manager.ImportService;
				importService.TablesForHitQueryCollection = new[] { RateEntry.Schema.TableName };
				importService.Import(stream);
			}

			var expectedStartLogs = @"Record: Rate failed to Import:
There is no RateEntry with";
			var actualLogs = string.Join("\r\n", manager.Logs.Buffer.Errors);

			AssertStartsWith("logs", expectedStartLogs, actualLogs);

			var rateEntries = new BusinessObjectFactory().Load<RateEntry>(new ZQuery());
			AssertEquals("Should be rate entries left", 3, rateEntries.Length);
		}

		public void TestDeleteEntity_UnspecificCriteia_Load1RowOnly()
		{
			AssertEntityWithUnspecificCriteia_DontLoadAllRows("DELETE");
		}

		public void TestMergeEntity_UnspecificCriteia_Load1RowOnly()
		{
			AssertEntityWithUnspecificCriteia_DontLoadAllRows("MERGE");
		}

		void AssertEntityWithUnspecificCriteia_DontLoadAllRows(string action)
		{
			var globalFRTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalFRTChargeCode.AC_GC = ZGuid.Empty;
			globalFRTChargeCode.AC_Code = "FRT";
			globalFRTChargeCode.AC_Desc = "Global FRT";
			globalFRTChargeCode.AC_ChargeType = "MRG";

			var globalClientRate1 = Factory.New<ClientRate>();
			globalClientRate1.TH_GC = ZGuid.Empty;
			globalClientRate1.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			globalClientRate1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			globalClientRate1.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			globalClientRate1.AddRateEntry("FCL", "SEA", "AUSYD", "HKHKG");

			Factory.Save();

			string ratingXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUSSYD</OwnerCode>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Rate version=""2.0"">
      <RatingHeader Action=""MERGE"">
        <PK>{globalClientRate1.PK.ToString()}</PK>
        <RateType>SAL</RateType>
        <RateEntryCollection>
          <RateEntry Action=""{action}"">
            <Mode>SEA</Mode>
          </RateEntry>
        </RateEntryCollection>
      </RatingHeader>
    </Rate>
  </Body>
</Native>";

			var manager = new ImportServiceManagerForTesting();

			using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(ratingXml)))
			{
				var importService = manager.ImportService;
				importService.TablesForHitQueryCollection = new[] { RateEntry.Schema.TableName };
				importService.Import(stream);
			}

			var tableHits = manager.ImportService.LastRowFactoryForTest.TableSelects.Where(t => t.TableName == RateEntry.Schema.TableName).First();
			AssertEquals(1, tableHits.Queries.Count());
			AssertContains("Should not load all rows", "SELECT  TOP 1", tableHits.Queries.First().Query);

			var expectedEntiesInDB = action == "DELETE" ? 2 : 3;
			var rateEntries = new BusinessObjectFactory().Load<RateEntry>(new ZQuery());
			AssertEquals("Should be rate entries left", expectedEntiesInDB, rateEntries.Length);
		}

		static void AssertTableHits(RowFactory rowFactory, string tableName, int expectedHits)
		{
			AssertTableHits(new RowFactory[] { rowFactory }, tableName, expectedHits);
		}

		static void AssertTableHits(IEnumerable<RowFactory> rowFactories, string tableName, int expectedHits)
		{
			var hits = new List<TableHitCount>();
			foreach (var rowFactory in rowFactories.Where(x => x != null))
			{
				hits.Add(rowFactory.GetTableHitCount(RateEntry.Schema.TableName));
			}
			var queries = hits.Where(x => x.Queries != null).SelectMany(x => x.Queries).Select(x => x.Query).ToList();
			AssertEquals("Expected hits on " + tableName + " " + string.Join("\r\n\r\n", queries), expectedHits, hits.Sum(x => x.Value));
		}

		static void AddRateEntries(RateEntry rateEntryToClone, int numberOfEntries, string prefix)
		{
			var ports = new List<string>();
			for (var i = 0; i < numberOfEntries; i++)
			{
				ports.Add(prefix + i);
			}

			var collection = rateEntryToClone.Parent.EntryCollections[rateEntryToClone.TI_RateCategory].LazyLoadingCollection;
			foreach (var portText in ports)
			{
				var newRateEntry = collection.AddNew();
				newRateEntry.CopyPersistentValuesFrom(rateEntryToClone);
				newRateEntry.TI_OriginLRC = portText;
			}
		}

		class NativeXMLGenerator
		{
			internal Stopwatch StopWatch { get; private set; }
			internal long BytesExported;

			internal void GenerateNativeXML(BusinessObject entityToExport)
			{
				var timer = new Timer(600000);
				timer.Elapsed += TimerElapsed;
				timer.Start();

				StopWatch = new Stopwatch();
				StopWatch.Start();

				var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
				var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
				var serializer = new NativeXmlSerializer() { Converter = converter };

				using (var stream = serializer.SerializeToStream(entityToExport))
				{
					BytesExported = stream.Length;
				}

				StopWatch.Stop();
				timer.Stop();
			}
		}

		static void TimerElapsed(object sender, ElapsedEventArgs e)
		{
			Assert("This tests has taken too long", false);
		}

		//[SnailTest]
		//public void TestNativeXMLExportPerformanceWithUATAlpha()
		//{
		//	//DO NOT CHECK IN THIS TEST: it relies on data only available in UATAlpha which doesn't exist in a clean test system

		//	var factory = new BusinessObjectFactory();
		//	var company = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEE"));

		//	var query = new ZQuery(RatingHeaderSchema.TH_OH, null);
		//	query.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.CostRate);
		//	query.AddToFilter(RatingHeaderSchema.TH_GC, company.PK);

		//	var costing = factory.LoadTop1<Costing>(query); //loads a specific costing in UAT alpha with lots of rate entries
		//	AssertNotNull(costing);

		//	var generator = new NativeXMLGenerator();
		//	generator.GenerateNativeXML(costing);
		//	Assert("Time Taken: " + generator.StopWatch.Elapsed.TotalSeconds.ToString() + " seconds for " + generator.BytesExported.ToString() + " bytes.", false);

		//	//DO NOT CHECK IN THIS TEST: it relies on data only available in UATAlpha which doesn't exist in a clean test system
		//}

		#endregion
	}
}
