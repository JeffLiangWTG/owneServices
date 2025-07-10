using System.Linq;
using CargoWise.Integration;
using Enterprise.BarcodeParsingEngine;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeRuleComponentValidationTest : BarcodeParsingValidationTestCase
	{
		#region TestCheckBRC_ApplicationID

		public void TestCheckBRC_ApplicationID()
		{
			var rule = Helper.CreateRule();
			rule.IsPartialRule = false;
			var component = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component.BRC_ApplicationIDInfo);

			component.BRC_ApplicationID = "";
			AssertNoErrors(component.BRC_ApplicationIDInfo);

			rule.IsPartialRule = true;
			AssertHasError(component.BRC_ApplicationIDInfo, "Application ID must be entered for Partial Rules.");

			component.BRC_ApplicationID = "00";
			AssertNoErrors(component.BRC_ApplicationIDInfo);

			component.BRC_ApplicationID = "";
			AssertHasError(component.BRC_ApplicationIDInfo, "Application ID must be entered for Partial Rules.");
		}

		#endregion

		#region TestCheckBRC_ApplicationID_UseIDAlreadyUsedByAnotherPartialRule

		public void TestCheckBRC_ApplicationID_UseIDAlreadyUsedByAnotherPartialRule()
		{
			var ruleSet = Helper.CreateRuleSet();
			var fullRule = Helper.CreateRule(ruleSet);
			var partialRule1 = Helper.CreateRule(ruleSet);
			var partialRule2 = Helper.CreateRule(ruleSet);
			partialRule1.IsPartialRule = true;
			partialRule2.IsPartialRule = true;
			fullRule.IsPartialRule = false;
			partialRule1.BRU_Name = "Partial 1";
			partialRule2.BRU_Name = "Partial 2";
			fullRule.BRU_Name = "Full";

			var fullRuleComponent = Helper.CreateRuleComponent(fullRule);
			fullRuleComponent.BRC_ApplicationID = "(A)";

			var rule1Component = Helper.CreateRuleComponent(partialRule1);
			rule1Component.BRC_ApplicationID = "(P)";
			rule1Component.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(rule1Component.BRC_ApplicationIDInfo);

			var rule2Component = Helper.CreateRuleComponent(partialRule2);
			AssertNoErrors("Precondition", rule2Component.BRC_ApplicationIDInfo);

			rule2Component.BRC_ApplicationID = "(P)";
			rule2Component.BRC_TargetField = DummyTargetFields.Codes.TargetField2;
			AssertHasWarning(rule2Component.BRC_ApplicationIDInfo, "This Application Identifier is already in use by Partial Rule 'Partial 1' which Targets 'Target 1'.");

			rule2Component.BRC_ApplicationID = "(Q)";
			AssertNoErrors(rule2Component.BRC_ApplicationIDInfo);

			rule2Component.BRC_ApplicationID = "(A)";
			AssertNoErrors(rule2Component.BRC_ApplicationIDInfo);
		}

		public void TestCheckBRC_ApplicationID_UseIDAlreadyUsedByAnotherPartialRule_ValidateWhenSameTargetField()
		{
			var ruleSet = Helper.CreateRuleSet();
			var partialRule1 = Helper.CreateRule(ruleSet);
			var partialRule2 = Helper.CreateRule(ruleSet);
			partialRule1.IsPartialRule = true;
			partialRule2.IsPartialRule = true;
			partialRule1.BRU_Name = "Partial 1";
			partialRule2.BRU_Name = "Partial 2";

			var rule1Component = Helper.CreateRuleComponent(partialRule1);
			rule1Component.BRC_ApplicationID = "(P)";
			rule1Component.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(rule1Component.BRC_ApplicationIDInfo);

			var rule2Component = Helper.CreateRuleComponent(partialRule2);
			AssertNoErrors("Precondition", rule2Component.BRC_ApplicationIDInfo);

			rule2Component.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			rule2Component.BRC_ApplicationID = "(P)";
			AssertHasError(rule2Component.BRC_ApplicationIDInfo, "This Application Identifier is already in use by Partial Rule 'Partial 1' which also Targets 'Target 1'.");
		}

		public void TestCheckBRC_ApplicationID_UseIDAlreadyUsedByAnotherPartialRule_ValidateWhenDifferentFormatting()
		{
			var ruleSet = Helper.CreateRuleSet();
			var partialRule1 = Helper.CreateRule(ruleSet);
			var partialRule2 = Helper.CreateRule(ruleSet);
			partialRule1.IsPartialRule = true;
			partialRule2.IsPartialRule = true;
			partialRule1.BRU_Name = "Partial 1";
			partialRule2.BRU_Name = "Partial 2";

			var rule1Component = Helper.CreateRuleComponent(partialRule1);
			rule1Component.BRC_ApplicationID = "(P)";
			rule1Component.BRC_Format = "AN";
			rule1Component.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(rule1Component.BRC_ApplicationIDInfo);

			var rule2Component = Helper.CreateRuleComponent(partialRule2);
			AssertNoErrors("Precondition", rule2Component.BRC_ApplicationIDInfo);

			rule2Component.BRC_Format = "AS";
			rule2Component.BRC_TargetField = DummyTargetFields.Codes.TargetField2;
			rule2Component.BRC_ApplicationID = "(P)";
			AssertHasError(rule2Component.BRC_ApplicationIDInfo, "This Application Identifier is already in use by Partial Rule 'Partial 1' whose formatting is not consistent with this rule.");
		}

		#endregion

		#region TestCheckBRC_ApplicationID_WhenBoundToList

		public void TestCheckBRC_ApplicationID_WhenBoundToList()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component.BRC_ApplicationIDInfo);

			component.BRC_ApplicationID = "(P)";
			AssertNoErrors(component.BRC_ApplicationIDInfo);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertHasError(component.BRC_ApplicationIDInfo, "Enter a valid Application Identifier.");

			component.BRC_ApplicationID = "00";
			AssertNoErrors(component.BRC_ApplicationIDInfo);

			component.BRC_ApplicationID = "XX";
			AssertHasError(component.BRC_ApplicationIDInfo, "Enter a valid Application Identifier.");

			component.BRC_ApplicationID = "00";
			AssertNoErrors(component.BRC_ApplicationIDInfo);
		}

		#endregion

		#region TestCheckBRC_Format

		public void TestCheckBRC_Format()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component.BRC_FormatInfo);

			component.BRC_Format = "";
			AssertHasError(component.BRC_FormatInfo, "Please enter a Format.");

			component.BRC_Format = "N";
			AssertNoErrors(component.BRC_FormatInfo);

			component.BRC_Format = "XX";
			AssertHasError(component.BRC_FormatInfo, "Enter a valid Format.");
		}

		#endregion

		#region TestCheckBRC_FormatIsValidForTargetField

		public void TestCheckBRC_FormatIsValidForTargetField()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = "DUM"; // Dummy
			var rule = Helper.CreateRule(ruleSet);
			var component = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component.BRC_FormatInfo);
			var dummy = (DummyBarcodeParsingConsumer)Factory.GetBarcodeParsingConsumerFromModuleCode("DUM");
			dummy.SetFormatsForTargetField(DummyTargetFields.Codes.TargetField1, new[] { FormatType.N });
			dummy.SetFormatsForTargetField(DummyTargetFields.Codes.TargetField2, new[] { FormatType.AS, FormatType.ANY });

			component.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			foreach (ICodeDescription pair in new GS1DataFormatTypes().ToArray().Union(new OtherDataFormatTypes().ToArray()))
			{
				component.BRC_Format = pair.Code;
				if (pair.Code != GS1DataFormatTypes.Codes.Numeric)
				{
					AssertHasError(component.BRC_FormatInfo, $"Format for field 'Target 1' should be {FormatType.N}");
				}
				else
				{
					AssertNoErrors(component.BRC_FormatInfo);
				}
			}

			component.BRC_TargetField = DummyTargetFields.Codes.TargetField2;
			foreach (ICodeDescription pair in new GS1DataFormatTypes().ToArray().Union(new OtherDataFormatTypes().ToArray()))
			{
				component.BRC_Format = pair.Code;
				if (pair.Code != GS1DataFormatTypes.Codes.AlphaWithSpace && pair.Code != GS1DataFormatTypes.Codes.AlphaNumericWithSymbols)
				{
					AssertHasError(component.BRC_FormatInfo, $"Format for field 'Target 2' should be one of: {FormatType.AS}, {FormatType.ANY}");
				}
				else
				{
					AssertNoErrors(component.BRC_FormatInfo);
				}
			}
		}

		#endregion

		#region TestCheckBRC_MaxLength

		public void TestCheckBRC_MaxLength()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("Precondition", true, component.IsLengthTypeRange());
			AssertEquals("Precondition", (short)0, component.BRC_MaxLength);
			AssertHasError(component.BRC_MaxLengthInfo, "Min and Max Length should be greater than zero if Length Type is not Any.");

			component.BRC_MaxLength = 10;
			AssertHasError(component.BRC_MaxLengthInfo, "Min Length should be greater than zero if Max Length is greater than zero.");

			component.BRC_MinLength = 10;
			AssertHasError(component.BRC_MaxLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");

			component.BRC_MinLength = 9;
			AssertNoErrors(component.BRC_MaxLengthInfo);

			component.BRC_MaxLength = 8;
			AssertHasError(component.BRC_MaxLengthInfo, "Max Length cannot be less than Min Length.");

			component.BRC_MinLength = -1;
			AssertHasError(component.BRC_MinLengthInfo, "Min Length cannot be negative.");
			AssertHasError(component.BRC_MaxLengthInfo, "Min Length should be greater than zero if Max Length is greater than zero.");

			component.BRC_MaxLength = -1;
			AssertHasError(component.BRC_MaxLengthInfo, "Max Length cannot be negative.");
		}

		#endregion

		#region TestCheckBRC_MinLength

		public void TestCheckBRC_MinLength()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("Precondition", true, component.IsLengthTypeRange());
			AssertEquals("Precondition", (short)0, component.BRC_MinLength);
			AssertHasError(component.BRC_MinLengthInfo, "Min and Max Length should be greater than zero if Length Type is not Any.");

			component.BRC_MinLength = 2;
			AssertHasError(component.BRC_MinLengthInfo, "Min Length cannot be greater than Max Length.");

			component.BRC_MaxLength = 2;
			AssertHasError(component.BRC_MinLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");

			component.BRC_MinLength = 1;
			AssertNoErrors(component.BRC_MinLengthInfo);

			component.BRC_MinLength = -1;
			AssertHasError(component.BRC_MinLengthInfo, "Min Length cannot be negative.");

			component.BRC_MinLength = 0;
			AssertHasError(component.BRC_MinLengthInfo, "Min Length should be greater than zero if Max Length is greater than zero.");

			component.BRC_MinLength = 1;
			AssertNoErrors(component.BRC_MinLengthInfo);
		}

		#endregion

		#region TestCheckMinLengthAndMaxLengthGreaterThanZeroWhenLengthTypeIsNOTAny

		public void TestCheckMinLengthAndMaxLengthGreaterThanZeroWhenLengthTypeIsNOTAny()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("Precondition", true, component.IsLengthTypeRange());

			AssertHasError(component.BRC_MinLengthInfo, "Min and Max Length should be greater than zero if Length Type is not Any.");
			AssertHasError(component.BRC_MaxLengthInfo, "Min and Max Length should be greater than zero if Length Type is not Any.");

			component.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertNoErrors("LengthType is Any, should have no error.", component.BRC_MinLengthInfo);
			AssertNoErrors("LengthType is Any, should have no error.", component.BRC_MaxLengthInfo);
		}

		#endregion

		#region TestChangingLengthTypeWillValidateMinMaxLength

		public void TestChangingLengthTypeWillValidateMinMaxLength()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertEquals("Precondition", true, component.IsLengthTypeRange());

			component.BRC_MaxLength = 8;
			component.BRC_MinLength = 8;
			AssertHasError(component.BRC_MinLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");
			AssertHasError(component.BRC_MaxLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");

			component.LengthTypeForBinding = LengthTypes.Codes.Fixed;
			AssertNoErrors("LengthType is Fixed, should has no error.", component.BRC_MaxLengthInfo);
			AssertNoErrors("LengthType is Fixed, should has no error.", component.BRC_MinLengthInfo);

			component.LengthTypeForBinding = LengthTypes.Codes.Range;
			AssertHasError(component.BRC_MinLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");
			AssertHasError(component.BRC_MaxLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");
		}

		#endregion

		#region TestCheckBRC_Sequence

		public void TestCheckBRC_Sequence()
		{
			var rule = Helper.CreateRule();
			var component1 = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component1.BRC_SequenceInfo);

			component1.BRC_Sequence = 0;
			AssertHasError(component1.BRC_SequenceInfo, "A full barcode rule must have its components sequenced in order starting from 1.");

			component1.BRC_Sequence = 1;
			AssertNoErrors(component1.BRC_SequenceInfo);

			var component2 = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component2.BRC_SequenceInfo);

			component2.BRC_Sequence = 1;
			AssertHasError(component2.BRC_SequenceInfo, "Sequence no. must be unique.");

			component2.BRC_Sequence = 2;
			AssertNoErrors(component2.BRC_SequenceInfo);

			rule.IsPartialRule = true;
			component1.BRC_Sequence = 1;
			AssertHasError(component1.BRC_SequenceInfo, "Sequence no. should not be entered for partial rules.");
		}

		#endregion

		#region TestCheckBRC_TargetField

		public void TestCheckBRC_TargetField()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component.BRC_TargetFieldInfo);

			component.BRC_TargetField = "";
			AssertHasError(component.BRC_TargetFieldInfo, "Please enter a Target Field.");

			component.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(component.BRC_TargetFieldInfo);

			component.BRC_TargetField = "XXX";
			AssertHasError(component.BRC_TargetFieldInfo, "Enter a valid Target Field.");
		}

		#endregion

		#region TestCheckBRC_TargetField_IsNotAlreadySpecifiedInThisRule

		public void TestCheckBRC_TargetField_IsNotAlreadySpecifiedInThisRule()
		{
			var rule = Helper.CreateRule();
			rule.IsPartialRule = false;
			var component1 = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component1.BRC_TargetFieldInfo);

			component1.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(component1.BRC_TargetFieldInfo);

			var component2 = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component2.BRC_TargetFieldInfo);

			component2.BRC_TargetField = DummyTargetFields.Codes.TargetField2;
			AssertNoErrors(component2.BRC_TargetFieldInfo);

			component2.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertHasError(component2.BRC_TargetFieldInfo, "The Target Field 'TF1' cannot be specified more than once per Rule.");

			component2.BRC_TargetField = DummyTargetFields.Codes.TargetField2;
			AssertNoErrors(component2.BRC_TargetFieldInfo);

			component2.BRC_TargetField = "IGN";
			AssertNoErrors(component2.BRC_TargetFieldInfo);

			var component3 = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component3.BRC_TargetFieldInfo);

			// Ignore is allowed to be specified multiple times
			component3.BRC_TargetField = "IGN";
			AssertNoErrors(component3.BRC_TargetFieldInfo);
		}

		#endregion

		#region TestCheckBRC_TargetField_Ignore

		public void TestCheckBRC_TargetField_Ignore()
		{
			var rule = Helper.CreateRule();
			rule.IsPartialRule = false;

			var component1 = Helper.CreateRuleComponent(rule);
			AssertNoErrors("Precondition", component1.BRC_TargetFieldInfo);

			component1.BRC_TargetField = "IGN";
			AssertHasError(component1.BRC_TargetFieldInfo, "At least one Component must specify a Field to Target.");

			component1.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(component1.BRC_TargetFieldInfo);

			var component2 = Helper.CreateRuleComponent(rule);
			component1.BRC_TargetField = "IGN";
			AssertNoErrors(component1.BRC_TargetFieldInfo);

			component1.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(component1.BRC_TargetFieldInfo);

			rule.Components.Delete(component2);
			rule.IsPartialRule = true;
			AssertNoErrors(component1.BRC_TargetFieldInfo);

			component1.BRC_TargetField = "IGN";
			AssertHasError(component1.BRC_TargetFieldInfo, "Partial Rule Components must specify a Field to Target.");

			component1.BRC_TargetField = DummyTargetFields.Codes.TargetField2;
			AssertNoErrors(component1.BRC_TargetFieldInfo);
		}

		#endregion

		#region TestCheckHasDelimiterIsEmptyForGS1Rule

		public void TestCheckHasDelimiterIsEmptyForGS1Rule()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			rule.IsGS1 = false;
			component.HasDelimiter = true;
			AssertNoErrors(component.HasDelimiterInfo);

			rule.IsGS1 = true;
			AssertHasError(component.HasDelimiterInfo, "GS1 rule should not have a delimiter specified.");

			component.HasDelimiter = false;
			AssertNoErrors(component.HasDelimiterInfo);

			component.HasDelimiter = true;
			AssertHasError(component.HasDelimiterInfo, "GS1 rule should not have a delimiter specified.");
		}

		#endregion

		#region TestComponentDelimiterIsNotRuleDelimiter

		public void TestComponentDelimiterIsNotRuleDelimiter()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			rule.BRU_Terminator = "7";
			component.BRC_Delimiter = (byte)'8';
			component.Validation.ValidateBRC_Delimiter();
			AssertNoErrors(component.DelimiterForBindingInfo);

			component.BRC_Delimiter = (byte)'7';
			component.Validation.ValidateBRC_Delimiter();
			AssertHasError(component.DelimiterForBindingInfo, "Component delimiter should not be same as rule terminator.");
		}

		#endregion

		#region TestCheckHasDelimiterWhenDelimiterMultiComponentIsSelected

		public void TestCheckHasDelimiterWhenDelimiterMultiComponentIsSelected()
		{
			var rule = Helper.CreateRule();
			var component1 = Helper.CreateRuleComponent(rule);
			var component2 = Helper.CreateRuleComponent(rule);
			var component3 = Helper.CreateRuleComponent(rule);
			component1.HasDelimiter = true;
			component1.IsDelimiterMultiComponent = true;
			component2.HasDelimiter = true;
			AssertNoErrors(component1.HasDelimiterInfo);
			AssertHasError(component2.HasDelimiterInfo,
				"Only one component can have a delimiter when using a Multi-Component delimiter.");

			component2.IsDelimiterMultiComponent = false;
			component2.HasDelimiter = false;
			component2.HasDelimiter = true; // Fire validation
			AssertEquals("Precondition", false, rule.BRU_IsDelimiterMultiComponent);
			AssertNoErrors(component1.HasDelimiterInfo);
			AssertNoErrors(component2.HasDelimiterInfo);
		}

		#endregion

		#region TestCheckHasDelimiterIsEmptyForGS1TerminatorType

		public void TestCheckHasDelimiterIsEmptyForGS1TerminatorType()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			rule.TerminatorType = TerminatorTypes.Codes.UserDefined;
			component.HasDelimiter = true;
			AssertNoErrors(component.HasDelimiterInfo);

			rule.TerminatorType = TerminatorTypes.Codes.GS1;
			AssertHasError(component.HasDelimiterInfo, "GS1 rule should not have a delimiter specified.");

			component.HasDelimiter = false;
			AssertNoErrors(component.HasDelimiterInfo);

			component.HasDelimiter = true;
			AssertHasError(component.HasDelimiterInfo, "GS1 rule should not have a delimiter specified.");
		}

		#endregion

		#region TestCheckIsDelimiterMultiComponent

		public void TestCheckIsDelimiterMultiComponent()
		{
			var rule = Helper.CreateRule();
			var component1 = Helper.CreateRuleComponent(rule);
			var component2 = Helper.CreateRuleComponent(rule);

			// GS1
			rule.IsGS1 = true;
			component1.IsDelimiterMultiComponent = true;
			AssertHasError(component1.IsDelimiterMultiComponentInfo,
				"GS1 Rule Components should not have Multi-Component delimiters.");
			component1.IsDelimiterMultiComponent = false;
			AssertNoErrors(component1.IsDelimiterMultiComponentInfo);

			// Non GS1 partial
			rule.IsGS1 = false;
			rule.IsPartialRule = true;
			component1.IsDelimiterMultiComponent = true;
			AssertHasError(component1.IsDelimiterMultiComponentInfo,
				"Partial rule components should not have Multi-Component delimiters.");
			component1.IsDelimiterMultiComponent = false;
			AssertNoErrors(component1.IsDelimiterMultiComponentInfo);

			// Non GS1 Full - Has a delimiter in a component other than last
			rule.IsPartialRule = false;
			component1.HasDelimiter = true;
			component1.IsDelimiterMultiComponent = true;
			AssertNoErrors(component1.IsDelimiterMultiComponentInfo);
			component1.IsDelimiterMultiComponent = false;
			AssertNoErrors(component1.IsDelimiterMultiComponentInfo);

			// Non GS1 Full - Has no delimiter in a component other than last, however marking as IsDelimiterMultiComponent will set a delimiter
			component1.HasDelimiter = false;
			component1.IsDelimiterMultiComponent = true;
			AssertNoErrors(component1.IsDelimiterMultiComponentInfo);
			component1.IsDelimiterMultiComponent = false;
			AssertNoErrors(component1.IsDelimiterMultiComponentInfo);

			// Non GS1 Full - Has a delimiter in last component
			component2.HasDelimiter = true;
			component2.IsDelimiterMultiComponent = true;
			AssertHasError(component2.IsDelimiterMultiComponentInfo,
				"The last rule component cannot be marked with a Multi-Component delimiter.");
			component2.IsDelimiterMultiComponent = false;
			AssertNoErrors(component2.IsDelimiterMultiComponentInfo);
		}

		#endregion

		#region TestCheckBRC_LengthType

		public void TestCheckBRC_LengthType()
		{
			var rule = Helper.CreateRule();
			var component1 = Helper.CreateRuleComponent(rule);
			var component2 = Helper.CreateRuleComponent(rule);

			//with terminator , all fixed length components
			rule.IsGS1 = false;
			rule.BRU_Terminator = ",";
			component1.LengthTypeForBinding = LengthTypes.Codes.Fixed;
			component1.BRC_MinLength = 1;
			component1.BRC_MaxLength = 1;
			component2.LengthTypeForBinding = LengthTypes.Codes.Fixed;
			component2.BRC_MinLength = 1;
			component2.BRC_MaxLength = 1;

			component1.Validation.ValidateLengthType();
			component2.Validation.ValidateLengthType();
			AssertHasError(component1.LengthTypeForBindingInfo,
				"Terminator should not be used when all the Components are defined as Fixed length.");
			AssertHasError(component2.LengthTypeForBindingInfo,
				"Terminator should not be used when all the Components are defined as Fixed length.");

			//No terminator , all fixed length components
			rule.BRU_Terminator = "";

			component1.Validation.ValidateLengthType();
			component2.Validation.ValidateLengthType();
			AssertNoErrors(component1.LengthTypeForBindingInfo);
			AssertNoErrors(component2.LengthTypeForBindingInfo);

			//with terminator , not all fixed length components
			rule.BRU_Terminator = ",";
			component2.LengthTypeForBinding = LengthTypes.Codes.Any;

			component1.Validation.ValidateLengthType();
			component2.Validation.ValidateLengthType();
			AssertNoErrors(component1.LengthTypeForBindingInfo);
			AssertNoErrors(component2.LengthTypeForBindingInfo);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			using (component.GetValidationSuspender())
			{
				rule.IsGS1 = true;
				component.HasDelimiter = true;

				AssertNoErrors(component.HasDelimiterInfo);
			}

			component.Validation.ValidateAll();
			AssertHasError(component.HasDelimiterInfo, "GS1 rule should not have a delimiter specified.");
		}

		#endregion
	}
}
