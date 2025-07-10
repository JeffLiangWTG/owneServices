using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.MFI.CaroTrans;
using Enterprise.Client.MFI.CaroTrans.Export;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.Test.CaroTrans.Export
{
	public class CaroTransFileConverterTest : TestCaseWithFactory
	{
		public void TestGetMinNumberOfBillBodies()
		{
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			string marksAndNumbers = "This string is going to be 3x14 chars long";
			string description = "This string is <30 chars long";
			AssertEquals("Should work out to be 3 bill body records", 3, converter.GetMinNumberOfBillBodies(marksAndNumbers, description));
			description = "This string is more than 30 characters long";
			AssertEquals("Should still be 3 bill body records - marks and numbers is longer", 3, converter.GetMinNumberOfBillBodies(marksAndNumbers, description));
			description = "This is a really long string that will end up being more than 90 characters long so that we have more than 3 x description fields required";
			AssertEquals("Should be 5 bill body records now", 5, converter.GetMinNumberOfBillBodies(marksAndNumbers, description));
		}

		public void TestGetShippingTerms()
		{
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			AssertEquals("ExWorks should become prepaid", "P", converter.GetShippingTerms(Core.Constants.IncoTerms.ExWorks));
			AssertEquals("Free On board should become prepaid", "P", converter.GetShippingTerms(Core.Constants.IncoTerms.FreeOnBoard));
			AssertEquals("Anything else becomes Collect", "C", converter.GetShippingTerms(Core.Constants.IncoTerms.DeliveredDutyPaid));
			AssertEquals("Anything else becomes Collect", "C", converter.GetShippingTerms(Core.Constants.IncoTerms.DeliveredDutyUnpaid));
		}

		public void TestGetPhoneNumberFromAddress()
		{
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			Xsd.OrgAddress address = new Xsd.OrgAddress();
			address.TelephoneNumbers = new Xsd.TelephoneNumberCollection();
			Xsd.TelephoneNumber faxNumber = address.TelephoneNumbers.AddNew();
			faxNumber.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			faxNumber.Value = "12345678";
			Xsd.TelephoneNumber telephone = address.TelephoneNumbers.AddNew();
			telephone.NumberType = Xsd.TelephoneNumberNumberType.Business;
			telephone.Value = "87654321";
			AssertEquals("fax number should be extracted correctly", "12345678", converter.GetPhoneNumberFromAddress(address, Xsd.TelephoneNumberNumberType.Fax));
			AssertEquals("phone number should be extracted correctly", "87654321", converter.GetPhoneNumberFromAddress(address, Xsd.TelephoneNumberNumberType.Business));
		}

		public void TestGetMasterbillFromConsol()
		{
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			AssertEquals("Masterbill should be extracted correctly", "12345", converter.GetMasterbillFromConsol(ConsolXSD.ConsolIdentifier));
			Consol.JK_MasterBillNum = "67890";
			ForwardingConsolValueObjectDataAdapter adapter = new ForwardingConsolValueObjectDataAdapter();
			ConsolXSD = adapter.ExportToValueObject(Consol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Masterbill should be extracted correctly", "67890", converter.GetMasterbillFromConsol(ConsolXSD.ConsolIdentifier));
		}

		public void TestGetHousebillFromShipment()
		{
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			AssertEquals("Housebill should be extracted correctly", "11111", converter.GetHousebillFromShipment(ConsolXSD.Shipments[0].ShipmentIdentifier));
			AssertEquals("Housebill should be extracted correctly", "22222", converter.GetHousebillFromShipment(ConsolXSD.Shipments[1].ShipmentIdentifier));
			AssertEquals("Housebill should be extracted correctly", "33333", converter.GetHousebillFromShipment(ConsolXSD.Shipments[2].ShipmentIdentifier));
		}

		public void TestGetWeightFromPackages()
		{
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			PackLineValueObjectDataAdapter<PackLine, Xsd.Package> adapter = new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>();
			Xsd.PackageCollection packages = new Xsd.PackageCollection();
			foreach (CommonShipment shipment in Consol.Shipments)
			{
				foreach (PackLine packLine in shipment.OuterPackLines)
				{
					packages.Add(adapter.ExportToValueObject(packLine, null));
				}
			}

			AssertEquals("Total weight of all packlines in all shipments on this consol", (decimal)31, converter.GetWeightInKGFromPackages(packages));
			packages = new Xsd.PackageCollection();
			foreach (PackLine packLine in Consol.Shipments[0].OuterPackLines)
			{
				packages.Add(adapter.ExportToValueObject(packLine, null));
			}

			AssertEquals("total weight of all packlines in just the first shipment", (decimal)15, converter.GetWeightInKGFromPackages(packages));
		}

		public void TestGetVolumeInM3FromPackages()
		{
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			PackLineValueObjectDataAdapter<PackLine, Xsd.Package> adapter = new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>();
			Xsd.PackageCollection packages = new Xsd.PackageCollection();
			foreach (CommonShipment shipment in Consol.Shipments)
			{
				foreach (PackLine packLine in shipment.OuterPackLines)
				{
					packages.Add(adapter.ExportToValueObject(packLine, null));
				}
			}

			AssertEquals("Total volume of all packlines in all shipments on this consol", (decimal)21, converter.GetVolumeInM3FromPackages(packages));
			packages = new Xsd.PackageCollection();
			foreach (PackLine packLine in Consol.Shipments[0].OuterPackLines)
			{
				packages.Add(adapter.ExportToValueObject(packLine, null));
			}

			AssertEquals("total volume of all packlines in just the first shipment", (decimal)5, converter.GetVolumeInM3FromPackages(packages));
			packages = new Xsd.PackageCollection();
			foreach (PackLine packLine in Consol.Shipments[0].OuterPackLines)
			{
				packages.Add(adapter.ExportToValueObject(packLine, null));
			}

			packages[0].Volume.Value = 2.3456M;
			packages[1].Volume.Value = 1.2825M;
			AssertEquals("total volume of all packlines in just the first shipment, roudned to 3 decimal places", (decimal)3.628, converter.GetVolumeInM3FromPackages(packages));
		}

		public void TestPackageWeightAndVolumeIsOutputToTheFirst03Record()
		{
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			PackLineValueObjectDataAdapter<PackLine, Xsd.Package> adapter = new PackLineValueObjectDataAdapter<PackLine, Xsd.Package>();
			Xsd.Shipment shipment = ConsolXSD.Shipments[0];
			Xsd.Package package = shipment.ShipmentDetails.Packages.AddNew();
			package.NumberOfPacks = 8;
			converter.MapBillBodyForBill(0, ConsolXSD, ConsolXSD.ConsolDetail.Containers[0], shipment);
			AssertEquals("1 Row should be exported", 1, converter.RowsForExport.Count);
			FlatFileDataRow row = converter.RowsForExport[0];
			AssertEquals("PackageCount", "7", row[Constants.BillBodyRecord.Packages]);
			AssertEquals("Weight", "10", row[Constants.BillBodyRecord.Weight]);
			AssertEquals("Volume", "3", row[Constants.BillBodyRecord.CBM]);
			converter.RowsForExport.Clear();
			converter.MapBillBodyForBill(1, ConsolXSD, ConsolXSD.ConsolDetail.Containers[0], ConsolXSD.Shipments[0]);
			AssertEquals("1 Row should be exported", 1, converter.RowsForExport.Count);
			row = converter.RowsForExport[0];
			AssertEquals("PackageCount should be 0", "0", row[Constants.BillBodyRecord.Packages]);
			AssertEquals("Weight should be 0", "0", row[Constants.BillBodyRecord.Weight]);
			AssertEquals("Volume should be 0", "0", row[Constants.BillBodyRecord.CBM]);
		}

		public void TestExportPopulatedConsol()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ADMIRALENGRACHT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "Voyage123";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "USLAX";
			origin.JA_E_DEP = new ZDateTime(2005, 04, 01);
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = new ZDateTime(2005, 04, 10);
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			ForwardingConsol populatedConsol = Factory.New<ForwardingConsol>();
			populatedConsol.JK_RL_NKLoadPort = "USLAX";
			populatedConsol.JK_RL_NKDischargePort = "AUSYD";
			Transport populatedTransport = populatedConsol.Transports[0];
			populatedTransport.JW_JX = sailing.PK;
			populatedConsol.AutomaticallyUpdatePackLineContainers = false;
			populatedConsol.JK_TransportMode = "SEA";
			populatedConsol.JK_UniqueConsignRef = "C00001002";
			populatedConsol.JK_MasterBillNum = "12345";
			populatedConsol.SetDefaultSendingForwarderAddress(Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS")));
			populatedConsol.SetDefaultReceivingForwarderAddress(Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BARGAL")));
			CommonContainer container = populatedConsol.Containers.AddNew();
			container.JC_ContainerNum = "ABCD1111";
			container.JC_RC = Factory.LoadTop1(typeof(RefContainer), new ZQuery(RefContainerSchema.RC_Code, "40FR")).PK;
			container.JC_ContainerMode = "SEA";
			CommonShipment shipment1 = populatedConsol.Shipments.AddNew();
			shipment1.ConsigneePK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "BARGAL")).PK;
			shipment1.ConsignorPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS")).PK;
			shipment1.JS_HouseBill = "11111";
			shipment1.JS_MarksAndNumbers = "This\r\nis a rea😃lly\r\nlong marks and\r\nnumbers field to see if we can\r\nget multiple\r\nbill body records\r\n😃😃😃 😃";
			shipment1.JS_GoodsDescription = "a description of some\r\n bad go😃0d"; // packline created automatically
			shipment1.JS_UnitFreightRate = new ZDecimal(0.74);
			shipment1.JS_RX_NKFrtRateCurrency = "USD";
			shipment1.JS_INCO = "FOB";
			shipment1.NotifyPartyDocumentaryAddress.OrganisationPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "CATPAC")).PK;
			PackLine line = (shipment1.OuterPackLines.Count > 0) ? shipment1.OuterPackLines[0] : shipment1.OuterPackLines.AddNew();
			line.Containers.Add(container);
			line.JL_ActualVolume = 3;
			line.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			line.JL_ActualWeight = 10;
			line.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			line.JL_PackageCount = 2;
			MainFormConsolCollection collection = new MainFormConsolCollection(Factory);
			collection.Add(populatedConsol);
			Factory.Save();
			ForwardingConsolValueObjectDataAdapter adapter = new ForwardingConsolValueObjectDataAdapter();
			Xsd.Consol populatedConsolXSD = adapter.ExportToValueObject(populatedConsol, new ValueObjectExportContext(new NotificationBuffer()));
			CaroTransFileConverter converter = new CaroTransFileConverter(null, new BusinessObjectFactory());
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter writer = new StreamWriter(tempFile.Filename))
				{
					converter.ExportFlatFile(populatedConsolXSD, new CsvFlatFileFormat(), writer);
				}

				using (StreamReader reader = new StreamReader(tempFile.Filename))
				{
					using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
					{
						string testFilePath = resourceRetriever.SaveResourceToFile("Export.Console.output.csv");
						using (StreamReader comparisonFile = new StreamReader(testFilePath))
						{
							AssertMultilineASCIIEquals("Output should be the same", comparisonFile.ReadToEnd().Trim(), reader.ReadToEnd().Trim());
						}
					}
				}
			}
		}

		public void TestShipmentAttachedToMultipleConsols()
		{
			var secondConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "11111")).First();
			shipment.JS_MarksAndNumbers = "123";
			shipment.JS_GoodsDescription = "456";
			secondConsol.Shipments.Add(shipment);
			Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_ContainerNum, "C2")).First().JC_JK = secondConsol.PK;
			Factory.Save();
			using (TempFile tempFile = TempFile.New())
			{
				using (StreamWriter writer = new StreamWriter(tempFile.Filename))
				{
					ForwardingConsolValueObjectDataAdapter adapter = new ForwardingConsolValueObjectDataAdapter();
					Xsd.Consol populatedConsolXSD = adapter.ExportToValueObject(Consol, new ValueObjectExportContext(new NotificationBuffer()));
					new CaroTransFileConverter(null, new BusinessObjectFactory()).ExportFlatFile(populatedConsolXSD, new CsvFlatFileFormat(), writer);
				}

				using (StreamReader reader = new StreamReader(tempFile.Filename))
				{
					using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
					{
						string testFilePath = resourceRetriever.SaveResourceToFile("Export.Console.MultiConsolOutput.csv");
						using (StreamReader comparisonFile = new StreamReader(testFilePath))
						{
							AssertMultilineASCIIEquals("Output should be the same", comparisonFile.ReadToEnd().Trim(), reader.ReadToEnd().Trim());
						}
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<ForwardingConsol>();
			Consol.AutomaticallyUpdatePackLineContainers = false;
			Consol.JK_AgentType = "AGT";
			Consol.JK_TransportMode = "AIR";
			Consol.JK_MasterBillNum = "12345";
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "C2";
			CommonContainer container3 = Consol.Containers.AddNew();
			container3.JC_ContainerNum = "C3";
			CommonShipment shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "11111";
			PackLine line1S1 = shipment1.OuterPackLines.AddNew();
			line1S1.Containers.Add(container1);
			line1S1.JL_ActualVolume = 3;
			line1S1.JL_ActualWeight = 10;
			line1S1.JL_PackageCount = 7;
			PackLine line2S1 = shipment1.OuterPackLines.AddNew();
			line2S1.Containers.Add(container2);
			line2S1.JL_ActualVolume = 2;
			line2S1.JL_ActualWeight = 5;
			line2S1.JL_PackageCount = 4;
			CommonShipment shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "22222";
			PackLine line1S2 = shipment2.OuterPackLines.AddNew();
			line1S2.Containers.Add(container2);
			line1S2.JL_ActualVolume = 1;
			line1S2.JL_ActualWeight = 1;
			CommonShipment shipment3 = Consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "33333";
			PackLine line1S3 = shipment3.OuterPackLines.AddNew();
			line1S3.Containers.Add(container3);
			line1S3.JL_ActualVolume = 10;
			line1S3.JL_ActualWeight = 10;
			PackLine line2S3 = shipment3.OuterPackLines.AddNew();
			line2S3.Containers.Add(container2);
			line2S3.JL_ActualVolume = 5;
			line2S3.JL_ActualWeight = 5;
			Factory.Save();
			ForwardingConsolValueObjectDataAdapter adapter = new ForwardingConsolValueObjectDataAdapter();
			ConsolXSD = adapter.ExportToValueObject(Consol, new ValueObjectExportContext(new NotificationBuffer()));
		}

		ForwardingConsol Consol;
		Xsd.Consol ConsolXSD;
	}
}
