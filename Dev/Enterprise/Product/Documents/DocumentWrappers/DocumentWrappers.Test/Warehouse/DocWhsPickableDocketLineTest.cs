using Enterprise.Barcode.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPickableDocketLine))]
	public class DocWhsPickableDocketLineTest : DocWhsDocketLineTest<WhsOrder, WhsPickableDocketLine, DocWhsPickableDocketLine>
	{
		#region Properties

		#region TestLineNoCore

		protected override void TestLineNoCore()
		{
			DocketLine.WE_LineNo = 3;
			AssertEquals(3, DocketLineWrapper.LineNo);

			WhsPickableDocketLine line2 = Docket.Lines.AddNew();
			line2.WE_LineNo = 5;
			DocketLineWrapper.AddLineForRollUp(line2);
			AssertEquals(3, DocketLineWrapper.LineNo);

			WhsPickableDocketLine line3 = Docket.Lines.AddNew();
			line3.WE_LineNo = 1;
			DocketLineWrapper.AddLineForRollUp(line3);
			AssertEquals(1, DocketLineWrapper.LineNo);
		}

		#endregion

		#region TestUnitsCore

		protected override void TestUnitsCore()
		{
			DocketLine.WE_TransactionQuantity = 10m;
			AssertEquals(10m, DocketLineWrapper.Units);

			DocketLineWrapper.AddLineForRollUp(DocketLine);
			AssertEquals(20m, DocketLineWrapper.Units);
		}

		#endregion

		#region TestUnitsMetCore

		protected override void TestUnitsMetCore()
		{
			Docket.Delete();
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var line1 = order.Lines[0];
			Helper.CreatePickNew(order);

			var docketLineWrapper = DocWhsPickableDocketLine.New(line1, Factory);
			AssertEquals(15m, docketLineWrapper.UnitsMet);

			docketLineWrapper.AddLineForRollUp(line1);
			AssertEquals(30m, docketLineWrapper.UnitsMet);
		}

		#endregion

		#region 

		protected override void TestUnitsMetBarcodeCore()
		{
			Docket.Delete();
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var line1 = order.Lines[0];
			Helper.CreatePickNew(order);

			var docketLineWrapper = DocWhsPickableDocketLine.New(line1, Factory);
			var barcode1 = new TextBarcode("10.00");
			AssertEquals(barcode1.TextAs128sFontString, docketLineWrapper.UnitsMetBarcode);

			line1.ReleaseLines[0].Quantity = 5.67m;
			var barcode2 = new TextBarcode("5.67");
			AssertEquals(barcode2.TextAs128sFontString, docketLineWrapper.UnitsMetBarcode);
		}

		#endregion

		public void TestPartAttribute2Name()
		{
			AssertEquals("", DocketLineWrapper.PartAttribute2Name);

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			Docket.WD_OH_Client = client.PK;
			AssertEquals("", DocketLineWrapper.PartAttribute2Name);

			client.MiscServ.OM_IMPartAttrib2Name = "PartAttribute2Name";
			AssertEquals("PartAttribute2Name", DocketLineWrapper.PartAttribute2Name);
		}

		public void TestPartAttribute3Name()
		{
			AssertEquals("", DocketLineWrapper.PartAttribute3Name);

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			Docket.WD_OH_Client = client.PK;
			AssertEquals("", DocketLineWrapper.PartAttribute3Name);

			client.MiscServ.OM_IMPartAttrib3Name = "PartAttribute3Name";
			AssertEquals("PartAttribute3Name", DocketLineWrapper.PartAttribute3Name);
		}

		public void TestProductDescription()
		{
			AssertEquals("Precondition:", "", DocketLineWrapper.ProductDescription);
			var part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			part.OP_Desc = "123";
			AssertEquals("123", DocketLineWrapper.ProductDescription);
		}

		#endregion

		#region Implementation

		protected override DocWhsPickableDocketLine CreateDocketLineWrapper(WhsPickableDocketLine docketLine)
		{
			return DocWhsPickableDocketLine.New(docketLine, Factory);
		}

		#endregion
	}
}
