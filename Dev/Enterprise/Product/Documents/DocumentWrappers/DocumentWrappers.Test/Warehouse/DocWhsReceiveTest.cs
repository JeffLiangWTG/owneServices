using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsReceive))]
	sealed class DocWhsReceiveTest : DocWhsDocketTest<WhsReceive, DocWhsReceive>
	{
		#region New

		public void TestNewMethodWithOrderDocument()
		{
			AssertNotNull(Receive);
			WhsDocketLabelControl docketLabel = new WhsDocketLabelControl(Receive, 1);
			ReceiveDocketWrapper = DocWhsReceive.New(docketLabel, Factory);
			AssertEquals(typeof(WhsReceive), ReceiveDocketWrapper.WrappedObject.GetType());
			AssertEquals(Receive, ReceiveDocketWrapper.WrappedObject);
		}

		#endregion

		#region Properties

		#region ZString Fields

		public void TestInventoryDateWithLabel()
		{
			AssertEquals(string.Format("(As at: {0})", ZDateTime.Today.ToShortDateString()), ReceiveDocketWrapper.InventoryDateWithLabel);
		}

		public void TestArrivalDateWithLabel()
		{
			AssertEquals("Pre-condition", "", ReceiveDocketWrapper.ArrivalDateWithLabel);
			var testDate = new ZDateTimeOffset(2010, 11, 01);
			Receive.WD_ArrivalDate = testDate;
			AssertEquals("(Arrived: 01-Nov-10)", ReceiveDocketWrapper.ArrivalDateWithLabel);
		}

		public void TestConfirmationInstructionsAndLabel()
		{
			Receive.Notes.RemoveAndDeleteAll();
			AssertEquals("DocWrapper ConfirmationInstructions should be empty", ZString.Empty, ReceiveDocketWrapper.ConfirmationInstructions);
			AssertEquals("DocWrapper ConfirmationInstructionsLabel should be empty", ZString.Empty, ReceiveDocketWrapper.ConfirmationInstructionsLabel);

			StmNote note = Receive.Notes.AddNew(true, PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote.Description, "TEST");
			AssertEquals("DocWrapper ConfirmationInstructions is incorrect", "TEST", ReceiveDocketWrapper.ConfirmationInstructions);
			AssertEquals("DocWrapper ConfirmationInstructionsLabel is incorrect", "Notes: ", ReceiveDocketWrapper.ConfirmationInstructionsLabel);
		}

		#endregion

		#endregion

		#region Collections

		public void TestPalletIDLabels_HasNone()
		{
			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(new List<GeneratedID>());

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				AssertEquals(0, ReceiveDocketWrapper.PalletIDLabels.Count);
				mockGenerator.Verify(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
			}
		}

		[TestDate(2008, 06, 27)]
		public void TestPalletIDLabels_HasArrival()
		{
			Receive.WD_ArrivalDate = new ZDateTimeOffset(2008, 06, 29);

			var ids = new List<GeneratedID>() { new GeneratedID("0001", 1), new GeneratedID("0002", 2), new GeneratedID("0003", 3), new GeneratedID("0004", 4), new GeneratedID("0005", 5) };

			AssertPalletIDLabelsWithDate(ids, 5, new ZDateTime(2008, 06, 29));
		}

		[TestDate(2008, 06, 27)]
		public void TestPalletIDLabels_NoArrivalHasETA()
		{
			Receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			Receive.WD_ETA = new ZDateTimeOffset(2008, 06, 28);

			var ids = new List<GeneratedID>() { new GeneratedID("0001", 1), new GeneratedID("0002", 2), new GeneratedID("0003", 3), new GeneratedID("0004", 4), new GeneratedID("0005", 5) };

			AssertPalletIDLabelsWithDate(ids, 5, new ZDateTime(2008, 06, 28));
		}

		[TestDate(2008, 06, 27)]
		public void TestPalletIDLabels_NoArrivalNoETA()
		{
			Receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			Receive.WD_ETA = ZDateTimeOffset.Empty;

			var ids = new List<GeneratedID>() { new GeneratedID("0001", 1), new GeneratedID("0002", 2), new GeneratedID("0003", 3), new GeneratedID("0004", 4), new GeneratedID("0005", 5) };

			AssertPalletIDLabelsWithDate(ids, 5, ZDateTime.Now);
		}

		[TestDate(2008, 06, 27)]
		public void TestPalletIDLabels_RunsOutOfIDs()
		{
			Receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			Receive.WD_ETA = ZDateTimeOffset.Empty;

			var ids = new List<GeneratedID> { new GeneratedID("0001", 1), new GeneratedID("0002", 2), new GeneratedID("0003", 3) };

			AssertPalletIDLabelsWithDate(ids, 5, ZDateTime.Now);
		}

		void AssertPalletIDLabelsWithDate(IEnumerable<GeneratedID> ids, int expectedIDsNum, ZDateTime expectedDate)
		{
			AssertEquals(0, ReceiveDocketWrapper.PalletIDLabels.Count);

			var docketLabel = new WhsDocketLabelControl(Receive, 1);
			ReceiveDocketWrapper = DocWhsReceive.New(docketLabel, Factory);

			docketLabel.NumberOfLabelsToPrint = expectedIDsNum;

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(ids);

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				var idLabels = ReceiveDocketWrapper.PalletIDLabels;
				mockGenerator.Verify(g => g.GenerateIDs(Receive, expectedIDsNum, false, 1), Times.Once);

				AssertEquals(expectedIDsNum, idLabels.Count);
				var idsArr = ids.ToArray();
				for (var i = 0; i < idLabels.Count; i++)
				{
					AssertEquals(i + 1, idLabels[i].LabelNumber);
					AssertEquals(expectedDate, idLabels[i].PrintDate);

					if (i < idsArr.Length)
					{
						AssertEquals(idsArr[i].FormattedID, idLabels[i].PalletID);
					}
					else
					{
						AssertEquals(ZString.Empty, idLabels[i].PalletID);
					}
				}
			}
		}

		public void TestInventoryCollection()
		{
			AssertEquals("Inventory collection on Receive object should be empty", 0, Receive.Lines.Count);
			AssertEquals("Inventory collection on Receive Wrapper should be empty", 0, ReceiveDocketWrapper.Lines.Count);

			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			AssertEquals("Inventory collection on Receive object should have count of 3", 3, Receive.Lines.Count);
			AssertEquals("Inventory collection on Receive Wrapper should have count of 3", 3, ReceiveDocketWrapper.Lines.Count);
		}

		#region PaletizedInventory

		public void TestPalletizedInventory_ForSelectedReceiveLine()
		{
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();

			Receive.Lines[0].WE_TransactionQuantity = 2m; // Has No Pallet ID
			Receive.Lines[1].WE_PalletID = "PLT-1";     // Has No Units
														//To make sure we have more that 1 inventory on PLT-2, but will Print Label only for 1 inventory.
			Receive.Lines[2].WE_TransactionQuantity = 3m;
			Receive.Lines[2].WE_StockOnHand = 20m;
			Receive.Lines[2].WE_PalletID = "PLT-2";
			Receive.Lines[3].WE_TransactionQuantity = 4m;
			Receive.Lines[3].WE_StockOnHand = 4m;
			Receive.Lines[3].WE_PalletID = "PLT-2";

			Receive.InventoryToPrintPalletLabelFor = Receive.Inventory[0];
			AssertEquals("Inventry doesn't have PalletID defined, so no Label should be printed for it.", 0, ReceiveDocketWrapper.PalletizedInventory.Count);

			Receive.InventoryToPrintPalletLabelFor = Receive.Inventory[1];
			AssertEquals("Inventory has 0 Receive Units and 0 Inventory Units, so no Label should be printed for it.", 0, ReceiveDocketWrapper.PalletizedInventory.Count);

			Receive.InventoryToPrintPalletLabelFor = Receive.Inventory[2];
			AssertEquals(1, ReceiveDocketWrapper.PalletizedInventory.Count);
			DocWhsPalletLabel palletLabel1 = ReceiveDocketWrapper.PalletizedInventory[0];
			AssertEquals(3m, palletLabel1.InventoryLine1.GroupedReceiveUnits);
			AssertEquals(20m, palletLabel1.InventoryLine1.GroupedInventoryUnits);
			AssertEquals("Make sure that Pallet Label is printed for the correct Inventory Line.", Receive.Lines[2], (WhsDocketLine)palletLabel1.InventoryLine1.WrappedObject);
			AssertNull("Make sure that Label for only one inventory will be printed.", palletLabel1.InventoryLine2);

			//check that inventory with no receive units but some inventory units (eg inventory created by an adjustment) will print a label
			Receive.Lines[4].WE_PalletID = "PLT-4";
			Receive.Lines[4].WE_StockOnHand = 100m;
			Receive.InventoryToPrintPalletLabelFor = Receive.Inventory[4];
			AssertEquals(1, ReceiveDocketWrapper.PalletizedInventory.Count);
			DocWhsPalletLabel palletLabel2 = ReceiveDocketWrapper.PalletizedInventory[0];
			AssertEquals(0m, palletLabel2.InventoryLine1.GroupedReceiveUnits);
			AssertEquals(100m, palletLabel2.InventoryLine1.GroupedInventoryUnits);
			AssertEquals("Make sure that Pallet Label is printed for the correct Inventory Line.", Receive.Lines[4], (WhsDocketLine)palletLabel2.InventoryLine1.WrappedObject);
			AssertNull("Make sure that Label for only one inventory will be printed.", palletLabel2.InventoryLine2);

			//check that inventory with some receive units but no inventory units (eg inventory has all been sent out) will print a label
			Receive.Lines[5].WE_PalletID = "PLT-5";
			Receive.Lines[5].WE_TransactionQuantity = 99m;
			Receive.Lines[5].WE_StockOnHand = 0m;
			Receive.InventoryToPrintPalletLabelFor = Receive.Inventory[5];
			AssertEquals(1, ReceiveDocketWrapper.PalletizedInventory.Count);
			DocWhsPalletLabel palletLabel3 = ReceiveDocketWrapper.PalletizedInventory[0];
			AssertEquals(99m, palletLabel3.InventoryLine1.GroupedReceiveUnits);
			AssertEquals(0m, palletLabel3.InventoryLine1.GroupedInventoryUnits);
			AssertEquals("Make sure that Pallet Label is printed for the correct Inventory Line.", Receive.Lines[5], (WhsDocketLine)palletLabel3.InventoryLine1.WrappedObject);
			AssertNull("Make sure that Label for only one inventory will be printed.", palletLabel3.InventoryLine2);
		}

		public void TestPalletizedInventory_ForAllReceiveLines()
		{
			AssertEquals(0, ReceiveDocketWrapper.PalletizedInventory.Count);

			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();

			AssertNull("Precondition - ensure that Pallet Labels are getting printed for all inventories", Receive.InventoryToPrintPalletLabelFor);
			AssertEquals(0, ReceiveDocketWrapper.PalletizedInventory.Count);

			Receive.Lines[0].WE_PalletID = "123";
			Receive.Lines[0].WE_TransactionQuantity = 10m;
			Receive.Lines[0].WE_StockOnHand = 5m;
			AssertEquals(1, ReceiveDocketWrapper.PalletizedInventory.Count);
			AssertEquals("123", ReceiveDocketWrapper.PalletizedInventory[0].LabelString);
			AssertEquals(10m, ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.GroupedReceiveUnits);
			AssertEquals(5m, ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.GroupedInventoryUnits);
			AssertEquals(Receive.Lines[0], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.WrappedObject);

			Receive.Lines[2].WE_PalletID = "123";
			Receive.Lines[2].WE_TransactionQuantity = 11m;
			Receive.Lines[2].WE_StockOnHand = 20m;
			AssertEquals(1, ReceiveDocketWrapper.PalletizedInventory.Count);
			AssertEquals("123", ReceiveDocketWrapper.PalletizedInventory[0].LabelString);
			AssertEquals(Receive.Lines[0], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.WrappedObject);
			AssertEquals(21m, ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.GroupedReceiveUnits);
			AssertEquals(25m, ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.GroupedInventoryUnits);
			AssertNull(ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine2);

			Receive.Lines[2].WE_PalletID = "123";
			Receive.Lines[2].WE_PartAttrib1 = "123";
			Receive.Lines[3].WE_PalletID = "123";
			Receive.Lines[3].WE_SerialNumber = "SER";
			Receive.Lines[3].WE_TransactionQuantity = 15m;
			Receive.Lines[3].WE_StockOnHand = 18m;
			AssertEquals(2, ReceiveDocketWrapper.PalletizedInventory.Count);
			AssertEquals("123", ReceiveDocketWrapper.PalletizedInventory[0].LabelString);
			AssertEquals(Receive.Lines[0], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.WrappedObject);
			AssertEquals(10m, ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.GroupedReceiveUnits);
			AssertEquals(5m, ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.GroupedInventoryUnits);
			AssertEquals(Receive.Lines[2], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine2.WrappedObject);
			AssertEquals(11m, ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine2.GroupedReceiveUnits);
			AssertEquals(20m, ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine2.GroupedInventoryUnits);

			AssertEquals("123", ReceiveDocketWrapper.PalletizedInventory[1].LabelString);
			AssertEquals(Receive.Lines[3], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[1].InventoryLine1.WrappedObject);
			AssertEquals(15m, ReceiveDocketWrapper.PalletizedInventory[1].InventoryLine1.GroupedReceiveUnits);
			AssertEquals(18m, ReceiveDocketWrapper.PalletizedInventory[1].InventoryLine1.GroupedInventoryUnits);
			AssertNull(ReceiveDocketWrapper.PalletizedInventory[1].InventoryLine2);

			Receive.Lines[1].WE_PalletID = "234";
			Receive.Lines[1].WE_TransactionQuantity = 11m;
			Receive.Lines[1].WE_StockOnHand = 100m;
			AssertEquals(3, ReceiveDocketWrapper.PalletizedInventory.Count);
			AssertEquals("123", ReceiveDocketWrapper.PalletizedInventory[0].LabelString);
			AssertEquals("123", ReceiveDocketWrapper.PalletizedInventory[1].LabelString);
			AssertEquals("234", ReceiveDocketWrapper.PalletizedInventory[2].LabelString);
			AssertEquals(Receive.Lines[0], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine1.WrappedObject);
			AssertEquals(Receive.Lines[2], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[0].InventoryLine2.WrappedObject);
			AssertEquals(Receive.Lines[3], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[1].InventoryLine1.WrappedObject);
			AssertEquals(Receive.Lines[1], (WhsDocketLine)ReceiveDocketWrapper.PalletizedInventory[2].InventoryLine1.WrappedObject);
			AssertEquals(11m, ReceiveDocketWrapper.PalletizedInventory[2].InventoryLine1.GroupedReceiveUnits);
			AssertEquals(100m, ReceiveDocketWrapper.PalletizedInventory[2].InventoryLine1.GroupedInventoryUnits);
			AssertNull(ReceiveDocketWrapper.PalletizedInventory[2].InventoryLine2);

			Receive.Lines[4].WE_PalletID = "1456";
			AssertEquals("inventory lines with 0 Receive Units and 0 Inventory Units should not count", 3, ReceiveDocketWrapper.PalletizedInventory.Count);
		}

		public void TestPalletizedInventory_ForAllReceiveLines_WithZeroQuantities()
		{
			AssertEquals(0, ReceiveDocketWrapper.PalletizedInventory.Count);

			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines.AddNew();
			Receive.Lines[0].WE_PalletID = "123";
			AssertEquals("inventory lines with 0 Receive Units and 0 Inventory Units should not count", 0, ReceiveDocketWrapper.PalletizedInventory.Count);

			Receive.Lines[1].WE_PalletID = "456";
			Receive.Lines[1].WE_TransactionQuantity = 10m;
			AssertEquals("inventory lines with some Receive Units and 0 Inventory Units should count", 1, ReceiveDocketWrapper.PalletizedInventory.Count);

			Receive.Lines[2].WE_PalletID = "789";
			Receive.Lines[2].WE_StockOnHand = 20m;
			AssertEquals("inventory lines with  0 Receive Units and some Inventory Units should count", 2, ReceiveDocketWrapper.PalletizedInventory.Count);
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Receive = Docket;
			ReceiveDocketWrapper = DocketWrapper;
		}

		protected override DocWhsReceive CreateWhsDocketWrapper(WhsDocketLabelControl docketLabel)
		{
			return DocWhsReceive.New(docketLabel, Factory);
		}

		protected override DocWhsReceive CreateWhsDocketWrapper(WhsReceive docket)
		{
			return DocWhsReceive.New(docket, Factory);
		}

		WhsReceive Receive;
		DocWhsReceive ReceiveDocketWrapper;

		#endregion
	}
}
