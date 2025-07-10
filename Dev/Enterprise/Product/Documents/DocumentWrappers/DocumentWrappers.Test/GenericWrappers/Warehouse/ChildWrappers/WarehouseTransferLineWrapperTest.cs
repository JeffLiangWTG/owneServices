using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseTransferLineWrapper))]
	sealed class WarehouseTransferLineWrapperTest : WarehouseDocketLineWrapperTest
	{
		#region Properties

		#region TestFromWarehouseCaption

		public void TestFromWarehouseCaption()
		{
			AssertEquals("Source Warehouse", TransferLineWrapper.FromWarehouseCaption);
		}

		#endregion

		#region TestFromLocationCaption

		public void TestFromLocationCaption()
		{
			AssertEquals("Source Location", TransferLineWrapper.FromLocationCaption);
		}

		#endregion

		#region TestDestWarehouseCaption

		public void TestDestWarehouseCaption()
		{
			AssertEquals("Dest. Warehouse", TransferLineWrapper.DestWarehouseCaption);
		}

		#endregion

		#region TestDestLocationCaption

		public void TestDestLocationCaption()
		{
			AssertEquals("Dest. Location", TransferLineWrapper.DestLocationCaption);
		}

		#endregion

		#region TestFromPalletIDCaption

		public void TestFromPalletIDCaption()
		{
			AssertEquals("Source Pallet ID", TransferLineWrapper.FromPalletIDCaption);
		}

		#endregion

		#region TestDestPalletIDCaption

		public void TestDestPalletIDCaption()
		{
			AssertEquals("Dest. Pallet ID", TransferLineWrapper.DestPalletIDCaption);
		}

		#endregion

		#region TestTransferFromWarehouseName

		public void TestTransferFromWarehouseName()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.TransferFromWarehouseName);

			WhsWarehouse warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TST";
			TransferLine.Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals("TST", TransferLineWrapper.TransferFromWarehouseName);
		}

		public void TestTransferFromWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TEST";
			TransferLine.Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals("TransferFromWarehouseName in English.", "TEST", TransferLineWrapper.TransferFromWarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("TransferFromWarehouseName in Chinese.", "测试", TransferLineWrapper.TransferFromWarehouseName);
			}
		}

		#endregion

		#region TestTransferToWarehouseName

		public void TestTransferToWarehouseName()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.TransferToWarehouseName);

			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TST";
			Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals("TST", TransferLineWrapper.TransferToWarehouseName);
		}

		public void TestTransferToWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TEST";
			Docket.WD_WW_Whs = warehouse.PK;
			AssertEquals("TransferToWarehouseName in English.", "TEST", TransferLineWrapper.TransferToWarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("TransferToWarehouseName in Chinese.", "测试", TransferLineWrapper.TransferToWarehouseName);
			}
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			TransferLine.WE_TransferFromPalletId = "P_ID_001";
			AssertEquals("P_ID_001", TransferLineWrapper.PalletID);
		}

		#endregion

		#region TestPalletID2

		public void TestPalletID2()
		{
			TransferLine.WE_PalletID = "P_ID_002";
			AssertEquals("P_ID_002", TransferLineWrapper.PalletID2);
		}

		#endregion

		#region TestPackType

		public void TestPackType()
		{
			TransferLine.WE_F3_NKPackType = "PLT";
			AssertEquals("PLT", TransferLineWrapper.PacksUQ);
		}

		#endregion

		#region TestLocationString

		protected override void AssertLocationString(WhsDocketLine line)
		{
			AssertEquals(((WhsTransferLine)line).TransferFromLocationString, GetNewWarehouseLineWrapper(line).LocationString);
		}

		#endregion

		#region TestLocationString2

		protected override void AssertLocationString2(WhsDocketLine line)
		{
			AssertEquals(line.LocationString, GetNewWarehouseLineWrapper(line).LocationString2);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			var part = Enterprise.Customs.Business.OrgSupplierPart.New(Factory);
			TransferLine.WE_OP = part.PK;
			part.OP_Desc = "123";
			AssertEquals("123", TransferLineWrapper.ProductDescription);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			var part = Enterprise.Customs.Business.OrgSupplierPart.New(Factory);
			TransferLine.WE_OP = part.PK;
			part.OP_PartNum = "123";
			AssertEquals("123", TransferLineWrapper.ProductCode);
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			var nullWrapper = new WarehouseTransferLineWrapper(null, Factory);
			AssertEquals("Pre-condition", "", nullWrapper.Status);

			var transferLine = Factory.New<WhsTransferLine>();
			var transferLineWrapper = new WarehouseTransferLineWrapper(transferLine, Factory);
			transferLine.WE_DocketLineStatus = "FIN";
			AssertEquals("FIN", transferLineWrapper.Status);
		}

		#endregion

		#region TestPacks

		public void TestPacks()
		{
			var nullWrapper = new WarehouseTransferLineWrapper(null, Factory);
			AssertEquals("Pre-condition", 0m, nullWrapper.Packs);

			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			var transferLineWrapper = new WarehouseTransferLineWrapper(transferLine, Factory);
			AssertEquals("Packs should include packs from matching lines.", 15m, transferLineWrapper.Packs);
		}

		#endregion

		#region TestUnits

		public void TestUnits()
		{
			var nullWrapper = new WarehouseTransferLineWrapper(null, Factory);
			AssertEquals("Pre-condition", 0m, nullWrapper.Units);

			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "");
			var matchingLine = Helper.CreateMatchingLine(transferLine, 5m);
			var transferLineWrapper = new WarehouseTransferLineWrapper(transferLine, Factory);
			AssertEquals("Packs should include packs from matching lines.", 15m, transferLineWrapper.Units);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTransferLine TransferLine
		{
			get { return (WhsTransferLine)DocketLine; }
		}

		WarehouseTransferLineWrapper TransferLineWrapper
		{
			get { return (WarehouseTransferLineWrapper)DocketLineWrapper; }
		}

		#region Captions

		protected override void TestWrapperMappingsEmpty_DestLocationCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.DestLocationCaption), "Dest. Location", emptyWrapper.DestLocationCaption);
		}

		protected override void TestWrapperMappingsEmpty_DestPalletIDCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.DestPalletIDCaption), "Dest. Pallet ID", emptyWrapper.DestPalletIDCaption);
		}

		protected override void TestWrapperMappingsEmpty_DestWarehouseCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.DestWarehouseCaption), "Dest. Warehouse", emptyWrapper.DestWarehouseCaption);
		}

		protected override void TestWrapperMappingsEmpty_FromLocationCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.FromLocationCaption), "Source Location", emptyWrapper.FromLocationCaption);
		}

		protected override void TestWrapperMappingsEmpty_FromPalletIDCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.FromPalletIDCaption), "Source Pallet ID", emptyWrapper.FromPalletIDCaption);
		}

		protected override void TestWrapperMappingsEmpty_FromWarehouseCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.FromWarehouseCaption), "Source Warehouse", emptyWrapper.FromWarehouseCaption);
		}

		#endregion

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CrossDockConsigneeAddress : 
DangerousGoodsSubstance :  is null
ExtendedLinePrice :  is null
ManufacturerAddress : 
Product : (No Default Field Value Available on Product)
RecommendedUnitPrice : 
Registry : (No Default Field Value Available on Registry)
UnitDiscountAmount : 
UnitDiscountPercent : 
UnitPriceAfterDiscount : 
UnitsMet : 
UnitsOrdered : 
UnitsPicked : 
UnitsShort :
";
			}
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return TransferLineWrapper;
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return TransferLineWrapper;
		}

		#endregion

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject whsLineBO)
		{
			return new WarehouseTransferLineWrapper((WhsTransferLine)whsLineBO, Factory);
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsTransfer>();
		}
	}
}
