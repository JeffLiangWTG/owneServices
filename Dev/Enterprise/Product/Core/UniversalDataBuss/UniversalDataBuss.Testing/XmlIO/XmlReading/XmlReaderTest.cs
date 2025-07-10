using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.TestDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using SampleXMLSource = Enterprise.UniversalDataBuss.XmlIO.XmlWriting.Testing.XmlWriterTest;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.Testing
{
	class XmlReaderTest : TestCaseWithFactory
	{
		public void TestInvalidXml_DoNoErrorReport()
		{
			var shipment = new UberShipment();
			var invalidXml = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>
    <ZZZMandatoryField>0</ZZZMandatoryField>
    <DateCollection UnknownAttribute=""Foo"" Style=""Classic"">
      <Date>
        <Type><>Departure</></Type>
        <Value></Value>
      </Date>
    </DateCollection>
	  <OrganizationCollection>
      <Organization>
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>
";

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(invalidXml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Warning - Line 6: <UberShipment>.<DateCollection> - Unrecognized attribute [UnknownAttribute] found. Attribute value skipped.
Error - Line 8: <UberShipment>.<DateCollection>.<Date>.<Type>Malformed Xml found when parsing simpleType element", logger.Logs);
			}
		}

		public void TestInvalidXmlCharactersInTag()
		{
			var validXml = "大鱼骑着小自行车😊😊😊";
			var invalidXml = "\uFFFE";

			var shipment = new UberShipment();
			var xml = $@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>
    <ReferenceData>{validXml}</ReferenceData>
	  <OrganizationCollection>
      <Organization>
        <Co{invalidXml}mpanyName></CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>
";
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertContains("logger.Logs", @"Error - Line 8: Hexadecimal value 0xFFFE is an invalid XML character.", logger.Logs);
			}
		}

		public void TestInvalidXmlCharactersInElementBody()
		{
			var invalidXml = "\0";

			var shipment = new UberShipment();
			var xml = $@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>
    <ReferenceData>Blah{invalidXml}Blah</ReferenceData>
  </UberShipment>
</UbiquitousShipment>
";
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertContains("logger.Logs", @"Error - Line 5: Hexadecimal value 0x00 is an invalid XML character.", logger.Logs);
			}
		}

		public void TestNothingInsideRoot()
		{
			var shipment = new Shipment();
			var xml = $@"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">";
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertContains("logger.Logs", @"Error - Line 3: Root element <UniversalShipment> must contain one <Shipment> element. <> is not valid in this scope.", logger.Logs);
			}
		}

		public void TestSelfEndingEmptyElementsWithAttributesWork()
		{
			const string xmlMessageText = @"
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>
    <ZZZMandatoryField>0</ZZZMandatoryField>
    <DateCollection Style=""Classic"" />
	  <OrganizationCollection>
      <Organization>
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>
 ";

			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlMessageText)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(() =>
				{
					AssertMultilineASCIIEquals("logger.Logs should have no errors", @"", logger.Logs);
					var dateCollection = shipment.DateCollection;
					AssertNotNull("shipment.DateCollection", dateCollection);
					AssertEquals("dateCollection.Count", 0, dateCollection.Count);
					AssertEquals("dateCollection.Style Attribute Value", CollectionStyle.Classic, dateCollection.Style);
				});
			}
		}

		public void TestOrganizationCodeOverflowForChargeLine()
		{
			var currentCompanyOrg = Factory.New<IOrgHeader>();
			currentCompanyOrg.OH_Code = "~OC";
			IGlbCompany currentCompany = (IGlbCompany)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			currentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			currentCompany.GC_OH_OrgProxy = currentCompanyOrg.PK;
			IGlbBranch currentBranch = (IGlbBranch)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			currentBranch.GB_GC = currentCompany.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(currentBranch.PK.ToGuid()))
			{
				var debtor = Factory.New<IOrgHeader>();
				debtor.OH_Code = "~DD";

				var creditor = Factory.New<IOrgHeader>();
				creditor.OH_Code = "~CC";

				CreateOrgPatternMatchOverride("^DD", Constants.OrgPatternMatchOverrideRelationships.Organisation, debtor.PK);
				CreateOrgPatternMatchOverride("^CC", Constants.OrgPatternMatchOverrideRelationships.Organisation, creditor.PK);

				Factory.Save();
				Factory.ClearQueryCache();

				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

				using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlThatNeedsCodeMappingWithJobCosting)))
				{
					var mapper = new CodeMappingManager(logger);
					new XmlReader(mapper).ReadXML(shipment, stream, logger);

					CombineAssertions(delegate
					{
						var creditorDataObject = shipment.JobCosting.ChargeLineCollection[0].Creditor;
						AssertEquals("creditorDataObject SourceValue", "^CC", creditorDataObject.Key.Value.SourceValue);
						Assert("creditorDataObject no MappedValue", !creditorDataObject.Key.Value.IsMapped);

						var debtorDataObject = shipment.JobCosting.ChargeLineCollection[0].Debtor;
						AssertEquals("debtorDataObject SourceValue", "^DD", debtorDataObject.Key.Value.SourceValue);
						Assert("debtorDataObject no MappedValue", !debtorDataObject.Key.Value.IsMapped);

						creditorDataObject = shipment.JobCosting.ChargeLineCollection[1].Creditor;
						AssertEquals("chargeCodeDataObject SourceValue", "^^CC", creditorDataObject.Key.Value.SourceValue);
						Assert("creditorDataObject IsMapped", !creditorDataObject.Key.Value.IsMapped);

						mapper.UpdateMappedCodes(shipment, Factory);

						AssertMultilineASCIIEquals("logger.Logs", @"
Information - Used code mapping defined in Organization(Code: ~OC) > Config > EDI Code Mapping.
Information - Line 98: Mapped Organization code '^CC' to '~CC'.
Information - Line 102: Mapped Organization code '^DD' to '~DD'.".Trim(),
							logger.Logs.Trim());

						creditorDataObject = shipment.JobCosting.ChargeLineCollection[0].Creditor;
						AssertEquals("creditorDataObject SourceValue", "^CC", creditorDataObject.Key.Value.SourceValue);
						AssertEquals("creditorDataObject MappedValue", "~CC", creditorDataObject.Key.Value.MappedValue);
						Assert("creditorDataObject IsMapped", creditorDataObject.Key.Value.IsMapped);

						debtorDataObject = shipment.JobCosting.ChargeLineCollection[0].Debtor;
						AssertEquals("debtorDataObject SourceValue", "^DD", debtorDataObject.Key.Value.SourceValue);
						AssertEquals("debtorDataObject MappedValue", "~DD", debtorDataObject.Key.Value.MappedValue);
						Assert("debtorDataObject IsMapped", debtorDataObject.Key.Value.IsMapped);

						creditorDataObject = shipment.JobCosting.ChargeLineCollection[1].Creditor;
						AssertEquals("chargeCodeDataObject SourceValue", "^^CC", creditorDataObject.Key.Value.SourceValue);
						Assert("creditorDataObject IsMapped", !creditorDataObject.Key.Value.IsMapped);

						debtorDataObject = shipment.JobCosting.ChargeLineCollection[1].Debtor;
						Assert("debtorDataObject Key is not set", !debtorDataObject.Key.HasValue);
					});
				}
			}
		}

		sealed class MyCodeMapper : IUniversalCodeMapper
		{
			public string GetMappedOrInput(string input, string codeMappingRelationshipCode, int? currentLineNumber = null) => string.Concat(string.Join("", input.Skip(2)), "ZZ");

			public string GetMappedOrEmpty(string input, string codeMappingRelationshipCode, int? currentLineNumber = null) => string.Concat(string.Join("", input.Skip(2)), "ZZ");

			public IOrgPatternMatchOverride CreateOrUpdateCodeMapping(string codeMappingRelationshipCode, string foreignCode, string localCode, ZGuid localGuid) => null;

			public IOrgHeader SourceOrganisation => null;
		}

		sealed class MyCodeMapperProvider : IUniversalCodeMapperProvider
		{
			public MyCodeMapperProvider(IUniversalCodeMapper mapper)
			{
				this.mapper = mapper;
			}

			readonly IUniversalCodeMapper mapper;
			public IUniversalCodeMapper Create(ITopLevelDataObject dataObject, BusinessObjectFactory factory) => mapper;
		}

		public void TestCustomCodeMapper()
		{
			var mockMapper = new MyCodeMapper();
			var provider = new MyCodeMapperProvider(mockMapper);

			var customCodeMappers = new Hashtable
			{
				["ForwardingShipment"] = new TestObjectHandle(provider)
			};

			using (ObjectFactory.Substitute("UniversalCodeMapperProviderList", customCodeMappers))
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

				const string uxml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001003</Key>
        </DataTarget>
      </DataTargetCollection>
      <CodesMappedToTarget>true</CodesMappedToTarget>
    </DataContext>
    <PortOfLoading>
        <Code>AUSYD</Code>
	</PortOfLoading>
    <PortOfDischarge>
		<Code>SGSIN</Code>
	</PortOfDischarge>
  </Shipment>
</UniversalShipment>";

				using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(uxml)))
				{
					var mapper = new CodeMappingManager(logger);
					new XmlReader(mapper).ReadXML(shipment, stream, logger);
					mapper.UpdateMappedCodes(shipment, Factory);

					AssertEquals(nameof(shipment.PortOfLoading), "SYDZZ", shipment.PortOfLoading.Code);
					AssertEquals(nameof(shipment.PortOfDischarge), "SINZZ", shipment.PortOfDischarge.Code);
				}
			}
		}

		public void TestOrganizationCodeOverflow()
		{
			var currentCompanyOrg = Factory.New<IOrgHeader>();
			currentCompanyOrg.OH_Code = "~OC";
			IGlbCompany currentCompany = (IGlbCompany)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			currentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			currentCompany.GC_OH_OrgProxy = currentCompanyOrg.PK;
			IGlbBranch currentBranch = (IGlbBranch)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			currentBranch.GB_GC = currentCompany.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(currentBranch.PK.ToGuid()))
			{
				var unlocoForAUSYD = Factory.LoadTop1<IRefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
				AssertNotNull("Precondition: unLocoForAUSYD", unlocoForAUSYD);
				var australia = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
				AssertNotNull("Precondition: australia", australia);
				var aud = Factory.LoadTop1<IRefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
				AssertNotNull("Precondition: AUD", aud);
				var containerType = Factory.LoadTop1<IRefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
				AssertNotNull("Precondition: 20GP", containerType);
				var serviceLevel = Factory.LoadTop1<IRefServiceLevel>(new ZQuery(RefServiceLevelSchema.RS_Code, "STD"));
				AssertNotNull("Precondition: STD", serviceLevel);
				var orgHeader = Factory.New<IOrgHeader>();
				orgHeader.OH_Code = "~OO";
				IWhsWarehouse warehouse = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
				warehouse.WW_WarehouseCode = "~W";
				IAccChargeCode mappedChargeCode = (IAccChargeCode)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccChargeCode)));
				mappedChargeCode.AC_Code = "~DOF";

				CreateOrgPatternMatchOverride("^AUSY", Constants.OrgPatternMatchOverrideRelationships.Port, unlocoForAUSYD.PK);
				CreateOrgPatternMatchOverride("^OO", Constants.OrgPatternMatchOverrideRelationships.Organisation, orgHeader.PK);
				CreateOrgPatternMatchOverride("^20G", Constants.OrgPatternMatchOverrideRelationships.ContainerType, containerType.PK);
				CreateOrgPatternMatchOverride("^ST", Constants.OrgPatternMatchOverrideRelationships.ServiceLevel, serviceLevel.PK);
				CreateOrgPatternMatchOverride("^W", Constants.OrgPatternMatchOverrideRelationships.Warehouse, warehouse.PK);
				CreateOrgPatternMatchOverride("^A", Constants.OrgPatternMatchOverrideRelationships.Country, australia.PK);
				CreateOrgPatternMatchOverride("^AU", Constants.OrgPatternMatchOverrideRelationships.Currency, aud.PK);
				CreateOrgPatternMatchOverride("^P", Constants.OrgPatternMatchOverrideRelationships.PackageType, "~CE");
				CreateOrgPatternMatchOverride("^F", Constants.OrgPatternMatchOverrideRelationships.IncoTerm, "~OB");
				CreateOrgPatternMatchOverride("^DOF", Constants.OrgPatternMatchOverrideRelationships.ChargeCodes, "~DOF");
				CreateOrgPatternMatchOverride("^E", Constants.OrgPatternMatchOverrideRelationships.EventCode, "~XW");

				Factory.Save();
				Factory.ClearQueryCache();

				var shipment = new UberShipment();

				var newXml = XmlThatNeedsCodeMapping.Replace(@"</OrganizationCollection>", @"</OrganizationCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <OrganizationCode>SHANGHAI TIANQUAN INDUSTRIAL CO.,LTD.</OrganizationCode>
            <CompanyName>SHANGHAI TIANQUAN INDUSTRIAL CO.,LTD.</CompanyName>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <OrganizationCode>SYDNEY DREAM INDUSTRIAL CO.,LTD.</OrganizationCode>
            <CompanyName>SYDNEY DREAM INDUSTRIAL CO.,LTD.</CompanyName>
          </OrganizationAddress>
        </OrganizationAddressCollection>");

				using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(newXml)))
				{
					var mapper = new CodeMappingManager(logger);
					new XmlReader(mapper).ReadXML(shipment, stream, logger);
					mapper.UpdateMappedCodes(shipment, Factory);
					CombineAssertions(delegate
					{
						AssertEquals("shipment.Origin.Code", "AUSYD", shipment.Origin.Code);
						AssertEquals("shipment.OrganizationCollection[0].Code", "~OO", shipment.OrganizationCollection[0].Code);
						AssertEquals("shipment.ContainerCollection[0].ContainerType.Code", "20GP", shipment.ContainerCollection[0].ContainerType.Code);
						AssertEquals("shipment.ServiceLevel.Code", "STD", shipment.ServiceLevel.Code);
						AssertEquals("shipment.Order.Warehouse.Code", "~W", shipment.Order.Warehouse.Code);
						AssertEquals("shipment.OrganizationCollection[0].Country.Code", "AU", shipment.OrganizationCollection[0].Country.Code);
						AssertEquals("shipment.FreightRateCurrency.Code", "AUD", shipment.FreightRateCurrency.Code);
						AssertEquals("shipment.OuterPacksPackageType.Code", "~CE", shipment.OuterPacksPackageType.Code);
						AssertEquals("shipment.ShipmentIncoTerm.Code", "~OB", shipment.ShipmentIncoTerm.Code);
						AssertEquals("shipment.ChargeCode.Code", "~DOF", shipment.ChargeCode.Code);
						AssertEquals("shipment.EventType.Code", "~XW", shipment.EventType.Code);
						AssertEquals("shipment.OrganizationAddressCollection[0].OrganizationCode", "SHANGHAI TIANQUAN INDUSTRIAL CO.,LTD.", shipment.OrganizationAddressCollection[0].OrganizationCode);
						AssertEquals("shipment.OrganizationAddressCollection[1].OrganizationCode", "SYDNEY DREAM INDUSTRIAL CO.,LTD.", shipment.OrganizationAddressCollection[1].OrganizationCode);
						AssertMultilineASCIIEquals("logger.Logs", @"
Information - Used code mapping defined in Organization(Code: ~OC) > Config > EDI Code Mapping.
Information - Line 22: Mapped Port code '^AUSY' to 'AUSYD'.
Information - Line 28: Mapped Package Type code '^P' to '~CE'.
Information - Line 32: Mapped Incoterm code '^F' to '~OB'.
Information - Line 37: Mapped Currency code '^AU' to 'AUD'.
Information - Line 41: Mapped Charge Code code '^DOF' to '~DOF'.
Information - Line 44: Mapped Service Level code '^ST' to 'STD'.
Information - Line 47: Mapped Event Code code '^E' to '~XW'.
Information - Line 53: Mapped Container Type code '^20G' to '20GP'.
Information - Line 61: Mapped Warehouse code '^W' to '~W'.
Information - Line 67: Mapped Organization code '^OO' to '~OO'.
Information - Line 70: Mapped Country/Region code '^A' to 'AU'.
Warning - Line 78: <UberShipment>.<OrganizationAddressCollection>.<OrganizationAddress>.<OrganizationCode> exceeded its maximum length of 12 characters. 37 characters were found.
Warning - Line 83: <UberShipment>.<OrganizationAddressCollection>.<OrganizationAddress>.<OrganizationCode> exceeded its maximum length of 12 characters. 32 characters were found.
					".Trim(), logger.Logs);
					});
				}
			}
		}

		public void TestCodeMappingStringsAreInterned()
		{
			var org1 = Factory.New<IOrgHeader>();
			org1.OH_Code = "~DD";
			var org2 = Factory.New<IOrgHeader>();
			org2.OH_Code = "~CC";
			CreateOrgPatternMatchOverride("^DD", Constants.OrgPatternMatchOverrideRelationships.Organisation, org1.PK);
			CreateOrgPatternMatchOverride("^CC", Constants.OrgPatternMatchOverrideRelationships.Organisation, org2.PK);
			Factory.Save();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlThatNeedsCodeMappingWithJobCosting)))
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				var mapper = new CodeMappingManager(logger);
				new XmlReader(mapper).ReadXML(shipment, stream, logger);
				mapper.UpdateMappedCodes(shipment, Factory);
				var stringPropertiesList = mapper.GetInternedStrings();

				foreach (var stringProperties in stringPropertiesList)
				{
					var uniqueStrings = stringProperties.Distinct().Count();
					var uniqueReferences = stringProperties.Distinct(new LambdaComparer<string>((s1, s2) => object.ReferenceEquals(s1, s2), s => s.GetHashCode())).Count();
					AssertGreaterThan("Precondition: At least one duplicate string required for this test", stringProperties.Count(), uniqueStrings);
					AssertEquals("Strings should be interned by code mapping manager", uniqueStrings, uniqueReferences);
				}
			}
		}

		public void TestUXmlDateTimeParser()
		{
			var shipment = new UberShipment();

			const string uxml = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>
    <ZZZMandatoryField>0</ZZZMandatoryField>
    <XmlDateCollection UnknownAttribute=""Foo"" Style=""Classic"">
      <XmlDate>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2021-05-14T02:05:00</Value>
      </XmlDate>
      <XmlDate>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2021-05-21T02:05:00 +10:00</Value>
      </XmlDate>
      <XmlDate>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
		<Value></Value>
      </XmlDate>
    </XmlDateCollection>
  </UberShipment>
</UbiquitousShipment>
";

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(uxml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);

				var departure = (UXmlDateTime)shipment.XmlDateCollection.Find(d => d.Type == UberXmlDateType.Departure).Value;
				AssertEquals("Time string with no offset parsed as ZDateTime", typeof(ZDateTime), departure.GetDateType());
				AssertEquals("ZDateTime correct", new ZDateTime(2021, 5, 14, 2, 5, 0), (ZDateTime)departure);

				var arrival = (UXmlDateTime)shipment.XmlDateCollection.Find(d => d.Type == UberXmlDateType.Arrival).Value;
				AssertEquals("Time string with offset parsed as ZDateTimeOffset", typeof(ZDateTimeOffset), arrival.GetDateType());
				AssertEquals("ZDateTimeOffset correct", new ZDateTimeOffset(2021, 5, 21, 2, 5, 0, TimeSpan.FromHours(10)), (ZDateTimeOffset)arrival);

				var shipped = (UXmlDateTime)shipment.XmlDateCollection.Find(d => d.Type == UberXmlDateType.ShippedOnBoard).Value;
				AssertEquals("Handles empty date", ZDateTime.Empty, (ZDateTime)shipped);
			}
		}

		public void TestCollectionContentAttibute()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(UberShipmentWithCollectionContentAttibute)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				var dateCollection = shipment.DateCollection;
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"Warning - Line 6: <UberShipment>.<DateCollection> - Unrecognized attribute [UnknownAttribute] found. Attribute value skipped.", logger.Logs);
					AssertNotNull("shipment.DateCollection", dateCollection);
					AssertEquals("dateCollection.Style Attribute Value", CollectionStyle.Classic, dateCollection.Style);
					AssertEquals("dateCollection.Count", 1, dateCollection.Count);
					var dateData = dateCollection[0];
					AssertEquals("dateData.Type", UberDateType.Departure, dateData.Type);
					AssertEquals("dateData.Value", ZDateTime.Empty, dateData.Value);
				});
			}
		}

		#region UberShipmentWithCollectionContentAttibute

		const string UberShipmentWithCollectionContentAttibute = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>
    <ZZZMandatoryField>0</ZZZMandatoryField>
    <DateCollection UnknownAttribute=""Foo"" Style=""Classic"">
      <Date>
        <Type>Departure</Type>
        <Value></Value>
      </Date>
    </DateCollection>
	  <OrganizationCollection>
      <Organization>
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>
";

		#endregion

		public void TestCanReadDataContext_2012_11()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(SampleXMLSource.UniversalShipmentWithDataContext_2012_11)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"", logger.Logs);
					var dataContext = shipment.DataContext;
					var ids = dataContext.GetEnterpriseServerAndCompanyIDs();
					AssertEquals("dataContext.GetEnterpriseServerAndCompanyIDs().EnterpriseID", "EDI", ids.EnterpriseID);
					AssertEquals("dataContext.GetEnterpriseServerAndCompanyIDs().ServerID", "DAT", ids.ServerID);
					AssertEquals("dataContext.GetEnterpriseServerAndCompanyIDs().CompanyCode", "EDI", ids.CompanyCode);
					AssertEquals("dataContext.CodesMappedToTarget", ZBool.False, dataContext.CodesMappedToTarget);
					AssertEquals("dataContext.CompanyCodeToImportInto", "EDI", dataContext.CompanyCodeToImportInto);
					AssertEquals("dataContext.CountryCodeToImportInto", "AU", dataContext.CountryCodeToImportInto);
					AssertEquals("dataContext.DataProviderForCodeMapping", "EDIDATEDI", dataContext.DataProviderForCodeMapping);

					AssertMultilineASCIIEquals("dataContext.ContextKeyValuePairs", @"
Data Source Action Purpose - ACT - My Action
Data Source Company - EDI - Eagle Datamation International
Data Source Data Provider - EDIDATEDI (EnterpriseID)
Data Source Event Reference - TriggerRef
Data Source Trigger Count - 2
Data Source Trigger Date - 13-Apr-12 00:00:00 +10:00
Data Source Trigger Description - Describe me a Trigger
Data Source Trigger Event - EVT - My Event
Data Source Trigger Event User - ATF - Awesome Top Fella
Data Source Trigger Reference - *TriggerRef*
Data Source Trigger Type - Manual
".Trim(), string.Join("\r\n", dataContext.ContextKeyValuePairs.OrderBy(o => o.Key).Select(o => o.Key + " - " + o.Value).ToArray()));

					AssertMultilineASCIIEquals("dataContext.DataSourceCollection", @"
DummyBusinessObject - JOB01010101
".Trim(), string.Join("\r\n", dataContext.DataSourceCollection.Select(o => o.Type.GetValueOrDefault() + " - " + o.Key.GetValueOrDefault()).ToArray()));

					AssertNull("dataContext.DataTargetCollection", dataContext.DataTargetCollection);
				});
			}
		}

		public void TestCanReadDataContext_2011_11()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(SampleXMLSource.UniversalShipmentWithDataContext_2011_11)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"", logger.Logs);
					var dataContext = shipment.DataContext;
					var ids = dataContext.GetEnterpriseServerAndCompanyIDs();
					AssertEquals("dataContext.GetEnterpriseServerAndCompanyIDs().EnterpriseID", "EDI", ids.EnterpriseID);
					AssertEquals("dataContext.GetEnterpriseServerAndCompanyIDs().ServerID", "DAT", ids.ServerID);
					AssertEquals("dataContext.GetEnterpriseServerAndCompanyIDs().CompanyCode", "EDI", ids.CompanyCode);
					AssertEquals("dataContext.CodesMappedToTarget", ZBool.False, dataContext.CodesMappedToTarget);
					AssertEquals("dataContext.CompanyCodeToImportInto", "EDI", dataContext.CompanyCodeToImportInto);
					AssertEquals("dataContext.CountryCodeToImportInto", "AU", dataContext.CountryCodeToImportInto);
					AssertEquals("dataContext.DataProviderForCodeMapping", "EDIDATEDI", dataContext.DataProviderForCodeMapping);

					AssertMultilineASCIIEquals("dataContext.ContextKeyValuePairs", @"
Data Source Action Purpose - ACT - My Action
Data Source Company - EDI - Eagle Datamation International
Data Source Enterprise ID - EDI
Data Source Event Reference - TriggerRef
Data Source Server ID - DAT
Data Source Trigger Count - 2
Data Source Trigger Date - 13-Apr-12 00:00:00 +10:00
Data Source Trigger Description - Describe me a Trigger
Data Source Trigger Event - EVT - My Event
Data Source Trigger Event User - ATF - Awesome Top Fella
Data Source Trigger Reference - *TriggerRef*
Data Source Trigger Type - Manual
".Trim(), string.Join("\r\n", dataContext.ContextKeyValuePairs.OrderBy(o => o.Key).Select(o => o.Key + " - " + o.Value).ToArray()));

					AssertMultilineASCIIEquals("dataContext.DataSourceCollection", @"
DummyBusinessObject - JOB01010101
".Trim(), string.Join("\r\n", dataContext.DataSourceCollection.Select(o => o.Type.GetValueOrDefault() + " - " + o.Key.GetValueOrDefault()).ToArray()));

					AssertNull("dataContext.DataTargetCollection", dataContext.DataTargetCollection);
				});
			}
		}

		public void TestReadShipmentWithAttributesOfAllTypes()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(UberShipmentWithAttributesOfAllTypes)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"", logger.Logs);
					AssertEquals("BUTTER", shipment.Pancake.Type);
					AssertEquals("Pancake with Butter", shipment.Pancake.Description);
					AssertEquals(122, shipment.Pancake.Calories);
					AssertEquals(CookerType.FryingPan, shipment.Pancake.CookedIn);
					AssertEquals(23.45m, shipment.Pancake.Diameter);
					AssertEquals(new ZDateTime(2012, 01, 04), shipment.Pancake.RecipeWritten);
				});
			}
		}

		#region UberShipmentWithAttributesOfAllTypes

		const string UberShipmentWithAttributesOfAllTypes = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <UberShipment>
    <Pancake Calories=""122"" CookedIn = ""FryingPan"" Description=""Pancake with Butter"" Diameter=""23.45"" RecipeWritten=""2012-01-04T00:00:00"">BUTTER</Pancake>

    <ZZZMandatoryField>0</ZZZMandatoryField>
		<OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>Mandatory Company</CompanyName>
        <Code>MAN</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>
";

		#endregion

		public void TestAllTypesOfCodeMappingWorks()
		{
			var currentCompanyOrg = Factory.New<IOrgHeader>();
			currentCompanyOrg.OH_Code = "~OC";
			IGlbCompany currentCompany = (IGlbCompany)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			currentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			currentCompany.GC_OH_OrgProxy = currentCompanyOrg.PK;
			IGlbBranch currentBranch = (IGlbBranch)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbBranch)));
			currentBranch.GB_GC = currentCompany.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(currentBranch.PK.ToGuid()))
			{
				var unlocoForAUSYD = Factory.LoadTop1<IRefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
				AssertNotNull("Precondition: unLocoForAUSYD", unlocoForAUSYD);
				var australia = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
				AssertNotNull("Precondition: australia", australia);
				var aud = Factory.LoadTop1<IRefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
				AssertNotNull("Precondition: AUD", aud);
				var containerType = Factory.LoadTop1<IRefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
				AssertNotNull("Precondition: 20GP", containerType);
				var serviceLevel = Factory.LoadTop1<IRefServiceLevel>(new ZQuery(RefServiceLevelSchema.RS_Code, "STD"));
				AssertNotNull("Precondition: STD", serviceLevel);
				var orgHeader = Factory.New<IOrgHeader>();
				orgHeader.OH_Code = "~OO";
				IWhsWarehouse warehouse = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
				warehouse.WW_WarehouseCode = "~W";
				IAccChargeCode mappedChargeCode = (IAccChargeCode)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccChargeCode)));
				mappedChargeCode.AC_Code = "~DOF";
				var commodity = Factory.LoadTop1<IRefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "MFUE"));
				AssertNotNull(commodity);
				var anotherCommodity = Factory.LoadTop1<IRefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "SHIP"));
				AssertNotNull(anotherCommodity);
				var equipment = (IRefEquipment)Factory.NewWithValidTestData(ObjectFactory.GetType<IRefEquipment>());
				equipment.RQ_ShortCode = "TRK";

				CreateOrgPatternMatchOverride("^AUSY", Constants.OrgPatternMatchOverrideRelationships.Port, unlocoForAUSYD.PK);
				CreateOrgPatternMatchOverride("^OO", Constants.OrgPatternMatchOverrideRelationships.Organisation, orgHeader.PK);
				CreateOrgPatternMatchOverride("^20G", Constants.OrgPatternMatchOverrideRelationships.ContainerType, containerType.PK);
				CreateOrgPatternMatchOverride("^ST", Constants.OrgPatternMatchOverrideRelationships.ServiceLevel, serviceLevel.PK);
				CreateOrgPatternMatchOverride("^W", Constants.OrgPatternMatchOverrideRelationships.Warehouse, warehouse.PK);
				CreateOrgPatternMatchOverride("^A", Constants.OrgPatternMatchOverrideRelationships.Country, australia.PK);
				CreateOrgPatternMatchOverride("^AU", Constants.OrgPatternMatchOverrideRelationships.Currency, aud.PK);
				CreateOrgPatternMatchOverride("^P", Constants.OrgPatternMatchOverrideRelationships.PackageType, "~CE");
				CreateOrgPatternMatchOverride("^F", Constants.OrgPatternMatchOverrideRelationships.IncoTerm, "~OB");
				CreateOrgPatternMatchOverride("^DOF", Constants.OrgPatternMatchOverrideRelationships.ChargeCodes, "~DOF");
				CreateOrgPatternMatchOverride("^E", Constants.OrgPatternMatchOverrideRelationships.EventCode, "~XW");
				CreateOrgPatternMatchOverride("^COM", Constants.OrgPatternMatchOverrideRelationships.Commodities, commodity.PK);
				CreateOrgPatternMatchOverride("^CO2", Constants.OrgPatternMatchOverrideRelationships.Commodities, anotherCommodity.PK);
				CreateOrgPatternMatchOverride("^DM", Constants.OrgPatternMatchOverrideRelationships.DropMode, "HLS");
				CreateOrgPatternMatchOverride("^EQP", Constants.OrgPatternMatchOverrideRelationships.Equipment, equipment.PK);

				Factory.Save();
				Factory.ClearQueryCache();

				var shipment = new UberShipment();
				using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlThatNeedsCodeMapping)))
				{
					var mapper = new CodeMappingManager(logger);
					new XmlReader(mapper).ReadXML(shipment, stream, logger);
					mapper.UpdateMappedCodes(shipment, Factory);
					CombineAssertions(delegate
					{
						AssertEquals("shipment.Origin.Code", "AUSYD", shipment.Origin.Code);
						AssertEquals("shipment.OrganizationCollection[0].Code", "~OO", shipment.OrganizationCollection[0].Code);
						AssertEquals("shipment.ContainerCollection[0].ContainerType.Code", "20GP", shipment.ContainerCollection[0].ContainerType.Code);
						AssertEquals("shipment.ServiceLevel.Code", "STD", shipment.ServiceLevel.Code);
						AssertEquals("shipment.Order.Warehouse.Code", "~W", shipment.Order.Warehouse.Code);
						AssertEquals("shipment.OrganizationCollection[0].Country.Code", "AU", shipment.OrganizationCollection[0].Country.Code);
						AssertEquals("shipment.FreightRateCurrency.Code", "AUD", shipment.FreightRateCurrency.Code);
						AssertEquals("shipment.OuterPacksPackageType.Code", "~CE", shipment.OuterPacksPackageType.Code);
						AssertEquals("shipment.ShipmentIncoTerm.Code", "~OB", shipment.ShipmentIncoTerm.Code);
						AssertEquals("shipment.ChargeCode.Code", "~DOF", shipment.ChargeCode.Code);
						AssertEquals("shipment.EventType.Code", "~XW", shipment.EventType.Code);
						AssertMultilineASCIIEquals("logger.Logs", @"
Information - Used code mapping defined in Organization(Code: ~OC) > Config > EDI Code Mapping.
Information - Line 22: Mapped Port code '^AUSY' to 'AUSYD'.
Information - Line 28: Mapped Package Type code '^P' to '~CE'.
Information - Line 32: Mapped Incoterm code '^F' to '~OB'.
Information - Line 37: Mapped Currency code '^AU' to 'AUD'.
Information - Line 41: Mapped Charge Code code '^DOF' to '~DOF'.
Information - Line 44: Mapped Service Level code '^ST' to 'STD'.
Information - Line 47: Mapped Event Code code '^E' to '~XW'.
Information - Line 53: Mapped Container Type code '^20G' to '20GP'.
Information - Line 61: Mapped Warehouse code '^W' to '~W'.
Information - Line 67: Mapped Organization code '^OO' to '~OO'.
Information - Line 70: Mapped Country/Region code '^A' to 'AU'.
					".Trim(), logger.Logs);
					});
				}

				logger.ClearLogs();
				var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlThatNeedsCodeMapping_UniversalShipment)))
				{
					var mapper = new CodeMappingManager(logger);
					new XmlReader(mapper).ReadXML(universalShipment, stream, logger);
					mapper.UpdateMappedCodes(universalShipment, Factory);
					CombineAssertions(delegate
					{
						AssertEquals("universalShipment.Order.DropMode.Code", "HLS", universalShipment.Order.DropMode.Code);
						AssertEquals("universalShipment.InstructionCollection[0].DropMode.Code", "HLS", universalShipment.InstructionCollection[0].DropMode.Code);
						AssertEquals("universalShipment.ContainerCollection[0].Commodity.Code", commodity.RH_Code, universalShipment.ContainerCollection[0].Commodity.Code);
						AssertEquals("universalShipment.ContainerCollection[0].RatingCommodity.Code", anotherCommodity.RH_Code, universalShipment.ContainerCollection[0].RatingCommodity.Code);
						AssertEquals("universalShipment.InstructionCollection[0].Equipment", equipment.RQ_ShortCode, universalShipment.InstructionCollection[0].Equipment);
						AssertMultilineASCIIEquals("logger.Logs",
@"Information - Used code mapping defined in Organization(Code: ~OC) > Config > EDI Code Mapping.
Information - Line 16: Mapped Commodity code '^COM' to 'MFUE'.
Information - Line 19: Mapped Commodity code '^CO2' to 'SHIP'.
Information - Line 31: Mapped Drop Mode code '^DM' to 'HLS'.
Information - Line 37: Mapped Drop Mode code '^DM' to 'HLS'.
Information - Line 39: Mapped Equipment code '^EQP' to 'TRK'.
", logger.Logs);
					});
				}
			}
		}

		public void TestCodeMappingWhenNoMappedCodeSetup()
		{
			var uberShipment = new UberShipment();
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlThatNeedsCodeMapping)))
			{
				var mapper = new CodeMappingManager(logger);
				new XmlReader(mapper).ReadXML(uberShipment, stream, logger);
				mapper.UpdateMappedCodes(uberShipment, Factory);
				CombineAssertions(delegate
				{
					AssertEquals("uberShipment.Origin.Code", "^AUSY", uberShipment.Origin.Code);
					AssertEquals("uberShipment.OrganizationCollection[0].Code", "^OO", uberShipment.OrganizationCollection[0].Code);
					AssertEquals("uberShipment.ContainerCollection[0].ContainerType.Code", "^20G", uberShipment.ContainerCollection[0].ContainerType.Code);
					AssertEquals("uberShipment.ServiceLevel.Code", "^ST", uberShipment.ServiceLevel.Code);
					AssertEquals("uberShipment.Order.Warehouse.Code", "^W", uberShipment.Order.Warehouse.Code);
					AssertEquals("uberShipment.OrganizationCollection[0].Country.Code", "^A", uberShipment.OrganizationCollection[0].Country.Code);
					AssertEquals("uberShipment.FreightRateCurrency.Code", "^AU", uberShipment.FreightRateCurrency.Code);
					AssertEquals("uberShipment.OuterPacksPackageType.Code", "^P", uberShipment.OuterPacksPackageType.Code);
					AssertEquals("uberShipment.ShipmentIncoTerm.Code", "^F", uberShipment.ShipmentIncoTerm.Code);
					AssertEquals("uberShipment.ChargeCode.Code", "^DOF", uberShipment.ChargeCode.Code);
					AssertEquals("uberShipment.EventType.Code", "^E", uberShipment.EventType.Code);
					AssertMultilineASCIIEquals("logger.Logs", "", logger.Logs);
				});
			}

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlThatNeedsCodeMapping_UniversalShipment)))
			{
				var mapper = new CodeMappingManager(logger);
				new XmlReader(mapper).ReadXML(universalShipment, stream, logger);
				mapper.UpdateMappedCodes(universalShipment, Factory);
				CombineAssertions(delegate
				{
					AssertEquals("universalShipment.Order.DropMode.Code", "^DM", universalShipment.Order.DropMode.Code);
					AssertEquals("universalShipment.InstructionCollection[0].DropMode.Code", "^DM", universalShipment.InstructionCollection[0].DropMode.Code);
					AssertEquals("universalShipment.ContainerCollection[0].Commodity.Code", "^COM", universalShipment.ContainerCollection[0].Commodity.Code);
					AssertEquals("universalShipment.ContainerCollection[0].RatingCommodity.Code", "^CO2", universalShipment.ContainerCollection[0].RatingCommodity.Code);
					AssertEquals("universalShipment.InstructionCollection[0].Equipment", "^EQP", universalShipment.InstructionCollection[0].Equipment);
					AssertMultilineASCIIEquals("logger.Logs", "", logger.Logs);
				});
			}
		}

		#region XmlThatNeedsCodeMapping

		const string XmlThatNeedsCodeMapping =
@"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<UberShipment>
		<DataContext>
			<Company>
				<Code>DAU</Code>
			</Company>
			<EnterpriseID>HYE</EnterpriseID>
			<ServerID>AYA</ServerID>
			<EventType>
				<Code>ATH</Code>
				<Description>Action Authorised</Description>
			</EventType>
		</DataContext>
		<ReferenceData>MyReference</ReferenceData>
		<CandidateKeyField>1001</CandidateKeyField>
		<ZZZMandatoryField>2</ZZZMandatoryField>
		<Destination>
			<Code>NZAKL</Code>
			<Name>Doorklund</Name>
		</Destination>
		<Origin>
			<Code>^AUSY</Code>
			<Name>Sydney</Name>
		</Origin>
		<PackageCount>12</PackageCount>
		<OuterPacks>2</OuterPacks>
		<OuterPacksPackageType>
			<Code>^P</Code>
			<Description>Piece</Description>
		</OuterPacksPackageType>
		<ShipmentIncoTerm>
			<Code>^F</Code>
			<Description>Free On Board</Description>
		</ShipmentIncoTerm>
		<FreightRate>0.0000</FreightRate>
		<FreightRateCurrency>
			<Code>^AU</Code>
			<Description>Australia, Dollars</Description>
		</FreightRateCurrency>
		<ChargeCode>
			<Code>^DOF</Code>
		</ChargeCode>
		<ServiceLevel>
			<Code>^ST</Code>
		</ServiceLevel>
		<EventType>
			<Code>^E</Code>
		</EventType>
		<ContainerCollection>
			<Container>
				<ContainerNumber>OOCL0000006</ContainerNumber>
		        <ContainerType>
					<Code>^20G</Code>
					<Description>Twenty foot flatrack</Description>
					<ISOCode>22P1</ISOCode>
				</ContainerType>
			</Container>
		</ContainerCollection>
		<Order>
			<Warehouse>
				<Code>^W</Code>
				<Name>Warehouse No. 1</Name>
			</Warehouse>
		</Order>
		<OrganizationCollection>
			<Organization>
				<Code>^OO</Code>
				<CompanyName>CargoWise</CompanyName>
				<Country>
					<Code>^A</Code>
					<Name>Australia</Name>
				</Country>
			</Organization>
		</OrganizationCollection>
	</UberShipment>
</UbiquitousShipment>";

		#endregion

		#region XmlThatNeedsCodeMapping_UniversalShipment

		const string XmlThatNeedsCodeMapping_UniversalShipment =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001003</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>
	<ContainerCollection>
		<Container>
			<ContainerNumber>OOCL0000006</ContainerNumber>
			<Commodity>
				 <Code>^COM</Code>
			</Commodity>
			<RatingCommodity>
				 <Code>^CO2</Code>
			</RatingCommodity>
			<ContainerType>
				<Code>20G</Code>
				<Description>Twenty foot flatrack</Description>
				<ISOCode>22P1</ISOCode>
			</ContainerType>
		</Container>
	</ContainerCollection>
	<Order>
        <OrderNumber>REFERENCE</OrderNumber>
		<DropMode>
			<Code>^DM</Code>
		</DropMode>
	</Order>
    <InstructionCollection>
      <Instruction>
        <DropMode>
          <Code>^DM</Code>
        </DropMode>
		<Equipment>^EQP</Equipment>
	  </Instruction>
    </InstructionCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#region XmlThatNeedsCodeMappingWithJobCosting

		const string XmlThatNeedsCodeMappingWithJobCosting =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001003</Key>
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
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Action Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDescription>dfd</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

    </DataContext>

    <JobCosting>
      <AccrualNotRecognized>0.0000</AccrualNotRecognized>
      <AccrualRecognized>-100.0000</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </Branch>
      <Currency>
        <Code>AUD</Code>
        <Description>Australia, Dollars</Description>
      </Currency>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>-100.0000</TotalAccrual>
      <TotalCost>0.0000</TotalCost>
      <TotalJobProfit>0.0000</TotalJobProfit>
      <TotalRevenue>0.0000</TotalRevenue>
      <TotalWIP>100.0000</TotalWIP>
      <WIPNotRecognized>0.0000</WIPNotRecognized>
      <WIPRecognized>100.0000</WIPRecognized>
      <ChargeLineCollection>
        <ChargeLine>
          <Branch>
            <Code>BNE</Code>
            <Name>BN - AUBNE</Name>
          </Branch>
          <ChargeCode>
            <Code>FRT</Code>
            <Description>International Freight</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>FRT</Code>
            <Description>Freight</Description>
          </ChargeCodeGroup>
          <CostGSTVATID>
            <TaxCode>FREEGST</TaxCode>
            <Description>Zero Rated</Description>
          </CostGSTVATID>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>100.0000</CostLocalAmount>
          <CostOSAmount>100.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>AUD</Code>
            <Description>Australia, Dollars</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0.0000</CostOSGSTVATAmount>
          <Creditor>
            <Type>Organization</Type>
            <Key>^CC</Key>
          </Creditor>
          <Debtor>
            <Type>Organization</Type>
            <Key>^DD</Key>
          </Debtor>
          <Department>
            <Code>FEA</Code>
            <Name>Forwarding Export Air</Name>
          </Department>
          <Description>International Freight</Description>
          <DisplaySequence>1</DisplaySequence>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>100.0000</SellLocalAmount>
          <SellOSAmount>100.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>AUD</Code>
            <Description>Australia, Dollars</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0.0000</SellOSGSTVATAmount>
        </ChargeLine>
        <ChargeLine>
          <Branch>
            <Code>BNE</Code>
            <Name>BN - AUBNE</Name>
          </Branch>
          <ChargeCode>
            <Code>FRT</Code>
            <Description>International Freight</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>FRT</Code>
            <Description>Freight</Description>
          </ChargeCodeGroup>
          <CostGSTVATID>
            <TaxCode>FREEGST</TaxCode>
            <Description>Zero Rated</Description>
          </CostGSTVATID>
          <CostIsPosted>false</CostIsPosted>
          <CostLocalAmount>100.0000</CostLocalAmount>
          <CostOSAmount>100.0000</CostOSAmount>
          <CostOSCurrency>
            <Code>AUD</Code>
            <Description>Australia, Dollars</Description>
          </CostOSCurrency>
          <CostOSGSTVATAmount>0.0000</CostOSGSTVATAmount>
          <Creditor>
            <Type>Organization</Type>
            <Key>^^CC</Key>
          </Creditor>
          <Debtor>
            <Type>Organization</Type>
          </Debtor>
          <Department>
            <Code>FEA</Code>
            <Name>Forwarding Export Air</Name>
          </Department>
          <Description>International Freight</Description>
          <DisplaySequence>1</DisplaySequence>
          <SellInvoiceType>FIN</SellInvoiceType>
          <SellIsPosted>false</SellIsPosted>
          <SellLocalAmount>100.0000</SellLocalAmount>
          <SellOSAmount>100.0000</SellOSAmount>
          <SellOSCurrency>
            <Code>AUD</Code>
            <Description>Australia, Dollars</Description>
          </SellOSCurrency>
          <SellOSGSTVATAmount>0.0000</SellOSGSTVATAmount>
        </ChargeLine>
      </ChargeLineCollection>
    </JobCosting>
  </Shipment>
</UniversalShipment>
";

		#endregion

		public void TestCodeMappingFromDataProvider()
		{
			CreateOrgPatternMatchOverride("^F", Constants.OrgPatternMatchOverrideRelationships.IncoTerm, "~OB");

			var dataProviderOrg = Factory.New<IOrgHeader>();
			dataProviderOrg.OH_Code = "~CodeMapSrc";
			CreateOrgPatternMatchOverride("FREDS FORWARDING", Constants.OrgPatternMatchOverrideRelationships.Organisation, dataProviderOrg.PK);
			CreateOrgPatternMatchOverrideInSource(dataProviderOrg.PK, "^F", Constants.OrgPatternMatchOverrideRelationships.IncoTerm, ZGuid.Empty, "~ZZ");

			Factory.Save();

			var shipment = new UberShipment();
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlThatNeedsCodeMappingFromDataProvider)))
			{
				var mapper = new CodeMappingManager(logger);
				new XmlReader(mapper).ReadXML(shipment, stream, logger);
				mapper.UpdateMappedCodes(shipment, Factory);
				CombineAssertions(delegate
				{
					AssertEquals("shipment.ShipmentIncoTerm.Code", "~ZZ", shipment.ShipmentIncoTerm.Code);
				});
			}
		}

		#region XmlThatNeedsCodeMappingFromDataProvider

		const string XmlThatNeedsCodeMappingFromDataProvider = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment>
    <ReferenceData>MyReference</ReferenceData>
    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination>
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </Origin>
    <PackageCount>12</PackageCount>
	<ShipmentIncoTerm>
		<Code>^F</Code>
		<Description>Free On Board</Description>
	</ShipmentIncoTerm>
	<OrganizationCollection>
      <Organization>
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
    <DataContext>
      <Company>
        <Code>DAU</Code>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <ServerID>AYA</ServerID>
      <DataProvider>FREDS FORWARDING</DataProvider>
    </DataContext>
	</UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestDataDoesntImportAndWarningGeneratedIfAnEnumValueIsInvalid()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithInvalidTransportMode)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"Warning - Line 14: <UberShipment>.<TransportMode> - Invalid value [Donkey], valid values are [Sea, Air, Road, Rail, Storage].", logger.Logs);
					AssertEquals("shipment.TransportMode", null, shipment.TransportMode);
				});
			}
		}

		#region XmlWithInvalidTransportMode

		const string XmlWithInvalidTransportMode = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>
    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Sy&quot;d-er&nee</Name>
    </Origin>
    <TransportMode>Donkey</TransportMode>
    <PackageCount>12</PackageCount>
		<OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
	</UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestDataIsImportedAndNoWarningIsGeneratedIfAnEnumValueIsUpperCase()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithValidTransportMode)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertEquals("logger.Logs", string.Empty, logger.Logs);
					AssertEquals("shipment.TransportMode", UberTransportMode.Sea, shipment.TransportMode);
				});
			}
		}

		#region XmlWithInvalidTransportMode

		const string XmlWithValidTransportMode = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>
    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Sy&quot;d-er&nee</Name>
    </Origin>
    <TransportMode>SEA</TransportMode>
    <PackageCount>12</PackageCount>
		<OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
	</UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLIgnoresUnrecognizedAttributesOrEmptyDescriptions()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithUnknownAttributeAndEmptyDescription)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"", logger.Logs);
					AssertEquals("shipment.PackageCount", 12, shipment.PackageCount);
					AssertEquals("shipment.UberMediumField.Code", "I am empty", shipment.UberMediumField.Code);
					AssertEquals("shipment.UberMediumField.Description", null, shipment.UberMediumField.Description);
				});
			}
		}

		#region XmlWithUnknownAttributeAndEmptyDescription

		const string XmlWithUnknownAttributeAndEmptyDescription = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Sy&quot;d-er&nee</Name>
    </Origin>
    <PackageCount SomeAttribute=""HAHAHA"">12</PackageCount>
    <UberMediumField>I am empty</UberMediumField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLReadsInDescriptionAttribute()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithDescriptionAttribute)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", @"Warning - Line 16: <UberShipment>.<UberMediumField> - Attribute [Description] exceeded its maximum length of 50 characters. 62 characters were found.", logger.Logs);
					AssertEquals("shipment.UberMediumField", "I have been described well", shipment.UberMediumField.Code);
					AssertEquals("shipment.UberMediumField_Description", @"& I am a <description> that is ""too"" long for this", shipment.UberMediumField.Description);
					AssertEquals("shipment.Origin.Name", "Sy&quot;d-er&nee", shipment.Origin.Name);
				});
			}
		}

		#region XmlWithDescriptionAttribute

		const string XmlWithDescriptionAttribute = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Sy&quot;d-er&nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <UberMediumField Description=""&amp; I am a &lt;description&gt; that is &quot;too&quot; long for this description"">I have been described well</UberMediumField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLFailsOnSelfEndingComplexElements()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithSelfEndingComplexElement)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Warning - Line 16: Element <UberShipment>.<PackageUnit> opened at line 16 was excluded as it was missing mandatory elements. Missing: Code.", logger.Logs);
			}
		}

		#region XmlWithSelfEndingComplexElement

		const string XmlWithSelfEndingComplexElement = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit />

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLHandlesSelfEndingUnknownSimpleElements()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithUnknownSelfEndingElement)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Warning - Line 9: <UberShipment>.<Destination> - Unrecognized element <Dummy> found. Element skipped.", logger.Logs);
			}
		}

		#region XmlWithUnknownSelfEndingElement

		const string XmlWithUnknownSelfEndingElement = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Dummy/>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLHandlesSelfEndingSimpleElements()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithSelfEndingElement)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"", logger.Logs);
				AssertEquals("shipment.CandidateKeyField", 0, shipment.CandidateKeyField);
			}
		}

		#region XmlWithSelfEndingElement

		const string XmlWithSelfEndingElement = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField/>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestSequenceElementsCanReadXMLWithSpaceInClosingTag()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInSequenceElementClosingTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", "", logger.Logs);
					AssertNotNull(shipment.OrganizationCollection);
					AssertEquals("One organization should be created", 1, shipment.OrganizationCollection.Count);
				});
			}
		}

		#region XmlWithErrorInSequenceElementClosingTag

		const string XmlWithErrorInSequenceElementClosingTag = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection  >
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLHandlesSelfEndingCollectionElement()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithSelfEndingCollectionElement)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"", logger.Logs);
			}
		}

		#region XmlWithSelfEndingCollectionElement

		const string XmlWithSelfEndingCollectionElement = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>
    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>

    <ContainerCollection/>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestUnknownElementsCanReadXMLWithSpaceInClosingTag()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInUnknownElementClosingTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", "Warning - Line 9: <UberShipment>.<Destination> - Unrecognized element <Dummy> found. Element skipped.", logger.Logs);
					AssertNotNull(shipment.Destination);
					AssertNotNull("NZAKL", shipment.Destination.Code);
					AssertNotNull("Doorklund", shipment.Destination.Name);
				});
			}
		}

		#region XmlWithErrorInUnknownElementClosingTag

		const string XmlWithErrorInUnknownElementClosingTag = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Dummy>random</Dummy  >
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLShowsErrorWhenNoClosingTagFoundForUnknownElements()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithNoClosingTagForUnknownElement)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Error - Line 11: Reached end of file without finding closing tag for element <UberShipment>.<Destination>.<Dummy> opened on line 8.", logger.Logs);
			}
		}

		#region XmlWithNoClosingTagForUnknownElement

		const string XmlWithNoClosingTagForUnknownElement = @"<UbiquitousShipment Version=""1.0"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination>
      <Dummy>
        <Something>

";

		#endregion

		public void TestReadXMLShowsErrorWhenCannotMatchOpeningTagForUnknownElements()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInUnknownElementOpeningTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Error - Line 9: <UberShipment>.<Destination> - Invalid opening tag <  Dummy> found.", logger.Logs);
			}
		}

		#region XmlWithErrorInUnknownElementOpeningTag

		const string XmlWithErrorInUnknownElementOpeningTag = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <  Dummy>random</Dummy>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLShowsErrorWhenCannotMatchOpeningTagForComplexElements()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInComplexElementOpeningTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Error - Line 7: <UberShipment> - Invalid opening tag <  Destination Action=""MERGE""> found.", logger.Logs);
			}
		}

		#region XmlWithErrorInComplexElementOpeningTag

		const string XmlWithErrorInComplexElementOpeningTag = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <  Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestComplexElementsCanReadXMLWithSpaceInClosingTag()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInComplexElementClosingTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("logger.Logs", "", logger.Logs);
					var destination = shipment.Destination;
					AssertNotNull(destination);
					AssertEquals("NZAKL", destination.Code);
					AssertEquals("Doorklund", destination.Name);
				});
			}
		}

		#region XmlWithErrorInComplexElementClosingTag

		const string XmlWithErrorInComplexElementClosingTag = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination  >
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLShowsErrorWhenErrorInElementOpeningTag()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInElementOpeningTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Error - Line 3: <UberShipment> - Invalid opening tag <  ReferenceData> found.", logger.Logs);
			}
		}

		#region XmlWithErrorInElementOpeningTag

		const string XmlWithErrorInElementOpeningTag = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <  ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLShowsContentInElementOpeningTagIfCouldNotMatchRatherThanEmpty()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorHavingOpeningTagInTheMiddleOfAnElement)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Error - Line 3: <UberShipment>.<ReferenceData> - Unexpected opening tag <ReferenceData  > found. Expected closing tag </ReferenceData> for opening tag on line 3.", logger.Logs);
			}
		}

		#region XmlWithErrorHavingOpeningTagInTheMiddleOfAnElement

		const string XmlWithErrorHavingOpeningTagInTheMiddleOfAnElement = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference<ReferenceData  >

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLShowsContentInElementClosingTagIfCouldNotMatchRatherThanEmpty()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInElementClosingTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Error - Line 3: <UberShipment>.<ReferenceData> - Unexpected closing tag </Japan> found. Expected closing tag </ReferenceData> for opening tag on line 3.", logger.Logs);
			}
		}

		#region XmlWithErrorInElementClosingTag

		const string XmlWithErrorInElementClosingTag = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</Japan>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLShowsErrorWhenCannotMatchOpeningTagOnTopLevelElement()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInTopLevelElementOpeningTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Error - Line 2: Root element <UbiquitousShipment> must contain one <UberShipment> element. < UberShipment Action=""MERGE""> is not valid in this scope.", logger.Logs);
			}
		}

		#region XmlWithErrorInTopLevelElementOpeningTag

		const string XmlWithErrorInTopLevelElementOpeningTag = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  < UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLWithErrorShowsTheContentInRootElementTagIfCouldNotMatchRatherThanEmpty()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithErrorInRootElementTag)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"Error - Line 1: Invalid Root element found. Expected <UbiquitousShipment> but found <UbiquitousShipment       xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0""> instead.", logger.Logs);
			}
		}

		#region XmlWithErrorInRootElementTag

		const string XmlWithErrorInRootElementTag = @"<UbiquitousShipment       xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLWithVersionAndNamespaceSwapped()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithVersionAndNamespaceSwapped)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("Should be no logs of warnings or errors", "", logger.Logs);
			}
		}

		#region XmlWithVersionAndNamespaceSwapped

		const string XmlWithVersionAndNamespaceSwapped = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestCheckXMLWithMultipleNamespaces()
		{
			AssertNamespacesCorrectlyParsed(@"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11""");
			AssertNamespacesCorrectlyParsed(@"xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""");
			AssertNamespacesCorrectlyParsed(@"xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""");
		}

		public void AssertNamespacesCorrectlyParsed(string newNamespaces)
		{
			var declaration = new Event();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ChangeXMLWithMultipleNamespaces(newNamespaces))))
			{
				var reader = ObjectFactory.Get<IXmlReader>();
				reader.ReadXML(declaration, stream, logger);
				Assert("Should accept multiple namespaces without error", string.IsNullOrEmpty(logger.Logs));
				Assert(reader.Namespace.Equals("http://www.cargowise.com/Schemas/Universal/2011/11"));
			}
		}

		#region XMLWithMultipleNamespaces

		string ChangeXMLWithMultipleNamespaces(string newNamespace)
		{
			return string.Format(XMLWithMultipleNamespaces, newNamespace);
		}

		const string XMLWithMultipleNamespaces = @"<UniversalEvent {0}>
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomsDeclaration</Type>
          <Key>B00169780</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2017-06-07T13:22:02</EventTime>
    <EventType>CES</EventType>
    <EventReference>ACK</EventReference>
  </Event>
</UniversalEvent>";

		#endregion

		public void TestReadXMLWithPreceedingAndTrailingBlankLines()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes("\r\n    \r\n		\r\n" + XmlWithoutEncoding + "\r\n    \r\n			")))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Should be no logs of warnings or errors", "", logger.Logs);
				AssertEquals("shipment.CandidateKeyField", 1001, shipment.CandidateKeyField);
				AssertEquals("shipment.ZZZMandatoryField", 2m, shipment.ZZZMandatoryField);
				AssertEquals("shipment.PackageCount", 12, shipment.PackageCount);
			});
		}

		public void TestReadXMLWithoutEncoding()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(XmlWithoutEncoding)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Should be no logs of warnings or errors", "", logger.Logs);
				AssertEquals("shipment.CandidateKeyField", 1001, shipment.CandidateKeyField);
				AssertEquals("shipment.ZZZMandatoryField", 2m, shipment.ZZZMandatoryField);
				AssertEquals("shipment.PackageCount", 12, shipment.PackageCount);
			});
		}

		#region XmlWithoutEncoding

		const string XmlWithoutEncoding = @"<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin Action=""MERGE"">
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXML()
		{
			var shipment = new UberShipment();
			var inputXml = string.Format(UberShipmentInputXML, @"Some really big block of text.
2 &gt; 1 &amp; 3 &lt; 4
I mean it should have at least 3 or 4 lines.
Enough to know better.");

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(inputXml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

				using (var reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("Serialized UberShipment is the same after having deserialized it", inputXml.Trim(), result);
				}
			}
		}

		#region UberShipmentInputXML

		const string UberShipmentInputXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <UberShipment>
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination>
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>
    <Origin>
      <Code>AUSYD</Code>
      <Name>Syd-er-nee</Name>
    </Origin>
    <PackageCount>12</PackageCount>
    <PackageUnit>
      <Code>PK</Code>
    </PackageUnit>
    <UberBigField>{0}</UberBigField>
    <UberBoolean>true</UberBoolean>
    <UberByteField>12</UberByteField>
    <UberDate>2017-11-27</UberDate>
    <UberDateTime>2010-02-12T00:00:00</UberDateTime>
    <UberDecimal>12.23</UberDecimal>
    <UberDuration>PT5H17M34S</UberDuration>
    <UberInteger>44</UberInteger>
    <UberLongInt>99</UberLongInt>
    <UberMediumField>A bit longer, but not too long.</UberMediumField>
    <UberShortIntField>99</UberShortIntField>
    <UberSmallField>SHORTFIELD</UberSmallField>
    <Volume>33</Volume>
    <VolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </VolumeUnit>
    <Weight>44</Weight>
    <WeightUnit Description=""Kilograms"">KG</WeightUnit>

    <InnerRelation>
      <ExtraBoolean>false</ExtraBoolean>
      <ExtraDecimal>1.224</ExtraDecimal>
      <ExtraField>hOOters</ExtraField>
    </InnerRelation>

    <OrganizationCollection>
      <Organization>
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>

    <DateCollection>
      <Date>
        <Type>Pickup</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2010-02-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <Value>2010-02-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>Pack</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2010-02-06T00:00:00</Value>
      </Date>
    </DateCollection>

    <HeaderCollection>
      <Header>
        <HeaderReference>Boganville</HeaderReference>
        <Origin>
          <Code>AUSYD</Code>
          <Name>Syd-er-nee</Name>
        </Origin>

        <LineCollection>
          <Line>
            <LineNumber>123</LineNumber>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
          </Line>
          <Line>
            <LineNumber>456</LineNumber>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>
          </Line>
        </LineCollection>
      </Header>
      <Header>
        <HeaderReference>Coolville</HeaderReference>
        <Importer>
          <CompanyName>Donald Trump</CompanyName>
          <Code>AAA</Code>
        </Importer>
        <InvoicedValue>1.3</InvoicedValue>
      </Header>
    </HeaderCollection>

    <ShipmentCollection>
      <Shipment>

        <ZZZMandatoryField>3</ZZZMandatoryField>

        <OrganizationCollection>
          <Organization>
            <CompanyName>Funky Town</CompanyName>
            <Code>CCC</Code>
          </Organization>
        </OrganizationCollection>
      </Shipment>
    </ShipmentCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadXMLWithoutLineBreaks()
		{
			var shipment = new UberShipment();
			var inputXml = string.Format(UberShipmentInputXML, @"Some really big block of text.
2 &gt; 1 &amp; 3 &lt; 4
I mean it should have at least 3 or 4 lines.
Enough to know better.").Replace("\r\n", "");

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(inputXml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			using (var stream = (SubStreamableStream)new MemoryStream())
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

				using (var reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("Serialized UberShipment is the same after having deserialized it", string.Format(UberShipmentInputXML.Trim(), @"Some really big block of text.2 &gt; 1 &amp; 3 &lt; 4I mean it should have at least 3 or 4 lines.Enough to know better."), result);
				}
			}
		}

		public void TestThrowsErrorWhenRootElementIsNotTheSameAsTheTypeOfTheObjectSentIn()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(TypeElementWithDifferentRootXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 2: Invalid Root element found. Expected <UbiquitousShipment> but found <UnidentifiedShipment> instead.", logger.Logs);
			}
		}

		#region TypeElementWithDifferentRootXML

		const string TypeElementWithDifferentRootXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UnidentifiedShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">




  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>zzzz</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UnidentifiedShipment>";

		#endregion

		public void TestThrowsErrorWhenTypeElementIsNotTheSameAsTheTypeOfTheObjectSentIn()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(TypeElementWithDifferentNameXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 7: Root element <UbiquitousShipment> must contain one <UberShipment> element. <UberUberShipment> is not valid in this scope.", logger.Logs);
			}
		}

		#region TypeElementWithDifferentNameXML

		const string TypeElementWithDifferentNameXML = @"<?xml version=""1.0"" encoding=""utf-8""?>


<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">


  <UberUberShipment Action=""MERGE"">


    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>zzzz</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestThrowsErrorWhenRootElementDoesNotHaveAClosingTag()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(NoRootElementClosingTagXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Line 12: <UberShipment>.<CandidateKeyField> - Invalid value [zzzz]. Value must be a valid Integer.
Error - Line 20: Reached end of file without finding closing tag for element <UbiquitousShipment> opened on line 2.
				".Trim(), logger.Logs);
			}
		}

		#region NoRootElementClosingTagXML

		const string NoRootElementClosingTagXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">





  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>zzzz</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</LaLaShipment>";

		#endregion

		public void TestThrowsErrorWhenSimpleElementWithNoClosingTagFound()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(SimpleElementWithNoClosingTagXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 8: Reached end of file without finding closing tag for element <UberShipment>.<ZZZMandatoryField> opened on line 7.", logger.Logs);
			}
		}

		#region SimpleElementWithNoClosingTagXML

		const string SimpleElementWithNoClosingTagXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>


    <ZZZMandatoryField>2
";

		#endregion

		public void TestThrowsErrorWhenComplexElementWithNoClosingTagFound()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ComplexElementWithNoClosingTagXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 7: Reached end of file without finding closing tag for element <UberShipment>.<Destination> opened on line 6.", logger.Logs);
			}
		}

		#region ComplexElementWithNoClosingTagXML

		const string ComplexElementWithNoClosingTagXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment Version=""1.0"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination>
";

		#endregion

		public void TestThrowsErrorWhenSequenceElementWithNoClosingTagFound()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(SequenceElementWithNoClosingTagXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 8: Reached end of file without finding closing tag for element <UberShipment>.<OrganizationCollection> opened on line 7.", logger.Logs);
			}
		}

		#region SequenceElementWithNoClosingTagXML

		const string SequenceElementWithNoClosingTagXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment Version=""1.0"" xmlns=""http://www.cargowise.com/Schemas/Native"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>
    <ZZZMandatoryField>2</ZZZMandatoryField>

    <OrganizationCollection>
";

		#endregion

		public void TestThrowsErrorWhenElementWithUnexpectedClosingTagFound()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(UnexpectedClosingTagOnElementXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 11: <UberShipment>.<ZZZMandatoryField> - Unexpected opening tag <CandidateKeyField> found. Expected closing tag </ZZZMandatoryField> for opening tag on line 7.", logger.Logs);
			}
		}

		#region UnexpectedClosingTagOnElementXML

		const string UnexpectedClosingTagOnElementXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>


    <ZZZMandatoryField>2



    <CandidateKeyField>zzzz</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestThrowsErrorWhenCollectionWithUnexpectedClosingTagFound()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(UnexpectedClosingTagOnCollectionXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 15: <UberShipment>.<OrganizationCollection> - Unexpected closing tag </UberOrganizationCollection> found. Expected closing tag </OrganizationCollection> for opening tag on line 10.", logger.Logs);
			}
		}

		#region UnexpectedClosingTagOnCollectionXML

		const string UnexpectedClosingTagOnCollectionXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>


    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>

    </UberOrganizationCollection>


  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksForClosingTagOnMultiLineFields()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MultiLineFieldWithWrongClosingTagXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 15: <UberShipment>.<UberBigField> - Unexpected closing tag </BigField> found. Expected closing tag </UberBigField> for opening tag on line 10.", logger.Logs);
			}
		}

		#region MultiLineFieldWithWrongClosingTagXML

		const string MultiLineFieldWithWrongClosingTagXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>


    <UberBigField>dgfsdfsd
sdfsdfd
sdfsdf
dfgsdgf
sdfgsdfg
adfg</BigField>sdfg
sdfgdsg

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestThrowsErrorWhenUnexpectedOpeningTagFoundInTheMiddleOfALine()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MultiLineFieldWithOpeningTagInMiddleXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 14: <UberShipment>.<UberBigField> - Unexpected opening tag <BigField> found. Expected closing tag </UberBigField> for opening tag on line 9.", logger.Logs);
			}
		}

		#region MultiLineFieldWithOpeningTagInMiddleXML

		const string MultiLineFieldWithOpeningTagInMiddleXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>

    <UberBigField>dgfsdfsd
sdfsdfd
sdfsdf
dfgsdgf
sdfgsdfg
adfg<BigField>sdfg
sdfgdsg

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestFieldsNotPartOfDataObjectAreJustDiscarded()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ExtraFieldXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Line 10: <UberShipment> - Unrecognized element <Extra> found. Element skipped.
Warning - Line 15: <UberShipment>.<Destination> - Unrecognized element <Dummy> found. Element skipped.
Warning - Line 21: <UberShipment> - Unrecognized element <Dummy> found. Element and all its children skipped.
Warning - Line 49: <UberShipment>.<DateCollection>.<Date> - Unrecognized element <Bruce> found. Element skipped.
Warning - Line 56: <UberShipment> - Unrecognized element <DummyCollection> found. Element and all its children skipped.

				".Trim(), logger.Logs);
			}

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(shipment, stream);

				using (var reader = new StreamReader(stream))
				{
					string result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("Serialized UberShipment removes Extra Fields", ExpectedExtraFieldXML.Trim(), result);
				}
			}
		}

		#region ExtraFieldXML

		const string ExtraFieldXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>


    <Extra>SpicySausage</Extra>

    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>

      <Dummy>lalala</Dummy>


      <Name>Doorklund</Name>
    </Destination>

    <Dummy Action=""MERGE"">


      <RandomField>random</RandomField>
      <Dummy Action=""MERGE"">
        <RandomField>random</RandomField>
        <Code>BBB</Code>
      </Dummy>
      <Code>BBB</Code>


    </Dummy>



    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>

    <DateCollection>
      <Date Action=""MERGE"">
        <Type>Pickup</Type>
        <IsEstimate>false</IsEstimate>


        <Bruce>Hello</Bruce>

        <Value>2010-02-01T00:00:00</Value>
      </Date>
    </DateCollection>


    <DummyCollection>

      <Dummy>
        <UberDummyField>I am Uber</UberDummyField>
      </Dummy>


    </DummyCollection>


  </UberShipment>
</UbiquitousShipment>";

		#endregion

		#region ExpectedExtraFieldXML

		const string ExpectedExtraFieldXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <UberShipment>
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>
    <Destination>
      <Code>NZAKL</Code>
      <Name>Doorklund</Name>
    </Destination>

    <OrganizationCollection>
      <Organization>
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>

    <DateCollection>
      <Date>
        <Type>Pickup</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2010-02-01T00:00:00</Value>
      </Date>
    </DateCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestWrongClosingTag()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(WrongWrongWrongClosingTagXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Line 11: <UberShipment>.<Destination> - Unrecognized element <Dummy> found. Element skipped.
Error - Line 15: <UberShipment>.<Destination> - Unexpected closing tag </WrongWrongWrongDestination> found. Expected closing tag </Destination> for opening tag on line 9.
				".Trim(), logger.Logs);
			}
		}

		#region WrongWrongWrongClosingTagXML

		const string WrongWrongWrongClosingTagXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>

    <Destination Action=""MERGE"">
      <Code>NZAKL</Code>
      <Dummy>lalala</Dummy>
      <Name>Doorklund</Name>


    </WrongWrongWrongDestination>

    <Dummy Action=""MERGE"">
      <RandomField>random</RandomField>
      <Code>BBB</Code>
    </Dummy>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>

    <DateCollection>
      <Date Action=""MERGE"">
        <Type>Pickup</Type>
        <IsEstimate>false</IsEstimate>
        <Bruce>Hello</Bruce>
        <Value>2010-02-01T00:00:00</Value>
      </Date>
    </DateCollection>

    <DummyCollection>
      <Dummy>
        <UberDummyField>I am Uber</UberDummyField>
      </Dummy>
    </DummyCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestEmptyTagsAreReadInAsEmptyValues()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(EmptyTagsXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Warning - Line 10: <UberShipment>.<UberBoolean> - Invalid value [vasya]. Value must be a valid Boolean (true or false).", logger.Logs);
			}

			AssertEquals("shipment.UberBigField", ZString.Empty, shipment.UberBigField);
			AssertEquals("shipment.UberBoolean", null, shipment.UberBoolean);
			AssertEquals("shipment.UberByteField", ZByte.Zero, shipment.UberByteField);
			AssertEquals("shipment.UberDate", ZDate.Empty, shipment.UberDate);
			AssertEquals("shipment.UberDateTime", ZDateTime.Empty, shipment.UberDateTime);
			AssertEquals("shipment.UberDecimal", ZDecimal.Zero, shipment.UberDecimal);
			AssertEquals("shipment.UberInteger", ZInt.Zero, shipment.UberInteger);
			AssertEquals("shipment.UberLongInt", ZLong.Zero, shipment.UberLongInt);
			AssertEquals("shipment.UberMediumField", ZString.Empty, shipment.UberMediumField.Code);
			AssertEquals("shipment.UberShortIntField", ZShort.Zero, shipment.UberShortIntField);
			AssertEquals("shipment.UberSmallField", ZString.Empty, shipment.UberSmallField);
			AssertEquals("shipment.UberDuration", TimeSpan.Zero, shipment.UberDuration);
		}

		#region EmptyTagsXML

		const string EmptyTagsXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>
    <CandidateKeyField>1001</CandidateKeyField>
    <ZZZMandatoryField>2</ZZZMandatoryField>

    <UberBigField></UberBigField>

    <UberBoolean>vasya</UberBoolean>


    <UberByteField></UberByteField>
    <UberDate></UberDate>
		<UberDateTime></UberDateTime>
    <UberDecimal></UberDecimal>
    <UberDuration></UberDuration>
    <UberInteger></UberInteger>
    <UberLongInt></UberLongInt>
    <UberMediumField></UberMediumField>
    <UberShortIntField></UberShortIntField>
    <UberSmallField></UberSmallField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
        <Code>CCC</Code>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksMandatoryFields()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MandatoryFieldsXML1)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Error - Line 17: Top Level Element <UberShipment> opened at line 5 cannot be imported as it is missing mandatory elements. Missing: ZZZMandatoryField.", logger.Logs);
			}
		}

		#region MandatoryFieldsXML1

		const string MandatoryFieldsXML1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">


  <UberShipment Action=""MERGE"">

    <ReferenceData>MyReference</ReferenceData>

    <CandidateKeyField>1001</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>

  </UberShipment>


</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksMandatoryFieldsInRelatedObjects()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MandatoryFieldsXML2)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Line 14: Element <UberShipment>.<OrganizationCollection>.<Organization> opened at line 12 was excluded as it was missing mandatory elements. Missing: CompanyName.
Warning - Line 18: Mandatory Collection <UberShipment>.<OrganizationCollection> opened at line 9 was excluded as it does not contain at least one valid element.
Error - Line 21: Top Level Element <UberShipment> opened at line 3 cannot be imported as it is missing mandatory elements. Missing: OrganizationCollection.
				".Trim(), logger.Logs);
				AssertEquals("shipment.OrganizationCollection", null, shipment.OrganizationCollection);
			}
		}

		#region MandatoryFieldsXML2

		const string MandatoryFieldsXML2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>

    <OrganizationCollection>


      <Organization Action=""MERGE"">

      </Organization>



    </OrganizationCollection>


  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksMandatoryCollectionsHaveAtLeastOneElement()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MandatoryFieldsXML3)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Line 24: Mandatory Collection <UberShipment>.<ShipmentCollection>.<Shipment>.<OrganizationCollection> opened at line 23 was excluded as it does not contain at least one valid element.
Warning - Line 27: Element <UberShipment>.<ShipmentCollection>.<Shipment> opened at line 17 was excluded as it was missing mandatory elements. Missing: OrganizationCollection.
				".Trim(), logger.Logs);
				AssertEquals("shipment.ShipmentCollection.Count", 0, shipment.ShipmentCollection.Count);
			}
		}

		#region MandatoryFieldsXML3

		const string MandatoryFieldsXML3 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>

    <ShipmentCollection>

      <Shipment Action=""MERGE"">



        <ZZZMandatoryField>3</ZZZMandatoryField>

        <OrganizationCollection>
        </OrganizationCollection>


      </Shipment>

    </ShipmentCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLAllowsEmptyMandatoryFieldsAsLongAsTheTagForTheFieldIsPresent()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MandatoryFieldsXML4)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", string.Empty, logger.Logs);
			}
		}

		#region MandatoryFieldsXML4

		const string MandatoryFieldsXML4 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>


    <ZZZMandatoryField></ZZZMandatoryField>

    <CandidateKeyField>1001</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksMandatoryCollectionsArePresent()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MandatoryFieldsXML5)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Warning - Line 24: Element <UberShipment>.<ShipmentCollection>.<Shipment> opened at line 17 was excluded as it was missing mandatory elements. Missing: OrganizationCollection.", logger.Logs);
				AssertEquals("shipment.ShipmentCollection.Count", 0, shipment.ShipmentCollection.Count);
			}
		}

		#region MandatoryFieldsXML5

		const string MandatoryFieldsXML5 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>

    <ShipmentCollection>

      <Shipment Action=""MERGE"">



        <ZZZMandatoryField>3</ZZZMandatoryField>


      </Shipment>

    </ShipmentCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksMandatoryFieldsOnObjectElementsArePresent()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MandatoryFieldsXML6)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Warning - Line 16: Element <UberShipment>.<Origin> opened at line 10 was excluded as it was missing mandatory elements. Missing: Code.", logger.Logs);
				AssertEquals("shipment.Origin", null, shipment.Origin);
			}
		}

		#region MandatoryFieldsXML6

		const string MandatoryFieldsXML6 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>


    <Origin Action=""MERGE"">

      <Name>Syd-er-nee</Name>



    </Origin>



    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>

    <HeaderCollection>
      <Header Action=""MERGE"">
        <HeaderReference>blah</HeaderReference>
      </Header>
    </HeaderCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksForInvalidValues()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(InvalidValueXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Warning - Line 9: <UberShipment>.<CandidateKeyField> - Invalid value [zzzz]. Value must be a valid Integer.", logger.Logs);
			}
		}

		#region InvalidValueXML

		const string InvalidValueXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>


    <CandidateKeyField>zzzz</CandidateKeyField>



    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksForInvalidCollectionObjects()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(InvalidCollectionObjectXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals("logger.Logs", @"Warning - Line 14: Element <UberShipment>.<OrganizationCollection> should only contain <Organization> elements. Unrecognized element <UberOrganization> found. Element and all its children skipped.", logger.Logs);
			}
		}

		#region InvalidCollectionObjectXML

		const string InvalidCollectionObjectXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>

      <UberOrganization>


        <CompanyName>UberCargoWise</CompanyName>
        <UberOrganization>
        </UberOrganization>
        <UberOrganization></UberOrganization>


      </UberOrganization>

    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLChecksMaxLengthOfFieldsAreNotExceeded()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ExceedingMaxLengthXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Line 5: <UberShipment>.<ReferenceData> exceeded its maximum length of 25 characters. 48 characters were found.
Warning - Line 8: <UberShipment>.<InnerRelation>.<ExtraField> exceeded its maximum length of 50 characters. 169 characters were found.
Warning - Line 16: <UberShipment>.<OrganizationCollection>.<Organization>.<CompanyName> exceeded its maximum length of 50 characters. 145 characters were found.
".Trim(), logger.Logs);
			}
		}

		#region ExceedingMaxLengthXML

		const string ExceedingMaxLengthXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberShipment Action=""MERGE"">

    <ReferenceData>MyReferenceIsWayTooBigForTheMaxLengthOfThisField</ReferenceData>

    <InnerRelation>
      <ExtraField>Sing me a song, you're the piano man, sing me a song tonight, cause we're all in the mood for a melody, and you've got me feeling allright... Oh, la, dadadah, dededadah.</ExtraField>
    </InnerRelation>

    <ZZZMandatoryField>2</ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>

    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise works with a number of innovative partners to deliver an integrated solution that is without equal in the global logistics marketplace.</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>";

		#endregion

		public void TestReadingInXMLCheckShipmentServiceCodeCanBeDuplicate()
		{
			var universalShipment = new Shipment();

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(UniversalShipmentXmlWithDupServiceCode)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalShipment, stream, logger);
			}

			CombineAssertions(() =>
			{
				AssertEquals("Services with duplicate service codes should not raise warning.", string.Empty, logger.Logs);
				AssertEquals("Services with duplicate service codes should be all converted.", 3, universalShipment.LocalProcessing.AdditionalServiceCollection.Count);
			});
		}

		#region UniversalShipmentXmlWithDupServiceCode
		const string UniversalShipmentXmlWithDupServiceCode = @"
<UniversalShipment>
	<Shipment>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>ForwardingShipment</Type>
					<Key/>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>DUS</Code>
				<Country>
					<Code>US</Code>
					<Name>United States</Name>
				</Country>
				<Name>Your United States Corp</Name>
			</Company>
			<DataProvider>WUTB0CDUS</DataProvider>
			<EnterpriseID>WUT</EnterpriseID>
			<EventBranch>
				<Code>LAX</Code>
				<Name>US - Los Angeles Branch</Name>
			</EventBranch>
			<EventDepartment>
				<Code>BRN</Code>
				<Name>Branch</Name>
			</EventDepartment>
			<EventType>
				<Code/>
			</EventType>
			<EventUser>
				<Code>E</Code>
				<Name>CargoWise Support</Name>
			</EventUser>
			<ServerID>B0C</ServerID>
			<TriggerCount>1</TriggerCount>
			<TriggerDate>2024-02-06T20:09:47.87</TriggerDate>
			<TriggerDescription/>
			<TriggerType>Manual</TriggerType>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>CNR</Code>
					<Description>Consignor</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<ActualChargeable>4.000</ActualChargeable>
		<AdditionalTerms>Test 1</AdditionalTerms>
		<BookingConfirmationReference/>
		<CartageWaybillNumber/>
		<CFSReference/>
		<CommunityTransitStatus>
			<Code/>
		</CommunityTransitStatus>
		<CompanyTariffLevelOverride>0</CompanyTariffLevelOverride>
		<ContainerCount>0</ContainerCount>
		<ContainerMode>
			<Code>FCL</Code>
			<Description>Full Container Load</Description>
		</ContainerMode>
		<DocumentedChargeable>4.000</DocumentedChargeable>
		<DocumentedVolume>4.000</DocumentedVolume>
		<DocumentedWeight>400.000</DocumentedWeight>
		<FMCTariffID/>
		<FreightRate>0.0000</FreightRate>
		<FreightRateCurrency>
			<Code/>
		</FreightRateCurrency>
		<GoodsDescription/>
		<GoodsValue>0.0000</GoodsValue>
		<GoodsValueCurrency>
			<Code>USD</Code>
			<Description>United States Dollar</Description>
		</GoodsValueCurrency>
		<HBLAWBChargesDisplay>
			<Code>SHW</Code>
			<Description>Show Collect Charges</Description>
		</HBLAWBChargesDisplay>
		<HBLContainerPackModeOverride/>
		<HouseBillOfLadingType>
			<Code>IAU</Code>
			<Description>IT Club Australia</Description>
		</HouseBillOfLadingType>
		<InsuranceValue>0.0000</InsuranceValue>
		<InsuranceValueCurrency>
			<Code>USD</Code>
			<Description>United States Dollar</Description>
		</InsuranceValueCurrency>
		<InterimReceiptNumber/>
		<IsBooking>true</IsBooking>
		<IsCancelled>false</IsCancelled>
		<IsCFSRegistered>true</IsCFSRegistered>
		<IsDirectBooking>false</IsDirectBooking>
		<IsForwardRegistered>true</IsForwardRegistered>
		<IsHighRisk>false</IsHighRisk>
		<IsNeutralMaster>false</IsNeutralMaster>
		<IsShipping>false</IsShipping>
		<IsSplitShipment>false</IsSplitShipment>
		<ManifestedChargeable>4.000</ManifestedChargeable>
		<ManifestedVolume>4.000</ManifestedVolume>
		<ManifestedWeight>400.000</ManifestedWeight>
		<NoCopyBills>1</NoCopyBills>
		<NoOriginalBills>0</NoOriginalBills>
		<OuterPacks>4</OuterPacks>
		<OuterPacksPackageType>
			<Code>PLT</Code>
			<Description>Pallet</Description>
		</OuterPacksPackageType>
		<PackingOrder>0</PackingOrder>
		<PortOfDestination>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfDestination>
		<PortOfDischarge>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfDischarge>
		<PortOfLoading>
			<Code>USSEA</Code>
			<Name>Seattle</Name>
		</PortOfLoading>
		<PortOfOrigin>
			<Code>USSEA</Code>
			<Name>Seattle</Name>
		</PortOfOrigin>
		<RateCommodity>
			<Code>GEN</Code>
			<Description>General</Description>
		</RateCommodity>
		<ReleaseType>
			<Code>EBL</Code>
			<Description>Express Bill of Lading</Description>
		</ReleaseType>
		<ScreeningStatus>
			<Code>REQ</Code>
			<Description>Requires Review</Description>
		</ScreeningStatus>
		<ServiceLevel>
			<Code>STD</Code>
			<Description>Standard</Description>
		</ServiceLevel>
		<ShipmentIncoTerm>
			<Code>FOB</Code>
			<Description>Free On Board</Description>
		</ShipmentIncoTerm>
		<ShipmentType>
			<Code>STD</Code>
			<Description>Standard House</Description>
		</ShipmentType>
		<ShippedOnBoard>
			<Code>SHP</Code>
			<Description>Shipped</Description>
		</ShippedOnBoard>
		<ShipperCODAmount>0.0000</ShipperCODAmount>
		<ShipperCODPayMethod>
			<Code/>
		</ShipperCODPayMethod>
		<TotalNoOfPacks>0</TotalNoOfPacks>
		<TotalNoOfPacksPackageType>
			<Code>CTN</Code>
			<Description>Carton</Description>
		</TotalNoOfPacksPackageType>
		<TotalVolume>4.000</TotalVolume>
		<TotalVolumeUnit>
			<Code>M3</Code>
			<Description>Cubic Meters</Description>
		</TotalVolumeUnit>
		<TotalWeight>400.000</TotalWeight>
		<TotalWeightUnit>
			<Code>KG</Code>
			<Description>Kilograms</Description>
		</TotalWeightUnit>
		<TranshipToOtherCFS>false</TranshipToOtherCFS>
		<TransportMode>
			<Code>SEA</Code>
			<Description>Sea Freight</Description>
		</TransportMode>
		<WarehouseLocation/>
		<WayBillNumber/>
		<WayBillType>
			<Code>HWB</Code>
			<Description>House Waybill</Description>
		</WayBillType>
		<LocalProcessing>
			<ArrivalCartageRef/>
			<DeliveryCartageAdvised/>
			<DeliveryCartageCompleted/>
			<DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
			<DeliveryLabourTime/>
			<DeliveryRequiredBy/>
			<DeliveryRequiredFrom/>
			<DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
			<DeliveryTruckWaitTime/>
			<DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
			<DemurrageOnDeliveryTime/>
			<DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
			<DemurrageOnPickupTime/>
			<EstimatedDelivery/>
			<EstimatedPickup/>
			<ExportStatement>
				<Code/>
			</ExportStatement>
			<FCLAvailable/>
			<FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
			<FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
			<FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
			<FCLDeliveryEquipmentNeeded>
				<Code>WUP</Code>
				<Description>Wait for Pack/Unpack</Description>
			</FCLDeliveryEquipmentNeeded>
			<FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
			<FCLPickupDetentionDays>0</FCLPickupDetentionDays>
			<FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
			<FCLPickupEquipmentNeeded>
				<Code>WUP</Code>
				<Description>Wait for Pack/Unpack</Description>
			</FCLPickupEquipmentNeeded>
			<FCLStorageCommences/>
			<HasProhibitedPackaging>false</HasProhibitedPackaging>
			<InsuranceRequired>false</InsuranceRequired>
			<IsContingencyRelease>false</IsContingencyRelease>
			<LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
			<LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
			<LCLAvailable/>
			<LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
			<LCLStorageCommences/>
			<PickupCartageAdvised/>
			<PickupCartageCompleted/>
			<PickupLabourCharge>0.0000</PickupLabourCharge>
			<PickupLabourTime/>
			<PickupRequiredBy/>
			<PickupRequiredFrom/>
			<PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
			<PickupTruckWaitTime/>
			<PrintOptionForPackagesOnAWB>
				<Code>DEF</Code>
				<Description>Default (Dims, fallback to Vol)</Description>
			</PrintOptionForPackagesOnAWB>
			<AdditionalServiceCollection Content=""Complete"">
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-06T18:20:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Contractor>
					<Duration/>
					<ExternalServiceId>SRV00000000000000007</ExternalServiceId>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>1.000</ServiceCount>
					<ServiceId>SRV00000000000000001</ServiceId>
					<ServiceNote>Test 001</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-05T18:21:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Contractor>
					<Duration/>
					<ExternalServiceId>SRV00000000000000008</ExternalServiceId>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>2.000</ServiceCount>
					<ServiceId>SRV00000000000000002</ServiceId>
					<ServiceNote>Test 002</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-04T18:22:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>213 W GRAND AVE</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>213 W GRAND AVENUE</AddressShortCode>
						<City>CHICAGO</City>
						<CompanyName>US CFS DEPOT (CHI)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>USCFSDCHI</OrganizationCode>
						<Phone>+13123220001</Phone>
						<Port>
							<Code>USCHI</Code>
							<Name>Chicago</Name>
						</Port>
						<Postcode>60654</Postcode>
						<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
						</ScreeningStatus>
						<State Description=""Illinois"">IL</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type>
									<Code>LSC</Code>
									<Description>Legacy System Code</Description>
								</Type>
								<CountryOfIssue>
									<Code>US</Code>
									<Name>United States</Name>
								</CountryOfIssue>
								<Value>USCFSDCHI</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</Contractor>
					<Duration/>
					<ExternalServiceId/>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>213 W GRAND AVE</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>213 W GRAND AVENUE</AddressShortCode>
						<City>CHICAGO</City>
						<CompanyName>US CFS DEPOT (CHI)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>USCFSDCHI</OrganizationCode>
						<Phone>+13123220001</Phone>
						<Port>
							<Code>USCHI</Code>
							<Name>Chicago</Name>
						</Port>
						<Postcode>60654</Postcode>
						<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
						</ScreeningStatus>
						<State Description=""Illinois"">IL</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type>
									<Code>LSC</Code>
									<Description>Legacy System Code</Description>
								</Type>
								<CountryOfIssue>
									<Code>US</Code>
									<Name>United States</Name>
								</CountryOfIssue>
								<Value>USCFSDCHI</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>3.000</ServiceCount>
					<ServiceId>SRV00000000000000003</ServiceId>
					<ServiceNote>Test 003</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
			</AdditionalServiceCollection>
		</LocalProcessing>
		<DateCollection>
			<Date>
				<Type>BookingConfirmed</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-02-06T18:17:00</Value>
			</Date>
			<Date>
				<Type>Received</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>Departure</Type>
				<IsEstimate>true</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>Arrival</Type>
				<IsEstimate>true</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>DeliveryDueDate</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>RevisedDeliveryDueDate</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>ShippedOnBoard</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>BillIssued</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>PickupReceiptRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-02-06T18:53:00</Value>
			</Date>
			<Date>
				<Type>DeliveryReceiptRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>PickupDispatchRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>DeliveryDispatchRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
		</DateCollection>
		<MessageNumberCollection>
			<MessageNumber Type=""TrackingID"">f95ad70a-936b-4b36-9bb9-7def767025c9</MessageNumber>
			<MessageNumber Type=""InterchangeNumber"">00000000000000000804</MessageNumber>
			<MessageNumber Type=""MessageNumber"">00000000000000002650</MessageNumber>
		</MessageNumberCollection>
		<MilestoneCollection>
			<Milestone>
				<Description>Register Shipment</Description>
				<EventCode>ADD</EventCode>
				<Sequence>10</Sequence>
				<ActualDate>2024-02-06T18:27:36.143</ActualDate>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Send Booking Confirmation</Description>
				<EventCode>DDV</EventCode>
				<Sequence>20</Sequence>
				<ActualDate/>
				<ConditionReference>*Booking Confirmation*</ConditionReference>
				<ConditionType>RFW</ConditionType>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Pickup Cartage Complete/Finalised</Description>
				<EventCode>PCF</EventCode>
				<Sequence>40</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Origin Receival from Wharf/Depot</Description>
				<EventCode>GIN</EventCode>
				<Sequence>50</Sequence>
				<ActualDate/>
				<ConditionReference>FAC=CTO</ConditionReference>
				<ConditionType>RFP</ConditionType>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>All Export Documents Received</Description>
				<EventCode>AED</EventCode>
				<Sequence>60</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Shipped on Board</Description>
				<EventCode>FLO</EventCode>
				<Sequence>70</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
		</MilestoneCollection>
		<PackingLineCollection Content=""Complete"">
			<PackingLine>
				<Commodity>
					<Code>GEN</Code>
					<Description>General</Description>
				</Commodity>
				<ContainerPackingOrder>0</ContainerPackingOrder>
				<CountryOfOrigin>
					<Code/>
				</CountryOfOrigin>
				<DetailedDescription/>
				<EndItemNo>0</EndItemNo>
				<ExportReferenceNumber/>
				<GoodsDescription/>
				<HarmonisedCode/>
				<Height>100.000</Height>
				<ImportReferenceNumber/>
				<ItemNo>0</ItemNo>
				<LastKnownCFSStatus>
					<Code/>
				</LastKnownCFSStatus>
				<LastKnownCFSStatusDate/>
				<Length>100.000</Length>
				<LengthUnit>
					<Code>CM</Code>
					<Description>Centimeters</Description>
				</LengthUnit>
				<LinePrice>0.0000</LinePrice>
				<Link>1</Link>
				<LoadingMeters>0.000</LoadingMeters>
				<MarksAndNos/>
				<OutturnComment/>
				<OutturnDamagedQty>0</OutturnDamagedQty>
				<OutturnedHeight>0.000</OutturnedHeight>
				<OutturnedLength>0.000</OutturnedLength>
				<OutturnedVolume>0.000</OutturnedVolume>
				<OutturnedWeight>0.000</OutturnedWeight>
				<OutturnedWidth>0.000</OutturnedWidth>
				<OutturnPillagedQty>0</OutturnPillagedQty>
				<OutturnQty>0</OutturnQty>
				<PackingLineID>WUTB0C00000458</PackingLineID>
				<PackQty>4</PackQty>
				<PackType>
					<Code>PLT</Code>
					<Description>Pallet</Description>
				</PackType>
				<ReferenceNumber/>
				<RequiresTemperatureControl>false</RequiresTemperatureControl>
				<Volume>4.000</Volume>
				<VolumeUnit>
					<Code>M3</Code>
					<Description>Cubic Meters</Description>
				</VolumeUnit>
				<Weight>400.000</Weight>
				<WeightUnit>
					<Code>KG</Code>
					<Description>Kilograms</Description>
				</WeightUnit>
				<Width>100.000</Width>
				<PackedItemCollection/>
			</PackingLine>
		</PackingLineCollection>
	</Shipment>
</UniversalShipment>
";
		#endregion

		public void TestReadingInXMLCheckShipmentServiceCodeAndServiceIdCanNotBeDuplicate()
		{
			var universalShipment = new Shipment();

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(UniversalShipmentXmlWithDupServiceCodeAndServiceId)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalShipment, stream, logger);
			}

			CombineAssertions(() =>
			{
				AssertMultilineASCIIEquals("Services with duplicate service id and service code should raise warning.", @"Warning - Line 402: <Shipment>.<LocalProcessing>.<AdditionalServiceCollection> - Duplicate Candidate Key found, element <AdditionalService> opened at line 322 was not imported. Candidate Key: [ServiceCode='FUM', ServiceId='']
Warning - Line 596: <Shipment>.<LocalProcessing>.<AdditionalServiceCollection> - Duplicate Candidate Key found, element <AdditionalService> opened at line 500 was not imported. Candidate Key: [ServiceCode='FUM', ServiceId='SRV00000000000000003']", logger.Logs);
				AssertEquals("Services with duplicate service id and service code should be only converted 2.", 2, universalShipment.LocalProcessing.AdditionalServiceCollection.Count);
			});
		}

		#region
		const string UniversalShipmentXmlWithDupServiceCodeAndServiceId = @"
<UniversalShipment>
	<Shipment>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>ForwardingShipment</Type>
					<Key/>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>DUS</Code>
				<Country>
					<Code>US</Code>
					<Name>United States</Name>
				</Country>
				<Name>Your United States Corp</Name>
			</Company>
			<DataProvider>WUTB0CDUS</DataProvider>
			<EnterpriseID>WUT</EnterpriseID>
			<EventBranch>
				<Code>LAX</Code>
				<Name>US - Los Angeles Branch</Name>
			</EventBranch>
			<EventDepartment>
				<Code>BRN</Code>
				<Name>Branch</Name>
			</EventDepartment>
			<EventType>
				<Code/>
			</EventType>
			<EventUser>
				<Code>E</Code>
				<Name>CargoWise Support</Name>
			</EventUser>
			<ServerID>B0C</ServerID>
			<TriggerCount>1</TriggerCount>
			<TriggerDate>2024-02-06T20:09:47.87</TriggerDate>
			<TriggerDescription/>
			<TriggerType>Manual</TriggerType>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>CNR</Code>
					<Description>Consignor</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<ActualChargeable>4.000</ActualChargeable>
		<AdditionalTerms>Test 1</AdditionalTerms>
		<BookingConfirmationReference/>
		<CartageWaybillNumber/>
		<CFSReference/>
		<CommunityTransitStatus>
			<Code/>
		</CommunityTransitStatus>
		<CompanyTariffLevelOverride>0</CompanyTariffLevelOverride>
		<ContainerCount>0</ContainerCount>
		<ContainerMode>
			<Code>FCL</Code>
			<Description>Full Container Load</Description>
		</ContainerMode>
		<DocumentedChargeable>4.000</DocumentedChargeable>
		<DocumentedVolume>4.000</DocumentedVolume>
		<DocumentedWeight>400.000</DocumentedWeight>
		<FMCTariffID/>
		<FreightRate>0.0000</FreightRate>
		<FreightRateCurrency>
			<Code/>
		</FreightRateCurrency>
		<GoodsDescription/>
		<GoodsValue>0.0000</GoodsValue>
		<GoodsValueCurrency>
			<Code>USD</Code>
			<Description>United States Dollar</Description>
		</GoodsValueCurrency>
		<HBLAWBChargesDisplay>
			<Code>SHW</Code>
			<Description>Show Collect Charges</Description>
		</HBLAWBChargesDisplay>
		<HBLContainerPackModeOverride/>
		<HouseBillOfLadingType>
			<Code>IAU</Code>
			<Description>IT Club Australia</Description>
		</HouseBillOfLadingType>
		<InsuranceValue>0.0000</InsuranceValue>
		<InsuranceValueCurrency>
			<Code>USD</Code>
			<Description>United States Dollar</Description>
		</InsuranceValueCurrency>
		<InterimReceiptNumber/>
		<IsBooking>true</IsBooking>
		<IsCancelled>false</IsCancelled>
		<IsCFSRegistered>true</IsCFSRegistered>
		<IsDirectBooking>false</IsDirectBooking>
		<IsForwardRegistered>true</IsForwardRegistered>
		<IsHighRisk>false</IsHighRisk>
		<IsNeutralMaster>false</IsNeutralMaster>
		<IsShipping>false</IsShipping>
		<IsSplitShipment>false</IsSplitShipment>
		<ManifestedChargeable>4.000</ManifestedChargeable>
		<ManifestedVolume>4.000</ManifestedVolume>
		<ManifestedWeight>400.000</ManifestedWeight>
		<NoCopyBills>1</NoCopyBills>
		<NoOriginalBills>0</NoOriginalBills>
		<OuterPacks>4</OuterPacks>
		<OuterPacksPackageType>
			<Code>PLT</Code>
			<Description>Pallet</Description>
		</OuterPacksPackageType>
		<PackingOrder>0</PackingOrder>
		<PortOfDestination>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfDestination>
		<PortOfDischarge>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfDischarge>
		<PortOfLoading>
			<Code>USSEA</Code>
			<Name>Seattle</Name>
		</PortOfLoading>
		<PortOfOrigin>
			<Code>USSEA</Code>
			<Name>Seattle</Name>
		</PortOfOrigin>
		<RateCommodity>
			<Code>GEN</Code>
			<Description>General</Description>
		</RateCommodity>
		<ReleaseType>
			<Code>EBL</Code>
			<Description>Express Bill of Lading</Description>
		</ReleaseType>
		<ScreeningStatus>
			<Code>REQ</Code>
			<Description>Requires Review</Description>
		</ScreeningStatus>
		<ServiceLevel>
			<Code>STD</Code>
			<Description>Standard</Description>
		</ServiceLevel>
		<ShipmentIncoTerm>
			<Code>FOB</Code>
			<Description>Free On Board</Description>
		</ShipmentIncoTerm>
		<ShipmentType>
			<Code>STD</Code>
			<Description>Standard House</Description>
		</ShipmentType>
		<ShippedOnBoard>
			<Code>SHP</Code>
			<Description>Shipped</Description>
		</ShippedOnBoard>
		<ShipperCODAmount>0.0000</ShipperCODAmount>
		<ShipperCODPayMethod>
			<Code/>
		</ShipperCODPayMethod>
		<TotalNoOfPacks>0</TotalNoOfPacks>
		<TotalNoOfPacksPackageType>
			<Code>CTN</Code>
			<Description>Carton</Description>
		</TotalNoOfPacksPackageType>
		<TotalVolume>4.000</TotalVolume>
		<TotalVolumeUnit>
			<Code>M3</Code>
			<Description>Cubic Meters</Description>
		</TotalVolumeUnit>
		<TotalWeight>400.000</TotalWeight>
		<TotalWeightUnit>
			<Code>KG</Code>
			<Description>Kilograms</Description>
		</TotalWeightUnit>
		<TranshipToOtherCFS>false</TranshipToOtherCFS>
		<TransportMode>
			<Code>SEA</Code>
			<Description>Sea Freight</Description>
		</TransportMode>
		<WarehouseLocation/>
		<WayBillNumber/>
		<WayBillType>
			<Code>HWB</Code>
			<Description>House Waybill</Description>
		</WayBillType>
		<LocalProcessing>
			<ArrivalCartageRef/>
			<DeliveryCartageAdvised/>
			<DeliveryCartageCompleted/>
			<DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
			<DeliveryLabourTime/>
			<DeliveryRequiredBy/>
			<DeliveryRequiredFrom/>
			<DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
			<DeliveryTruckWaitTime/>
			<DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
			<DemurrageOnDeliveryTime/>
			<DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
			<DemurrageOnPickupTime/>
			<EstimatedDelivery/>
			<EstimatedPickup/>
			<ExportStatement>
				<Code/>
			</ExportStatement>
			<FCLAvailable/>
			<FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
			<FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
			<FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
			<FCLDeliveryEquipmentNeeded>
				<Code>WUP</Code>
				<Description>Wait for Pack/Unpack</Description>
			</FCLDeliveryEquipmentNeeded>
			<FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
			<FCLPickupDetentionDays>0</FCLPickupDetentionDays>
			<FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
			<FCLPickupEquipmentNeeded>
				<Code>WUP</Code>
				<Description>Wait for Pack/Unpack</Description>
			</FCLPickupEquipmentNeeded>
			<FCLStorageCommences/>
			<HasProhibitedPackaging>false</HasProhibitedPackaging>
			<InsuranceRequired>false</InsuranceRequired>
			<IsContingencyRelease>false</IsContingencyRelease>
			<LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
			<LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
			<LCLAvailable/>
			<LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
			<LCLStorageCommences/>
			<PickupCartageAdvised/>
			<PickupCartageCompleted/>
			<PickupLabourCharge>0.0000</PickupLabourCharge>
			<PickupLabourTime/>
			<PickupRequiredBy/>
			<PickupRequiredFrom/>
			<PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
			<PickupTruckWaitTime/>
			<PrintOptionForPackagesOnAWB>
				<Code>DEF</Code>
				<Description>Default (Dims, fallback to Vol)</Description>
			</PrintOptionForPackagesOnAWB>
			<AdditionalServiceCollection Content=""Complete"">
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-06T18:20:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Contractor>
					<Duration/>
					<ExternalServiceId>SRV00000000000000007</ExternalServiceId>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>1.000</ServiceCount>
					<ServiceId></ServiceId>
					<ServiceNote>Test 001</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-05T18:21:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Contractor>
					<Duration/>
					<ExternalServiceId>SRV00000000000000008</ExternalServiceId>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>2.000</ServiceCount>
					<ServiceId></ServiceId>
					<ServiceNote>Test 002</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-04T18:22:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>213 W GRAND AVE</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>213 W GRAND AVENUE</AddressShortCode>
						<City>CHICAGO</City>
						<CompanyName>US CFS DEPOT (CHI)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>USCFSDCHI</OrganizationCode>
						<Phone>+13123220001</Phone>
						<Port>
							<Code>USCHI</Code>
							<Name>Chicago</Name>
						</Port>
						<Postcode>60654</Postcode>
						<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
						</ScreeningStatus>
						<State Description=""Illinois"">IL</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type>
									<Code>LSC</Code>
									<Description>Legacy System Code</Description>
								</Type>
								<CountryOfIssue>
									<Code>US</Code>
									<Name>United States</Name>
								</CountryOfIssue>
								<Value>USCFSDCHI</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</Contractor>
					<Duration/>
					<ExternalServiceId/>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>213 W GRAND AVE</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>213 W GRAND AVENUE</AddressShortCode>
						<City>CHICAGO</City>
						<CompanyName>US CFS DEPOT (CHI)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>USCFSDCHI</OrganizationCode>
						<Phone>+13123220001</Phone>
						<Port>
							<Code>USCHI</Code>
							<Name>Chicago</Name>
						</Port>
						<Postcode>60654</Postcode>
						<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
						</ScreeningStatus>
						<State Description=""Illinois"">IL</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type>
									<Code>LSC</Code>
									<Description>Legacy System Code</Description>
								</Type>
								<CountryOfIssue>
									<Code>US</Code>
									<Name>United States</Name>
								</CountryOfIssue>
								<Value>USCFSDCHI</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>3.000</ServiceCount>
					<ServiceId>SRV00000000000000003</ServiceId>
					<ServiceNote>Test 003</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-04T18:22:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>213 W GRAND AVE</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>213 W GRAND AVENUE</AddressShortCode>
						<City>CHICAGO</City>
						<CompanyName>US CFS DEPOT (CHI)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>USCFSDCHI</OrganizationCode>
						<Phone>+13123220001</Phone>
						<Port>
							<Code>USCHI</Code>
							<Name>Chicago</Name>
						</Port>
						<Postcode>60654</Postcode>
						<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
						</ScreeningStatus>
						<State Description=""Illinois"">IL</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type>
									<Code>LSC</Code>
									<Description>Legacy System Code</Description>
								</Type>
								<CountryOfIssue>
									<Code>US</Code>
									<Name>United States</Name>
								</CountryOfIssue>
								<Value>USCFSDCHI</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</Contractor>
					<Duration/>
					<ExternalServiceId/>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>213 W GRAND AVE</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>213 W GRAND AVENUE</AddressShortCode>
						<City>CHICAGO</City>
						<CompanyName>US CFS DEPOT (CHI)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>USCFSDCHI</OrganizationCode>
						<Phone>+13123220001</Phone>
						<Port>
							<Code>USCHI</Code>
							<Name>Chicago</Name>
						</Port>
						<Postcode>60654</Postcode>
						<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
						</ScreeningStatus>
						<State Description=""Illinois"">IL</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type>
									<Code>LSC</Code>
									<Description>Legacy System Code</Description>
								</Type>
								<CountryOfIssue>
									<Code>US</Code>
									<Name>United States</Name>
								</CountryOfIssue>
								<Value>USCFSDCHI</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>3.000</ServiceCount>
					<ServiceId>SRV00000000000000003</ServiceId>
					<ServiceNote>Test 003</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
			</AdditionalServiceCollection>
		</LocalProcessing>
		<DateCollection>
			<Date>
				<Type>BookingConfirmed</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-02-06T18:17:00</Value>
			</Date>
			<Date>
				<Type>Received</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>Departure</Type>
				<IsEstimate>true</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>Arrival</Type>
				<IsEstimate>true</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>DeliveryDueDate</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>RevisedDeliveryDueDate</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>ShippedOnBoard</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>BillIssued</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>PickupReceiptRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-02-06T18:53:00</Value>
			</Date>
			<Date>
				<Type>DeliveryReceiptRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>PickupDispatchRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>DeliveryDispatchRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
		</DateCollection>
		<MessageNumberCollection>
			<MessageNumber Type=""TrackingID"">f95ad70a-936b-4b36-9bb9-7def767025c9</MessageNumber>
			<MessageNumber Type=""InterchangeNumber"">00000000000000000804</MessageNumber>
			<MessageNumber Type=""MessageNumber"">00000000000000002650</MessageNumber>
		</MessageNumberCollection>
		<MilestoneCollection>
			<Milestone>
				<Description>Register Shipment</Description>
				<EventCode>ADD</EventCode>
				<Sequence>10</Sequence>
				<ActualDate>2024-02-06T18:27:36.143</ActualDate>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Send Booking Confirmation</Description>
				<EventCode>DDV</EventCode>
				<Sequence>20</Sequence>
				<ActualDate/>
				<ConditionReference>*Booking Confirmation*</ConditionReference>
				<ConditionType>RFW</ConditionType>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Pickup Cartage Complete/Finalised</Description>
				<EventCode>PCF</EventCode>
				<Sequence>40</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Origin Receival from Wharf/Depot</Description>
				<EventCode>GIN</EventCode>
				<Sequence>50</Sequence>
				<ActualDate/>
				<ConditionReference>FAC=CTO</ConditionReference>
				<ConditionType>RFP</ConditionType>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>All Export Documents Received</Description>
				<EventCode>AED</EventCode>
				<Sequence>60</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Shipped on Board</Description>
				<EventCode>FLO</EventCode>
				<Sequence>70</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
		</MilestoneCollection>
		<PackingLineCollection Content=""Complete"">
			<PackingLine>
				<Commodity>
					<Code>GEN</Code>
					<Description>General</Description>
				</Commodity>
				<ContainerPackingOrder>0</ContainerPackingOrder>
				<CountryOfOrigin>
					<Code/>
				</CountryOfOrigin>
				<DetailedDescription/>
				<EndItemNo>0</EndItemNo>
				<ExportReferenceNumber/>
				<GoodsDescription/>
				<HarmonisedCode/>
				<Height>100.000</Height>
				<ImportReferenceNumber/>
				<ItemNo>0</ItemNo>
				<LastKnownCFSStatus>
					<Code/>
				</LastKnownCFSStatus>
				<LastKnownCFSStatusDate/>
				<Length>100.000</Length>
				<LengthUnit>
					<Code>CM</Code>
					<Description>Centimeters</Description>
				</LengthUnit>
				<LinePrice>0.0000</LinePrice>
				<Link>1</Link>
				<LoadingMeters>0.000</LoadingMeters>
				<MarksAndNos/>
				<OutturnComment/>
				<OutturnDamagedQty>0</OutturnDamagedQty>
				<OutturnedHeight>0.000</OutturnedHeight>
				<OutturnedLength>0.000</OutturnedLength>
				<OutturnedVolume>0.000</OutturnedVolume>
				<OutturnedWeight>0.000</OutturnedWeight>
				<OutturnedWidth>0.000</OutturnedWidth>
				<OutturnPillagedQty>0</OutturnPillagedQty>
				<OutturnQty>0</OutturnQty>
				<PackingLineID>WUTB0C00000458</PackingLineID>
				<PackQty>4</PackQty>
				<PackType>
					<Code>PLT</Code>
					<Description>Pallet</Description>
				</PackType>
				<ReferenceNumber/>
				<RequiresTemperatureControl>false</RequiresTemperatureControl>
				<Volume>4.000</Volume>
				<VolumeUnit>
					<Code>M3</Code>
					<Description>Cubic Meters</Description>
				</VolumeUnit>
				<Weight>400.000</Weight>
				<WeightUnit>
					<Code>KG</Code>
					<Description>Kilograms</Description>
				</WeightUnit>
				<Width>100.000</Width>
				<PackedItemCollection/>
			</PackingLine>
		</PackingLineCollection>
	</Shipment>
</UniversalShipment>
";
		#endregion

		public void TestReadingInXMLCheckShipmentServiceIdCanNotBeDuplicate()
		{
			var universalShipment = new Shipment();

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(UniversalShipmentXmlWithDupServiceId)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalShipment, stream, logger);
			}

			CombineAssertions(() =>
			{
				AssertMultilineASCIIEquals("Services with duplicate service id should raise warning.", @"Warning - Line 402: <Shipment>.<LocalProcessing>.<AdditionalServiceCollection> - Duplicate Candidate Key found, element <AdditionalService> opened at line 322 was not imported. Candidate Key: [ServiceCode='FUM', ServiceId='SRV00000000000000001']
Warning - Line 499: <Shipment>.<LocalProcessing>.<AdditionalServiceCollection> - Duplicate Candidate Key found, element <AdditionalService> opened at line 403 was not imported. Candidate Key: [ServiceCode='FUM', ServiceId='SRV00000000000000001']", logger.Logs);
				AssertEquals("Services with duplicate service id should be only converted one.", 1, universalShipment.LocalProcessing.AdditionalServiceCollection.Count);
			});
		}

		#region UniversalShipmentXmlWithDupServiceId
		const string UniversalShipmentXmlWithDupServiceId = @"
<UniversalShipment>
	<Shipment>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>ForwardingShipment</Type>
					<Key/>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>DUS</Code>
				<Country>
					<Code>US</Code>
					<Name>United States</Name>
				</Country>
				<Name>Your United States Corp</Name>
			</Company>
			<DataProvider>WUTB0CDUS</DataProvider>
			<EnterpriseID>WUT</EnterpriseID>
			<EventBranch>
				<Code>LAX</Code>
				<Name>US - Los Angeles Branch</Name>
			</EventBranch>
			<EventDepartment>
				<Code>BRN</Code>
				<Name>Branch</Name>
			</EventDepartment>
			<EventType>
				<Code/>
			</EventType>
			<EventUser>
				<Code>E</Code>
				<Name>CargoWise Support</Name>
			</EventUser>
			<ServerID>B0C</ServerID>
			<TriggerCount>1</TriggerCount>
			<TriggerDate>2024-02-06T20:09:47.87</TriggerDate>
			<TriggerDescription/>
			<TriggerType>Manual</TriggerType>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>CNR</Code>
					<Description>Consignor</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<ActualChargeable>4.000</ActualChargeable>
		<AdditionalTerms>Test 1</AdditionalTerms>
		<BookingConfirmationReference/>
		<CartageWaybillNumber/>
		<CFSReference/>
		<CommunityTransitStatus>
			<Code/>
		</CommunityTransitStatus>
		<CompanyTariffLevelOverride>0</CompanyTariffLevelOverride>
		<ContainerCount>0</ContainerCount>
		<ContainerMode>
			<Code>FCL</Code>
			<Description>Full Container Load</Description>
		</ContainerMode>
		<DocumentedChargeable>4.000</DocumentedChargeable>
		<DocumentedVolume>4.000</DocumentedVolume>
		<DocumentedWeight>400.000</DocumentedWeight>
		<FMCTariffID/>
		<FreightRate>0.0000</FreightRate>
		<FreightRateCurrency>
			<Code/>
		</FreightRateCurrency>
		<GoodsDescription/>
		<GoodsValue>0.0000</GoodsValue>
		<GoodsValueCurrency>
			<Code>USD</Code>
			<Description>United States Dollar</Description>
		</GoodsValueCurrency>
		<HBLAWBChargesDisplay>
			<Code>SHW</Code>
			<Description>Show Collect Charges</Description>
		</HBLAWBChargesDisplay>
		<HBLContainerPackModeOverride/>
		<HouseBillOfLadingType>
			<Code>IAU</Code>
			<Description>IT Club Australia</Description>
		</HouseBillOfLadingType>
		<InsuranceValue>0.0000</InsuranceValue>
		<InsuranceValueCurrency>
			<Code>USD</Code>
			<Description>United States Dollar</Description>
		</InsuranceValueCurrency>
		<InterimReceiptNumber/>
		<IsBooking>true</IsBooking>
		<IsCancelled>false</IsCancelled>
		<IsCFSRegistered>true</IsCFSRegistered>
		<IsDirectBooking>false</IsDirectBooking>
		<IsForwardRegistered>true</IsForwardRegistered>
		<IsHighRisk>false</IsHighRisk>
		<IsNeutralMaster>false</IsNeutralMaster>
		<IsShipping>false</IsShipping>
		<IsSplitShipment>false</IsSplitShipment>
		<ManifestedChargeable>4.000</ManifestedChargeable>
		<ManifestedVolume>4.000</ManifestedVolume>
		<ManifestedWeight>400.000</ManifestedWeight>
		<NoCopyBills>1</NoCopyBills>
		<NoOriginalBills>0</NoOriginalBills>
		<OuterPacks>4</OuterPacks>
		<OuterPacksPackageType>
			<Code>PLT</Code>
			<Description>Pallet</Description>
		</OuterPacksPackageType>
		<PackingOrder>0</PackingOrder>
		<PortOfDestination>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfDestination>
		<PortOfDischarge>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfDischarge>
		<PortOfLoading>
			<Code>USSEA</Code>
			<Name>Seattle</Name>
		</PortOfLoading>
		<PortOfOrigin>
			<Code>USSEA</Code>
			<Name>Seattle</Name>
		</PortOfOrigin>
		<RateCommodity>
			<Code>GEN</Code>
			<Description>General</Description>
		</RateCommodity>
		<ReleaseType>
			<Code>EBL</Code>
			<Description>Express Bill of Lading</Description>
		</ReleaseType>
		<ScreeningStatus>
			<Code>REQ</Code>
			<Description>Requires Review</Description>
		</ScreeningStatus>
		<ServiceLevel>
			<Code>STD</Code>
			<Description>Standard</Description>
		</ServiceLevel>
		<ShipmentIncoTerm>
			<Code>FOB</Code>
			<Description>Free On Board</Description>
		</ShipmentIncoTerm>
		<ShipmentType>
			<Code>STD</Code>
			<Description>Standard House</Description>
		</ShipmentType>
		<ShippedOnBoard>
			<Code>SHP</Code>
			<Description>Shipped</Description>
		</ShippedOnBoard>
		<ShipperCODAmount>0.0000</ShipperCODAmount>
		<ShipperCODPayMethod>
			<Code/>
		</ShipperCODPayMethod>
		<TotalNoOfPacks>0</TotalNoOfPacks>
		<TotalNoOfPacksPackageType>
			<Code>CTN</Code>
			<Description>Carton</Description>
		</TotalNoOfPacksPackageType>
		<TotalVolume>4.000</TotalVolume>
		<TotalVolumeUnit>
			<Code>M3</Code>
			<Description>Cubic Meters</Description>
		</TotalVolumeUnit>
		<TotalWeight>400.000</TotalWeight>
		<TotalWeightUnit>
			<Code>KG</Code>
			<Description>Kilograms</Description>
		</TotalWeightUnit>
		<TranshipToOtherCFS>false</TranshipToOtherCFS>
		<TransportMode>
			<Code>SEA</Code>
			<Description>Sea Freight</Description>
		</TransportMode>
		<WarehouseLocation/>
		<WayBillNumber/>
		<WayBillType>
			<Code>HWB</Code>
			<Description>House Waybill</Description>
		</WayBillType>
		<LocalProcessing>
			<ArrivalCartageRef/>
			<DeliveryCartageAdvised/>
			<DeliveryCartageCompleted/>
			<DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
			<DeliveryLabourTime/>
			<DeliveryRequiredBy/>
			<DeliveryRequiredFrom/>
			<DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
			<DeliveryTruckWaitTime/>
			<DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
			<DemurrageOnDeliveryTime/>
			<DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
			<DemurrageOnPickupTime/>
			<EstimatedDelivery/>
			<EstimatedPickup/>
			<ExportStatement>
				<Code/>
			</ExportStatement>
			<FCLAvailable/>
			<FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
			<FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
			<FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
			<FCLDeliveryEquipmentNeeded>
				<Code>WUP</Code>
				<Description>Wait for Pack/Unpack</Description>
			</FCLDeliveryEquipmentNeeded>
			<FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
			<FCLPickupDetentionDays>0</FCLPickupDetentionDays>
			<FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
			<FCLPickupEquipmentNeeded>
				<Code>WUP</Code>
				<Description>Wait for Pack/Unpack</Description>
			</FCLPickupEquipmentNeeded>
			<FCLStorageCommences/>
			<HasProhibitedPackaging>false</HasProhibitedPackaging>
			<InsuranceRequired>false</InsuranceRequired>
			<IsContingencyRelease>false</IsContingencyRelease>
			<LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
			<LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
			<LCLAvailable/>
			<LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
			<LCLStorageCommences/>
			<PickupCartageAdvised/>
			<PickupCartageCompleted/>
			<PickupLabourCharge>0.0000</PickupLabourCharge>
			<PickupLabourTime/>
			<PickupRequiredBy/>
			<PickupRequiredFrom/>
			<PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
			<PickupTruckWaitTime/>
			<PrintOptionForPackagesOnAWB>
				<Code>DEF</Code>
				<Description>Default (Dims, fallback to Vol)</Description>
			</PrintOptionForPackagesOnAWB>
			<AdditionalServiceCollection Content=""Complete"">
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-06T18:20:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Contractor>
					<Duration/>
					<ExternalServiceId>SRV00000000000000007</ExternalServiceId>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>1.000</ServiceCount>
					<ServiceId>SRV00000000000000001</ServiceId>
					<ServiceNote>Test 001</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-05T18:21:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Contractor>
					<Duration/>
					<ExternalServiceId>SRV00000000000000008</ExternalServiceId>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>6161 W CENTURY BLVD</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>6161 W CENTURY BLVD</AddressShortCode>
						<City>LOS ANGELES</City>
						<CompanyName>YOUR UNITED STATES COMPANY (LOS ANGELES)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax>+13106492222</Fax>
						<GovRegNum>78-789452147</GovRegNum>
						<GovRegNumType>
							<Code>EIN</Code>
							<Description>Employer Identification Number</Description>
						</GovRegNumType>
						<OrganizationCode>YOUUNI_WW</OrganizationCode>
						<Phone>+13106491111</Phone>
						<Port>
							<Code>USLAX</Code>
							<Name>Los Angeles</Name>
						</Port>
						<Postcode>90045</Postcode>
						<ScreeningStatus>
							<Code>CLR</Code>
							<Description>Clear</Description>
						</ScreeningStatus>
						<State Description=""California"">CA</State>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>2.000</ServiceCount>
					<ServiceId>SRV00000000000000001</ServiceId>
					<ServiceNote>Test 002</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
				<AdditionalService>
					<ServiceCode>
						<Code>FUM</Code>
						<Description>Fumigation</Description>
					</ServiceCode>
					<Booked>2024-02-04T18:22:00</Booked>
					<Completed/>
					<Contractor>
						<AddressType>Contractor</AddressType>
						<Address1>213 W GRAND AVE</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>213 W GRAND AVENUE</AddressShortCode>
						<City>CHICAGO</City>
						<CompanyName>US CFS DEPOT (CHI)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>USCFSDCHI</OrganizationCode>
						<Phone>+13123220001</Phone>
						<Port>
							<Code>USCHI</Code>
							<Name>Chicago</Name>
						</Port>
						<Postcode>60654</Postcode>
						<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
						</ScreeningStatus>
						<State Description=""Illinois"">IL</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type>
									<Code>LSC</Code>
									<Description>Legacy System Code</Description>
								</Type>
								<CountryOfIssue>
									<Code>US</Code>
									<Name>United States</Name>
								</CountryOfIssue>
								<Value>USCFSDCHI</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</Contractor>
					<Duration/>
					<ExternalServiceId/>
					<Location>
						<AddressType>Location</AddressType>
						<Address1>213 W GRAND AVE</Address1>
						<Address2/>
						<AddressOverride>false</AddressOverride>
						<AddressShortCode>213 W GRAND AVENUE</AddressShortCode>
						<City>CHICAGO</City>
						<CompanyName>US CFS DEPOT (CHI)</CompanyName>
						<Country>
							<Code>US</Code>
							<Name>United States</Name>
						</Country>
						<Email/>
						<Fax/>
						<OrganizationCode>USCFSDCHI</OrganizationCode>
						<Phone>+13123220001</Phone>
						<Port>
							<Code>USCHI</Code>
							<Name>Chicago</Name>
						</Port>
						<Postcode>60654</Postcode>
						<ScreeningStatus>
							<Code>UNK</Code>
							<Description>Unknown</Description>
						</ScreeningStatus>
						<State Description=""Illinois"">IL</State>
						<RegistrationNumberCollection>
							<RegistrationNumber>
								<Type>
									<Code>LSC</Code>
									<Description>Legacy System Code</Description>
								</Type>
								<CountryOfIssue>
									<Code>US</Code>
									<Name>United States</Name>
								</CountryOfIssue>
								<Value>USCFSDCHI</Value>
							</RegistrationNumber>
						</RegistrationNumberCollection>
					</Location>
					<MeasurementBasis/>
					<References/>
					<ServiceCount>3.000</ServiceCount>
					<ServiceId>SRV00000000000000001</ServiceId>
					<ServiceNote>Test 003</ServiceNote>
					<ServiceRate>0.0000</ServiceRate>
					<ServiceRateCurrency>USD</ServiceRateCurrency>
				</AdditionalService>
			</AdditionalServiceCollection>
		</LocalProcessing>
		<DateCollection>
			<Date>
				<Type>BookingConfirmed</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-02-06T18:17:00</Value>
			</Date>
			<Date>
				<Type>Received</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>Departure</Type>
				<IsEstimate>true</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>Arrival</Type>
				<IsEstimate>true</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>DeliveryDueDate</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>RevisedDeliveryDueDate</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>ShippedOnBoard</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>BillIssued</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>PickupReceiptRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value>2024-02-06T18:53:00</Value>
			</Date>
			<Date>
				<Type>DeliveryReceiptRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>PickupDispatchRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
			<Date>
				<Type>DeliveryDispatchRequested</Type>
				<IsEstimate>false</IsEstimate>
				<Value/>
			</Date>
		</DateCollection>
		<MessageNumberCollection>
			<MessageNumber Type=""TrackingID"">f95ad70a-936b-4b36-9bb9-7def767025c9</MessageNumber>
			<MessageNumber Type=""InterchangeNumber"">00000000000000000804</MessageNumber>
			<MessageNumber Type=""MessageNumber"">00000000000000002650</MessageNumber>
		</MessageNumberCollection>
		<MilestoneCollection>
			<Milestone>
				<Description>Register Shipment</Description>
				<EventCode>ADD</EventCode>
				<Sequence>10</Sequence>
				<ActualDate>2024-02-06T18:27:36.143</ActualDate>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Send Booking Confirmation</Description>
				<EventCode>DDV</EventCode>
				<Sequence>20</Sequence>
				<ActualDate/>
				<ConditionReference>*Booking Confirmation*</ConditionReference>
				<ConditionType>RFW</ConditionType>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Pickup Cartage Complete/Finalised</Description>
				<EventCode>PCF</EventCode>
				<Sequence>40</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Origin Receival from Wharf/Depot</Description>
				<EventCode>GIN</EventCode>
				<Sequence>50</Sequence>
				<ActualDate/>
				<ConditionReference>FAC=CTO</ConditionReference>
				<ConditionType>RFP</ConditionType>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>All Export Documents Received</Description>
				<EventCode>AED</EventCode>
				<Sequence>60</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
			<Milestone>
				<Description>Shipped on Board</Description>
				<EventCode>FLO</EventCode>
				<Sequence>70</Sequence>
				<ActualDate/>
				<ConditionReference/>
				<ConditionType/>
				<EstimatedDate/>
			</Milestone>
		</MilestoneCollection>
		<PackingLineCollection Content=""Complete"">
			<PackingLine>
				<Commodity>
					<Code>GEN</Code>
					<Description>General</Description>
				</Commodity>
				<ContainerPackingOrder>0</ContainerPackingOrder>
				<CountryOfOrigin>
					<Code/>
				</CountryOfOrigin>
				<DetailedDescription/>
				<EndItemNo>0</EndItemNo>
				<ExportReferenceNumber/>
				<GoodsDescription/>
				<HarmonisedCode/>
				<Height>100.000</Height>
				<ImportReferenceNumber/>
				<ItemNo>0</ItemNo>
				<LastKnownCFSStatus>
					<Code/>
				</LastKnownCFSStatus>
				<LastKnownCFSStatusDate/>
				<Length>100.000</Length>
				<LengthUnit>
					<Code>CM</Code>
					<Description>Centimeters</Description>
				</LengthUnit>
				<LinePrice>0.0000</LinePrice>
				<Link>1</Link>
				<LoadingMeters>0.000</LoadingMeters>
				<MarksAndNos/>
				<OutturnComment/>
				<OutturnDamagedQty>0</OutturnDamagedQty>
				<OutturnedHeight>0.000</OutturnedHeight>
				<OutturnedLength>0.000</OutturnedLength>
				<OutturnedVolume>0.000</OutturnedVolume>
				<OutturnedWeight>0.000</OutturnedWeight>
				<OutturnedWidth>0.000</OutturnedWidth>
				<OutturnPillagedQty>0</OutturnPillagedQty>
				<OutturnQty>0</OutturnQty>
				<PackingLineID>WUTB0C00000458</PackingLineID>
				<PackQty>4</PackQty>
				<PackType>
					<Code>PLT</Code>
					<Description>Pallet</Description>
				</PackType>
				<ReferenceNumber/>
				<RequiresTemperatureControl>false</RequiresTemperatureControl>
				<Volume>4.000</Volume>
				<VolumeUnit>
					<Code>M3</Code>
					<Description>Cubic Meters</Description>
				</VolumeUnit>
				<Weight>400.000</Weight>
				<WeightUnit>
					<Code>KG</Code>
					<Description>Kilograms</Description>
				</WeightUnit>
				<Width>100.000</Width>
				<PackedItemCollection/>
			</PackingLine>
		</PackingLineCollection>
	</Shipment>
</UniversalShipment>";
		#endregion

		public void TestReadingInXMLChecksForDuplicateCandidateKeys()
		{
			var candidateKeyObject = new UberCandidateKeyObject();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(DuplicateCandidateKeysXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(candidateKeyObject, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Line 61: <UberCandidateKeyObject>.<CandidateKeyObjectCollection>.<CandidateKeyObject>.<BooleanCandidateKey> - Invalid value [vasya]. Value must be a valid Boolean (true or false).
Warning - Line 74: <UberCandidateKeyObject>.<CandidateKeyObjectCollection> - Duplicate Candidate Key found, element <CandidateKeyObject> opened at line 58 was not imported. Candidate Key: [EnumCandidateKey='Key', BooleanCandidateKey='N', DateCandidateKey='', DecimalCandidateKey='21.245', IntCandidateKey='0', LongCandidateKey='0', MandatoryUNLOCOCandidateKey='AU', NonMandatoryUNLOCOCandidateKey='NZ', ShortCandidateKey='0', StringCandidateKey='', ZStringCandidateKey='']
Warning - Line 90: <UberCandidateKeyObject>.<CandidateKeyObjectCollection> - Duplicate Candidate Key found, element <CandidateKeyObject> opened at line 78 was not imported. Candidate Key: [EnumCandidateKey='Key', BooleanCandidateKey='N', DateCandidateKey='', DecimalCandidateKey='21.245', IntCandidateKey='0', LongCandidateKey='0', MandatoryUNLOCOCandidateKey='AU', NonMandatoryUNLOCOCandidateKey='NZ', ShortCandidateKey='0', StringCandidateKey='', ZStringCandidateKey='']
Warning - Line 140: <UberCandidateKeyObject>.<CandidateKeyObjectCollection> - Duplicate Candidate Key found, element <CandidateKeyObject> opened at line 127 was not imported. Candidate Key: [EnumCandidateKey='Candidate', BooleanCandidateKey='Y', DateCandidateKey='01-Feb-10 00:00:00', DecimalCandidateKey='3.0', IntCandidateKey='2', LongCandidateKey='0', MandatoryUNLOCOCandidateKey='AU', ShortCandidateKey='2', StringCandidateKey='lal|a^lala', ZStringCandidateKey='wow']
Warning - Line 154: <UberCandidateKeyObject>.<CandidateKeyObjectCollection> - Duplicate Candidate Key found, element <CandidateKeyObject> opened at line 141 was not imported. Candidate Key: [EnumCandidateKey='Candidate', BooleanCandidateKey='Y', DateCandidateKey='01-Feb-10 00:00:00', DecimalCandidateKey='3.0', IntCandidateKey='2', LongCandidateKey='0', MandatoryUNLOCOCandidateKey='AU', ShortCandidateKey='2', StringCandidateKey='lal|a^lala', ZStringCandidateKey='wow']
				".Trim(), logger.Logs);
				AssertEquals("Duplicate Elements are not imported", 6, candidateKeyObject.CandidateKeyObjectCollection.Count);
			}
		}

		#region DuplicateCandidateKeysXML

		const string DuplicateCandidateKeysXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<SoManyKeys xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <UberCandidateKeyObject Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>

    <EnumCandidateKey>Type</EnumCandidateKey>
    <MandatoryUNLOCOCandidateKey>
      <Code>AU</Code>
      <Name>Australia</Name>
    </MandatoryUNLOCOCandidateKey>

    <CandidateKeyObjectCollection>
      <CandidateKeyObject Action=""MERGE"">
        <IntCandidateKey></IntCandidateKey>
        <DecimalCandidateKey>1.0</DecimalCandidateKey>
        <StringCandidateKey>one</StringCandidateKey>
        <ZStringCandidateKey>two|three</ZStringCandidateKey>
        <BooleanCandidateKey>true</BooleanCandidateKey>
        <ShortCandidateKey></ShortCandidateKey>
        <EnumCandidateKey>Type</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>
      <CandidateKeyObject Action=""MERGE"">
        <IntCandidateKey></IntCandidateKey>
        <DecimalCandidateKey>1.0</DecimalCandidateKey>
        <StringCandidateKey>one|two</StringCandidateKey>
        <ZStringCandidateKey>three</ZStringCandidateKey>
        <BooleanCandidateKey>true</BooleanCandidateKey>
        <ShortCandidateKey></ShortCandidateKey>
        <EnumCandidateKey>Type</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>
      <CandidateKeyObject Action=""MERGE"">
        <IntCandidateKey></IntCandidateKey>
        <DecimalCandidateKey>21.245</DecimalCandidateKey>
        <StringCandidateKey></StringCandidateKey>
        <BooleanCandidateKey>false</BooleanCandidateKey>
        <ShortCandidateKey></ShortCandidateKey>
        <DateCandidateKey></DateCandidateKey>
        <EnumCandidateKey>Key</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
        <NonMandatoryUNLOCOCandidateKey>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </NonMandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>


      <CandidateKeyObject Action=""MERGE"">

        <DecimalCandidateKey>21.2450</DecimalCandidateKey>
        <BooleanCandidateKey>vasya</BooleanCandidateKey>
        <DateCandidateKey></DateCandidateKey>
        <EnumCandidateKey>Key</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
        <NonMandatoryUNLOCOCandidateKey>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </NonMandatoryUNLOCOCandidateKey>


      </CandidateKeyObject>



      <CandidateKeyObject Action=""MERGE"">
        <DecimalCandidateKey>21.24500</DecimalCandidateKey>
        <DateCandidateKey></DateCandidateKey>
        <EnumCandidateKey>Key</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
        <NonMandatoryUNLOCOCandidateKey>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </NonMandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>
      <CandidateKeyObject Action=""MERGE"">
        <DecimalCandidateKey>2.0</DecimalCandidateKey>
        <StringCandidateKey></StringCandidateKey>
        <ZStringCandidateKey>', ZStringCandidateKey='', ZStringCandidateKey='', ZStringCandidateKey='</ZStringCandidateKey>
        <DateCandidateKey></DateCandidateKey>
        <EnumCandidateKey>Key</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>
      <CandidateKeyObject Action=""MERGE"">
        <DecimalCandidateKey>2.0</DecimalCandidateKey>
        <StringCandidateKey>', ZStringCandidateKey='</StringCandidateKey>
        <ZStringCandidateKey>', ZStringCandidateKey='', ZStringCandidateKey='</ZStringCandidateKey>
        <DateCandidateKey></DateCandidateKey>
        <EnumCandidateKey>Key</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>
      <CandidateKeyObject Action=""MERGE"">
        <IntCandidateKey>2</IntCandidateKey>
        <DecimalCandidateKey>3.0</DecimalCandidateKey>
        <StringCandidateKey>lal|a^lala</StringCandidateKey>
        <ZStringCandidateKey>wow</ZStringCandidateKey>
        <BooleanCandidateKey>true</BooleanCandidateKey>
        <DateCandidateKey>2010-02-01T00:00:00</DateCandidateKey>
        <ShortCandidateKey>2</ShortCandidateKey>
        <EnumCandidateKey>Candidate</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>
      <CandidateKeyObject Action=""MERGE"">
        <IntCandidateKey>2</IntCandidateKey>
        <DecimalCandidateKey>3.0000000</DecimalCandidateKey>
        <StringCandidateKey>lal|a^lala</StringCandidateKey>
        <ZStringCandidateKey>wow</ZStringCandidateKey>
        <BooleanCandidateKey>true</BooleanCandidateKey>
        <DateCandidateKey>2010-02-01T00:00:00</DateCandidateKey>
        <ShortCandidateKey>2</ShortCandidateKey>
        <EnumCandidateKey>Candidate</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>
      <CandidateKeyObject Action=""MERGE"">
        <IntCandidateKey>2</IntCandidateKey>
        <DecimalCandidateKey>3</DecimalCandidateKey>
        <StringCandidateKey>lal|a^lala</StringCandidateKey>
        <ZStringCandidateKey>wow</ZStringCandidateKey>
        <BooleanCandidateKey>true</BooleanCandidateKey>
        <DateCandidateKey>2010-02-01T00:00:00</DateCandidateKey>
        <ShortCandidateKey>2</ShortCandidateKey>
        <EnumCandidateKey>Candidate</EnumCandidateKey>
        <MandatoryUNLOCOCandidateKey>
          <Code>AU</Code>
          <Name>Australia</Name>
        </MandatoryUNLOCOCandidateKey>
      </CandidateKeyObject>
    </CandidateKeyObjectCollection>
  </UberCandidateKeyObject>
</SoManyKeys>";

		#endregion

		const string EmptyCommentXML = @"<!---->
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
 <UberShipment Action=""MERGE"">
    <ReferenceData>MyReference</ReferenceData>
    <ZZZMandatoryField></ZZZMandatoryField>
    <CandidateKeyField>1001</CandidateKeyField>
    <OrganizationCollection>
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>
";
		public void TestEmptyComment()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(EmptyCommentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Comment found at Line 1 :-".Trim(), logger.Logs.Trim());
			}
		}

		public void TestCommentsAreOnlyAddedAsInformationToTheLogger()
		{
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(CommentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Comment found at Line 1 :- Hello
Information - Comment found at Line 2 :- Anything can be within comments
<AnythingAtAll>
<<<<<<<Anything!!>>>>>
Information - Comment found at Line 5 :- So
Many
Comments!
Information - Comment found at Line 9 :- Another one
Information - Comment found at Line 10 :- And
Another
Information - Comment found at Line 13 :- Gotta love comments
Information - Comment found at Line 14 :- They
are fun
Information - Comment found at Line 17 :- Comments make the world go round
Information - Comment found at Line 18 :- Round
and round
Information - Comment found at Line 23 :- Love
Information - Comment found at Line 24 :- No
Love
Information - Comment found at Line 29 :- Comments in unrecognized elements
Should be picked up
Information - Comment found at Line 32 :- No matter where
they are
Warning - Line 28: <UberShipment> - Unrecognized element <Dummy> found. Element and all its children skipped.
Information - Comment found at Line 37 :- Collections
Information - Comment found at Line 38 :- Should
Handle Comments
Information - Comment found at Line 46 :- What Comments After the XML?
Surely Not!
				".Trim(), logger.Logs);
			}
		}

		public void TestCommentWithInvalidXmlChar()
		{
			var shipment = new UberShipment();

			var xml = CommentXML.Replace("Hello", "Hello\0");
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertContains("logger.Logs", @"Error - Line 1: Hexadecimal value 0x00 is an invalid XML character.".Trim(), logger.Logs);
			}
		}

		#region CommentXML

		const string CommentXML = @"<!--Hello-->
<!--Anything can be within comments
<AnythingAtAll>
<<<<<<<Anything!!>>>>>-->
<!--So
Many
Comments!-->
<?xml version=""1.0"" encoding=""utf-8""?>
<!--Another one-->
<!--And
Another-->
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
<!--Gotta love comments-->
<!--They
are fun-->
  <UberShipment Action=""MERGE"">
<!--Comments make the world go round-->
<!--Round
and round-->
    <ReferenceData>MyReference</ReferenceData>

    <ZZZMandatoryField></ZZZMandatoryField>
<!--Love-->
<!--No
Love-->
    <CandidateKeyField>1001</CandidateKeyField>

    <Dummy Action=""MERGE"">
<!--Comments in unrecognized elements
Should be picked up-->
      <RandomField>random<RandomField>
<!--No matter where
they are-->
    </Dummy>

    <OrganizationCollection>
<!--Collections-->
<!--Should
Handle Comments-->
      <Organization Action=""MERGE"">
        <CompanyName>CargoWise</CompanyName>
      </Organization>
    </OrganizationCollection>
  </UberShipment>
</UbiquitousShipment>
<!--What Comments After the XML?
Surely Not!-->";

		#endregion

		public void TestReadingInSelfEndingElementInRelatedObjects()
		{
			{
				var transactionBatch = new DataObjects.Accounting.TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

				using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(MandatoryFieldsIsSelfEndingElementXML)))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(transactionBatch, stream, logger);
					AssertMultilineASCIIEquals("logger.Logs", string.Empty, logger.Logs);
					AssertEquals("transactionBatch.TransactionCollection.Count", 1, transactionBatch.TransactionCollection.Count);
					AssertEquals("transactionBatch.TransactionCollection.PostingJournalCollection.Count", 1, transactionBatch.TransactionCollection[0].PostingJournalCollection.Count);
				}
			}
		}

		#region MandatoryFieldsIsSelfEndingElementXML

		const string MandatoryFieldsIsSelfEndingElementXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<BatchType>
			<Code>TST</Code>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<PostingJournalCollection>
					<PostingJournal />
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

		#endregion

		#region Date Limits

		public void TestValidateXmlDateTime()
		{
			AssertXmlDateTime("Dates possibly < 1900 as UTC are invalid", null, "01-JAN-1900 23:59:59");
			AssertXmlDateTime("Dates definitely >= 1900 as UTC are valid", new ZDateTime(1900, 1, 2, 0, 0, 0), "02-JAN-1900 00:00:00");
			AssertXmlDateTime("Dates >= 7/6/2079 are invalid", null, "07-JUN-2079 00:00:00");
			AssertXmlDateTime("Dates < 7/6/2079 are valid", new ZDateTime(2079, 6, 6, 23, 59, 29), "06-JUN-2079 23:59:29");
		}

		public void TestValidateXmlDate()
		{
			AssertXmlDate("Dates possibly < 1900 as UTC are invalid", null, "01-JAN-1900");
			AssertXmlDate("Dates definitely >= 1900 as UTC are valid", new ZDate(1900, 1, 2), "02-JAN-1900");
			AssertXmlDate("Dates >= 7/6/2079 are invalid", null, "07-JUN-2079");
			AssertXmlDate("Dates < 7/6/2079 are valid", new ZDate(2079, 6, 6), "06-JUN-2079");
		}

		void AssertXmlDateTime(string message, ZDateTime? expectedReplacement, string dateTime)
		{
			var shipment = new UberShipment();
			var xmlMessageText = $@"
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
	<UberShipment>
		<ZZZMandatoryField>0</ZZZMandatoryField>
		<UberDateTime>{dateTime}</UberDateTime>
		<OrganizationCollection>
			<Organization>
				<Code>^OO</Code>
				<CompanyName>CargoWise</CompanyName>
				<Country>
					<Code>^A</Code>
					<Name>Australia</Name>
				</Country>
			</Organization>
		</OrganizationCollection>
	</UberShipment>
</UbiquitousShipment>";

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlMessageText)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals(message, expectedReplacement, shipment.UberDateTime);
			}
		}

		void AssertXmlDate(string message, ZDate? expectedReplacement, string date)
		{
			var shipment = new UberShipment();
			var xmlMessageText = $@"
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
	<UberShipment>
		<ZZZMandatoryField>0</ZZZMandatoryField>
		<UberDate>{date}</UberDate>
		<OrganizationCollection>
			<Organization>
				<Code>^OO</Code>
				<CompanyName>CargoWise</CompanyName>
				<Country>
					<Code>^A</Code>
					<Name>Australia</Name>
				</Country>
			</Organization>
		</OrganizationCollection>
	</UberShipment>
</UbiquitousShipment>";

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlMessageText)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				AssertEquals(message, expectedReplacement, shipment.UberDate);
			}
		}

		#endregion

		#region Strip White Spaces

		public void TestConditionOfRunningStripWhiteSpacesAndWarningLogs()
		{
			var addressType = $"{'\r'}ConsignorDocumentaryAddress";
			var organizationCode = $"{'\r'}WTG";
			var companyName = $"{'\r'}Wise Tech Global";
			var contact = $"{'\r'}Rope Team";
			var xmlMessageText = CreateXmlMessageForText(addressType, organizationCode, companyName, contact);
			var shipment = new UberShipment();

			CombineAssertions(() =>
			{
				Assert("PreConditon", ContainsWhiteSpaces(addressType));
				Assert("PreConditon", ContainsWhiteSpaces(organizationCode));
				Assert("PreConditon", ContainsWhiteSpaces(companyName));
				Assert("PreConditon", ContainsWhiteSpaces(contact));
			});

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlMessageText)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				var address = shipment.OrganizationAddressCollection.FirstOrDefault();
				CombineAssertions(() =>
				{
					AssertNotNull(address);
					AssertEquals("White spaces have been stripped because fields max length less than 200.", false, ContainsWhiteSpaces(address.AddressType));
					AssertEquals("White spaces have been stripped because fields max length less than 200.", false, ContainsWhiteSpaces(address.OrganizationCode));
					AssertEquals("White spaces have been stripped because 'AllowLineControlWhiteSpace' attribute is not applied.", false, ContainsWhiteSpaces(address.CompanyName));
					AssertEquals("White spaces have not been stripped because 'AllowLineControlWhiteSpace' attribute is applied.", true, ContainsWhiteSpaces(address.Contact));
					AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Line 17: <UberShipment>.<OrganizationAddressCollection>.<OrganizationAddress>.<AddressType>The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.
Warning - Line 19: <UberShipment>.<OrganizationAddressCollection>.<OrganizationAddress>.<OrganizationCode>The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.
Warning - Line 21: <UberShipment>.<OrganizationAddressCollection>.<OrganizationAddress>.<CompanyName>The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.".Trim(), logger.Logs);
				});
			}
		}

		public void TestResultAfterRunningStripWhiteSpaces()
		{
			var addressType = $@"{'\r'}Consignor
{'\n'}Documentary {'\t'}Address{'\n'}";
			var organizationCode = $@"{'\t'}W{'\r'}{'\r'}{'\t'}T{'\r'}{'\n'}{'\r'}{'\n'}G{'\t'}

{'\r'}{'\t'}";
			var companyName = $@"

{'\t'}
Wise
Tech {'\r'}{'\t'}{'\r'}{'\n'}{'\n'}Global

{'\r'}{'\r'}{'\t'}

";

			var contact = $"{'\r'}Rope {'\t'}Team{'\n'}";

			var xmlMessageText = CreateXmlMessageForText(addressType, organizationCode, companyName, contact);
			var shipment = new UberShipment();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlMessageText)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
				var address = shipment.OrganizationAddressCollection.FirstOrDefault();
				CombineAssertions(() =>
				{
					AssertNotNull(address);
					AssertEquals("Consignor Documentary Address", address.AddressType.ToString());
					AssertEquals("W T G", address.OrganizationCode.ToString());
					AssertEquals("Wise Tech Global", address.CompanyName.ToString());
					AssertEquals($"{'\r'}Rope {'\t'}Team{'\n'}", address.Contact.ToString());
				});
			}
		}

		bool ContainsWhiteSpaces(string text)
		{
			return text.Contains('\r') || text.Contains('\n') || text.Contains('\t');
		}

		string CreateXmlMessageForText(string addressType, string organizationCode, string companyName, string contact)
		{
			var xmlMessageText = $@"
<UbiquitousShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
	<UberShipment>
		<ZZZMandatoryField>0</ZZZMandatoryField>
		<OrganizationCollection>
			<Organization>
				<Code>^OO</Code>
				<CompanyName>CargoWise</CompanyName>
				<Country>
					<Code>^A</Code>
					<Name>Australia</Name>
				</Country>
			</Organization>
		</OrganizationCollection>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>{addressType}</AddressType>
				<OrganizationCode>{organizationCode}</OrganizationCode>
				<CompanyName>{companyName}</CompanyName>
				<Contact>{contact}</Contact>
			</OrganizationAddress>
		</OrganizationAddressCollection>
	</UberShipment>
</UbiquitousShipment>";

			return xmlMessageText;
		}

		#endregion

		#region Trim WhiteSpace

		public void TestTrimWhiteSpaceProperty()
		{
			var consolKey = "C123456789";

			AssertContextXmlMessage(consolKey, "TCNU7039484 ", "TCNU7039484");
			AssertContextXmlMessage(consolKey, "TCNU70	394 84", "TCNU70	394 84");
			AssertContextXmlMessage(consolKey, "TCNU70	394 84    ", "TCNU70	394 84");
			AssertContextXmlMessage(consolKey, "   TCNU7039484  ", "   TCNU7039484");
		}

		void AssertContextXmlMessage(string consolKey, string containerNumber, string expectedResult)
		{
			var ev = new Event();
			var xmlMessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
			</Company>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
					<Key>{consolKey}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2023-03-03T11:25:06</EventTime>
		<EventType>PUP</EventType>
		<EventReference>Non dichiarato &gt;A2&gt;P1/1680|SRC=ESP|LOC=ITCGB|TYP=FUL</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>{containerNumber}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlMessageText)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(ev, stream, logger);
				AssertEquals(expectedResult, ev.ContextCollection[0].Value);
			}
		}

		#endregion

		public void TestReadXml_CanReadValidCdata()
		{
			var universalEvent = new Event();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(ValidCdataXml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalEvent, stream, logger);
				Assert(!logger.HasErrors);
			}
		}

		public void TestReadXml_CanReadEmptyCdata()
		{
			var universalEvent = new Event();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(EmptyCdataXml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalEvent, stream, logger);
				Assert(!logger.HasErrors);
			}
		}

		public void TestReadXml_CanNotReadInvalidCdata()
		{
			var universalEvent = new Event();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(InvalidCdataXml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalEvent, stream, logger);
				Assert(logger.HasErrors);
				Assert(logger.Errors.Contains("Line 22: <Event>.<ContextCollection>.<Context>.<Type> - Unexpected closing CDATA tag ]] found within CDATA."));
			}
		}

		#region CdataXml

		const string ValidCdataXml = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
			</Company>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
					<Key>{consolKey}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2023-03-03T11:25:06</EventTime>
		<EventType>PUP</EventType>
		<EventReference>Non dichiarato &gt;A2&gt;P1/1680|SRC=ESP|LOC=ITCGB|TYP=FUL</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>CDATA</Type>
				<Value><![CDATA[THIS IS VALID CDATA $%^&*</]]></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string EmptyCdataXml = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
			</Company>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
					<Key>{consolKey}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2023-03-03T11:25:06</EventTime>
		<EventType>PUP</EventType>
		<EventReference>Non dichiarato &gt;A2&gt;P1/1680|SRC=ESP|LOC=ITCGB|TYP=FUL</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>CDATA</Type>
				<Value><![CDATA[]]></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string InvalidCdataXml = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<DataContext>
			<Company>
				<Code>EDI</Code>
			</Company>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
					<Key>{consolKey}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2023-03-03T11:25:06</EventTime>
		<EventType>PUP</EventType>
		<EventReference>Non dichiarato &gt;A2&gt;P1/1680|SRC=ESP|LOC=ITCGB|TYP=FUL</EventReference>
		<IsEstimate>false</IsEstimate>
		<ContextCollection>
			<Context>
				<Type>CDATA</Type>
				<Value><![CDATA[This is invalid nested <![CDATA[This is not valid]] and must throw an error.]]></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region Implementation

		void CreateOrgPatternMatchOverride(ZString foreignCode, ZString relationship, ZGuid localGuid)
		{
			CreateOrgPatternMatchOverrideInSource(Env.CurrentCompany.OrganisationPK, foreignCode, relationship, localGuid, "");
		}

		void CreateOrgPatternMatchOverride(ZString foreignCode, ZString relationship, ZString localCode)
		{
			CreateOrgPatternMatchOverrideInSource(Env.CurrentCompany.OrganisationPK, foreignCode, relationship, ZGuid.Empty, localCode);
		}

		void CreateOrgPatternMatchOverrideInSource(ZGuid codeMapSourcePK, ZString foreignCode, ZString relationship, ZGuid localGuid, ZString localCode)
		{
			var matchOverride = Factory.New<IOrgPatternMatchOverride>();
			matchOverride.OO_OH = codeMapSourcePK;
			matchOverride.OO_ForeignCode = foreignCode;
			matchOverride.OO_Relationship = relationship;
			if (localGuid != ZGuid.Empty)
			{
				matchOverride.OO_LocalGuid = localGuid;
			}

			if (!string.IsNullOrEmpty(localCode))
			{
				matchOverride.OO_LocalCode = localCode;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		#endregion
	}
}
