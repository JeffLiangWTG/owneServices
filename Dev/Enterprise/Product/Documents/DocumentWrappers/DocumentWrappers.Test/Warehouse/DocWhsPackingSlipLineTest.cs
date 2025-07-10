using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPackingSlipLine))]
	sealed class DocWhsPackingSlipLineTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { PackingSlipWrapper };
		}

		#region TestStaticNew

		public void TestStaticNew()
		{
			AssertNotNull(DocWhsPackingSlipLine.New(ReleaseLine, OrderLine, Factory));
		}

		#endregion

		#region Related Business Objects

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

		#endregion

		#region TestOrderLine

		public void TestOrderLine()
		{
			AssertEquals(PackingSlipWrapper.OrderLine.WrappedObject, OrderLine);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			OrderLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			OrderLine.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("Precodition: UnitsUQ is incorrect", "TST", OrderLine.ProductUQ);

			AssertEquals("Document Wrapper UnitsUQ property is incorrect", "TST", PackingSlipWrapper.UnitsUQ);

			PackingSlipWrapper = DocWhsPackingSlipLine.New(OrderLine.ReleaseLines[0], OrderLine, Factory);
			AssertEquals("Document Wrapper UnitsUQ property is incorrect", "TST", PackingSlipWrapper.UnitsUQ);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PRODUCTCODE";
			OrderLine.WE_OP = part.PK;
			AssertEquals("Product Code", "PRODUCTCODE", PackingSlipWrapper.ProductCode);

			PackingSlipWrapper = DocWhsPackingSlipLine.New(OrderLine.ReleaseLines[0], OrderLine, Factory);
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

			OrderLine.ReleaseLines.AddNew();
			PackingSlipWrapper = DocWhsPackingSlipLine.New(OrderLine.ReleaseLines[0], OrderLine, Factory);
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

		#region TestCurrencySymbol

		public void TestCurrencySymbol()
		{
			AssertEquals(PackingSlipWrapper.CurrencySymbol, ZString.Empty);
			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "OH1";
			client.OH_RL_NKClosestPort = "AFKBL";
			Order.WD_OH_Client = client.PK;
			AssertEquals(PackingSlipWrapper.CurrencySymbol, "Af");
			OrderLine.WE_RX_NKUnitPriceCurrency = "AUD";
			AssertEquals(PackingSlipWrapper.CurrencySymbol, "$");
		}

		#endregion

		#region TestProperties

		public void TestProperties()
		{
			OrderLine.SupplierPart.OP_Weight = 20m;
			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_UNNO = "0004";
			substance.DG_Variant = "a";
			substance.DG_PG = "g";
			substance.DG_PSN = "x";
			substance.DG_Class = "c";
			OrderLine.SupplierPart.UNDGs.AddNew().DI_DG = substance.PK;
			OrderLine.WE_RecommendedUnitPrice = 123.45;
			OrderLine.WE_UnitDiscountAmount = 6.78;
			OrderLine.WE_UnitDiscountPercent = 89.12;
			OrderLine.WE_UnitPriceAfterDiscount = 345.67;
			OrderLine.WE_ExtendedLinePrice = 100.99;
			OrderLine.WE_RX_NKUnitPriceCurrency = "AUD";

			ReleaseLine.SetupReleaseLine("PartAttrib1", "PartAttrib2", "PartAttrib3", ZDateTime.Today.Date, ZDateTime.Today.Date, "TrackedSerialNumber");
			ReleaseLine.Quantity = 9m;

			AssertEquals("LinePK", OrderLine.PK, PackingSlipWrapper.LinePK);
			AssertEquals("LineUnitMet", OrderLine.SumOfUnitsMet, PackingSlipWrapper.LineUnitsMet);
			AssertEquals("LineUnitsWeight", OrderLine.SumOfUnitsMet * OrderLine.SupplierPart.OP_Weight, PackingSlipWrapper.LineUnitsWeight);
			AssertEquals("LineUnitsOrdered", 10m, PackingSlipWrapper.LineUnitsOrdered);
			AssertEquals("Product", OrderLine.SupplierPart.OP_PartNum, PackingSlipWrapper.Product.PartNum);
			AssertEquals("IsDG", "X", PackingSlipWrapper.IsDG);
			AssertEquals("Class", substance.DG_Class, PackingSlipWrapper.DGSubstance.IMOClass);
			AssertEquals("Proper Shipping Name", substance.DG_PSN, PackingSlipWrapper.DGSubstance.ProperShippingName);
			AssertEquals("UNNumber", substance.DG_UNNO, PackingSlipWrapper.DGSubstance.UNNumber);
			AssertEquals("UNNumber", substance.DG_UNNO + substance.DG_Variant, PackingSlipWrapper.DGSubstance.UNNumberWithVariant);
			AssertEquals("Packing Group", substance.DG_PG, PackingSlipWrapper.DGSubstance.PackingGroup);
			AssertEquals("HazMatString", "UN Number: " + PackingSlipWrapper.DGSubstance.UNNumber + "  DG Class: " + PackingSlipWrapper.DGSubstance.IMOClass + "  Proper Shipping Name: " + PackingSlipWrapper.ProperShippingName + "  Packing Group: " + PackingSlipWrapper.DGSubstance.PackingGroup, PackingSlipWrapper.HazMatString);
			AssertEquals("AttributeUnits", "", PackingSlipWrapper.AttributeUnits);
			AssertEquals("PartAttrib1", "PartAttrib1", PackingSlipWrapper.PartAttrib1);
			AssertEquals("PartAttrib2", "PartAttrib2", PackingSlipWrapper.PartAttrib2);
			AssertEquals("PartAttrib3", "PartAttrib3", PackingSlipWrapper.PartAttrib3);
			AssertEquals("TrackedSerialNumber", "TrackedSerialNumber", PackingSlipWrapper.TrackedSerialNumber);
			AssertEquals("PackingDate", ZDateTime.Today.Date, PackingSlipWrapper.PackingDate);
			AssertEquals("ExpiryDate", ZDateTime.Today.Date, PackingSlipWrapper.ExpiryDate);
			AssertEquals("RecUnitPrice", 123.45m, PackingSlipWrapper.RecommendedUnitPrice);
			AssertEquals("UnitDiscAmount", 6.78m, PackingSlipWrapper.UnitDiscountAmount);
			AssertEquals("UnitDiscPercent", 89.12m, PackingSlipWrapper.UnitDiscountPercent);
			AssertEquals("UnitPriceAfterDisc", 345.67m, PackingSlipWrapper.UnitPriceAfterDiscount);
			AssertEquals("ExtendedLinePrice", 100.99m, PackingSlipWrapper.ExtendedLinePrice);
			AssertEquals("Currency", "AUD", PackingSlipWrapper.UnitPriceCurrency);
			AssertEquals("PricingString", "Rec Unit Price: $123.45, Unit Disc: $6.78, Unit Disc %: 89.12, Unit Price After Disc: $345.67, Currency: AUD", PackingSlipWrapper.PricingString);
			AssertEquals("RecUnitPriceWithSymbol", "$123.45", PackingSlipWrapper.RecommendedUnitPriceWithSymbol);
			AssertEquals("UnitDiscAmountWithSymbol", "$6.78", PackingSlipWrapper.UnitDiscountAmountWithSymbol);
			AssertEquals("UnitPriceAfterDiscWithSymbol", "$345.67", PackingSlipWrapper.UnitPriceAfterDiscountWithSymbol);
			AssertEquals("ExtendedLinePriceWithSymbol", "$100.99", PackingSlipWrapper.ExtendedLinePriceWithSymbol);

			OrderLine.WE_RecommendedUnitPrice = 0.0;
			OrderLine.WE_UnitDiscountAmount = 0.0;
			OrderLine.WE_UnitDiscountPercent = 0.0;
			OrderLine.WE_UnitPriceAfterDiscount = 0.0;
			AssertEquals("PricingString", "Currency: AUD", PackingSlipWrapper.PricingString);

			AssertEquals("IsDG", "X", PackingSlipWrapper.IsDG);
			AssertEquals("LineUnitsWeight", 180m, PackingSlipWrapper.LineUnitsWeight);

			OrderLine.SupplierPart.UNDGs.DeleteAll();
			AssertEquals("IsDG", ZString.Empty, PackingSlipWrapper.IsDG);
			OrderLine.WE_OP = ZGuid.Empty;
			AssertEquals("LineUnitsWeight", 0m, PackingSlipWrapper.LineUnitsWeight);
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

			PackingSlipWrapper = DocWhsPackingSlipLine.New(order1Line.ReleaseLines[0], order1Line, Factory);
			AssertEquals("it is a work order", true, PackingSlipWrapper.IsWorkOrder);

			PackingSlipWrapper = DocWhsPackingSlipLine.New(order2Line.ReleaseLines[0], order2Line, Factory);
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

			PackingSlipWrapper = DocWhsPackingSlipLine.New(workOrderLine.ReleaseLines[0], workOrderLine, Factory);
			AssertEquals("BomLevel", 0m, workOrderLine.WE_Level);
			AssertEquals("SLineUnitMet", PackingSlipWrapper.LineUnitsMet.ToString(), PackingSlipWrapper.SLineUnitsMet);
			AssertEquals("SLineUnitOrdered", PackingSlipWrapper.LineUnitsOrdered.ToString(), PackingSlipWrapper.SLineUnitsOrdered);

			PackingSlipWrapper = DocWhsPackingSlipLine.New(workSubOrderLine.ReleaseLines[0], workSubOrderLine, Factory);
			AssertEquals("BomLevel", 1m, workSubOrderLine.WE_Level);
			AssertEquals("SLineUnitMet", "", PackingSlipWrapper.SLineUnitsMet);
			AssertEquals("SLineUnitOrdered", "", PackingSlipWrapper.SLineUnitsOrdered);
		}

		#endregion

		#region TestBomLevel_TopLevelUnitsMet_TopLevelUnitsOrdered

		public void TestBomLevel_TopLevelUnitsMet_TopLevelUnitsOrdered()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			var workSubOrderLine = workOrder.Lines.AddNew();
			workOrderLine.WE_TransactionQuantity = 10;
			workOrderLine.ReleaseLines.AddNew();
			workSubOrderLine.WE_TransactionQuantity = 15;
			workSubOrderLine.ReleaseLines.AddNew();
			workSubOrderLine.WE_WE_ParentDocketLine = workOrderLine.PK;

			PackingSlipWrapper = DocWhsPackingSlipLine.New(workOrderLine.ReleaseLines[0], workOrderLine, Factory);

			AssertEquals("BomLevel", 0m, workOrderLine.WE_Level);
			AssertEquals("TopLevelUnitsMet", workOrderLine.SumOfUnitsMet, PackingSlipWrapper.TopLevelUnitsMet);
			AssertEquals("TopLevelUnitsOrdered", workOrderLine.WE_TransactionQuantity, PackingSlipWrapper.TopLevelUnitsOrdered);

			PackingSlipWrapper = DocWhsPackingSlipLine.New(workSubOrderLine.ReleaseLines[0], workSubOrderLine, Factory);
			AssertEquals("BomLevel", 1m, workSubOrderLine.WE_Level);
			AssertEquals("TopLevelUnitsMet", workSubOrderLine.SumOfUnitsMet, PackingSlipWrapper.TopLevelUnitsMet);
			AssertEquals("TopLevelUnitsOrdered", 0m, PackingSlipWrapper.TopLevelUnitsOrdered);
		}

		#endregion

		#region TestNMFCProperties

		public void TestNMFCProperties()
		{
			AssertNull(PackingSlipWrapper.NMFC);

			var nmfc = Factory.New<RefNMFC>();
			nmfc.FN_ItemNo = "123";
			nmfc.FN_Class = "456";
			nmfc.FN_Description = "aaa";

			var commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = "ABC";
			commodityCode.RH_FN_NKNMFC = "123|456";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_RH_NKCommodityCode = commodityCode.RH_Code;

			OrderLine.WE_OP = part.PK;
			OrderLine.ReleaseLines.AddNew();
			PackingSlipWrapper = DocWhsPackingSlipLine.New(OrderLine.ReleaseLines[0], OrderLine, Factory);
			AssertNotNull(PackingSlipWrapper.NMFC);
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCClass);
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCItemNo);
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCDescription);

			var transportCo = Factory.New<OrgHeader>();
			OrderLine.Order.TransportCoPK = transportCo.PK;
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCClass);
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCItemNo);
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCDescription);

			transportCo.CustomsCodes.AddNew();
			transportCo.CustomsCodes[0].OK_CodeType = OrgCusCode.USACodeTypes.NMFCParticipant;
			transportCo.CustomsCodes[0].OK_CustomsRegNo = OrgConstants.NMFCParticipantCodes.Code.Yes;
			AssertEquals("456", PackingSlipWrapper.NMFCClass);
			AssertEquals("123", PackingSlipWrapper.NMFCItemNo);
			AssertEquals("aaa", PackingSlipWrapper.NMFCDescription);

			nmfc.FN_IsActive = false;
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCClass);
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCItemNo);
			AssertEquals(ZString.Empty, PackingSlipWrapper.NMFCDescription);
		}

		#endregion

		#region TestPropertiesAttributeMet

		public void TestPropertiesAttributeMet()
		{
			ReleaseLine.Quantity = 9.001m;
			AssertEquals("Do Not Display", "", PackingSlipWrapper.AttributeUnits);

			var releaseLine2 = OrderLine.ReleaseLines.AddNew();
			AssertEquals("Display if there are more than one attribute", "9.001", PackingSlipWrapper.AttributeUnits);
		}

		#endregion

		#region TestGroupedGroupedLineUnitsMetString

		public void TestGroupedGroupedLineUnitsMetString()
		{
			var part = Factory.New<OrgSupplierPart>();
			OrderLine.WE_OP = part.PK;
			PackingSlipWrapper.GroupedLineUnitsMet = 10m;
			AssertEquals("10", PackingSlipWrapper.GroupedLineUnitsMetString);

			PackingSlipWrapper = DocWhsPackingSlipLine.New(OrderLine.ReleaseLines[0], OrderLine, Factory);
			AssertEquals("0", PackingSlipWrapper.GroupedLineUnitsMetString);

			PackingSlipWrapper.GroupedLineUnitsMet = 10m;
			AssertEquals("10", PackingSlipWrapper.GroupedLineUnitsMetString);
		}

		#endregion

		#region TestGroupedLineUnitsWeightString

		public void TestGroupedLineUnitsWeightString()
		{
			var part = Factory.New<OrgSupplierPart>();
			OrderLine.WE_OP = part.PK;
			part.OP_Weight = 10m;

			PackingSlipWrapper.GroupedLineUnitsMet = 10m;
			AssertEquals("100", PackingSlipWrapper.GroupedLineUnitsWeightString);

			PackingSlipWrapper = DocWhsPackingSlipLine.New(OrderLine.ReleaseLines[0], OrderLine, Factory);
			AssertEquals("0", PackingSlipWrapper.GroupedLineUnitsWeightString);

			PackingSlipWrapper.GroupedLineUnitsMet = 10m;
			AssertEquals("100", PackingSlipWrapper.GroupedLineUnitsWeightString);

			OrderLine.WE_OP = ZGuid.Empty;
			AssertEquals(ZString.Empty, PackingSlipWrapper.GroupedLineUnitsWeightString);
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
			AssertEquals("", PackingSlipWrapper.PartAttrib1);

			ReleaseLine.PartAttribute1 = "PA1";
			AssertEquals("PA1", PackingSlipWrapper.PartAttrib1);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("PA1", PackingSlipWrapper.PartAttrib1);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("PA1", PackingSlipWrapper.PartAttrib1);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("", PackingSlipWrapper.PartAttrib1);

			OrderLine.WE_PartAttrib1 = "PA1";
			AssertEquals("PA1", PackingSlipWrapper.PartAttrib1);
		}

		#endregion

		#region TestPartAttrib2

		public void TestPartAttrib2()
		{
			AssertEquals("", PackingSlipWrapper.PartAttrib2);

			ReleaseLine.PartAttribute2 = "PA2";
			AssertEquals("PA2", PackingSlipWrapper.PartAttrib2);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("PA2", PackingSlipWrapper.PartAttrib2);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("PA2", PackingSlipWrapper.PartAttrib2);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("", PackingSlipWrapper.PartAttrib2);

			OrderLine.WE_PartAttrib2 = "PA2";
			AssertEquals("PA2", PackingSlipWrapper.PartAttrib2);
		}

		#endregion

		#region TestPartAttrib3

		public void TestPartAttrib3()
		{
			AssertEquals("", PackingSlipWrapper.PartAttrib3);

			ReleaseLine.PartAttribute3 = "PA3";
			AssertEquals("PA3", PackingSlipWrapper.PartAttrib3);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("PA3", PackingSlipWrapper.PartAttrib3);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("PA3", PackingSlipWrapper.PartAttrib3);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("", PackingSlipWrapper.PartAttrib3);

			OrderLine.WE_PartAttrib3 = "PA3";
			AssertEquals("PA3", PackingSlipWrapper.PartAttrib3);
		}

		#endregion

		#endregion

		#region Number Fields

		#region TestAttributeUnits

		public void TestAttributeUnits()
		{
			OrderLine.ReleaseLines[0].Quantity = 10.001m;
			AssertEquals("If OrderLine has one WhsPickableDocketLineAttributes line, then AttributeUnits should not be displayed.", "", PackingSlipWrapper.AttributeUnits);

			var releaseLine2 = OrderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 20.001m;
			AssertEquals("When OrderLine has multiple WhsPickableDocketLineAttributes, then AttributeUnits should be displayed.", "10.001", PackingSlipWrapper.AttributeUnits);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals("When Products Pick Mode is Attribute Specified, then AttributeUnits should be displayed.", "10.001", PackingSlipWrapper.AttributeUnits);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals("When Products Pick Mode is Attribute Neutral, then AttributeUnits should be displayed.", "10.001", PackingSlipWrapper.AttributeUnits);

			OrderLine.SupplierPart.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("When Products Pick Mode is Attribute Neutral and Roll Up is True, then AttributeUnits should not be displayed.", "", PackingSlipWrapper.AttributeUnits);
		}

		#endregion

		#region TestLineUnitsMet

		public void TestLineUnitsMet()
		{
			var releaseLine1 = OrderLine.ReleaseLines[0];
			releaseLine1.Quantity = 5m;
			AssertEquals(5m, PackingSlipWrapper.LineUnitsMet);

			var line2 = Helper.CreateWhsOrderLine(Order, data.Part1, 1m);
			Order.Pick.ClearOrderedInventoriesCache();

			var releaseLine2 = line2.ReleaseLines[0];
			releaseLine2.Quantity = 10m;
			PackingSlipWrapper.AddParentToRollUp(line2);
			AssertEquals(15m, PackingSlipWrapper.LineUnitsMet);
		}

		#endregion

		#region TestLineUnitsOrdered

		public void TestLineUnitsOrdered()
		{
			AssertEquals(10m, PackingSlipWrapper.LineUnitsOrdered);

			OrderLine.WE_TransactionQuantity = 5m;
			AssertEquals(5m, PackingSlipWrapper.LineUnitsOrdered);

			WhsPickableDocketLine line2 = Order.Lines.AddNew();
			line2.WE_TransactionQuantity = 10m;
			PackingSlipWrapper.AddParentToRollUp(line2);
			AssertEquals(15m, PackingSlipWrapper.LineUnitsOrdered);
		}

		#endregion

		#region TestLineNo

		public void TestLineNo()
		{
			OrderLine.WE_LineNo = 5;
			AssertEquals("00005", PackingSlipWrapper.LineNo);

			var line2 = Order.Lines.AddNew();
			line2.WE_LineNo = 7;
			PackingSlipWrapper.AddParentToRollUp(line2);
			AssertEquals("00005", PackingSlipWrapper.LineNo);

			var line3 = Order.Lines.AddNew();
			line3.WE_LineNo = 3;
			PackingSlipWrapper.AddParentToRollUp(line3);
			AssertEquals("00003", PackingSlipWrapper.LineNo);
		}

		#endregion

		#endregion

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
			IPackingSlipWrapper wrapper = DocWhsPackingSlipLine.New(releaseLine, orderLine, Factory);
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

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, Data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Data.Part1, 10m);
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(Data.Org1, Data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, Data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			return DocWhsPackingSlipLine.New(orderLine.ReleaseLines[0], orderLine, Factory);
		}

		WhsOrder Order;
		WhsOrderLine OrderLine;
		DocWhsPackingSlipLine PackingSlipWrapper;
		WhsReleaseLine ReleaseLine;

		protected override void SetUp()
		{
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, Data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Data.Part1, 10m);
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			Order = Helper.CreateWhsOrder(Data.Org1, Data.Whs1);
			OrderLine = Helper.CreateWhsOrderLine(Order, Data.Part1, 10m);
			var pick = Helper.CreatePickNew(Order);

			ReleaseLine = OrderLine.ReleaseLines[0];

			PackingSlipWrapper = DocWhsPackingSlipLine.New(ReleaseLine, OrderLine, Factory);
			base.SetUp();
		}

		TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory)); }
		}

		TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup data;

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
