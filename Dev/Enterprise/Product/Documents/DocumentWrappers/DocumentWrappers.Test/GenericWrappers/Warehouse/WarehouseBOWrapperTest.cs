using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseBOWrapper))]
	sealed class WarehouseBOWrapperTest : GenericWrapperTest
	{
		public void TestWarehouseName()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}
			Docket.Warehouse.WW_WarehouseName = "WHS1";
			WarehouseBOWrapper warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);
			AssertEquals("WHS1", warehouseWrapper.Name);
		}

		public void TestWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Warehouse";
			Docket.WD_WW_Whs = warehouse.PK;

			var warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);
			AssertEquals("WarehouseName in Engliseh.", "Test Warehouse", warehouseWrapper.Name);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Test Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("WarehouseName in Chinese", "测试仓库", warehouseWrapper.Name);
			}
		}

		public void TestWarehouseNameAndAddress()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}

			Docket.Warehouse.WW_WarehouseName = "Warehouse One";

			OrgHeader whsOrg = Factory.New<OrgHeader>();
			OrgAddress address = whsOrg.MainAddress;

			whsOrg.OH_RL_NKClosestPort = "AU";
			whsOrg.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "US";
			address.OA_RN_NKCountryCode = "AU";

			address.OA_OH = whsOrg.PK;
			Docket.Warehouse.WW_OA_WarehouseAddress = address.PK;

			WarehouseBOWrapper warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);

			AssertEquals("Warehouse name and address", (ZString)"Warehouse One\r\nMY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", warehouseWrapper.NameAndAddress);
			AssertEquals("Warehouse address", "MY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", warehouseWrapper.Address.ToString());
		}

		public void TestWarehouseNameAndAddress_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Warehouse One";
			Docket.WD_WW_Whs = warehouse.PK;

			var whsOrg = Factory.New<OrgHeader>();
			var address = whsOrg.MainAddress;

			whsOrg.OH_RL_NKClosestPort = "AU";
			whsOrg.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "US";
			address.OA_RN_NKCountryCode = "AU";

			address.OA_OH = whsOrg.PK;
			warehouse.WW_OA_WarehouseAddress = address.PK;

			var warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);
			AssertEquals("Warehouse name and address in English.", "Warehouse One\r\nMY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", warehouseWrapper.NameAndAddress);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Warehouse One").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "仓库1"));
				AssertEquals("Warehouse Name and Address in Chinese", "仓库1\r\nMY ORGANISATION NAME\nMY ADDRESS 1\nMY ADDRESS 2\nMY CITY MY STATE MY CODE\nAUSTRALIA", warehouseWrapper.NameAndAddress);
			}
		}

		#region TestAddress

		public void TestAddress_Cached()
		{
			var whs = Helper.CreateWarehouse("WH1");
			var whsWrapper = CreateWarehouseWrapper(whs);

			var addressWrapper1 = whsWrapper.Address;
			var addressWrapper2 = whsWrapper.Address;
			AssertNotNull(addressWrapper1);
			AssertNotNull(addressWrapper2);
			AssertEquals("Should be same instance of class.", true, addressWrapper1.Equals(addressWrapper2));
		}

		#endregion

		public void TestPhoneAndFax()
		{
			var whsOrg = Factory.New<OrgHeader>();
			var address = whsOrg.MainAddress;

			whsOrg.OH_RL_NKClosestPort = "AU";
			whsOrg.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "AU";
			address.OA_Phone = "03 9289 2829";

			address.OA_OH = whsOrg.PK;
			Docket.Warehouse.WW_OA_WarehouseAddress = address.PK;

			var warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);
			AssertEquals("Warehouse phone", "Tel: +61 3 9289 2829", warehouseWrapper.PhoneAndFax);

			address.OA_Fax = "02 5555 9999";
			warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);
			AssertEquals("Warehouse phone and fax", "Tel: +61 3 9289 2829   Fax: +61 2 5555 9999", warehouseWrapper.PhoneAndFax);
		}

		public void TestWarehouseTypeDescription()
		{
			WarehouseBOWrapper warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);
			AssertEquals("Type Description", "Warehouse", warehouseWrapper.TypeDescription);
		}

		public void TestWarehouseCode()
		{
			WarehouseBOWrapper warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);
			AssertEquals("Warehouse Code", "WH1", warehouseWrapper.Code);
		}

		public void TestAutoPrintFields()
		{
			Docket.Warehouse.WW_AutoPrintOrderCopyForMOPOnPick = true;
			Docket.Warehouse.WW_AutoPrintOrderSummaryOnPick = true;
			Docket.Warehouse.WW_AutoPrintPackingSlip = true;
			Docket.Warehouse.WW_AutoPrintPickingNonPickedItems = true;
			Docket.Warehouse.WW_AutoPrintPickingShortfallItems = true;

			WarehouseBOWrapper warehouseWrapper = CreateWarehouseWrapper(Docket.Warehouse);
			AssertEquals("warehouseWrapper.AutoPrintPickingSlip", true, warehouseWrapper.AutoPrintPickingSlip);
			AssertEquals("warehouseWrapper.AutoPrintOrderSummary", true, warehouseWrapper.AutoPrintOrderSummary);
			AssertEquals("warehouseWrapper.AutoPrintNonPickedItems", true, warehouseWrapper.AutoPrintNonPickedItems);
			AssertEquals("warehouseWrapper.AutoPrintShortfallItems", true, warehouseWrapper.AutoPrintShortfallItems);
			AssertEquals("warehouseWrapper.AutoPrintOrderCopyForMOP", true, warehouseWrapper.AutoPrintOrderCopyForMOP);
		}

		#region Implementaiton

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		WhsTestHelperFunctions helper;

		#endregion

		#region Docket

		WhsOrder Docket => docket ?? (docket = GetNewDocket());

		WhsOrder docket;

		WhsOrder GetNewDocket()
		{
			WhsWarehouse warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Warehouse One";
			warehouse.WW_WarehouseCode = "WH1";

			WhsOrder order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			return order;
		}

		#endregion

		WarehouseBOWrapper CreateWarehouseWrapper(WhsWarehouse warehouse)
		{
			return new WarehouseBOWrapper("Warehouse", warehouse, Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			WarehouseBOWrapper wrapperEmpty = new WarehouseBOWrapper("", null, Factory);

			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Code", ZString.Empty, wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.Name", ZString.Empty, wrapperEmpty.Name);
			AssertEquals("wrapperEmpty.NameAndAddress", ZString.Empty, wrapperEmpty.NameAndAddress);
			AssertNull("wrapperEmpty.Address", wrapperEmpty.Address);
			AssertEquals("wrapperEmpty.TypeDescription", ZString.Empty, wrapperEmpty.TypeDescription);
			AssertEquals("wrapperEmpty.AutoPrintPickingSlip", false, wrapperEmpty.AutoPrintPickingSlip);
			AssertEquals("wrapperEmpty.AutoPrintOrderSummary", false, wrapperEmpty.AutoPrintOrderSummary);
			AssertEquals("wrapperEmpty.AutoPrintNonPickedItems", false, wrapperEmpty.AutoPrintNonPickedItems);
			AssertEquals("wrapperEmpty.AutoPrintShortfallItems", false, wrapperEmpty.AutoPrintShortfallItems);
			AssertEquals("wrapperEmpty.AutoPrintOrderCopyForMOP", false, wrapperEmpty.AutoPrintOrderCopyForMOP);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Address : 
Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return CreateWarehouseWrapper(Docket.Warehouse);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
WarehouseBO                                      (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Address                                 Address
AutoPrintNonPickedItems                 Bool
AutoPrintOrderCopyForMOP                Bool
AutoPrintOrderSummary                   Bool
AutoPrintPickingSlip                    Bool
AutoPrintShortfallItems                 Bool
Code                                    String
Name                                    MultilingualString
NameAndAddress                          MultilingualString
PhoneAndFax                             String
TypeDescription                         String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return CreateWarehouseWrapper(GetNewDocket().Warehouse);
		}

		#endregion
	}
}
