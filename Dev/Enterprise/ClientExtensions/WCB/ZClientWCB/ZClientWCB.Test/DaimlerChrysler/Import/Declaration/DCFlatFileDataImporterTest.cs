using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB.DaimlerChrysler.Testing
{
	sealed class DCFlatFileDataImporterTest : FlatFileDataImporterTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractToDataAdapter()
		{
			var anyDeclarationImported = Importer.ImportData(PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Declarations imported", true, anyDeclarationImported);
			var jobDecs = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery());
			AssertEquals("No of declarations imported", 1, jobDecs.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPacksToBond()
		{
			Importer.IsPackToBond = true;
			var jobDecs = Factory.Load<JobDeclaration>(new ZQuery());
			AssertEquals("No declarations imported", ZInt.Zero, jobDecs.Length);
			var anyDeclarationImported = Importer.ImportData(PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Declarations imported", true, anyDeclarationImported);
			jobDecs = Factory.Load<JobDeclaration>(new ZQuery());
			AssertEquals("No of declarations imported", 1, jobDecs.Length);
			var jobDec = jobDecs[0];
			AssertEquals("No of invoice headers", 2, jobDec.Invoices.Count);
			var invHead = jobDec.Invoices[0];
			Assert("Nature10PackCount should not be considered.", !invHead.JZ_AddInfo.Contains("PackCountForNature10_Hidden"));
			Assert("BondPackCount should be 1.", invHead.JZ_AddInfo.Contains("PackCountForBond_Hidden=1"));
			invHead = jobDec.Invoices[1];
			Assert("BondPackCount should be 1.", invHead.JZ_AddInfo.Contains("PackCountForBond_Hidden=1"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPacksForRelease()
		{
			Importer.IsPackToBond = false;
			var jobDecs = Factory.Load<JobDeclaration>(new ZQuery());
			AssertEquals("No declarations imported", ZInt.Zero, jobDecs.Length);
			var anyDeclarationImported = Importer.ImportData(PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Declarations imported", true, anyDeclarationImported);
			jobDecs = Factory.Load<JobDeclaration>(new ZQuery());
			AssertEquals("No of declarations imported", 1, jobDecs.Length);
			var jobDec = jobDecs[0];
			AssertEquals("No of invoice headers", 2, jobDec.Invoices.Count);
			var invHead = jobDec.Invoices[0];
			Assert("BondPackCount should not be considered.", !invHead.JZ_AddInfo.Contains("PackCountForBond_Hidden"));
			Assert("Nature10PackCount should be 1.", invHead.JZ_AddInfo.Contains("PackCountForNature10_Hidden=1"));
			invHead = jobDec.Invoices[1];
			Assert("Nature10PackCount should be 1.", invHead.JZ_AddInfo.Contains("PackCountForNature10_Hidden=1"));
		}

		public void TestIsMercedes()
		{
			DCFlatFileDataImporter importer = new DCFlatFileDataImporter(true, null);
			AssertEquals("IsMercedes", true, importer.IsMercedes);
			importer = new DCFlatFileDataImporter(false, null);
			AssertEquals("IsMercedes", false, importer.IsMercedes);
		}

		public void TestGetConsolidatedDeclarationValueObjects()
		{
			Xsd.ConsolAndShipmentCollection xsdDecs = new Xsd.ConsolAndShipmentCollection();
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.AIR, ZString.Empty, "QF11", "AUSYD", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.AIR, ZString.Empty, "QF11", "AUSYD", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.AIR, ZString.Empty, "QF22", "AUSYD", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.AIR, ZString.Empty, "QF22", "AUMEL", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.SEA, "AAA", "1111", "AUSYD", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.SEA, "AAA", "1111", "AUSYD", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.SEA, "BBB", "1111", "AUSYD", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.SEA, "AAA", "2222", "AUSYD", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.SEA, "AAA", "1111", "AUMEL", "8888"));
			xsdDecs.Add(SetupXsdDeclaration(Xsd.ConsolTransportMode.SEA, "BBB", "2222", "AUMEL", "8888"));
			AssertEquals("Number of declarations", 10, xsdDecs.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[0].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[0].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[1].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[1].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[2].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[2].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[3].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[3].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[4].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[4].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[5].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[5].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[6].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[6].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[7].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[7].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[8].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[8].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, xsdDecs[9].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, xsdDecs[9].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			Xsd.ConsolAndShipmentCollection consolidatedXsdDecs = Importer.GetConsolidatedDeclarationValueObjects(xsdDecs);
			AssertEquals("Number of consolidated declarations", 8, consolidatedXsdDecs.Count);
			AssertEquals("Number of invoice headers", 1, consolidatedXsdDecs[0].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 2, consolidatedXsdDecs[0].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, consolidatedXsdDecs[1].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, consolidatedXsdDecs[1].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, consolidatedXsdDecs[2].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, consolidatedXsdDecs[2].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, consolidatedXsdDecs[3].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 2, consolidatedXsdDecs[3].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, consolidatedXsdDecs[4].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, consolidatedXsdDecs[4].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, consolidatedXsdDecs[5].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, consolidatedXsdDecs[5].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, consolidatedXsdDecs[6].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, consolidatedXsdDecs[6].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
			AssertEquals("Number of invoice headers", 1, consolidatedXsdDecs[7].Consol.Shipments[0].Invoices.Count);
			AssertEquals("Number of invoice lines", 1, consolidatedXsdDecs[7].Consol.Shipments[0].Invoices[0].InvoiceLines.Count);
		}

		protected override string PathToTestFile
		{
			get
			{
				return BaseSourcePath + @"Enterprise\ClientExtensions\WCB\ZClientWCB\ZClientWCB.Test\DaimlerChrysler\Import\Declaration\TestFiles\SWTTOWCBSHIP20040623110030.txt";
			}
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			return new DCFlatFileDataImporterForTest();
		}

		static Xsd.ConsolAndShipment SetupXsdDeclaration(Xsd.ConsolTransportMode transportMode, string vessel, string voyageFlight, string destinationPort, string invoiceNumber)
		{
			Xsd.ConsolAndShipment result = new Xsd.ConsolAndShipment();
			result.Consol.ConsolDetail.TransportMode = transportMode;
			if (transportMode == Xsd.ConsolTransportMode.AIR)
			{
				Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
				flight.FlightNoJourneyNoTruckRegNo = voyageFlight;
				result.Consol.ConsolDetail.Item = flight;
			}
			else
			{
				Xsd.SailingWithVesselVoyage sailing = new Xsd.SailingWithVesselVoyage();
				sailing.VesselName = vessel;
				sailing.VoyageNo = voyageFlight;
				result.Consol.ConsolDetail.Item = sailing;
			}

			Xsd.Shipment xsdShipment = result.Consol.Shipments.AddNew();
			xsdShipment.ShipmentDetails.PortofDestination.Port.Value = destinationPort;
			Xsd.InvoiceHeader xsdInvHead = xsdShipment.Invoices.AddNew();
			xsdInvHead.InvoiceNumber = invoiceNumber;
			xsdInvHead.InvoiceLines.AddNew();
			return result;
		}

		DCFlatFileDataImporterForTest importer;
		DCFlatFileDataImporterForTest Importer => importer ?? (importer = new DCFlatFileDataImporterForTest());

		sealed class DCFlatFileDataImporterForTest : DCFlatFileDataImporter
		{
			public DCFlatFileDataImporterForTest() : base(true, null)
			{
			}

			protected override void SetPacksToBondAndPacksForRelease(JobDeclarationCollection jobDecs)
			{
				foreach (JobDeclaration jobDec in jobDecs)
				{
					foreach (JobComInvoiceHeader invHead in jobDec.Invoices)
					{
						foreach (JobComInvoiceLine invLine in invHead.JobComInvoiceLines)
						{
							invLine.JI_IsPackToBondForLine = IsPackToBond;
						}
					}
				}

				base.SetPacksToBondAndPacksForRelease(jobDecs);
			}

			internal new Xsd.ConsolAndShipmentCollection GetConsolidatedDeclarationValueObjects(Xsd.ConsolAndShipmentCollection xsdDecs)
			{
				return base.GetConsolidatedDeclarationValueObjects(xsdDecs);
			}

			public bool IsPackToBond;
		}
	}
}
