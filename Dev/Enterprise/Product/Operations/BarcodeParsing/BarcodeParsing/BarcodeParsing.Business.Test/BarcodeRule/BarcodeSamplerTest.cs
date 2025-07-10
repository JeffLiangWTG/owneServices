using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;
using Enterprise.BarcodeParsingEngine.Warehouse;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeSamplerTest : BarcodeParsingTestCase
	{
		#region Samples for Date formats

		[TestDate(2016, 08, 22)]
		public void TestSampleBarcode_FormatYYMMDD()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			component.BRC_Format = GS1DataFormatTypes.Codes.YYMMDD;
			AssertEquals("160822", BarcodeSampler.GetSampleBarcode(rule));
		}

		[TestDate(2016, 08, 22)]
		public void TestSampleBarcode_FormatDDMMYY()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			component.BRC_Format = OtherDataFormatTypes.Codes.DDMMYY;
			AssertEquals("220816", BarcodeSampler.GetSampleBarcode(rule));
		}

		[TestDate(2016, 08, 22)]
		public void TestSampleBarcode_FormatDDMMYYYY()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			component.BRC_Format = OtherDataFormatTypes.Codes.DDMMYYYY;
			AssertEquals("22082016", BarcodeSampler.GetSampleBarcode(rule));
		}

		[TestDate(2016, 08, 22)]
		public void TestSampleBarcode_FormatMMDDYY()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			component.BRC_Format = OtherDataFormatTypes.Codes.MMDDYY;
			AssertEquals("082216", BarcodeSampler.GetSampleBarcode(rule));
		}

		[TestDate(2016, 08, 22)]
		public void TestSampleBarcode_FormatMMDDYYYY()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			component.BRC_Format = OtherDataFormatTypes.Codes.MMDDYYYY;
			AssertEquals("08222016", BarcodeSampler.GetSampleBarcode(rule));
		}

		#endregion

		#region Samples for non-Date formats

		public void TestSampleBarcode_Numeric()
		{
			TestSampleBarcodeForFormat(@"
7
77
7,7
7,77
7,777
7,7777
7,777.7
7,777.77
7,777.777
", GS1DataFormatTypes.Codes.Numeric);
		}

		public void TestSampleBarcode_Digit()
		{
			TestSampleBarcodeForFormat(@"
7
77
777
7777
77777
777777
7777777
77777777
777777777
", GS1DataFormatTypes.Codes.Digit);
		}

		public void TestSampleBarcode_Alpha()
		{
			TestSampleBarcodeForFormat(@"
A
AA
AAA
AAAA
AAAAA
AAAAAA
AAAAAAA
AAAAAAAA
AAAAAAAAA
", GS1DataFormatTypes.Codes.Alpha);
		}

		public void TestSampleBarcode_AlphaWithSpace()
		{
			TestSampleBarcodeForFormat(@"
A
AA
A A
A AA
A A A
A A AA
A A A A
A A A AA
A A A A A
", GS1DataFormatTypes.Codes.AlphaWithSpace);
		}

		public void TestSampleBarcode_AlphaNumeric()
		{
			TestSampleBarcodeForFormat(@"
A
A6
A6A
A6A6
A6A6A
A6A6A6
A6A6A6A
A6A6A6A6
A6A6A6A6A
", GS1DataFormatTypes.Codes.AlphaNumeric);
		}

		public void TestSampleBarcode_AlphaNumericWithSpace()
		{
			TestSampleBarcodeForFormat(@"
A
A6
A6A
A6 A
A6 A6
A6 A6A
A6 A6 A
A6 A6 A6
A6 A6 A6A
", GS1DataFormatTypes.Codes.AlphaNumericWithSpace);
		}

		public void TestSampleBarcode_AlphaNumericWithSymbols()
		{
			TestSampleBarcodeForFormat(@"
*
A*
A6*
A6*A
A6*A6
A6*A6*
A6*A6*A
A6*A6*A6
A6*A6*A6*
", GS1DataFormatTypes.Codes.AlphaNumericWithSymbols);
		}

		void TestSampleBarcodeForFormat(ZString expectedSampleValues, ZString format)
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);
			component.BRC_Format = format;

			var stringBuilder = new ZStringBuilder();
			stringBuilder.AppendLine();
			for (ZShort i = 1; i < 10; i++)
			{
				// fixed length
				component.BRC_MinLength = i;
				component.BRC_MaxLength = i;
				stringBuilder.AppendLine(BarcodeSampler.GetSampleBarcode(rule));
			}

			AssertEquals("Sample values are wrong for format " + format, expectedSampleValues, stringBuilder.ToString());
		}

		#endregion

		#region Special formats

		public void TestSampleBarcode_NumericFormatCollisions()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule, 10, 15, false, GS1DataFormatTypes.Codes.Numeric);
			AssertEquals("There are comma and period for long sample.", "7,777.7777", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = ",";
			AssertEquals("We put comma only if it is not used as terminator.", "77777.7777", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = ".";
			AssertEquals("We put period only if it is not used as terminator.", "7,77777777", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = "#";
			component.DelimiterForBinding = ",";
			AssertEquals("We put comma inside value only if it is not used as delimeter.", "77777.7777,77777.7777", BarcodeSampler.GetSampleBarcode(rule));

			component.DelimiterForBinding = ".";
			AssertEquals("We put period inside value only if it is not used as delimeter.", "7,77777777.7,77777777", BarcodeSampler.GetSampleBarcode(rule));

			component.DelimiterForBinding = "";
			component.BRC_ApplicationID = "7";
			AssertEquals("In full rule we don't care if AppID is same as character we use in sample.", "77,777.7777", BarcodeSampler.GetSampleBarcode(rule));

			rule.IsPartialRule = true;
			AssertEquals("For partial rule we must change sample charactes to avoid collision with AppID.", "...78,888.8888#...", BarcodeSampler.GetSampleBarcode(rule));
		}

		public void TestSampleBarcode_NumericWithSymbolsFormatCollisions()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule, 2, 2, false, GS1DataFormatTypes.Codes.AlphaNumericWithSymbols);
			AssertEquals("A*", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = "A";
			AssertEquals("B*", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = "*";
			AssertEquals("A+", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = "#";
			component.DelimiterForBinding = "*";
			AssertEquals("A+*A+", BarcodeSampler.GetSampleBarcode(rule));
		}

		public void TestSampleBarcode_SpaceHandlingAlphaWithSpace()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule, 5, 5, false, GS1DataFormatTypes.Codes.AlphaWithSpace);
			AssertEquals("A A A", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = " ";
			AssertEquals("If space is a terminator - he won't be used inside sample.", "AAAAA", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = "#";
			component.DelimiterForBinding = " ";
			AssertEquals("If space is a delimeter - he won't be used inside sample.", "AAAAA AAAAA", BarcodeSampler.GetSampleBarcode(rule));
		}

		public void TestSampleBarcode_SpaceHandlingAlphaNumericWithSpace()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule, 5, 5, false, GS1DataFormatTypes.Codes.AlphaNumericWithSpace);
			AssertEquals("A6 A6", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = " ";
			AssertEquals("If space is a terminator - it won't be used inside sample.", "A6A6A", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = "#";
			component.DelimiterForBinding = " ";
			AssertEquals("If space is a delimeter - it won't be used inside sample.", "A6A6A A6A6A", BarcodeSampler.GetSampleBarcode(rule));
		}

		#endregion

		#region TestSampleBarcode_ForIgnoreField

		public void TestSampleBarcode_ForIgnoreField()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule, 5, 5);
			component.BRC_TargetField = BarcodeCaptureConstants.IgnoreCode;

			CombineAssertions(() =>
			{
				foreach (var code in new GS1DataFormatTypes().GetAllCodes().Union(new OtherDataFormatTypes().GetAllCodes()))
				{
					component.BRC_Format = code;
					if (!component.IsDateFormat)
					{
						if (code == GS1DataFormatTypes.Codes.Digit)
						{
							AssertEquals($"Should be a proper sample for IgnoreCode field and {code} format.", "00000", BarcodeSampler.GetSampleBarcode(rule));
						}
						else if (code == GS1DataFormatTypes.Codes.Numeric)
						{
							AssertEquals($"Should be a proper sample for IgnoreCode field and {code} format.", "0,000", BarcodeSampler.GetSampleBarcode(rule));
						}
						else
						{
							AssertEquals($"Should be a proper sample for IgnoreCode field and {code} format.", "xxxxx", BarcodeSampler.GetSampleBarcode(rule));
						}
					}
				}
			});

			// have to check date formats manually
			CombineAssertions(() =>
			{
				component.BRC_Format = GS1DataFormatTypes.Codes.YYMMDD;
				AssertEquals($"Should be a proper sample for IgnoreCode field and {component.BRC_Format} format.", new ZDateTime(2000, 1, 1).ToString("yyMMdd"), BarcodeSampler.GetSampleBarcode(rule));

				component.BRC_Format = OtherDataFormatTypes.Codes.DDMMYYYY;
				AssertEquals($"Should be a proper sample for IgnoreCode field and {component.BRC_Format} format.", new ZDateTime(2000, 1, 1).ToString("ddMMyyyy"), BarcodeSampler.GetSampleBarcode(rule));

				component.BRC_Format = OtherDataFormatTypes.Codes.DDMMYY;
				AssertEquals($"Should be a proper sample for IgnoreCode field and {component.BRC_Format} format.", new ZDateTime(2000, 1, 1).ToString("ddMMyy"), BarcodeSampler.GetSampleBarcode(rule));

				component.BRC_Format = OtherDataFormatTypes.Codes.MMDDYY;
				AssertEquals($"Should be a proper sample for IgnoreCode field and {component.BRC_Format} format.", new ZDateTime(2000, 1, 1).ToString("MMddyy"), BarcodeSampler.GetSampleBarcode(rule));

				component.BRC_Format = OtherDataFormatTypes.Codes.MMDDYYYY;
				AssertEquals($"Should be a proper sample for IgnoreCode field and {component.BRC_Format} format.", new ZDateTime(2000, 1, 1).ToString("MMddyyyy"), BarcodeSampler.GetSampleBarcode(rule));
			});
		}

		#endregion

		#region TestSampleBarcodeForPartialRules

		public void TestSampleBarcodeForPartialRules()
		{
			var rule = Helper.CreateRule();
			Helper.CreateRuleComponent(rule, 2, 9, false, GS1DataFormatTypes.Codes.Alpha);
			rule.IsPartialRule = true;

			AssertEquals("Partial rule sample must be surrounded by dots.", "...AAAAA...", BarcodeSampler.GetSampleBarcode(rule));
		}

		#endregion

		#region TestSampleBarcodeForRuleWithMultipleComponentsWithDelimiters

		public void TestSampleBarcodeForRuleWithMultipleComponentsWithDelimiters()
		{
			var rule = Helper.CreateRule();
			rule.BRU_Terminator = "#";
			var component1 = Helper.CreateRuleComponent(rule, 2, 9, false, GS1DataFormatTypes.Codes.Alpha);
			component1.DelimiterForBinding = "=";
			AssertEquals("Component with delimiter doubles the sample.", "AAAAA=AAAAA", BarcodeSampler.GetSampleBarcode(rule));

			var component2 = Helper.CreateRuleComponent(rule, 2, 9, false, GS1DataFormatTypes.Codes.Digit);
			component2.DelimiterForBinding = "-";
			AssertEquals("Multi-component rule doubles the sample.", "AAAAA=AAAAA#77777-77777", BarcodeSampler.GetSampleBarcode(rule));
		}

		#endregion

		#region TestSampleBarcodeJoinsSamplesForEachComponent

		public void TestSampleBarcodeJoinsSamplesForEachComponent()
		{
			var rule = Helper.CreateRule();
			rule.BRU_Terminator = "#";
			var component1 = Helper.CreateRuleComponent(rule, 3, 3, false, GS1DataFormatTypes.Codes.Alpha);
			var component2 = Helper.CreateRuleComponent(rule, 1, 4, false, GS1DataFormatTypes.Codes.Digit);
			var component3 = Helper.CreateRuleComponent(rule, 4, 4, false, GS1DataFormatTypes.Codes.AlphaNumeric);
			AssertEquals("Sample for rule must be combined from samples of components.", "AAA7777#A6A6", BarcodeSampler.GetSampleBarcode(rule));

			component1.BRC_Sequence = 2;
			component2.BRC_Sequence = 1;

			AssertEquals("If we change sequence of components - sample should change.", "7777#AAAA6A6", BarcodeSampler.GetSampleBarcode(rule));
		}

		#endregion

		#region TestSampleLengthForNonDateFormats

		public void TestSampleLengthForNonDateFormats()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule);

			CombineAssertions(() =>
			{
				foreach (var code in new GS1DataFormatTypes().GetAllCodes().Union(new OtherDataFormatTypes().GetAllCodes()))
				{
					component.BRC_Format = code;
					if (component.IsDateFormat)
					{
						continue; // this test is for non-date samples only
					}

					component.BRC_MinLength = 0;
					component.BRC_MaxLength = 0;
					AssertEquals($"For Any Length components of format {code} we try to pick 5 as a sample size.", 5, BarcodeSampler.GetSampleBarcode(rule).Length);

					component.BRC_MinLength = 12;
					component.BRC_MaxLength = 12;
					AssertEquals($"For Fixed-Length components of format {code} we use the length as the sample size.", 12, BarcodeSampler.GetSampleBarcode(rule).Length);

					component.BRC_MinLength = 1;
					component.BRC_MaxLength = 100;
					AssertEquals($"For Range Length components of format {code} we try to pick 5 as a sample size.", 5, BarcodeSampler.GetSampleBarcode(rule).Length);

					component.BRC_MinLength = 6;
					component.BRC_MaxLength = 20;
					AssertEquals($"Sample size should not be less than MinLength for components of format {code} and close to 5.", 6, BarcodeSampler.GetSampleBarcode(rule).Length);

					component.BRC_MinLength = 1;
					component.BRC_MaxLength = 4;
					AssertEquals($"Sample size should not be greater than MaxLength for components of format {code} and close to 5.", 4, BarcodeSampler.GetSampleBarcode(rule).Length);
				}
			});
		}

		#endregion

		#region TestSampleBarcode_ExistsForEveryFormatType

		public void TestSampleBarcode_ExistsForEveryFormatType()
		{
			var rule = Helper.CreateRule();
			var component = Helper.CreateRuleComponent(rule, 2, 9);
			foreach (var code in new GS1DataFormatTypes().GetAllCodes())
			{
				component.BRC_Format = code;
				Assert($"Sample for format {code} must not be empty.", !BarcodeSampler.GetSampleBarcode(rule).IsEmpty);
			}

			foreach (var code in new OtherDataFormatTypes().GetAllCodes())
			{
				component.BRC_Format = code;
				Assert($"Sample for format {code} must not be empty.", !BarcodeSampler.GetSampleBarcode(rule).IsEmpty);
			}
		}

		#endregion

		#region TestSampleBarcode_ExistsForEveryLengthType

		public void TestSampleBarcode_ExistsForEveryLengthType()
		{
			var rangeRule = Helper.CreateRule();
			var rangeComponent = Helper.CreateRuleComponent(rangeRule, 2, 9);
			Assert("Sample for Range Length must not be empty.", !BarcodeSampler.GetSampleBarcode(rangeRule).IsEmpty);

			var fixedRule = Helper.CreateRule();
			var fixedComponent = Helper.CreateRuleComponent(fixedRule, 2);
			Assert("Sample for Fixed Length must not be empty.", !BarcodeSampler.GetSampleBarcode(fixedRule).IsEmpty);

			var anyRule = Helper.CreateRule();
			var anyComponent = Helper.CreateRuleComponent(anyRule, 0);
			Assert("Sample for Any Length must not be empty.", !BarcodeSampler.GetSampleBarcode(anyRule).IsEmpty);
		}

		#endregion

		#region TestSampleBarcode old tests

		[Obsolete("Original author: ANK. Rewrite it if you have time.")]
		public void TestSampleBarcode_ParsesBack()
		{
			var ruleset = Helper.CreateRuleSet();
			ruleset.BRS_Module = "WHS";

			var rule = Helper.CreateRule(ruleset);
			rule.BRU_Name = "TestRule";

			rule.BRU_Terminator = "(T)";
			AssertEquals("Precondition", ZString.Empty, BarcodeSampler.GetSampleBarcode(rule));

			var alphaComponent = Helper.CreateRuleComponent(rule);
			alphaComponent.BRC_Format = nameof(FormatType.AS);
			alphaComponent.BRC_MaxLength = 3;
			alphaComponent.BRC_MinLength = 2;
			alphaComponent.BRC_ApplicationID = "(A)";
			alphaComponent.BRC_TargetField = nameof(WarehouseTargetField.PA1);

			var numericComponent = Helper.CreateRuleComponent(rule);
			numericComponent.BRC_MinLength = 1;
			numericComponent.BRC_MaxLength = 2;
			numericComponent.BRC_Format = nameof(FormatType.N);
			numericComponent.BRC_ApplicationID = "(N)";
			numericComponent.BRC_TargetField = nameof(WarehouseTargetField.PA2);

			var alNumStrictComponent = Helper.CreateRuleComponent(rule);
			alNumStrictComponent.BRC_MaxLength = 5;
			alNumStrictComponent.BRC_MinLength = 2;
			alNumStrictComponent.BRC_Format = nameof(FormatType.ANS);
			alNumStrictComponent.BRC_ApplicationID = "(ANS)";
			alNumStrictComponent.BRC_TargetField = nameof(WarehouseTargetField.PA3);

			Factory.Save();

			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.Barcode = BarcodeSampler.GetSampleBarcode(rule);
			diagnostic.Run();
			AssertEquals(@"The following rules were matched:
Rule No.: 1 - Name: TestRule

The following Data was matched:

Field - Part Attribute 1:
A A

Field - Part Attribute 2:
77

Field - Part Attribute 3:
A6 A6

", diagnostic.Results);

			rule.IsGS1 = true;
			alphaComponent.BRC_ApplicationID = "00";
			numericComponent.BRC_ApplicationID = "10";
			alNumStrictComponent.BRC_ApplicationID = "01";
			Factory.Save();
			diagnostic = new BarcodeParsingDiagnostics(Factory);

			diagnostic.Barcode = BarcodeSampler.GetSampleBarcode(rule);
			diagnostic.Run();

			AssertEquals(@"The following rules were matched:
Rule No.: 1 - Name: TestRule

The following Data was matched:

Field - Part Attribute 1:
777777777777777777

Field - Part Attribute 2:
A6*A6

Field - Part Attribute 3:
77777777777777

",
				diagnostic.Results);
		}

		[Obsolete("Original author: ANK. Rewrite it if you have time.")]
		public void TestSampleBarcode_SingleComponent()
		{
			var rule = Helper.CreateRule();
			rule.BRU_Terminator = "$";
			AssertEquals("Precondition", ZString.Empty, BarcodeSampler.GetSampleBarcode(rule));
			rule.IsPartialRule = true;

			var component = Helper.CreateRuleComponent(rule);
			component.BRC_MaxLength = 5;
			component.BRC_MinLength = 3;
			component.BRC_ApplicationID = "(ID)";

			component.BRC_Format = GS1DataFormatTypes.Codes.Numeric;
			AssertEquals("Sample Output should display Numeric sample.", "...(ID)7,777$...", BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = GS1DataFormatTypes.Codes.AlphaNumericWithSpace;
			AssertEquals("Sample Output should display AlphaNumericWithSpace sample.", "...(ID)A6 A6$...", BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols;
			AssertEquals("Sample Output should display AlphaNumericWithSymbols sample.", "...(ID)A6*A6$...", BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = GS1DataFormatTypes.Codes.YYMMDD;
			AssertEquals("Sample Output should display YYMMDD sample.", string.Format("...(ID){0}...", ZDate.Today.ToString("yyMMdd")), BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = OtherDataFormatTypes.Codes.DDMMYY;
			AssertEquals("Sample Output should display DDMMYY sample.", string.Format("...(ID){0}...", ZDate.Today.ToString("ddMMyy")), BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = OtherDataFormatTypes.Codes.DDMMYYYY;
			AssertEquals("Sample Output should display DDMMYYYY sample.", string.Format("...(ID){0}...", ZDate.Today.ToString("ddMMyyyy")), BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = OtherDataFormatTypes.Codes.MMDDYY;
			AssertEquals("Sample Output should display MMDDYY sample.", string.Format("...(ID){0}...", ZDate.Today.ToString("MMddyy")), BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = OtherDataFormatTypes.Codes.MMDDYYYY;
			AssertEquals("Sample Output should display MMDDYYYY sample.", string.Format("...(ID){0}...", ZDate.Today.ToString("MMddyyyy")), BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = GS1DataFormatTypes.Codes.AlphaWithSpace;
			component.BRC_MaxLength = 3;
			component.BRC_MinLength = 3;
			AssertEquals("Sample Output should display Alpha sample without terminator.", "...(ID)A A...", BarcodeSampler.GetSampleBarcode(rule));

			rule.IsPartialRule = false;
			AssertEquals("Sample Output should display Alpha sample without the elipses.", "(ID)A A", BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Delimiter = (byte)',';
			AssertEquals("Sample Output should display Alpha sample with a delim.", "(ID)A A,A A", BarcodeSampler.GetSampleBarcode(rule));

			rule.BRU_Terminator = "6";
			component.BRC_Delimiter = (byte)'7';
			component.BRC_ApplicationID = "6";
			component.BRC_Format = GS1DataFormatTypes.Codes.Digit;
			rule.IsPartialRule = true;
			component.LengthTypeForBinding = LengthTypes.Codes.Range;
			component.BRC_MaxLength = 4;
			AssertEquals("Sample Output should display Alpha sample with a delim.", "...68888788886...", BarcodeSampler.GetSampleBarcode(rule));

			component.BRC_Format = GS1DataFormatTypes.Codes.AlphaWithSpace;
			component.BRC_ApplicationID = "(ID)";
			component.BRC_Delimiter = (byte)',';
			rule.IsPartialRule = false;
			AssertEquals("Sample Output should display Alpha sample with a delim and not a terminator.", "(ID)A AA,A AA", BarcodeSampler.GetSampleBarcode(rule));
		}

		#endregion
	}
}
