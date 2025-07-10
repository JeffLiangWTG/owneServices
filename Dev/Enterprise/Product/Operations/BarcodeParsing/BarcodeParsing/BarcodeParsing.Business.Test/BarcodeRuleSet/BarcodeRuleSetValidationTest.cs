using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeRuleSetValidationTest : BarcodeParsingValidationTestCase
	{
		#region TestCheckBRS_Module

		public void TestCheckBRS_Module()
		{
			var ruleSet = Helper.CreateRuleSet();
			AssertNoErrors("Precondition", ruleSet.BRS_ModuleInfo);

			ruleSet.BRS_Module = "";
			AssertHasError(ruleSet.BRS_ModuleInfo, "Please enter a Module.");

			ruleSet.BRS_Module = "WHS";
			AssertNoErrors(ruleSet.BRS_ModuleInfo);

			ruleSet.BRS_Module = "XXX";
			AssertHasError(ruleSet.BRS_ModuleInfo, "Enter a valid Module.");

			ruleSet.BRS_Module = "WHS";
			AssertNoErrors(ruleSet.BRS_ModuleInfo);

			ruleSet.BRS_Module = "ETL";
			AssertNoErrors(ruleSet.BRS_ModuleInfo);
		}

		public void TestWarehouseModule_WithValidationRules()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = BarcodeModuleTypes.Codes.Warehouse;
			var parsingRule = Helper.CreateRule(ruleSet);
			var component = parsingRule.Components.AddNew();
			component.BRC_ApplicationID = "TestApp";
			component.BRC_MinLength = 1;
			component.BRC_MaxLength = 9;
			component.BRC_Format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols;
			component.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			Helper.CreateValidationRule(ruleSet, 1, 2, DummyTargetFields.Codes.TargetField1, GS1DataFormatTypes.Codes.AlphaNumericWithSymbols);

			AssertNoErrors(ruleSet.BRS_ModuleInfo);
		}

		public void TestModuleWithoutSupport_WithValidationRules()
		{
			OverrideDummyBarcodeParsingConsumer(new DummyBarcodeParsingConsumer(Factory));

			var ruleSet = Helper.CreateRuleSet();
			var errorMessage = "This module does not support validation rules.";

			var parsingRule = Helper.CreateRule(ruleSet);
			var component = parsingRule.Components.AddNew();
			component.BRC_ApplicationID = "TestApp";
			component.BRC_MinLength = 1;
			component.BRC_MaxLength = 9;
			component.BRC_Format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols;
			component.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			Helper.CreateValidationRule(ruleSet, 1, 2, DummyTargetFields.Codes.TargetField1, GS1DataFormatTypes.Codes.AlphaNumericWithSymbols);

			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertHasError(ruleSet.BRS_ModuleInfo, errorMessage);
		}

		#endregion

		#region TestCheckBRS_RelatedEntity

		#region TestCheckBRS_RelatedEntity_ChecksPKIsInCollection

		public void TestCheckBRS_RelatedEntity_ChecksPKIsInCollection()
		{
			var buyer = Helper.CreateOrg("Buyer");
			Factory.Save();

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			var companies = new GlbCompanyCollection(Factory);
			dummy.IsRelatedEntityAvailable = true;
			dummy.RelatedEntityList = companies;
			var ruleSet = Helper.CreateRuleSet(null, null, GlbCompany.CurrentCompany);
			AssertEquals("Precondition: Related Entity is set.", GlbCompany.CurrentCompany.PK, ruleSet.BRS_RelatedEntityId);
			AssertNoErrors("Precondition", ruleSet.BRS_RelatedEntityIdInfo);

			companies.AdditionalFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			ruleSet.BRS_OH_Buyer = buyer.PK;
			AssertHasError(ruleSet.BRS_RelatedEntityIdInfo, "Enter a valid Dummy Entity.");
		}

		#endregion

		#region TestCheckBRS_RelatedEntity_WarnsAboutUsageOfRelatedEntityField

		public void TestCheckBRS_RelatedEntity_WarnsAboutUsageOfRelatedEntityField()
		{
			var ruleSet = Helper.CreateRuleSet();
			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.RelatedEntityList = new GlbCompanyCollection(Factory);
			AssertNoWarnings("Precondition", ruleSet.BRS_RelatedEntityIdInfo);

			ruleSet.BRS_RelatedEntityId = GlbCompany.CurrentCompany.PK;
			AssertHasWarning(ruleSet.BRS_RelatedEntityIdInfo, "Setting a Dummy Entity is not advised, only use this if Parsing Rules will differ per Dummy Entity.");
		}

		#endregion

		#endregion

		#region TestCheckForDuplicateCombination

		public void TestCheckForDuplicateCombination()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var buyer2 = Helper.CreateOrg("Buyer2");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var supplier2 = Helper.CreateOrg("Supplier2");

			var ruleSet1 = Helper.CreateRuleSet(buyer1);
			Factory.Save();

			var ruleSet2 = Helper.CreateRuleSet();
			AssertNoErrors("Precondition", ruleSet2.BRS_OH_BuyerInfo);

			ruleSet2.BRS_OH_Buyer = buyer1.PK;
			AssertHasError(ruleSet2.BRS_OH_BuyerInfo, "A Dummy Rule Set for Buyer (Buyer1) and Supplier (<none>) already exists.");

			ruleSet2.BRS_OH_Buyer = buyer2.PK;
			AssertNoErrors(ruleSet2.BRS_OH_BuyerInfo);

			ruleSet2.BRS_IsSystem = true;
			ruleSet2.BRS_OH_Buyer = buyer1.PK;
			AssertNoErrors(ruleSet2.BRS_OH_BuyerInfo);

			ruleSet2.BRS_IsSystem = false;
			ruleSet2.BRS_OH_Buyer = buyer1.PK;
			AssertHasError(ruleSet2.BRS_OH_BuyerInfo, "A Dummy Rule Set for Buyer (Buyer1) and Supplier (<none>) already exists.");

			ruleSet2.BRS_OH_Supplier = supplier1.PK;
			AssertNoErrors(ruleSet2.BRS_OH_SupplierInfo);
			AssertNoErrors(ruleSet2.BRS_OH_BuyerInfo);

			Factory.Save();

			var ruleSet3 = Helper.CreateRuleSet();
			AssertNoErrors("Precondition", ruleSet3.BRS_OH_BuyerInfo);
			AssertNoErrors("Precondition", ruleSet3.BRS_OH_SupplierInfo);

			ruleSet3.BRS_OH_Buyer = buyer1.PK;
			ruleSet3.BRS_OH_Supplier = supplier1.PK;
			AssertHasError(ruleSet3.BRS_OH_BuyerInfo, "A Dummy Rule Set for Buyer (Buyer1) and Supplier (Supplier1) already exists.");
			AssertHasError(ruleSet3.BRS_OH_SupplierInfo, "A Dummy Rule Set for Buyer (Buyer1) and Supplier (Supplier1) already exists.");

			ruleSet3.BRS_OH_Supplier = supplier2.PK;
			AssertNoErrors(ruleSet3.BRS_OH_SupplierInfo);
			AssertNoErrors(ruleSet3.BRS_OH_BuyerInfo);

			ruleSet3.BRS_OH_Supplier = supplier1.PK;
			AssertHasError(ruleSet3.BRS_OH_BuyerInfo, "A Dummy Rule Set for Buyer (Buyer1) and Supplier (Supplier1) already exists.");
			AssertHasError(ruleSet3.BRS_OH_SupplierInfo, "A Dummy Rule Set for Buyer (Buyer1) and Supplier (Supplier1) already exists.");

			ruleSet3.BRS_Module = BarcodeModuleTypes.Codes.Warehouse;
			AssertNoErrors(ruleSet3.BRS_ModuleInfo);
			AssertNoErrors(ruleSet3.BRS_OH_SupplierInfo);
			AssertNoErrors(ruleSet3.BRS_OH_BuyerInfo);

			ruleSet3.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertNoErrors(ruleSet3.BRS_ModuleInfo);
			AssertNoErrors(ruleSet3.BRS_OH_BuyerInfo);
			AssertNoErrors(ruleSet3.BRS_OH_SupplierInfo);
		}

		#endregion

		#region TestCheckForDuplicateCombination_WithRelatedEntity

		public void TestCheckForDuplicateCombination_WithRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var buyer2 = Helper.CreateOrg("Buyer2");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var supplier2 = Helper.CreateOrg("Supplier2");
			var part1 = Helper.CreatePart("PART1", buyer1);
			part1.OP_Desc = "PART1";
			var part2 = Helper.CreatePart("PART2", buyer1);
			part2.OP_Desc = "PART2";
			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;
			dummy.RelatedEntityList = new OrgSupplierPartCollection(Factory);

			var ruleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet2 = Helper.CreateRuleSet(buyer1, supplier1, part1);
			Factory.Save();
			AssertNoErrors("Precondition", ruleSet1.BRS_RelatedEntityTableCodeInfo);

			var errorMessage = "A Dummy Rule Set for Buyer (Buyer1), Supplier (Supplier1) and Dummy Entity (PART1) already exists.";
			ruleSet1.BRS_RelatedEntityId = part1.PK;
			AssertHasError(ruleSet1.BRS_ModuleInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_OH_BuyerInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_OH_SupplierInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_RelatedEntityIdInfo, errorMessage);

			ruleSet1.BRS_OH_Supplier = supplier2.PK;
			AssertNoErrors(ruleSet1.BRS_ModuleInfo);
			AssertNoErrors(ruleSet1.BRS_OH_BuyerInfo);
			AssertNoErrors(ruleSet1.BRS_OH_SupplierInfo);
			AssertNoErrors(ruleSet1.BRS_RelatedEntityIdInfo);

			ruleSet1.BRS_OH_Supplier = supplier1.PK;
			AssertHasError(ruleSet1.BRS_ModuleInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_OH_BuyerInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_OH_SupplierInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_RelatedEntityIdInfo, errorMessage);

			ruleSet1.BRS_RelatedEntityId = part2.PK;
			AssertNoErrors(ruleSet1.BRS_ModuleInfo);
			AssertNoErrors(ruleSet1.BRS_OH_BuyerInfo);
			AssertNoErrors(ruleSet1.BRS_OH_SupplierInfo);
			AssertNoErrors(ruleSet1.BRS_RelatedEntityIdInfo);

			ruleSet1.BRS_RelatedEntityId = part1.PK;
			AssertHasError(ruleSet1.BRS_ModuleInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_OH_BuyerInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_OH_SupplierInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_RelatedEntityIdInfo, errorMessage);

			ruleSet1.BRS_OH_Buyer = buyer2.PK;
			AssertNoErrors(ruleSet1.BRS_ModuleInfo);
			AssertNoErrors(ruleSet1.BRS_OH_BuyerInfo);
			AssertNoErrors(ruleSet1.BRS_OH_SupplierInfo);
			AssertNoErrors(ruleSet1.BRS_RelatedEntityIdInfo);

			ruleSet1.BRS_OH_Buyer = buyer1.PK;
			AssertHasError(ruleSet1.BRS_ModuleInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_OH_BuyerInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_OH_SupplierInfo, errorMessage);
			AssertHasError(ruleSet1.BRS_RelatedEntityIdInfo, errorMessage);

			ruleSet1.BRS_Module = BarcodeModuleTypes.Codes.Warehouse;
			AssertNoErrors(ruleSet1.BRS_ModuleInfo);
			AssertNoErrors(ruleSet1.BRS_OH_BuyerInfo);
			AssertNoErrors(ruleSet1.BRS_OH_SupplierInfo);
			AssertNoErrors(ruleSet1.BRS_RelatedEntityIdInfo);
		}

		#endregion

		#region TestValidateBRS_HasAtLeastOneRule

		public void TestValidateBRS_HasAtLeastOneParsingRule()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.Validation.ValidateAll();
			AssertHasRowError(ruleSet, "Barcode Rule Sets require at least One parsing rule or One validation rule.");

			ruleSet.Rules.AddNew();
			ruleSet.Validation.ValidateAll();
			AssertNoRowErrors(ruleSet);
		}

		public void TestValidateBRS_HasAtLeastOneValidationRule()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.Validation.ValidateAll();
			AssertHasRowError(ruleSet, "Barcode Rule Sets require at least One parsing rule or One validation rule.");

			ruleSet.ValidationRules.AddNew();
			ruleSet.Validation.ValidateAll();
			AssertNoRowErrors(ruleSet);
		}

		#endregion

		#region TestRelatedEntityTableCodeConstraint

		public void TestRelatedEntityTableCodeConstraint_Allowed_Empty() => AssertRelatedEntityTableCodeConstraint("", ZGuid.Empty, true);
		public void TestRelatedEntityTableCodeConstraint_Allowed_OP() => AssertRelatedEntityTableCodeConstraint("OP", ZGuid.NewZGuid(), true);
		public void TestRelatedEntityTableCodeConstraint_NotAllowedOtherValues_WD() => AssertRelatedEntityTableCodeConstraint("WD", ZGuid.NewZGuid(), false);
		public void TestRelatedEntityTableCodeConstraint_NotAllowedOtherValues_WTK() => AssertRelatedEntityTableCodeConstraint("WTK", ZGuid.NewZGuid(), false);

		public void AssertRelatedEntityTableCodeConstraint(string relatedEntityTableCode, ZGuid relatedEntityId, bool expectedAllowed)
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "Buyer";
			var barcodeRuleset = Factory.New<BarcodeRuleSet>();
			barcodeRuleset.BRS_RelatedEntityId = relatedEntityId;
			barcodeRuleset.BRS_RelatedEntityTableCode = relatedEntityTableCode;
			barcodeRuleset.BRS_OH_Buyer = buyer.PK;

			if (expectedAllowed)
			{
				AssertNoExceptionThrown(Factory.Save);
			}
			else
			{
				var expectedErrorMsg = "The INSERT statement conflicted with the CHECK constraint \"Constraint_RelatedEntityTableCode\".";
				AssertInnermostException("SqlException should throw", typeof(SqlException), expectedErrorMsg, Factory.Save, true);
			}
		}

		#endregion
	}
}
