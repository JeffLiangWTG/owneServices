using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(WarehouseNctsHeaderSender))]
	sealed class WarehouseNctsHeaderSenderTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestPreSend()
		{
			var helper = new WhsDataTestHelper(Factory);
			WarehouseIntegrationTest.SetupOrdersStock(helper, Factory);

			var header = Factory.New<NctsHeader>();

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var nctsBill = header.Bills.AddNew();
			var gi = nctsBill.GoodsItems.AddNew();
			gi.BY_BondedWhsQuantity = 50;
			gi.BY_BondedWhsUnitQty = Core.Constants.Weight.Kilograms;
			gi.BY_WarehouseEntryNumber = "ENT1234";
			gi.BY_WarehouseEntryLineNo = 1;
			gi.BY_CommercialReferenceNumber = "ENT002";
			gi.BY_LineNo = 1;
			header.Consignor.E2_OA_Address = helper.Importer.MainAddress.PK;
			header.Consignee.E2_OA_Address = helper.Supplier.MainAddress.PK;
			helper.Warehouse.OH_IsWarehouseClient = true;
			header.MovementHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;

			gi.BY_OP_Part = helper.Part.PK;

			Factory.Save();

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;

			var result = WarehouseNctsHeaderSender.PreSend(header);
			AssertEquals("No Errors", null, messageInitiator.InvalidOperationText);
			AssertEquals("PreSend", true, result);
			var link = Factory.Load<IWhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, header.PK));
			AssertEquals("docket link for this header created", 1, link.Length);

			gi.BY_BondedWhsQuantity = 1000;
			result = WarehouseNctsHeaderSender.PreSend(header);
			AssertEquals("PreSend", false, result);
			AssertEquals("User notified of errors", true, messageInitiator.InvalidOperationText?.Contains("You do not have enough stock to fulfill shortfalls on this order"));
		}

		[GuiTest]
		public void TestPreSend_OutwardOrderImported()
		{
			var helper = new WhsDataTestHelper(Factory);
			WarehouseIntegrationTest.SetupOrdersStock(helper, Factory);

			var header = Factory.New<NctsHeader>();

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var nctsBill = header.Bills.AddNew();

			nctsBill.IsOutwardOrderImported = true;

			var gi = nctsBill.GoodsItems.AddNew();
			gi.BY_BondedWhsQuantity = 50;
			gi.BY_BondedWhsUnitQty = Core.Constants.Weight.Kilograms;
			gi.BY_WarehouseEntryNumber = "ENT1234";
			gi.BY_WarehouseEntryLineNo = 1;
			gi.BY_CommercialReferenceNumber = "ENT002";
			gi.BY_LineNo = 1;
			header.Consignor.E2_OA_Address = helper.Importer.MainAddress.PK;
			header.Consignee.E2_OA_Address = helper.Supplier.MainAddress.PK;
			helper.Warehouse.OH_IsWarehouseClient = true;
			header.MovementHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;

			gi.BY_OP_Part = helper.Part.PK;

			Factory.Save();

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;

			var result = WarehouseNctsHeaderSender.PreSend(header);

			CombineAssertions(() =>
			{
				AssertEquals("PreSend", true, result);
				AssertEquals("No Errors", null, messageInitiator.InvalidOperationText);
				var link = Factory.Load<IWhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, header.PK));
				AssertEquals("docket link for this header not created", 0, link.Length);

				var dataExportLogs = header.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DataExportCode);
				AssertEquals("Warehouse was not updated, no UXML sent.", 0, dataExportLogs.Count());
			});
		}

		public void TestPreSend_Without_WarehouseAddress()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;

			var result = WarehouseNctsHeaderSender.PreSend(header);
			AssertEquals("PreSend", true, result);
			AssertEquals("No Errors", null, messageInitiator.InvalidOperationText);
			var link = Factory.Load<IWhsDocketJobPivot>(new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, header.PK));
			AssertEquals("docket link for this header not created", 0, link.Length);
		}
	}
}
