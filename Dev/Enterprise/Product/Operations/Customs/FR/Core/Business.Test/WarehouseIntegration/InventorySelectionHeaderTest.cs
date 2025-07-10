using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeader))]
	public sealed class InventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFillInventoryDetails_PreviousDocuments()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				WhsHelper.EnableWarehouseForBond(WhsWarehouse, true);
				Helper.WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = Helper.WhsHelper.CreateWhsArea(WhsWarehouse.PK, "IPR", "IPR");
				var row = Helper.WhsHelper.CreateRowAndGenerateLocations(WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var spoke = Helper.CreateProduct(Importer.PK, "SPOKE");
				var hub = Helper.CreateProduct(Importer.PK, "WHEEL HUB");
				var wheel = Helper.CreateProduct(Importer.PK, "WHEEL");
				var frame = Helper.CreateProduct(Importer.PK, "FRAME");
				var bike = Helper.CreateProduct(Importer.PK, "BIKE");

				WhsHelper.CreateProductBOM(wheel.PK, hub.PK, 1m);
				WhsHelper.CreateProductBOM(wheel.PK, spoke.PK, 20m);
				WhsHelper.CreateProductBOM(bike.PK, frame.PK, 1m);
				WhsHelper.CreateProductBOM(bike.PK, wheel.PK, 2m);

				var componentsReceive = Helper.GetNewWhsReceive(WhsWarehouse.PK, Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
				componentsReceive.WD_IsInwardsProcessingJob = true;
				var spokesReceiveLine = Helper.GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, spoke.PK, vfd: 10000m, qty: 10000m, location.PK, 1, "EN00123");
				spokesReceiveLine.CustomsData.WB_EntryDate = new ZDateTime(2023, 10, 16);
				var hubsReceiveLine = Helper.GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, hub.PK, vfd: 60000m, qty: 300m, location.PK, 2, "EN00123");
				hubsReceiveLine.CustomsData.WB_EntryDate = new ZDateTime(2023, 10, 17);
				var framesReceiveLine = Helper.GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, frame.PK, vfd: 50000m, qty: 500m, location.PK, 3, "EN00123");
				framesReceiveLine.CustomsData.WB_EntryDate = new ZDateTime(2023, 10, 18);

				componentsReceive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				Assert("Precondition: Receive is finalised", !componentsReceive.WD_FinalisedDate.IsEmpty);

				Helper.CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, wheel.PK, 200m);
				Helper.CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, bike.PK, 10m);

				var bikeInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, bike.PK));
				var bikeInventoryLine = Factory.Load<IWhsDocketLine>(bikeInventory.WI_WE_InDocketLine);
				var inventorySelectionHeader = new InventorySelectionHeader(Declaration);
				Factory.Save();

				var wrapper = new WhsInventoryWrapper(bikeInventory, inventorySelectionHeader);
				wrapper.QuantityToDraw = 1;
				inventorySelectionHeader.SelectedLines.Add(wrapper);

				inventorySelectionHeader.ImportInventories();
				var invoiceLine = Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Single();

				var prevDocuments = invoiceLine.PreviousDocuments.Select(x => new { x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_DateOfIssue });
				var expectedResult = new[] {
					new { CSI_Code = (ZString)PreviousDocumentCodeList.Codes.IM, CSI_ReferenceNumber = (ZString)"EN00123", CSI_DateOfIssue = new ZDateTime(2023, 10, 16) },
					new { CSI_Code = (ZString)PreviousDocumentCodeList.Codes.IM, CSI_ReferenceNumber = (ZString)"EN00123", CSI_DateOfIssue = new ZDateTime(2023, 10, 17) },
					new { CSI_Code = (ZString)PreviousDocumentCodeList.Codes.IM, CSI_ReferenceNumber = (ZString)"EN00123" , CSI_DateOfIssue = new ZDateTime(2023, 10, 18) },
				};

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Select Inventory - Matches Lowest Level Components", expectedResult, prevDocuments);

					invoiceLine.PreviousDocuments.RemoveAndDelete(invoiceLine.PreviousDocuments.Cast<BusinessObject>().Skip(1).First());

					AssertEquals("Prerequisite: 2 items left of 3", 2, invoiceLine.PreviousDocuments.Count);
					AssertEquals("No Errors on Update", ZString.Empty, inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>()));
					AssertContainsExactElementsInAnyOrder("Synchronise with Inventory - Missing PrevDocuments readded", expectedResult, prevDocuments);
				});
			}
		}

		public void TestPopulateComponentInventory()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var inventorySelectionHeader = new InventorySelectionHeader(Declaration);
				var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
				var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);

				IWhsInventoryView whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE2", 1m, 100m, 100m, bondedEntryKey: "BN-EntryKey-1");
				IWhsInventoryView whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE3", 1m, 100m, 100m, bondedEntryKey: "BN-EntryKey-2");
				whsInventory2.WI_AllocationKey = "WI-AllocationKey";

				var newSelectedLine1 = new WhsInventoryWrapper(whsInventory1, inventorySelectionHeader);
				newSelectedLine1.QuantityToDraw = 10m;
				var newSelectedLine2 = new WhsInventoryWrapper(whsInventory2, inventorySelectionHeader);
				newSelectedLine2.QuantityToDraw = 10m;
				inventorySelectionHeader.SelectedLines.Add(newSelectedLine1);
				inventorySelectionHeader.SelectedLines.Add(newSelectedLine2);

				Factory.Save();

				inventorySelectionHeader.ImportInventories();
				var invoiceLines = declaration.InvoiceLines;

				AssertEquals("Invoice lines", 2, invoiceLines.Count);

				CombineAssertions("Component Inventory should not be populated when the product inventory has an empty WI_AllocationKey", () =>
				{
					AssertNullOrEmpty(whsInventory1.WI_AllocationKey);
					AssertEquals(0, invoiceLines[0].ComponentInventoryCollection.Count);
				});

				CombineAssertions("Component Inventory should be populated when the product inventory has a non-empty WI_AllocationKey", () =>
				{
					AssertNotNullOrEmpty(whsInventory2.WI_AllocationKey);
					AssertEquals(1, invoiceLines[1].ComponentInventoryCollection.Count);
				});
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InventorySelectionHeader(Factory.New<BaseJobDeclaration>());
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;
		IWhsTransactionTestHelper WhsHelper => Helper.WhsHelper;
		IWhsWarehouse WhsWarehouse => Helper.WhsWarehouse;
		OrgHeader Importer => Helper.Importer;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_OH_Importer = Helper.Importer.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
	}
}
