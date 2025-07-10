using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsTransferLine))]
	sealed class DocWhsTransferLineTest : DocWhsDocketLineTest<WhsTransfer, WhsTransferLine, DocWhsTransferLine>
	{
		#region Related Business Objects

		public void TestDocket()
		{
			AssertEquals(typeof(DocWhsTransfer), DocketLineWrapper.Docket.GetType());
		}

		#endregion

		#region Properties

		#region ZDateTime Fields

		public void TestArrivalDate()
		{
			var date = ZDateTime.Today.AddDays(1);
			DocketLine.WE_AdjustmentArrivalDate = date.ToOffset();
			AssertEquals(date, DocketLineWrapper.ArrivalDate);
		}

		#endregion

		#region ZString Fields

		public void TestFromWarehouseCaption()
		{
			AssertEquals("Source Warehouse", DocketLineWrapper.FromWarehouseCaption);
		}

		public void TestFromLocationCaption()
		{
			AssertEquals("Source Location", DocketLineWrapper.FromLocationCaption);
		}

		public void TestDestWarehouseCaption()
		{
			AssertEquals("Dest. Warehouse", DocketLineWrapper.DestWarehouseCaption);
		}

		public void TestDestLocationCaption()
		{
			AssertEquals("Dest. Location", DocketLineWrapper.DestLocationCaption);
		}

		public void TestTransferFromWarehouseName()
		{
			AssertEquals(ZString.Empty, DocketLineWrapper.TransferFromWarehouseName);

			DocketLine.WE_WD = Factory.New<WhsTransfer>().PK;
			AssertEquals(ZString.Empty, DocketLineWrapper.TransferFromWarehouseName);

			WhsWarehouse warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TST";
			DocketLine.Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(ZString.Empty, DocketLineWrapper.TransferFromWarehouseName);

			DocketLine.Docket.WD_DocketSubType = Enterprise.Warehouse.Transactions.CodeLists.TransferType.Codes.InterWhsSource;
			AssertEquals("TST", DocketLineWrapper.TransferFromWarehouseName);
		}

		public void TestTransferFromWarehouseName_Translatable()
		{
			DocketLine.WE_WD = Factory.New<WhsTransfer>().PK;
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TEST";
			DocketLine.Docket.WD_WW_Whs = warehouse.PK;
			DocketLine.Docket.WD_DocketSubType = Enterprise.Warehouse.Transactions.CodeLists.TransferType.Codes.InterWhsSource;
			AssertEquals("TransferFromWarehouseName in English", "TEST", DocketLineWrapper.TransferFromWarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("TransferFromWarehouseName in Chinese", "测试仓库", DocketLineWrapper.TransferFromWarehouseName);
			}
		}

		public void TestTransferToWarehouseName()
		{
			AssertEquals(ZString.Empty, DocketLineWrapper.TransferToWarehouseName);

			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TST";
			Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(ZString.Empty, DocketLineWrapper.TransferFromWarehouseName);
		}

		public void TestTransferToWarehouseName_Translatable()
		{
			DocketLine.WE_WD = Factory.New<WhsTransfer>().PK;
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TEST";
			DocketLine.Docket.WD_WW_Whs = warehouse.PK;
			DocketLine.Docket.WD_DocketSubType = Enterprise.Warehouse.Transactions.CodeLists.TransferType.Codes.InterWhsSource;
			AssertEquals("TransferToWarehouseName in English", "", DocketLineWrapper.TransferToWarehouseName);
		}

		public void TestFromPalletIDCaption()
		{
			AssertEquals("Source Pallet ID", DocketLineWrapper.FromPalletIDCaption);
		}

		public void TestDestPalletIDCaption()
		{
			AssertEquals("Dest. Pallet ID", DocketLineWrapper.DestPalletIDCaption);
		}

		public void TestPalletID1()
		{
			DocketLine.WE_TransferFromPalletId = "P_ID_001";
			AssertEquals("P_ID_001", DocketLineWrapper.PalletID1);
		}

		public void TestPalletID2()
		{
			DocketLine.WE_PalletID = "P_ID_002";
			AssertEquals("P_ID_002", DocketLineWrapper.PalletID2);
		}

		#endregion

		#region ZDecimal Fields

		#region TestPackQtyCore

		protected override void TestPackQtyCore()
		{
			Docket.Delete();    // Setup in base class, but causing problems in this test
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			var transferLineWrapper = DocWhsTransferLine.New(transferLine, Factory);
			AssertEquals("Packs should include packs from matching lines.", 15m, transferLineWrapper.PackQty);
		}

		#endregion

		#region TestPacksCore

		protected override void TestPacksCore()
		{
			Docket.Delete();  // Setup in base class, but causing problems in this test
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			var transferLineWrapper = DocWhsTransferLine.New(transferLine, Factory);
			AssertEquals("Packs should include packs from matching lines.", 15m, transferLineWrapper.Packs);
		}

		#endregion

		#region TestUnitsCore

		protected override void TestUnitsCore()
		{
			Docket.Delete(); // Setup in base class, but causing problems in this test
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			var transferLineWrapper = DocWhsTransferLine.New(transferLine, Factory);
			AssertEquals("Units should include units from matching lines.", 15m, transferLineWrapper.Units);
		}

		#endregion

		#endregion

		#endregion

		#region Implementation

		protected override DocWhsTransferLine CreateDocketLineWrapper(WhsTransferLine docketLine)
		{
			return DocWhsTransferLine.New(docketLine, Factory);
		}

		#endregion
	}
}
