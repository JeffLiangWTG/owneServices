using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(DocCFSShipment))]
	sealed class DocCFSShipmentTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocCFSShipment.New(Shipment, Factory) };
		}

		public void TestCFSContainerNumber()
		{
			Shipment.CFSContainerNumber = "CONT 234";
			AssertEquals("CONT 234", CFSShipmentWrapper.CFSContainerNumber);
		}

		public void TestCFSVessel()
		{
			Shipment.CFSVessel = "VESSEL";
			AssertEquals("VESSEL", CFSShipmentWrapper.CFSVessel);
		}

		public void TestCFSVoyage()
		{
			Shipment.CFSVoyage = "VOY 123";
			AssertEquals("VOY 123", CFSShipmentWrapper.CFSVoyage);
		}

		public void TestCFSPackageType()
		{
			CFSShipmentWrapper.CFSPackageType = "PLT";
			AssertEquals("PLT", CFSShipmentWrapper.CFSPackageType);
		}

		public void TestLabelConNoteHeading()
		{
			Shipment.LabelConNoteHeading = "CON";
			AssertEquals("CON", CFSShipmentWrapper.LabelConNoteHeading);
		}

		public void TestLabelConNote()
		{
			Shipment.LabelConNote = "CONNOTE";
			AssertEquals("CONNOTE", CFSShipmentWrapper.LabelConNote);
		}

		public void TestLabelConsigneeHeading()
		{
			Shipment.LabelConsigneeHeading = "CNE";
			AssertEquals("CNE", CFSShipmentWrapper.LabelConsigneeHeading);
		}

		public void TestLabelConsignee()
		{
			Shipment.LabelConsignee = "CONSIGNEE NAME";
			AssertEquals("CONSIGNEE NAME", CFSShipmentWrapper.LabelConsignee);
		}

		public void TestClientName()
		{
			Shipment.ClientName = "CLIENT NAME";
			AssertEquals("CLIENT NAME", CFSShipmentWrapper.ClientName);
		}

		public void TestTotalPackages()
		{
			CFSShipmentWrapper.TotalPackages = 12;
			AssertEquals(12, CFSShipmentWrapper.TotalPackages);
		}

		public void TestETA()
		{
			Shipment.ETA = "12-Jun-90";
			AssertEquals("12-Jun-90", CFSShipmentWrapper.ETA);
		}

		public void TestDest()
		{
			Shipment.Dest = "AUMEL";
			AssertEquals("AUMEL", CFSShipmentWrapper.Dest);
		}

		public void TestHBL()
		{
			Shipment.HBL = "HBL 123";
			AssertEquals("HBL 123", CFSShipmentWrapper.HBL);
		}

		public void TestMarks()
		{
			Shipment.Marks = "Marks testing 123";
			AssertEquals("Marks testing 123", CFSShipmentWrapper.Marks);
		}

		public void TestShipmentNumber()
		{
			Shipment.ShipmentNumber = "S00028383";
			AssertEquals("S00028383", CFSShipmentWrapper.ShipmentNumber);
		}

		public void TestBarCode()
		{
			Shipment.CFSContainerNumber = "CONT123";
			Shipment.ShipmentNumber = "SHIP123";
			AssertEquals(new TextBarcode("CONT123SHIP123").TextAs128sFontString, CFSShipmentWrapper.BarCodeAs128Font);
		}

		public void TestIsTranshipment()
		{
			Shipment.Dest = "AUMEL";
			AssertEquals(true, CFSShipmentWrapper.IsTranshipment("JPOSA"));
			AssertEquals(true, CFSShipmentWrapper.IsTranshipment("NZAKL"));
			AssertEquals(false, CFSShipmentWrapper.IsTranshipment("AUMEL"));
			AssertEquals(false, CFSShipmentWrapper.IsTranshipment("AUSYD"));
			AssertEquals(false, CFSShipmentWrapper.IsTranshipment(""));

			Shipment.Dest = "";
			AssertEquals(false, CFSShipmentWrapper.IsTranshipment("AUMEL"));
			AssertEquals(false, CFSShipmentWrapper.IsTranshipment("NZAKL"));
		}

		public void TestIsOnForwarding()
		{
			Shipment.Dest = "AUMEL";
			AssertEquals(true, CFSShipmentWrapper.IsOnForwarding("AUPER"));
			AssertEquals(true, CFSShipmentWrapper.IsOnForwarding("AUSYD"));
			AssertEquals(false, CFSShipmentWrapper.IsOnForwarding("AUMEL"));
			AssertEquals(false, CFSShipmentWrapper.IsOnForwarding("JPOSA"));
			AssertEquals(false, CFSShipmentWrapper.IsOnForwarding("NZAKL"));
			AssertEquals(false, CFSShipmentWrapper.IsTranshipment(""));

			Shipment.Dest = "";
			AssertEquals(false, CFSShipmentWrapper.IsOnForwarding("AUMEL"));
			AssertEquals(false, CFSShipmentWrapper.IsOnForwarding("AUSYD"));
			AssertEquals(false, CFSShipmentWrapper.IsOnForwarding("NZAKL"));
		}

		public void TestIsImport()
		{
			Shipment.Dest = "AUMEL";
			AssertEquals(true, CFSShipmentWrapper.IsImport("AUMEL"));
			AssertEquals(true, CFSShipmentWrapper.IsImport("AUSYD"));
			AssertEquals(false, CFSShipmentWrapper.IsImport("NZAKL"));
			AssertEquals(false, CFSShipmentWrapper.IsImport(""));

			Shipment.Dest = "";
			AssertEquals(false, CFSShipmentWrapper.IsImport("AUSYD"));
			AssertEquals(false, CFSShipmentWrapper.IsImport("NZAKL"));
		}

		NonPersistentCFSShipment Shipment;
		DocCFSShipment CFSShipmentWrapper;

		protected override void SetUp()
		{
			Shipment = new NonPersistentCFSShipment();
			CFSShipmentWrapper = DocCFSShipment.New(Shipment, Factory);
			base.SetUp();
		}
	}
}
