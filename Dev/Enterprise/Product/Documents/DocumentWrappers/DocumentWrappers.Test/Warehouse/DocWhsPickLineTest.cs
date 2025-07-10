using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPickLine))]
	public class DocWhsPickLineTest : DocumentWrapperTestCase
	{
		#region Current

		#region TestExtraDetails

		public void TestExtraDetails()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ExtraDetails is incorrect", "0 ; 0 ; 0 PLT", DocWrapper.ExtraDetails);

			DocketLine.Docket.WD_OH_Client = data.Org1.PK;
			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("DocWrapper ExtraDetails is incorrect", "0.0 KG; 0.00 M3; 0 PLT", DocWrapper.ExtraDetails);

			Helper.CreateProductUnit(data.Part1, "PLT", 5);
			DocWrapper.Units = 10m;
			data.Part1.OP_Cubic = 10m;
			data.Part1.OP_CubicUQ = "M3";
			data.Part1.OP_Weight = 20m;
			data.Part1.OP_WeightUQ = "KG";
			AssertEquals("DocWrapper ExtraDetails is incorrect", "200 KG; 100 M3; 2 PLT", DocWrapper.ExtraDetails);

			// setup pickline such that we have a valid unit conversion for pack type on the product
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			PickLine.WZ_WE_TransactionLine = order.Lines.AddNew().PK;
			PickLine.InventoryLine.WE_OP = data.Part1.PK;
			Helper.CreateProductUnit(data.Part1, "BOX", 2);

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_F3_NKReleasedPackType = "BOX";

			DocWrapper.Units = 10.121212m;

			AssertEquals("DocWrapper ExtraDetails is incorrect", "202.42 KG; 101.2121 M3; 5.1 BOXES; 2.0 PLT", DocWrapper.ExtraDetails);
		}

		#endregion

		#region TestVolume

		public void TestVolume()
		{
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			AssertEquals("DocWrapper Volume is incorrect", 0m, DocWrapper.Volume);

			DocWrapper.Units = 10m;
			DocketLine.SupplierPart.OP_Cubic = 10m;
			AssertEquals("DocWrapper Volume is incorrect", 100m, DocWrapper.Volume);

			DocketLine.SupplierPart.OP_Cubic = 20m;
			DocWrapper.Units = 20m;
			AssertEquals("DocWrapper Volume is incorrect", 400m, DocWrapper.Volume);

			DocWrapper.Units = 20.1212m;
			AssertEquals("DocWrapper Volume is incorrect", 402.4240m, DocWrapper.Volume);

			PickLine.WZ_Units = 50m;
			AssertEquals("DocWrapper Volume should depend on DocWrapper.Units field and not PickLine.WZ_Units", 402.4240m, DocWrapper.Volume);
		}

		#endregion

		#region TestVolumeUQ

		public void TestVolumeUQ()
		{
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_CubicUQ = "IN";
			AssertEquals("DocWrapper Weight is incorrect", "IN", DocWrapper.VolumeUQ);

			DocketLine.SupplierPart.OP_CubicUQ = "M3";
			AssertEquals("DocWrapper Weight is incorrect", "M3", DocWrapper.VolumeUQ);
		}

		#endregion

		#region TestWeight

		public void TestWeight()
		{
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_Weight = 10m;
			DocWrapper.Units = 10m;
			AssertEquals("DocWrapper Weight is incorrect", 100m, DocWrapper.Weight);

			DocketLine.SupplierPart.OP_Weight = 20m;
			DocWrapper.Units = 20m;
			AssertEquals("DocWrapper Weight is incorrect", 400m, DocWrapper.Weight);

			DocWrapper.Units = 20.1212m;
			AssertEquals("DocWrapper Weight is incorrect", 402.42m, DocWrapper.Weight);

			PickLine.WZ_Units = 50m;
			AssertEquals("DocWrapper Weight should depend on DocWrapper.Units field and not PickLine.WZ_Units", 402.42m, DocWrapper.Weight);
		}

		#endregion

		#region TestWeightUQ

		public void TestWeightUQ()
		{
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_WeightUQ = "KG";
			AssertEquals("DocWrapper Weight is incorrect", "KG", DocWrapper.WeightUQ);

			DocketLine.SupplierPart.OP_WeightUQ = "PB";
			AssertEquals("DocWrapper Weight is incorrect", "PB", DocWrapper.WeightUQ);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("DocWrapper UnitsUQ is incorrect", "TST", DocWrapper.UnitsUQ);
		}

		#endregion

		#region Part Attributes

		#region TestPackingDate

		public void TestPackingDate()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var dateTime_FromInventory = ZDate.Today.AddDays(3);
			var dateTime_FromOrderLine = ZDate.Today.AddDays(5);
			Factory.Save();

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, dateTime_FromInventory, "SN:1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_PackingDate = dateTime_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var docWhsPickLine = DocWhsPickLine.New(orderLine.PickLines[0], Factory);

			AssertEquals("docWhsPickLine.PackingDate should come from Inventory", dateTime_FromInventory, docWhsPickLine.PackingDate);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("docWhsPickLine.PackingDate should come from Order Line", dateTime_FromOrderLine, docWhsPickLine.PackingDate);
		}

		#endregion

		#region TestExpiryDate

		public void TestExpiryDate()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var dateTime_FromInventory = ZDate.Today.AddDays(3);
			var dateTime_FromOrderLine = ZDate.Today.AddDays(5);
			Factory.Save();

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, dateTime_FromInventory, ZDate.Empty, "SN:1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_ExpiryDate = dateTime_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var docWhsPickLine = DocWhsPickLine.New(orderLine.PickLines[0], Factory);

			AssertEquals("docWhsPickLine.ExpiryDate should come from Inventory", dateTime_FromInventory, docWhsPickLine.ExpiryDate);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("docWhsPickLine.ExpiryDate should come from Order Line", dateTime_FromOrderLine, docWhsPickLine.ExpiryDate);
		}

		#endregion

		#region TestPackingDateWithLabel

		public void TestPackingDateWithLabel()
		{
			var testDate1 = ZDate.Today;
			var testDate2 = ZDate.Today.AddDays(1);
			DocketLine.WE_PackingDate = testDate1;
			AssertEquals("DocWrapper PackingDateWithLabel is incorrect", "Packing Date: " + testDate1.ToShortDateString(), DocWrapper.PackingDateWithLabel);
		}

		#endregion

		#region TestExpiryDateWithLabel

		public void TestExpiryDateWithLabel()
		{
			var testDate1 = ZDate.Today;
			var testDate2 = ZDate.Today.AddDays(1);
			DocketLine.WE_ExpiryDate = testDate1;
			AssertEquals("DocWrapper ExpiryDateWithLabel is incorrect", "Expiry Date: " + testDate1.ToShortDateString(), DocWrapper.ExpiryDateWithLabel);
		}

		#endregion

		#region TestPartAttribute1

		public void TestPartAttribute1()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var serial_FromInventory = "SN:1";
			var serial_FromOrderLine = "SN:2";
			Factory.Save();

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, serial_FromInventory, "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_PartAttrib1 = serial_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var docWhsPickLine = DocWhsPickLine.New(orderLine.PickLines[0], Factory);

			AssertEquals("docWhsPickLine.PartAttribute1 should come from Inventory", serial_FromInventory, docWhsPickLine.PartAttribute1);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("docWhsPickLine.PartAttribute1 should come from Order Line", serial_FromOrderLine, docWhsPickLine.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute2

		public void TestPartAttribute2()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var serial_FromInventory = "SN:1";
			var serial_FromOrderLine = "SN:2";
			Factory.Save();

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", serial_FromInventory, "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_PartAttrib2 = serial_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var docWhsPickLine = DocWhsPickLine.New(orderLine.PickLines[0], Factory);

			AssertEquals("docWhsPickLine.PartAttribute2 should come from Inventory", serial_FromInventory, docWhsPickLine.PartAttribute2);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("docWhsPickLine.PartAttribute2 should come from Order Line", serial_FromOrderLine, docWhsPickLine.PartAttribute2);
		}

		#endregion

		#region TestPartAttribute3

		public void TestPartAttribute3()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var serial_FromInventory = "SN:1";
			var serial_FromOrderLine = "SN:2";
			Factory.Save();

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", serial_FromInventory, "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_PartAttrib3 = serial_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var docWhsPickLine = DocWhsPickLine.New(orderLine.PickLines[0], Factory);

			AssertEquals("docWhsPickLine.PartAttribute3 should come from Inventory", serial_FromInventory, docWhsPickLine.PartAttribute3);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("docWhsPickLine.PartAttribute3 should come from Order Line", serial_FromOrderLine, docWhsPickLine.PartAttribute3);
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestTrackedSerialNumber()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var serial_FromInventory = "SN:1";
			var serial_FromOrderLine = "SN:2";
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory.WI_SerialNumber = serial_FromInventory;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_SerialNumber = serial_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var docWhsPickLine = DocWhsPickLine.New(orderLine.PickLines[0], Factory);

			AssertEquals("docWhsPickLine.TrackedSerialNumber should come from Inventory", serial_FromInventory, docWhsPickLine.TrackedSerialNumber);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("docWhsPickLine.TrackedSerialNumber should come from Order Line", serial_FromOrderLine, docWhsPickLine.TrackedSerialNumber);
		}

		#endregion

		#region TestPartAttribute1WithLabel

		public void TestPartAttribute1WithLabel()
		{
			CombineAssertions(() =>
				{
					AssertEquals("DocWrapper PartAttribute1WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute1WithLabel);

					DocketLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
					DocketLine.WE_PartAttrib1 = ZString.Empty;
					AssertEquals("DocWrapper PartAttribute1WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute1WithLabel);

					DocketLine.WE_PartAttrib1 = "TEST";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
					AssertEquals("DocWrapper PartAttribute1WithLabel is correct", "Attribute 1: TEST", DocWrapper.PartAttribute1WithLabel);

					DocketLine.WE_PartAttrib1 = "TEST";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
					AssertEquals("DocWrapper PartAttribute1WithLabel is correct", "LABEL: TEST", DocWrapper.PartAttribute1WithLabel);
				});
		}

		public void TestPartAttribute1WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			DocketLine.Docket.WD_OH_Client = client.PK;
			DocketLine.WE_PartAttrib1 = "TEST";
			var miscServ = client.MiscServ;
			miscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute1WithLabel);

			var resKey = miscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute1WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute1WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute2WithLabel

		public void TestPartAttribute2WithLabel()
		{
			CombineAssertions(() =>
				{
					AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

					DocketLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
					DocketLine.WE_PartAttrib2 = ZString.Empty;
					AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

					DocketLine.WE_PartAttrib2 = "TEST";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
					AssertEquals("DocWrapper PartAttribute2WithLabel is correct", "Attribute 2: TEST", DocWrapper.PartAttribute2WithLabel);

					DocketLine.WE_PartAttrib2 = "TEST";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
					AssertEquals("DocWrapper PartAttribute2WithLabel is correct", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);
				});
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			DocketLine.Docket.WD_OH_Client = client.PK;
			DocketLine.WE_PartAttrib2 = "TEST";
			var miscServ = client.MiscServ;
			miscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute2WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);

			var resKey = miscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute2WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute2WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute3WithLabel

		public void TestPartAttribute3WithLabel()
		{
			CombineAssertions(() =>
				{
					AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

					DocketLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
					DocketLine.WE_PartAttrib3 = ZString.Empty;
					AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

					DocketLine.WE_PartAttrib3 = "TEST";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
					AssertEquals("DocWrapper PartAttribute3WithLabel is correct", "Attribute 3: TEST", DocWrapper.PartAttribute3WithLabel);

					DocketLine.WE_PartAttrib3 = "TEST";
					DocketLine.Docket.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
					AssertEquals("DocWrapper PartAttribute3WithLabel is correct", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);
				});
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			DocketLine.Docket.WD_OH_Client = client.PK;
			DocketLine.WE_PartAttrib3 = "TEST";
			var miscServ = client.MiscServ;
			miscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute3WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);

			var resKey = miscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute3WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute3WithLabel);
			}
		}

		#endregion

		#region TestTrackedSerialWithLabel 

		public void TestTrackedSerialWithLabel()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DocWrapper TrackedSerialWithLabel  should be empty", ZString.Empty, DocWrapper.TrackedSerialWithLabel);

				DocketLine.WE_SerialNumber = ZString.Empty;
				AssertEquals("DocWrapper TrackedSerialWithLabel  should be empty", ZString.Empty, DocWrapper.TrackedSerialWithLabel);

				DocketLine.WE_SerialNumber = "TEST";
				AssertEquals("DocWrapper TrackedSerialWithLabel  is correct", "Tracked Serial Number: TEST", DocWrapper.TrackedSerialWithLabel);
			});
		}

		public void TestTrackedSerialWithLabel_Translatable()
		{
			DocketLine.WE_SerialNumber = "TEST";
			AssertEquals("DocWrapper TrackedSerialWithLabel in English", "Tracked Serial Number: TEST", DocWrapper.TrackedSerialWithLabel);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", new ResourceStringData("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "跟踪序列号"));
				AssertEquals("DocWrapper TrackedSerialWithLabel in Chinese", "跟踪序列号: TEST", DocWrapper.TrackedSerialWithLabel);
			}
		}

		#endregion

		#endregion

		#region TestLocationString

		public void TestLocationString()
		{
			var whs = Helper.CreateWarehouse("1");
			var locations = Helper.CreateRowAndGenerateLocations(whs, "AA", 4, 3, 2).Locations;
			DocketLine.WE_WL = locations[locations.Count - 1].PK;
			AssertEquals("DocWrapper LocationString is incorrect", "AA-4-3-2", DocWrapper.LocationString);
		}

		public void TestLocationString_InTransit()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines[0];
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var newPickLine = orderLine.PickLines[0];
			var wrapper = GetNewDocWrapper(newPickLine);
			AssertEquals("LocationString should come from originally Picked Inventory.", "A", wrapper.LocationString);
		}

		#endregion

		#region TestClientName

		public void TestClientName()
		{
			DocketLine.Docket.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper client name should be empty", ZString.Empty, DocWrapper.ClientName);
			DocketLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			DocketLine.Docket.Client.OH_FullName = "TEST";
			AssertEquals("DocWrapper ClientName is incorrect", "TEST", DocWrapper.ClientName);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			DocketLine.Docket.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper client code should be empty", ZString.Empty, DocWrapper.ClientCode);
			DocketLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			DocketLine.Docket.Client.OH_Code = "TST";
			AssertEquals("DocWrapper ClientName is incorrect", "TST", DocWrapper.ClientCode);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductCode should be empty", ZString.Empty, DocWrapper.ProductCode);
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_PartNum = "TEST";
			AssertEquals("DocWrapper ProductCode is incorrect", "TEST", DocWrapper.ProductCode);
		}

		#endregion

		#region TestProductDesc

		public void TestProductDesc()
		{
			DocketLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductDesc should be empty", ZString.Empty, DocWrapper.ProductDesc);
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_Desc = "TEST";
			AssertEquals("DocWrapper ProductDesc is incorrect", "TEST", DocWrapper.ProductDesc);
		}

		#endregion

		#region TestProductDesc2

		public void TestProductDesc2()
		{
			AssertEquals("DocWrapper ProductDesc2 is incorrect", ZString.Empty, DocWrapper.ProductDesc2);

			var owner = Helper.CreateClient();
			var part = Helper.CreateProduct(owner, "P1");
			DocketLine.Docket.WD_OH_Client = owner.PK;
			DocketLine.WE_OP = part.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var partRelation1 = Helper.CreateProductClientRelationShip(supplier, part, OrgPartRelation.RelationshipTypes.Supplier);
			partRelation1.OU_LocalPartDescription = "YYY";
			AssertEquals("Document Wrapper ProductDesc2 is incorrect", ZString.Empty, DocWrapper.ProductDesc2);

			var partRelation2 = Helper.CreateProductClientRelationShip(owner, part, OrgPartRelation.RelationshipTypes.Owner);
			AssertEquals("Document Wrapper ProductDesc2 is incorrect", ZString.Empty, DocWrapper.ProductDesc2);

			partRelation2.OU_LocalPartDescription = "XXX";
			AssertEquals("Document Wrapper ProductDesc2 is incorrect", "XXX", DocWrapper.ProductDesc2);
		}

		#endregion

		#region TestProductBrandName

		public void TestProductBrandName()
		{
			AssertEquals("DocWrapper ProductBrandName should be empty", ZString.Empty, DocWrapper.ProductBrandName);
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_Brand = "TEST BRAND";
			AssertEquals("DocWrapper ProductBrandName is incorrect", "TEST BRAND", DocWrapper.ProductBrandName);
		}

		#endregion

		#region TestProductModel

		public void TestProductModel()
		{
			AssertEquals("DocWrapper ProductModel should be empty", ZString.Empty, DocWrapper.ProductModel);
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			DocketLine.SupplierPart.OP_Model = "TEST MODEL";
			AssertEquals("DocWrapper ProductModel is incorrect", "TEST MODEL", DocWrapper.ProductModel);
		}

		#endregion

		#region TestPallets

		public void TestPallets()
		{
			AssertEquals("DocWrapper Pallets should be 0", 0m, DocWrapper.Pallets);

			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			Helper.CreateProductUnit(part, "PLT", 5);

			DocketLine.Docket.WD_OH_Client = org.PK;
			DocketLine.WE_OP = part.PK;
			AssertEquals("DocWrapper Pallets should be 0", 0m, DocWrapper.Pallets);

			DocWrapper.Units = 10;
			AssertEquals("DocWrapper Pallets is incorrect", 2m, DocWrapper.Pallets);

			DocWrapper.Units = 10.121212m;
			AssertEquals("DocWrapper Pallets is incorrect", 2.0m, DocWrapper.Pallets);

			PickLine.WZ_Units = 50m;
			AssertEquals("DocWrapper Pallets should depend on DocWrapper.Units field and not PickLine.WZ_Units", 2.0m, DocWrapper.Pallets);
		}

		#endregion

		#region TestUnits

		public void TestUnits()
		{
			PickLine.WZ_Units = 10;
			DocWrapper = GetNewDocWrapper(PickLine);
			AssertEquals("DocWrapper Units is incorrect", 10m, DocWrapper.Units);

			PickLine.WZ_Units = 20;
			AssertEquals("DocWrapper Units is incorrect", 10m, DocWrapper.Units);

			DocWrapper.Units = 50;
			AssertEquals("DocWrapper Units is incorrect", 50m, DocWrapper.Units);
			AssertEquals("Pick Line Units is incorrect", 20m, PickLine.WZ_Units);
		}

		#endregion

		#region TestPickMethod

		public void TestPickMethod()
		{
			DocketLine.WE_WL = Factory.NewWithValidTestData<WhsLocation>().PK;
			PickLine.InventoryLine.Location.WLV_PickMethod = "ANY";
			AssertEquals("DocWrapper PickMethod is incorrect", "Any", DocWrapper.PickMethod);

			DocketLine.WE_WL = ZGuid.Empty;
			AssertEquals("Should not blow up if no location on inventory", ZString.Empty, DocWrapper.PickMethod);
		}

		public void TestPickMethod_InTransit()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			var pickMethod = pickMethods.AddNew();
			pickMethod.Code = "TRB";
			pickMethod.Bool = true;
			pickMethod.Description = (NoResString)"Tractor Beam";

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickMethods))
			{
				data.Whs1.DefaultLocation.WLV_PickMethod = "TRB";

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				Helper.CreatePickNew(order);

				var pickLine = orderLine.PickLines[0];
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

				var newPickLine = orderLine.PickLines[0];
				var wrapper = GetNewDocWrapper(newPickLine);
				AssertEquals("PickMethod should come from originally Picked Inventory.", "Tractor Beam", wrapper.PickMethod);
			}
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			AssertEquals("DocWrapper Pallet ID should be empty", ZString.Empty, DocWrapper.PalletID);
			DocketLine.WE_PalletID = "P_ID_001";
			AssertEquals("DocWrapper Pallet ID is incorrect", "P_ID_001", DocWrapper.PalletID);
		}

		public void TestPalletID_InTransit()
		{
			ClearFactoryBeforeSave();

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines[0];
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_PalletID = ""; // clear pallet id

			var newPickLine = orderLine.PickLines[0];
			var wrapper = GetNewDocWrapper(newPickLine);
			AssertEquals("PalletID should come from originally Picked Inventory.", "PLT-1", wrapper.PalletID);
		}

		#endregion

		#region TestReleaseUnitsAndUQ

		public void TestReleaseUnitsAndUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			data.Part1.OP_StockKeepingUnit = "PCE"; // it should not be UNT.
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			Helper.CreateProductUnit(data.Part1, "UNT", 2); // previous defect was always doing UNT conversion, so setup a conversion to make sure it isn't used.

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			PickLine.WZ_WE_TransactionLine = order.Lines.AddNew().PK;
			PickLine.InventoryLine.WE_OP = data.Part1.PK;

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);

			DocWrapper.Units = 15m;
			AssertEquals("DocWrapper ReleaseUnitsAndUQ is incorrect.", "", DocWrapper.ReleaseUnitsAndUQ);

			productParams.W3_F3_NKReleasedPackType = "BOX";
			AssertEquals("DocWrapper ReleaseUnitsAndUQ is incorrect.", "1.5 BOXES", DocWrapper.ReleaseUnitsAndUQ);

			DocWrapper.Units = 25m;
			AssertEquals("DocWrapper ReleaseUnitsAndUQ is incorrect.", "2.5 BOXES", DocWrapper.ReleaseUnitsAndUQ);

			PickLine.WZ_Units = 50m;
			AssertEquals("DocWrapper ReleaseUnitsAndQU should be dependant on DocWrapper.Units rather that PickLine.WZ_Units.", "2.5 BOXES", DocWrapper.ReleaseUnitsAndUQ);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		WhsPickLine PickLine
		{
			get
			{
				if (pickLine == null)
				{
					pickLine = Factory.New<WhsPickLine>();
					pickLine.WZ_WE_InventoryLine = DocketLine.PK;
				}
				return pickLine;
			}
			set { pickLine = value; }
		}

		WhsDocketLine DocketLine
		{
			get { return docketLine ?? (docketLine = Factory.NewWithValidTestData<WhsReceiveLine>()); }
			set { docketLine = value; }
		}

		DocWhsPickLine DocWrapper
		{
			get { return docWrapper ?? (docWrapper = GetNewDocWrapper(PickLine)); }
			set { docWrapper = value; }
		}

		protected virtual DocWhsPickLine GetNewDocWrapper(WhsPickLine pickLine)
		{
			return DocWhsPickLine.New(pickLine, Factory);
		}

		#region ClearFactoryBeforeSave

		void ClearFactoryBeforeSave()
		{
			// needed to be able to use Factory.Save() when creating a Pick.
			DocWrapper.Delete();
			PickLine.Delete();
			DocketLine.Delete();
		}

		#endregion

		WhsTestHelperFunctions helper;
		WhsPickLine pickLine;
		WhsDocketLine docketLine;
		DocWhsPickLine docWrapper;

		#endregion
	}
}
