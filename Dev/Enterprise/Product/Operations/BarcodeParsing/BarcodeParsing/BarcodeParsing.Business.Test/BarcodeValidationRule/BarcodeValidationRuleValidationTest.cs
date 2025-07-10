using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeValidationRuleValidationTest : BarcodeParsingValidationTestCase
	{
		#region TestPrefix

		public void TestPrefixErrorWhenFormatIsDateAndPrefixHasValue()
		{
			CombineAssertions(() =>
			{
				TestPrefixErrorWhenFormatIsDateAndPrefixHasValueCore(OtherDataFormatTypes.Codes.DDMMYY);
				TestPrefixErrorWhenFormatIsDateAndPrefixHasValueCore(OtherDataFormatTypes.Codes.DDMMYYYY);
				TestPrefixErrorWhenFormatIsDateAndPrefixHasValueCore(OtherDataFormatTypes.Codes.MMDDYY);
				TestPrefixErrorWhenFormatIsDateAndPrefixHasValueCore(OtherDataFormatTypes.Codes.MMDDYYYY);
				TestPrefixErrorWhenFormatIsDateAndPrefixHasValueCore(GS1DataFormatTypes.Codes.YYMMDD);
			});
		}

		void TestPrefixErrorWhenFormatIsDateAndPrefixHasValueCore(ZString dateFormat)
		{
			var rule = Helper.CreateValidationRule();
			rule.BVR_Format = dateFormat;
			AssertNoErrors(rule.BVR_PrefixInfo);

			rule.BVR_Prefix = "Test";
			AssertHasError(rule.BVR_PrefixInfo, "Date format should not have prefix");
		}

		public void TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValue()
		{
			CombineAssertions(() =>
			{
				TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValueCore(GS1DataFormatTypes.Codes.AlphaNumericWithSymbols);
				TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValueCore(GS1DataFormatTypes.Codes.AlphaNumericWithSpace);
				TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValueCore(GS1DataFormatTypes.Codes.AlphaNumeric);
				TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValueCore(GS1DataFormatTypes.Codes.AlphaWithSpace);
				TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValueCore(GS1DataFormatTypes.Codes.Alpha);
				TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValueCore(GS1DataFormatTypes.Codes.Numeric);
				TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValueCore(GS1DataFormatTypes.Codes.Digit);
			});
		}

		void TestPrefixErrorWhenFormatIsNotDateAndPrefixHasValueCore(ZString nonDateFormat)
		{
			var rule = Helper.CreateValidationRule();
			rule.BVR_Format = nonDateFormat;
			AssertNoErrors(rule.BVR_PrefixInfo);

			rule.BVR_Prefix = "Test";
			AssertNoErrors(rule.BVR_PrefixInfo);
		}

		#endregion

		#region TestCheckBVR_MinLength

		public void TestCheckBVR_MinLength()
		{
			var rule = Helper.CreateValidationRule();
			AssertEquals("Precondition", true, rule.IsLengthTypeRange());
			AssertEquals("Precondition", (short)0, rule.BVR_MinLength);
			AssertHasError(rule.BVR_MinLengthInfo, "Min and Max Length should be greater than zero if Length Type is not Any.");

			rule.BVR_MinLength = 2;
			AssertHasError(rule.BVR_MinLengthInfo, "Min Length cannot be greater than Max Length.");

			rule.BVR_MaxLength = 2;
			AssertHasError(rule.BVR_MinLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");

			rule.BVR_MinLength = 1;
			AssertNoErrors(rule.BVR_MinLengthInfo);

			rule.BVR_MinLength = -1;
			AssertHasError(rule.BVR_MinLengthInfo, "Min Length cannot be negative.");

			rule.BVR_MinLength = 0;
			AssertHasError(rule.BVR_MinLengthInfo, "Min Length should be greater than zero if Max Length is greater than zero.");

			rule.BVR_MinLength = 1;
			AssertNoErrors(rule.BVR_MinLengthInfo);
		}

		#endregion

		#region TestCheckBVR_MaxLength

		public void TestCheckBVR_MaxLength()
		{
			var rule = Helper.CreateValidationRule();
			AssertEquals("Precondition", true, rule.IsLengthTypeRange());
			AssertEquals("Precondition", (short)0, rule.BVR_MaxLength);
			AssertHasError(rule.BVR_MaxLengthInfo, "Min and Max Length should be greater than zero if Length Type is not Any.");

			rule.BVR_MaxLength = 10;
			AssertHasError(rule.BVR_MaxLengthInfo, "Min Length should be greater than zero if Max Length is greater than zero.");

			rule.BVR_MinLength = 10;
			AssertHasError(rule.BVR_MaxLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");

			rule.BVR_MinLength = 9;
			AssertNoErrors(rule.BVR_MaxLengthInfo);

			rule.BVR_MaxLength = 8;
			AssertHasError(rule.BVR_MaxLengthInfo, "Max Length cannot be less than Min Length.");

			rule.BVR_MinLength = -1;
			AssertHasError(rule.BVR_MinLengthInfo, "Min Length cannot be negative.");
			AssertHasError(rule.BVR_MaxLengthInfo, "Min Length should be greater than zero if Max Length is greater than zero.");

			rule.BVR_MaxLength = -1;
			AssertHasError(rule.BVR_MaxLengthInfo, "Max Length cannot be negative.");
		}

		#endregion

		#region TestCheckMinLengthAndMaxLengthGreaterThanZeroWhenLengthTypeIsNOTAny

		public void TestCheckMinLengthAndMaxLengthGreaterThanZeroWhenLengthTypeIsNOTAny()
		{
			var rule = Helper.CreateValidationRule();
			AssertEquals("Precondition", true, rule.IsLengthTypeRange());

			AssertHasError(rule.BVR_MinLengthInfo, "Min and Max Length should be greater than zero if Length Type is not Any.");
			AssertHasError(rule.BVR_MaxLengthInfo, "Min and Max Length should be greater than zero if Length Type is not Any.");

			rule.LengthTypeForBinding = LengthTypes.Codes.Any;
			AssertNoErrors("LengthType is Any, should have no error.", rule.BVR_MinLengthInfo);
			AssertNoErrors("LengthType is Any, should have no error.", rule.BVR_MaxLengthInfo);
		}

		#endregion

		#region TestChangingLengthTypeWillValidateMinMaxLength

		public void TestChangingLengthTypeWillValidateMinMaxLength()
		{
			var rule = Helper.CreateValidationRule();
			AssertEquals("Precondition", true, rule.IsLengthTypeRange());

			rule.BVR_MaxLength = 8;
			rule.BVR_MinLength = 8;
			AssertHasError(rule.BVR_MinLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");
			AssertHasError(rule.BVR_MaxLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");

			rule.LengthTypeForBinding = LengthTypes.Codes.Fixed;
			AssertNoErrors("LengthType is Fixed, should has no error.", rule.BVR_MaxLengthInfo);
			AssertNoErrors("LengthType is Fixed, should has no error.", rule.BVR_MinLengthInfo);

			rule.LengthTypeForBinding = LengthTypes.Codes.Range;
			AssertHasError(rule.BVR_MinLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");
			AssertHasError(rule.BVR_MaxLengthInfo, "Max Length should be greater than Min Length if Length Type is Range.");
		}

		#endregion

		#region TestMinMaxLengthWhenFormatIsDate

		public void TestMinMaxLengthWhenFormatIsDate()
		{
			var rule = Helper.CreateValidationRule();
			var assert = (string dateFormat, short length) =>
			{
				rule.BVR_Format = dateFormat;
				CombineAssertions(() =>
				{
					AssertEquals("Min Length should be " + length, length, rule.BVR_MinLength);
					AssertEquals("Max Length should be " + length, length, rule.BVR_MaxLength);
					Assert("Mix Length should be read only", rule.BVR_MinLengthInfo.ReadOnly);
					Assert("Max Length should be read only", rule.BVR_MaxLengthInfo.ReadOnly);
					AssertNoErrors("Mix Length should not have error", rule.BVR_MinLengthInfo);
					AssertNoErrors("Max Length should not have error", rule.BVR_MaxLengthInfo);
				});
			};

			assert("D1", 6);
			assert("D2", 8);
			assert("D3", 6);
			assert("D4", 8);
			assert("D5", 6);
		}

		#endregion

		#region TestCheckBVR_Format

		public void TestCheckBVR_Format()
		{
			var rule = Helper.CreateValidationRule();
			AssertNoErrors("Precondition", rule.BVR_FormatInfo);

			rule.BVR_Format = "";
			AssertHasError(rule.BVR_FormatInfo, "Please enter a Format.");

			rule.BVR_Format = "N";
			AssertNoErrors(rule.BVR_FormatInfo);

			rule.BVR_Format = "XX";
			AssertHasError(rule.BVR_FormatInfo, "Enter a valid Format.");
		}

		#endregion

		#region TestCheckBVR_TargetField

		public void TestCheckBVR_TargetField()
		{
			var rule = Helper.CreateValidationRule(DummyTargetFields.Codes.TargetField1);
			AssertNoErrors("Precondition", rule.BVR_TargetFieldInfo);

			rule.BVR_TargetField = "";
			AssertHasError(rule.BVR_TargetFieldInfo, "Please enter a Target Field.");

			rule.BVR_TargetField = "IGN";
			AssertHasError(rule.BVR_TargetFieldInfo, "Enter a valid Target Field.");

			rule.BVR_TargetField = "XXX";
			AssertHasError(rule.BVR_TargetFieldInfo, "Enter a valid Target Field.");

			rule.BVR_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(rule.BVR_TargetFieldInfo);
		}

		#endregion

		#region TestTargetFieldIsUsedOnlyOnce

		public void TestTargetFieldIsUsedOnlyOnce()
		{
			var ruleSet = Helper.CreateRuleSet();
			var rule1 = Helper.CreateValidationRule(ruleSet, DummyTargetFields.Codes.TargetField1);
			AssertNoErrors("Precondition", rule1.BVR_TargetFieldInfo);

			rule1.BVR_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertNoErrors(rule1.BVR_TargetFieldInfo);

			var rule2 = Helper.CreateValidationRule(ruleSet, DummyTargetFields.Codes.TargetField2);
			AssertNoErrors("Precondition", rule2.BVR_TargetFieldInfo);

			rule2.BVR_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertHasError(rule2.BVR_TargetFieldInfo, "The Target Field 'TF1' cannot be specified more than once per Rule Set.");

			rule2.BVR_TargetField = DummyTargetFields.Codes.TargetField2;
			AssertNoErrors(rule2.BVR_TargetFieldInfo);
		}

		#endregion

		#region TestCheckBVR_TargetField

		public void TestCheckBVR_TargetField_ConsumerValidation()
		{
			var rule = Helper.CreateValidationRule(DummyTargetFields.Codes.TargetField1);
			rule.RuleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			AssertNoErrors("Precondition", rule.BVR_TargetFieldInfo);

			var barcodeParsingConsumer = (DummyBarcodeValidationRulesConsumer)DummyBarcodeParsingConsumer.GetDummy(Factory);
			barcodeParsingConsumer.TargetFieldValidationErrorForTest = "Some Validation Error.";

			rule.BVR_TargetField = DummyTargetFields.Codes.TargetField1;
			AssertHasError(rule.BVR_TargetFieldInfo, "Some Validation Error.");
		}

		#endregion

		#region TestFormatIsValidForTargetField

		public void TestFormatIsValidForTargetField()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = "DUM"; // Dummy
			var rule = Helper.CreateValidationRule(ruleSet, DummyTargetFields.Codes.TargetField1);
			AssertNoErrors("Precondition", rule.BVR_FormatInfo);

			var dummy = (DummyBarcodeParsingConsumer)Factory.GetBarcodeParsingConsumerFromModuleCode("DUM");
			dummy.SetFormatsForTargetField(DummyTargetFields.Codes.TargetField1, new[] { FormatType.N });
			dummy.SetFormatsForTargetField(DummyTargetFields.Codes.TargetField2, new[] { FormatType.AS, FormatType.ANY });

			rule.BVR_TargetField = DummyTargetFields.Codes.TargetField1;
			foreach (ICodeDescription pair in new GS1DataFormatTypes().ToArray().Union(new OtherDataFormatTypes().ToArray()))
			{
				rule.BVR_Format = pair.Code;
				if (pair.Code != GS1DataFormatTypes.Codes.Numeric)
				{
					AssertHasError(rule.BVR_FormatInfo, $"Format for field 'Target 1' should be {FormatType.N}");
				}
				else
				{
					AssertNoErrors(rule.BVR_FormatInfo);
				}
			}

			rule.BVR_TargetField = DummyTargetFields.Codes.TargetField2;
			foreach (ICodeDescription pair in new GS1DataFormatTypes().ToArray().Union(new OtherDataFormatTypes().ToArray()))
			{
				rule.BVR_Format = pair.Code;
				if (pair.Code != GS1DataFormatTypes.Codes.AlphaWithSpace && pair.Code != GS1DataFormatTypes.Codes.AlphaNumericWithSymbols)
				{
					AssertHasError(rule.BVR_FormatInfo, $"Format for field 'Target 2' should be one of: {FormatType.AS}, {FormatType.ANY}");
				}
				else
				{
					AssertNoErrors(rule.BVR_FormatInfo);
				}
			}
		}

		#endregion
	}
}
