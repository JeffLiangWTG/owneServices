using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickOrderedInventoryWrapper))]
	sealed class WarehousePickOrderedInventoryWrapperTest : WarehouseGenericLineWrapperTest
	{
		#region TestPartAttribute1

		public void TestPartAttribute1()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			OrderedInventory.Owners.Add(orderLine);
			AssertEquals("DocWrapper PartAttribute1 is incorrect", ZString.Empty, DocWrapper.PartAttribute1);

			orderLine.WE_PartAttrib1 = "TEST";
			AssertEquals("DocWrapper PartAttribute1 is incorrect", "TEST", DocWrapper.PartAttribute1);
		}

		#endregion

		#region TestPartAttribte1Name

		public void TestPartAttribute1Name()
		{
			AssertNull("Precondition: Client should be null", OrderedInventory.Client);
			AssertEquals("DocWrapper PartAttribute1Name should be empty", ZString.Empty, DocWrapper.PartAttribute1Name);

			SetupOrderedInventoryWithClientInfo("TEST CODE", "TEST NAME");
			OrderedInventory.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1Name is incorrect", "LABEL", DocWrapper.PartAttribute1Name);
		}

		public void TestPartAttribute1Name_Translatable()
		{
			SetupOrderedInventoryWithClientInfo("TEST CODE", "TEST NAME");
			OrderedInventory.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("PartAttribute1Name in English", "LABEL", DocWrapper.PartAttribute1Name);

			var resKey = OrderedInventory.Client.MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(OrderedInventory.Client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute1Name in Chinese", "标签", DocWrapper.PartAttribute1Name);
			}
		}

		#endregion

		#region TestPartAttribte2WithLabel

		public void TestPartAttribute2WithLabel()
		{
			var client = Helper.CreateClient("C1");
			var orderLine = Helper.CreateWhsOrderWithOrderLine(client, Helper.CreateWarehouse("W1"), Helper.CreateProduct(client, "P1"), 5).Lines[0];
			OrderedInventory.Owners.Add(orderLine);
			AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			//Client is null, PartAttrib is set
			orderLine.WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", DocWrapper.PartAttribute2WithLabel);

			//Client is not null, Client PartAttribName is empty, PartAttrib is empty
			OrderedInventory.Owners.RemoveFromRelationship(orderLine);
			SetupOrderedInventoryWithClientInfo("TEST CODE", "TEST NAME");
			OrderedInventory.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			//Client is not null, Client PartAttribName is empty, PartAttrib is set
			OrderedInventory.Owners[0].WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", DocWrapper.PartAttribute2WithLabel);

			//Client is not null, Client PartAttribName is set, PartAttrib is empty
			OrderedInventory.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			OrderedInventory.Owners[0].WE_PartAttrib2 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			//Client is not null, Client PartAttribName is set, PartAttrib is set
			OrderedInventory.Owners[0].WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			var client = Helper.CreateClient("C1");
			var orderLine = Helper.CreateWhsOrderWithOrderLine(client, Helper.CreateWarehouse("W1"), Helper.CreateProduct(client, "P1"), 5).Lines[0];
			OrderedInventory.Owners.Add(orderLine);

			orderLine.WE_PartAttrib2 = "TEST";
			client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("PartAttribute2WithLabel in English.", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);

			var resKey = client.MiscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute2WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute2WithLabel);
			}
		}

		#endregion

		#region TestPartAttribte3WithLabel

		public void TestPartAttribute3WithLabel()
		{
			var client = Helper.CreateClient("C1");
			var orderLine = Helper.CreateWhsOrderWithOrderLine(client, Helper.CreateWarehouse("W1"), Helper.CreateProduct(client, "P1"), 5).Lines[0];
			OrderedInventory.Owners.Add(orderLine);
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			//Client is null, PartAttrib is set
			orderLine.WE_PartAttrib3 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 3: TEST", DocWrapper.PartAttribute3WithLabel);

			//Client is not null, Client PartAttribName is empty, PartAttrib is empty
			OrderedInventory.Owners.RemoveFromRelationship(orderLine);
			SetupOrderedInventoryWithClientInfo("TEST CODE", "TEST NAME");
			OrderedInventory.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			//Client is not null, Client PartAttribName is empty, PartAttrib is set
			OrderedInventory.Owners[0].WE_PartAttrib3 = "TEST";
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "Attribute 3: TEST", DocWrapper.PartAttribute3WithLabel);

			//Client is not null, Client PartAttribName is set, PartAttrib is empty
			OrderedInventory.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			OrderedInventory.Owners[0].WE_PartAttrib3 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			//Client is not null, Client PartAttribName is set, PartAttrib is set
			OrderedInventory.Owners[0].WE_PartAttrib3 = "TEST";
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			var client = Helper.CreateClient("C1");
			var orderLine = Helper.CreateWhsOrderWithOrderLine(client, Helper.CreateWarehouse("W1"), Helper.CreateProduct(client, "P1"), 5).Lines[0];
			OrderedInventory.Owners.Add(orderLine);

			orderLine.WE_PartAttrib3 = "TEST";
			client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("PartAttribute3WithLabel in English.", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);

			var resKey = client.MiscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute3WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute3WithLabel);
			}
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestTrackedSerialNumber()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			OrderedInventory.Owners.Add(orderLine);
			AssertEquals(ZString.Empty, DocWrapper.TrackedSerialNumber);

			orderLine.WE_SerialNumber = "Serial Number";
			AssertEquals("Serial Number", DocWrapper.TrackedSerialNumber);
		}

		#endregion

		#region TestTrackedSerialWithLabel

		public void TestTrackedSerialWithLabel()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			OrderedInventory.Owners.Add(orderLine);
			AssertEquals(ZString.Empty, DocWrapper.TrackedSerialWithLabel);

			orderLine.WE_SerialNumber = "Serial Number";
			AssertEquals("Tracked Serial Number: " + "Serial Number", DocWrapper.TrackedSerialWithLabel);
		}

		#endregion

		#region TestPackingDateWithLabel

		public void TestPackingDateWithLabel()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			OrderedInventory.Owners.Add(orderLine);
			AssertEquals(ZString.Empty, DocWrapper.PackingDateWithLabel);

			var packingDate = ZDate.Today;
			orderLine.WE_PackingDate = packingDate;
			AssertEquals("Packing Date: " + packingDate.ToShortDateString(), DocWrapper.PackingDateWithLabel);
		}

		#endregion

		#region TestExpiryDateWithLabel

		public void TestExpiryDateWithLabel()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			OrderedInventory.Owners.Add(orderLine);
			AssertEquals(ZString.Empty, DocWrapper.ExpiryDateWithLabel);

			var expiryDate = ZDate.Today;
			orderLine.WE_ExpiryDate = expiryDate;
			AssertEquals("Expiry Date: " + expiryDate.ToShortDateString(), DocWrapper.ExpiryDateWithLabel);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			AssertNull("Precondition: OrderedInventory Product should be null", OrderedInventory.SupplierPart);
			AssertEquals("Precondition: OrderedInventory UnitsUQ should be UNT", "UNT", OrderedInventory.UnitsUQ);
			AssertEquals("DocWrapper UnitsUQ is incorrect", "UNT", DocWrapper.UnitsUQ);

			SetupOrderedInventoryWithProduct("KG");
			AssertEquals("DocWrapper UnitsUQ is incorrect", "KG", DocWrapper.UnitsUQ);
		}

		#endregion

		#region TestClientName

		public void TestClientName()
		{
			AssertNull("Precondition: Client should be null", OrderedInventory.Client);
			AssertEquals(ZString.Empty, DocWrapper.ClientName);

			SetupOrderedInventoryWithClientInfo("TEST CODE", "TEST NAME");
			AssertEquals("DocWrapper Client Name is incorrect", "TEST NAME", DocWrapper.ClientName);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			AssertNull("Precondition: Client should be null", OrderedInventory.Client);
			AssertEquals(ZString.Empty, DocWrapper.ClientName);

			SetupOrderedInventoryWithClientInfo("TEST CODE", "TEST NAME");
			AssertEquals("DocWrapper Client Code is incorrect", "TEST CODE", DocWrapper.ClientCode);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			AssertNull("Precondition: OrderedInventory Product should be null", OrderedInventory.SupplierPart);
			AssertEquals("DocWrapper ProductCode should be empty", ZString.Empty, DocWrapper.ProductCode);

			SetupOrderedInventoryWithProduct("TEST CODE", "TEST DESCRIPTION");

			AssertEquals("DocWrapper ProductCode is incorrect", "TEST CODE", DocWrapper.ProductCode);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			AssertNull("Precondition: OrderedInventory Product should be null", OrderedInventory.SupplierPart);
			AssertEquals("DocWrapper ProductDesc should be empty", ZString.Empty, DocWrapper.ProductDescription);

			SetupOrderedInventoryWithProduct("TEST CODE", "TEST DESCRIPTION");

			AssertEquals("DocWrapper ProductDesc is incorrect", "TEST DESCRIPTION", DocWrapper.ProductDescription);
		}

		#endregion

		#region TestProductBrandName

		public void TestProductBrandName()
		{
			AssertNull("Precondition: OrderedInventory Product should be null", OrderedInventory.SupplierPart);
			AssertEquals("DocWrapper ProductBrandName should be empty", ZString.Empty, DocWrapper.ProductBrandName);

			SetupOrderedInventoryWithProduct("TEST CODE", "TEST DESCRIPTION");
			OrderedInventory.SupplierPart.OP_Brand = "TEST BRAND";

			AssertEquals("DocWrapper ProductBrandName is incorrect", "TEST BRAND", DocWrapper.ProductBrandName);
		}

		#endregion

		#region TestProductModel

		public void TestProductModel()
		{
			AssertNull("Precondition: OrderedInventory Product should be null", OrderedInventory.SupplierPart);
			AssertEquals("DocWrapper ProductModel should be empty", ZString.Empty, DocWrapper.ProductModel);

			SetupOrderedInventoryWithProduct("TEST CODE", "TEST DESCRIPTION");
			OrderedInventory.SupplierPart.OP_Model = "TEST MODEL";

			AssertEquals("DocWrapper ProductModel is incorrect", "TEST MODEL", DocWrapper.ProductModel);
		}

		#endregion

		#region TestUnitsOrdered

		public void TestUnitsOrdered()
		{
			AssertEquals("Pre-condition", 0m, OrderedInventory.QuantityOrdered);
			AssertEquals("Pre-condition", 0m, DocWrapper.UnitsOrdered.NativeValue);

			SetupInventoryWithOrderedInventory(50m, 70m);
			AssertEquals(70m, OrderedInventory.QuantityOrdered);
			AssertEquals(70m, DocWrapper.UnitsOrdered.NativeValue);
		}

		public void TestUnitsOrdered_UnitOrderedOverride()
		{
			var wrapper = new WarehousePickOrderedInventoryWrapper(OrderedInventory, Factory, unitOrderedOverride: 1m);
			AssertEquals(0m, OrderedInventory.QuantityOrdered);
			AssertEquals(1m, wrapper.UnitsOrdered.NativeValue);

			SetupInventoryWithOrderedInventory(50m, 70m);
			AssertEquals(70m, OrderedInventory.QuantityOrdered);
			AssertEquals(1m, wrapper.UnitsOrdered.NativeValue);
		}

		#endregion

		#region TestUnitsPicked

		public void TestUnitsPicked()
		{
			AssertEquals(0m, OrderedInventory.PickLineQuantity);
			AssertEquals(0m, DocWrapper.UnitsPicked.NativeValue);

			SetupInventoryWithOrderedInventory(50m, 70m);
			AssertEquals(50m, OrderedInventory.PickLineQuantity);
			AssertEquals(50m, DocWrapper.UnitsPicked.NativeValue);
		}

		public void TestUnitsPicked_UnitOrderedOverride()
		{
			var wrapper = new WarehousePickOrderedInventoryWrapper(OrderedInventory, Factory, unitsPickedOverride: 2m);
			AssertEquals(0m, OrderedInventory.PickLineQuantity);
			AssertEquals(2m, wrapper.UnitsPicked.NativeValue);

			SetupInventoryWithOrderedInventory(50m, 70m);
			AssertEquals(50m, OrderedInventory.PickLineQuantity);
			AssertEquals(2m, wrapper.UnitsPicked.NativeValue);
		}

		#endregion

		#region TestUnitsShort

		public void TestUnitsShort()
		{
			AssertEquals(0m, OrderedInventory.QuantityOrdered);
			AssertEquals(0m, DocWrapper.UnitsShort.NativeValue);

			SetupInventoryWithOrderedInventory(50m, 70m);
			AssertEquals(20m, OrderedInventory.QuantityShort);
			AssertEquals(20m, DocWrapper.UnitsShort.NativeValue);
		}

		#endregion

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override bool CanSetProductAttributesInWrapperCore => false;

		#endregion

		#region Implementation

		void SetupOrderedInventory(WhsOrder order)
		{
			Factory.Save();
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			AssertNotEquals("Should have ItemsToPick", 0, pick.OrderedInventories.Count);
			OrderedInventory = pick.OrderedInventories[0];
			DocWrapper = WarehousePickOrderedInventoryWrapper.New(OrderedInventory, Factory);
			AssertNotNull("Precondition: DocWrapper was not created", DocWrapper);
		}

		void SetupOrderedInventoryWithProduct(ZString stockKeepingUnit)
		{
			SetupOrderedInventoryWithProduct("TST CODE", "TST DESC");
			AssertNotNull("Should have product", OrderedInventory.SupplierPart);
			OrderedInventory.SupplierPart.OP_StockKeepingUnit = stockKeepingUnit;
			AssertEquals("OrderedInventory StockKeepingUnit is incorrect", stockKeepingUnit, OrderedInventory.SupplierPart.OP_StockKeepingUnit);
		}

		void SetupOrderedInventoryWithProduct(ZString productCode, ZString productDesc)
		{
			var whs = Helper.CreateWarehouse("WH1");
			var client = Helper.CreateClient("CL1");
			var order = Factory.New<WhsOrder>();
			order.WD_OH_Client = client.PK;
			order.WD_WW_Whs = whs.PK;
			var part = Helper.CreateProduct(client, "P1");
			part.OP_PartNum = productCode;
			part.OP_Desc = productDesc;
			Helper.CreateWhsOrderLine(order, part, 10m);

			SetupOrderedInventory(order);

			AssertEquals("OrderedInventory Product code is incorrect", productCode, OrderedInventory.ProductCode);
			AssertEquals("OrderedInventory Product Description is incorrect", productDesc, OrderedInventory.ProductDesc);
		}

		void SetupOrderedInventoryWithClientInfo(ZString clientCode, ZString clientName)
		{
			var client = Helper.CreateClient(clientCode, clientName);
			var order = Helper.CreateWhsOrder(client, Helper.CreateWarehouse("TST WHS"));
			Helper.CreateWhsOrderLine(order, Helper.CreateProduct(client, "PROD1"), 10m);

			SetupOrderedInventory(order);

			AssertNotNull("Precondition: Client should not be null", OrderedInventory.Client);
			AssertEquals("Precondition: Client Code is incorrect", clientCode, OrderedInventory.Client.OH_Code);
			AssertEquals("Precondition: Client Name is incorrect", clientName, OrderedInventory.Client.OH_FullName);
		}

		void SetupInventoryWithOrderedInventory(ZDecimal pickedQty, ZDecimal orderedQty)
		{
			TestDataSimpleEnvironment data = new TestDataSimpleEnvironment(Factory);
			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, pickedQty, "TEST");

			receive.AllocateLocationsWithMock();
			AssertEquals("Precondition: Putaway should be created", true, receive.IsPuttingAway);

			receive.FinaliseDocket();
			AssertEquals("Precondition: Receive should be finalized", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, orderedQty, "TEST", "DummyOutward-1");

			SetupOrderedInventory(order);
			OrderedInventory.AvailableInventories[0].PickLineQuantity = OrderedInventory.AvailableInventories[0].QuantityAvailableToPick;

			AssertEquals("Precondition: OrderedInventory  PickLineQuantity not correct", pickedQty, OrderedInventory.PickLineQuantity);
			AssertEquals("Precondition: OrderedInventory  QuantityOrdered not correct", orderedQty, OrderedInventory.QuantityOrdered);
			AssertEquals("Precondition: OrderedInventory  QuantityShort not correct", (pickedQty < orderedQty ? orderedQty - pickedQty : 0), OrderedInventory.QuantityShort);
		}

		protected override void SetUp()
		{
			Notify = new TestNotificationBuffer();
			OrderedInventory = new WhsPickOrderedInventory(Factory);
			DocWrapper = WarehousePickOrderedInventoryWrapper.New(OrderedInventory, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		#region Abstract members Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return DocWrapper;
		}

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
UnitsOrdered : 0
UnitsPicked : 0
UnitsShort : 0
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new WarehousePickOrderedInventoryWrapper(null, Factory);
		}

		#endregion

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject whsLineBO)
		{
			return new WarehousePickOrderedInventoryWrapper((WhsPickOrderedInventory)whsLineBO, Factory);
		}

		WhsPickOrderedInventory OrderedInventory;
		WarehousePickOrderedInventoryWrapper DocWrapper;
		TestNotificationBuffer Notify;

		protected override BusinessObject GetNewBusinessObject()
		{
			return OrderedInventory;
		}

		#endregion
	}
}
