using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePackingSlipLineWrapper))]
	sealed class WarehousePackingSlipLineWrapperTest : WarehouseDocketLineWrapperTest
	{
		#region TestRolledUpParents

		public void TestRolledUpParents()
		{
			AssertEquals(true, PackingSlipWrapper.ContainsRolledUpParent(OrderLine));

			WhsOrderLine line2 = Order.Lines.AddNew();
			AssertEquals(false, PackingSlipWrapper.ContainsRolledUpParent(line2));

			PackingSlipWrapper.AddParentToRollUp(line2);
			AssertEquals(true, PackingSlipWrapper.ContainsRolledUpParent(line2));
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			OrderLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			OrderLine.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("Precodition: UnitsUQ is incorrect", "TST", OrderLine.ProductUQ);

			var releaseLine = OrderLine.ReleaseLines.AddNew();
			PackingSlipWrapper = new WarehousePackingSlipLineWrapper(releaseLine, OrderLine, Factory);
			AssertEquals("Document Wrapper UnitsUQ property is incorrect", "TST", PackingSlipWrapper.UnitsUQ);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PRODUCTCODE";
			OrderLine.WE_OP = part.PK;
			OrderLine.ReleaseLines.AddNew();
			PackingSlipWrapper = new WarehousePackingSlipLineWrapper(OrderLine.ReleaseLines[0], OrderLine, Factory);
			AssertEquals("Product Code", "PRODUCTCODE", PackingSlipWrapper.ProductCode);

			var consignee = Factory.New<OrgHeader>();
			Order.ConsigneePK = consignee.PK;
			var orgPart = part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = consignee.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			orgPart.OU_LocalPartNumber = "LOCALCODE";
			AssertEquals("Product Code", "LOCALCODE", PackingSlipWrapper.ProductCode);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_Desc = "PRODUCT DESCRIPTION";
			OrderLine.WE_OP = part.PK;
			PackingSlipWrapper = new WarehousePackingSlipLineWrapper(ReleaseLine, OrderLine, Factory);
			AssertEquals("Product Desc", "PRODUCT DESCRIPTION", PackingSlipWrapper.ProductDescription);

			var consignee = Factory.New<OrgHeader>();
			Order.ConsigneePK = consignee.PK;
			var orgPart = part.RelatedOrganisations.AddNew();
			orgPart.OU_OH = consignee.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			orgPart.OU_LocalPartDescription = "LOCAL DESCRIPTION";
			AssertEquals("Product Description", "LOCAL DESCRIPTION", PackingSlipWrapper.ProductDescription);
		}

		#endregion

		#region TestProperties

		public void TestProperties()
		{
			var part = OrderLine.SupplierPart;
			part.OP_PartNum = "PRODUCT01";
			part.OP_Weight = 20m;

			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_UNNO = "0004";
			substance.DG_Variant = "a";
			substance.DG_PG = "g";
			substance.DG_PSN = "x";
			substance.DG_Class = "c";
			part.UNDGs.AddNew().DI_DG = substance.PK;
			OrderLine.WE_OP = part.PK;
			OrderLine.WE_TransactionQuantity = 10m;
			OrderLine.WE_RecommendedUnitPrice = 123.45;
			OrderLine.WE_UnitDiscountAmount = 6.78;
			OrderLine.WE_UnitDiscountPercent = 89.12;
			OrderLine.WE_UnitPriceAfterDiscount = 345.67;
			OrderLine.WE_ExtendedLinePrice = 100.99;
			OrderLine.WE_RX_NKUnitPriceCurrency = "AUD";

			ReleaseLine.SetupReleaseLine("PartAttrib1", "PartAttrib2", "PartAttrib3", ZDateTime.Today.Date, ZDateTime.Today.Date, "TrackedSerialNumber");
			ReleaseLine.Quantity = 9m;

			CombineAssertions(() =>
			{
				AssertEquals("UnitsMet.Value", "9", PackingSlipWrapper.UnitsMet.Value);
				AssertEquals("UnitsOrdered.Value", "10", PackingSlipWrapper.UnitsOrdered.Value);
				AssertEquals("Product", part.OP_PartNum, PackingSlipWrapper.ProductCode);
				AssertEquals("Class", substance.DG_Class, PackingSlipWrapper.DangerousGoodsSubstance.IMOClass);
				AssertEquals("AttributeUnits", "", PackingSlipWrapper.AttributeUnits);
				AssertEquals("PartAttrib1", "PartAttrib1", PackingSlipWrapper.PartAttribute1);
				AssertEquals("PartAttrib2", "PartAttrib2", PackingSlipWrapper.PartAttribute2);
				AssertEquals("PartAttrib3", "PartAttrib3", PackingSlipWrapper.PartAttribute3);
				AssertEquals("TrackedSerialNumber", "TrackedSerialNumber", PackingSlipWrapper.TrackedSerialNumber);
				AssertEquals("PackingDate", ZDateTime.Today.Date, PackingSlipWrapper.PackingDate);
				AssertEquals("ExpiryDate", ZDateTime.Today.Date, PackingSlipWrapper.ExpiryDate);
				AssertEquals("RecUnitPrice", "123.45 AUD", PackingSlipWrapper.RecommendedUnitPrice.Value);
				AssertEquals("RecUnitPrice", 123.45m, PackingSlipWrapper.RecommendedUnitPrice.ValueAsDecimal);
				AssertEquals("UnitDiscAmount", "6.78 AUD", PackingSlipWrapper.UnitDiscountAmount.Value);
				AssertEquals("UnitDiscPercent", "89.12", PackingSlipWrapper.UnitDiscountPercent.Value);
				AssertEquals("UnitPriceAfterDisc", "345.67 AUD", PackingSlipWrapper.UnitPriceAfterDiscount.Value);
				AssertEquals("ExtendedLinePrice", 100.99m, PackingSlipWrapper.ExtendedLinePrice.Amount);
				AssertEquals("PricingString", "Rec Unit Price: 123.45 AUD, Unit Disc: 6.78 AUD, Unit Disc %: 89.12, Unit Price After Disc: 345.67 AUD", PackingSlipWrapper.AdditionalMoneys);

				OrderLine.WE_RecommendedUnitPrice = 0.0;
				OrderLine.WE_UnitDiscountAmount = 0.0;
				OrderLine.WE_UnitDiscountPercent = 0.0;
				OrderLine.WE_UnitPriceAfterDiscount = 0.0;
				AssertEquals("AdditionalMoneys", "", PackingSlipWrapper.AdditionalMoneys);

				part.UNDGs.DeleteAll();
				AssertNull("DangerousGoodsSubstance", PackingSlipWrapper.DangerousGoodsSubstance);
			});
		}

		#endregion

		#region TestTrackedSerialNumber

		protected override void TestTrackedSerialNumberCore()
		{
			var part = OrderLine.SupplierPart;
			part.OP_PartNum = "PRODUCT01";

			OrderLine.WE_OP = part.PK;

			ReleaseLine.SetupReleaseLine("PartAttrib1", "PartAttrib2", "PartAttrib3", ZDateTime.Today.Date, ZDateTime.Today.Date, "CRT");
			ReleaseLine.Quantity = 9m;

			AssertEquals("TrackedSerialNumber", "CRT", PackingSlipWrapper.TrackedSerialNumber);
		}

		#endregion

		#region TestTrackedSerialWithLabel

		protected override void TestTrackedSerialWithLabelCore()
		{
			var part = OrderLine.SupplierPart;
			part.OP_PartNum = "PRODUCT01";

			OrderLine.WE_OP = part.PK;

			ReleaseLine.SetupReleaseLine("PartAttrib1", "PartAttrib2", "PartAttrib3", ZDateTime.Today.Date, ZDateTime.Today.Date, "CRT");
			ReleaseLine.Quantity = 9m;

			AssertEquals("TrackedSerialWithLabel", "Tracked Serial Number: CRT", PackingSlipWrapper.TrackedSerialWithLabel);
		}

		#endregion

		#region TestIsWorkOrder

		public void TestIsWorkOrder()
		{
			var order1 = Factory.New<WhsWorkOrder>();
			var order2 = Factory.New<WhsOrder>();
			var order1Line = order1.Lines.AddNew();
			var order2Line = order2.Lines.AddNew();
			order1Line.ReleaseLines.AddNew();
			order2Line.ReleaseLines.AddNew();

			PackingSlipWrapper = new WarehousePackingSlipLineWrapper(order1Line.ReleaseLines[0], order1Line, Factory);
			AssertEquals("it is a work order", true, PackingSlipWrapper.IsWorkOrder);

			PackingSlipWrapper = new WarehousePackingSlipLineWrapper(order2Line.ReleaseLines[0], order2Line, Factory);
			AssertEquals("it is not a work order", false, PackingSlipWrapper.IsWorkOrder);
		}

		#endregion

		#region TestSLineUnitMet_SLineUnitsOrdered

		public void TestSLineUnitMet_SLineUnitsOrdered()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			var workSubOrderLine = workOrder.Lines.AddNew();
			workOrderLine.WE_TransactionQuantity = 10;
			workOrderLine.ReleaseLines.AddNew();

			workSubOrderLine.WE_TransactionQuantity = 15;
			workSubOrderLine.ReleaseLines.AddNew();
			workSubOrderLine.WE_WE_ParentDocketLine = workOrderLine.PK;

			PackingSlipWrapper = new WarehousePackingSlipLineWrapper(workOrderLine.ReleaseLines[0], workOrderLine, Factory);
			AssertEquals("BomLevel", 0m, workOrderLine.WE_Level);

			PackingSlipWrapper = new WarehousePackingSlipLineWrapper(workSubOrderLine.ReleaseLines[0], workSubOrderLine, Factory);
			AssertEquals("BomLevel", 1m, workSubOrderLine.WE_Level);
		}

		#endregion

		#region TestPropertiesAttributeMet

		public void TestPropertiesAttributeMet()
		{
			ReleaseLine.Quantity = 9.001m;
			AssertEquals("Do Not Display", "", PackingSlipWrapper.AttributeUnits);

			var orderLineAttribute2 = OrderLine.ReleaseLines.AddNew();
			AssertEquals("Display if there are more than one attribute", "9.001", PackingSlipWrapper.AttributeUnits);
		}

		#endregion

		#region Properties

		#region ZDateTime Fields

		#region TestPackingDate

		public void TestPackingDate()
		{
			var today = ZDate.Today;
			AssertEquals(ZDateTime.Empty, PackingSlipWrapper.PackingDate);

			ReleaseLine.SetupReleaseLine(string.Empty, string.Empty, string.Empty, ZDate.Empty, today, string.Empty);
			ReleaseLine.ParentCollection.UpdateSumOfUnitsMet(ReleaseLine, ReleaseLine.Quantity);
			AssertEquals(today, PackingSlipWrapper.PackingDate);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals(today, PackingSlipWrapper.PackingDate);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals(today, PackingSlipWrapper.PackingDate);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals(ZDateTime.Empty, PackingSlipWrapper.PackingDate);

			OrderLine.WE_PackingDate = today;
			AssertEquals(today, PackingSlipWrapper.PackingDate);
		}

		#endregion

		#region TestExpiryDate

		public void TestExpiryDate()
		{
			var today = ZDate.Today;
			AssertEquals(ZDateTime.Empty, PackingSlipWrapper.ExpiryDate);

			ReleaseLine.SetupReleaseLine(string.Empty, string.Empty, string.Empty, today, ZDate.Empty, string.Empty);
			ReleaseLine.ParentCollection.UpdateSumOfUnitsMet(ReleaseLine, ReleaseLine.Quantity);
			AssertEquals(today, PackingSlipWrapper.ExpiryDate);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals(today, PackingSlipWrapper.ExpiryDate);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals(today, PackingSlipWrapper.ExpiryDate);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals(ZDateTime.Empty, PackingSlipWrapper.ExpiryDate);

			OrderLine.WE_ExpiryDate = today;
			AssertEquals(today, PackingSlipWrapper.ExpiryDate);
		}

		#endregion

		#endregion

		#region ZString Fields

		#region TestPositionAfterSorting

		public void TestPositionAfterSorting()
		{
			PackingSlipWrapper.PositionAfterSorting = "1";
			AssertEquals("1", PackingSlipWrapper.PositionAfterSorting);
			PackingSlipWrapper.PositionAfterSorting = "0025";
			AssertEquals("0025", PackingSlipWrapper.PositionAfterSorting);
		}

		#endregion

		#region TestPartAttrib1

		public void TestPartAttrib1()
		{
			AssertEquals("", PackingSlipWrapper.PartAttribute1);

			ReleaseLine.PartAttribute1 = "PA1";
			AssertEquals("PA1", PackingSlipWrapper.PartAttribute1);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("PA1", PackingSlipWrapper.PartAttribute1);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("PA1", PackingSlipWrapper.PartAttribute1);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("", PackingSlipWrapper.PartAttribute1);

			OrderLine.WE_PartAttrib1 = "PA1";
			AssertEquals("PA1", PackingSlipWrapper.PartAttribute1);
		}

		#endregion

		#region TestPartAttrib2

		public void TestPartAttrib2()
		{
			AssertEquals("", PackingSlipWrapper.PartAttribute2);

			ReleaseLine.PartAttribute2 = "PA2";
			AssertEquals("PA2", PackingSlipWrapper.PartAttribute2);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("PA2", PackingSlipWrapper.PartAttribute2);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("PA2", PackingSlipWrapper.PartAttribute2);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("", PackingSlipWrapper.PartAttribute2);

			OrderLine.WE_PartAttrib2 = "PA2";
			AssertEquals("PA2", PackingSlipWrapper.PartAttribute2);
		}

		#endregion

		#region TestPartAttrib3

		public void TestPartAttrib3()
		{
			AssertEquals("", PackingSlipWrapper.PartAttribute3);

			ReleaseLine.PartAttribute3 = "PA3";
			AssertEquals("PA3", PackingSlipWrapper.PartAttribute3);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("PA3", PackingSlipWrapper.PartAttribute3);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("PA3", PackingSlipWrapper.PartAttribute3);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("", PackingSlipWrapper.PartAttribute3);

			OrderLine.WE_PartAttrib3 = "PA3";
			AssertEquals("PA3", PackingSlipWrapper.PartAttribute3);
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestPackingSlip_TrackedSerialNumber()
		{
			AssertEquals("", PackingSlipWrapper.TrackedSerialNumber);

			ReleaseLine.SetupReleaseLine(string.Empty, string.Empty, string.Empty, ZDate.Empty, ZDate.Empty, "SN6");
			AssertEquals("SN6", PackingSlipWrapper.TrackedSerialNumber);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("SN6", PackingSlipWrapper.TrackedSerialNumber);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("SN6", PackingSlipWrapper.TrackedSerialNumber);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("", PackingSlipWrapper.TrackedSerialNumber);

			OrderLine.WE_SerialNumber = "SN6";
			AssertEquals("SN6", PackingSlipWrapper.TrackedSerialNumber);
		}

		#endregion

		#endregion

		#region Number Fields

		#region TestAttributeUnits

		public void TestAttributeUnits()
		{
			ReleaseLine.Quantity = 10.001m;
			AssertEquals("If OrderLine has one Attributes line, then AttributeUnits should not be displayed.", "", PackingSlipWrapper.AttributeUnits);

			var releaseLine2 = OrderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 20.001m;
			AssertEquals("When OrderLine has multiple Attributes, then AttributeUnits should be displayed.", "10.001", PackingSlipWrapper.AttributeUnits);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("When Products Pick Mode is Attribute Specified, then AttributeUnits should be displayed.", "10.001", PackingSlipWrapper.AttributeUnits);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("When Products Pick Mode is Attribute Neutral and Roll Up is set to false, then AttributeUnits should be displayed.", "10.001", PackingSlipWrapper.AttributeUnits);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("When Products Pick Mode is Attribute Neutral and Roll Up is set to true, then AttributeUnits shouldn't be displayed.", "", PackingSlipWrapper.AttributeUnits);
		}

		#endregion

		#region TestUnitsMet

		public void TestUnitsMet()
		{
			AssertEquals(10m, PackingSlipWrapper.UnitsMet.NativeValue);

			ReleaseLine.Quantity = 5m;
			AssertEquals(5m, PackingSlipWrapper.UnitsMet.NativeValue);

			var line2 = Helper.CreateWhsOrderLine(Order, data.Part1, 10m);
			Order.Pick.ClearOrderedInventoriesCache();

			var releaseLine2 = line2.ReleaseLines.AddNew();
			releaseLine2.Quantity = 10m;
			PackingSlipWrapper.AddParentToRollUp(line2);
			AssertEquals(15m, PackingSlipWrapper.UnitsMet.NativeValue);
		}

		#endregion

		#region TestUnitsOrdered

		public void TestUnitsOrdered()
		{
			AssertEquals(10m, PackingSlipWrapper.UnitsOrdered.NativeValue);

			OrderLine.WE_TransactionQuantity = 5m;
			AssertEquals(5m, PackingSlipWrapper.UnitsOrdered.NativeValue);

			WhsPickableDocketLine line2 = Order.Lines.AddNew();
			line2.WE_TransactionQuantity = 10m;
			PackingSlipWrapper.AddParentToRollUp(line2);
			AssertEquals(15m, PackingSlipWrapper.UnitsOrdered.NativeValue);
		}

		#endregion

		#region TestLineNo

		protected override void TestLineNoCore()
		{
			OrderLine.WE_LineNo = 5;
			AssertEquals("00005", PackingSlipWrapper.LineNo);

			WhsPickableDocketLine line2 = Order.Lines.AddNew();
			line2.WE_LineNo = 7;
			PackingSlipWrapper.AddParentToRollUp(line2);
			AssertEquals("00005", PackingSlipWrapper.LineNo);

			WhsPickableDocketLine line3 = Order.Lines.AddNew();
			line3.WE_LineNo = 3;
			PackingSlipWrapper.AddParentToRollUp(line3);
			AssertEquals("00003", PackingSlipWrapper.LineNo);
		}

		#endregion

		#region TestExtendedLinePrice

		public void TestExtendedLinePrice_RolledUpOrderLines()
		{
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "B1", "B2", "B3", "");
			Factory.Save();

			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline3 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			Helper.CreateWhsPickLine(orderline1, receive.Inventory[0], 3m);
			Helper.CreateWhsPickLine(orderline2, receive.Inventory[0], 2m);
			Helper.CreateWhsPickLine(orderline3, receive.Inventory[1], 1m);

			orderline1.WE_UnitPriceAfterDiscount = 2.2m;
			orderline2.WE_UnitPriceAfterDiscount = 2.5m;
			orderline3.WE_UnitPriceAfterDiscount = 3m;

			var wrapper = new WarehousePackingSlipLineWrapper(orderline1.ReleaseLines[0], orderline1, Factory);
			AssertEquals("Extended Line Price should be: 3 * 2.2m.", 6.6m, wrapper.ExtendedLinePrice.Amount);

			wrapper.AddParentToRollUp(orderline2);
			AssertEquals("Extended Line Price should be: 3 * 2.2m + 2 * 2.5m.", 11.6m, wrapper.ExtendedLinePrice.Amount);

			wrapper.AddParentToRollUp(orderline3);
			AssertEquals("Extended Line Price should be: 3 * 2.2m + 2 * 2.5m + 1 * 3m.", 14.6m, wrapper.ExtendedLinePrice.Amount);
		}

		public void TestExtendedLinePrice_RolledUpOrderLines_DifferentCurrencies()
		{
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "B1", "B2", "B3", "");
			Factory.Save();

			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderline1.WE_RX_NKUnitPriceCurrency = "AUD";
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderline2.WE_RX_NKUnitPriceCurrency = "USD";

			Helper.CreatePickNew(order);
			Helper.CreateWhsPickLine(orderline1, receive.Inventory[0], 3m);
			Helper.CreateWhsPickLine(orderline2, receive.Inventory[0], 2m);

			orderline1.WE_UnitPriceAfterDiscount = 2.2m;
			orderline2.WE_UnitPriceAfterDiscount = 2.5m;

			var wrapper1 = new WarehousePackingSlipLineWrapper(orderline1.ReleaseLines[0], orderline1, Factory);
			AssertEquals("Extended Line Price should be: 3 * $2.2 in AUD.", "6.60 AUD", wrapper1.ExtendedLinePrice.ToString());

			var wrapper2 = new WarehousePackingSlipLineWrapper(orderline2.ReleaseLines[0], orderline2, Factory);
			AssertEquals("Extended Line Price should be: 2 * $2.5 in USD.", "5.00 USD", wrapper2.ExtendedLinePrice.ToString());

			wrapper2.AddParentToRollUp(orderline1);
			AssertEquals("Extended Line Price should be: 3 * $2.2m + 2 * $2.5.", "11.60", wrapper2.ExtendedLinePrice.ToString());
		}

		#endregion

		#region TestExtendedLinePriceForTotal

		public void TestExtendedLinePriceForTotal_PriceProportionalToUnitsMet()
		{
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "B1", "B2", "B3", "");
			Factory.Save();

			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			Helper.CreateWhsPickLine(orderline, receive.Inventory[0], 3m);
			Helper.CreateWhsPickLine(orderline, receive.Inventory[1], 2m);

			orderline.WE_UnitPriceAfterDiscount = 2.2m;

			var wrapper1 = new WarehousePackingSlipLineWrapper(orderline.ReleaseLines[0], orderline, Factory);
			AssertEquals("Price for Total should be: 3 * 2.2m.", 6.6m, wrapper1.ExtendedLinePriceForTotal);

			var wrapper2 = new WarehousePackingSlipLineWrapper(orderline.ReleaseLines[1], orderline, Factory);
			AssertEquals("Price for Total should be: 2 * 2.2m.", 4.4m, wrapper2.ExtendedLinePriceForTotal);
		}

		public void TestExtendedLinePriceForTotal_PriceProportionalToUnitsMet_PriceManuallySet()
		{
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "B1", "B2", "B3", "");
			Factory.Save();

			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = false;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			Helper.CreateWhsPickLine(orderline, receive.Inventory[0], 3m);
			Helper.CreateWhsPickLine(orderline, receive.Inventory[1], 2m);

			orderline.WE_ExtendedLinePrice = 15m;

			var wrapper1 = new WarehousePackingSlipLineWrapper(orderline.ReleaseLines[0], orderline, Factory);
			AssertEquals("Price for Total should be: 15m * 3/5.", 9m, wrapper1.ExtendedLinePriceForTotal);

			var wrapper2 = new WarehousePackingSlipLineWrapper(orderline.ReleaseLines[1], orderline, Factory);
			AssertEquals("Price for Total should be: 15m * 2/5.", 6m, wrapper2.ExtendedLinePriceForTotal);
		}

		public void TestExtendedLinePriceForTotal_NoUnitsMet()
		{
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			orderline.ReleaseLines.AddNew();

			orderline.WE_UnitPriceAfterDiscount = 2.2m;

			var wrapper1 = new WarehousePackingSlipLineWrapper(orderline.ReleaseLines[0], orderline, Factory);
			AssertEquals("Price for Total should be: 0m.", 0m, wrapper1.ExtendedLinePriceForTotal);
		}

		public void TestExtendedLinePriceForTotal_RolledUpOrderLines()
		{
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "B1", "B2", "B3", "");
			Factory.Save();

			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline3 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			Helper.CreateWhsPickLine(orderline1, receive.Inventory[0], 3m);
			Helper.CreateWhsPickLine(orderline2, receive.Inventory[0], 2m);
			Helper.CreateWhsPickLine(orderline3, receive.Inventory[1], 1m);

			orderline1.WE_UnitPriceAfterDiscount = 2.2m;
			orderline2.WE_UnitPriceAfterDiscount = 2.5m;
			orderline3.WE_UnitPriceAfterDiscount = 3m;

			var wrapper = new WarehousePackingSlipLineWrapper(orderline1.ReleaseLines[0], orderline1, Factory);
			AssertEquals("Price for Total should be: 3 * 2.2m.", 6.6m, wrapper.ExtendedLinePriceForTotal);

			wrapper.AddParentToRollUp(orderline2);
			AssertEquals("Price for Total should be: 3 * 2.2m + 2 * 2.5m.", 11.6m, wrapper.ExtendedLinePriceForTotal);

			wrapper.AddParentToRollUp(orderline3);
			AssertEquals("Price for Total should be: 3 * 2.2m + 2 * 2.5m + 1 * 3m.", 14.6m, wrapper.ExtendedLinePriceForTotal);
		}

		public void TestExtendedLinePriceForTotal_RolledUpOrderLines_PriceManuallySet()
		{
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "B1", "B2", "B3", "");
			Factory.Save();

			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = false;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline3 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			Helper.CreateWhsPickLine(orderline1, receive.Inventory[0], 3m);
			Helper.CreateWhsPickLine(orderline2, receive.Inventory[0], 2m);
			Helper.CreateWhsPickLine(orderline3, receive.Inventory[1], 1m);

			orderline1.WE_ExtendedLinePrice = 10m;
			orderline2.WE_ExtendedLinePrice = 15m;
			orderline3.WE_ExtendedLinePrice = 20m;

			var wrapper = new WarehousePackingSlipLineWrapper(orderline1.ReleaseLines[0], orderline1, Factory);
			AssertEquals("Price for Total should be: 10m * 3/3.", 10m, wrapper.ExtendedLinePriceForTotal);

			wrapper.AddParentToRollUp(orderline2);
			AssertEquals("Price for Total should be: 10m * 3/3 + 15m * 2/2.", 25m, wrapper.ExtendedLinePriceForTotal);

			wrapper.AddParentToRollUp(orderline3);
			AssertEquals("Price for Total should be: 10m * 3/3 + 15m * 2/2 + 20m * 1/1.", 45m, wrapper.ExtendedLinePriceForTotal);
		}

		#endregion

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override void SetAttribute1Core() => ReleaseLine.PartAttribute1 = "PA1";

		protected override void SetAttribute2Core() => ReleaseLine.PartAttribute2 = "AT2";

		protected override void SetAttribute3Core() => ReleaseLine.PartAttribute3 = "AT3";

		protected override void SetExpiryDateCore()
		{
			ReleaseLine.SetupReleaseLine("AT1", "AT2", "AT3", ZDate.Today, ZDate.Empty, string.Empty);
		}

		protected override void SetPackingDateCore()
		{
			ReleaseLine.SetupReleaseLine("AT1", "AT2", "AT3", ZDate.Today, ZDate.Today, string.Empty);
		}

		protected override void SetSerialNumberCore()
		{
			ReleaseLine.SetupReleaseLine("AT1", "AT2", "AT3", ZDate.Today, ZDate.Today, "SNX");
		}

		protected override WarehouseGenericLineWrapper SetProductDGCore()
		{
			var part = Factory.New<OrgSupplierPart>();
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_UNNO = "9001";
			undg.DG_Variant = "a";
			part.UNDGs.AddNew().DI_DG = undg.PK;
			OrderLine.WE_OP = part.PK;
			return GetAndSetWarehousePackingSlipLineWrapper();
		}

		#endregion

		#endregion

		#region TestNMFC

		public void TestNMFCProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1);
			var releaseLine = orderLine.ReleaseLines.AddNew();

			var wrapper = new WarehousePackingSlipLineWrapper(releaseLine, orderLine, Factory);
			AssertNull(wrapper.NMFC);

			var nmfc = Factory.New<RefNMFC>();
			nmfc.FN_ItemNo = "123";
			nmfc.FN_Class = "456";
			nmfc.FN_Description = "aaa";

			var commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = "ABC";
			commodityCode.RH_FN_NKNMFC = "123|456";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_RH_NKCommodityCode = commodityCode.RH_Code;

			orderLine.WE_OP = part.PK;
			AssertNull(wrapper.NMFC);

			var transportCo = Factory.New<OrgHeader>();
			orderLine.Order.TransportCoPK = transportCo.PK;
			AssertNull(wrapper.NMFC);

			transportCo.CustomsCodes.AddNew();
			transportCo.CustomsCodes[0].OK_CodeType = OrgCusCode.USACodeTypes.NMFCParticipant;
			transportCo.CustomsCodes[0].OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.Yes;
			AssertEquals("456", wrapper.NMFC.Class);
			AssertEquals("123", wrapper.NMFC.ItemNo);
			AssertEquals("aaa", wrapper.NMFC.Description);

			nmfc.FN_IsActive = false;
			AssertNull(wrapper.NMFC);
		}

		#endregion

		#region TestCustomFields

		public void TestCustomFields()
		{
			OrderLine.WE_CustomAttrib1 = "PO12";
			AssertEquals("Should add both Custom Fields that are enabled.", 1, PackingSlipWrapper.CustomFields.Count);
			AssertEquals("Custom Field should return correct Caption.", "PO#", PackingSlipWrapper.CustomFields[Constants.CustomLabels.WhsDocketLine.CustomAttribute1].Caption);
			AssertEquals("Custom Field should get value from OrderLine.", "PO12", PackingSlipWrapper.CustomFields[Constants.CustomLabels.WhsDocketLine.CustomAttribute1].Value);
		}

		#endregion

		#endregion

		#region TestProduct

		public new void TestProduct()
		{
			OrderLine.SupplierPart.OP_Width = 10;
			AssertEquals(OrderLine.SupplierPart, PackingSlipWrapper.Product.WrappedObject);
			AssertEquals(10m, PackingSlipWrapper.Product.Product.OP_Width);
		}

		#endregion

		#region TestLocationString

		public new void TestLocationString()
		{
			OrderLine.LocationString = "A-2";
			AssertLocationString(OrderLine);
		}
		protected override void AssertLocationString(WhsDocketLine line)
		{
			AssertEquals(line.LocationString, PackingSlipWrapper.LocationString);
		}

		#endregion

		#region TestTranslatable

		public override void TestPartAttribute1NameWithLabel_TranslatableCore()
		{
			Assert(true); // not applicable
		}

		public override void TestPartAttribute2NameWithLabel_TranslatableCore()
		{
			Assert(true); // not applicable
		}

		public override void TestPartAttribute3NameWithLabel_TranslatableCore()
		{
			Assert(true); // not applicable
		}

		#endregion

		#region TestHoldCode

		protected override void TestHoldCodeCore()
		{
			AssertEquals(ZString.Empty, PackingSlipWrapper.HoldCode);
		}

		#endregion

		#region TestHoldReason

		protected override void TestHoldReasonCore()
		{
			AssertEquals(ZString.Empty, PackingSlipWrapper.HoldReason);
		}

		#endregion

		// interfaces

		#region IPackingSlipWrapper

		public void TestIPackingSlipWrapper()
		{
			Order.Delete();

			Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "R1", Data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(Data.Org1, Data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, Data.Part1, 1);
			var releaseLine = orderLine.ReleaseLines.AddNew();
			IPackingSlipWrapper wrapper = new WarehousePackingSlipLineWrapper(releaseLine, orderLine, Factory);
			AssertEquals("Should contain Parent Order Line.", true, wrapper.ContainsRolledUpParent(orderLine));
			AssertEquals("LineNo", "00001", wrapper.LineNo);

			var randomOrderLine = Factory.New<WhsOrderLine>();
			wrapper.AddParentToRollUp(randomOrderLine);
			AssertEquals("Should Contain Order Lines that are added.", true, wrapper.ContainsRolledUpParent(randomOrderLine));
			AssertEquals("Default Position is empty.", "", wrapper.PositionAfterSorting);

			wrapper.PositionAfterSorting = "XXX";
			AssertEquals("PositionAfterSorting", "XXX", wrapper.PositionAfterSorting);
		}

		#endregion

		//

		#region Implementation

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject whsLineBO)
		{
			WarehouseGenericLineWrapper result;
			if (whsLineBO == null)
			{
				result = new WarehousePackingSlipLineWrapper(null, null, Factory);
			}
			else
			{
				result = GetAndSetWarehousePackingSlipLineWrapper();
			}

			return result;
		}

		WarehousePackingSlipLineWrapper GetAndSetWarehousePackingSlipLineWrapper()
		{
			PackingSlipWrapper = new WarehousePackingSlipLineWrapper(ReleaseLine, OrderLine, Factory);
			return PackingSlipWrapper;
		}

		WhsOrder Order
		{
			get { return (WhsOrder)Docket; }
		}

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsOrder>();
		}

		WhsOrderLine OrderLine;
		WarehousePackingSlipLineWrapper PackingSlipWrapper;
		WhsReleaseLine ReleaseLine;

		protected override void SetUp()
		{
			Helper.CreateCustomLabel(Data.Org1, Constants.CustomLabels.WhsDocketLine.CustomAttribute1, "PO#", false);
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, Data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Data.Part1, 10m);
			Factory.Save();

			Order.WD_OH_Client = Data.Org1.PK;
			Order.WD_WW_Whs = Data.Whs1.PK;
			Order.WD_RequiredDate = ZDateTimeOffset.Now;
			Order.ConsigneeNameOrPK = Data.Org1.PK.ToString();

			OrderLine = Helper.CreateWhsOrderLine(Order, Data.Part1, 10m);
			var pick = Helper.CreatePickNew(Order);

			ReleaseLine = OrderLine.ReleaseLines[0];
			GetAndSetWarehousePackingSlipLineWrapper();

			base.SetUp();
		}

		TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory)); }
		}
		TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup data;

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CrossDockConsigneeAddress : 
DangerousGoodsSubstance :  is null
ExtendedLinePrice : 
ManufacturerAddress : 
Product : (No Default Field Value Available on Product)
RecommendedUnitPrice : 
Registry : (No Default Field Value Available on Registry)
UnitDiscountAmount : 
UnitDiscountPercent : 0.00
UnitPriceAfterDiscount : 
UnitsMet : 10
UnitsOrdered : 10
UnitsPicked : 
UnitsShort :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return GetAndSetWarehousePackingSlipLineWrapper();
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return GetAndSetWarehousePackingSlipLineWrapper();
		}

		protected override IWhsBondedWarehouseAttribute GetCustomsData()
		{
			return OrderLine.CustomsData;
		}

		#endregion
	}
}
