using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsOrder))]
	sealed class DocWhsOrderTest : DocWhsPickableDocketTest<WhsOrder, DocWhsOrder>
	{
		#region New

		public void TestNewMethodWithOrderDocument()
		{
			AssertNotNull("Precondition: Order should not be null", Order);
			WhsDocketLabelControl docketLabel = new WhsDocketLabelControl(Order, 1);
			OrderWrapper = DocWhsOrder.New(docketLabel, Factory);
			AssertEquals(typeof(WhsOrder), OrderWrapper.WrappedObject.GetType());
			AssertEquals(Order, (WhsOrder)OrderWrapper.WrappedObject);
		}

		#endregion

		#region Customs Stuff

		public void TestWarehouseCCPCode()
		{
			WhsWarehouse whs = Factory.New<WhsWarehouse>();
			whs.WW_OA_WarehouseAddress = SetupClientCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1234A").MainAddress.PK;
			Order.WD_WW_Whs = whs.PK;
			AssertEquals("1234A", OrderWrapper.WarehouseCCPCode);
		}

		public void TestClientCPC()
		{
			Order.WD_OH_Client = SetupClientCode(OrgCusCode.CodeTypes.CustomsCPPermitCode, "1234A").PK;
			AssertEquals("1234A", OrderWrapper.ClientCPC);
		}

		public void TestClientGCR()
		{
			Order.WD_OH_Client = SetupClientCode(OrgCusCode.CodeTypes.CorporationCode, "1234A").PK;
			AssertEquals("1234A", OrderWrapper.ClientGCR);
		}

		public void TestCP_IssueNo()
		{
			Order.WD_OH_Client = SetupClientCode(OrgCusCode.CodeTypes.CustomsCPPermitCode, "1234A").PK;
			Order.WD_DocketID = "W12345678";
			AssertEquals("1234A12345678", OrderWrapper.CP_IssueNo);
		}

		public void TestACSEstCode()
		{
			AssertEquals("", OrderWrapper.ACSEstCode);
			Order.ConsigneePK = SetupClientCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1234A").PK;
			AssertEquals("1234A", OrderWrapper.ACSEstCode);
		}

		public void TestATOEstCode()
		{
			WhsWarehouse whs = Factory.New<WhsWarehouse>();
			whs.WW_OA_WarehouseAddress = SetupClientCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1234A").MainAddress.PK;
			Order.WD_WW_Whs = whs.PK;
			AssertEquals("1234A", OrderWrapper.ATOEstCode);
		}

		#endregion

		#region Related Business Objects

		#region Collections

		#region TestBillOfLadingPackingLines

		public void TestBillOfLadingPackingLines()
		{
			var orderWrapper = GetWrapperSetupForBillOfLadingPackingLinesTests();
			AssertEquals("Count should be 1", 1, orderWrapper.BillOfLadingPackingLines.Count);
			AssertEquals(22m, orderWrapper.BillOfLadingPackingLines[0].GroupedLineUnitsMet);
		}

		public void TestBillOfLadingPackingLinesUS()
		{
			var orderWrapper = GetWrapperSetupForBillOfLadingPackingLinesTests();
			AssertEquals("Count should be 1", 1, orderWrapper.BillOfLadingPackingLinesUS.Count);
			AssertEquals(22m, orderWrapper.BillOfLadingPackingLinesUS[0].GroupedLineUnitsMet);
		}

		DocWhsOrder GetWrapperSetupForBillOfLadingPackingLinesTests()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 22m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 22m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Factory.Save();

			var pick = helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);

			orderLine1.ReleaseLines[0].Quantity = 10m;
			orderLine1.ReleaseLines[0].PartAttribute1 = "A";

			orderLine1.ReleaseLines.AddNew().Quantity = 12m;
			orderLine1.ReleaseLines[1].PartAttribute1 = "B";
			AssertEquals("Precondition", 2, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 0, orderLine2.ReleaseLines.Count);

			return DocWhsOrder.New(order, Factory);
		}

		#endregion

		public void TestDeliveryLabels()
		{
			Order.WD_PackagesSent = 10;
			AssertEquals(0, OrderWrapper.DeliveryLabels.Count);

			WhsDocketLabelControl docketLabel = new WhsDocketLabelControl(Order, 1);
			OrderWrapper = DocWhsOrder.New(docketLabel, Factory);
			int counter = 0;
			docketLabel.NumberOfLabelsToPrint = 5;
			AssertEquals(5, OrderWrapper.DeliveryLabels.Count);
			foreach (DocWhsLabel label in OrderWrapper.DeliveryLabels)
			{
				counter++;
				AssertEquals(counter, label.LabelNumber);
			}

			counter = 0;
			docketLabel.NumberOfLabelsToPrint = 10;
			AssertEquals(10, OrderWrapper.DeliveryLabels.Count);
			foreach (DocWhsLabel label in OrderWrapper.DeliveryLabels)
			{
				counter++;
				AssertEquals(counter, label.LabelNumber);
			}
		}

		#region TestPackageLabels

		public void TestPackageLabels()
		{
			AssertEquals(0, OrderWrapper.PackageLabels.Count);

			var docketLabel = new WhsDocketLabelControl(Order, 1);
			OrderWrapper = DocWhsOrder.New(docketLabel, Factory);
			WhsOrderLine line1 = Order.Lines.AddNew();
			WhsOrderLine line2 = Order.Lines.AddNew();
			line1.WE_PackQuantity = 2.1;
			line2.WE_PackQuantity = 3.8;
			AssertEquals(7, OrderWrapper.PackageLabels.Count);

			int counter = 1;
			foreach (DocWhsLabel label in OrderWrapper.PackageLabels)
			{
				AssertEquals(counter++, label.LabelNumber);
			}
		}

		#endregion

		#region TestPackageLabelsForBOM

		public void TestPackageLabelsForBOM()
		{
			OrderWrapper.Delete();
			Order.Delete();

			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts(saveFactoryForWarehouse: true);
			Helper.CreateProductUnit(data.BOM.Bike, "UNT", "PLT", 5m);

			// create some parts
			OrgSupplierPart laptop = Helper.CreateProduct(data.Org1, "Laptop");
			OrgSupplierPart tV = Helper.CreateProduct(data.Org1, "TV");

			OrgSupplierPart tvCable = Helper.CreateProduct(data.Org1, "Cable");
			OrgSupplierPart tvScreen = Helper.CreateProduct(data.Org1, "Screen");

			OrgPartBOM tvCableBOMPart = Helper.CreateProductBOM(tV, tvCable, 1, "UNT");
			OrgPartBOM tvScreenBOMPart = Helper.CreateProductBOM(tV, tvScreen, 1, "UNT");

			// order the parts
			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order, data.BOM.Bike, 4m);
			orderLine1.WE_F3_NKPackType = "PLT";
			orderLine1.WE_PackQuantity = 0.8m;
			WhsOrderLine orderLine2 = Helper.CreateWhsOrderLine(order, laptop, 2m);
			WhsOrderLine orderLine3 = Helper.CreateWhsOrderLine(order, tV, 3m);

			Factory.Save(); // All Products and Orders will be in the DB prior to printing documents (ediEnterprise Docs requirement)
			int workOrdersCountBefore = Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder)).Length;

			AssertEquals(true, orderLine1.IsBOMProduct);
			AssertEquals(false, orderLine2.IsBOMProduct);
			AssertEquals(true, orderLine3.IsBOMProduct);

			var docketLabel = new WhsDocketLabelControl(order, 1);
			var orderWrapper = DocWhsOrder.New(docketLabel, Factory);

			AssertEquals(6, orderWrapper.PackageLabels.Count);
			const int bikeLabels = 14;
			const int tvLabels = 3 * 3;
			const int laptopLabels = 2;
			AssertEquals(bikeLabels + tvLabels + laptopLabels, orderWrapper.PackageLabelsForBOM.Count);

			int counter = 0;
			int subLabelCounter = 0;
			List<int> labelQuantity = new List<int>();
			List<String> labelProduct = new List<String>();

			foreach (DocWhsPackageLabel label in orderWrapper.PackageLabels)
			{
				counter++;
				AssertEquals(counter, label.LabelNumber);
				foreach (DocWhsPackageLabel subLabel in orderWrapper.PackageLabelsForBOM)
				{
					if (label.LabelNumber == subLabel.LabelNumber)
					{
						subLabelCounter++;
					}
				}
				labelProduct.Add(label.ProductCode);
				labelQuantity.Add(subLabelCounter);

				subLabelCounter = 0;
			}

			for (int i = 0; i < counter; i++)
			{
				if (labelProduct[i].ToUpper() == "LAPTOP")
				{
					AssertEquals("Each Laptop should have 1 label.", 1, labelQuantity[i]);
				}
				else if (labelProduct[i].ToUpper() == "MOTORBIKE")
				{
					AssertEquals("Each Motorbike should have 14 labels.", 14, labelQuantity[i]);
				}
				else
				{
					AssertEquals("Each TV should have 3 labels.", 3, labelQuantity[i]);
				}
			}

			order.WD_ExternalReference = "Changed Value";
			AssertEquals("No errors", true, !order.HasErrors);
			Factory.Save();  // Virtual aka temp Work Order shouldn't save nor affect an Order save.
			int workOrdersCountAfter = Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder)).Length;
			AssertEquals("Still no errors after creating a virtual work order", true, !order.HasErrors);
			AssertEquals("No new work orders", workOrdersCountBefore, workOrdersCountAfter);
		}

		#endregion

		public void TestLines()
		{
			AssertEquals("Initially no lines", 0, DocketWrapper.Lines.Count);
			Docket.Lines.AddNew();
			Docket.Lines.AddNew();
			AssertEquals("2 lines", 2, DocketWrapper.Lines.Count);
		}

		#region TestLinesSort

		public void TestLinesSort()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			Order.WD_OH_Client = orgHeader.PK;

			WhsOrderLine line1 = CreateOrderLine(Order, "C3", "DESC2", 1);
			WhsOrderLine line2 = CreateOrderLine(Order, "C1", "DESC3", 3);
			WhsOrderLine line3 = CreateOrderLine(Order, "C2", "DESC1", 2);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductCode;
			AssertLinesSortedOrder(line2, line3, line1);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductDescription;
			AssertLinesSortedOrder(line3, line1, line2);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.LineNo;
			AssertLinesSortedOrder(line1, line3, line2);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.SameAsOnGrid;
			AssertLinesSortedOrder(line1, line2, line3);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = "DEF";
			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductCode);
			AssertLinesSortedOrder(line2, line3, line1);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductDescription);
			AssertLinesSortedOrder(line3, line1, line2);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.LineNo);
			AssertLinesSortedOrder(line1, line3, line2);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.SameAsOnGrid);
			AssertLinesSortedOrder(line1, line2, line3);
		}

		WhsOrderLine CreateOrderLine(WhsOrder order, ZString partCode, ZString partDesc, ZShort lineNo)
		{
			WhsOrderLine line = order.Lines.AddNew();
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partCode;
			part.OP_Desc = partDesc;
			line.WE_OP = part.PK;
			line.WE_LineNo = lineNo;
			return line;
		}

		void AssertLinesSortedOrder(WhsOrderLine expectedLine1, WhsOrderLine expectedLine2, WhsOrderLine expectedLine3)
		{
			DocWhsPickableDocket orderWrapper = CreateWhsDocketWrapper(Order);
			ZString sortedBy = Order.Client.MiscServ.OM_WhsPackingSlipOrderBy_List.GetDescriptionFromCode(Order.Client.MiscServ.OM_WhsPackingSlipOrderBy);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine1, orderWrapper.OrderLines[0].WrappedObject);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine2, orderWrapper.OrderLines[1].WrappedObject);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine3, orderWrapper.OrderLines[2].WrappedObject);
		}

		#endregion

		#region TestPackingLines

		#region TestPackingLines_SortCore

		protected override void TestPackingLines_SortCore()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			PickableDocket.WD_OH_Client = orgHeader.PK;

			var line1 = CreateOrderLine(Order, "C3", "DESC2", 1);
			var line2 = CreateOrderLine(Order, "C1", "DESC3", 3);
			var line3 = CreateOrderLine(Order, "C2", "DESC1", 2);

			var pick = Factory.New<WhsPick>();
			PickableDocket.WD_WP = pick.PK;

			CreateReleaseLine(line1);
			CreateReleaseLine(line2);
			CreateReleaseLine(line3);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductCode;
			AssertPackingLinesSortedOrder("00003", "00001", "00002");

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductDescription;
			AssertPackingLinesSortedOrder("00002", "00003", "00001");

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.LineNo;
			AssertPackingLinesSortedOrder("00001", "00003", "00002");

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.SameAsOnGrid;
			AssertPackingLinesSortedOrder("00001", "00002", "00003");

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = "DEF";
			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductCode);
			AssertPackingLinesSortedOrder("00003", "00001", "00002");

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductDescription);
			AssertPackingLinesSortedOrder("00002", "00003", "00001");

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.LineNo);
			AssertPackingLinesSortedOrder("00001", "00003", "00002");

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.SameAsOnGrid);
			AssertPackingLinesSortedOrder("00001", "00002", "00003");
		}

		#endregion

		#region TestPackingLines_RollingUpCore

		protected override void TestPackingLines_RollingUpCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var relation2 = data.Part2.RelatedOrganisations[0];
			relation2.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation2.OU_RollUpAttributesOnDocuments = true;

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var relation3 = part3.RelatedOrganisations[0];
			relation3.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation3.OU_RollUpAttributesOnDocuments = true;

			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var relation4 = part4.RelatedOrganisations[0];
			relation4.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation4.OU_RollUpAttributesOnDocuments = false;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, part3, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, part4, true, useSerialNumber: false);

			var tomorrow = ZDate.Today.AddDays(1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 100m, null, tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, part4, 100m, null, tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);

			// Part1 - Not Attribute Neutral.
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1_1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var orderLine1_2 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var orderLine1_3 = Helper.CreateWhsOrderLine(order, data.Part1, 2m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");

			// Part2 - Attribute Neutral and Roll Up.
			var orderLine2_1 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var orderLine2_2 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var orderLine2_3 = Helper.CreateWhsOrderLine(order, data.Part2, 2m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");

			// Part3 - Attribute Neutral and Roll Up. 
			var orderLine3_1 = Helper.CreateWhsOrderLine(order, part3, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			var orderLine3_2 = Helper.CreateWhsOrderLine(order, part3, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");

			// Part4 - Attribute Neutral and No Roll Up.
			var orderLine4_1 = Helper.CreateWhsOrderLine(order, part4, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			var orderLine4_2 = Helper.CreateWhsOrderLine(order, part4, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			Factory.Save();

			Helper.CreatePickNew(order);

			// Part1 Attribute Lines - with no Attribute Neutral on a Product and No Roll Up.
			orderLine1_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_1 = CreateReleaseLine(orderLine1_1, 1m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine1_2 = CreateReleaseLine(orderLine1_1, 1m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine1_3 = CreateReleaseLine(orderLine1_1, 1m, "PA1", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine1_4 = CreateReleaseLine(orderLine1_1, 1m, "PA1", "PA2", "", "SN1", ZDateTime.Empty, ZDateTime.Empty);

			orderLine1_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_5 = CreateReleaseLine(orderLine1_2, 1m, "PA1", "PA2", "PA3", "SN1", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine1_6 = CreateReleaseLine(orderLine1_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, ZDateTime.Empty);
			var attribLine1_7 = CreateReleaseLine(orderLine1_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);
			var attribLine1_8 = CreateReleaseLine(orderLine1_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			// Part1 Attribute Lines - with no Attribute Neutral on a Product and No Roll Up. Different OrderLine but same Attributes shouldn't roll up.
			orderLine1_3.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_9 = CreateReleaseLine(orderLine1_3, 2m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);

			// Part2 Attribute Lines - with an Attribute Neutral on a Product and Roll Up.
			orderLine2_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine2_1 = CreateReleaseLine(orderLine2_1, 1m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine2_2 = CreateReleaseLine(orderLine2_1, 1m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine2_3 = CreateReleaseLine(orderLine2_1, 1m, "PA1", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine2_4 = CreateReleaseLine(orderLine2_1, 1m, "PA1", "PA2", "", "SN1", ZDateTime.Empty, ZDateTime.Empty);

			orderLine2_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine2_5 = CreateReleaseLine(orderLine2_2, 1m, "PA1", "PA2", "PA3", "SN1", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine2_6 = CreateReleaseLine(orderLine2_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, ZDateTime.Empty);
			var attribLine2_7 = CreateReleaseLine(orderLine2_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);
			var attribLine2_8 = CreateReleaseLine(orderLine2_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			// Part2 Attribute Lines - with an Attribute Neutral on a Product and Roll Up. Different OrderLine but same Attributes should be rolled up.
			orderLine2_3.ReleaseLines.RemoveAndDeleteAll();
			var attribLine2_9 = CreateReleaseLine(orderLine2_3, 2m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);

			// Part3 Attribute Lines - with an Attribute Neutral on a Product and Roll Up.
			orderLine3_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine3_1 = CreateReleaseLine(orderLine3_1, 5m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			orderLine3_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine3_2 = CreateReleaseLine(orderLine3_2, 5m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			// Part4 Attribute Lines - with an Attribute Neutral on a Product and No Roll Up.
			orderLine4_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine4_1 = CreateReleaseLine(orderLine4_1, 3m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			orderLine4_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine4_2 = CreateReleaseLine(orderLine4_2, 3m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			var orderWrapper = DocWhsOrder.New(order, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("22 Original attribute lines should be rolled up into 9 + 2 + 2 + 2  = 14 attribute lines.", 15, orderWrapper.PackingLines.Count);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 1m, attribLine1_1);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 1m, attribLine1_2);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 1m, attribLine1_3);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 1m, attribLine1_4);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 1m, attribLine1_5);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 1m, attribLine1_6);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 1m, attribLine1_7);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 1m, attribLine1_8);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 2m, attribLine1_9);

				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 6m, orderLine2_1); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 6m, orderLine2_1); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 6m, orderLine2_1); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 6m, orderLine2_1); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 4m, orderLine2_2); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 4m, orderLine2_2); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 4m, orderLine2_2); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 4m, orderLine2_2); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 6m, orderLine2_3); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9

				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 10m, orderLine3_1); // Should Be Rolled Up AttribLine3_1, 3_2
				AssertRolledUpDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 10m, orderLine3_2); // Should Be Rolled Up AttribLine3_1, 3_2

				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 3m, attribLine4_1);
				AssertDocWhsPackingSlipLineExist(orderWrapper.PackingLines, 3m, attribLine4_2);
			});
		}

		void AssertDocWhsPackingSlipLineExist(DocWhsPackingSlipLineCollection docWhsPackingSlipLineCollection, ZDecimal expectedQuantity, WhsReleaseLine expectedAttributes)
		{
			bool result = false;
			foreach (DocWhsPackingSlipLine packingLine in docWhsPackingSlipLineCollection)
			{
				if (packingLine.PartAttrib1 == expectedAttributes.PartAttribute1 &&
					packingLine.PartAttrib2 == expectedAttributes.PartAttribute2 &&
					packingLine.PartAttrib3 == expectedAttributes.PartAttribute3 &&
					packingLine.TrackedSerialNumber == expectedAttributes.SerialNumber &&
					packingLine.ExpiryDate == expectedAttributes.ExpiryDate &&
					packingLine.PackingDate == expectedAttributes.PackingDate &&
					((WhsReleaseLine)packingLine.WrappedObject).Quantity == expectedQuantity)
				{
					result = true;
					break;
				}
			}
			Assert("DocWhsPackingSlipLine with set parameters couldn't be found", result);
		}

		void AssertRolledUpDocWhsPackingSlipLineExist(DocWhsPackingSlipLineCollection docWhsPackingSlipLineCollection, ZDecimal expectedQuantity, WhsPickableDocketLine parent)
		{
			bool result = false;
			foreach (DocWhsPackingSlipLine packingLine in docWhsPackingSlipLineCollection)
			{
				if (packingLine.PartAttrib1 == parent.WE_PartAttrib1 &&
					packingLine.PartAttrib2 == parent.WE_PartAttrib2 &&
					packingLine.PartAttrib3 == parent.WE_PartAttrib3 &&
					packingLine.ExpiryDate == parent.WE_ExpiryDate &&
					packingLine.PackingDate == parent.WE_PackingDate &&
					packingLine.LineUnitsMet == expectedQuantity)
				{
					result = true;
					break;
				}
			}
			Assert("DocWhsPackingSlipLine with set parameters couldn't be found", result);
		}

		#endregion

		#endregion

		#region TestRolledUpLinesForOrderCopy

		#region TestRolledUpLinesForOrderCopy

		public void TestRolledUpLinesForOrderCopy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDate.Today;

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			data.Part2.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			part3.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			part3.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;

			Order.WD_OH_Client = data.Org1.PK;
			var line1_1 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line1_2 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line1_3 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			var line1_4 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "", "");
			var line1_5 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line1_6 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, today, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line1_7 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			var line1_8 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			var line1_9 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line1_9.WE_LineComment = "COMMENT1";

			// attribute neutral, no roll up.
			var line2_1 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line2_2 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line2_3 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			var line2_4 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "", "");
			var line2_5 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line2_6 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, today, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line2_7 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			var line2_8 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			var line2_9 = Helper.CreateWhsOrderLine(Order, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line2_9.WE_LineComment = "COMMENT2";

			//attribute neutral, roll up.
			var line3_1 = Helper.CreateWhsOrderLine(Order, part3, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line3_2 = Helper.CreateWhsOrderLine(Order, part3, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var line3_3 = Helper.CreateWhsOrderLine(Order, part3, 1m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			var line3_4 = Helper.CreateWhsOrderLine(Order, part3, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "", "");
			var line3_5 = Helper.CreateWhsOrderLine(Order, part3, 1m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line3_6 = Helper.CreateWhsOrderLine(Order, part3, 1m, today, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			var line3_7 = Helper.CreateWhsOrderLine(Order, part3, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			var line3_8 = Helper.CreateWhsOrderLine(Order, part3, 1m, today, today, "PA1", "PA2", "PA3", "", "");
			var line3_9 = Helper.CreateWhsOrderLine(Order, part3, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			line3_9.WE_LineComment = "COMMENT3";

			AssertEquals("27 Original Order lines should be rolled up into 9 + 9 + 7 = 25 Order lines", 25, OrderWrapper.RolledUpLinesForOrderCopy.Count);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_1);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_2);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_3);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_4);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_5);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_6);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_7);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_8);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line1_9);

			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_1);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_2);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_3);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_4);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_5);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_6);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_7);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_8);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line2_9);

			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 2m, line3_1); // line31 and line32 are rolled up.
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 2m, line3_2); // line31 and line32 are rolled up.
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line3_3);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line3_4);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line3_5);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line3_6);
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 2m, line3_7); // line37 and line38 are rolled up.
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 2m, line3_8); // line37 and line38 are rolled up.
			AssertDocWhsOrderLineExist(OrderWrapper.RolledUpLinesForOrderCopy, 1m, line3_9);
		}

		void AssertDocWhsOrderLineExist(DocWhsOrderLineCollection docOrderLines, ZDecimal expectedQuantity, WhsOrderLine expectedOrderLine)
		{
			foreach (DocWhsPickableDocketLine docOrderLine in docOrderLines)
			{
				if (docOrderLine.ProductCode == expectedOrderLine.SupplierPart.OP_PartNum &&
					docOrderLine.Units == expectedQuantity &&
					docOrderLine.LineComment == expectedOrderLine.WE_LineComment &&
					docOrderLine.PartAttribute1 == expectedOrderLine.WE_PartAttrib1 &&
					docOrderLine.PartAttribute2 == expectedOrderLine.WE_PartAttrib2 &&
					docOrderLine.PartAttribute3 == expectedOrderLine.WE_PartAttrib3 &&
					docOrderLine.ExpiryDate == expectedOrderLine.WE_ExpiryDate &&
					docOrderLine.PackingDate == expectedOrderLine.WE_PackingDate)
				{
					return;
				}
			}
			Assert("The line you are expecting doesn't exist.", false);
		}

		#endregion

		#region TestRolledUpLinesForOrderCopy_Sorting

		public void TestRolledUpLinesForOrderCopy_Sorting()
		{
			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			Order.WD_OH_Client = orgHeader.PK;

			WhsOrderLine line1 = CreateOrderLine(Order, "C3", "DESC2", 1);
			WhsOrderLine line2 = CreateOrderLine(Order, "C1", "DESC3", 3);
			WhsOrderLine line3 = CreateOrderLine(Order, "C2", "DESC1", 2);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductCode;
			AssertRolledUpLinesForOrderCopySortedOrder(line2, line3, line1);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductDescription;
			AssertRolledUpLinesForOrderCopySortedOrder(line3, line1, line2);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.LineNo;
			AssertRolledUpLinesForOrderCopySortedOrder(line1, line3, line2);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.SameAsOnGrid;
			AssertRolledUpLinesForOrderCopySortedOrder(line1, line2, line3);

			Order.Client.MiscServ.OM_WhsPackingSlipOrderBy = "DEF";
			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductCode);
			AssertRolledUpLinesForOrderCopySortedOrder(line2, line3, line1);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductDescription);
			AssertRolledUpLinesForOrderCopySortedOrder(line3, line1, line2);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.LineNo);
			AssertRolledUpLinesForOrderCopySortedOrder(line1, line3, line2);

			WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.SameAsOnGrid);
			AssertRolledUpLinesForOrderCopySortedOrder(line1, line2, line3);
		}

		void AssertRolledUpLinesForOrderCopySortedOrder(WhsOrderLine expectedLine1, WhsOrderLine expectedLine2, WhsOrderLine expectedLine3)
		{
			DocWhsOrder orderWrapper = CreateWhsDocketWrapper(Order);
			ZString sortedBy = Order.Client.MiscServ.OM_WhsPackingSlipOrderBy_List.GetDescriptionFromCode(Order.Client.MiscServ.OM_WhsPackingSlipOrderBy);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine1, orderWrapper.RolledUpLinesForOrderCopy[0].WrappedObject);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine2, orderWrapper.RolledUpLinesForOrderCopy[1].WrappedObject);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine3, orderWrapper.RolledUpLinesForOrderCopy[2].WrappedObject);
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region TestCartageDropMode

		public void TestCartageDropMode()
		{
			AssertEquals("Precondition:", "", OrderWrapper.CartageDropMode);

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "AU";
			consignee.OH_FullName = "My Organisation Name";

			OrgAddress orgAddress = consignee.Addresses[0];
			orgAddress.OA_LCLEquipmentNeeded = Core.Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			orgAddress.OA_FCLEquipmentNeeded = Core.Constants.FCLEquipmentNeeded.WaitForUnpack;

			Order.ConsigneePK = consignee.PK;
			Order.WD_DropMode = "";
			AssertEquals("Should have loaded Cartage Drop Mode for LCL freight", "HUL - Hand Unload/Load by Premise", DocketWrapper.CartageDropMode);

			Order.Containers.AddNew().WC_ContainerNum = "C0001";
			AssertEquals("Should have loaded Cartage Drop Mode for FCL freight", "WUP - Wait for Pack/Unpack", DocketWrapper.CartageDropMode);

			Order.WD_DropMode = Core.Constants.EquipmentNeeded.Ask;
			AssertEquals("Should be the order drop mode", "ASK - Ask Client", DocketWrapper.CartageDropMode);
		}

		#endregion

		#region Properties

		#region ZDateTime

		protected override void TestRequiredDateCore()
		{
			var today = ZDateTime.Today;
			Docket.RequiredDate = today;

			var offset = Docket.Warehouse.GetWarehouseBranchLocalDateTimeOffset(today.ToUniversalBranchTime());
			var expectedRequiredDate = new ZDateTimeOffset(today.Year, today.Month, today.Day, 23, 59, 00, offset.Offset);

			AssertEquals(expectedRequiredDate, DocketWrapper.RequiredDate);
		}

		#endregion

		#region ZInt

		public void TestTotalNumberOfPackageLabels()
		{
			AssertEquals(0, OrderWrapper.TotalNumberOfPackageLabels);

			WhsDocketLabelControl docketLabel = new WhsDocketLabelControl(Order, 1);
			OrderWrapper = DocWhsOrder.New(docketLabel, Factory);
			Order.Lines.AddNew();
			Order.Lines[0].WE_PackQuantity = 5;
			AssertEquals(5, OrderWrapper.TotalNumberOfPackageLabels);
		}

		#endregion

		#region ZString

		#region TestDockDoorLocation

		protected override void TestDockDoorLocationCore()
		{
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save(); // needed as architecture doesn't know that WhsLocationView need to be saved before WHsDocketLine.

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			var orderWrapper = DocWhsOrder.New(order, Factory);
			AssertEquals("Precondition - not picked orders should not have associated DDL.", "", orderWrapper.DockDoorLocation);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Should return default DLL from warehouse.", "DOCKDOOR", orderWrapper.DockDoorLocation);

			pick.WP_WL_DockDoor = locationA1.PK;
			AssertEquals("Should return DLL assigned to the pick.", "A-1", orderWrapper.DockDoorLocation);
		}

		#endregion

		public void TestSpecialInstructions()
		{
			AssertEquals("Special instructions", "", OrderWrapper.SpecialInstructions);
			Order.WD_HandlingInstructions = "Instructions so special they need a special name";
			AssertEquals("Special instructions", "Instructions so special they need a special name", OrderWrapper.SpecialInstructions);
		}

		public void TestPackingSlipTitle()
		{
			WhsWarehouse whs = Factory.New<WhsWarehouse>();
			whs.WW_GB_RelatedCompanyBranch = Factory.New<GlbBranch>().PK;
			WarehouseDataRegistry.Instance.PackingSlipTitles.SetValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, "Despatch Slip");
			Order.WD_WW_Whs = whs.PK;
			AssertEquals("Despatch Slip", OrderWrapper.PackingSlipTitle);

			Order.WD_WW_Whs = ZGuid.Empty;
			AssertEquals("Packing Slip", OrderWrapper.PackingSlipTitle);
		}

		public void TestBarcodeTextForExternalReference()
		{
			TextBarcode barcode = new TextBarcode("W00004352");
			Order.WD_ExternalReference = "W00004352";
			AssertEquals(barcode.TextAs128sFontString, OrderWrapper.BarcodeTextForExternalReference);
		}

		public override void TestPickNo()
		{
			AssertEquals("", DocketWrapper.PickNo);
			WhsPick pick = Factory.New<WhsPick>();
			pick.WP_PickNo = "P00002222";
			Order.WD_WP = pick.PK;
			AssertEquals("P00002222", DocketWrapper.PickNo);
		}

		public void TestEmergencyContactMessageString_Empty()
		{
			AssertEquals(ZString.Empty, OrderWrapper.PrintDGDetails);
			AssertEquals(ZString.Empty, OrderWrapper.EmergencyContactMessageString);
		}

		public void TestEmergencyContactMessageString()
		{
			var undg = Factory.NewWithValidTestData<UNDGSubstance>();

			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			data.Part1.UNDGs.AddNew().DI_DG = undg.PK;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);

			var whsOrderWrapper1 = DocWhsPickableDocket.New(order, Factory);
			AssertEquals("Y", whsOrderWrapper1.PrintDGDetails);
			AssertEquals("Hazardous materials emergency contact number:\r\n ", whsOrderWrapper1.EmergencyContactMessageString);

			var contact = order.Client.Contacts.AddNew();
			contact.OC_ContactName = "A";
			order.Client.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			contact.OC_HomePhone = "123";
			order.Client.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;

			AssertEquals("Hazardous materials emergency contact number:\r\nA 123", whsOrderWrapper1.EmergencyContactMessageString);
		}

		public void TestPrintDGDetails()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);

			AssertEquals(ZString.Empty, DocWhsPickableDocket.New(order, Factory).PrintDGDetails);

			var undg = Factory.NewWithValidTestData<UNDGSubstance>();
			data.Part1.UNDGs.AddNew().DI_DG = undg.PK;

			AssertEquals("Y", DocWhsPickableDocket.New(order, Factory).PrintDGDetails);
		}

		public void TestCurrencySymbol()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			line1.WE_ExtendedLinePrice = 10.00m;
			line1.WE_RX_NKUnitPriceCurrency = "AUD";
			line2.WE_ExtendedLinePrice = 9.67m;
			line2.WE_RX_NKUnitPriceCurrency = "AUD";
			Helper.CreatePickNew(order);

			var whsOrderWrapper1 = DocWhsPickableDocket.New(order, Factory);
			AssertEquals("$", whsOrderWrapper1.CurrencySymbol);

			var line3 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			line3.WE_ExtendedLinePrice = 4.56m;
			line3.WE_RX_NKUnitPriceCurrency = "AFA";
			line3.ReleaseLines.AddNew();

			var whsOrderWrapper2 = DocWhsPickableDocket.New(order, Factory);
			AssertEquals(ZString.Empty, whsOrderWrapper2.CurrencySymbol);
		}

		public void TestPickingInstructions()
		{
			AssertEquals("Document Wrapper PickingInstruction is incorrect", ZString.Empty, OrderWrapper.PickingInstructions);
			StmNote pickingInstructionNote = Order.Notes.AddNew();
			pickingInstructionNote.ST_Description = PredefinedNoteTypes.Instance.PickingInstructions.Description;
			pickingInstructionNote.ST_NoteText = "Order note";
			Order.WD_ExternalReference = "OrderNumber1";
			AssertEquals("Document Wrapper PickingInstruction is incorrect", "Order OrderNumber1 - Order note", OrderWrapper.PickingInstructions);
		}

		public void TestWarehouseCartageCoordinatorName()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}

			Docket.Warehouse.WW_WarehouseName = "Warehouse One";

			OrgHeader whsOrg = Factory.New<OrgHeader>();
			OrgAddress address = whsOrg.MainAddress;
			address.OA_OH = whsOrg.PK;
			Docket.Warehouse.WW_OA_WarehouseAddress = address.PK;

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_FullName = "Person 1";
			glbStaff.GS_Code = "AAA";

			OrgStaffAssignments assignedStaff = whsOrg.StaffAssignments.AddNew();
			assignedStaff.O8_GS_NKPersonResponsible = glbStaff.GS_Code;
			assignedStaff.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;

			AssertEquals("Person 1", DocketWrapper.WarehouseCartageCoordinatorName);
		}

		public void TestWarehouseCartageCoordinatorPhone()
		{
			if (Docket.Warehouse == null)
			{
				Docket.WD_WW_Whs = Factory.New(typeof(WhsWarehouse)).PK;
			}

			Docket.Warehouse.WW_WarehouseName = "Warehouse One";

			OrgHeader whsOrg = Factory.New<OrgHeader>();
			OrgAddress address = whsOrg.MainAddress;
			address.OA_OH = whsOrg.PK;
			Docket.Warehouse.WW_OA_WarehouseAddress = address.PK;

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			glbStaff.GS_FullName = "Person 1";
			glbStaff.GS_Code = "BBB";
			glbStaff.GS_WorkPhone = "02 5555 9999";

			OrgStaffAssignments assignedStaff = whsOrg.StaffAssignments.AddNew();
			assignedStaff.O8_GS_NKPersonResponsible = glbStaff.GS_Code;
			assignedStaff.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;

			AssertEquals("+61 2 5555 9999", DocketWrapper.WarehouseCartageCoordinatorPhone);
		}

		public void TestCartageAdviceOpeningText()
		{
			AssertEquals("Precondition:", "", OrderWrapper.CartageAdviceOpeningText);
			DocumentsDataRegistry.Instance.WarehouseCartageAdviceOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Warehouse Cartage Advice Opening Text");
			AssertEquals("Document isn't Cartage Advice so Opening Text is Empty", "", OrderWrapper.CartageAdviceOpeningText);

			DocketWrapper.SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Cartage Advice Test Document" } });
			AssertEquals("DocumentName", "Cartage Advice Test Document", DocketWrapper.DocumentName);
			AssertEquals("Warehouse Cartage Advice Opening Text", OrderWrapper.CartageAdviceOpeningText);
		}

		public void TestCartageAdviceClosingText()
		{
			AssertEquals("Precondition:", "", OrderWrapper.CartageAdviceClosingText);
			DocumentsDataRegistry.Instance.WarehouseCartageAdviceClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Warehouse Cartage Advice Closing Text");
			AssertEquals("Document isn't Cartage Advice so Closing Text is Empty", "", OrderWrapper.CartageAdviceClosingText);

			DocketWrapper.SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Cartage Advice Test Document" } });
			AssertEquals("DocumentName", "Cartage Advice Test Document", DocketWrapper.DocumentName);
			AssertEquals("Warehouse Cartage Advice Closing Text", OrderWrapper.CartageAdviceClosingText);
		}

		public void TestCargateAdviceContainerNumberAndTypeLine()
		{
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Cartage Advice Container Number And Type Line", ZString.Empty, CreateWhsDocketWrapper(Docket).CargateAdviceContainerNumberAndTypeLine);

			RefContainer ref1 = Factory.New<RefContainer>();
			ref1.RC_Code = "20FR";
			RefContainer ref3 = Factory.New<RefContainer>();
			ref3.RC_Code = "40FR";
			RefContainer ref4 = Factory.New<RefContainer>();
			ref4.RC_Code = "30FR";

			WhsDocketContainer container1 = Docket.Containers.AddNew();
			container1.WC_ContainerNum = "CONTAINER1";
			container1.WC_RC = ref1.PK;

			Docket.Containers.AddNew().WC_ContainerNum = "CONTAINER2";

			WhsDocketContainer container3 = Docket.Containers.AddNew();
			container3.WC_ContainerNum = "CONTAINER3";
			container3.WC_RC = ref3.PK;

			WhsDocketContainer container4 = Docket.Containers.AddNew();
			container4.WC_ContainerNum = "CONTAINER4";
			container4.WC_RC = ref4.PK;

			Docket.Containers.AddNew().WC_ContainerNum = "CONTAINER5";
			AssertEquals("Cartage Advice Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5", DocketWrapper.CargateAdviceContainerNumberAndTypeLine);

			Docket.Containers.AddNew().WC_ContainerNum = "CONTAINER6";
			AssertEquals("Cartage Advice Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5 ...", CreateWhsDocketWrapper(Docket).CargateAdviceContainerNumberAndTypeLine);

			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Cartage Advice Container Number And Type Line", "CONTAINER1 (20FR), CONTAINER2, CONTAINER3 (40FR), CONTAINER4 (30FR), CONTAINER5 ...", CreateWhsDocketWrapper(Docket).CargateAdviceContainerNumberAndTypeLine);
		}

		#endregion

		#region ZDecimal

		public void TestTotalGroupedLineUnitsMet_Empty()
		{
			AssertEquals(ZDecimal.Zero, OrderWrapper.TotalGroupedLineUnitsMet);
		}

		public void TestTotalGroupedLineUnitsMet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 22m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);

			orderLine1.ReleaseLines[0].Quantity = 9m;
			orderLine1.ReleaseLines[0].PartAttribute1 = "A";

			orderLine1.ReleaseLines.AddNew().Quantity = 1m;
			orderLine1.ReleaseLines[1].PartAttribute1 = "B";

			var whsOrderWrapper1 = DocWhsPickableDocket.New(order, Factory);
			AssertEquals(10m, whsOrderWrapper1.TotalGroupedLineUnitsMet);
		}

		public void TestTotalGroupedLineUnitsWeight()
		{
			AssertEquals(ZDecimal.Zero, OrderWrapper.TotalGroupedLineUnitsWeight);

			Order.WD_WeightSent = 140m;

			DocWhsPickableDocket whsOrderWrapper1 = DocWhsPickableDocket.New(Order, Factory);
			AssertEquals(140m, whsOrderWrapper1.TotalGroupedLineUnitsWeight);
		}

		public void TestTotalExtendedLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			line1.WE_ExtendedLinePrice = 10.00m;
			line2.WE_ExtendedLinePrice = 9.67m;
			Helper.CreatePickNew(order);

			var whsOrderWrapper1 = DocWhsPickableDocket.New(order, Factory);
			AssertEquals(19.67m, whsOrderWrapper1.TotalExtendedLinePrice);
			AssertEquals("$19.67", whsOrderWrapper1.TotalExtendedLinePriceWithSymbol);
		}

		public void TestTotalTopLevelUnitsMet_TotalTopLevelUnitsOrdered_Empty()
		{
			AssertEquals(ZDecimal.Zero, OrderWrapper.TotalTopLevelUnitsMet);
			AssertEquals(ZDecimal.Zero, OrderWrapper.TotalTopLevelUnitsOrdered);
		}

		public void TestTotalTopLevelUnitsMet_TotalTopLevelUnitsOrdered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			Factory.Save();

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 12m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 38m);

			var pick = helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, orderLine1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, orderLine2.ReleaseLines.Count);

			var packingSlipWrapper1 = DocWhsPackingSlipLine.New(orderLine1.ReleaseLines[0], orderLine1, Factory);
			var line1UnitMet = packingSlipWrapper1.TopLevelUnitsMet;
			var line1UnitOrd = packingSlipWrapper1.TopLevelUnitsOrdered;

			var packingSlipWrapper2 = DocWhsPackingSlipLine.New(orderLine2.ReleaseLines[0], orderLine2, Factory);
			var line2UnitMet = packingSlipWrapper2.TopLevelUnitsMet;
			var line2UnitOrd = packingSlipWrapper2.TopLevelUnitsOrdered;

			var whsOrderWrapper1 = DocWhsPickableDocket.New(order, Factory);
			AssertEquals("TotalTopLevelUnitsMet", line1UnitMet + line2UnitMet, whsOrderWrapper1.TotalTopLevelUnitsMet);
			AssertEquals("TotalTopLevelUnitsOrdered", line1UnitOrd + line2UnitOrd, whsOrderWrapper1.TotalTopLevelUnitsOrdered);
		}

		public void TestCollectionFee()
		{
			AssertEquals(0m, OrderWrapper.CollectionFee);
		}

		public void TestTotalCODCharges()
		{
			AssertEquals(0m, OrderWrapper.TotalCODCharges);
			Order.WD_ShipperCODAmount = 12m;
			AssertEquals(0m, OrderWrapper.TotalCODCharges);

			Order.WD_INCO = "FCD";
			AssertEquals(12m, OrderWrapper.TotalCODCharges);
		}

		#endregion

		#region ZBool

		#region TestAutoPrintOrderCopyForMOP

		public void TestAutoPrintOrderCopyForMOP()
		{
			Order.WD_WW_Whs = ZGuid.Empty;
			AssertNull("Pre-Condition", Order.Warehouse);
			AssertEquals(false, OrderWrapper.AutoPrintOrderCopyForMOP);

			WhsWarehouse whs = Factory.New<WhsWarehouse>();
			Order.WD_WW_Whs = whs.PK;

			whs.WW_AutoPrintOrderCopyForMOPOnPick = false;
			AssertEquals(false, OrderWrapper.AutoPrintOrderCopyForMOP);

			whs.WW_AutoPrintOrderCopyForMOPOnPick = true;
			AssertEquals(true, OrderWrapper.AutoPrintOrderCopyForMOP);
		}

		#endregion

		#endregion

		#endregion

		#region IDocServicesParent Members

		public new void TestPackages()
		{
			Order.WD_PackagesSent = 2;
			AssertEquals("2 Sent", ServicesParentWrapper.Packages);
		}

		public new void TestWeight()
		{
			Order.WD_TotalWeight = 500m;
			Order.WD_WeightSent = 100m;
			AssertEquals("Weight of Order is taking value from WD_WeightSent", "100", ServicesParentWrapper.Weight);
		}

		public new void TestVolume()
		{
			Order.WD_TotalCubic = 200m;
			Order.WD_CubicSent = 100m;
			AssertEquals("Volume of Order is taking value from WD_CubicSent", "100", ServicesParentWrapper.Volume);
		}

		#endregion

		protected override void TestPackingLinesBOMCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var orderLine1 = Helper.CreateWhsOrderLine(Order, data.Part1, 2m);
			var orderLine2 = Helper.CreateWhsOrderLine(Order, data.Part1, 2m);
			orderLine2.WE_WE_ParentDocketLine = orderLine1.PK;
			Factory.Save();

			AssertEquals("Count should be 1, should not consider child lines", 1, PickableDocketWrapper.PackingLines.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Order = PickableDocket;
			OrderWrapper = PickableDocketWrapper;
			ServicesParentWrapper = OrderWrapper;
		}

		protected override DocWhsOrder CreateWhsDocketWrapper(WhsDocketLabelControl docketLabel)
		{
			return DocWhsOrder.New(docketLabel, Factory);
		}

		protected override DocWhsOrder CreateWhsDocketWrapper(WhsOrder docket)
		{
			return DocWhsOrder.New(docket, Factory);
		}

		WhsOrder Order;
		DocWhsOrder OrderWrapper;

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
