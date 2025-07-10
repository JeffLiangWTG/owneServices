using System.Linq;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageAuditFailureWrapper))]
	sealed class PackageAuditFailureWrapperTest : GenericWrapperTest
	{
		#region GenericWrapperOverride

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PackageAuditFailureWrapper            (Default Field: PartDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
AuditedQuantity                         Decimal
ExpectedQuantity                        Decimal
Location                                String
PartDescription                         String
PartNum                                 String
Picker                                  String
ProductBarcode                          String
Variance                                Decimal
";
			}
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new PackageAuditFailureWrapper(null, Factory);
			AssertEquals("ExpectedQuantity", 0m, wrapperEmpty.ExpectedQuantity);
			AssertEquals("AuditedQuantity", 0m, wrapperEmpty.AuditedQuantity);
			AssertEquals("Variance", 0m, wrapperEmpty.Variance);
			AssertEquals("PartNum", "", wrapperEmpty.PartNum);
			AssertEquals("PartDescription", "", wrapperEmpty.PartDescription);
			AssertEquals("Picker", "", wrapperEmpty.Picker);
			AssertEquals("ProductBarcode", "", wrapperEmpty.ProductBarcode);
			AssertEquals("Location", "", wrapperEmpty.Location);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageAuditFailureWrapper(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new PackageAuditFailureWrapper(null, Factory);
		}

		#endregion

		#region TestProperties

		#region TestBasicProperties

		public void TestBasicProperties()
		{
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "REC1", Data.Part1, 100m);
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			AssertNotNull("Precondition: ", audit.PackageAuditFailureLines.Single());

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			CombineAssertions("Error testing basic properties with audit failure values.", () =>
			{
				AssertEquals("ExpectedQuantity is incorrect", 50m, wrapper.ExpectedQuantity);
				AssertEquals("AuditedQuantity is incorrect", 49m, wrapper.AuditedQuantity);
				AssertEquals("Variance is incorrect", -1m, wrapper.Variance);
				AssertEquals("PartNum is incorrect", Data.Part1.OP_PartNum, wrapper.PartNum);
				AssertEquals("PartDescription is incorrect", Data.Part1.OP_Desc, wrapper.PartDescription);
			});
		}

		#endregion

		#region TestPicker

		public void TestGetPickerSingle()
		{
			var receiveLine1 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec1", Data.Part1, 50m);
			var receiveLine2 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec2", Data.Part1, 50m);
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 100m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = Helper.CreateWhsPickLine(order.Lines.Single(), receiveLine1.Lines[0].Inventory[0], 50m);
			var pickLine2 = Helper.CreateWhsPickLine(order.Lines.Single(), receiveLine2.Lines[0].Inventory[0], 50m);
			pickLine1.WZ_GS_NKAssignedTo = "USR";
			pickLine2.WZ_GS_NKAssignedTo = "USR";

			var divot1 = PackingHelper.CreatePackageDivot(package, pickLine1);
			var divot2 = PackingHelper.CreatePackageDivot(package, pickLine2);
			AssertEquals("Precondition: Package must have 2 divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Incorrect inventory in package divot1", receiveLine1.Inventory.Single().PK, ((WhsPickLine)divot1.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Incorrect inventory in package divot2", receiveLine2.Inventory.Single().PK, ((WhsPickLine)divot2.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Incorrect picker in package divot", "USR", ((WhsPickLine)divot1.PackedItem).WZ_GS_NKAssignedTo);
			AssertEquals("Precondition: Incorrect picker in package divot", "USR", ((WhsPickLine)divot2.PackedItem).WZ_GS_NKAssignedTo);

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			AssertEquals("Picker is incorrect.", "USR", wrapper.Picker);
		}

		public void TestGetPickerMultiple()
		{
			var receiveLine1 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec1", Data.Part1, 50m);
			var receiveLine2 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec2", Data.Part1, 50m);
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 100m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = Helper.CreateWhsPickLine(order.Lines.Single(), receiveLine1.Lines[0].Inventory[0], 50m);
			var pickLine2 = Helper.CreateWhsPickLine(order.Lines.Single(), receiveLine2.Lines[0].Inventory[0], 50m);
			pickLine1.WZ_GS_NKAssignedTo = "US1";
			pickLine2.WZ_GS_NKAssignedTo = "US2";

			var divot1 = PackingHelper.CreatePackageDivot(package, pickLine1);
			var divot2 = PackingHelper.CreatePackageDivot(package, pickLine2);
			AssertEquals("Precondition: Package must have 2 divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Incorrect inventory in package divot1", receiveLine1.Inventory.Single().PK, ((WhsPickLine)divot1.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Incorrect inventory in package divot2", receiveLine2.Inventory.Single().PK, ((WhsPickLine)divot2.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Incorrect picker in package divot1", "US1", ((WhsPickLine)divot1.PackedItem).WZ_GS_NKAssignedTo);
			AssertEquals("Precondition: Incorrect picker in package divot2", "US2", ((WhsPickLine)divot2.PackedItem).WZ_GS_NKAssignedTo);

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			AssertEquals("Picker is incorrect.", "MULTIPLE", wrapper.Picker);
		}

		public void TestGetPickerDeletedPackage()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 100m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			order.PackageJob.Packages.DeleteAll();
			AssertEquals("Precondition: Package must have 0 divots.", 0, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Order must not be picked.", false, order.IsAttachedToPick);
			AssertEquals("Precondition: Order should not have any packages.", 0, order.PackageJob.Packages.Count);

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			AssertNullOrEmpty("Picker must be empty since we don't have pick lines or package.", wrapper.Picker);
		}

		public void TestGetPickerSingle_InTransit()
		{
			var staff1 = Helper.CreateGlbStaff("USR", "User");
			var staff2 = Helper.CreateGlbStaff("OTH", "Other");
			var receive1 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec1", Data.Part1, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec2", Data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = order.Lines[0].PickLines.Single(pl => pl.WZ_WE_InventoryLine == receive1.Lines[0].PK);
			var pickLine2 = order.Lines[0].PickLines.Single(pl => pl.WZ_WE_InventoryLine == receive2.Lines[0].PK);
			pickLine1.WZ_GS_NKAssignedTo = "USR";
			pickLine2.WZ_GS_NKAssignedTo = "USR";
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);

			var divot1 = PackingHelper.CreatePackageDivot(package, pickLine1);
			var divot2 = PackingHelper.CreatePackageDivot(package, pickLine2);
			AssertEquals("Precondition: Package must have 2 divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Correct inventory in package divot1", receive1.Inventory.Single().PK, ((WhsPickLine)divot1.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Correct inventory in package divot2", receive2.Inventory.Single().PK, ((WhsPickLine)divot2.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Correct picker in package divot", "USR", ((WhsPickLine)divot1.PackedItem).WZ_GS_NKAssignedTo);
			AssertEquals("Precondition: Correct picker in package divot", "USR", ((WhsPickLine)divot2.PackedItem).WZ_GS_NKAssignedTo);

			var now = ZDateTimeOffset.Now;
			var inTransitLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, now);
			var inTransitLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, now);

			pickLine1.WZ_GS_NKAssignedTo = "OTH";
			pickLine2.WZ_GS_NKAssignedTo = "OTH";
			var newPickLine1 = inTransitLine1.PickLines.Single();
			var newPickLine2 = inTransitLine2.PickLines.Single();
			newPickLine1.WZ_GS_NKAssignedTo = "USR";
			newPickLine2.WZ_GS_NKAssignedTo = "USR";

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			AssertEquals("Picker is incorrect.", "USR", wrapper.Picker);
		}

		#endregion

		#region TestProductBarcode

		public void TestGetProductBarcode()
		{
			var order = Helper.CreateWhsOrder(Data.Org1, Data.Whs1);
			var package = PackingHelper.CreatePackage(order, "PKG1", 1, "PLT");
			var audit = helper.CreateWhsPackageAudit(package);
			var auditLineBarcoded = Helper.CreateWhsPackageAuditLineFailure(audit, Data.Part1, 50m, 49m);
			var auditLineNoBarcode = Helper.CreateWhsPackageAuditLineFailure(audit, Data.Part2, 10m, 9m);
			Helper.CreateProductUnit(Data.Part1, "UNT", "PLT", 10m);
			var barcode = Helper.CreateProductBarcode(Data.Part1, "UNT", "1234567890A");
			barcode.PH_UseForDocuments = true;

			var wrapper1 = new PackageAuditFailureWrapper(auditLineBarcoded, Factory);
			var wrapper2 = new PackageAuditFailureWrapper(auditLineNoBarcode, Factory);
			AssertEquals("Barcode for Part1 is incorrect.", new TextBarcode("1234567890A", true, true).TextAs128sFontString, wrapper1.ProductBarcode);
			AssertEquals("Barcode for Part2 is incorrect.", new TextBarcode(Data.Part2.OP_PartNum, true, true).TextAs128sFontString, wrapper2.ProductBarcode);
		}

		public void TestGetProductBarcodeNoPackagesFound()
		{
			var order = Helper.CreateWhsOrder(Data.Org1, Data.Whs1);
			var package = PackingHelper.CreatePackage(order, "PKG1", 1, "PLT");
			var audit = helper.CreateWhsPackageAudit(package);
			var auditLine = Helper.CreateWhsPackageAuditLineFailure(audit, Data.Part1, 50m, 49m);
			Helper.CreateProductUnit(Data.Part1, "UNT", "PLT", 10m);
			Helper.CreateProductBarcode(Data.Part1, "PLT", "1234567890A");
			order.PackageJob.Packages.DeleteAll();
			AssertEquals("Precondition: Order should not have any packages.", 0, order.PackageJob.Packages.Count);

			var wrapper = new PackageAuditFailureWrapper(auditLine, Factory);
			AssertEquals("Barcode must correspond to the part number.", new TextBarcode(Data.Part1.OP_PartNum, true, true).TextAs128sFontString, wrapper.ProductBarcode);
		}

		#endregion

		#region TestLocation

		public void TestGetLocationSingle()
		{
			var receiveLine1 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec1", Data.Part1, 50m, Data.Whs1.FindLocation("A-1-1"), "123");
			var receiveLine2 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec2", Data.Part1, 50m, Data.Whs1.FindLocation("A-1-1"), "456");
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 100m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = Helper.CreateWhsPickLine(order.Lines.Single(), receiveLine1.Lines[0].Inventory[0], 50m);
			var pickLine2 = Helper.CreateWhsPickLine(order.Lines.Single(), receiveLine2.Lines[0].Inventory[0], 50m);
			var divot1 = PackingHelper.CreatePackageDivot(package, pickLine1);
			var divot2 = PackingHelper.CreatePackageDivot(package, pickLine2);
			AssertEquals("Precondition: Package must have 2 divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Incorrect inventory in package divot1", receiveLine1.Inventory.Single().PK, ((WhsPickLine)divot1.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Incorrect inventory in package divot2", receiveLine2.Inventory.Single().PK, ((WhsPickLine)divot2.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Incorrect location in package divot1", "A-1-1", ((WhsPickLine)divot1.PackedItem).InventoryLine.LocationString);
			AssertEquals("Precondition: Incorrect location in package divot2", "A-1-1", ((WhsPickLine)divot2.PackedItem).InventoryLine.LocationString);

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			AssertEquals("Location is incorrect.", "A-1-1", wrapper.Location);
		}

		public void TestGetLocationMultiple()
		{
			var receiveLine1 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec1", Data.Part1, 50m, Data.Whs1.FindLocation("A-1-1"), "123");
			var receiveLine2 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec2", Data.Part1, 50m, Data.Whs1.FindLocation("A-2-2"), "456");
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 100m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = Helper.CreateWhsPickLine(order.Lines.Single(), receiveLine1.Lines[0].Inventory[0], 50m);
			var pickLine2 = Helper.CreateWhsPickLine(order.Lines.Single(), receiveLine2.Lines[0].Inventory[0], 50m);
			var divot1 = PackingHelper.CreatePackageDivot(package, pickLine1);
			var divot2 = PackingHelper.CreatePackageDivot(package, pickLine2);
			AssertEquals("Precondition: Package must have 2 divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Incorrect inventory in package divot1", receiveLine1.Inventory.Single().PK, ((WhsPickLine)divot1.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Incorrect inventory in package divot2", receiveLine2.Inventory.Single().PK, ((WhsPickLine)divot2.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Incorrect location in package divot1", "A-1-1", ((WhsPickLine)divot1.PackedItem).InventoryLine.LocationString);
			AssertEquals("Precondition: Incorrect location in package divot2", "A-2-2", ((WhsPickLine)divot2.PackedItem).InventoryLine.LocationString);

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			AssertEquals("Location is incorrect.", "MULTIPLE", wrapper.Location);
		}

		public void TestGetLocationDeletedPackage()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 100m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			order.PackageJob.Packages.DeleteAll();
			AssertEquals("Precondition: Package must have NO divots.", 0, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Order must not be picked.", false, order.IsAttachedToPick);
			AssertEquals("Precondition: Order should not have any packages.", 0, order.PackageJob.Packages.Count);

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			AssertNullOrEmpty("Location must be empty since we have no pick lines.", wrapper.Location);
		}

		public void TestGetLocationSingle_InTransit()
		{
			var receive1 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec1", Data.Part1, 50m, Data.Whs1.FindLocation("A-1-1"), "123");
			var receive2 = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "Rec2", Data.Part1, 50m, Data.Whs1.FindLocation("A-1-1"), "456");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = order.Lines[0].PickLines.Single(pl => pl.WZ_WE_InventoryLine == receive1.Lines[0].PK);
			var pickLine2 = order.Lines[0].PickLines.Single(pl => pl.WZ_WE_InventoryLine == receive2.Lines[0].PK);

			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			var divot1 = PackingHelper.CreatePackageDivot(package, pickLine1);
			var divot2 = PackingHelper.CreatePackageDivot(package, pickLine2);
			AssertEquals("Precondition: Package must have 2 divots.", 2, package.PackedItemDivots.Count);
			AssertEquals("Precondition: Correct inventory in package divot1", receive1.Inventory.Single().PK, ((WhsPickLine)divot1.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Correct inventory in package divot2", receive2.Inventory.Single().PK, ((WhsPickLine)divot2.PackedItem).InventoryLine.PK);
			AssertEquals("Precondition: Correct location in package divot1", "A-1-1", ((WhsPickLine)divot1.PackedItem).InventoryLine.LocationString);
			AssertEquals("Precondition: Correct location in package divot2", "A-1-1", ((WhsPickLine)divot2.PackedItem).InventoryLine.LocationString);

			var now = ZDateTimeOffset.Now;
			Helper.PickAndMakeInTransitTransfer(pickLine1, now);
			Helper.PickAndMakeInTransitTransfer(pickLine2, now);

			var wrapper = new PackageAuditFailureWrapper(audit.PackageAuditFailureLines.Single(), Factory);
			AssertEquals("Location is incorrect.", "A-1-1", wrapper.Location);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}
		PackingTestHelper packingHelper;

		TestDataSimpleEnvironment Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment(Factory, 2, 2)); }
		}
		TestDataSimpleEnvironment data;

		#endregion
	}
}
