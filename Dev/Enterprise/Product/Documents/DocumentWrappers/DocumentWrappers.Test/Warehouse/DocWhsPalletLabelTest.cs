using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPalletLabel))]
	sealed class DocWhsPalletLabelTest : DocWhsLabelTest
	{
		#region Related Business Objects

		public void TestInventoryLine1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals(data.DocInventory1, data.LabelWrapperInventory.InventoryLine1);

			var newDocWhsInventory = DocWhsInventory.New(data.Line1, Factory);
			data.LabelWrapperInventory.InventoryLine1 = newDocWhsInventory;
			AssertEquals(newDocWhsInventory, data.LabelWrapperInventory.InventoryLine1);
		}

		public void TestInventoryLine2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals(data.DocInventory2, data.LabelWrapperInventory.InventoryLine2);

			var newDocWhsInventory = DocWhsInventory.New(data.Line1, Factory);
			data.LabelWrapperInventory.InventoryLine2 = newDocWhsInventory;
			AssertEquals(newDocWhsInventory, data.LabelWrapperInventory.InventoryLine2);
		}

		#endregion

		#region Properties

		#region ZString fields

		public void TestTotalWeight()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals("0 ", data.LabelWrapper.TotalWeight);

			data.LabelWrapper.InventoryLine1 = data.DocInventory1;
			data.Part1.OP_Weight = 10m;
			data.Part1.OP_WeightUQ = "KG";
			((WhsDocketLine)data.DocInventory1.WrappedObject).WE_OP = data.Part1.PK;
			data.DocInventory1.GroupedReceiveUnits = 10m;
			AssertEquals("100 KG", data.LabelWrapper.TotalWeight);

			data.LabelWrapper.InventoryLine2 = data.DocInventory2;
			data.Part2.OP_Weight = 10m;
			data.Part2.OP_WeightUQ = "G";
			((WhsDocketLine)data.DocInventory2.WrappedObject).WE_OP = data.Part2.PK;
			data.DocInventory2.GroupedReceiveUnits = 10m;
			AssertEquals("100.10 KG", data.LabelWrapper.TotalWeight);

			data.LabelWrapper.InventoryLine1 = null;
			AssertEquals("0.10 KG", data.LabelWrapper.TotalWeight);

			data.LabelWrapper.InventoryLine2 = null;
			AssertEquals("0 ", data.LabelWrapper.TotalWeight);
		}

		public void TestUnitsUQ1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals("UNT", data.LabelWrapperInventory.UnitsUQ1);
			data.Part1.OP_StockKeepingUnit = "BOX";
			data.Line1.WE_OP = data.Part1.PK;
			AssertEquals("BOX", data.LabelWrapperInventory.UnitsUQ1);
		}

		public void TestUnitsUQ2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals("UNT", data.LabelWrapperInventory.UnitsUQ2);
			data.Part1.OP_StockKeepingUnit = "BOX";
			data.Line2.WE_OP = data.Part1.PK;
			AssertEquals("BOX", data.LabelWrapperInventory.UnitsUQ2);
		}

		public void TestReceivedQty1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals("Pre-condition", "0", data.LabelWrapperInventory.ReceivedQty1);
			//#warning use the data.part1
			data.LabelWrapperInventory.InventoryLine1.GroupedReceiveUnits = 12m;
			AssertEquals("12", data.LabelWrapperInventory.ReceivedQty1);

			data.Part1.OP_CountDecimalPlaces = 2;
			data.Line1.WE_OP = data.Part1.PK;
			data.LabelWrapperInventory.InventoryLine1.GroupedReceiveUnits = 12.34m;
			AssertEquals("12.34", data.LabelWrapperInventory.ReceivedQty1);

			data.LabelWrapperInventory.InventoryLine1 = null;
			AssertEquals("", data.LabelWrapperInventory.ReceivedQty1);
		}

		public void TestInventoryQty1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals("Pre-condition", "0", data.LabelWrapperInventory.InventoryQty1);

			data.LabelWrapperInventory.InventoryLine1.GroupedInventoryUnits = 12m;
			AssertEquals("12", data.LabelWrapperInventory.InventoryQty1);

			data.Part1.OP_CountDecimalPlaces = 2;
			data.Line1.WE_OP = data.Part1.PK;
			data.LabelWrapperInventory.InventoryLine1.GroupedInventoryUnits = 12.34m;
			AssertEquals("12.34", data.LabelWrapperInventory.InventoryQty1);

			data.LabelWrapperInventory.InventoryLine1 = null;
			AssertEquals("", data.LabelWrapperInventory.InventoryQty1);
		}

		public void TestReceivedQt2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals("Pre-condition", "0", data.LabelWrapperInventory.ReceivedQty2);

			data.LabelWrapperInventory.InventoryLine2.GroupedReceiveUnits = 12m;
			AssertEquals("12", data.LabelWrapperInventory.ReceivedQty2);

			data.Part1.OP_CountDecimalPlaces = 2;
			data.Line2.WE_OP = data.Part1.PK;
			data.LabelWrapperInventory.InventoryLine2.GroupedReceiveUnits = 12.34m;
			AssertEquals("12.34", data.LabelWrapperInventory.ReceivedQty2);

			data.LabelWrapperInventory.InventoryLine2 = null;
			AssertEquals("", data.LabelWrapperInventory.ReceivedQty2);
		}

		public void TestInventoryQty2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertEquals("Pre-condition", "0", data.LabelWrapperInventory.InventoryQty2);

			data.LabelWrapperInventory.InventoryLine2.GroupedInventoryUnits = 12m;
			AssertEquals("12", data.LabelWrapperInventory.InventoryQty2);

			data.Part1.OP_CountDecimalPlaces = 2;
			data.Line2.WE_OP = data.Part1.PK;
			data.LabelWrapperInventory.InventoryLine2.GroupedInventoryUnits = 12.34m;
			AssertEquals("12.34", data.LabelWrapperInventory.InventoryQty2);

			data.LabelWrapperInventory.InventoryLine2 = null;
			AssertEquals("", data.LabelWrapperInventory.InventoryQty2);
		}

		public void TestLeftOverAttributes1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory1);
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.LeftOverAttributes1);
			data.Line1.WE_PartAttrib1 = "PartAttr1";
			AssertEquals(": PartAttr1", data.LabelWrapperInventory.LeftOverAttributes1);
			data.Line1.WE_PartAttrib2 = "PartAttr2";
			AssertEquals(": PartAttr1, : PartAttr2", data.LabelWrapperInventory.LeftOverAttributes1);
			data.Line1.WE_PartAttrib3 = "PartAttr3";
			AssertEquals(": PartAttr1, : PartAttr2, : PartAttr3", data.LabelWrapperInventory.LeftOverAttributes1);
			data.Line1.WE_PartAttrib1 = ZString.Empty;
			AssertEquals(": PartAttr2, : PartAttr3", data.LabelWrapperInventory.LeftOverAttributes1);

			data.Org1.MiscServ.OM_IMPartAttrib1Name = "P1";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "P2";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "P3";
			data.Line1.Docket.WD_OH_Client = data.Org1.PK;

			data.Line1.WE_PartAttrib1 = "PartAttr1";
			AssertEquals("P1: PartAttr1, P2: PartAttr2, P3: PartAttr3", data.LabelWrapperInventory.LeftOverAttributes1);
			data.Line1.WE_PartAttrib1 = ZString.Empty;
			AssertEquals("P2: PartAttr2, P3: PartAttr3", data.LabelWrapperInventory.LeftOverAttributes1);
			data.Line1.WE_PartAttrib2 = ZString.Empty;
			AssertEquals("P3: PartAttr3", data.LabelWrapperInventory.LeftOverAttributes1);

			data.LabelWrapperInventory.InventoryLine1 = null;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.LeftOverAttributes1);
		}

		public void TestLeftOverAttributes2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory2);
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.LeftOverAttributes2);
			data.Line2.WE_PartAttrib1 = "PartAttr1";
			AssertEquals(": PartAttr1", data.LabelWrapperInventory.LeftOverAttributes2);
			data.Line2.WE_PartAttrib2 = "PartAttr2";
			AssertEquals(": PartAttr1, : PartAttr2", data.LabelWrapperInventory.LeftOverAttributes2);
			data.Line2.WE_PartAttrib3 = "PartAttr3";
			AssertEquals(": PartAttr1, : PartAttr2, : PartAttr3", data.LabelWrapperInventory.LeftOverAttributes2);
			data.Line2.WE_PartAttrib1 = ZString.Empty;
			AssertEquals(": PartAttr2, : PartAttr3", data.LabelWrapperInventory.LeftOverAttributes2);

			data.Org1.MiscServ.OM_IMPartAttrib1Name = "P1";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "P2";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "P3";
			data.Line2.Docket.WD_OH_Client = data.Org1.PK;

			data.Line2.WE_PartAttrib1 = "PartAttr1";
			AssertEquals("P1: PartAttr1, P2: PartAttr2, P3: PartAttr3", data.LabelWrapperInventory.LeftOverAttributes2);
			data.Line2.WE_PartAttrib1 = ZString.Empty;
			AssertEquals("P2: PartAttr2, P3: PartAttr3", data.LabelWrapperInventory.LeftOverAttributes2);
			data.Line2.WE_PartAttrib2 = ZString.Empty;
			AssertEquals("P3: PartAttr3", data.LabelWrapperInventory.LeftOverAttributes2);

			data.LabelWrapperInventory.InventoryLine2 = null;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.LeftOverAttributes2);
		}

		public void TestFirstDate1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory1);
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.FirstDate1);
			data.Line1.WE_PackingDate = new ZDate(1978, 10, 1);
			AssertEquals("Packing Date: 01-Oct-78", data.LabelWrapperInventory.FirstDate1);
			data.Line1.WE_ExpiryDate = new ZDate(1978, 1, 10);
			AssertEquals("Expiry Date: 10-Jan-78", data.LabelWrapperInventory.FirstDate1);

			data.LabelWrapperInventory.InventoryLine1 = null;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.FirstDate1);
		}

		public void TestFirstDate2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory2);
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.FirstDate2);
			data.Line2.WE_PackingDate = new ZDate(1978, 10, 1);
			AssertEquals("Packing Date: 01-Oct-78", data.LabelWrapperInventory.FirstDate2);
			data.Line2.WE_ExpiryDate = new ZDate(1978, 1, 10);
			AssertEquals("Expiry Date: 10-Jan-78", data.LabelWrapperInventory.FirstDate2);

			data.LabelWrapperInventory.InventoryLine2 = null;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.FirstDate2);
		}

		public void TestSecondDate1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory1);
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.SecondDate1);
			data.Line1.WE_PackingDate = ZDate.Today;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.SecondDate1);
			data.Line1.WE_ExpiryDate = ZDate.Today;
			AssertEquals("Packing Date:", data.LabelWrapperInventory.SecondDate1);

			data.LabelWrapperInventory.InventoryLine1 = null;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.SecondDate1);
		}

		public void TestSecondDate2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory2);
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.SecondDate2);
			data.Line2.WE_PackingDate = ZDate.Today;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.SecondDate2);
			data.Line2.WE_ExpiryDate = ZDate.Today;
			AssertEquals("Packing Date:", data.LabelWrapperInventory.SecondDate2);

			data.LabelWrapperInventory.InventoryLine2 = null;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.SecondDate2);
		}

		public void TestLocationString()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory1);
			AssertEquals("A", data.LabelWrapperInventory.LocationString);

			data.Line1.LocationString = "";
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.LocationString);

			data.Line1.LocationString = "TT";
			AssertEquals("TT", data.LabelWrapperInventory.LocationString);

			data.LabelWrapperInventory.InventoryLine1 = null;
			AssertEquals(ZString.Empty, data.LabelWrapperInventory.LocationString);
		}

		#endregion

		#region ZDateTime Fields

		public void TestExpiryDate1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory1);
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.ExpiryDate1);

			var temporaryDate1 = ZDate.Today;
			data.Line1.WE_PackingDate = temporaryDate1;
			AssertEquals(temporaryDate1, data.LabelWrapperInventory.ExpiryDate1);

			var temporaryDate2 = ZDate.Today;
			data.Line1.WE_ExpiryDate = temporaryDate2;
			AssertEquals(temporaryDate2, data.LabelWrapperInventory.ExpiryDate1);

			data.LabelWrapperInventory.InventoryLine1 = null;
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.ExpiryDate1);
		}

		public void TestExpiryDate2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory2);
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.ExpiryDate2);

			var temporaryDate1 = ZDate.Today;
			data.Line2.WE_PackingDate = temporaryDate1;
			AssertEquals(temporaryDate1, data.LabelWrapperInventory.ExpiryDate2);

			var temporaryDate2 = ZDate.Today;
			data.Line2.WE_ExpiryDate = temporaryDate2;
			AssertEquals(temporaryDate2, data.LabelWrapperInventory.ExpiryDate2);

			data.LabelWrapperInventory.InventoryLine2 = null;
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.ExpiryDate2);
		}

		public void TestPackingDateTemporary1()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory1);
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.PackingDateTemporary1);

			var temporaryDate1 = ZDate.Today;
			data.Line1.WE_PackingDate = temporaryDate1;
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.PackingDateTemporary1);

			var temporaryDate2 = ZDate.Today;
			data.Line1.WE_ExpiryDate = temporaryDate2;
			AssertEquals(temporaryDate1, data.LabelWrapperInventory.PackingDateTemporary1);

			data.LabelWrapperInventory.InventoryLine1 = null;
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.PackingDateTemporary1);
		}

		public void TestPackingDateTemporary2()
		{
			var data = new TestEnviromentForPalletLable(Factory);
			AssertNotNull(data.DocInventory2);
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.PackingDateTemporary2);

			var temporaryDate1 = ZDate.Today;
			data.Line2.WE_PackingDate = temporaryDate1;
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.PackingDateTemporary2);

			var temporaryDate2 = ZDate.Today;
			data.Line2.WE_ExpiryDate = temporaryDate2;
			AssertEquals(temporaryDate1, data.LabelWrapperInventory.PackingDateTemporary2);

			data.LabelWrapperInventory.InventoryLine2 = null;
			AssertEquals(ZDateTime.Empty, data.LabelWrapperInventory.PackingDateTemporary2);
		}

		#endregion

		#endregion

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocWhsPalletLabel.New(labelObj, Factory),
				};
		}

		class TestEnviromentForPalletLable : TestDataSimpleEnvironment
		{
			#region Constructors

			public TestEnviromentForPalletLable(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#endregion

			#region CreateEnvironment

			protected override void CreateEnvironment()
			{
				base.CreateEnvironment();

				var labelObj = new WhsLabel();
				LabelWrapper = DocWhsPalletLabel.New(labelObj, Factory);
				Receive = Helper.CreateWhsReceive(Org1, Whs1, "111", Helper.Notify);
				Line1 = Helper.CreateWhsReceiveInventoryLine(Receive, Part1, 5m).InDocketLine;
				Line2 = Helper.CreateWhsReceiveInventoryLine(Receive, Part1, 5m).InDocketLine;
				DocInventory1 = DocWhsInventory.New(Line1, Factory);
				DocInventory2 = DocWhsInventory.New(Line2, Factory);
				LabelWrapperInventory = DocWhsPalletLabel.New(labelObj, DocInventory1, DocInventory2, Factory);
				Receive.AllocateLocationsWithMock();
				Receive.FinaliseDocket();
				AssertEquals("Precondition - ensure Receive is finalised.", true, Receive.IsFinalised);

				Factory.Save();
			}

			#endregion

			public WhsDocketLine Line1;
			public WhsDocketLine Line2;
			public DocWhsInventory DocInventory1;
			public DocWhsInventory DocInventory2;
			public DocWhsPalletLabel LabelWrapper;
			public DocWhsPalletLabel LabelWrapperInventory;
			public WhsReceive Receive;
		}

		#endregion
	}
}
