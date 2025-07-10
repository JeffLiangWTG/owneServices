
namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeRuleValidationTest : BarcodeParsingValidationTestCase
	{
		#region TestCheckIsPartialRule

		public void TestCheckIsPartialRule()
		{
			var rule = Helper.CreateRule();
			AssertNoErrors("Precondition", rule.IsPartialRuleInfo);

			rule.IsPartialRule = false;
			AssertNoErrors(rule.IsPartialRuleInfo);

			Helper.CreateRuleComponent(rule);
			AssertNoErrors(rule.IsPartialRuleInfo);

			rule.IsPartialRule = true;
			AssertNoErrors(rule.IsPartialRuleInfo);

			rule.IsPartialRule = false;
			AssertNoErrors(rule.IsPartialRuleInfo);

			Helper.CreateRuleComponent(rule);
			AssertNoErrors(rule.IsPartialRuleInfo);

			rule.IsPartialRule = true;
			AssertHasError(rule.IsPartialRuleInfo, "Partial Rules can only have one Component.");

			rule.IsPartialRule = false;
			AssertNoErrors(rule.IsPartialRuleInfo);

			rule.IsPartialRule = true;
			AssertHasError(rule.IsPartialRuleInfo, "Partial Rules can only have one Component.");

			rule.Components.Delete(rule.Components[0]);
			AssertHasError(rule.IsPartialRuleInfo, "Partial Rules can only have one Component.");

			rule.Validation.ValidateIsPartialRule();
			AssertNoErrors(rule.IsPartialRuleInfo);
		}

		#endregion

		#region TestCheckIsGS1AndIsPartialRuleForIsDelimiterMultiComponent

		public void TestCheckIsGS1AndIsPartialRuleForIsDelimiterMultiComponent()
		{
			var rule = Helper.CreateRule();
			var component1 = Helper.CreateRuleComponent(rule);
			var component2 = Helper.CreateRuleComponent(rule);

			// GS1
			component1.IsDelimiterMultiComponent = true;
			AssertEquals("Precondition", false, rule.IsGS1);
			AssertNoErrors("Precondition", component1.IsDelimiterMultiComponentInfo);
			AssertNoErrors("Precondition", component2.IsDelimiterMultiComponentInfo);

			rule.IsGS1 = true;
			AssertHasError(component1.IsDelimiterMultiComponentInfo,
				"GS1 Rule Components should not have Multi-Component delimiters.");
			AssertNoErrors(component2.IsDelimiterMultiComponentInfo);

			// Partial Rule
			rule.IsGS1 = false;
			component1.IsDelimiterMultiComponent = true;
			AssertNoErrors("Precondition", component1.IsDelimiterMultiComponentInfo);
			AssertNoErrors("Precondition", component2.IsDelimiterMultiComponentInfo);
			AssertEquals("Precondition", false, rule.IsPartialRule);

			rule.IsPartialRule = true;
			AssertHasError(component1.IsDelimiterMultiComponentInfo,
				"Partial rule components should not have Multi-Component delimiters.");
			AssertNoErrors(component2.IsDelimiterMultiComponentInfo);
		}

		#endregion

		#region TestCheckBRU_Name

		public void TestCheckBRU_Name()
		{
			var rule = Helper.CreateRule();
			AssertNoErrors("Precondition", rule.BRU_NameInfo);

			rule.BRU_Name = "";
			AssertHasError(rule.BRU_NameInfo, "Please enter a Rule Name.");

			rule.BRU_Name = "Product Barcode";
			AssertNoErrors(rule.BRU_NameInfo);
		}

		#endregion

		#region TestCheckBRU_RuleNumber

		public void TestCheckBRU_RuleNumber()
		{
			var ruleSet = Helper.CreateRuleSet();
			var rule1 = Helper.CreateRule(ruleSet);
			AssertNoErrors("Precondition", rule1.BRU_RuleNumberInfo);

			rule1.BRU_RuleNumber = 0;
			AssertHasError(rule1.BRU_RuleNumberInfo, "Rule Number cannot be zero.");

			rule1.BRU_RuleNumber = -1;
			AssertHasError(rule1.BRU_RuleNumberInfo, "Rule Number cannot be negative.");

			rule1.BRU_RuleNumber = 1;
			AssertNoErrors(rule1.BRU_RuleNumberInfo);

			var rule2 = Helper.CreateRule(ruleSet);
			AssertNoErrors("Precondition", rule2.BRU_RuleNumberInfo);

			rule2.BRU_RuleNumber = 1;
			AssertHasError(rule2.BRU_RuleNumberInfo, "Rule No. must be unique.");
		}

		#endregion

		#region TestCheckBRU_Terminator

		public void TestCheckBRU_Terminator()
		{
			var rule = Helper.CreateRule();
			rule.IsPartialRule = true;
			rule.BRU_Terminator = "1";
			AssertNoErrors("Precondition", rule.BRU_TerminatorInfo);

			var errorMessage = "Enter a Terminator or make all Components (except the last for Full rules) have Fixed Length.";
			rule.BRU_Terminator = "";
			AssertHasError(rule.BRU_TerminatorInfo, errorMessage);

			rule.BRU_Terminator = "1";
			AssertNoErrors(rule.BRU_TerminatorInfo);

			rule.BRU_Terminator = "";
			AssertHasError(rule.BRU_TerminatorInfo, errorMessage);

			Helper.CreateRuleComponent(rule, fixedLength: 1);
			rule.Validation.ValidateBRU_Terminator();
			AssertNoErrors(rule.BRU_TerminatorInfo);
		}

		#endregion

		#region TestCheckBRU_Terminator_WhenSingleComponentOnFullRule

		public void TestCheckBRU_Terminator_WhenSingleComponentOnFullRule()
		{
			var rule = Helper.CreateRule();
			rule.IsPartialRule = true;
			var component1 = Helper.CreateRuleComponent(rule, minLength: 10, maxLength: 15);
			rule.BRU_Terminator = "1";
			AssertNoErrors("Precondition", rule.BRU_TerminatorInfo);

			var errorMessage = "Enter a Terminator or make all Components (except the last for Full rules) have Fixed Length.";
			rule.BRU_Terminator = "";
			AssertHasError(rule.BRU_TerminatorInfo, errorMessage);

			rule.BRU_Terminator = "1";
			AssertNoErrors(rule.BRU_TerminatorInfo);

			rule.BRU_Terminator = "";
			AssertHasError(rule.BRU_TerminatorInfo, errorMessage);

			rule.IsPartialRule = false;
			AssertNoErrors(rule.BRU_TerminatorInfo);

			Helper.CreateRuleComponent(rule, minLength: 5, maxLength: 10);
			rule.Validation.ValidateBRU_Terminator();
			AssertHasError(rule.BRU_TerminatorInfo, errorMessage);

			// set 1st component to have fixed Length
			component1.BRC_MaxLength = 10;
			rule.Validation.ValidateBRU_Terminator();
			AssertNoErrors(rule.BRU_TerminatorInfo);
		}

		#endregion

		#region TestCheckBRU_TerminatorIsWesternEuropean

		public void TestCheckBRU_TerminatorIsWesternEuropean()
		{
			var rule = Helper.CreateRule();
			rule.TerminatorType = TerminatorTypes.Codes.UserDefined;
			AssertNoErrors("Precondition", rule.BRU_TerminatorInfo);

			rule.BRU_Terminator = BarcodeRule.GS1Terminator;
			AssertHasError(rule.BRU_TerminatorInfo, "Terminator only accepts Western European languages characters.");

			rule.BRU_Terminator = "|";
			AssertNoErrors(rule.BRU_TerminatorInfo);

			rule.BRU_Terminator = BarcodeRule.GS1Terminator;
			AssertHasError("Precondition", rule.BRU_TerminatorInfo, "Terminator only accepts Western European languages characters.");
			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			rule.Validation.ValidateBRU_Terminator();
			AssertNoErrors(rule.BRU_TerminatorInfo);
		}

		#endregion

		#region TestCheckTerminatorType

		public void TestCheckTerminatorType()
		{
			var rule = Helper.CreateRule();
			AssertNoErrors("Precondition", rule.TerminatorTypeInfo);

			rule.TerminatorType = "";
			AssertHasError(rule.TerminatorTypeInfo, "Please enter a Terminator Type.");

			rule.TerminatorType = TerminatorTypes.Codes.UserDefined;
			AssertNoErrors(rule.TerminatorTypeInfo);

			rule.TerminatorType = "xXx";
			AssertHasError(rule.TerminatorTypeInfo, "Enter a valid Terminator Type.");

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertNoErrors(rule.TerminatorTypeInfo);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var rule = Helper.CreateRule();
			AssertNoRowErrors("Precondition", rule);

			rule.RunPreSaveValidation();
			AssertHasRowError(rule, "A Rule requires at least one Component.");

			rule.Components.AddNew();
			rule.RunPreSaveValidation();
			AssertNoRowErrors(rule);
		}

		#endregion

		#region TestValidateAll_NonPersistentProperties

		public void TestValidateAll_NonPersistentProperties()
		{
			var rule = Helper.CreateRule();
			Helper.CreateRuleComponent(rule);
			Helper.CreateRuleComponent(rule);

			using (rule.GetValidationSuspender())
			{
				rule.TerminatorType = "XXX";
				rule.IsPartialRule = true;

				AssertNoErrors(rule.TerminatorTypeInfo);
				AssertNoErrors(rule.IsPartialRuleInfo);
			}

			rule.Validation.ValidateAll();
			AssertHasError(rule.TerminatorTypeInfo, "Enter a valid Terminator Type.");
			AssertHasError(rule.IsPartialRuleInfo, "Partial Rules can only have one Component.");
		}

		#endregion
	}
}
