using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemTransferLineWrapper))]
	sealed class WhsItemTransferLineWrapperTest : WarehouseGenericLineWrapperTest
	{
		#region Properties

		#region From

		#region TestFromLocationCaption

		public void TestFromLocationCaption()
		{
			AssertEquals("From Location", TransferLineWrapper.FromLocationCaption);
		}

		#endregion

		#region TestFromWarehouseCaption

		public void TestFromWarehouseCaption()
		{
			AssertEquals("From Warehouse", TransferLineWrapper.FromWarehouseCaption);
		}

		#endregion

		#region TestFromLocationString

		public void TestFromLocationString()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.LocationString);

			var location = Factory.NewWithValidTestData<WhsLocation>();
			TransferLine.WTF_WL_From = location.PK;
			AssertEquals(location.WLV_LocationString, TransferLineWrapper.LocationString);
		}

		#endregion

		#region TestTransferFromWarehouseName

		public void TestTransferFromWarehouseName()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.TransferFromWarehouseName);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			TransferHeader.WTH_WW_Warehouse = warehouse.PK;
			AssertEquals(warehouse.WW_WarehouseName, TransferLineWrapper.TransferFromWarehouseName);
		}

		public void TestTransferFromWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TEST";
			TransferHeader.WTH_WW_Warehouse = warehouse.PK;
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

		#endregion

		#region To

		#region TestToLocationCaption

		public void TestToLocationCaption()
		{
			AssertEquals("To Location", TransferLineWrapper.DestLocationCaption);
		}

		#endregion

		#region TestToWarehouseCaption

		public void TestToWarehouseCaption()
		{
			AssertEquals("To Warehouse", TransferLineWrapper.DestWarehouseCaption);
		}

		#endregion

		#region TestToLocationString

		public void TestToLocationString()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.LocationString2);

			var location = Factory.NewWithValidTestData<WhsLocation>();
			TransferLine.WTF_WL_To = location.PK;
			AssertEquals(location.WLV_LocationString, TransferLineWrapper.LocationString2);
		}

		#endregion

		#region TestTransferToWarehouseName

		public void TestTransferToWarehouseName()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.TransferToWarehouseName);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			TransferHeader.WTH_WW_Warehouse = warehouse.PK;
			AssertEquals(warehouse.WW_WarehouseName, TransferLineWrapper.TransferToWarehouseName);
		}

		public void TestTransferToWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "TEST";
			TransferHeader.WTH_WW_Warehouse = warehouse.PK;
			AssertEquals("TransferFromWarehouseName in English.", "TEST", TransferLineWrapper.TransferToWarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("TransferFromWarehouseName in Chinese.", "测试", TransferLineWrapper.TransferToWarehouseName);
			}
		}

		#endregion

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.Status);

			var packageState = Factory.NewWithValidTestData<WhsItemPackageState>();
			TransferLine.WTF_WPS_PackageState = packageState.PK;
			AssertEquals(packageState.WPS_Status, TransferLineWrapper.Status);
		}

		#endregion

		#region TestArrivalDate

		[TestDate(2020, 01, 01, 00, 00, 00)]
		public void TestArrivalDate()
		{
			AssertEquals(ZDateTime.Empty, TransferLineWrapper.ArrivalDate);

			GetPackageCore();
			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			rtu.WRH_UnloadCompleteTime = ZDateTimeOffset.Now;
			TransferLine.PackageState.WPS_WRH_TransitReceiveHeader = rtu.PK;

			AssertEquals(ZDateTimeOffset.Now.ToLocalZDateTime(), TransferLineWrapper.ArrivalDate);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.ProductCode);

			var package = GetPackageCore();
			AssertEquals(package.KP_PackageID, TransferLineWrapper.ProductCode);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.ProductDescription);

			var package = GetPackageCore();
			AssertEquals(package.KP_GoodsDescription, TransferLineWrapper.ProductDescription);
		}

		#endregion

		#region TestPacks

		public void TestPacks()
		{
			AssertEquals(ZDecimal.Zero, TransferLineWrapper.Packs);

			GetPackageCore();
			AssertEquals(1m, TransferLineWrapper.Packs);
		}

		#endregion

		#region TestPackType

		public void TestPackType()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.PacksUQ);

			var package = GetPackageCore();
			AssertEquals(package.KP_F3_NKPackType, TransferLineWrapper.PacksUQ);
		}

		#endregion

		#region TestUnits

		public void TestUnits()
		{
			AssertEquals(ZDecimal.Zero, TransferLineWrapper.Units);

			GetPackageCore();
			AssertEquals(1m, TransferLineWrapper.Units);
		}

		#endregion

		#region TestUnitType

		public void TestUnitType()
		{
			AssertEquals(ZString.Empty, TransferLineWrapper.UnitsUQ);

			var package = GetPackageCore();
			AssertEquals(package.KP_F3_NKPackType, TransferLineWrapper.UnitsUQ);
		}

		#endregion

		PkgPackage GetPackageCore()
		{
			var packageHeader = Factory.NewWithValidTestData<PkgPackageHeader>();
			var package = Factory.NewWithValidTestData<PkgPackage>();
			var packageState = Factory.NewWithValidTestData<WhsItemPackageState>();
			package.KP_KPH_PackageHeader = packageHeader.PK;
			packageState.WPS_KP_Package = package.PK;
			TransferLine.WTF_WPS_PackageState = packageState.PK;

			return package;
		}

		#endregion

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override bool CanSetProductAttributesInWrapperCore => false;

		#endregion

		#region Implementation

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

		#region Captions

		protected override void TestWrapperMappingsEmpty_DestLocationCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.DestLocationCaption), "To Location", emptyWrapper.DestLocationCaption);
		}

		protected override void TestWrapperMappingsEmpty_DestWarehouseCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.DestWarehouseCaption), "To Warehouse", emptyWrapper.DestWarehouseCaption);
		}

		protected override void TestWrapperMappingsEmpty_FromLocationCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.FromLocationCaption), "From Location", emptyWrapper.FromLocationCaption);
		}

		protected override void TestWrapperMappingsEmpty_FromWarehouseCaption(WarehouseGenericLineWrapper emptyWrapper)
		{
			AssertEquals(nameof(emptyWrapper.FromWarehouseCaption), "From Warehouse", emptyWrapper.FromWarehouseCaption);
		}

		#endregion

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject whsLineBO)
		{
			return new WhsItemTransferLineWrapper((WhsItemTransferLine)whsLineBO, Factory);
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return TransferLineWrapper;
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return TransferLineWrapper;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return TransferLine;
		}

		#endregion

		WhsItemTransferHeader TransferHeader => transferHeader ?? (transferHeader = Factory.New<WhsItemTransferHeader>());
		WhsItemTransferHeader transferHeader;

		WhsItemTransferLine TransferLine => transferLine ?? (transferLine = TransferHeader.Lines.AddNew());
		WhsItemTransferLine transferLine;

		WhsItemTransferLineWrapper TransferLineWrapper => (WhsItemTransferLineWrapper)GetNewWarehouseLineWrapper(TransferLine);
	}
}
