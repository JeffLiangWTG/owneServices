using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
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
				var hubsReceiveLine = Helper.GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, hub.PK, vfd: 60000m, qty: 300m, location.PK, 2, "EN00123");
				var framesReceiveLine = Helper.GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, frame.PK, vfd: 50000m, qty: 500m, location.PK, 3, "EN00123");

				componentsReceive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				Assert("Precondition: Receive is finalised", !componentsReceive.WD_FinalisedDate.IsEmpty);

				Helper.CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, wheel.PK, 200m);
				Helper.CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, bike.PK, 10m);

				var bikeInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, bike.PK));
				var bikeInventoryLine = Factory.Load<IWhsDocketLine>(bikeInventory.WI_WE_InDocketLine);

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_OH_Importer = Helper.Importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;

				var inventorySelectionHeader = new InventorySelectionHeader(declaration);
				Factory.Save();

				var wrapper = new WhsInventoryWrapper(bikeInventory, inventorySelectionHeader);
				wrapper.QuantityToDraw = 1;
				inventorySelectionHeader.SelectedLines.Add(wrapper);
				inventorySelectionHeader.ImportInventories();

				var invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Single();

				var prevDocuments = invoiceLine.PreviousDocuments.Select(x => new { x.CSI_ItemNumber, x.CSI_ReferenceNumber });
				var expectedResult = new[] {
					new { CSI_ItemNumber = (ZInt)1, CSI_ReferenceNumber = (ZString)"EN00123" },
					new { CSI_ItemNumber = (ZInt)2, CSI_ReferenceNumber = (ZString)"EN00123" },
					new { CSI_ItemNumber = (ZInt)3, CSI_ReferenceNumber = (ZString)"EN00123" },
				};

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Select Inventory - Matches Lowest Level Components", expectedResult, prevDocuments);

					invoiceLine.PreviousDocuments.RemoveAndDelete(invoiceLine.PreviousDocuments.Cast<BusinessObject>().Skip(1).First());

					AssertEquals("Prerequisite: 2 items left of 3", 2, invoiceLine.PreviousDocuments.Count);
					AssertEquals("No Errors on Update", ZString.Empty, inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>()));
					AssertContainsExactElementsInAnyOrder("Synchronise with Inventory - Missing PrevDocuments readded", expectedResult, prevDocuments);
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
	}
}
