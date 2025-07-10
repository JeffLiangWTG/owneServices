using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.WCB.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB.DaimlerChrysler.Testing
{
	sealed class DCImportFlatFileConverterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		[TestDate(2007, 12, 10)]
		public void TestMapImport()
		{
			OrgHeader chryslerImporter = TestHelper.FindOrCreateOrgHeader("HARDIS");
			OrgHeader chryslerSupplier = TestHelper.FindOrCreateOrgHeader("NATMUT");
			OrgHeader mercedesImporter = TestHelper.FindOrCreateOrgHeader("JAYSCH");
			OrgHeader mercedesSupplier = TestHelper.FindOrCreateOrgHeader("MONDIE");
			TestHelper.SetValidRegistryChryslerImporter(chryslerImporter.PK);
			TestHelper.SetValidRegistryChryslerSupplier(chryslerSupplier.PK);
			TestHelper.SetValidRegistryMercedesImporter(mercedesImporter.PK);
			TestHelper.SetValidRegistryMercedesSupplier(mercedesSupplier.PK);
			FlatFileDataRowCollection sourceRows = SetSource(PathToTestFile);
			Xsd.ConsolAndShipmentCollection expectedXml = SetExpectedXml(mercedesImporter, mercedesSupplier);
			Xsd.ConsolAndShipmentCollection actualXml = SetActualXml(sourceRows, true);
			AssertXMLCorrect("Assert Mercedes Xml", expectedXml, actualXml);
			expectedXml = SetExpectedXml(chryslerImporter, chryslerSupplier);
			actualXml = SetActualXml(sourceRows, false);
			AssertXMLCorrect("Assert Mercedes Xml", expectedXml, actualXml);
		}

		static Xsd.ConsolAndShipmentCollection SetExpectedXml(OrgHeader importer, OrgHeader supplier)
		{
			Xsd.ConsolAndShipmentCollection xmlResult = new Xsd.ConsolAndShipmentCollection();
			Xsd.ConsolAndShipment xmlDec = xmlResult.AddNew();
			Xsd.SailingWithVesselVoyage vesselVoyage = new Xsd.SailingWithVesselVoyage();
			vesselVoyage.VesselName = "MANON";
			vesselVoyage.VoyageNo = "EE415";
			xmlDec.Consol.ConsolDetail.Item = vesselVoyage;
			Xsd.Shipment xmlShipment = xmlDec.Consol.Shipments.AddNew();
			Xsd.ShipmentIdentifier xmlShipmentIdentifier = xmlShipment.ShipmentIdentifier.AddNew();
			xmlShipmentIdentifier.Masterbill = "OBL1234567";
			xmlShipment.ShipmentDetails.Consignee.EDICode = importer.OH_Code;
			xmlShipment.ShipmentDetails.Consignor.EDICode = supplier.OH_Code;
			xmlShipment.ShipmentDetails.PortofDestination.Port.Value = "90130";
			Xsd.InvoiceHeader invoiceHeader = xmlShipment.Invoices.AddNew();
			invoiceHeader.InvoiceAmount.Value = 104042.00m;
			invoiceHeader.InvoiceNumber = "060";
			Xsd.InvoiceLine invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.PartAttrib1 = "1490100152";
			invoiceLine.ProductNumber = "93414122-AU1";
			invoiceLine.LinePrice.Value = 104042.00m;
			invoiceLine.OrderNumber = "037992";
			invoiceLine.InvoiceQty.Value = 1m;
			invoiceLine.InvoiceQty.DimensionType = "NO";
			// Second declaration...
			xmlDec = xmlResult.AddNew();
			vesselVoyage = new Xsd.SailingWithVesselVoyage();
			vesselVoyage.VesselName = "MANON";
			vesselVoyage.VoyageNo = "EE415";
			xmlDec.Consol.ConsolDetail.Item = vesselVoyage;
			xmlShipment = xmlDec.Consol.Shipments.AddNew();
			xmlShipmentIdentifier = xmlShipment.ShipmentIdentifier.AddNew();
			xmlShipment.ShipmentDetails.Consignee.EDICode = importer.OH_Code;
			xmlShipment.ShipmentDetails.Consignor.EDICode = supplier.OH_Code;
			xmlShipment.ShipmentDetails.PortofDestination.Port.Value = "90130";
			invoiceHeader = xmlShipment.Invoices.AddNew();
			invoiceHeader.InvoiceAmount.Value = 57730.00m;
			invoiceHeader.InvoiceNumber = "090";
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.PartAttrib1 = "1490150124";
			invoiceLine.ProductNumber = "97027822-AU1";
			invoiceLine.LinePrice.Value = 57730.00m;
			invoiceLine.OrderNumber = "038527";
			invoiceLine.InvoiceQty.Value = 1m;
			invoiceLine.InvoiceQty.DimensionType = "NO";
			return xmlResult;
		}

		Xsd.ConsolAndShipmentCollection SetActualXml(FlatFileDataRowCollection sourceRows, bool isMercedes)
		{
			Xsd.ConsolAndShipmentCollection xmlResult = new Xsd.ConsolAndShipmentCollection();
			ImportConverter(isMercedes).MapImport(xmlResult, sourceRows);
			return xmlResult;
		}

		static void AssertXMLCorrect(string message, Xsd.ConsolAndShipmentCollection expectedXml, Xsd.ConsolAndShipmentCollection actualXml)
		{
			AssertEquals(message + "ConsolAndShipmentCollection Count", expectedXml.Count, actualXml.Count);
			AssertEquals(message + "ConsolAndShipmentCollection Count", 2, actualXml.Count);
			AssertXMLCorrectCore(message, expectedXml[0], actualXml[0]);
			AssertXMLCorrectCore(message, expectedXml[1], actualXml[1]);
		}

		static void AssertXMLCorrectCore(string message, Xsd.ConsolAndShipment expectedXmlDec, Xsd.ConsolAndShipment actualXmlDec)
		{
			Xsd.SailingWithVesselVoyage expectedVesselVoyage = (Xsd.SailingWithVesselVoyage)expectedXmlDec.Consol.ConsolDetail.Item;
			Xsd.SailingWithVesselVoyage actualVesselVoyage = (Xsd.SailingWithVesselVoyage)actualXmlDec.Consol.ConsolDetail.Item;
			AssertEquals(message + "Vessel Name", expectedVesselVoyage.VesselName, actualVesselVoyage.VesselName);
			AssertEquals(message + "Voyage No", expectedVesselVoyage.VoyageNo, actualVesselVoyage.VoyageNo);
			AssertEquals(message + "Shipments.Count", expectedXmlDec.Consol.Shipments.Count, actualXmlDec.Consol.Shipments.Count);
			Xsd.Shipment expectedXmlShipment = expectedXmlDec.Consol.Shipments[0];
			Xsd.Shipment actualXmlShipment = actualXmlDec.Consol.Shipments[0];
			AssertEquals(message + "Ocean Bill", expectedXmlShipment.ShipmentIdentifier[0].Masterbill, actualXmlShipment.ShipmentIdentifier[0].Masterbill);
			AssertEquals(message + "Consignee", expectedXmlShipment.ShipmentDetails.Consignee.EDICode, actualXmlShipment.ShipmentDetails.Consignee.EDICode);
			AssertEquals(message + "Consignor", expectedXmlShipment.ShipmentDetails.Consignor.EDICode, actualXmlShipment.ShipmentDetails.Consignor.EDICode);
			AssertEquals(message + "Port Of Destination", expectedXmlShipment.ShipmentDetails.PortofDestination.Port.Value, actualXmlShipment.ShipmentDetails.PortofDestination.Port.Value);
			AssertEquals(message + "Invoices.Count", expectedXmlShipment.Invoices.Count, actualXmlShipment.Invoices.Count);
			Xsd.InvoiceHeader expectedXmlInvoiceHeader = expectedXmlShipment.Invoices[0];
			Xsd.InvoiceHeader actualXmlInvoiceHeader = actualXmlShipment.Invoices[0];
			AssertEquals(message + "Invoice Amount", expectedXmlInvoiceHeader.InvoiceAmount.Value, actualXmlInvoiceHeader.InvoiceAmount.Value);
			AssertEquals(message + "Invoice No", expectedXmlInvoiceHeader.InvoiceNumber, actualXmlInvoiceHeader.InvoiceNumber);
			AssertEquals(message + "InvoiceLines.Count", expectedXmlInvoiceHeader.InvoiceLines.Count, actualXmlInvoiceHeader.InvoiceLines.Count);
			Xsd.InvoiceLine actualXmlInvoiceLine = actualXmlInvoiceHeader.InvoiceLines[0];
			Xsd.InvoiceLine expectedXmlInvoiceLine = expectedXmlInvoiceHeader.InvoiceLines[0];
			AssertEquals(message + "Part Attrib 1", expectedXmlInvoiceLine.PartAttrib1, actualXmlInvoiceLine.PartAttrib1);
			AssertEquals(message + "Product No", expectedXmlInvoiceLine.ProductNumber, actualXmlInvoiceLine.ProductNumber);
			AssertEquals(message + "Line Price", expectedXmlInvoiceLine.LinePrice.Value, actualXmlInvoiceLine.LinePrice.Value);
			AssertEquals(message + "Order No", expectedXmlInvoiceLine.OrderNumber, actualXmlInvoiceLine.OrderNumber);
			AssertEquals(message + "Invoice Quantity", expectedXmlInvoiceLine.InvoiceQty.Value, actualXmlInvoiceLine.InvoiceQty.Value);
			AssertEquals(message + "Invoice Unit of Quantity", expectedXmlInvoiceLine.InvoiceQty.DimensionType, actualXmlInvoiceLine.InvoiceQty.DimensionType);
		}

		[ExpectException(typeof(WCBException))]
		public void TestInvalidFileFormat_NullRows()
		{
			ImportConverter(false).MapImport(XmlDeclarations, null);
		}

		[ExpectException(typeof(WCBException))]
		public void TestInvalidFileFormat_EmptyRows()
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			ImportConverter(false).MapImport(XmlDeclarations, dataRows);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectException(typeof(WCBException))]
		public void TestInvalidFileFormat_NoPortCode()
		{
			var dataRows = SetSource(PathToTestFile);
			var headerRow = dataRows[1] as DecInvoiceHeaderDataRow;
			headerRow.RegionalAllocation = ZString.Empty;
			ImportConverter(false).MapImport(XmlDeclarations, dataRows);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestValidFileFormat_NewHeaderAndFooterFormat()
		{
			var dataRows = SetSource(PathToTestFileNewFormat);
			ImportConverter(false).MapImport(XmlDeclarations, dataRows);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetUsePartAttrib1Flags()
		{
			RunSetUsePartAttrib1Flags(OrgPartRelation.RelationshipTypes.Owner);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetUsePartAttrib1FlagsWithBoth()
		{
			RunSetUsePartAttrib1Flags(OrgPartRelation.RelationshipTypes.Both);
		}

		void RunSetUsePartAttrib1Flags(string relationship)
		{
			OrgHeader importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, importer.PK));
			TestHelper.SetValidRegistryChryslerImporter(importer.PK);
			TestHelper.SetValidRegistryChryslerSupplier(supplier.PK);
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "123456789";
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = relationship;
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			AssertEquals("Flag should not be set", false, relation1.OU_UsePartAttrib1);
			AssertEquals("Flag should not be set", false, relation2.OU_UsePartAttrib1);
			FlatFileDataRowCollection dataRows = SetSource(PathToTestFile);
			((DecInvoiceLineDataRow)dataRows[2]).Model = part.OP_PartNum;
			Xsd.ConsolAndShipmentCollection xsdDecs = new Xsd.ConsolAndShipmentCollection();
			ImportConverter(false).MapImport(xsdDecs, dataRows);
			AssertEquals("Flag should be set for Owner or Both relationship", true, relation1.OU_UsePartAttrib1);
			AssertEquals("Flag should not be set for Supplier relationship", false, relation2.OU_UsePartAttrib1);
		}

		static FlatFileDataRowCollection SetSource(string testFilePath)
		{
			FlatFileDataRowCollection result = new FlatFileDataRowCollection();
			DCImportFlatFileFormat formatter = new DCImportFlatFileFormat();
			using (StreamReader dataReader = new StreamReader(testFilePath))
			{
				string line;
				while ((line = dataReader.ReadLine()) != null)
				{
					FlatFileDataRow dataRow = formatter.ConvertToRow(line);
					if (dataRow != null)
					{
						result.Add(dataRow);
					}
				}
			}

			return result;
		}

		static string PathToTestFile
		{
			get
			{
				return (BaseSourcePath + @"Enterprise\ClientExtensions\WCB\ZClientWCB\ZClientWCB.Test\DaimlerChrysler\Import\Declaration\TestFiles\SWTTOWCBSHIP20040623110030.txt");
			}
		}

		static string PathToTestFileNewFormat
		{
			get
			{
				return (BaseSourcePath + @"Enterprise\ClientExtensions\WCB\ZClientWCB\ZClientWCB.Test\DaimlerChrysler\Import\Declaration\TestFiles\SWTTOWCBSHIP20040623110030_UpdateHeaderFooter.txt");
			}
		}

		WCBTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new WCBTestHelper(Factory));
			}
		}

		Xsd.ConsolAndShipmentCollection XmlDeclarations
		{
			get
			{
				return xmlDeclarations ?? (xmlDeclarations = new Xsd.ConsolAndShipmentCollection());
			}
		}

		Xsd.ConsolAndShipmentCollection xmlDeclarations;
		WCBTestHelper testHelper;
		DCImportFlatFileConverterForTest ImportConverter(bool isMercedes)
		{
			if (isMercedes)
			{
				return mercedesImportConverter ?? (mercedesImportConverter = new DCImportFlatFileConverterForTest(new NotificationBuffer(), Factory, true));
			}
			else
			{
				return chryslerImportConverter ?? (chryslerImportConverter = new DCImportFlatFileConverterForTest(new NotificationBuffer(), Factory, false));
			}
		}

		DCImportFlatFileConverterForTest chryslerImportConverter;
		DCImportFlatFileConverterForTest mercedesImportConverter;
		class DCImportFlatFileConverterForTest : DCImportFlatFileConverter
		{
			public DCImportFlatFileConverterForTest(INotifications notification, BusinessObjectFactory factory, bool isMercedes) : base(notification, factory, isMercedes)
			{
			}

			public new void MapImport(IValueObject valueObject, FlatFileDataRowCollection rows)
			{
				base.MapImport(valueObject, rows);
			}
		}
	}
}
