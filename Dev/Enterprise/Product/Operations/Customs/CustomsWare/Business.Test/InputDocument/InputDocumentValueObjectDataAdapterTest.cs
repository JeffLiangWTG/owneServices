using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common.EU;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(InputDocumentValueObjectDataAdapter))]
	class InputDocumentValueObjectDataAdapterTest : BaseInputDocumentValueObjectDataAdapterTest
	{
		[ExpectNoExceptions]
		public void TestInvalidWeightUnit()
		{
			Declaration.JE_TotalWeightUnit = "84";
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			var result = dataAdapter.ExportToValueObject(Declaration, context);
			XmlSerializer s = new XmlSerializer(typeof(XSD.InputDocument));
			TextWriter w = new StringWriter();
			s.Serialize(w, result);
			var actual = w.ToString();
			NUnit.Framework.Assert.That(actual, Does.Contain(@"        <Measure UOMCode=""DocumentGrossWeight"">
          <UOMValue>0</UOMValue>
        </Measure>"));
		}

		[TestDate(2012, 05, 08)]
		public void TestExport1()
		{
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			Declaration.JE_DeclarationReference = "4";
			Declaration.JE_RL_NKFinalDestination = "CNNJG";
			Declaration.JE_RL_NKPortOfArrival = "CNXYZ";
			var shipment = Factory.New<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			var port1 = Declaration.TransportsIncludingRelated.AddNew();
			port1.JW_RL_NKLoadPort = "AUSYD";
			port1.JW_RL_NKDiscPort = "AUBNE";
			var port2 = Declaration.TransportsIncludingRelated.AddNew();
			port2.JW_RL_NKLoadPort = "AUBNE";
			port2.JW_RL_NKDiscPort = "SGSIN";
			var port3 = Declaration.TransportsIncludingRelated.AddNew();
			port3.JW_RL_NKLoadPort = "SGSIN";
			port3.JW_RL_NKDiscPort = "CNSHA";
			var port4 = Declaration.TransportsIncludingRelated.AddNew();
			port4.JW_RL_NKLoadPort = "CNSHA";
			port4.JW_RL_NKDiscPort = "CNNJG";
			var result = dataAdapter.ExportToValueObject(Declaration, context);
			XmlSerializer s = new XmlSerializer(typeof(XSD.InputDocument));
			TextWriter w = new StringWriter();
			s.Serialize(w, result);
			var actual = w.ToString();
			const string expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<InputDocument xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.customsware.com/schema/api"">
  <Credentials />
  <ConsignmentList>
    <Consignment Command=""UPDATE"">
      <ConsignmentHeader>
        <ConsignmentReference>4</ConsignmentReference>
        <Reference RefCode=""HWB"">
          <RefText>HOUSE</RefText>
        </Reference>
        <Reference RefCode=""MWB"">
          <RefText>MASTER</RefText>
        </Reference>
        <Country CodeType=""ISO"" CountryType=""Dispatch"">US</Country>
        <Country CodeType=""ISO"" CountryType=""Destination"">CN</Country>
        <Port PortCountry=""US"" PortType=""ConsignmentOrigin"" CodeType=""UNLOC"">USLAX</Port>
        <Port PortCountry=""CN"" PortType=""ConsignmentDestination"" CodeType=""UNLOC"">CNNJG</Port>
        <Port PortCountry=""AU"" PortType=""FirstLoading"" CodeType=""UNLOC"">AUBNE</Port>
        <Port PortCountry=""US"" PortType=""LastLoading"" CodeType=""UNLOC"">USLAX</Port>
        <Port PortCountry=""CN"" PortType=""FirstArrival"" CodeType=""UNLOC"">CNSHA</Port>
        <Port PortCountry=""CN"" PortType=""Discharge"" CodeType=""UNLOC"">CNXYZ</Port>
        <Party PartyType=""Consignor"">
          <NameAddress>
            <Name>Supplier Full Name</Name>
            <Address1>Supplier Address1</Address1>
            <Address2>Supplier Address2</Address2>
            <Address3>Los Angeles</Address3>
            <PostCode>8485</PostCode>
            <Country CodeType=""ISO"" CountryType=""Consignor"">US</Country>
          </NameAddress>
          <AddressLocation />
        </Party>
        <Party PartyType=""Consignee"">
          <NameAddress>
            <Name>Importer Full Name</Name>
            <Address1>Importer Address1</Address1>
            <Address2>Importer Address2</Address2>
            <Address3>London</Address3>
            <PostCode>P3434</PostCode>
            <Country CodeType=""ISO"" CountryType=""Consignee"">GB</Country>
          </NameAddress>
          <AddressLocation />
        </Party>
        <GoodsDescription>GOODS DESCRIPTION</GoodsDescription>
        <Measure UOMCode=""DocumentPieces"">
          <UOMValue>326</UOMValue>
        </Measure>
        <Measure UOMCode=""DocumentGrossWeight"">
          <UOMValue>765235</UOMValue>
        </Measure>
        <ConsignmentDate>
          <DateTime>2012-05-08T00:00:00</DateTime>
        </ConsignmentDate>
        <BookIn />
        <Container>
          <ContainerItem>
            <ContainerType>20XJ</ContainerType>
            <ContainerRef>CONTAINER1</ContainerRef>
            <ContainerSize>20</ContainerSize>
            <ContainerSealNumber>SEAL</ContainerSealNumber>
          </ContainerItem>
        </Container>
        <Terms>
          <TermsCode>FOB</TermsCode>
        </Terms>
        <Transport TransportType=""Border"">
          <TPMode>1</TPMode>
        </Transport>
      </ConsignmentHeader>
    </Consignment>
  </ConsignmentList>
</InputDocument>";
			AssertXMLEquals("should match", expected, actual);
		}

		[TestDate(2014, 05, 06)]
		public void TestExportWhenElementEmpty()
		{
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			Declaration.JE_DeclarationReference = "5";
			var result = dataAdapter.ExportToValueObject(Declaration, context);
			XmlSerializer s = new XmlSerializer(typeof(XSD.InputDocument));
			TextWriter w = new StringWriter();
			s.Serialize(w, result);
			var actual = w.ToString();
			const string expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<InputDocument xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.customsware.com/schema/api"">
  <Credentials />
  <ConsignmentList>
    <Consignment Command=""UPDATE"">
      <ConsignmentHeader>
        <ConsignmentReference>5</ConsignmentReference>
        <Reference RefCode=""HWB"">
          <RefText>HOUSE</RefText>
        </Reference>
        <Reference RefCode=""MWB"">
          <RefText>MASTER</RefText>
        </Reference>
        <Country CodeType=""ISO"" CountryType=""Dispatch"">US</Country>
        <Country CodeType=""ISO"" CountryType=""Destination"">GB</Country>
        <Port PortCountry=""US"" PortType=""ConsignmentOrigin"" CodeType=""UNLOC"">USLAX</Port>
        <Port PortCountry=""GB"" PortType=""ConsignmentDestination"" CodeType=""UNLOC"">GBLON</Port>
        <Port PortCountry=""US"" PortType=""LastLoading"" CodeType=""UNLOC"">USLAX</Port>
        <Port PortCountry=""FR"" PortType=""Discharge"" CodeType=""UNLOC"">FRXYZ</Port>
        <Party PartyType=""Consignor"">
          <NameAddress>
            <Name>Supplier Full Name</Name>
            <Address1>Supplier Address1</Address1>
            <Address2>Supplier Address2</Address2>
            <Address3>Los Angeles</Address3>
            <PostCode>8485</PostCode>
            <Country CodeType=""ISO"" CountryType=""Consignor"">US</Country>
          </NameAddress>
          <AddressLocation />
        </Party>
        <Party PartyType=""Consignee"">
          <NameAddress>
            <Name>Importer Full Name</Name>
            <Address1>Importer Address1</Address1>
            <Address2>Importer Address2</Address2>
            <Address3>London</Address3>
            <PostCode>P3434</PostCode>
            <Country CodeType=""ISO"" CountryType=""Consignee"">GB</Country>
          </NameAddress>
          <AddressLocation />
        </Party>
        <GoodsDescription>GOODS DESCRIPTION</GoodsDescription>
        <Measure UOMCode=""DocumentPieces"">
          <UOMValue>326</UOMValue>
        </Measure>
        <Measure UOMCode=""DocumentGrossWeight"">
          <UOMValue>765235</UOMValue>
        </Measure>
        <ConsignmentDate>
          <DateTime>2014-05-06T00:00:00</DateTime>
        </ConsignmentDate>
        <BookIn />
        <Container>
          <ContainerItem>
            <ContainerType>20XJ</ContainerType>
            <ContainerRef>CONTAINER1</ContainerRef>
            <ContainerSize>20</ContainerSize>
            <ContainerSealNumber>SEAL</ContainerSealNumber>
          </ContainerItem>
        </Container>
        <Terms>
          <TermsCode>FOB</TermsCode>
        </Terms>
        <Transport TransportType=""Border"">
          <TPMode>1</TPMode>
        </Transport>
      </ConsignmentHeader>
    </Consignment>
  </ConsignmentList>
</InputDocument>";
			AssertXMLEquals("should match", expected, actual);
		}

		[TestDate(2014, 05, 06)]
		public void TestExport2()
		{
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			Declaration.JE_DeclarationReference = "4";
			Declaration.JE_RL_NKFinalDestination = "CNNJG";
			var port2 = Declaration.TransportsIncludingRelated.AddNew();
			port2.JW_RL_NKLoadPort = "AUBNE";
			port2.JW_RL_NKDiscPort = "SGSIN";
			var port3 = Declaration.TransportsIncludingRelated.AddNew();
			port3.JW_RL_NKLoadPort = "SGSIN";
			port3.JW_RL_NKDiscPort = "CNSHA";
			var port4 = Declaration.TransportsIncludingRelated.AddNew();
			port4.JW_RL_NKLoadPort = "CNSHA";
			port4.JW_RL_NKDiscPort = "CNNJG";
			var result = dataAdapter.ExportToValueObject(Declaration, context);
			XmlSerializer s = new XmlSerializer(typeof(XSD.InputDocument));
			TextWriter w = new StringWriter();
			s.Serialize(w, result);
			var actual = w.ToString();
			const string expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<InputDocument xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.customsware.com/schema/api"">
  <Credentials />
  <ConsignmentList>
    <Consignment Command=""UPDATE"">
      <ConsignmentHeader>
        <ConsignmentReference>4</ConsignmentReference>
        <Reference RefCode=""HWB"">
          <RefText>HOUSE</RefText>
        </Reference>
        <Reference RefCode=""MWB"">
          <RefText>MASTER</RefText>
        </Reference>
        <Country CodeType=""ISO"" CountryType=""Dispatch"">US</Country>
        <Country CodeType=""ISO"" CountryType=""Destination"">CN</Country>
        <Port PortCountry=""US"" PortType=""ConsignmentOrigin"" CodeType=""UNLOC"">USLAX</Port>
        <Port PortCountry=""CN"" PortType=""ConsignmentDestination"" CodeType=""UNLOC"">CNNJG</Port>
        <Port PortCountry=""AU"" PortType=""FirstLoading"" CodeType=""UNLOC"">AUBNE</Port>
        <Port PortCountry=""AU"" PortType=""LastLoading"" CodeType=""UNLOC"">AUBNE</Port>
        <Port PortCountry=""SG"" PortType=""FirstArrival"" CodeType=""UNLOC"">SGSIN</Port>
        <Port PortCountry=""SG"" PortType=""Discharge"" CodeType=""UNLOC"">SGSIN</Port>
        <Party PartyType=""Consignor"">
          <NameAddress>
            <Name>Supplier Full Name</Name>
            <Address1>Supplier Address1</Address1>
            <Address2>Supplier Address2</Address2>
            <Address3>Los Angeles</Address3>
            <PostCode>8485</PostCode>
            <Country CodeType=""ISO"" CountryType=""Consignor"">US</Country>
          </NameAddress>
          <AddressLocation />
        </Party>
        <Party PartyType=""Consignee"">
          <NameAddress>
            <Name>Importer Full Name</Name>
            <Address1>Importer Address1</Address1>
            <Address2>Importer Address2</Address2>
            <Address3>London</Address3>
            <PostCode>P3434</PostCode>
            <Country CodeType=""ISO"" CountryType=""Consignee"">GB</Country>
          </NameAddress>
          <AddressLocation />
        </Party>
        <GoodsDescription>GOODS DESCRIPTION</GoodsDescription>
        <Measure UOMCode=""DocumentPieces"">
          <UOMValue>326</UOMValue>
        </Measure>
        <Measure UOMCode=""DocumentGrossWeight"">
          <UOMValue>765235</UOMValue>
        </Measure>
        <ConsignmentDate>
          <DateTime>2014-05-06T00:00:00</DateTime>
        </ConsignmentDate>
        <BookIn />
        <Container>
          <ContainerItem>
            <ContainerType>20XJ</ContainerType>
            <ContainerRef>CONTAINER1</ContainerRef>
            <ContainerSize>20</ContainerSize>
            <ContainerSealNumber>SEAL</ContainerSealNumber>
          </ContainerItem>
        </Container>
        <Terms>
          <TermsCode>FOB</TermsCode>
        </Terms>
        <Transport TransportType=""Border"">
          <TPMode>1</TPMode>
        </Transport>
      </ConsignmentHeader>
    </Consignment>
  </ConsignmentList>
</InputDocument>";
			AssertXMLEquals("should match", expected, actual);
		}

		[ExpectNoExceptions]
		public void TestDispatchAndDestination()
		{
			var dataAdapter = InputDocumentValueObjectDataAdapter.New();
			var context = new ValueObjectExportContext(new NotificationBuffer());
			Declaration.JE_GoodsOrigin = "GB";
			Declaration.JE_GoodsDestination = "AU";
			var result = dataAdapter.ExportToValueObject(Declaration, context);
			XmlSerializer s = new XmlSerializer(typeof(XSD.InputDocument));
			TextWriter w = new StringWriter();
			s.Serialize(w, result);
			var actual = w.ToString();
			NUnit.Framework.Assert.That(Declaration.JE_RL_NKOrigin, Is.EqualTo("USLAX").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(Declaration.JE_RL_NKFinalDestination, Is.EqualTo("GBLON").Using(CustomComparers.TypeComparison));
			AssertXMLContains(@"<Country CodeType=""ISO"" CountryType=""Dispatch"">GB</Country>", actual);
			AssertXMLContains(@"<Country CodeType=""ISO"" CountryType=""Destination"">AU</Country>", actual);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportEvent()
		{
			Declaration.JE_DeclarationReference = "EventSample1";
			Factory.Save();
			DoImport("EventSample2.xml", Core.Constants.CountryCodes.France);
			NUnit.Framework.Assert.That(Declaration.ActiveEntryHeaders.Count, Is.EqualTo(0));
			var entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = @"EventSample1\001";
			var estimatedLog = entry.Logs.AddNew(AutoEvents.CustomsEntryStatus, ZDateTimeOffset.Now.AddHours(1), ZBool.True);
			DoImport("EventSample2.xml", Core.Constants.CountryCodes.France);
			var log = entry.Logs.MostRecentLogByEventTimeExcludingEstimated(AutoEvents.CustomsEntryStatus);
			NUnit.Framework.Assert.That(log.SL_Reference, Is.EqualTo("CLEARED").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entry.CH_EntryStatus, Is.EqualTo(CustomsWareEntryStatusList.Codes.Cleared).Using(CustomComparers.TypeComparison));
			Factory.Save();
			log = entry.Declaration.Logs.MostRecentLogByEventTimeExcludingEstimated(AutoEvents.CustomsCleared);
			NUnit.Framework.Assert.That(log, Is.Not.EqualTo(default(StmALog)));
			NUnit.Framework.Assert.That(entry.CH_EntryReleaseDate, Is.EqualTo(new ZDateTime(2011, 08, 18, 10, 09, 0)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestAttachment()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.GB.IJobDeclaration>();
			declaration.JE_DeclarationReference = "EventSample1";
			declaration.JE_GB = branchInOtherCountry.PK;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = @"AttachmentSample1\001";
			Declaration.JE_DeclarationReference = "AttachmentSample1";
			entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = @"AttachmentSample1\001";
			Factory.Save();
			//simulating two messages processed in one batch
			DoImport("AttachmentSample1.xml", Core.Constants.CountryCodes.France);
			DoImport("AttachmentSample1.xml", Core.Constants.CountryCodes.France);
			Factory.Save();
			Declaration.Reload();
			NUnit.Framework.Assert.That(Declaration.DocManagerInfo.AllEDocs.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(Declaration.DocManagerInfo.AllEDocs[0].Description, Is.EqualTo("Miscellaneous").Using(CustomComparers.TypeComparison));
			var factory2 = new BusinessObjectFactory();
			var declarationLoaded = factory2.Load<BaseJobDeclaration>(Declaration.PK);
			NUnit.Framework.Assert.That(declarationLoaded.DocManagerInfo.AllEDocs.Count, Is.EqualTo(2));
			var events = declarationLoaded.Logs.GetAllLogs().OfType<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.DocumentAllocated.Code);
			NUnit.Framework.Assert.That(events.Count(), Is.EqualTo(2));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestImportEvent_CheckSubmittedDate()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.GB.IJobDeclaration>();
			declaration.JE_DeclarationReference = "EventSample3";
			declaration.JE_GB = branchInOtherCountry.PK;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = @"EventSample3\001";
			Declaration.JE_DeclarationReference = "EventSample3";
			Factory.Save();
			entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = @"EventSample3\001";
			DoImport("EventSample3.xml", Core.Constants.CountryCodes.Belgium);
			NUnit.Framework.Assert.That(entry.CH_EntrySubmittedDate, Is.EqualTo(new ZDateTime(2010, 08, 18, 10, 09, 0)));
		}
	}
}
