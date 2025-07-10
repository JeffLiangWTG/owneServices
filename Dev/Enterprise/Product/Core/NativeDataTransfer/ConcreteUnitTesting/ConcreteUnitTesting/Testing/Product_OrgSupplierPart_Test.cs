using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class Product_OrgSupplierPart_Test : TestCaseWithFactory
	{
		public void TestNoteDeDuplicateWhenIsCustomNoteElementNotIncluded()
		{
			#region part xml

			const string part = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Header />
  <Body>
    <Product>
      <OrgSupplierPart Action=""MERGE"">
        <PartNum>PART1</PartNum>
        <StockKeepingUnit>UNT</StockKeepingUnit>
        <Desc>TEST PART 1</Desc>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ADESTE</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
        <StmNoteCollection>
          <StmNote Action=""MERGE"">
            <Description>MYNOTE</Description>
            <NoteText>CRAZY HORSE</NoteText>
            <NoteType>INT</NoteType>
            <NoteContext>AAA</NoteContext>
          </StmNote>
        </StmNoteCollection>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

			#endregion

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(part)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: PART1
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
StmNote - 1 inserts, 0 updates, 0 deletes";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals("Log Text on Add Part (Note should be Added)", expectedLog, logs);

			manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(part)))
			{
				manager.ImportService.Import(memoryStream);
			}

			expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: PART1
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
StmNote - 0 inserts, 0 updates, 0 deletes";

			logs = manager.GetLogs();
			AssertMultilineASCIIEquals("Log Text on Update Part (Note should not be Added again even though it has no 'IsCustomNote' element)", expectedLog, logs);
		}

		[TestDate(2020,1,1)]
		public void TestProduct_OrgSupplierPart_EdtEventOnChildUpdate()
		{
			var craig = Factory.New<OrgHeader>();
			craig.OH_Code = "CRAIMPCHI";
			Factory.Save();

			#region XML
			var xml = @"<?xml version='1.0' encoding='utf-8'?>
<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
   <Header>
      <OwnerCode>CW1DUSCHI</OwnerCode>
      <EnableCodeMapping>true</EnableCodeMapping>
   </Header>
   <Body>
      <Product version='2.0'>
         <OrgSupplierPart Action='MERGE'> 
            <PartNum>PRODUCTA_XYL</PartNum>
            <CusClassPartPivotCollection>
               <CusClassPartPivot Action='MERGE'> 
                  <TariffNum>0101100010</TariffNum>
                  <ChildType>HTI</ChildType>
                  <ChildListOrder>0</ChildListOrder>
                  <CusUSClassificationCollection>
                     <CusUSClassification Action='MERGE'>
                        <SPI>A</SPI>
                        <ProductClaim>F</ProductClaim>
                        <TaxApplicability>O</TaxApplicability>
                     </CusUSClassification>
                  </CusUSClassificationCollection>
                  <Country TableName='RefCountry'>
                     <Code>US</Code>
                  </Country>            
               </CusClassPartPivot>
            </CusClassPartPivotCollection>
            <OrgPartRelationCollection>
               <OrgPartRelation Action='MERGE'>
                  <Relationship>OWN</Relationship>            
                  <OrgHeader>
                     <Code>CRAIMPCHI</Code>
                     <PK>988dbe0b-d5c3-4d81-be4c-c3b19d97c52c</PK>
                  </OrgHeader>            
               </OrgPartRelation>
            </OrgPartRelationCollection>        
         </OrgSupplierPart>
      </Product>
   </Body>
</Native>";
			#endregion

			var manager = new ImportServiceManagerForTesting();
			var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
			manager.ImportService.Import(memoryStream);
			var logs = manager.GetLogs();
			AssertContains(@"--- Start Import Process --------------------------------------------------------------
Importing Product: PRODUCTA_XYL
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);
			var queryPartNumber = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "PRODUCTA_XYL");
			var supplierPart = new BusinessObjectFactory().LoadTop1<Enterprise.Customs.US.Business.OrgSupplierPart>(queryPartNumber);
			AssertEquals("Precondition: No EDT Event", 0, supplierPart.Logs.Find(l => l.SL_SE_NKEvent == "EDT").Count());

			TestDateAttribute.AddDays(1);
			manager = new ImportServiceManagerForTesting();
			memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml.Replace("<ProductClaim>F</ProductClaim>", "<ProductClaim>E</ProductClaim>")));
			manager.ImportService.Import(memoryStream);
			supplierPart = new BusinessObjectFactory().LoadTop1<Enterprise.Customs.US.Business.OrgSupplierPart>(queryPartNumber);
			logs = manager.GetLogs();
			AssertContains(@"--- Start Import Process --------------------------------------------------------------
Importing Product: PRODUCTA_XYL
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 1 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes", logs);

			AssertEquals("No EDT event created on child update", 0, supplierPart.Logs.Find(l => l.SL_SE_NKEvent == "EDT").Count());
			AssertEquals("Audit fields updated", ZDateTime.Now, supplierPart.OP_SystemLastEditTimeUtc);
		}

		public void TestNoteDeDuplicationDoesntPickUpNotesFromOtherProducts()
		{
			#region part1 and part2 xml

			const string part1 = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Header />
  <Body>
    <Product>
      <OrgSupplierPart Action=""MERGE"">
        <PartNum>PART1</PartNum>
        <StockKeepingUnit>UNT</StockKeepingUnit>
        <Desc>TEST PART 1</Desc>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ADESTE</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
        <StmNoteCollection>
          <StmNote Action=""MERGE"">
            <Description>MYNOTE</Description>
            <NoteText>CRAZY HORSE</NoteText>
            <IsCustomDescription>false</IsCustomDescription>
            <NoteType>INT</NoteType>
            <NoteContext>AAA</NoteContext>
          </StmNote>
        </StmNoteCollection>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

			const string part2 = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Header />
  <Body>
    <Product>
      <OrgSupplierPart Action=""MERGE"">
        <PartNum>PART2</PartNum>
        <StockKeepingUnit>UNT</StockKeepingUnit>
        <Desc>TEST PART 2</Desc>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ADESTE</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
        <StmNoteCollection>
          <StmNote Action=""MERGE"">
            <Description>MYNOTE</Description>
            <NoteText>CRAZY HORSE</NoteText>
            <IsCustomDescription>false</IsCustomDescription>
            <NoteType>INT</NoteType>
            <NoteContext>AAA</NoteContext>
          </StmNote>
        </StmNoteCollection>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

			#endregion

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(part1)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: PART1
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
StmNote - 1 inserts, 0 updates, 0 deletes";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals("Log Text on Add Part1 (Note should be Added)", expectedLog, logs);

			manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(part2)))
			{
				manager.ImportService.Import(memoryStream);
			}

			expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: PART2
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
StmNote - 1 inserts, 0 updates, 0 deletes";

			logs = manager.GetLogs();
			AssertMultilineASCIIEquals("Log Text on Add Part2 (Note should be Added as it's a different Product Code)", expectedLog, logs);
		}

		public void TestCheckFullyPopulatedAddressAndCheckIsValidAgainstUniversalSchema()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			var addressIrrelevant = Factory.NewWithValidTestData<OrgAddress>();
			addressIrrelevant.OA_OH = org.PK;
			org.OH_FullName = "Daniel Test";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = "123 Hight Street";
			address.OA_Address2 = "Subtown";
			address.OA_City = "London";
			address.OA_Email = "daniel@domain.com";
			address.OA_Phone = "0123456";
			address.OA_Fax = "is this still 1979?";
			address.OA_PostCode = "W123";
			address.OA_State = "We don't all live in Australia".Substring(address.OA_StateInfo.MaxLength);
			org.OH_Code = "DANTESTSYD";
			var cusCodeOrg = org.CustomsCodes.AddNew("IPR", "BBB", "GB");
			var cusCodeAddress = org.CustomsCodes.AddNew("CCP", "DDD");
			var cusCodeAddressIrrelevant = org.CustomsCodes.AddNew("CCP", "FFF");
			cusCodeAddress.OK_OA_PremisesAddress = address.PK;
			cusCodeAddressIrrelevant.OK_OA_PremisesAddress = addressIrrelevant.PK;
			Factory.Save();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var ou = product.RelatedOrganisations.AddNew();
			ou.OU_Relationship = "OWN";
			ou.OU_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Factory.Save();
			var productAsUsa = new BusinessObjectFactory().Load<Enterprise.Customs.US.Business.OrgSupplierPart>(product.PK);
			var usPivot = productAsUsa.PivotsForBinding.AddNew();

			productAsUsa.Factory.Save();
			var element = SerialiseToXml(product);
			var rawXml = element.ToString();
			var expectedFdaString = @"<AdditionalInformationChild Action=""MERGE"">
                <PK>{0}</PK>
                <Type>USA</Type>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>FDAShipperAddress</Key>
                    <Value>{1}</Value>
                    <OrganizationAddress>
                      <OrganizationCode>DANTESTSYD</OrganizationCode>
                      <Address1>123 Hight Street</Address1>
                      <Address2>Subtown</Address2>
                      <City>London</City>
                      <CompanyName>Daniel Test</CompanyName>
                      <Country>
                        <Code>AU</Code>
                        <Name>Australia</Name>
                      </Country>
                      <Email>daniel@domain.com</Email>
                      <Fax>is this still 1979?</Fax>
                      <Phone>0123456</Phone>
                      <Port>
                        <Code>AUSYD</Code>
                        <Name>Sydney</Name>
                      </Port>
                      <Postcode>W123</Postcode>
                      <State>ralia</State>
                      <AddressType>OFC</AddressType>
                      <AddressShortCode>123 Hight Street</AddressShortCode>
                      <RegistrationNumberCollection>
                        <RegistrationNumber>
                          <Type>
                            <Code>IPR</Code>
                            <Description>Inward Processing Relief Authorizat</Description>
                          </Type>
                          <CountryOfIssue>
                            <Code>GB</Code>
                            <Name>United Kingdom</Name>
                          </CountryOfIssue>
                          <Value>BBB</Value>
                        </RegistrationNumber>
                        <RegistrationNumber>
                          <Type>
                            <Code>CCP</Code>
                            <Description>Customs Controlled Premises Code</Description>
                          </Type>
                          <CountryOfIssue>
                            <Code>AU</Code>
                            <Name>Australia</Name>
                          </CountryOfIssue>
                          <Value>DDD</Value>
                        </RegistrationNumber>
                      </RegistrationNumberCollection>
                    </OrganizationAddress>
                  </AddInfo>
                  <AddInfo>
                    <Key>OFT</Key>
                    <Value>I</Value>
                  </AddInfo>
                </AddInfoCollection>
              </AdditionalInformationChild>";
			// Now validate address against UNIVERSAL schema
			var fdaElement = XElement.Parse(expectedFdaString);
			var addressInNewNamespace = fdaElement.XPathSelectElement("//OrganizationAddress");
			XNamespace universalNamespace = UniversalXmlInfo.Namespace_2011_11;
			foreach (XElement el in addressInNewNamespace.DescendantsAndSelf())
			{
				el.Name = universalNamespace + el.Name.LocalName;  // change to universal
			}
			var addressWithDuffContent = XElement.Parse(addressInNewNamespace.ToString().Replace("AddressType", "FakeNode"));
			var fullUniversalSchemaWhichDefinesAddressType = SchemaValidationTest.GenerateUniversalCommonSchemaForTesting();
			var simpleSchemaThatWantsOnlyAnAddress = @"<xs:schema 
																targetNamespace=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"" elementFormDefault=""qualified""
																xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
															<xs:element name=""OrganizationAddress"" type=""OrganizationAddress""/>		
														</xs:schema>";
			var simpleSchemaThatWantsOnlyAnAddressDocument = XDocument.Parse(simpleSchemaThatWantsOnlyAnAddress);
			string errorMessages = ValidateAddressAgainstSchema(addressInNewNamespace, fullUniversalSchemaWhichDefinesAddressType, simpleSchemaThatWantsOnlyAnAddressDocument);
			AssertEquals("The OrganizationAddress generated by Native should be valid against Universal's OrganizationAddress too.  if this test fails, you may have changed what is expected by UniversalCommon.xsd or may have changed what is output by Native. Check that the duplicate address definition for Native matches Universal, and that what is output by Native AddInfo matches both. Ask Daniel. ",
						"", errorMessages);
			errorMessages = ValidateAddressAgainstSchema(addressWithDuffContent, fullUniversalSchemaWhichDefinesAddressType, simpleSchemaThatWantsOnlyAnAddressDocument);
			AssertContains("has invalid child element 'FakeNode'", errorMessages);  // Check that we're not getting a false negative above - checks that duff XML results in an error reported. 
		}

		static string ValidateAddressAgainstSchema(XElement addressInNewNamespace, XDocument fullUniversalSchemaWhichDefinesAddressType, XDocument simpleSchemaThatWantsOnlyAnAddressDocument)
		{
			using (var commonSchemaReader = fullUniversalSchemaWhichDefinesAddressType.CreateReader())
			using (var simpleSchemaReader = simpleSchemaThatWantsOnlyAnAddressDocument.CreateReader())
			{
				var schemaSet = new XmlSchemaSet();
				schemaSet.Add(UniversalXmlInfo.Namespace_2011_11, commonSchemaReader);
				schemaSet.Add(UniversalXmlInfo.Namespace_2011_11, simpleSchemaReader);
				var addressDocument = new XDocument(addressInNewNamespace);
				string errorMessages = "";
				XmlReaderSettings xrs = new XmlReaderSettings();
				xrs.ValidationType = System.Xml.ValidationType.Schema;
				xrs.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
				xrs.Schemas = schemaSet;
				xrs.ValidationEventHandler += (o, s) =>
				{
					errorMessages += s.Message;
				};

				using (XmlReader xr = XmlReader.Create(addressDocument.CreateReader(), xrs))
				{
					while (xr.Read()) { }
				}
				return errorMessages;
			}
		}

		public void TestImportAndReimportUsRecordWatchingForReUseOfCusUSClassification()
		{
			var xml = @"<?xml version='1.0' encoding='utf-8'?>
<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='2.0'>
   <Header>
      <OwnerCode>CW1DUSCHI</OwnerCode>
      <EnableCodeMapping>true</EnableCodeMapping>
   </Header>
   <Body>
      <Product version='2.0'>
         <OrgSupplierPart Action='UPDATE'> 
            <PartNum>PRODUCTA_XYL</PartNum>
            <CusClassPartPivotCollection>
               <CusClassPartPivot Action='UPDATE'> 
                  <TariffNum>0101100010</TariffNum>
                  <ChildType>HTI</ChildType>
                  <ChildListOrder>0</ChildListOrder>
                  <CusUSClassificationCollection>
                     <CusUSClassification Action='UPDATE'>
                        <SPI>A</SPI>
                        <ProductClaim>F</ProductClaim>
                        <TaxApplicability>O</TaxApplicability>
                     </CusUSClassification>
                  </CusUSClassificationCollection>
                  <Country TableName='RefCountry'>
                     <Code>US</Code>
                  </Country>            
                  <AdditionalInformationChildCollection TableName='CusAddInfo'>
                     <AdditionalInformationChild Action = 'MERGE'>
                        <Type>PST</Type>
                        <AddInfoCollection>
                           <AddInfo>
                              <Key>UnregReasonRemarks</Key>
                              <Value>KEVIN</Value>
                           </AddInfo>
                        </AddInfoCollection>
                     </AdditionalInformationChild>
                  </AdditionalInformationChildCollection>
               </CusClassPartPivot>
            </CusClassPartPivotCollection>
            <OrgPartRelationCollection>
               <OrgPartRelation Action='UPDATE'>
                  <Relationship>OWN</Relationship>            
                  <OrgHeader>
                     <Code>CRAIMPCHI</Code>
                     <PK>988dbe0b-d5c3-4d81-be4c-c3b19d97c52c</PK>
                  </OrgHeader>            
               </OrgPartRelation>
            </OrgPartRelationCollection>        
         </OrgSupplierPart>
      </Product>
   </Body>
</Native>";
			var craig = Factory.New<OrgHeader>();
			craig.OH_Code = "CRAIMPCHI";
			Factory.Save();

			var manager = new ImportServiceManagerForTesting();
			var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml.Replace("Action='UPDATE'", "Action='MERGE'")));
			manager.ImportService.Import(memoryStream);
			var logs = manager.GetLogs();
			AssertContains(@"--- Start Import Process --------------------------------------------------------------
Importing Product: PRODUCTA_XYL
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 1 inserts, 0 updates, 0 deletes
AdditionalInformationChild - 1 inserts, 0 updates, 0 deletes
CusUSClassification - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes", logs);
			var queryPartNumber = new ZQuery(OrgSupplierPartSchema.OP_PartNum, "PRODUCTA_XYL");
			var usProduct = new BusinessObjectFactory().LoadTop1<Enterprise.Customs.US.Business.OrgSupplierPart>(queryPartNumber);
			var usPivot = usProduct.PivotsForBinding[0];
			usPivot.CD_SPI = "W";
			usPivot.CD_ProductClaim = "C";
			usPivot.CD_UC_NKCountryOfExport = "KR";
			usPivot.CD_UC_NKCountryOfOrigin = "KR";
			usPivot.CD_TaxApplicability = "";
			usPivot.Factory.Save();
			var usClassPK = usPivot.Details.PK;

			manager = new ImportServiceManagerForTesting();
			memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
			manager.ImportService.Import(memoryStream);
			usProduct = new BusinessObjectFactory().LoadTop1<Enterprise.Customs.US.Business.OrgSupplierPart>(queryPartNumber);
			usPivot = usProduct.PivotsForBinding[0];
			AssertEquals("KR", usPivot.CD_UC_NKCountryOfOrigin);
			AssertEquals("KR", usPivot.CD_UC_NKCountryOfExport);
			AssertEquals("A", usPivot.CD_SPI);
			AssertEquals("F", usPivot.CD_ProductClaim);
			AssertEquals("O", usPivot.CD_TaxApplicability);
			AssertEquals("CusUSClassification gets reused", usClassPK, usPivot.Details.PK);

			var newXml = xml.Replace("<SPI>A</SPI>", "<SPI></SPI>").Replace("<ProductClaim>F</ProductClaim>", "<ProductClaim>X</ProductClaim>").Replace("<TaxApplicability>O</TaxApplicability>", "<TaxApplicability></TaxApplicability>");
			manager = new ImportServiceManagerForTesting();
			memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(newXml));
			manager.ImportService.Import(memoryStream);
			usProduct = new BusinessObjectFactory().LoadTop1<Enterprise.Customs.US.Business.OrgSupplierPart>(queryPartNumber);
			usPivot = usProduct.PivotsForBinding[0];
			AssertEquals("KR", usPivot.CD_UC_NKCountryOfOrigin);
			AssertEquals("KR", usPivot.CD_UC_NKCountryOfExport);
			AssertEquals("", usPivot.CD_SPI);
			AssertEquals("X", usPivot.CD_ProductClaim);
			AssertEquals("", usPivot.CD_TaxApplicability);
			AssertEquals(usClassPK, usPivot.Details.PK);
		}

		public void TestImportWithExeedingMaxLengthLogError()
		{
			#region part xml

			const string part =
@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11'>
  <Header />
  <Body>
    <Product>
      <OrgSupplierPart Action='MERGE'>
        <PartNum>PART1</PartNum>
        <Desc>TEST PART 1</Desc>
        <OrgPartRelationCollection>
          <OrgPartRelation Action='MERGE'>
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ADESTE</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
      </OrgSupplierPart>
    </Product>
    <Product>
      <OrgSupplierPart Action='MERGE'> 
        <PartNum>PART2</PartNum>
        <Desc>TEST PART 2</Desc>
        <CusClassPartPivotCollection>
          <CusClassPartPivot Action='MERGE'> 
            <TariffNum>0101100010</TariffNum>
            <ChildType>HTI</ChildType>
            <CusUSClassificationCollection>
              <CusUSClassification Action='MERGE'>
                <SPI>A</SPI>
                <SecondarySPI>F</SecondarySPI>
                <TaxApplicability>O</TaxApplicability>
              </CusUSClassification>
            </CusUSClassificationCollection>
            <Country TableName='RefCountry'>
              <Code>AU</Code>
            </Country>
          </CusClassPartPivot>
        </CusClassPartPivotCollection>
        <OrgPartRelationCollection>
          <OrgPartRelation Action='MERGE'>
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ADESTE</Code>
            </OrgHeader>
           </OrgPartRelation>
        </OrgPartRelationCollection>
        <LastWeightedCostCurr TableName=""RefCurrency"">
			<Code>Australia</Code>
        </LastWeightedCostCurr>
	  </OrgSupplierPart>
    </Product>
    <Product>
      <OrgSupplierPart Action='MERGE'>
        <PartNum>PART3</PartNum>
        <Desc>TEST PART 3</Desc>
        <OrgPartRelationCollection>
          <OrgPartRelation Action='MERGE'>
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ADESTE</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

			#endregion

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(part)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: PART1
Processed: Product
Importing Product: PART2
Record: Product failed to Import:
Could not set <OrgSupplierPart>.<OP_RX_NKLastWeightedCostCurr> to [Australia] as the value exceeds the maximum length of 3 characters. Verify that you did not use a full name if code was expected.
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals("Log Text on Add Part (Note should be Added)", expectedLog, logs);
		}

		public void TestImportWithBadDecimal()
		{
			#region part xml

			const string part =
@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11'>
  <Header />
  <Body>
    <Product>
      <OrgSupplierPart Action=""MERGE"">
        <PartNum>PART1</PartNum>
        <StockKeepingUnit>UNT</StockKeepingUnit>
        <Desc>TEST PART 1</Desc>
        <Depth>1234567.1234</Depth>
        <Height>1234.1234</Height>
        <Width>1.23</Width>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ADESTE</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
        <StmNoteCollection>
          <StmNote Action=""MERGE"">
            <Description>MYNOTE</Description>
            <NoteText>CRAZY HORSE</NoteText>
            <NoteType>INT</NoteType>
            <NoteContext>AAA</NoteContext>
          </StmNote>
        </StmNoteCollection>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

			#endregion

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(part)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: PART1
Record: Product failed to Import:
[OrgSupplierPart.Depth] : Value was too large for a decimal type. Couldn't store <1234567.1234> in decimal column. Limit is 6 digits before the decimal point but 7 were provided.
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals("Log Text on Add Part (Note should be Added)", expectedLog, logs);
		}

		public void TestImportWithBadDecimalNoFraction()
		{
			#region part xml

			const string part1 =
@"<Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11'>
  <Header />
  <Body>
    <Product>
      <OrgSupplierPart Action=""MERGE"">
        <PK>6bcc3ebc-e56f-45b8-8743-50d860436f4a</PK>
        <PartNum>PART1</PartNum>
        <StockKeepingUnit>UNT</StockKeepingUnit>
        <Desc>TEST PART 1</Desc>
        <Depth>1.2</Depth>
        <Height>1234567</Height>
        <Width>1.4</Width>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <Relationship>OWN</Relationship>
            <OrgHeader>
              <Code>ADESTE</Code>
            </OrgHeader>
          </OrgPartRelation>
        </OrgPartRelationCollection>
        <StmNoteCollection>
          <StmNote Action=""MERGE"">
            <Description>MYNOTE</Description>
            <NoteText>CRAZY HORSE</NoteText>
            <NoteType>INT</NoteType>
            <NoteContext>AAA</NoteContext>
          </StmNote>
        </StmNoteCollection>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";
			#endregion

			var manager = new ImportServiceManagerForTesting();
			using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(part1)))
			{
				manager.ImportService.Import(memoryStream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: PART1
Record: Product failed to Import:
[OrgSupplierPart.Height] : Value was too large for a decimal type. Couldn't store <1234567> in decimal column. Limit is 6 digits before the decimal point but 7 were provided.
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals("Log Text on Add Part (Note should be Added)", expectedLog, logs);
		}

		public void TestImportProductXmlWithUsaAndEuAndNzPivots()
		{
			var manager = new ImportServiceManagerForTesting();
			var product = GetSourceProduct();
			var element = SerialiseToXml(product);
			var rawXml = element.ToString();
			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			query.ReLoadExistingRows = true;
			Factory.Load<Enterprise.Integration.Customs.Shared.ICusClassPartPivot>(query).Cast<BusinessObject>().DeleteAll();
			product.Delete();
			Factory.Save();
			var rawXmlInBytes = System.Text.Encoding.UTF8.GetBytes(rawXml);
			var memoryStream = new MemoryStream(rawXmlInBytes);
			manager.ImportService.Import(memoryStream);

			var queryPartNumber = new ZQuery(OrgSupplierPartSchema.OP_PartNum, productCodeFromXml);
			var usProduct = new BusinessObjectFactory().LoadTop1<Enterprise.Customs.US.Business.OrgSupplierPart>(queryPartNumber);
			var gbProduct = new BusinessObjectFactory().LoadTop1<Enterprise.Integration.Customs.GB.IOrgSupplierPart>(queryPartNumber);
			var nzProduct = new BusinessObjectFactory().LoadTop1<Enterprise.Integration.Customs.NZ.IOrgSupplierPart>(queryPartNumber);

			var usaPivot = usProduct.PivotsForBinding[0];
			usaPivot.Children.Sort(CusClassPartPivotSchema.CI_ChildListOrder.Name, System.ComponentModel.ListSortDirection.Ascending);
			var childA = ((ICusClassPartPivotCollection<Customs.US.Business.CusClassPartPivot>)usaPivot.Children).First();
			var usaChildB = ((ICusClassPartPivotCollection<Customs.US.Business.CusClassPartPivot>)usaPivot.Children).Last();
			AssertEquals("22222", childA.CI_TariffNum);
			AssertEquals("33333", usaChildB.CI_TariffNum);

			var packConv = usProduct.PartUnits[0];

			CombineAssertions(delegate
			{
				AssertEquals(usProduct.PK, gbProduct.PK);
				AssertEquals(usProduct.PK, nzProduct.PK);
				AssertEquals("6-BLC-BAG", string.Format("{0}-{1}-{2}", packConv.OF_QuantityInParent.ToZInt(), packConv.OF_PackType, packConv.OF_ParentPackType));
				AssertUsPivot(usaPivot, "P", true);
				AssertUsPivot(usaChildB, "C", false);

				AssertEquals("4000000", gbProduct.PivotsForBinding[0].CI_CPC);
				AssertEquals("A00", gbProduct.PivotsForBinding[0].Taxes[0].Data.G4_Type);
				AssertEquals("B00", gbProduct.PivotsForBinding[0].Taxes[1].Data.G4_Type);
				AssertEquals("C634", gbProduct.PivotsForBinding[0].SupportingDocuments[0].CSI_Code);
				AssertEquals("380", gbProduct.PivotsForBinding[0].PreviousDocuments[0].CSI_Code);
				AssertEquals("RPTID", gbProduct.PivotsForBinding[0].AdditionalInfos[0].CSI_Code);

				AssertEquals("0004a", usProduct.UNDGs[0].Substance.DG_Code);
				AssertEquals("2000", usProduct.UNDGs[1].Substance.DG_Code);
				AssertEquals("DANIEL", usProduct.UNDGs[0].DGContact.OC_ContactName);
				AssertEquals("DANIEL", usProduct.UNDGs[1].DGContact.OC_ContactName);

				AssertEquals("APA", nzProduct.PivotsForBinding[0].PermitCodes[0].ZO_Code);
				AssertEquals("AFL31", nzProduct.PivotsForBinding[0].PermitCodes[0].ZO_Data);
				AssertEquals("MEL", nzProduct.PivotsForBinding[0].PermitCodes[1].ZO_Code);
				AssertEquals("1105", nzProduct.PivotsForBinding[0].PermitCodes[1].ZO_Data);
				AssertEquals("ANT", nzProduct.PivotsForBinding[0].ProhibitedCodes[0].ZO_Code);
				AssertEquals("", nzProduct.PivotsForBinding[0].ProhibitedCodes[0].ZO_Data);
				AssertEquals("APC", nzProduct.PivotsForBinding[0].ProhibitedCodes[1].ZO_Code);
				AssertEquals("", nzProduct.PivotsForBinding[0].ProhibitedCodes[1].ZO_Data);
				AssertEquals("AWC", nzProduct.PivotsForBinding[0].OtherInfos[0].ZO_Code);
				AssertEquals("", nzProduct.PivotsForBinding[0].OtherInfos[0].ZO_Data);
				AssertEquals("COO", nzProduct.PivotsForBinding[0].OtherInfos[1].ZO_Code);
				AssertEquals("123456", nzProduct.PivotsForBinding[0].OtherInfos[1].ZO_Data);

				foreach (StmNote note in usProduct.Notes.GetAllNotes())
				{
					AssertEquals("I am a note", note.ST_NoteText);
					break;
				}
				var genAddOn = Factory.LoadTop1<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, usProduct.PK));
				AssertEquals("Smelly Dog", genAddOn.XV_Data);

				var logs = manager.GetLogs();

				string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: LIQUIDLACEY37
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
CusClassPartPivot - 3 inserts, 0 updates, 0 deletes
AdditionalInformationChild - 4 inserts, 0 updates, 0 deletes
CusSupportingInfo - 3 inserts, 0 updates, 0 deletes
ComponentCusClassPartPivot - 2 inserts, 0 updates, 0 deletes
CusUSClassification - 3 inserts, 0 updates, 0 deletes
AdditionalInformationGrandChild - 4 inserts, 0 updates, 0 deletes
AdditionalInformationGreatGrandChild - 8 inserts, 0 updates, 0 deletes
CusCodeDataCensus - 2 inserts, 0 updates, 0 deletes
CusAttributeFilter - 6 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
OrgPartUnit - 1 inserts, 0 updates, 0 deletes
UNDGDataItem - 2 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 2 inserts, 0 updates, 0 deletes
StmNote - 1 inserts, 0 updates, 0 deletes
GenCustomAddOnValue - 1 inserts, 0 updates, 0 deletes";

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, logs);
			});

			CombineAssertions("Second Import", () =>
			{
				memoryStream = new MemoryStream(rawXmlInBytes);
				manager.Logs.Clear();
				manager.ImportService.Import(memoryStream);
				var logs = manager.GetLogs();

				string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Importing Product: LIQUIDLACEY37
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
CusClassPartPivot - 0 inserts, 0 updates, 0 deletes
AdditionalInformationChild - 0 inserts, 0 updates, 0 deletes
CusSupportingInfo - 0 inserts, 0 updates, 0 deletes
ComponentCusClassPartPivot - 0 inserts, 0 updates, 0 deletes
CusUSClassification - 0 inserts, 0 updates, 0 deletes
AdditionalInformationGrandChild - 0 inserts, 0 updates, 0 deletes
AdditionalInformationGreatGrandChild - 0 inserts, 0 updates, 0 deletes
CusCodeDataCensus - 0 inserts, 0 updates, 0 deletes
CusAttributeFilter - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
OrgPartUnit - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 0 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 0 inserts, 0 updates, 0 deletes
StmNote - 0 inserts, 0 updates, 0 deletes
GenCustomAddOnValue - 0 inserts, 0 updates, 0 deletes";

				AssertMultilineASCIIEquals("Log Text on Update", expectedLog, logs);
			});
		}

		void AssertUsPivot(Customs.US.Business.CusClassPartPivot usaPivot, string suffix, bool expectAttributes)
		{
			AssertEquals("M" + suffix, usaPivot.CD_UC_NKCountryOfOrigin);
			AssertEquals("Lacey Underwear" + suffix, usaPivot.PGAs[0].US_PGACommercialDescription);
			AssertEquals("Apis Mellifera" + suffix, usaPivot.PGAs[0].PG04ConstituentElements[0].ScientificDataCollection[0].Data.US_PGAScientificSpeciesName);
			AssertEquals("Vespa Crabro" + suffix, usaPivot.PGAs[0].PG04ConstituentElements[0].ScientificDataCollection[1].Data.US_PGAScientificSpeciesName);
			AssertEquals("Vespula Vulgaris" + suffix, usaPivot.PGAs[0].PG04ConstituentElements[1].ScientificDataCollection[0].Data.US_PGAScientificSpeciesName);
			AssertEquals("Bombus Hypnorum" + suffix, usaPivot.PGAs[0].PG04ConstituentElements[1].ScientificDataCollection[1].Data.US_PGAScientificSpeciesName);

			AssertEquals("CENSUS ONE" + suffix, usaPivot.CensusWarningOverrides[0].CY_Data);

			AssertEquals("Jumper" + suffix, usaPivot.CD_WoolLicenceNo);
			AssertEquals("CD2" + suffix, usaPivot.PGAs[0].PG04ConstituentElements[1].Data.US_PGANameOfTheConstituentElement);
			if (expectAttributes)
			{
				AssertEquals("AT1.1", usaPivot.Attributes1[0].BG_AttributeValue1);
				AssertEquals("AT3.2", usaPivot.Attributes3[1].BG_AttributeValue1);
			}

			AssertEquals(ZString.Empty, usaPivot.Details.CD_RN_NKCastCountry);
			AssertEquals("SG", usaPivot.Details.CD_RN_NKPrimaryCountry);
			AssertEquals("US", usaPivot.Details.CD_RN_NKSecondaryCountry);
		}

		public void TestProductSchemaValidity()
		{
			var product = GetSourceProduct();
			SchemaValidationTest.CheckSchemaValidity(product, "Product");
		}

		public void TestExportProductToXml()
		{
			var product = GetSourceProduct();
			var element = SerialiseToXml(product);
			CombineAssertions(delegate
			{
				var rawString = element.ToString().ToUpper();
				AssertEquals("Should have USA Lacey details", true, rawString.Contains("UNDERWEAR"));
				AssertEquals("Should not have USA Lacey details repeated three times (parent)", 1, Regex.Matches(rawString, "UNDERWEARP").Count);
				AssertEquals("Should not have USA Lacey details repeated three times (component)", 1, Regex.Matches(rawString, "UNDERWEARC").Count);
				AssertEquals("Should have USA species details - Bee", true, rawString.Contains("APIS"));
				AssertEquals("Should have USA species details - Hornet", true, rawString.Contains("VESPA"));
				AssertEquals("Should have USA species details - Wasp", true, rawString.Contains("VESPULA"));
				AssertEquals("Should have USA species details - Bumble", true, rawString.Contains("BOMBUS"));
				AssertEquals("Should have USA classification details", true, rawString.Contains("JUMPER"));
				AssertEquals("Should have USA classification details", true, rawString.Contains("CD1"));
				AssertEquals("Should have USA attribute details", true, rawString.Contains("AT1.1"));
				AssertEquals("Should have USA attribute details", true, rawString.Contains("AT3.2"));
				AssertEquals("Should have EU general details", true, rawString.Contains(">4000000<"));
				AssertEquals("Should have EU tax details", true, rawString.Contains(">A00<"));
				AssertEquals("Should have EU PD details", true, rawString.Contains(">380<"));
				AssertEquals("Should have EU SD details", true, rawString.Contains(">C634<"));
				AssertEquals("Should have EU AI details", true, rawString.Contains(">RPTID<"));
				AssertEquals("Should have NZ PermitCodes details", true, rawString.Contains(">APA=AFL31^MEL=1105<"));
				AssertEquals("Should have NZ ProhibitedCodes details", true, rawString.Contains(">ANT^APC<"));
				AssertEquals("Should have NZ OtherInfos details", true, rawString.Contains(">AWC^COO=123456<"));
				AssertEquals("Should have pack conversion details", true, rawString.Contains("<PARENTPACKTYPE>BAG</PARENTPACKTYPE>"));
				AssertEquals("Should have only one OrgSupplierPart node - don't get one for each component pivot, and don't get one for OrgPartRelation either. One occurence means split into two halves.", 2, Regex.Split(rawString, "<ORGSUPPLIERPART").Length);
				AssertEquals("Should have ACTION='MERGE' on OrgPartRelation", true, rawString.Contains("<ORGPARTRELATION ACTION=\"MERGE\">"));
				AssertEquals("Should not have OrgPartRelation without action attribute", false, rawString.Contains("<ORGPARTRELATION>"));
			});
		}

		public void TestExportProductToXml_WithBOM()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ParentProduct";

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var ou1 = product.RelatedOrganisations.AddNew();
			ou1.OU_Relationship = "OWN";
			ou1.OU_OH = orgHeader.PK;

			var childProduct1 = Factory.New<OrgSupplierPart>();
			childProduct1.OP_PartNum = "BOM1";

			var ou2 = childProduct1.RelatedOrganisations.AddNew();
			ou2.OU_Relationship = "OWN";
			ou2.OU_OH = orgHeader.PK;

			var childProduct2 = Factory.New<OrgSupplierPart>();
			childProduct2.OP_PartNum = "BOM2";

			var ou3 = childProduct2.RelatedOrganisations.AddNew();
			ou3.OU_Relationship = "BTH";
			ou3.OU_OH = orgHeader.PK;

			var partBOM1 = Factory.New<OrgPartBOM>();
			partBOM1.OE_OP_MainProduct = product.PK;
			partBOM1.OE_OP_Component = childProduct1.PK;
			partBOM1.OE_ComponentQty = 10m;

			var partBOM2 = Factory.New<OrgPartBOM>();
			partBOM2.OE_OP_MainProduct = product.PK;
			partBOM2.OE_OP_Component = childProduct2.PK;
			partBOM2.OE_ComponentQty = 5m;

			Factory.Save();

			var element = SerialiseToXml(product);
			CombineAssertions(delegate
			{
				var rawString = element.ToString().ToUpper();
				AssertEquals("Should have included orgPartBOM collection", true, rawString.Contains("<ORGPARTBOMCOLLECTION>"));
				AssertEquals("Should have two partBOM parts. Two occurences means split into three parts.", 3, Regex.Split(rawString, "<ORGPARTBOM ACTION=\"MERGE\">").Length);
				AssertEquals("Should have two partBOM parts. Two occurences means split into three parts.", 3, Regex.Split(rawString, "<COMPONENT TABLENAME=\"ORGSUPPLIERPART\">").Length);
				AssertEquals("Should not have OrgPartBOM without action attribute", false, rawString.Contains("<ORGPARTBOM>"));

				AssertEquals("Should include the PK for BOM product", true, rawString.Contains($"<PK>{childProduct1.PK.ToString().ToUpper()}</PK>"));
				AssertEquals("Should include the qty for BOM product", true, rawString.Contains("<COMPONENTQTY>10.000</COMPONENTQTY>"));

				AssertEquals("Should include the PK for BOM product", true, rawString.Contains($"<PK>{childProduct2.PK.ToString().ToUpper()}</PK>"));
				AssertEquals("Should include the qty for BOM product", true, rawString.Contains("<COMPONENTQTY>5.000</COMPONENTQTY>"));
			});
		}

		public void TestExportProductToXml_WithBOM_DetailsComponentRelations()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ParentProduct";

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "ORG1";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "ORG2";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "ORG3";

			var ou1 = product.RelatedOrganisations.AddNew();
			ou1.OU_Relationship = "OWN";
			ou1.OU_OH = orgHeader1.PK;

			var childProduct1 = Factory.New<OrgSupplierPart>();
			childProduct1.OP_PartNum = "BOM1";

			var ou2_1 = childProduct1.RelatedOrganisations.AddNew();
			ou2_1.OU_Relationship = "OWN";
			ou2_1.OU_OH = orgHeader1.PK;

			var ou2_2 = childProduct1.RelatedOrganisations.AddNew();
			ou2_2.OU_Relationship = "SUP";
			ou2_2.OU_OH = orgHeader2.PK;

			var ou2_3 = childProduct1.RelatedOrganisations.AddNew();
			ou2_3.OU_Relationship = "BTH";
			ou2_3.OU_OH = orgHeader3.PK;

			var partBOM1 = Factory.New<OrgPartBOM>();
			partBOM1.OE_OP_MainProduct = product.PK;
			partBOM1.OE_OP_Component = childProduct1.PK;
			partBOM1.OE_ComponentQty = 10m;

			Factory.Save();

			var element = SerialiseToXml(product);
			CombineAssertions(delegate
			{
				var rawString = element.ToString().ToUpper();
				AssertEquals("Should have included Component", true, rawString.Contains("<COMPONENT TABLENAME=\"ORGSUPPLIERPART\">"));
				AssertEquals("Should have included Component PartNum", true, rawString.Contains("<PARTNUM>BOM1</PARTNUM>"));

				Assert(RawXMLContainsStringIgnoringCaseAndWhiteSpace(rawString,
					ExpectedXMLForComponentOrgPartRelation(ou2_1.PK, "OWN", orgHeader1.PK, orgHeader1.OH_Code)));

				Assert(RawXMLContainsStringIgnoringCaseAndWhiteSpace(rawString,
					ExpectedXMLForComponentOrgPartRelation(ou2_2.PK, "SUP", orgHeader2.PK, orgHeader2.OH_Code)));

				Assert(RawXMLContainsStringIgnoringCaseAndWhiteSpace(rawString,
					ExpectedXMLForComponentOrgPartRelation(ou2_3.PK, "BTH", orgHeader3.PK, orgHeader3.OH_Code)));
			});
		}

		string ExpectedXMLForComponentOrgPartRelation(ZGuid partRelationPK, string relationship, ZGuid orgHeaderPK, string orgHeaderCode)
		=> $@"<ComponentOrgPartRelation>
				<PK>{partRelationPK}</PK>
				<Relationship>{relationship}</Relationship>
				<OrgHeader>
					<Code>{orgHeaderCode}</Code>
					<PK>{orgHeaderPK}</PK>
				</OrgHeader>
			</ComponentOrgPartRelation>";

		public void TestExportProductToXml_WithBOM_ExcludesByProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ParentProduct";

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var ou1 = product.RelatedOrganisations.AddNew();
			ou1.OU_Relationship = "OWN";
			ou1.OU_OH = orgHeader.PK;

			var childProduct1 = Factory.New<OrgSupplierPart>();
			childProduct1.OP_PartNum = "BOM1";

			var ou2 = childProduct1.RelatedOrganisations.AddNew();
			ou2.OU_Relationship = "OWN";
			ou2.OU_OH = orgHeader.PK;

			var partBOM1 = Factory.New<OrgPartBOM>();
			partBOM1.OE_OP_MainProduct = product.PK;
			partBOM1.OE_OP_Component = childProduct1.PK;
			partBOM1.OE_ComponentQty = 10m;

			Factory.Save();

			var element = SerialiseToXml(product);
			CombineAssertions(delegate
			{
				var rawString = element.ToString().ToUpper();
				AssertEquals("Should have included orgPartBOM collection", true, rawString.Contains("<ORGPARTBOMCOLLECTION>"));
				AssertEquals("Should have single partBOM part. One occurence means split into two parts.", 2, Regex.Split(rawString, "<ORGPARTBOM ACTION=\"MERGE\">").Length);

				AssertEquals("Should not have OrgPartBOM.ByProduct", false, rawString.Contains("<BYPRODUCT TABLENAME=\"ORGSUPPLIERPART\""));
				AssertEquals("Should not have OrgPartBOM.ByProductPackType", false, rawString.Contains("<BYPRODUCTPACKTYPE TABLENAME=\"REFPACKTYPE"));
			});
		}

		bool RawXMLContainsStringIgnoringCaseAndWhiteSpace(string rawXML, string expectedString)
		{
			var normalisedRawString = Regex.Replace(rawXML.ToUpper(), @"\s", "");
			var normalisedExpectedString = Regex.Replace(expectedString.ToUpper(), @"\s", "");

			return normalisedRawString.Contains(normalisedExpectedString);
		}

		OrgSupplierPart GetSourceProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = productCodeFromXml;
			var unitConversion = product.PartUnits.AddNew();
			unitConversion.OF_ParentPackType = "BAG";
			unitConversion.OF_PackType = "BLC";
			unitConversion.OF_QuantityInParent = 6;
			var orgContact = Factory.LoadTop1<OrgContact>(new ZQuery());
			orgContact.OC_ContactName = "DANIEL";
			var undg1 = product.UNDGs.AddNew();
			var undg2 = product.UNDGs.AddNew();
			var ammoniumPicrate = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "A", "IMO").FirstOrDefault();
			var celluloid = UNDGSubstanceLoader.LoadSubstances(Factory, "2000", "", "IMO").FirstOrDefault();
			AssertNotNull("Your UNDG table is incomplete - celluloid is missing", celluloid);
			AssertNotNull("Your UNDG table is incomplete - ammonium picrate is missing", ammoniumPicrate);
			undg1.DI_DG = ammoniumPicrate.PK;
			undg2.DI_DG = celluloid.PK;
			undg1.DI_OC_DGContact = orgContact.PK;
			undg2.DI_OC_DGContact = orgContact.PK;

			var ou = product.RelatedOrganisations.AddNew();
			ou.OU_Relationship = "OWN";
			ou.OU_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Factory.Save();

			var productAsUsa = new BusinessObjectFactory().Load<Enterprise.Customs.US.Business.OrgSupplierPart>(product.PK);
			var productAsGb = new BusinessObjectFactory().Load<Enterprise.Integration.Customs.GB.IOrgSupplierPart>(product.PK);
			var productAsNZ = new BusinessObjectFactory().Load<Enterprise.Integration.Customs.NZ.IOrgSupplierPart>(product.PK);
			var usPivot = productAsUsa.PivotsForBinding.AddNew();
			var gbPivot = productAsGb.PivotsForBinding.AddNew();
			var nzPivot = productAsNZ.PivotsForBinding.AddNew();

			PopulateUsPivot(ou, usPivot, "P", true);  // P for parent

			var childA = usPivot.Children.AddNew();
			var usaChildB = usPivot.Children.AddNew();
			childA.CI_TariffNum = "22222";
			usaChildB.CI_TariffNum = "33333";
			childA.CI_ChildListOrder = 1;
			usaChildB.CI_ChildListOrder = 2;
			childA.Details.CD_RN_NKCastCountry = ZString.Empty;
			childA.Details.CD_RN_NKPrimaryCountry = "SG";
			childA.Details.CD_RN_NKSecondaryCountry = "US";
			usaChildB.Details.CD_RN_NKCastCountry = ZString.Empty;
			usaChildB.Details.CD_RN_NKPrimaryCountry = "SG";
			usaChildB.Details.CD_RN_NKSecondaryCountry = "US";
			PopulateUsPivot(null, usaChildB, "C", false);  // C for component
			PopulateGbPivot(productAsGb, gbPivot);
			PopulateNZPivot(nzPivot);

			productAsGb.Factory.Save();
			productAsUsa.Factory.Save();
			productAsNZ.Factory.Save();
			Factory.Save();
			return product;
		}

		void PopulateUsPivot(OrgPartRelation ou, Customs.US.Business.CusClassPartPivot usPivot, string suffixToDifferentiateParentAndComponent, bool addAttributesToo)
		{
			usPivot.CD_UC_NKCountryOfOrigin = "M" + suffixToDifferentiateParentAndComponent; // this gets data from the external reference database
			if (ou != null)
			{
				usPivot.CI_OH = ou.OU_OH;
			}

			var pga1 = usPivot.PGAs.AddNew();
			var constData1 = pga1.PG04ConstituentElements.AddNew();
			var constData2 = pga1.PG04ConstituentElements.AddNew();
			var scientificData11 = constData1.ScientificDataCollection.AddNew();
			var scientificData12 = constData1.ScientificDataCollection.AddNew();
			var scientificData21 = constData2.ScientificDataCollection.AddNew();
			var scientificData22 = constData2.ScientificDataCollection.AddNew();

			SetupAddressesIfNecessary();

			usPivot.CD_WoolLicenceNo = "Jumper" + suffixToDifferentiateParentAndComponent;
			pga1.Data.US_PGACommercialDescription = "Lacey Underwear" + suffixToDifferentiateParentAndComponent;
			constData1.Data.US_PGANameOfTheConstituentElement = "CD1" + suffixToDifferentiateParentAndComponent;
			constData2.Data.US_PGANameOfTheConstituentElement = "CD2" + suffixToDifferentiateParentAndComponent;
			scientificData11.Data.US_PGAScientificSpeciesName = "Apis Mellifera" + suffixToDifferentiateParentAndComponent;
			scientificData12.Data.US_PGAScientificSpeciesName = "Vespa Crabro" + suffixToDifferentiateParentAndComponent;
			scientificData21.Data.US_PGAScientificSpeciesName = "Vespula Vulgaris" + suffixToDifferentiateParentAndComponent;
			scientificData22.Data.US_PGAScientificSpeciesName = "Bombus Hypnorum" + suffixToDifferentiateParentAndComponent;

			if (addAttributesToo)
			{
				usPivot.Attributes1.AddNew().BG_AttributeValue1 = "AT1.1";
				usPivot.Attributes1.AddNew().BG_AttributeValue1 = "AT1.2";
				usPivot.Attributes2.AddNew().BG_AttributeValue1 = "AT2.1";
				usPivot.Attributes2.AddNew().BG_AttributeValue1 = "AT2.2";
				usPivot.Attributes3.AddNew().BG_AttributeValue1 = "AT3.1";
				usPivot.Attributes3.AddNew().BG_AttributeValue1 = "AT3.2";
			}

			var census1 = usPivot.CensusWarningOverrides.AddNew();
			census1.CY_Data = "CENSUS ONE" + suffixToDifferentiateParentAndComponent;

			usPivot.Details.CD_RN_NKCastCountry = ZString.Empty;
			usPivot.Details.CD_RN_NKPrimaryCountry = "SG";
			usPivot.Details.CD_RN_NKSecondaryCountry = "US";
		}

		void PopulateGbPivot(Enterprise.Integration.Customs.GB.IOrgSupplierPart productAsGb, Enterprise.Integration.Customs.GB.ICusClassPartPivot gbPivot)
		{
			var taxA00 = gbPivot.Taxes.AddNew().Data;
			var taxB00 = gbPivot.Taxes.AddNew().Data;
			var suppDoc = gbPivot.SupportingDocuments.AddNew();
			var addInfo = gbPivot.AdditionalInfos.AddNew();
			var prevDoc = gbPivot.PreviousDocuments.AddNew();
			taxA00.G4_Type = "A00";
			taxB00.G4_Type = "B00";
			suppDoc.CSI_Code = "C634";
			addInfo.CSI_Code = "RPTID";
			prevDoc.CSI_Code = "380";
			gbPivot.CI_CPC = "4000000";

			productAsGb.AddNote("I am a note", "PUB");

			var customField = Factory.New<GenCustomAddOnValue>();
			customField.XV_Name = "C11";
			customField.XV_Data = "Smelly Dog";
			customField.XV_Type = AddOnColumnDataType.Codes.String;
			customField.XV_ParentID = productAsGb.PK;
		}

		void PopulateNZPivot(Enterprise.Integration.Customs.NZ.ICusClassPartPivot nzPivot)
		{
			var permitCodeAPA = nzPivot.PermitCodes.AddNew();
			permitCodeAPA.ZO_Code = "APA";
			permitCodeAPA.ZO_Data = "AFL31";
			var permitCodeMEL = nzPivot.PermitCodes.AddNew();
			permitCodeMEL.ZO_Code = "MEL";
			permitCodeMEL.ZO_Data = "1105";

			var prohibitedCodeANT = nzPivot.ProhibitedCodes.AddNew();
			prohibitedCodeANT.ZO_Code = "ANT";
			var prohibitedCodeAPC = nzPivot.ProhibitedCodes.AddNew();
			prohibitedCodeAPC.ZO_Code = "APC";

			var otherInfoAWC = nzPivot.OtherInfos.AddNew();
			otherInfoAWC.ZO_Code = "AWC";
			var otherInfoCOO = nzPivot.OtherInfos.AddNew();
			otherInfoCOO.ZO_Code = "COO";
			otherInfoCOO.ZO_Data = "123456";
		}

		void SetupAddressesIfNecessary()
		{
			if (orgAddressFEI1 == null)
			{
				var oh1 = Factory.New<OrgHeader>();
				oh1.OH_Code = "OH1";
				oh1.MainAddress.Delete();
				orgAddressFEI1 = oh1.MainAddress;
				orgAddressFEI1.OA_Address1 = "One";
				SetupOrgCusCode(orgAddressFEI1, "1");
				var oh2 = Factory.New<OrgHeader>();
				oh2.OH_Code = "OH2";
				orgAddressManuf2 = oh2.Addresses.AddNew();
				orgAddressManuf2.OA_Address1 = "Two";
				SetupOrgCusCode(orgAddressManuf2, "2");
				var oh3 = Factory.New<OrgHeader>();
				oh3.OH_Code = "OH3";
				orgAddressShipper3 = oh3.Addresses.AddNew();
				orgAddressShipper3.OA_Address1 = "Three";
				SetupOrgCusCode(orgAddressShipper3, "3");
			}
		}

		void SetupOrgCusCode(OrgAddress orgAddress, string p)
		{
			var orgCusCodeA = Factory.New<OrgCusCode>();
			orgCusCodeA.OK_CodeType = "AA" + p;
			orgCusCodeA.OK_CustomsRegNo = "Address." + p;
			orgCusCodeA.OK_OA_PremisesAddress = orgAddress.PK;
			orgCusCodeA.OK_OH = orgAddress.Header.PK;
			var orgCusCodeB = Factory.New<OrgCusCode>();
			orgCusCodeB.OK_CodeType = "BB" + p;
			orgCusCodeB.OK_CustomsRegNo = "Org." + p;
			orgCusCodeB.OK_OH = orgAddress.Header.PK;
		}

		static XElement SerialiseToXml(OrgSupplierPart product)
		{
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			var actualMessage = "";
			using (var dataStream = xmlSerializer.SerializeToStream(product))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}
			var element = XElement.Load(new StringReader(actualMessage));
			return element;
		}

		const string productCodeFromXml = "LIQUIDLACEY37";
		OrgAddress orgAddressFEI1;
		OrgAddress orgAddressManuf2;
		OrgAddress orgAddressShipper3;
	}
}
