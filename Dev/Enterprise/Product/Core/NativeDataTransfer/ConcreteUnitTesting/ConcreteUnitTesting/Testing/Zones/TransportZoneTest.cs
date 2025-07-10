using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	public class TransportZoneTest : TransactionedTestCase
	{
		public void TestXMLImportsIntoTransportZoneFromXML()
		{
			var preexistingProviders = factory.Load<RateTransportProvider>(new ZQuery());

			var countryCodeAus = "AU";
			var zone1Name = "Zone 1";

			var postCode1 = factory.NewWithValidTestData<RefPostCode>();
			postCode1.RK_CityTownPostCode = "9999";
			postCode1.RK_RN_NKCountry = "AU";
			var postCode2 = factory.NewWithValidTestData<RefPostCode>();
			postCode2.RK_CityTownPostCode = "9998";
			postCode2.RK_RN_NKCountry = countryCodeAus;

			factory.Save();

			var zoneImportData = string.Format(
			$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			  <Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
			  </Header>
			  <Body>
				<TransportZone version=""2.0"">
				  <RateTransportProvider Action=""INSERT"">
					<IsActive>true</IsActive>
					<ZoneType>ALL</ZoneType>
					<ZoneMode>ALL</ZoneMode>
					<Country TableName=""RefCountry"">
					  <Code>{countryCodeAus}</Code>
					</Country>
					<RelatedParty TableName=""OrgHeader""/>
					<ZoneHubLocation TableName=""RefCityTown""/>
					<RateTransportZonesCollection>
					  <RateTransportZones Action=""INSERT"">
						<IsActive>true</IsActive>
						<ZoneName>{zone1Name}</ZoneName>
						<RateTransportZoneItemCollection>
						  <RateTransportZoneItem Action=""INSERT"">
							<FromDistance>0</FromDistance>
							<ToDistance>0</ToDistance>
							<DistanceUnit>KM</DistanceUnit>
							<IsBeyond>false</IsBeyond>
							<CityTown TableName=""RefCityTown""/>
							<Country TableName=""RefCountry"">
							  <Code>{countryCodeAus}</Code>
							</Country>
							<FromPostCode>{postCode1.RK_CityTownPostCode}</FromPostCode>
							<ToPostCode>{postCode2.RK_CityTownPostCode}</ToPostCode>
						  </RateTransportZoneItem>
						</RateTransportZoneItemCollection>
					  </RateTransportZones>
					</RateTransportZonesCollection>
				  </RateTransportProvider>
				</TransportZone>
			  </Body>
			</Native>");
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(zoneImportData);
			writer.Flush();
			stream.Position = 0;

			manager.Import(stream);

			var actualLogs = GetLogsMessage();
			var expectedLogs =
@"RateTransportProvider - 1 inserts, 0 updates, 0 deletes
RateTransportZones - 1 inserts, 0 updates, 0 deletes
RateTransportZoneItem - 1 inserts, 0 updates, 0 deletes";

			AssertMultilineASCIIEquals("Should insert dbo.RateTransportProvider, RateTransportZone, RateTransportZoneItem and 'From' and 'To' Postcodes", expectedLogs, actualLogs);

			var allProviders = factory.Load<RateTransportProvider>(new ZQuery());
			var provider = allProviders.Where(x => !preexistingProviders.Select(y => y.PK).Contains(x.PK)).Single();
			var zone1 = factory.Load<RateTransportZone>(new ZQuery(new ZQuery(RateTransportZonesSchema.TZ_ZoneName, zone1Name), new ZQuery(RateTransportZonesSchema.TZ_TP, provider.PK))).Single();
			var item1 = zone1.Items[0];

			AssertNotNull("Should should have created the Provider", provider);
			AssertEquals("Expected provider Country", countryCodeAus, provider.Country.Code);
			AssertEquals("Expected provider IsActive", true, provider.TP_IsActive);
			AssertEquals("Expected provider ZoneType", "ALL", provider.TP_ZoneType);
			AssertEquals("Expected provider ZoneMode", "ALL", provider.TP_ZoneMode);

			AssertNotNull("Should have created Zone 1", zone1);
			AssertEquals("Expected Zone 1 Code", zone1Name, zone1.TZ_ZoneName);
			AssertEquals("Expected Zone 1 IsActive", true, zone1.TZ_IsActive);

			AssertEquals("Expected item country", countryCodeAus, item1.Country.Code);
			AssertEquals("Expected item 'From' postcode column", postCode1.RK_CityTownPostCode, item1.TQ_FromPostCode);
			AssertNotNull("Expected item 'From' postcode business object found", item1.FromPostCode);
			AssertEquals("Expected item 'From' postcode business object Postcode", postCode1.RK_CityTownPostCode, item1.FromPostCode.RK_CityTownPostCode);
			AssertEquals("Expected item 'From' postcode business object Country", countryCodeAus, item1.FromPostCode.Country.Code);
			AssertEquals("Expected item 'To' postcode column", postCode2.RK_CityTownPostCode, item1.TQ_ToPostCode);
			AssertNotNull("Expected item 'To' postcode business object found", item1.ToPostCode);
			AssertEquals("Expected item 'To' postcode business object Postcode", postCode2.RK_CityTownPostCode, item1.ToPostCode.RK_CityTownPostCode);
			AssertEquals("Expected item 'To' postcode business object Country", countryCodeAus, item1.ToPostCode.Country.Code);
			AssertEquals("Expected item FromDistance", 0, item1.TQ_FromDistance);
			AssertEquals("Expected item ToDistance", 0, item1.TQ_ToDistance);
			AssertEquals("Expected item DistanceUnit", "KM", item1.TQ_DistanceUnit);
			AssertEquals("Expected item IsBeyond", false, item1.TQ_IsBeyond);
		}

		public void TestXMLUpdate()
		{
			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_ZoneType = "ALL";
			provider.TP_IsActive = false;
			provider.TP_ZoneMode = "ALL";
			provider.TP_RN_NKCountry = "AU";
			var zone = factory.NewWithValidTestData<RateTransportZone>();
			zone.TZ_ZoneName = "Zone 1";
			var item1 = factory.NewWithValidTestData<RateTransportZoneItem>();
			item1.TQ_IsBeyond = false;
			var item2 = factory.NewWithValidTestData<RateTransportZoneItem>();
			var postcode1 = factory.NewWithValidTestData<RefPostCode>();
			postcode1.RK_CityTownPostCode = "9899";
			postcode1.RK_RN_NKCountry = "AU";
			var postcode2 = factory.NewWithValidTestData<RefPostCode>();
			postcode2.RK_CityTownPostCode = "9898";
			postcode2.RK_RN_NKCountry = "AU";
			var postcode3 = factory.NewWithValidTestData<RefPostCode>();
			postcode3.RK_CityTownPostCode = "9896";
			postcode3.RK_RN_NKCountry = "AU";
			zone.Items.Add(item1);
			zone.Items.Add(item2);
			item1.TQ_RN_NKCountry = "AU";
			item1.TQ_FromPostCode = postcode2.RK_CityTownPostCode;
			item1.TQ_ToPostCode = postcode1.RK_CityTownPostCode;
			item2.TQ_RN_NKCountry = "AU";
			item2.TQ_FromPostCode = postcode1.RK_CityTownPostCode;
			item2.TQ_ToPostCode = postcode2.RK_CityTownPostCode;
			provider.Zones.Add(zone);
			factory.Save();

			var zoneImportData =
			$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			  <Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
			  </Header>
			  <Body>
				<TransportZone version=""2.0"">
				  <RateTransportProvider Action=""UPDATE"">
					<PK>{provider.PK}</PK>
					<IsActive>true</IsActive>
					<ZoneMode>FCL</ZoneMode>
					<Country TableName=""RefCountry"">
					  <Code>NZ</Code>
					</Country>
					<RateTransportZonesCollection>
					  <RateTransportZones Action=""UPDATE"">
						<PK>{zone.PK}</PK>
						<ZoneName>Zone 11</ZoneName>
						<RateTransportZoneItemCollection>
						  <RateTransportZoneItem Action=""UPDATE"">
							<PK>{item1.PK}</PK>
							<IsBeyond>true</IsBeyond>
							<ToPostCode>9995</ToPostCode>
						  </RateTransportZoneItem>
						  <RateTransportZoneItem Action=""UPDATE"">
							<PK>{item2.PK}</PK>
							<FromPostCode>{postcode3.RK_CityTownPostCode}</FromPostCode>
						  </RateTransportZoneItem>
						</RateTransportZoneItemCollection>
					  </RateTransportZones>
					</RateTransportZonesCollection>
				  </RateTransportProvider>
				</TransportZone>
			  </Body>
			</Native>";

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(zoneImportData);
			writer.Flush();
			stream.Position = 0;

			manager.Import(stream);
			var actualLogs = GetLogsMessage();
			var expectedLogs =
@"RateTransportProvider - 0 inserts, 1 updates, 0 deletes
RateTransportZones - 0 inserts, 1 updates, 0 deletes
RateTransportZoneItem - 0 inserts, 2 updates, 0 deletes";

			AssertMultilineASCIIEquals("Should insert dbo.RateTransportProvider, RateTransportZone, RateTransportZoneItem and 'From' and 'To' Postcodes", expectedLogs, actualLogs);

			factory = new BusinessObjectFactory();

			provider = factory.Load<RateTransportProvider>(provider.PK);

			AssertEquals("Provider zone type is unchanged", "ALL", provider.TP_ZoneType);
			AssertEquals("Provider zone mode changed to FCL", "FCL", provider.TP_ZoneMode);
			AssertEquals("Provider country changed to NZ", "NZ", provider.Country.Code);
			AssertEquals("Provider IsActive changed to true", true, provider.TP_IsActive);
			AssertEquals("Provider zone count unchanged", 1, provider.Zones.Count);
			AssertEquals("Provider zone unchanged", zone.PK, provider.Zones[0].PK);

			zone = provider.Zones[0];
			AssertEquals("Zone Name updated", "Zone 11", zone.TZ_ZoneName);
			AssertEquals("Zone item count unchanged", 2, zone.Items.Count);
			AssertEquals("Zone item 1 unchanged", item1.PK, zone.Items[0].PK);
			AssertEquals("Zone item 2 unchanged", item2.PK, zone.Items[1].PK);

			item1 = zone.Items[0];
			AssertEquals("Item 1 country unchanged", "AU", item1.TQ_RN_NKCountry);
			AssertEquals("Item 1 'To' postcode updated", "9995", item1.TQ_ToPostCode);
			AssertNull("Item 1 new postcode business object not found", item1.ToPostCode);
			AssertEquals("Item 1 IsBeyond changed", true, item1.TQ_IsBeyond);
			item2 = zone.Items[1];
			AssertEquals("Item 2 country unchanged", postcode3.RK_RN_NKCountry, "AU");
			AssertEquals("Item 2 'From' postcode updated", postcode3.RK_CityTownPostCode, item2.TQ_FromPostCode);
			AssertNotNull("Item 2 new postcode business object found", item2.FromPostCode);
		}

		public void TestXMLDelete()
		{
			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			var zone1 = factory.NewWithValidTestData<RateTransportZone>();
			var zone2 = factory.NewWithValidTestData<RateTransportZone>();
			var item1 = factory.NewWithValidTestData<RateTransportZoneItem>();
			item1.TQ_FromPostCode = "1234";
			var item2 = factory.NewWithValidTestData<RateTransportZoneItem>();
			item2.TQ_FromPostCode = "1234";
			var item3 = factory.NewWithValidTestData<RateTransportZoneItem>();
			item3.TQ_FromPostCode = "1234";
			zone1.Items.Add(item1);
			zone2.Items.Add(item2);
			zone2.Items.Add(item3);
			provider.Zones.Add(zone1);
			provider.Zones.Add(zone2);
			factory.Save();

			var zoneImportData =
			$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			  <Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
			  </Header>
			  <Body>
				<TransportZone version=""2.0"">
				  <RateTransportProvider Action=""DELETE"">
					<PK>{provider.PK}</PK>
					<RateTransportZonesCollection>
					  <RateTransportZones Action=""DELETE"">
						<PK>{zone1.PK}</PK>
						<RateTransportZoneItemCollection>
						  <RateTransportZoneItem Action=""DELETE"">
							<PK>{item1.PK}</PK>
						  </RateTransportZoneItem>
						</RateTransportZoneItemCollection>
					  </RateTransportZones>
					  <RateTransportZones Action=""DELETE"">
						<PK>{zone2.PK}</PK>
						<RateTransportZoneItemCollection>
						  <RateTransportZoneItem Action=""DELETE"">
							<PK>{item2.PK}</PK>
						  </RateTransportZoneItem>
						  <RateTransportZoneItem Action=""DELETE"">
							<PK>{item3.PK}</PK>
						  </RateTransportZoneItem>
						</RateTransportZoneItemCollection>
					  </RateTransportZones>
					</RateTransportZonesCollection>
				  </RateTransportProvider>
				</TransportZone>
			  </Body>
			</Native>";

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(zoneImportData);
			writer.Flush();
			stream.Position = 0;

			manager.Import(stream);
			var actualLogs = GetLogsMessage();
			var expectedLogs =
@"RateTransportProvider - 0 inserts, 0 updates, 1 deletes
RateTransportZones - 0 inserts, 0 updates, 2 deletes
RateTransportZoneItem - 0 inserts, 0 updates, 3 deletes";

			AssertMultilineASCIIEquals("Log shows deletes for RateTransportProvider, RateTransportZone, RateTransportZoneItem and 'From' and 'To' Postcodes", expectedLogs, actualLogs);

			factory = new BusinessObjectFactory();

			provider = factory.LoadTop1<RateTransportProvider>(new ZQuery(RateTransportProviderSchema.PK, provider.PK));
			AssertNull("Provider removed", provider);

			zone1 = factory.LoadTop1<RateTransportZone>(new ZQuery(RateTransportZonesSchema.PK, zone1.PK));
			AssertNull("Zone 1 removed", zone1);
			zone2 = factory.LoadTop1<RateTransportZone>(new ZQuery(RateTransportZonesSchema.PK, zone2.PK));
			AssertNull("Zone 2 removed", zone2);

			item1 = factory.LoadTop1<RateTransportZoneItem>(new ZQuery(RateTransportZoneItemSchema.PK, item1.PK));
			AssertNull("Item 1 removed", item1);

			item2 = factory.LoadTop1<RateTransportZoneItem>(new ZQuery(RateTransportZoneItemSchema.PK, item2.PK));
			AssertNull("Item 2 removed", item2);

			item3 = factory.LoadTop1<RateTransportZoneItem>(new ZQuery(RateTransportZoneItemSchema.PK, item3.PK));
			AssertNull("Item 3 removed", item3);
		}

		public void TestXMLUpdateItemsAddedRemovedInsertedAndDeletedFromXML()
		{
			var countryCodeAus = "AU";

			var provider1 = factory.NewWithValidTestData<RateTransportProvider>();
			var zone1 = factory.NewWithValidTestData<RateTransportZone>();
			zone1.TZ_ZoneName = "Zone1";
			var zone2 = factory.NewWithValidTestData<RateTransportZone>();
			zone2.TZ_ZoneName = "Zone2";
			var zone3 = factory.NewWithValidTestData<RateTransportZone>();
			zone3.TZ_ZoneName = "Zone3";
			var zone4 = factory.NewWithValidTestData<RateTransportZone>();
			zone4.TZ_ZoneName = "Zone4";
			var item1 = factory.NewWithValidTestData<RateTransportZoneItem>();
			var item3 = factory.NewWithValidTestData<RateTransportZoneItem>();
			var item4 = factory.NewWithValidTestData<RateTransportZoneItem>();
			var item5 = factory.NewWithValidTestData<RateTransportZoneItem>();
			item1.TQ_FromPostCode = "1234";
			item3.TQ_FromPostCode = "1234";
			item4.TQ_FromPostCode = "1234";
			item5.TQ_FromPostCode = "1234";
			zone1.Items.Add(item1);
			zone3.Items.Add(item3);
			zone4.Items.Add(item4);
			zone4.Items.Add(item5);
			provider1.Zones.Add(zone1);
			provider1.Zones.Add(zone2);
			provider1.Zones.Add(zone3);
			provider1.Zones.Add(zone4);
			factory.Save();

			factory = new BusinessObjectFactory();
			var preexistingProviders = factory.Load<RateTransportProvider>(new ZQuery());

			var zoneImportData =
			$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			  <Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
			  </Header>
			  <Body>
				<TransportZone version=""2.0"">
				  <RateTransportProvider>
					<PK>{provider1.PK}</PK>
					<RateTransportZonesCollection>
						<RateTransportZones Action=""DELETE"">
							<PK>{zone1.PK}</PK>
							<RateTransportZoneItemCollection>
								<RateTransportZoneItem Action=""DELETE"">
									<PK>{item1.PK}</PK>
								</RateTransportZoneItem>
							</RateTransportZoneItemCollection>
						</RateTransportZones>
						<RateTransportZones>
							<PK>{zone2.PK}</PK>
							<RateTransportZoneItemCollection>
								<RateTransportZoneItem Action=""INSERT"">
									<FromPostCode>1234</FromPostCode>
									<Country TableName=""RefCountry"">
									  <Code>{countryCodeAus}</Code>
									</Country>
								</RateTransportZoneItem>
							</RateTransportZoneItemCollection>
						</RateTransportZones>
						<RateTransportZones>
							<PK>{zone3.PK}</PK>
							<RateTransportZoneItemCollection>
							  <RateTransportZoneItem Action=""DELETE"">
								<PK>{item3.PK}</PK>
							  </RateTransportZoneItem>
							</RateTransportZoneItemCollection>
						</RateTransportZones>
						<RateTransportZones Action=""INSERT"">
							<ZoneName>Zone5</ZoneName>
						 </RateTransportZones>
					</RateTransportZonesCollection>
				  </RateTransportProvider>
				</TransportZone>
				<TransportZone version=""2.0"">
				  <RateTransportProvider Action=""INSERT"">
					<Country TableName=""RefCountry"">
						<Code>{countryCodeAus}</Code>
					</Country>
				  </RateTransportProvider>
				</TransportZone>
			  </Body>
			</Native>
";

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(zoneImportData);
			writer.Flush();
			stream.Position = 0;

			manager.Import(stream);
			var actualLogs = GetLogsMessage();
			var expectedLogs =
@"RateTransportZones - 1 inserts, 0 updates, 1 deletes
RateTransportZoneItem - 1 inserts, 0 updates, 2 deletes
RateTransportProvider - 1 inserts, 0 updates, 0 deletes";

			AssertMultilineASCIIEquals("Import log as expected", expectedLogs, actualLogs);

			factory = new BusinessObjectFactory();

			var allProviders = factory.Load<RateTransportProvider>(new ZQuery());
			var newproviders = allProviders.Where(x => !preexistingProviders.Select(y => y.PK).Contains(x.PK));
			AssertEquals(1, newproviders.Count());
			var provider2 = newproviders.Single();
			AssertNotNull("Expected provider 2 created", provider2);

			provider1 = factory.Load<RateTransportProvider>(provider1.PK);

			AssertEquals("Expected 4 zones in provider 1", 4, provider1.Zones.Count);
			AssertArrayEqualsByElements("Provider 1 zones all found in expected order", new ZString[] { zone2.TZ_ZoneName, zone3.TZ_ZoneName, zone4.TZ_ZoneName, "Zone5" }, provider1.Zones.ToArray().Select(x => x.TZ_ZoneName).ToArray());

			AssertEquals("Expected Zone2 item added", 1, provider1.Zones[0].Items.Count);
			AssertEquals("Expected Zone3 item deleted", 0, provider1.Zones[1].Items.Count);
			AssertEquals("Expected Zone4 items unchanged", 2, provider1.Zones[2].Items.Count);
			AssertEquals("Expected Zone5 created without items", 0, provider1.Zones[3].Items.Count);
		}

		public void TestExport()
		{
			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_ZoneType = "ALL";
			provider.TP_IsActive = false;
			provider.TP_ZoneMode = "FCL";
			provider.TP_RN_NKCountry = "NZ";

			var zone = factory.NewWithValidTestData<RateTransportZone>();
			zone.TZ_ZoneName = "Zone 1";
			var item1 = factory.NewWithValidTestData<RateTransportZoneItem>();
			item1.TQ_IsBeyond = true;
			var item2 = factory.NewWithValidTestData<RateTransportZoneItem>();
			var postcode1 = factory.NewWithValidTestData<RefPostCode>();
			postcode1.RK_CityTownPostCode = "9899";
			postcode1.RK_RN_NKCountry = "AU";
			var postcode2 = factory.NewWithValidTestData<RefPostCode>();
			postcode2.RK_CityTownPostCode = "9898";
			postcode2.RK_RN_NKCountry = "AU";
			var postcode3Missing = "9999";
			zone.Items.Add(item1);
			zone.Items.Add(item2);
			item1.TQ_RN_NKCountry = "AU";
			item1.TQ_FromPostCode = postcode2.RK_CityTownPostCode;
			item1.TQ_ToPostCode = postcode1.RK_CityTownPostCode;
			item2.TQ_RN_NKCountry = "AU";
			item2.TQ_FromPostCode = postcode1.RK_CityTownPostCode;
			item2.TQ_ToPostCode = postcode3Missing;
			provider.Zones.Add(zone);
			factory.Save();

			using (var baseStream = NativeDataTransferTestHelper.ExportToStream(provider))
			{
				item1.Delete();
				item2.Delete();
				zone.Delete();
				provider.Delete();

				factory.Save();

				var preexistingProviders = factory.Load<RateTransportProvider>(new ZQuery());

				var writer = new StreamWriter(baseStream);
				baseStream.Position = 0;
				manager.Import(baseStream);
				var actualLogs = GetLogsMessage();
				var expectedLogs =
@"RateTransportProvider - 1 inserts, 0 updates, 0 deletes
RateTransportZones - 1 inserts, 0 updates, 0 deletes
RateTransportZoneItem - 2 inserts, 0 updates, 0 deletes
";

				AssertMultilineASCIIEquals("Import log as expected", expectedLogs, actualLogs);

				var allProviders = factory.Load<RateTransportProvider>(new ZQuery());
				provider = allProviders.Where(x => !preexistingProviders.Select(y => y.PK).Contains(x.PK)).Single();

				AssertEquals("Expected provider zone type", "ALL", provider.TP_ZoneType);
				AssertEquals("Expected provider zone mode", "FCL", provider.TP_ZoneMode);
				AssertEquals("Expected provider country changed to NZ", "NZ", provider.Country.Code);
				AssertEquals("Expected provider IsActive changed to false", false, provider.TP_IsActive);
				AssertEquals("Expected provider zone count unchanged", 1, provider.Zones.Count);

				zone = provider.Zones[0];
				AssertEquals("Expected zone name", "Zone 1", zone.TZ_ZoneName);
				AssertEquals("Expected zone item count", 2, zone.Items.Count);

				item1 = zone.Items[0];
				AssertEquals("Expected item 1 country", "AU", item1.TQ_RN_NKCountry);
				AssertEquals("Expected item 1 'From' postcode", postcode2.RK_CityTownPostCode, item1.TQ_FromPostCode);
				AssertEquals("Expected item 1 'To' postcode", postcode1.RK_CityTownPostCode, item1.TQ_ToPostCode);
				AssertEquals("Expected item 1 IsBeyond", true, item1.TQ_IsBeyond);

				item2 = zone.Items[1];
				AssertEquals("Expected item 2 country", postcode1.RK_RN_NKCountry, "AU");
				AssertEquals("Expected item 2 'From' postcode", postcode1.RK_CityTownPostCode, item2.TQ_FromPostCode);
				AssertNotNull("Expected item 2 'From' postcode business object found", item2.FromPostCode);
				AssertEquals("Expected item 2 'To' postcode", postcode3Missing, item2.TQ_ToPostCode);
				AssertNull("Expected item 2 'To' postcode business object not found", item2.ToPostCode);
			}
		}

		string GetLogsMessage()
		{
			return string.Join(System.Environment.NewLine, dummyLogger.Buffer.Logs().Select(log => log.Message));
		}

		void ErrorOccur(XElement source, Exception ex)
		{
			dummyLogger.Error("Test error: " + ex.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			sessionServices = new AncillaryImportServices();
			dummyLogger = sessionServices.Logger as MemoryLogger;
			manager = new ImportHandler(sessionServices)
			{
				ErrorOccur = ErrorOccur
			};

			factory = new BusinessObjectFactory();
		}

		AncillaryImportServices sessionServices;
		MemoryLogger dummyLogger;
		ImportHandler manager;
		BusinessObjectFactory factory;
	}
}
