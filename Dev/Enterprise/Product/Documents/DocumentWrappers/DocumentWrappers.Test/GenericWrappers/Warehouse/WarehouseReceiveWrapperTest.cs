using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseReceiveWrapper))]
	sealed class WarehouseReceiveWrapperTest : WarehouseDocketWrapperTest
	{
		#region TestAddresses

		#region TestPickUpAddress

		protected override void TestPickUpAddressCore()
		{
			var docket = GetNewDocket();
			docket.PickUpAddressPK = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.PickUpAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestDropOffAddress

		protected override void TestDropOffAddressCore()
		{
			var docket = GetNewDocket();
			docket.DropOffAddressPK = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.DropOffAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestSupplierDocumentaryAddress

		protected override void TestSupplierDocAddressCore()
		{
			var docket = GetNewDocket();
			docket.SupplierDocAddress.E2_OA_Address = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.SupplierDocAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestGoodsBillToAddressCore

		protected override void TestGoodsBillToAddressCore()
		{
			var docket = GetNewDocket();
			docket.GoodsBillToAddressPK = AddressPKWithDetailsFilled();

			var docketWrapper = GetNewWarehouseJobGenericWrapper(docket);
			AssertEquals("MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", docketWrapper.GoodsBillToAddress.CompanyNameAndAddress);
		}

		#endregion

		#region TestAddress

		ZGuid AddressPKWithDetailsFilled()
		{
			var org = Helper.CreateClient("Org1");
			var address = org.MainAddress;
			org.OH_RL_NKClosestPort = "AU";
			org.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "US";
			address.OA_RN_NKCountryCode = "AU";
			return address.PK;
		}

		#endregion

		#endregion

		#region TestConsignor

		[SetOrgAllowMixedCase(true)]
		public void TestConsignor()
		{
			var supplier = Helper.CreateClient("SUP", "Test Supplier");

			var receive = Helper.CreateWhsReceive(Helper.CreateClient(), Helper.CreateWarehouse("WHS"));
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			var wrapper = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Test Supplier", wrapper.Consignor.CompanyName);
		}

		#endregion

		#region TestConsignee

		[SetOrgAllowMixedCase(true)]
		protected override void TestConsigneeCore()
		{
			var whsOrg = Helper.CreateClient("WHO", "Test Whs Org");
			var whs = Helper.CreateWarehouse("WHS");
			whs.WarehouseAddress.OA_OH = whsOrg.PK;
			whs.WarehouseAddress.OA_Code = "Address";
			Factory.Save();

			var receive = Helper.CreateWhsReceive(Helper.CreateClient(), whs);
			var wrapper = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Test Whs Org", wrapper.Consignee.CompanyName);
		}

		#endregion

		#region Test ConfirmationInstrationsAndLabel

		protected override void TestConfirmationInstructionsCore()
		{
			var receive = GetNewDocket();
			receive.Notes.RemoveAndDeleteAll();

			var receiveWrapper = GetNewWarehouseJobGenericWrapper(receive);
			AssertEquals("DocWrapper ConfirmationInstructions should be empty", ZString.Empty, receiveWrapper.ConfirmationInstructions.Value);
			AssertEquals("DocWrapper ConfirmationInstructionsLabel should be empty", "Notes: ", receiveWrapper.ConfirmationInstructions.Label);
		}

		public void TestConfirmationInstructionsAndLabel_WithNotes()
		{
			var receive = GetNewDocket();
			receive.Notes.RemoveAndDeleteAll();
			receive.Notes.AddNew(true, PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote.Description, "TEST");

			var receiveWrapper = GetNewWarehouseJobGenericWrapper(receive);
			AssertEquals("DocWrapper ConfirmationInstructions is incorrect", "TEST", receiveWrapper.ConfirmationInstructions.Value);
			AssertEquals("DocWrapper ConfirmationInstructionsLabel is incorrect", "Notes: ", receiveWrapper.ConfirmationInstructions.Label);
		}

		#endregion

		#region TestJobLines

		protected override void TestJobLinesCore()
		{
			var receive = (WhsReceive)GetNewDocket();
			AssertEquals("Inventory collection on Receive object should be empty", 0, receive.Lines.Count);
			var wrapperWithNoInventory = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Inventory collection on Receive Wrapper should be empty", 0, wrapperWithNoInventory.JobLines.Count);

			receive.Lines.AddNew();
			receive.Lines.AddNew();
			receive.Lines.AddNew();

			AssertEquals("Inventory collection on Receive object should have count of 3", 3, receive.Lines.Count);
			var wrapperWithInventory = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Inventory collection on Receive Wrapper should have count of 3", 3, wrapperWithInventory.JobLines.Count);
		}

		public void TestJobLines_PutawaySheetOrderedBy_WL_PutawayPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			location1.WLV_PutawayPathSequence = 4;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_PutawayPathSequence = 2;
			var location3 = data.Whs1.FindLocation("A-2-1");
			location3.WLV_PutawayPathSequence = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1);
			receiveLine1.WE_LineComment = "1";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, location2);
			receiveLine2.WE_LineComment = "2";

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 8m, location3);
			receiveLine3.WE_LineComment = "3";
			Factory.Save();

			AssertEquals("Inventory collection on Receive object should have count of 3", 3, receive.Lines.Count);
			var wrapperWithInventory = new WarehouseReceiveWrapper(receive, Factory);
			var constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Putaway Sheet");
			wrapperWithInventory.SetTemplateConstants(constants);

			CombineAssertions(() =>
			{
				AssertEquals("Inventory collection on Receive Wrapper should have count of 3", 3, wrapperWithInventory.JobLines.Count);
				AssertEquals("Receive Wrapper should correct menu title", "Putaway Sheet", wrapperWithInventory.MenuTitle);
				AssertEquals("First JobLine is correct", receiveLine3.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[0].WrappedObject).WE_LineComment);
				AssertEquals("Second JobLine is correct", receiveLine2.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[1].WrappedObject).WE_LineComment);
				AssertEquals("Third JobLine is correct", receiveLine1.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[2].WrappedObject).WE_LineComment);
			});
		}

		public void TestJobLines_NotPutawaySheet_SoNotOrderedBy_WL_PutawayPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			location1.WLV_PutawayPathSequence = 4;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_PutawayPathSequence = 2;
			var location3 = data.Whs1.FindLocation("A-2-1");
			location3.WLV_PutawayPathSequence = 1;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1);
			receiveLine1.WE_LineComment = "1";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, location2);
			receiveLine2.WE_LineComment = "2";

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 8m, location3);
			receiveLine3.WE_LineComment = "3";
			Factory.Save();

			AssertEquals("Inventory collection on Receive object should have count of 3", 3, receive.Lines.Count);
			var wrapperWithInventory = new WarehouseReceiveWrapper(receive, Factory);
			var constants = new Dictionary<string, object>();
			wrapperWithInventory.SetTemplateConstants(constants);

			CombineAssertions(() =>
			{
				AssertEquals("Inventory collection on Receive Wrapper should have count of 3", 3, wrapperWithInventory.JobLines.Count);
				AssertNotEquals("Receive Wrapper menu title should not be Putaway Sheet", "Putaway Sheet", wrapperWithInventory.MenuTitle);
				AssertEquals("First JobLine is correct", receiveLine1.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[0].WrappedObject).WE_LineComment);
				AssertEquals("Second JobLine is correct", receiveLine2.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[1].WrappedObject).WE_LineComment);
				AssertEquals("Third JobLine is correct", receiveLine3.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[2].WrappedObject).WE_LineComment);
			});
		}

		public void TestJobLines_PutawaySheet_OrderedBy_WL_PutawayPathSequence_DuplicateValues()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			location1.WLV_PutawayPathSequence = 14;
			var location2 = data.Whs1.FindLocation("A-1-2");
			location2.WLV_PutawayPathSequence = 2;
			var location3 = data.Whs1.FindLocation("A-2-1");
			location3.WLV_PutawayPathSequence = 1;
			var location4 = data.Whs1.FindLocation("A-2-2");
			location4.WLV_PutawayPathSequence = 2;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1);
			receiveLine1.WE_LineComment = "1";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, location2);
			receiveLine2.WE_LineComment = "2";

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 8m, location3);
			receiveLine3.WE_LineComment = "3";

			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 8m, location4);
			receiveLine4.WE_LineComment = "4";
			Factory.Save();

			AssertEquals("Inventory collection on Receive object should have count of 4", 4, receive.Lines.Count);
			var wrapperWithInventory = new WarehouseReceiveWrapper(receive, Factory);
			var constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Putaway Sheet");
			wrapperWithInventory.SetTemplateConstants(constants);

			CombineAssertions(() =>
			{
				AssertEquals("Inventory collection on Receive Wrapper should have count of 4", 4, wrapperWithInventory.JobLines.Count);
				AssertEquals("Receive Wrapper menu title should be Putaway Sheet", "Putaway Sheet", wrapperWithInventory.MenuTitle);
				AssertEquals("First JobLine is correct", receiveLine3.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[0].WrappedObject).WE_LineComment);
				AssertEquals("Second JobLine is correct", receiveLine2.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[1].WrappedObject).WE_LineComment);
				AssertEquals("Third JobLine is correct", receiveLine4.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[2].WrappedObject).WE_LineComment);
				AssertEquals("Fourth JobLine is correct", receiveLine1.WE_LineComment, ((WhsReceiveLine)wrapperWithInventory.JobLines[3].WrappedObject).WE_LineComment);
			});
		}

		#endregion

		#region Test JobChargeLines

		protected override void TestJobChargeLinesCore()
		{
			var receive = (WhsReceive)GetNewDocket();
			AssertNull("The initial JobHeader of Receive should be null", receive.JobHeader);
			var wrapper = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Job charge line collection on Receive Wrapper should be empty when JobHeader is null", 0, wrapper.JobChargeLines.Count);

			var newReceive = (WhsReceive)GetNewDocket();
			var newWrapper = new WarehouseReceiveWrapper(newReceive, Factory);
			AssertNotSame("Wrappers for different receivies do not share the same cached value of JobChargeLines", wrapper.JobChargeLines, newWrapper.JobChargeLines);

			Factory.NewJobWithValidTestDataForTesting<Job>().JH_ParentID = receive.PK;
			var receiveJob = (Job)receive.JobHeader;
			AssertNotNull("After the Job is created, the JobHeader of Receive should not be null", receiveJob);
			AssertEquals("Charge collection on Receive object should be empty", 0, receiveJob.Charges.Count);
			var wrapperWithJob = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Job charge line collection on Receive Wrapper should be empty when JobHeader's Charge collection is empty", 0, wrapperWithJob.JobChargeLines.Count);

			receiveJob.Charges.AddNew();
			receiveJob.Charges.AddNew();
			receiveJob.Charges.AddNew();
			AssertEquals("Charge collection on Receive object should have count of 3", 3, receiveJob.Charges.Count);
			// No need to create a new Wrapper as a cached value of JobChargeLines was reset on JobCharge changes
			AssertNotSame("Wrappers for same Receive have different collections cached on Factory level", wrapper.JobChargeLines, wrapperWithJob.JobChargeLines);
			AssertEquals("Job charge line collection on Receive Wrapper should have count of 3", 3, wrapperWithJob.JobChargeLines.Count);

			JobChargeCollectionTest.AssertNoJobChargesContainedInJobChargeCollection(Factory);
		}

		#endregion

		#region IWhsDocumentInventory_SetInventory()

		public void TestIWhsDocumentInventory_SetInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "122");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "123");
			Factory.Save();
			var wrapper = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals(2, wrapper.JobLines.Count);

			wrapper = new WarehouseReceiveWrapper(receive, Factory);
			var iWrapper = (IWhsDocumentInventory)wrapper;
			var docInventory = new WhsDocumentInventory(inventory2);
			docInventory.LabelsToPrint = 3;
			iWrapper.SetInventory(docInventory);

			AssertEquals(3, wrapper.JobLines.Count);
			AssertEquals(inventory2.InDocketLine, wrapper.JobLines[0].WrappedObject);
		}

		#endregion

		#region TestSecondaryReference

		protected override void TestSecondaryReferenceCore()
		{
			var receive = (WhsReceive)GetNewDocket();
			var receiveWrapper = GetNewWarehouseJobGenericWrapper(receive);
			AssertEquals("Pre-condition", "", receiveWrapper.SecondaryReference.Value);

			receive.WD_ExternalReference = "Test12345";
			AssertEquals("Receive Reference", receiveWrapper.SecondaryReference.Label);
			AssertEquals("Test12345", receiveWrapper.SecondaryReference.Value);
		}

		#endregion

		#region TestHasOversAndUndersCore

		protected override void TestHasOversAndUndersCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "123");
			var receiveWrapper = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Precondition", 10m, line.InDocketLine.WE_ClientOrderedUnits);
			AssertEquals("HasOversAndUnders should return false", false, receiveWrapper.HasOversAndUnders);

			line.InDocketLine.WE_TransactionQuantity = 5m;
			AssertEquals("HasOversAndUnders should return true", true, receiveWrapper.HasOversAndUnders);

			line.InDocketLine.WE_TransactionQuantity = 15m;
			AssertEquals("HasOversAndUnders should return true", true, receiveWrapper.HasOversAndUnders);

			line.InDocketLine.WE_TransactionQuantity = 10m;
			AssertEquals("HasOversAndUnders should return false", false, receiveWrapper.HasOversAndUnders);
		}

		#endregion

		#region TestPalletizedInventory

		protected override void TestPalletizedInventory_Setup(TestDataSimpleEnvironment data, out WhsDocket docket, out WhsInventoryView inventory)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.FindLocation("A-2"), "PLT-1", true, false);
			docket = receive;
			inventory = receive.Inventory[0];
			Factory.Save();
		}

		public void TestPalletizedInventory_ForAllReceiveLines_GroupSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveWrapper = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals(0, receiveWrapper.PalletizedInventory.Count);
			AssertNull("Precondition - ensure that Pallet Labels are getting printed for all inventories", receive.InventoryToPrintPalletLabelFor);
			AssertEquals(0, receiveWrapper.PalletizedInventory.Count);

			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "123");
			Factory.Save();
			line1.InDocketLine.WE_TransactionQuantity = 10m;
			line1.InDocketLine.WE_StockOnHand = 5m;
			receiveWrapper = new WarehouseReceiveWrapper(receive, Factory);
			var inventoryWrapper = receiveWrapper.PalletizedInventory[0];

			AssertEquals(1, receiveWrapper.PalletizedInventory.Count);
			AssertEquals("123", inventoryWrapper.PalletID);
			AssertEquals(10m, inventoryWrapper.GroupedReceiveUnits);
			AssertEquals(5m, inventoryWrapper.GroupedInventoryUnits);
			AssertEquals(receive.Lines[0], (WhsDocketLine)inventoryWrapper.WrappedObject);

			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 11m, null, "123");

			line1.InDocketLine.WE_TransactionQuantity = 10m;
			line1.InDocketLine.WE_StockOnHand = 5m;
			line2.InDocketLine.WE_TransactionQuantity = 11m;
			line2.InDocketLine.WE_StockOnHand = 20m;

			var receiveWrapper2 = new WarehouseReceiveWrapper(receive, Factory);
			var inventoryWrapper2 = receiveWrapper2.PalletizedInventory[0];
			AssertEquals(1, receiveWrapper2.PalletizedInventory.Count);
			AssertEquals("123", inventoryWrapper2.PalletID);
			AssertEquals(receive.Lines[0], (WhsDocketLine)inventoryWrapper2.WrappedObject);
			AssertEquals(21m, inventoryWrapper2.GroupedReceiveUnits);
			AssertEquals(25m, inventoryWrapper2.GroupedInventoryUnits);
		}

		public void TestPalletizedInventory_ForAllReceiveLines_NotGroupSamePalletIDWithDifferentPartAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveWrapper = new WarehouseReceiveWrapper(receive, Factory);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "123");
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 11m, null, "123");
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 11m, null, "234");
			Factory.Save();
			line1.InDocketLine.WE_TransactionQuantity = 10m;
			line1.InDocketLine.WE_StockOnHand = 5m;
			line2.InDocketLine.WE_TransactionQuantity = 11m;
			line2.InDocketLine.WE_StockOnHand = 20m;
			line2.InDocketLine.WE_PartAttrib1 = "123";
			line3.InDocketLine.WE_TransactionQuantity = 11m;
			line3.InDocketLine.WE_StockOnHand = 100m;

			var receiveWrapper3 = new WarehouseReceiveWrapper(receive, Factory);
			var inventoryWrapper3 = receiveWrapper3.PalletizedInventory[0];

			AssertEquals("123", inventoryWrapper3.PalletID);
			AssertEquals(receive.Lines[0], (WhsDocketLine)inventoryWrapper3.WrappedObject);
			AssertEquals(10m, inventoryWrapper3.GroupedReceiveUnits);
			AssertEquals(5m, inventoryWrapper3.GroupedInventoryUnits);
			AssertEquals(3, receiveWrapper3.PalletizedInventory.Count);

			var inventoryWrapper4 = receiveWrapper3.PalletizedInventory[1];
			AssertEquals("123", inventoryWrapper4.PalletID);
			AssertEquals(receive.Lines[1], (WhsDocketLine)inventoryWrapper4.WrappedObject);
			AssertEquals(11m, inventoryWrapper4.GroupedReceiveUnits);
			AssertEquals(20m, inventoryWrapper4.GroupedInventoryUnits);

			var receiveWrapper4 = new WarehouseReceiveWrapper(receive, Factory);
			var inventoryWrapper5 = receiveWrapper4.PalletizedInventory[0];
			AssertEquals(3, receiveWrapper4.PalletizedInventory.Count);
			AssertEquals("123", inventoryWrapper5.PalletID);
			AssertEquals(receive.Lines[0], (WhsDocketLine)inventoryWrapper5.WrappedObject);
			AssertEquals(10m, inventoryWrapper5.GroupedReceiveUnits);
			AssertEquals(5m, inventoryWrapper5.GroupedInventoryUnits);

			var inventoryWrapper6 = receiveWrapper4.PalletizedInventory[1];
			AssertEquals("123", inventoryWrapper6.PalletID);
			AssertEquals(receive.Lines[1], (WhsDocketLine)inventoryWrapper6.WrappedObject);
			AssertEquals(11m, inventoryWrapper6.GroupedReceiveUnits);
			AssertEquals(20m, inventoryWrapper6.GroupedInventoryUnits);

			var inventoryWrapper7 = receiveWrapper4.PalletizedInventory[2];
			AssertEquals("234", inventoryWrapper7.PalletID);
			AssertEquals(receive.Lines[2], (WhsDocketLine)inventoryWrapper7.WrappedObject);
			AssertEquals(11m, inventoryWrapper7.GroupedReceiveUnits);
			AssertEquals(100m, inventoryWrapper7.GroupedInventoryUnits);
		}

		public void TestPalletizedInventory_ForSelectedReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveWrapper = new WarehouseReceiveWrapper(receive, Factory);

			var line0 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);// Has No Pallet ID
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 0m, null, "PLT-1");// Has No Units
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m, null, "PLT-2");//To make sure we have more that 1 inventory on PLT-2, but will Print Label only for 1 inventory.
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 4m, null, "PLT-2");//To make sure we have more that 1 inventory on PLT-2, but will Print Label only for 1 inventory.
			var line4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 0m, null, "PLT-4");
			var line5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 0m, null, "PLT-5");
			Factory.Save();

			line0.WI_InDocketLineUnits = 2m;
			receive.InventoryToPrintPalletLabelFor = line0;
			AssertEquals("Inventry doesn't have PalletID defined, so no Label should be printed for it.", 0, receiveWrapper.PalletizedInventory.Count);

			receive.InventoryToPrintPalletLabelFor = line1;
			var receiveWrapper1 = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Inventory has 0 Receive Units and 0 Inventory Units, so no Label should be printed for it.", 0, receiveWrapper1.PalletizedInventory.Count);

			line1.WI_InDocketLineUnits = 3m;
			line1.WI_TotalUnits = 20m;
			line2.WI_InDocketLineUnits = 3m;
			line2.WI_TotalUnits = 20m;
			line3.WI_InDocketLineUnits = 4m;
			line3.WI_TotalUnits = 4m;

			receive.InventoryToPrintPalletLabelFor = line2;
			var receiveWrapper2 = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals(1, receiveWrapper2.PalletizedInventory.Count);
			var inventoryWrapper1 = receiveWrapper2.PalletizedInventory[0];
			AssertEquals(3m, inventoryWrapper1.GroupedReceiveUnits);
			AssertEquals(20m, inventoryWrapper1.GroupedInventoryUnits);
			AssertEquals("Make sure that Pallet Label is printed for the correct Inventory Line.", receive.Lines[2], (WhsDocketLine)inventoryWrapper1.WrappedObject);

			//check that inventory with no receive units but some inventory units (eg inventory created by an adjustment) will print a label
			line4.WI_TotalUnits = 100m;
			receive.InventoryToPrintPalletLabelFor = line4;
			var receiveWrapper3 = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals(1, receiveWrapper3.PalletizedInventory.Count);
			var inventoryWrapper2 = receiveWrapper3.PalletizedInventory[0];
			AssertEquals(0m, inventoryWrapper2.GroupedReceiveUnits);
			AssertEquals(100m, inventoryWrapper2.GroupedInventoryUnits);
			AssertEquals("Make sure that Pallet Label is printed for the correct Inventory Line.", receive.Lines[4], (WhsDocketLine)inventoryWrapper2.WrappedObject);

			//check that inventory with some receive units but no inventory units (eg inventory has all been sent out) will not print a label
			line5.WI_InDocketLineUnits = 99m;
			line5.WI_TotalUnits = 0m;
			receive.InventoryToPrintPalletLabelFor = line5;
			var receiveWrapper4 = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals(0, receiveWrapper4.PalletizedInventory.Count);
		}

		public void TestPalletizedInventory_ForAllReceiveLines_WithZeroQuantities()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveWrapper = new WarehouseReceiveWrapper(receive, Factory);

			var line0 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "123");
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "456");
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "789");
			Factory.Save();

			receive.InventoryToPrintPalletLabelFor = null;
			AssertEquals(0, receiveWrapper.PalletizedInventory.Count);

			var receiveWrapper1 = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("inventory lines with 0 Receive Units and 0 Inventory Units should not count", 0, receiveWrapper1.PalletizedInventory.Count);

			line1.WI_InDocketLineUnits = 10m;
			line1.WI_TotalUnits = 0m;
			var receiveWrapper2 = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("inventory lines with some Receive Units and 0 Inventory Units should not count", 0, receiveWrapper2.PalletizedInventory.Count);

			line2.WI_InDocketLineUnits = 0m;
			line2.WI_TotalUnits = 20m;
			var receiveWrapper3 = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("inventory lines with  0 Receive Units and some Inventory Units should count", 1, receiveWrapper3.PalletizedInventory.Count);
		}

		public void TestPalletizedInventory_ForAllReceiveLines_SerialNumbersAreGrouped()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.RelatedOrganisations[0].Organisation.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_UseSerialNumber = true;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "PLT-1");
			line1.WI_SerialNumber = "S/N 1";
			line1.WI_TotalUnits = 1;
			line1.WI_PartAttrib1 = "Part1";
			line1.WI_PartAttrib2 = "Part2";
			line1.WI_PartAttrib3 = "Part3";

			var receiveWrapper1 = new WarehouseReceiveWrapper(receive, Factory);

			AssertEquals("Precondition", 1, receiveWrapper1.PalletizedInventory.Count);
			AssertEquals("Precondition - Single serial number should still display if not rolled up.", "S/N 1", receiveWrapper1.PalletizedInventory[0].TrackedSerialNumber);

			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var line4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT-1");
			line2.WI_SerialNumber = "S/N 2";
			line2.WI_PartAttrib1 = "Part1";
			line2.WI_PartAttrib2 = "Part2";
			line2.WI_PartAttrib3 = "Part3";
			line2.WI_TotalUnits = 1;
			line3.WI_SerialNumber = "S/N 3";
			line3.WI_PartAttrib1 = "Part1";
			line3.WI_PartAttrib2 = "Part2";
			line3.WI_PartAttrib3 = "Part3";
			line3.WI_TotalUnits = 1;
			line4.WI_SerialNumber = "S/N 4";
			line4.WI_PartAttrib1 = "Part1";
			line4.WI_PartAttrib2 = "Part2";
			line4.WI_PartAttrib3 = "Part3";
			line4.WI_TotalUnits = 1;

			var receiveWrapper2 = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Inventory should roll up all the lines into one.", 1, receiveWrapper2.PalletizedInventory.Count);
			AssertEquals("Multiple serial #'s should be represented as 'Multiple'", "Multiple", receiveWrapper2.PalletizedInventory[0].TrackedSerialNumber);
			AssertEquals("PartAttrib1 should not have changed", "Part1", receiveWrapper2.PalletizedInventory[0].PartAttribute1);
			AssertEquals("PartAttrib2 should not have changed", "Part2", receiveWrapper2.PalletizedInventory[0].PartAttribute2);
			AssertEquals("PartAttrib3 should not have changed", "Part3", receiveWrapper2.PalletizedInventory[0].PartAttribute3);
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			var receive = Factory.New<WhsReceive>();
			var wrapper = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("Receive", wrapper.JobType);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehouseReceiveWrapper((WhsReceive)bizO, factory);
		}

		protected override WhsDocket GetNewDocketInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsReceive>();
		}

		#endregion

		#region TestJobLinesVariances

		protected override void TestJobLinesVariancesCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			data.Part2.OP_Weight = 3m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			Factory.Save();

			var wrapperWithVariances = new WarehouseReceiveWrapper(receive, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, wrapperWithVariances.JobLinesVariances.Count);
		}

		#endregion

		#region Implementation

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseReceiveWrapper((WhsReceive)bizO, Factory);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsReceive>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			return new WarehouseReceiveWrapper(receive, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return string.Format(@"
CarrierAccount :  is null
CarrierServiceLevel : 
Client : CLIENT\n#1
ClientRequestedBillToParty :  is null
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee : WAREHOUSEADDRESS\n#2 PATH=WAREHOUSE.WAREHOUSEADDRESS.HEADER
ConsigneeAddress :  is null
Consignor : 
CubicSent :  is null
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode : 
DropOffAddress : 
FinalisedDate : 
Forwarder : 
FulfillRule :  is null
GoodsBillToAddress : 
HandlingInstructions : 
IncoTerm :  is null
Insurance :  is null
JobClient : CLIENT\n#1
PackagesSent :  is null
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption :  is null
PickUpAddress : 
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : CN0L31M3TCKBEDMMWA2H14DUEUMDNXSGDHW
ServiceLevel : 
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : New Entry (Unsaved)
Supplier : 
SupplierBuyerLink :  is null
SupplierDocAddress : 
TotalExtendedLinePrice :  is null
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress :  is null
TransportCoAddress : 
TransportCompany : 
TransportReference : 
VehicleReference : 
Warehouse : 
WarehouseName : 
WeightSent :  is null
WhoCreated : {0}
WhoFinalised :
", Env.CurrentUser.Initials);
			}
		}

		#endregion
	}
}
