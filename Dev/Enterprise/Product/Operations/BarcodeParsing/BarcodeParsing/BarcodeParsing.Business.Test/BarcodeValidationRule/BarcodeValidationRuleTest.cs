using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine.Warehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(BarcodeValidationRule))]
	class BarcodeValidationRuleTest : BarcodeParsingBusinessObjectTestCase
	{
		#region TestRuleSet

		public void TestRuleSet()
		{
			var rule = Factory.New<BarcodeValidationRule>();
			AssertNull(rule.RuleSet);

			var ruleSet = Helper.CreateRuleSet();
			rule.BVR_BRS_RuleSet = ruleSet.PK;
			AssertEquals(ruleSet, rule.RuleSet);
		}

		#endregion

		#region TestPrefix

		public void TestPrefixStatusWhenDateFormatIsSelected()
		{
			CombineAssertions(() =>
			{
				TestPrefixStatusWhenDateFormatIsSelectedCore(OtherDataFormatTypes.Codes.DDMMYY);
				TestPrefixStatusWhenDateFormatIsSelectedCore(OtherDataFormatTypes.Codes.DDMMYYYY);
				TestPrefixStatusWhenDateFormatIsSelectedCore(OtherDataFormatTypes.Codes.MMDDYY);
				TestPrefixStatusWhenDateFormatIsSelectedCore(OtherDataFormatTypes.Codes.MMDDYYYY);
				TestPrefixStatusWhenDateFormatIsSelectedCore(GS1DataFormatTypes.Codes.YYMMDD);
			});
		}

		void TestPrefixStatusWhenDateFormatIsSelectedCore(ZString dateFormat)
		{
			var rule = Helper.CreateValidationRule();
			rule.BVR_Prefix = "Test";

			rule.BVR_Format = dateFormat;
			AssertEquals(string.Empty, rule.BVR_Prefix);
			AssertEquals(true, rule.BVR_PrefixInfo.ReadOnly);
		}

		public void TestPrefixStatusWhenFormatIsNotDate()
		{
			CombineAssertions(() =>
			{
				TestPrefixStatusWhenFormatIsNotDateCore(GS1DataFormatTypes.Codes.AlphaNumericWithSymbols);
				TestPrefixStatusWhenFormatIsNotDateCore(GS1DataFormatTypes.Codes.AlphaNumericWithSpace);
				TestPrefixStatusWhenFormatIsNotDateCore(GS1DataFormatTypes.Codes.AlphaNumeric);
				TestPrefixStatusWhenFormatIsNotDateCore(GS1DataFormatTypes.Codes.AlphaWithSpace);
				TestPrefixStatusWhenFormatIsNotDateCore(GS1DataFormatTypes.Codes.Alpha);
				TestPrefixStatusWhenFormatIsNotDateCore(GS1DataFormatTypes.Codes.Numeric);
				TestPrefixStatusWhenFormatIsNotDateCore(GS1DataFormatTypes.Codes.Digit);
			});
		}

		void TestPrefixStatusWhenFormatIsNotDateCore(ZString nonDateFormat)
		{
			var rule = Helper.CreateValidationRule();
			rule.BVR_Prefix = "Test";

			rule.BVR_Format = nonDateFormat;
			AssertEquals("Test", rule.BVR_Prefix);
			AssertEquals(false, rule.BVR_PrefixInfo.ReadOnly);
		}

		#endregion

		#region TestDateFormatSetsMinLengthAndMaxLengthAutomatically

		public void TestDateFormatSetsMinLengthAndMaxLengthAutomatically()
		{
			var rule = Helper.CreateValidationRule();

			rule.BVR_Format = GS1DataFormatTypes.Codes.Alpha;
			AssertEquals((short)0, rule.BVR_MaxLength);
			AssertEquals((short)0, rule.BVR_MinLength);
			AssertEquals(false, rule.BVR_MaxLengthInfo.ReadOnly);
			AssertEquals(false, rule.BVR_MinLengthInfo.ReadOnly);
			AssertEquals(false, rule.LengthTypeForBindingInfo.ReadOnly);

			rule.BVR_Format = OtherDataFormatTypes.Codes.MMDDYY;
			AssertEquals((short)6, rule.BVR_MaxLength);
			AssertEquals((short)6, rule.BVR_MinLength);
			AssertEquals(true, rule.BVR_MaxLengthInfo.ReadOnly);
			AssertEquals(true, rule.BVR_MinLengthInfo.ReadOnly);
			AssertEquals(true, rule.LengthTypeForBindingInfo.ReadOnly);

			rule.BVR_Format = OtherDataFormatTypes.Codes.MMDDYYYY;
			AssertEquals((short)8, rule.BVR_MaxLength);
			AssertEquals((short)8, rule.BVR_MinLength);
			AssertEquals(true, rule.BVR_MaxLengthInfo.ReadOnly);
			AssertEquals(true, rule.BVR_MinLengthInfo.ReadOnly);
			AssertEquals(true, rule.LengthTypeForBindingInfo.ReadOnly);
		}

		#endregion

		#region TestLengthTypeForBinding

		public void TestLengthTypeForBinding()
		{
			var rule = Helper.CreateValidationRule();

			AssertEquals("Validate rule should have Length Type of Range by default.", LengthTypes.Descriptions.Range, rule.LengthTypeForBinding);
			AssertEquals(false, rule.BVR_MinLengthInfo.ReadOnly);
			AssertEquals(false, rule.BVR_MaxLengthInfo.ReadOnly);

			rule.BVR_MaxLength = 5;
			AssertEquals("Precondition", (short)0, rule.BVR_MinLength);

			rule.LengthTypeForBinding = LengthTypes.Codes.Fixed;
			AssertEquals("Setting LengthTypeForBinding should make the getter return the correct description.",
				LengthTypes.Descriptions.Fixed, rule.LengthTypeForBinding);
			AssertEquals((short)5, rule.BVR_MinLength);

			rule.BVR_MaxLength = 8;
			AssertEquals((short)8, rule.BVR_MinLength);

			rule.BVR_MinLength = 4;
			AssertEquals((short)4, rule.BVR_MaxLength);

			rule.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertEquals("Setting LengthTypeForBinding should make the getter return the correct description.",
				LengthTypes.Descriptions.Any, rule.LengthTypeForBinding);
			AssertEquals((short)0, rule.BVR_MaxLength);
			AssertEquals((short)0, rule.BVR_MinLength);
			AssertEquals(true, rule.BVR_MinLengthInfo.ReadOnly);
			AssertEquals(true, rule.BVR_MaxLengthInfo.ReadOnly);
		}

		#endregion

		#region TestLengthTypeForBinding_DoesNotAcceptInvalidInput

		public void TestLengthTypeForBinding_DoesNotAcceptInvalidInput()
		{
			var rule = Helper.CreateValidationRule();

			AssertEquals("Validate rule should have Length Type of Range by default.", true, rule.IsLengthTypeRange());
			AssertEquals("Validate rule should have Length Type of Range by default.", LengthTypes.Descriptions.Range, rule.LengthTypeForBinding);

			rule.LengthTypeForBinding = "X";
			AssertEquals("Invalid input should be ignored.", true, rule.IsLengthTypeRange());
			AssertEquals("Invalid input should be ignored.", LengthTypes.Descriptions.Range, rule.LengthTypeForBinding);

			rule.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertEquals("Invalid input should be ignored.", true, rule.IsLengthTypeAny());
			AssertEquals("Invalid input should be ignored.", LengthTypes.Descriptions.Any, rule.LengthTypeForBinding);
		}

		#endregion

		#region TestLengthTypeForBinding_GetLengthTypeBasedOnMinAndMaxLength

		public void TestLengthTypeForBinding_GetLengthTypeBasedOnMinAndMaxLength()
		{
			var ruleSet = Helper.CreateRuleSet();
			Helper.CreateValidationRule(ruleSet, 1, 1, nameof(WarehouseTargetField.PA1), GS1DataFormatTypes.Codes.AlphaNumeric);
			Helper.CreateValidationRule(ruleSet, 0, 0, nameof(WarehouseTargetField.PA2), GS1DataFormatTypes.Codes.AlphaNumeric);
			Helper.CreateValidationRule(ruleSet, 1, 2, nameof(WarehouseTargetField.PA3), GS1DataFormatTypes.Codes.AlphaNumeric);
			Factory.Save();

			var ruleInOtherFactory = Factory.Load<BarcodeValidationRule>(new ZQuery(BarcodeValidationRuleSchema.BVR_BRS_RuleSet, ruleSet.PK));
			var r1 = ruleInOtherFactory.AsEnumerable().Single(c => c.BVR_MinLength == 1 && c.BVR_MaxLength == 1);
			var r2 = ruleInOtherFactory.AsEnumerable().Single(c => c.BVR_MinLength == 0 && c.BVR_MaxLength == 0);
			var r3 = ruleInOtherFactory.AsEnumerable().Single(c => c.BVR_MinLength == 1 && c.BVR_MaxLength == 2);

			AssertEquals(LengthTypes.Descriptions.Fixed, r1.LengthTypeForBinding);
			AssertEquals(LengthTypes.Descriptions.Any, r2.LengthTypeForBinding);
			AssertEquals(LengthTypes.Descriptions.Range, r3.LengthTypeForBinding);
		}

		#endregion

		#region TestTargetFieldDescription

		public void TestTargetFieldDescription()
		{
			var rule = Helper.CreateValidationRule();
			rule.BVR_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertEquals(DummyTargetFields.Descriptions.TargetField1, rule.TargetFieldDescription);
		}

		#endregion

		#region TestModule

		public void TestModule()
		{
			var validationRule = Factory.New<BarcodeValidationRule>();
			AssertEquals("", validationRule.Module);

			var ruleSet = Helper.CreateRuleSet();
			validationRule.BVR_BRS_RuleSet = ruleSet.PK;
			AssertEquals("DUM", validationRule.Module);
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			var rule = Helper.CreateValidationRule();
			AssertEquals("RuleSet is not system defined, should be editable.", false, rule.ReadOnly);

			rule.RuleSet.BRS_IsSystem = true;
			AssertEquals("RuleSet is system defined, should be read-only.", true, rule.ReadOnly);
		}

		#endregion

		#region Flags

		#region TestIsLengthTypeAny

		public void TestIsLengthTypeAny()
		{
			var rule = Helper.CreateValidationRule();
			AssertEquals("Validation rule should have Length Type of Range by default.", true, rule.IsLengthTypeRange());
			AssertEquals("Validation rule should have Length Type of Range by default.", LengthTypes.Descriptions.Range, rule.LengthTypeForBinding);
			AssertEquals(false, rule.IsLengthTypeAny());

			rule.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertEquals(true, rule.IsLengthTypeAny());
		}

		#endregion

		#region TestIsLengthTypeRange

		public void TestIsLengthTypeRange()
		{
			var rule = Helper.CreateValidationRule();
			AssertEquals("Validation rule should have Length Type of Range by default.", true, rule.IsLengthTypeRange());
			AssertEquals("Validation rule should have Length Type of Range by default.", LengthTypes.Descriptions.Range, rule.LengthTypeForBinding);

			rule.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertEquals(false, rule.IsLengthTypeRange());
		}

		#endregion

		#endregion

		#region TestCloningSupport

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Prevent change to base class as we would lose context of the actual schema class")]
		public void TestCloningSupport()
		{
			var rule = Helper.CreateValidationRule();
			rule.BVR_MinLength = 5;
			rule.BVR_MaxLength = 13;
			rule.LengthTypeForBinding = LengthTypes.Codes.Range;
			Assert("BarcodeValidationRule should support cloninig", rule.SupportsClone());

			var clone = (BarcodeValidationRule)rule.Clone();
			AssertNotEquals("Should not clone foreign key to rule set.", rule.BVR_BRS_RuleSet, clone.BVR_BRS_RuleSet);
			AssertEquals("Foreign key for cloned item should be empty", ZGuid.Empty, clone.BVR_BRS_RuleSet);
			AssertEquals(BarcodeValidationRule.Schema.BVR_MinLength, clone.BVR_MinLength, rule.BVR_MinLength);
			AssertEquals(BarcodeValidationRule.Schema.BVR_MaxLength, clone.BVR_MaxLength, rule.BVR_MaxLength);
			AssertEquals(BarcodeValidationRule.Schema.LengthTypeForBinding, clone.LengthTypeForBinding, rule.LengthTypeForBinding);
		}

		#endregion

		#region LoadMatchingRules

		#region TestLoadMatchingRules_NoBuyerSupplierRelatedEntity

		public void TestLoadMatchingRules_NoBuyerSupplierRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched1 = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched1.Select(r => r.PK));

			var rulesMatchedWithCode1 = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", "", ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchedWithCode1.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_Buyer_NoSupplierRelatedEntity

		public void TestLoadMatchingRules_Buyer_NoSupplierRelatedEntity_MatchRuleWithSameBuyer()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, ZGuid.Empty, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, "", ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplier.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_Buyer_NoSupplierRelatedEntity_MatchRuleWithNoBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, ZGuid.Empty, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, "", ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_Supplier_NoBuyerRelatedEntity

		public void TestLoadMatchingRules_Supplier_NoBuyerRelatedEntity_MatchRuleWithSameSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, supplier1.PK, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", supplier1.OH_Code, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_Supplier_NoBuyerRelatedEntity_MatchRuleWithNoBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, supplier1.PK, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", supplier1.OH_Code, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_RelatedEntity_NoBuyerSupplier_RelatedEntityMustHaveBuyer

		public void TestLoadMatchingRules_RelatedEntity_NoBuyerSupplier_RelatedEntityMustHaveBuyer()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;
			dummy.RelatedEntityRequirements = RelatedEntityRequirements.MustHaveBuyer;

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, ZGuid.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, string.Empty, string.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_RelatedEntity_NoBuyerSupplier_BuyerIsNotMustHave

		public void TestLoadMatchingRules_RelatedEntity_NoBuyerSupplier_BuyerIsNotMustHave_MatchRuleWithSameRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, ZGuid.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoSupplierNoBuyerPart1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, string.Empty, string.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoSupplierNoBuyerPart1.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_RelatedEntity_NoBuyerSupplier_BuyerIsNotMustHave_MatchRuleWithNoBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, ZGuid.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, string.Empty, string.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_BuyerRelatedEntity_NoSupplier

		public void TestLoadMatchingRules_BuyerRelatedEntity_NoSupplier_MatchRuleWithSameBuyerRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, ZGuid.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplierPart1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, string.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplierPart1.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerRelatedEntity_NoSupplier_MatchRuleWithSameBuyer()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, ZGuid.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, string.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplier.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerRelatedEntity_NoSupplier_MatchRuleWithSameRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, ZGuid.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoSupplierNoBuyerPart1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, string.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoSupplierNoBuyerPart1.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerRelatedEntity_NoSupplier_MatchRuleWithNoBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, ZGuid.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, string.Empty, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_BuyerSupplier_NoRelatedEntity

		public void TestLoadMatchingRules_BuyerSupplier_NoRelatedEntity_MatchRuleWithSameBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1Supplier1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1Supplier1.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplier_NoRelatedEntity_MatchRuleWithSameBuyer()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplier.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplier_NoRelatedEntity_MatchRuleWithSameSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplier_NoRelatedEntity_MatchRuleWithNoBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_SupplierRelatedEntity_NoBuyer_RelatedEntityMustHaveBuyer

		public void TestLoadMatchingRules_SupplierRelatedEntity_NoBuyer_RelatedEntityMustHaveBuyer()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;
			dummy.RelatedEntityRequirements = RelatedEntityRequirements.MustHaveBuyer;

			// Should match rules where the supplier is the same *only*. Since buyer is not provided, we ignore related entity
			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, string.Empty, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_SupplierRelatedEntity_NoBuyer_BuyerIsNotMustHave

		public void TestLoadMatchingRules_SupplierRelatedEntity_NoBuyer_BuyerIsNotMustHave_MatchRuleWithSameSupplierRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyerPart1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, string.Empty, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyerPart1.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_SupplierRelatedEntity_NoBuyer_BuyerIsNotMustHave_MatchRuleWithSameSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_SupplierRelatedEntity_NoBuyer_BuyerIsNotMustHave_MatchRuleWithSameRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet6 = Helper.CreateRuleSet();
			var ruleSet15 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet13 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet2 = Helper.CreateRuleSet(buyer1);
			var ruleSet9 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet1 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet15, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet9, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched7 = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoSupplierNoBuyerPart1.PK }, rulesMatched7.Select(r => r.PK));

			var rulesMatchedWithCode7 = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoSupplierNoBuyerPart1.PK }, rulesMatchedWithCode7.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_SupplierRelatedEntity_NoBuyer_BuyerIsNotMustHave_MatchRuleWithNoBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, ZGuid.Empty, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchedWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, "", supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchedWithCode.Select(r => r.PK));
		}

		#endregion

		#region TestLoadMatchingRules_BuyerSupplierRelatedEntity

		public void TestLoadMatchingRules_BuyerSupplierRelatedEntity_MatchRuleWithSameBuyerSupplierRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);
			var ruleSet8 = Helper.CreateRuleSet(buyer1, supplier1, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1Part1 = Helper.CreateValidationRule(ruleSet8, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1Supplier1Part1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1Supplier1Part1.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplierRelatedEntity_MatchRuleWithSameBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);
			var ruleSet7 = Helper.CreateRuleSet(buyer1, supplier1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1Supplier1 = Helper.CreateValidationRule(ruleSet7, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1Supplier1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1Supplier1.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplierRelatedEntity_MatchRuleWithSameBuyerRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);
			var ruleSet6 = Helper.CreateRuleSet(buyer1, null, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplierPart1 = Helper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplierPart1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplierPart1.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplierRelatedEntity_MatchRuleWithSameSupplierRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet4 = Helper.CreateRuleSet(null, supplier1, part1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyerPart1 = Helper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyerPart1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyerPart1.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplierRelatedEntity_MatchRuleWithSameSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);
			var ruleSet5 = Helper.CreateRuleSet(buyer1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			var ruleWithBuyer1NoSupplier = Helper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithBuyer1NoSupplier.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplierRelatedEntity_MatchRuleWithSameBuyer()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);
			var ruleSet3 = Helper.CreateRuleSet(null, supplier1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			var ruleWithSupplier1NoBuyer = Helper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithSupplier1NoBuyer.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplierRelatedEntity_MatchRuleWithSameRelatedEntity()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);

			var ruleSet1 = Helper.CreateRuleSet();
			var ruleSet2 = Helper.CreateRuleSet(null, null, part1);

			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var ruleWithNoSupplierNoBuyerPart1 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoSupplierNoBuyerPart1.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoSupplierNoBuyerPart1.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		public void TestLoadMatchingRules_BuyerSupplierRelatedEntity_MatchRuleWithNoBuyerSupplier()
		{
			var buyer1 = Helper.CreateOrg("Buyer1");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var part1 = Helper.CreatePart("P1", buyer1);
			var ruleSet1 = Helper.CreateRuleSet();
			var ruleWithNoBuyerNoSupplier = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var rulesMatched = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.PK, supplier1.PK, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatched.Select(r => r.PK));

			var rulesMatchWithCode = BarcodeValidationRule.LoadMatchingRules(Factory, DummyBarcodeParsingConsumer.Module, buyer1.OH_Code, supplier1.OH_Code, part1.PK);
			AssertContainsExactElementsInAnyOrder(new[] { ruleWithNoBuyerNoSupplier.PK }, rulesMatchWithCode.Select(r => r.PK));
		}

		#endregion

		#endregion
	}
}
