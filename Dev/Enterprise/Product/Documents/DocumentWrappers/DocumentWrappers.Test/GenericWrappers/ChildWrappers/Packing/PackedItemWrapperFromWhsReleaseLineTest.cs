using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackedItemWrapperFromWhsReleaseLine))]
	sealed class PackedItemWrapperFromWhsReleaseLineTest : PackedItemWrapperTest
	{
		#region TestWrapperMappingFull

		[TestDate(2011, 1, 1)]
		public override void TestWrapperMappingFull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "NO";
			data.Part1.OP_Desc = "Product 1";
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Serial#");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var barcodeRulesHelper = new BarcodeParsingTestHelper(Factory);
			var ruleSet = barcodeRulesHelper.CreateRuleSet(data.Org1, null, data.Part1);
			var ruleAI = barcodeRulesHelper.CreateRule(ruleSet, "ApplicatoinIdentifier", true, true);
			var expiryComponent = barcodeRulesHelper.CreateRuleComponent(ruleAI, WarehouseTargetFields.Codes.ExpiryDate, "17");
			var partAttribute3Component = barcodeRulesHelper.CreateRuleComponent(ruleAI, WarehouseTargetFields.Codes.PartAttrib3, "10");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "002345", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is finalised.", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var consignee = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			order.ConsigneeDocAddress.E2_OA_Address = consignee.PK;
			var relation = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(consignee.Header.PK, OrgPartRelation.RelationshipTypes.WarehouseConsignee);
			relation.OU_LocalPartNumber = "P1-LocalCode";
			relation.OU_LocalPartDescription = "P1 Local Description";
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 5m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "002345", "", "");
			Factory.Save();

			var pick = helper.CreatePickNew(order);

			IPackableItem pickLine = orderLine.PickLines[0];
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.KI_ParentID = pickLine.PK;
			packageItemDivot.KI_ParentTableCode = pickLine.TablePrefix;
			packageItemDivot.KI_PackedQty = 3m;
			packageItemDivot.KI_KP_Package = Factory.New<PkgPackage>().PK;

			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			orgPartRelation.OU_UseExpiryDate = true;
			orgPartRelation.OU_UsePartAttrib3 = true;
			orgPartRelation.OU_LocalPartNumber = "DONTUSETHIS";
			orgPartRelation.OU_LocalPartDescription = "DONTUSETHIS";

			var barcode = data.Part1.PartBarcodes.AddNew();
			barcode.PH_F3_NKPackType = data.Part1.OP_StockKeepingUnit;
			barcode.PH_Barcode = "12345678901234";
			barcode.PH_UseForDocuments = true;

			var packedItem = PkgPackageItemDivotsWrapper.New(packageItemDivot, orderLine.ReleaseLines[0]);
			var wrapperFull = PackedItemWrapper.New(orderLine.ReleaseLines[0], Factory, packedItem);
			AssertEquals("wrapperFull.Code", "P1", wrapperFull.Code);
			AssertEquals("wrapperFull.LocalCode", "P1-LocalCode", wrapperFull.LocalCode);
			AssertEquals("wrapperFull.LocalCodeWithFallback", "P1-LocalCode", wrapperFull.LocalCodeWithFallback);
			AssertEquals("wrapperFull.Description", "Product 1", wrapperFull.Description);
			AssertEquals("wrapperFull.LocalDescription", "P1 Local Description", wrapperFull.LocalDescription);
			AssertEquals("wrapperFull.LocalDescriptionWithFallback", "P1 Local Description", wrapperFull.LocalDescriptionWithFallback);
			AssertEquals("wrapperFull.DescriptionSupplement", "Color: RED, Serial#: SN:0001, Part Attrib. 3: 002345, EXP: 03-Sep-11, PKD: 13-Jul-11", wrapperFull.DescriptionSupplement);
			AssertEquals("wrapperFull.PackedQty", 3m, wrapperFull.PackedQty.Value);
			AssertEquals("wrapperFull.TotalQtyUQ", "NO", wrapperFull.TotalQty.Unit.Code);
			AssertEquals("wrapperFull.TotalQtyUQDescription", "Number", wrapperFull.TotalQty.Unit.Description);
			AssertEquals("wrapperFull.TotalQtyUQWithDescription", "NO - Number", wrapperFull.TotalQty.Unit.CodeAndDescription);
			AssertEquals("wrapperFull.Product", data.Part1, wrapperFull.Product.WrappedObject);
			AssertEquals("wrapperFull.IsExpiryUsed", true, wrapperFull.IsExpiryUsed);
			AssertEquals("wrapperFull.IsPackingDateUsed", true, wrapperFull.IsPackingDateUsed);
			AssertEquals("wrapperFull.IsPartAttrib1Used", true, wrapperFull.IsPartAttrib1Used);
			AssertEquals("wrapperFull.IsPartAttrib2Used", true, wrapperFull.IsPartAttrib2Used);
			AssertEquals("wrapperFull.IsPartAttrib3Used", true, wrapperFull.IsPartAttrib3Used);
			AssertEquals("wrapperFull.PartAttrib1Name", "Color", wrapperFull.PartAttrib1Name);
			AssertEquals("wrapperFull.PartAttrib2Name", "Serial#", wrapperFull.PartAttrib2Name);
			AssertEquals("wrapperFull.PartAttrib3Name", "Part Attrib. 3", wrapperFull.PartAttrib3Name);
			AssertEquals("wrapperFull.PartAttribute1", "RED", wrapperFull.PartAttribute1);
			AssertEquals("wrapperFull.PartAttribute2", "SN:0001", wrapperFull.PartAttribute2);
			AssertEquals("wrapperFull.PartAttribute3", "002345", wrapperFull.PartAttribute3);
			AssertEquals("wrapperFull.ExpiryLabel", "USE BY (ddmmyyyy)", wrapperFull.ExpiryLabel);
			AssertEquals("wrapperFull.Expiry", "03.09.2011", wrapperFull.Expiry);
			AssertEquals("wrapperFull.PackingDate", "13.07.2011", wrapperFull.PackingDate);
			AssertEquals("wrapperFull.BatchLabel", "BATCH/LOT", wrapperFull.BatchLabel);
			AssertEquals("wrapperFull.Batch", "002345", wrapperFull.Batch);
			AssertEquals("wrapperFull.ProductCodeStockUnitBarcodeNumber", "12345678901234", wrapperFull.ProductCodeStockUnitBarcodeNumber);
			AssertEquals("wrapperFull.ProductBarcodeWithPrefixes", "(02)12345678901234(37)3(17)110903(10)002345", wrapperFull.ProductBarcodeWithPrefixes);
			AssertEquals("wrapperFull.ProductBarcode", "ÈÆ0Ã57Mcy!7KiÆ1+)#*¯7MhÊ", wrapperFull.ProductBarcode);

			relation.OU_LocalPartNumber = "";
			relation.OU_LocalPartDescription = "";
			orgPartRelation.OU_UseExpiryDate = false;
			data.Part1.OP_StockKeepingUnit = "XXX";
			AssertEquals("Local Code should be empty if no Local Code exists.", "", wrapperFull.LocalCode);
			AssertEquals("No Local Code exists, should fall back to Product Code.", "P1", wrapperFull.LocalCodeWithFallback);
			AssertEquals("Local Description should be empty if no Local Description exists.", "", wrapperFull.LocalDescription);
			AssertEquals("No Local Description exists, should fall back to Product Description.", "Product 1", wrapperFull.LocalDescriptionWithFallback);
			AssertEquals("wrapperFull.TotalQtyUQ", "XXX", wrapperFull.TotalQty.Unit.Code);
			AssertEquals("wrapperFull.TotalQtyUQDescription", "XXX", wrapperFull.TotalQty.Unit.Description);
			AssertEquals("wrapperFull.TotalQtyUQWithDescription", "XXX", wrapperFull.TotalQty.Unit.CodeAndDescription);
			AssertEquals("wrapperFull.IsExpiryUsed", false, wrapperFull.IsExpiryUsed);
		}

		#endregion

		#region TestCustomFields

		public void TestCustomFields()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateCustomLabel(data.Org1, Constants.CustomLabels.WhsDocketLine.CustomAttribute1, "PO#", false);
			helper.CreateCustomLabel(data.Org1, Constants.CustomLabels.WhsDocketLine.CustomAttribute2, "QAReviewer", false);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_CustomAttrib1 = "PO12";
			var pick = helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack_ForTesting(orderLine.ReleaseLines[0], 5m);
			var wrapper = new PackedItemWrapperFromWhsReleaseLine(orderLine.ReleaseLines[0], new[] { packedItem }, Factory);
			AssertEquals("Should add both Custom Fields that are enabled.", 2, wrapper.CustomFields.Count);
			AssertEquals("Custom Field should return correct Caption.", "PO#", wrapper.CustomFields[Constants.CustomLabels.WhsDocketLine.CustomAttribute1].Caption);
			AssertEquals("Custom Field should get value from OrderLine.", "PO12", wrapper.CustomFields[Constants.CustomLabels.WhsDocketLine.CustomAttribute1].Value);
			AssertEquals("Custom Field should return correct Caption.", "QAReviewer", wrapper.CustomFields[Constants.CustomLabels.WhsDocketLine.CustomAttribute2].Caption);
			AssertEquals("OrderLine does not have value for this Custom Field, so should be empty.", "", wrapper.CustomFields[Constants.CustomLabels.WhsDocketLine.CustomAttribute2].Value);
		}

		#endregion

		#region Expiry

		[TestDate(2011, 1, 1)]
		public void TestExpiry()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Serial#");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var barcodeRulesHelper = new BarcodeParsingTestHelper(Factory);
			var ruleSet = barcodeRulesHelper.CreateRuleSet(data.Org1, null, data.Part1);
			var ruleAI = barcodeRulesHelper.CreateRule(ruleSet, "ApplicatoinIdentifier", true, true);
			var expiryComponent = barcodeRulesHelper.CreateRuleComponent(ruleAI, WarehouseTargetFields.Codes.ExpiryDate, "15");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "BN:00234", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 5m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "BN:00234", "", "");
			var pick = helper.CreatePickNew(order);

			IPackableItem pickLine = orderLine.PickLines[0];
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.KI_ParentID = pickLine.PK;
			packageItemDivot.KI_ParentTableCode = pickLine.TablePrefix;
			packageItemDivot.KI_PackedQty = 3m;

			var releaseLine = orderLine.ReleaseLines[0];
			var packedItem = PkgPackageItemDivotsWrapper.New(packageItemDivot, releaseLine);

			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			orgPartRelation.OU_UseExpiryDate = true;
			var wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("ExpiryLabel", "BEST BEFORE (ddmmyyyy)", wrapper.ExpiryLabel);
			AssertEquals("Expiry", "03.09.2011", wrapper.Expiry);

			expiryComponent.BRC_ApplicationID = "17";
			PackingRegistry.Instance.ProductLabelDateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "yy.MM.dd");
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("ExpiryLabel", "USE BY (yymmdd)", wrapper.ExpiryLabel);
			AssertEquals("Expiry", "11.09.03", wrapper.Expiry);

			expiryComponent.BRC_ApplicationID = "1";
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("ExpiryLabel", "EXPIRY DATE (yymmdd)", wrapper.ExpiryLabel);
			AssertEquals("Expiry", "11.09.03", wrapper.Expiry);
		}

		#endregion

		#region TestGetApplicationIdentifier

		public void TestGetApplicationIdentifier()
		{
			// use GetBatch to test if GetApplicationIdentifier returns expected result
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var barcodeRulesHelper = new BarcodeParsingTestHelper(Factory);
			var ruleSet = barcodeRulesHelper.CreateRuleSet(data.Org1, null, data.Part1);
			var ruleAI = barcodeRulesHelper.CreateRule(ruleSet, "ApplicatoinIdentifier", true, true); // IsGS1: true, IsPartial: true
			var partAttribute1Component = barcodeRulesHelper.CreateRuleComponent(ruleAI, WarehouseTargetFields.Codes.PartAttrib1, "10");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var today = ZDate.Today;
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "RED", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "RED", "", "", "", "");
			var pick = helper.CreatePickNew(order);

			IPackableItem pickLine = orderLine.PickLines[0];
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.KI_ParentID = pickLine.PK;
			packageItemDivot.KI_ParentTableCode = pickLine.TablePrefix;
			packageItemDivot.KI_PackedQty = 3m;

			var releaseLine = orderLine.ReleaseLines[0];
			var packedItem = PkgPackageItemDivotsWrapper.New(packageItemDivot, releaseLine);

			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			orgPartRelation.OU_UsePartAttrib1 = true;
			var wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("Batch", "RED", wrapper.Batch);

			ruleAI.IsGS1 = false;
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("Batch", "", wrapper.Batch); // no rule found

			ruleAI.IsGS1 = true;
			ruleAI.IsPartialRule = false;
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("Batch", "", wrapper.Batch); // no rule found

			ruleAI.IsPartialRule = true;
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("Batch", "RED", wrapper.Batch);  // found rule
		}

		#endregion

		#region Batch

		[TestDate(2011, 1, 1)]
		public void TestBatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Serial#");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var barcodeRulesHelper = new BarcodeParsingTestHelper(Factory);
			var ruleSet = barcodeRulesHelper.CreateRuleSet(data.Org1, null, data.Part1);
			var ruleAI = barcodeRulesHelper.CreateRule(ruleSet, "ApplicatoinIdentifier", true, true);
			var partAttribute1Component = barcodeRulesHelper.CreateRuleComponent(ruleAI, WarehouseTargetFields.Codes.PartAttrib1, "10");
			var partAttribute2Component = barcodeRulesHelper.CreateRuleComponent(ruleAI, WarehouseTargetFields.Codes.PartAttrib2, "10");
			var partAttribute3Component = barcodeRulesHelper.CreateRuleComponent(ruleAI, WarehouseTargetFields.Codes.PartAttrib3, "10");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "BN:00234", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 5m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "BN:00234", "", "");
			var pick = helper.CreatePickNew(order);

			IPackableItem pickLine = orderLine.PickLines[0];
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.KI_ParentID = pickLine.PK;
			packageItemDivot.KI_ParentTableCode = pickLine.TablePrefix;
			packageItemDivot.KI_PackedQty = 3m;

			var releaseLine = orderLine.ReleaseLines[0];
			var packedItem = PkgPackageItemDivotsWrapper.New(packageItemDivot, releaseLine);

			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			orgPartRelation.OU_UsePartAttrib1 = true;
			orgPartRelation.OU_UsePartAttrib2 = true;
			orgPartRelation.OU_UsePartAttrib3 = true;
			var wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("BatchLabel", "BATCH/LOT", wrapper.BatchLabel);
			AssertEquals("Batch", "RED", wrapper.Batch);

			orgPartRelation.OU_UsePartAttrib1 = false;
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("BatchLabel", "BATCH/LOT", wrapper.BatchLabel);
			AssertEquals("Batch", "SN:0001", wrapper.Batch);

			orgPartRelation.OU_UsePartAttrib2 = false;
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("BatchLabel", "BATCH/LOT", wrapper.BatchLabel);
			AssertEquals("Batch", "BN:00234", wrapper.Batch);

			orgPartRelation.OU_UsePartAttrib3 = false;
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("Batch", "", wrapper.Batch);

			orgPartRelation.OU_UsePartAttrib1 = true;
			orgPartRelation.OU_UsePartAttrib2 = true;
			orgPartRelation.OU_UsePartAttrib3 = true;

			partAttribute1Component.BRC_ApplicationID = "1";
			partAttribute2Component.BRC_ApplicationID = "1";
			partAttribute3Component.BRC_ApplicationID = "1";

			data.Org1.MiscServ.OM_IMPartAttrib3Type = "BAT";
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("Batch", "BN:00234", wrapper.Batch);

			data.Org1.MiscServ.OM_IMPartAttrib2Type = "BAT";
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("Batch", "SN:0001", wrapper.Batch);

			data.Org1.MiscServ.OM_IMPartAttrib1Type = "BAT";
			wrapper = PackedItemWrapper.New(releaseLine, Factory, packedItem);
			AssertEquals("Batch", "RED", wrapper.Batch);
		}

		#endregion

		#region ProductBarcode

		[TestDate(2011, 1, 1)]
		public void TestProductBarcode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Serial#");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "002345", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 5m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "002345", "", "");
			var pick = helper.CreatePickNew(order);

			IPackableItem pickLine = orderLine.PickLines[0];
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.KI_ParentID = pickLine.PK;
			packageItemDivot.KI_ParentTableCode = pickLine.TablePrefix;
			packageItemDivot.KI_PackedQty = 3m;

			var packedItem = PkgPackageItemDivotsWrapper.New(packageItemDivot, orderLine.ReleaseLines[0]);
			var wrapperFull = PackedItemWrapper.New(orderLine.ReleaseLines[0], Factory, packedItem);
			AssertEquals("wrapperFull.ProductBarcodeWithPrefixes", "(02)P1(37)3(17)110903", wrapperFull.ProductBarcodeWithPrefixes);
			AssertEquals("wrapperFull.ProductBarcode", "ÈÆ02PÃ-iÆ1+)#!Ê", wrapperFull.ProductBarcode);

			var barcode = data.Part1.PartBarcodes.AddNew();
			barcode.PH_F3_NKPackType = data.Part1.OP_StockKeepingUnit;
			barcode.PH_UseForDocuments = true;
			AssertEquals("wrapperFull.ProductBarcodeWithPrefixes", "(02)P1(37)3(17)110903", wrapperFull.ProductBarcodeWithPrefixes);
			AssertEquals("wrapperFull.ProductBarcode", "ÈÆ02PÃ-iÆ1+)#!Ê", wrapperFull.ProductBarcode);

			barcode.PH_Barcode = "12345678901234";
			AssertEquals("wrapperFull.ProductBarcodeWithPrefixes", "(02)12345678901234(37)3(17)110903", wrapperFull.ProductBarcodeWithPrefixes);
			AssertEquals("wrapperFull.ProductBarcode", "ÈÆ0Ã57Mcy!7KiÆ1+)#@Ê", wrapperFull.ProductBarcode);

			barcode.PH_Barcode = "123456789012345";
			AssertEquals("wrapperFull.ProductBarcodeWithPrefixes", "(02)123456789012345(37)3(17)110903", wrapperFull.ProductBarcodeWithPrefixes);
			AssertEquals("wrapperFull.ProductBarcode", "ÉÆ\",BXnz,BUiÆ1+)#9Ê", wrapperFull.ProductBarcode);
		}

		#endregion

		#region CustomPropertyMissing

		public void TestCustomPropertyMissing()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_CustomAttrib1 = "PO12";
			var pick = helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack_ForTesting(orderLine.ReleaseLines[0], 5m);
			var wrapper = new PackedItemWrapperFromWhsReleaseLineWithMissingProperty(packedItem, Factory);

			AssertEquals("Missing Property", "", wrapper.Code);
		}

		#endregion

		#region TestCommonCurrency

		public void TestCommonCurrency_HasCommonCurrency()
		{
			var uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack_ForTesting(orderLine.ReleaseLines[0], 5m);
			var wrapper = new PackedItemWrapperFromWhsReleaseLine(orderLine.ReleaseLines[0], new[] { packedItem }, Factory);
			AssertEquals("Wrapper has non-empty currency wrapper", uSD.RX_Code + " - " + uSD.RX_Desc, wrapper.CommonCurrency.CodeAndDescription);
		}

		public void TestCommonCurrency_NoCommonCurrency()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack_ForTesting(orderLine.ReleaseLines[0], 5m);
			var wrapper = new PackedItemWrapperFromWhsReleaseLine(orderLine.ReleaseLines[0], new[] { packedItem }, Factory);
			AssertEquals("Wrapper has empty currency wrapper", ZString.Empty, wrapper.CommonCurrency.CodeAndDescription);
		}

		public void TestCommonCurrency_TwoCurrencies()
		{
			var uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			var aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine1.WE_RX_NKUnitPriceCurrency = "USD";

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "AUD";
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack_ForTesting(orderLine1.ReleaseLines[0], 5m);
			var wrapper = new PackedItemWrapperFromWhsReleaseLine(orderLine1.ReleaseLines[0], new[] { packedItem }, Factory);
			AssertEquals("Wrapper has empty currency wrapper", ZString.Empty, wrapper.CommonCurrency.CodeAndDescription);
		}

		#endregion

		#region TestLinePrice

		public void TestLinePrice_DoesNotHaveCommonCurrency()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_UnitPriceAfterDiscount = 15.45m;
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack_ForTesting(orderLine.ReleaseLines[0], 5m);
			var wrapper = new PackedItemWrapperFromWhsReleaseLine(orderLine.ReleaseLines[0], new[] { packedItem }, Factory);

			AssertEquals("Expected 0m LinePrice as not currency set.", 0m, wrapper.LinePrice);
		}

		public void TestLinePrice_HasCommonCurrency_OneProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine.WE_UnitPriceAfterDiscount = 15.45m;
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack_ForTesting(orderLine.ReleaseLines[0], 5m);
			var wrapper = new PackedItemWrapperFromWhsReleaseLine(orderLine.ReleaseLines[0], new[] { packedItem }, Factory);

			AssertEquals("Expected correct LinePrice returned.", 77.25m, wrapper.LinePrice);
		}

		public void TestLinePrice_HasCommonCurrency_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine1.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine1.WE_UnitPriceAfterDiscount = 56.27m;

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 10m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine2.WE_UnitPriceAfterDiscount = 79.38m;

			helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");
			var packedItem1 = package.Pack_ForTesting(orderLine1.ReleaseLines[0], 4m);
			var wrapper1 = new PackedItemWrapperFromWhsReleaseLine(orderLine1.ReleaseLines[0], new[] { packedItem1 }, Factory);
			AssertEquals("Expected correct LinePrice returned.", 225.08m, wrapper1.LinePrice);

			var packedItem2 = package.Pack_ForTesting(orderLine2.ReleaseLines[0], 7m);
			var wrapper2 = new PackedItemWrapperFromWhsReleaseLine(orderLine2.ReleaseLines[0], new[] { packedItem2 }, Factory);
			AssertEquals("Expected correct LinePrice returned.", 555.66m, wrapper2.LinePrice);
		}

		public void TestLinePrice_OneProductInMultiplePackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine1.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine1.WE_UnitPriceAfterDiscount = 56.27m;

			helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packageJob.Packages.AddNew("BOX");
			var packedItem1 = package1.Pack_ForTesting(orderLine1.ReleaseLines[0], 4m);
			var wrapper1 = new PackedItemWrapperFromWhsReleaseLine(orderLine1.ReleaseLines[0], new[] { packedItem1 }, Factory);
			AssertEquals("Expected correct LinePrice returned.", 225.08m, wrapper1.LinePrice);

			var package2 = packageJob.Packages.AddNew("CTN");
			var packedItem2 = package2.Pack_ForTesting(orderLine1.ReleaseLines[0], 6m);
			var wrapper2 = new PackedItemWrapperFromWhsReleaseLine(orderLine1.ReleaseLines[0], new[] { packedItem2 }, Factory);
			AssertEquals("Expected correct LinePrice returned.", 337.62m, wrapper2.LinePrice);
		}

		#endregion

		#region Implementation

		protected override void AssertTotalQtyAndExpiryLabel(PackedItemWrapper wrapperEmpty)
		{
			AssertEquals("wrapperEmpty.TotalQtyUQ", "UNT", wrapperEmpty.TotalQty.Unit.Code);
			AssertEquals("wrapperEmpty.TotalQtyUQDescription", "Unit", wrapperEmpty.TotalQty.Unit.Description);
			AssertEquals("wrapperEmpty.TotalQtyUQWithDescription", "UNT - Unit", wrapperEmpty.TotalQty.Unit.CodeAndDescription);
			AssertEquals("wrapperEmpty.ExpiryLabel", "EXPIRY DATE (ddmmyyyy)", wrapperEmpty.ExpiryLabel);
		}

		protected override string ExpectedWrapperNameInFieldMap => "PackedItemFromWhsReleaseLine";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = order.Lines.AddNew();
			var releaseline = orderLine.ReleaseLines.AddNew();
			return new PackedItemWrapperFromWhsReleaseLine(releaseline, Array.Empty<PkgPackageItemDivotsWrapper>(), Factory);
		}

		public class PackedItemWrapperFromWhsReleaseLineWithMissingProperty : PackedItemWrapperFromWhsReleaseLine
		{
			public PackedItemWrapperFromWhsReleaseLineWithMissingProperty(PkgPackageItemDivotsWrapper packedItem, BusinessObjectFactory factory)
				: base((WhsReleaseLine)packedItem.PackableItemParent, new[] { packedItem }, factory)
			{
			}

			protected override ZString GetCode()
			{
				return GetCustomProperty("Missing Property");
			}
		}

		#endregion
	}
}
