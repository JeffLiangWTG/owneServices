using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public abstract class WarehouseIntegrationBaseOnlyTest<THeader> : TestCaseWithFactory
		where THeader : NctsHeader
	{
		[GuiTest]
		public void TestPublishShipmentForWHSOutward() => CombineAssertions(() =>
		{
			var helper = new WhsDataTestHelper(Factory);

			SetupOrdersStock(helper, Factory);

			var header = Factory.New<THeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var nctsBill = header.Bills.AddNew();
			var gi = nctsBill.GoodsItems.AddNew();
			gi.BY_BondedWhsQuantity = 50;
			gi.BY_BondedWhsUnitQty = "KG";
			gi.BY_WarehouseEntryNumber = "ENT1234";
			gi.BY_WarehouseEntryLineNo = 2;
			gi.BY_CommercialReferenceNumber = "ENT002";
			gi.BY_LineNo = 1;
			header.Consignor.E2_OA_Address = helper.Importer.MainAddress.PK;
			header.Consignee.E2_OA_Address = helper.Supplier.MainAddress.PK;
			helper.Warehouse.OH_IsWarehouseClient = true;
			var supporter = (IWarehouseIntegrationSupporter)header;

			gi.BY_OP_Part = helper.Part.PK;
			header.MovementHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			Factory.Save();
			var errors = supporter.PublishShipmentForWHSOutward(true);
			AssertEquals(ZString.Empty, errors.ErrorMessage);

			errors = supporter.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(shouldSave: true);
			Factory.Save();

			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1234-1", 100);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1234-2", 50);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT12341", 1, 100);

			AssertEquals(ZString.Empty, errors.ErrorMessage);
			var link = Factory.Load<IWhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, header.PK));
			AssertEquals("docket link for this header created", 1, link.Length);
		});

		public static void SetupOrdersStock(WhsDataTestHelper helper, BusinessObjectFactory factory)
		{
			helper.WhsHelper.CreateRowAndGenerateLocations(helper.WhsWarehouse, "A");
			factory.Save();

			var receive = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, "ref1");
			var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
			var locationAPK = helper.WhsHelper.FindLocation(helper.WhsWarehouse.PK, "A").PK;
			receiveLine.WE_WL = locationAPK;
			receiveLine.WE_CurrentInventoryStatus = "AVL";
			receiveLine.WE_OriginalInventoryStatus = "AVL";

			var receiveLine2 = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-2", ZDateTime.Today.AddMonths(-1));
			receiveLine2.WE_WL = locationAPK;
			receiveLine2.WE_CurrentInventoryStatus = "AVL";
			receiveLine2.WE_OriginalInventoryStatus = "AVL";

			receive.WD_DocketStatus = "FIN";
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receiveLine.WE_DocketLineStatus = "FIN";
			receiveLine2.WE_DocketLineStatus = "FIN";

			var receive2 = helper.GetNewWhsReceive(helper.WhsWarehouse.PK, helper.Importer.PK, "ref2");

			var receiveLine3 = helper.GetNewWhsReceiveLine(receive2.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT12341-1", ZDateTime.Today.AddMonths(-1));
			receiveLine3.WE_WL = locationAPK;
			receiveLine3.WE_CurrentInventoryStatus = "AVL";
			receiveLine3.WE_OriginalInventoryStatus = "AVL";

			receive2.WD_DocketStatus = "FIN";
			receive2.WD_FinalisedDate = ZDateTimeOffset.Now;
			receiveLine3.WE_DocketLineStatus = "FIN";
			factory.Save();

			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1234", 1, 100);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1234", 2, 100);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT12341", 1, 100);
		}
	}
}
