using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BarcodeParsingEngine;
using Enterprise.BarcodeParsingEngine.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeParsingDiagnostics))]
	class BarcodeParsingDiagnosticsTest : NonPersistentBusinessObjectTestCase
	{
		#region TestBarcode

		public void TestBarcode()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.Barcode = string.Format("{0}ABCDE{0}FDDD{0}GH{0}", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator);
			AssertEquals(string.Format("ABCDE{0}FDDD{0}GH", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator), diagnostic.Barcode);

			diagnostic.Barcode = string.Format("ABCDE{0}FDDD{0}GH{0}", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator);
			AssertEquals(string.Format("ABCDE{0}FDDD{0}GH", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator), diagnostic.Barcode);

			diagnostic.Barcode = string.Format("ABCDE{0}FDDD{0}GH", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator);
			AssertEquals(string.Format("ABCDE{0}FDDD{0}GH", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator), diagnostic.Barcode);

			diagnostic.Barcode = string.Format("{0}ABCDE{0}FDDD{0}GH", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator);
			AssertEquals(string.Format("ABCDE{0}FDDD{0}GH", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator), diagnostic.Barcode);
		}

		#endregion

		#region TestBuyer

		public void TestBuyer()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var buyer = diagnostic.Factory.New<OrgHeader>();
			diagnostic.BuyerPK = buyer.PK;
			AssertEquals(buyer, diagnostic.Buyer);

			diagnostic.RelatedEntityPK = Guid.NewGuid();
			diagnostic.ModuleCode = "";
			diagnostic.BuyerPK = Guid.NewGuid();
			AssertEquals("Since module code is invalid, related entity pk should be cleared.", Guid.Empty, diagnostic.RelatedEntityPK);

			var relatedEntityPKForWarehouseModule = Guid.NewGuid();
			diagnostic.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
			diagnostic.RelatedEntityPK = relatedEntityPKForWarehouseModule;
			diagnostic.BuyerPK = Guid.NewGuid();
			AssertEquals("Since module code is warehouse, related entity pk should be not cleared.", relatedEntityPKForWarehouseModule, diagnostic.RelatedEntityPK);
		}

		#endregion

		#region TestBuyerCaption

		public void TestBuyerCaption()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var factory = diagnostic.Factory;

			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(factory))
			{
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;
				AssertEquals("Dummy Buyer", diagnostic.BuyerCaption);

				diagnostic.ModuleCode = "";
				AssertEquals("Buyer", diagnostic.BuyerCaption);
			}
		}

		#endregion

		#region TestDefaultValues

		public void TestDefaultValues()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			AssertEquals(BarcodeModuleTypes.Codes.Warehouse, diagnostic.ModuleCode);
		}

		#endregion

		#region TestModuleCode

		public void TestModuleCode()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
			diagnostic.RelatedEntityPK = Guid.NewGuid();
			diagnostic.ModuleCode = "ABC";
			AssertEquals("Since module code is changed, related entity pk should be cleared.", Guid.Empty, diagnostic.RelatedEntityPK);
		}

		#endregion

		#region TestSupplier

		public void TestSupplier()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var supplier = diagnostic.Factory.New<OrgHeader>();
			diagnostic.SupplierPK = supplier.PK;
			AssertEquals(supplier, diagnostic.Supplier);
		}

		#endregion

		#region TestSupplierCaption

		public void TestSupplierCaption()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var factory = diagnostic.Factory;

			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(factory))
			{
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;
				AssertEquals("Dummy Supplier", diagnostic.SupplierCaption);

				diagnostic.ModuleCode = "";
				AssertEquals("Supplier", diagnostic.SupplierCaption);
			}
		}

		#endregion

		#region TestIsRelatedEntityAvailable

		public void TestIsRelatedEntityAvailable()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var factory = diagnostic.Factory;

			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(factory))
			{
				var dummy = DummyBarcodeParsingConsumer.GetDummy(factory);
				dummy.IsRelatedEntityAvailable = true;
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;
				AssertEquals(true, diagnostic.IsRelatedEntityAvailable);

				dummy.IsRelatedEntityAvailable = false;
				AssertEquals(false, diagnostic.IsRelatedEntityAvailable);
			}
		}

		#endregion

		#region TestRelatedEntityCaption

		public void TestRelatedEntityCaption()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var factory = diagnostic.Factory;

			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(factory))
			{
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;
				AssertEquals("Dummy Entity", diagnostic.RelatedEntityCaption);

				diagnostic.ModuleCode = "";
				AssertEquals("Related Entity", diagnostic.RelatedEntityCaption);
			}
		}

		#endregion

		#region TestRunBarcodeParsingRules

		public void TestRunBarcodeParsingRules()
		{
			var buyer = Helper.CreateOrg("Buyer");
			var supplier = Helper.CreateOrg("Supplier");
			var part = Helper.CreatePart("P1", buyer);
			var ruleSet = Helper.CreateRuleSet(buyer, supplier, part);
			var ruleForProductCode = Helper.CreateRule(ruleSet, "ProductCode", false, true, "@");
			var ruleForAttribute1 = Helper.CreateRule(ruleSet, "Attribute", false, false, "#");
			var componentForProductCode = Helper.CreateRuleComponent(ruleForProductCode, "PRC", "X");
			var component1ForAttribute1 = Helper.CreateRuleComponent(ruleForAttribute1, "PA1", "A1");
			var component2ForAttribute1 = Helper.CreateRuleComponent(ruleForAttribute1, "PA2", "A2");
			var component3ForAttribute1 = Helper.CreateRuleComponent(ruleForAttribute1, BarcodeCaptureConstants.IgnoreCode, "T");
			Factory.Save();

			var diagnostics = new BarcodeParsingDiagnostics(Factory);
			diagnostics.BuyerPK = buyer.PK;
			diagnostics.SupplierPK = supplier.PK;
			diagnostics.RelatedEntityPK = part.PK;
			diagnostics.Barcode = "X123@RTABCDX234@GSCA1Attribute1#ABCW";
			diagnostics.Run();
			AssertEquals(
@"The following rules were matched:
Rule No.: 1 - Name: ProductCode

The following Data was matched:

Field - Product Code:
123
234", diagnostics.Results.Trim().ToString());
			AssertEquals(ruleForProductCode, diagnostics.LastMatchedRule);

			diagnostics.Barcode = "A1Attribute1#A2Attribute2#TYHGD@";
			diagnostics.Run();
			AssertEquals(
@"The following rules were matched:
Rule No.: 2 - Name: Attribute

The following Data was matched:

Field - Part Attribute 1:
Attribute1

Field - Part Attribute 2:
Attribute2", diagnostics.Results.Trim().ToString());
			AssertEquals(ruleForAttribute1, diagnostics.LastMatchedRule);

			diagnostics.Barcode = "A1Attribute1%A2Attribute2#TYHGD%";
			diagnostics.Run();
			AssertEquals("No rules were matched.", diagnostics.Results.Trim().ToString());
			AssertNull(diagnostics.LastMatchedRule);

			diagnostics.Barcode = "";
			diagnostics.Run();
			AssertEquals("No rules were matched.", diagnostics.Results.Trim().ToString());
			AssertNull(diagnostics.LastMatchedRule);
		}

		#endregion

		#region TestRunBarcodeParsingRulesForGS1

		public void TestRunBarcodeParsingRulesForGS1()
		{
			//Relies on System defined barcode parsing rules for GS1
			var diagnostics = new BarcodeParsingDiagnostics(Factory);
			diagnostics.Barcode = string.Format("{0}02093123450000053720{0}1505120110246813{0}", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator);
			diagnostics.Run();
			AssertEquals(
@"The following rules were matched:
Rule No.: 3 - Name: GTIN of Trade Items Contained in a Logistic Unit
Rule No.: 6 - Name: Count of Trade Items Contained in a Logistic Unit

The following Data was matched:

Field - Product Code:
09312345000005

Field - Quantity:
20", diagnostics.Results.Trim().ToString());
		}

		public void TestRunBarcodeParsingRulesForGS1_MatchSamePartBarcode()
		{
			//Relies on System defined barcode parsing rules for GS1
			var diagnostics = new BarcodeParsingDiagnostics(Factory);
			diagnostics.Barcode = "0012345678912345678900";
			diagnostics.IsGS1Barcode = true;
			diagnostics.Run();
			AssertEquals(
@"The following rules were matched:
Rule No.: 1 - Name: Serial Shipping Container Code
Rule No.: 7 - Name: Serial Shipping Container Code

The following Data was matched:

Field - Pallet ID:
123456789123456789

Field - Package ID:
123456789123456789", diagnostics.Results.Trim().ToString());
		}

		public void TestRunBarcodeParsingRulesForGS1_MatchDiffPartBarcode()
		{
			//Update System defined barcode parsing rules for GS1
			var diagnostics = new BarcodeParsingDiagnostics(Factory);
			var query = new ZQuery();
			query.AddToFilter(BarcodeRuleComponentSchema.BRC_TargetField, "PCK");

			var component = Factory.LoadTop1<BarcodeRuleComponent>(query);
			component.BRC_MinLength = 1;
			component.BRC_MaxLength = 46;
			component.BRC_Format = "AN";

			diagnostics.Barcode = "0032145678912345678900";
			diagnostics.IsGS1Barcode = true;
			diagnostics.Run();
			AssertEquals(
@"No rules were matched.", diagnostics.Results.Trim().ToString());
		}

		#endregion

		public void TestRunBarcodeValidationRules_WhenRuleMatchAndBarcodeIsValid()
		{
			var buyer = Helper.CreateOrg("Buyer");
			var supplier = Helper.CreateOrg("Supplier");
			var part = Helper.CreatePart("P1", buyer);
			var ruleSet = Helper.CreateRuleSet(buyer, supplier, part);
			var rule = Helper.CreateValidationRule(ruleSet, "PRC");
			Factory.Save();

			var diagnostics = new BarcodeParsingDiagnostics(Factory);
			diagnostics.DiagnosticsType = DiagnosticsTypes.Codes.Validation;
			diagnostics.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
			diagnostics.BuyerPK = buyer.PK;
			diagnostics.SupplierPK = supplier.PK;
			diagnostics.RelatedEntityPK = part.PK;
			diagnostics.Barcode = "Test123";
			diagnostics.TargetField = nameof(WarehouseTargetField.PRC);
			diagnostics.Run();
			AssertEquals("A validation rule exists for the current configuration and Target Field. The barcode is valid for the matched rule.", diagnostics.Results.ToString());
			AssertEquals(rule, diagnostics.LastMatchedRule);
		}

		public void TestRunBarcodeValidationRules_WhenRuleMatchAndBarcodeIsInvalid()
		{
			var buyer = Helper.CreateOrg("Buyer");
			var supplier = Helper.CreateOrg("Supplier");
			var part = Helper.CreatePart("P1", buyer);
			var ruleSet = Helper.CreateRuleSet(buyer, supplier, part);
			var rule = Helper.CreateValidationRule(ruleSet, nameof(WarehouseTargetField.PRC), format: nameof(FormatType.A));
			Factory.Save();

			var diagnostics = new BarcodeParsingDiagnostics(Factory);
			diagnostics.DiagnosticsType = DiagnosticsTypes.Codes.Validation;
			diagnostics.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
			diagnostics.BuyerPK = buyer.PK;
			diagnostics.SupplierPK = supplier.PK;
			diagnostics.RelatedEntityPK = part.PK;
			diagnostics.Barcode = "Test123";
			diagnostics.TargetField = nameof(WarehouseTargetField.PRC);
			diagnostics.Run();
			AssertEquals("A validation rule exists for the current configuration and Target Field. The barcode is not valid for the matched rule.", diagnostics.Results.ToString());
			AssertEquals(rule, diagnostics.LastMatchedRule);
		}

		public void TestRunBarcodeValidationRules_WhenNoRuleMatch()
		{
			var buyer = Helper.CreateOrg("Buyer");
			var supplier = Helper.CreateOrg("Supplier");
			var part = Helper.CreatePart("P1", buyer);
			Factory.Save();

			var diagnostics = new BarcodeParsingDiagnostics(Factory);
			diagnostics.DiagnosticsType = DiagnosticsTypes.Codes.Validation;
			diagnostics.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
			diagnostics.BuyerPK = buyer.PK;
			diagnostics.SupplierPK = supplier.PK;
			diagnostics.RelatedEntityPK = part.PK;
			diagnostics.Barcode = "Test123";
			diagnostics.TargetField = nameof(WarehouseTargetField.PRC);
			diagnostics.Run();
			AssertEquals("No validation rule was found matching the current configuration and Target Field.", diagnostics.Results.Trim().ToString());
			AssertNull(diagnostics.LastMatchedRule);
		}

		#region TestIsGS1Override

		public void TestIsGS1Override()
		{
			var diagnostics = new BarcodeParsingDiagnostics(Factory);
			AssertEquals(false, diagnostics.IsGS1Barcode);

			diagnostics.Barcode = string.Format("{0}02093123450000053720{0}1505120110246813{0}", BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator);
			AssertEquals(true, diagnostics.IsGS1Barcode);

			diagnostics.Barcode = "";
			AssertEquals(false, diagnostics.IsGS1Barcode);

			diagnostics.IsGS1Barcode = true;
			AssertEquals(true, diagnostics.IsGS1Barcode);
		}

		#endregion

		#region Implementation

		#region Helper

		BarcodeParsingTestHelper Helper
		{
			get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
		}
		BarcodeParsingTestHelper helper;

		#endregion

		#endregion
	}
}
