using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPickOrderedInventory))]
	sealed class DocWhsPickOrderedInventoryTest : DocumentWrapperTestCase
	{
		#region Properties

		#region ZString Fields

		public void TestPartAttribute1()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			ItemToPick.Owners.Add(orderLine);
			AssertEquals("DocWrapper PartAttribute1 is incorrect", ZString.Empty, DocWrapper.PartAttribute1);

			orderLine.WE_PartAttrib1 = "TEST";
			AssertEquals("DocWrapper PartAttribute1 is incorrect", "TEST", DocWrapper.PartAttribute1);
		}

		public void TestPartAttribute1Name()
		{
			AssertNull("Precondition: Client should be null", ItemToPick.Client);
			AssertEquals("DocWrapper PartAttribute1Name should be empty", ZString.Empty, DocWrapper.PartAttribute1Name);

			SetupItemToPickWithClientInfo("TEST CODE", "TEST NAME");
			ItemToPick.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1Name is incorrect", "LABEL", DocWrapper.PartAttribute1Name);
		}

		public void TestPartAttribute1Name_Translatable()
		{
			SetupItemToPickWithClientInfo("TEST CODE", "TEST NAME");
			ItemToPick.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1Name in English", "LABEL", DocWrapper.PartAttribute1Name);

			var resKey = ItemToPick.Client.MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(ItemToPick.Client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute1Name in Chinese", "标签", DocWrapper.PartAttribute1Name);
			}
		}

		public void TestPartAttribute2WithLabel_ClientNull_PartAttribSet()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			ItemToPick.Owners.Add(orderLine);
			AssertNull("Precondition: Client should be null", ItemToPick.Client);
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			//Client is null, PartAttrib is set
			orderLine.WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", DocWrapper.PartAttribute2WithLabel);
		}

		public void TestPartAttribute2WithLabel()
		{
			var clientCode = "TEST CODE";
			var clientName = "TEST NAME";
			var client = Helper.CreateClient(clientCode, clientName);
			var whs = Helper.CreateWarehouse("TST WHS");
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order, Helper.CreateProduct(client, "PROD1"), 10m);

			SetupItemToPick(order);
			AssertNotNull("Precondition: Client should not be null", ItemToPick.Client);
			AssertEquals("Precondition: Client Code is incorrect", clientCode, ItemToPick.Client.OH_Code);
			AssertEquals("Precondition: Client Name is incorrect", clientName, ItemToPick.Client.OH_FullName);

			ItemToPick.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			//Client is not null, Client PartAttribName is empty, PartAttrib is set
			ItemToPick.Owners[0].WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", DocWrapper.PartAttribute2WithLabel);

			//Client is not null, Client PartAttribName is set, PartAttrib is empty
			ItemToPick.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			ItemToPick.Owners[0].WE_PartAttrib2 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			//Client is not null, Client PartAttribName is set, PartAttrib is set
			ItemToPick.Owners[0].WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			var clientCode = "TEST CODE";
			var clientName = "TEST NAME";
			var client = Helper.CreateClient(clientCode, clientName);
			var whs = Helper.CreateWarehouse("TST WHS");
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			var orderLine = Helper.CreateWhsOrderLine(order, Helper.CreateProduct(client, "PROD1"), 10m);

			SetupItemToPick(order);
			AssertNotNull("Precondition: Client should not be null", ItemToPick.Client);
			AssertEquals("Precondition: Client Code is incorrect", clientCode, ItemToPick.Client.OH_Code);
			AssertEquals("Precondition: Client Name is incorrect", clientName, ItemToPick.Client.OH_FullName);

			ItemToPick.Owners.Add(orderLine);
			ItemToPick.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			ItemToPick.Owners[0].WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);

			var resKey = ItemToPick.Client.MiscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(ItemToPick.Client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute2WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute2WithLabel);
			}
		}

		public void TestPartAttribute3WithLabel_ClientNull_PartAttribSet()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			ItemToPick.Owners.Add(orderLine);
			AssertNull("Precondition: Client should be null", ItemToPick.Client);
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			//Client is null, PartAttrib is set
			orderLine.WE_PartAttrib3 = "TEST";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 3: TEST", DocWrapper.PartAttribute3WithLabel);
		}

		public void TestPartAttribute3WithLabel()
		{
			var clientCode = "TEST CODE";
			var clientName = "TEST NAME";
			var client = Helper.CreateClient(clientCode, clientName);
			var whs = Helper.CreateWarehouse("TST WHS");
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order, Helper.CreateProduct(client, "PROD1"), 10m);

			SetupItemToPick(order);
			AssertNotNull("Precondition: Client should not be null", ItemToPick.Client);
			AssertEquals("Precondition: Client Code is incorrect", clientCode, ItemToPick.Client.OH_Code);
			AssertEquals("Precondition: Client Name is incorrect", clientName, ItemToPick.Client.OH_FullName);

			ItemToPick.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			//Client is not null, Client PartAttribName is empty, PartAttrib is set
			ItemToPick.Owners[0].WE_PartAttrib3 = "TEST";
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "Attribute 3: TEST", DocWrapper.PartAttribute3WithLabel);

			//Client is not null, Client PartAttribName is set, PartAttrib is empty
			ItemToPick.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			ItemToPick.Owners[0].WE_PartAttrib3 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			//Client is not null, Client PartAttribName is set, PartAttrib is set
			ItemToPick.Owners[0].WE_PartAttrib3 = "TEST";
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			var clientCode = "TEST CODE";
			var clientName = "TEST NAME";
			var client = Helper.CreateClient(clientCode, clientName);
			var whs = Helper.CreateWarehouse("TST WHS");
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			var orderLine = Helper.CreateWhsOrderLine(order, Helper.CreateProduct(client, "PROD1"), 10m);

			SetupItemToPick(order);
			AssertNotNull("Precondition: Client should not be null", ItemToPick.Client);
			AssertEquals("Precondition: Client Code is incorrect", clientCode, ItemToPick.Client.OH_Code);
			AssertEquals("Precondition: Client Name is incorrect", clientName, ItemToPick.Client.OH_FullName);

			ItemToPick.Owners.Add(orderLine);
			ItemToPick.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			ItemToPick.Owners[0].WE_PartAttrib3 = "TEST";
			AssertEquals("DocWrapper PartAttribute3WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);

			var resKey = ItemToPick.Client.MiscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(ItemToPick.Client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute3WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute3WithLabel);
			}
		}

		public void TestTrackedSerialWithLabel()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			ItemToPick.Owners.Add(orderLine);
			AssertEquals("DocWrapper TrackedSerialWithLabel shoule be empty", ZString.Empty, DocWrapper.TrackedSerialWithLabel);

			orderLine.WE_SerialNumber = "TEST";
			AssertEquals("DocWrapper TrackedSerialWithLabel is incorrect", "Tracked Serial Number: TEST", DocWrapper.TrackedSerialWithLabel);

			orderLine.WE_SerialNumber = ZString.Empty;
			AssertEquals("DocWrapper TrackedSerialWithLabel is incorrect", ZString.Empty, DocWrapper.TrackedSerialWithLabel);
		}

		public void TestTrackedSerialWithLabel_Translatable()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			ItemToPick.Owners.Add(orderLine);
			ItemToPick.Owners[0].WE_SerialNumber = "FRESER";
			AssertEquals("DocWrapper TrackedSerialWithLabel in English", "Tracked Serial Number: FRESER", DocWrapper.TrackedSerialWithLabel);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", new ResourceStringData("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "跟踪序列号"));
				AssertEquals("DocWrapper PartAttribute2WithLabel in Chinese", "跟踪序列号: FRESER", DocWrapper.TrackedSerialWithLabel);
			}
		}

		public void TestUnitsUQ()
		{
			AssertNull("Precondition: ItemToPick Product should be null", ItemToPick.SupplierPart);
			AssertEquals("Precondition: ItemToPick UnitsUQ should be UNT", "UNT", ItemToPick.UnitsUQ);
			AssertEquals("DocWrapper UnitsUQ is incorrect", "UNT", DocWrapper.UnitsUQ);

			SetupItemToPickWithProduct("KG");
			AssertEquals("DocWrapper UnitsUQ is incorrect", "KG", DocWrapper.UnitsUQ);

			SetupItemToPickWithProduct("PLT");
			AssertEquals("DocWrapper UnitsUQ is incorrect", "PLT", DocWrapper.UnitsUQ);
		}

		public void TestClientName()
		{
			AssertNull("Precondition: Client should be null", ItemToPick.Client);
			AssertEquals(ZString.Empty, DocWrapper.ClientName);

			SetupItemToPickWithClientInfo("TEST CODE", "TEST NAME");
			AssertEquals("DocWrapper Client Name is incorrect", "TEST NAME", DocWrapper.ClientName);
		}

		public void TestClientCode()
		{
			AssertNull("Precondition: Client should be null", ItemToPick.Client);
			AssertEquals(ZString.Empty, DocWrapper.ClientName);

			SetupItemToPickWithClientInfo("TEST CODE", "TEST NAME");
			AssertEquals("DocWrapper Client Code is incorrect", "TEST CODE", DocWrapper.ClientCode);
		}

		#region TestProductCode

		public void TestProductCode()
		{
			AssertNull("Precondition: ItemToPick Product should be null", ItemToPick.SupplierPart);
			AssertEquals("DocWrapper ProductCode should be empty", ZString.Empty, DocWrapper.ProductCode);

			SetupItemToPickWithProduct("TEST CODE", "TEST DESCRIPTION");

			AssertEquals("DocWrapper ProductCode is incorrect", "TEST CODE", DocWrapper.ProductCode);
		}

		#endregion

		#region TestProductDesc

		public void TestProductDesc()
		{
			AssertNull("Precondition: ItemToPick Product should be null", ItemToPick.SupplierPart);
			AssertEquals("DocWrapper ProductDesc should be empty", ZString.Empty, DocWrapper.ProductDesc);

			SetupItemToPickWithProduct("TEST CODE", "TEST DESCRIPTION");

			AssertEquals("DocWrapper ProductDesc is incorrect", "TEST DESCRIPTION", DocWrapper.ProductDesc);
		}

		#endregion

		#region TestProductBrandName

		public void TestProductBrandName()
		{
			AssertNull("Precondition: ItemToPick Product should be null", ItemToPick.SupplierPart);
			AssertEquals("DocWrapper ProductBrandName should be empty", ZString.Empty, DocWrapper.ProductBrandName);

			SetupItemToPickWithProduct("TEST CODE", "TEST DESCRIPTION");
			ItemToPick.SupplierPart.OP_Brand = "TEST BRAND";

			AssertEquals("DocWrapper ProductBrandName is incorrect", "TEST BRAND", DocWrapper.ProductBrandName);
		}

		#endregion

		#region TestProductModel

		public void TestProductModel()
		{
			AssertNull("Precondition: ItemToPick Product should be null", ItemToPick.SupplierPart);
			AssertEquals("DocWrapper ProductModel should be empty", ZString.Empty, DocWrapper.ProductModel);

			SetupItemToPickWithProduct("TEST CODE", "TEST DESCRIPTION");
			ItemToPick.SupplierPart.OP_Model = "TEST MODEL";

			AssertEquals("DocWrapper ProductModel is incorrect", "TEST MODEL", DocWrapper.ProductModel);
		}

		#endregion

		#endregion

		#region ZDecimal Fields

		public void TestQuantityOrdered()
		{
			AssertEquals(0m, ItemToPick.QuantityOrdered);
			AssertEquals(0m, DocWrapper.QuantityOrdered);

			SetupInventoryWithItemToPick(50m, 70m);
			AssertEquals(70m, ItemToPick.QuantityOrdered);
			AssertEquals(70m, DocWrapper.QuantityOrdered);
		}

		public void TestPickLineQuantity()
		{
			AssertEquals(0m, ItemToPick.PickLineQuantity);
			AssertEquals(0m, DocWrapper.QuantityPicked);

			SetupInventoryWithItemToPick(50m, 70m);
			AssertEquals(50m, ItemToPick.PickLineQuantity);
			AssertEquals(50m, DocWrapper.QuantityPicked);
		}

		public void TestQuantityShort()
		{
			AssertEquals(0m, ItemToPick.QuantityOrdered);
			AssertEquals(0m, DocWrapper.QuantityShort);

			SetupInventoryWithItemToPick(50m, 70m);
			AssertEquals(20m, ItemToPick.QuantityShort);
			AssertEquals(20m, DocWrapper.QuantityShort);
		}

		#endregion

		#endregion

		#region Implementation

		void SetupItemToPick(WhsOrder order)
		{
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			AssertNotEquals("Should have ItemsToPick", 0, pick.OrderedInventories.Count);
			ItemToPick = pick.OrderedInventories[0];
			DocWrapper = DocWhsPickOrderedInventory.New(ItemToPick, Factory);
			AssertNotNull("Precondition: DocWrapper was not created", DocWrapper);
		}

		void SetupItemToPickWithProduct(ZString stockKeepingUnit)
		{
			SetupItemToPickWithProduct("TST CODE", "TST DESC");
			AssertNotNull("Should have product", ItemToPick.SupplierPart);
			ItemToPick.SupplierPart.OP_StockKeepingUnit = stockKeepingUnit;
			AssertEquals("ItemToPick StockKeepingUnit is incorrect", stockKeepingUnit, ItemToPick.SupplierPart.OP_StockKeepingUnit);
		}

		void SetupItemToPickWithProduct(ZString productCode, ZString productDesc)
		{
			var order = Factory.New<WhsOrder>();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = productCode;
			part.OP_Desc = productDesc;
			Helper.CreateWhsOrderLine(order, part, 10m);

			SetupItemToPick(order);

			AssertEquals("ItemToPick Product code is incorrect", productCode, ItemToPick.ProductCode);
			AssertEquals("ItemToPick Product Description is incorrect", productDesc, ItemToPick.ProductDesc);
		}

		void SetupItemToPickWithClientInfo(ZString clientCode, ZString clientName)
		{
			var client = Helper.CreateClient(clientCode, clientName);
			var whs = Helper.CreateWarehouse("TST WHS");
			Factory.Save();
			var order = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order, Helper.CreateProduct(client, "PROD1"), 10m);

			SetupItemToPick(order);

			AssertNotNull("Precondition: Client should not be null", ItemToPick.Client);
			AssertEquals("Precondition: Client Code is incorrect", clientCode, ItemToPick.Client.OH_Code);
			AssertEquals("Precondition: Client Name is incorrect", clientName, ItemToPick.Client.OH_FullName);
		}

		void SetupInventoryWithItemToPick(ZDecimal pickedQty, ZDecimal orderedQty)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, pickedQty, "TEST");

			receive.AllocateLocationsWithMock();
			AssertEquals("Precondition: Putaway should be created", true, receive.IsPuttingAway);

			receive.FinaliseDocket();
			AssertEquals("Precondition: Receive should be finalized", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, orderedQty, "TEST", "DummyOutward-1");
			Factory.Save();

			SetupItemToPick(order);
			ItemToPick.AvailableInventories[0].PickLineQuantity = ItemToPick.AvailableInventories[0].QuantityAvailableToPick;

			AssertEquals("Precondition: ItemToPick  PickLineQuantity not correct", pickedQty, ItemToPick.PickLineQuantity);
			AssertEquals("Precondition: ItemToPick  QuantityOrdered not correct", orderedQty, ItemToPick.QuantityOrdered);
			AssertEquals("Precondition: ItemToPick  QuantityShort not correct", (pickedQty < orderedQty ? orderedQty - pickedQty : 0), ItemToPick.QuantityShort);
		}

		protected override void SetUp()
		{
			Helper = new WhsTestHelperFunctions(Factory);
			Notify = new TestNotificationBuffer();
			ItemToPick = new WhsPickOrderedInventory(Factory);
			DocWrapper = DocWhsPickOrderedInventory.New(ItemToPick, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		WhsPickOrderedInventory ItemToPick;
		DocWhsPickOrderedInventory DocWrapper;
		WhsTestHelperFunctions Helper;
		TestNotificationBuffer Notify;

		#endregion
	}
}
