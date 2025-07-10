using System;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class UserDefinedFieldCollectionBuilderTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDateFieldIsDetected()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				userDefinedFieldBuilder.Build();
				AssertEquals(0, userDefinedFieldBuilder.Errors.Count);
				AssertEquals(1, userDefinedFieldBuilder.UserDefinedFields.Count);
				AssertEquals("Unit test date", userDefinedFieldBuilder.UserDefinedFields[0].DisplayName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTabNameIsSet()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with tabs.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				userDefinedFieldBuilder.Build();
				AssertEquals(0, userDefinedFieldBuilder.Errors.Count);
				AssertEquals(6, userDefinedFieldBuilder.UserDefinedFields.Count);

				AssertEquals("I'm on alpha", userDefinedFieldBuilder.UserDefinedFields[0].DisplayName);
				AssertEquals("Alpha", userDefinedFieldBuilder.UserDefinedFields[0].TabNames[0]);

				AssertEquals("I'm on alpha2", userDefinedFieldBuilder.UserDefinedFields[1].DisplayName);
				AssertEquals("Alpha", userDefinedFieldBuilder.UserDefinedFields[1].TabNames[0]);

				AssertEquals("I'm on beta", userDefinedFieldBuilder.UserDefinedFields[2].DisplayName);
				AssertEquals("Beta", userDefinedFieldBuilder.UserDefinedFields[2].TabNames[0]);

				AssertEquals("I'm on beta too", userDefinedFieldBuilder.UserDefinedFields[3].DisplayName);
				AssertEquals("Beta", userDefinedFieldBuilder.UserDefinedFields[3].TabNames[0]);

				AssertEquals("I'm not on a tab", userDefinedFieldBuilder.UserDefinedFields[4].DisplayName);
				AssertEquals(0, userDefinedFieldBuilder.UserDefinedFields[4].TabNames.Count);

				AssertEquals("I'm on both", userDefinedFieldBuilder.UserDefinedFields[5].DisplayName);
				AssertEquals("Alpha", userDefinedFieldBuilder.UserDefinedFields[5].TabNames[0]);
				AssertEquals("Beta", userDefinedFieldBuilder.UserDefinedFields[5].TabNames[1]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestErrorsAreReportedCorrectlyWhenLookupTypeIsMissingOrInvalid()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("UDF with invalid lookup fields.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(pack, excelTemplate))
			{
				report.PrepareForRender();
				var userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(report.UDFSheet, report.Analyser.ValidatorPack, DummyEvaluator);
				userDefinedFieldBuilder.Build();

				AssertEquals(userDefinedFieldBuilder.Errors.Count, 2);
				AssertStartsWith("Unknown lookup type", "Error Building UDF's from Tree: Unknown user defined field type \"Lookup\"", userDefinedFieldBuilder.Errors[0].Message);
				AssertStartsWith("Unknown user defined field type", "Error Building UDF's from Tree: Unknown lookup type \"WrongType\"", userDefinedFieldBuilder.Errors[1].Message);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSerialisedRootNode()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with tabs.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				StringTreeNode rootNode = userDefinedFieldBuilder.RootNode;

				rootNode = StringTreeNode.Deserialise(StringTreeNode.Serialise(rootNode));

				userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rootNode, nameof(Core.Constants.DataContext.None), new ValidatorPack(), DummyEvaluator);
				userDefinedFieldBuilder.Build();
				AssertEquals(0, userDefinedFieldBuilder.Errors.Count);
				AssertEquals(6, userDefinedFieldBuilder.UserDefinedFields.Count);

				AssertEquals("I'm on alpha", userDefinedFieldBuilder.UserDefinedFields[0].DisplayName);
				AssertEquals("Alpha", userDefinedFieldBuilder.UserDefinedFields[0].TabNames[0]);

				AssertEquals("I'm on alpha2", userDefinedFieldBuilder.UserDefinedFields[1].DisplayName);
				AssertEquals("Alpha", userDefinedFieldBuilder.UserDefinedFields[1].TabNames[0]);

				AssertEquals("I'm on beta", userDefinedFieldBuilder.UserDefinedFields[2].DisplayName);
				AssertEquals("Beta", userDefinedFieldBuilder.UserDefinedFields[2].TabNames[0]);

				AssertEquals("I'm on beta too", userDefinedFieldBuilder.UserDefinedFields[3].DisplayName);
				AssertEquals("Beta", userDefinedFieldBuilder.UserDefinedFields[3].TabNames[0]);

				AssertEquals("I'm not on a tab", userDefinedFieldBuilder.UserDefinedFields[4].DisplayName);
				AssertEquals(0, userDefinedFieldBuilder.UserDefinedFields[4].TabNames.Count);

				AssertEquals("I'm on both", userDefinedFieldBuilder.UserDefinedFields[5].DisplayName);
				AssertEquals("Alpha", userDefinedFieldBuilder.UserDefinedFields[5].TabNames[0]);
				AssertEquals("Beta", userDefinedFieldBuilder.UserDefinedFields[5].TabNames[1]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataContextSettings()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with Data Context.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.Shipment))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				StringTreeNode rootNode = userDefinedFieldBuilder.RootNode;

				rootNode = StringTreeNode.Deserialise(StringTreeNode.Serialise(rootNode));

				userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rootNode, rpt.DataContextValue.ToString(), new ValidatorPack(), DummyEvaluator);
				userDefinedFieldBuilder.Build();
				AssertEquals(0, userDefinedFieldBuilder.Errors.Count);
				AssertEquals(3, userDefinedFieldBuilder.UserDefinedFields.Count);

				AssertEquals("Display Name", "Alpha", userDefinedFieldBuilder.UserDefinedFields[0].DisplayName);
				AssertEquals("Assigned Data Context for Alpha", Core.Constants.DataContext.Consol, userDefinedFieldBuilder.UserDefinedFields[0].DataContextValue.DataContext);

				AssertEquals("Display Name", "Beta", userDefinedFieldBuilder.UserDefinedFields[1].DisplayName);
				AssertEquals("Template default data context for Beta", Core.Constants.DataContext.Shipment, userDefinedFieldBuilder.UserDefinedFields[1].DataContextValue.DataContext);

				AssertEquals("Display Name", "Gamma", userDefinedFieldBuilder.UserDefinedFields[2].DisplayName);
				AssertEquals("Assigned Data Context for Gamma", Core.Constants.DataContext.CartageAdvice, userDefinedFieldBuilder.UserDefinedFields[2].DataContextValue.DataContext);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefaultsSet()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with defaults.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				userDefinedFieldBuilder.Build();
				AssertEquals(0, userDefinedFieldBuilder.Errors.Count);
				AssertEquals(4, userDefinedFieldBuilder.UserDefinedFields.Count);

				AssertEquals("<Boo>", userDefinedFieldBuilder.UserDefinedFields[0].DefaultExpression);
				AssertEquals("Hiss", userDefinedFieldBuilder.UserDefinedFields[1].DefaultExpression);
				AssertEquals("<1974 Oct 14>", userDefinedFieldBuilder.UserDefinedFields[2].DefaultExpression);
				AssertEquals("", userDefinedFieldBuilder.UserDefinedFields[3].DefaultExpression);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDecimalsSet()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with decimals.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				userDefinedFieldBuilder.Build();
				AssertEquals(0, userDefinedFieldBuilder.Errors.Count);
				AssertEquals(2, userDefinedFieldBuilder.UserDefinedFields.Count);

				AssertEquals("Explicitly specified 3 dp", 3, ((NumberField)userDefinedFieldBuilder.UserDefinedFields[0]).DecimalPlaces);
				AssertEquals("DP not specified, should leave default of 2", 2, ((NumberField)userDefinedFieldBuilder.UserDefinedFields[1]).DecimalPlaces);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUdfWithMultipleChoice()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with MultipleChoice.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				userDefinedFieldBuilder.Build();
				AssertEquals(0, userDefinedFieldBuilder.Errors.Count);
				AssertEquals(2, userDefinedFieldBuilder.UserDefinedFields.Count);

				AssertEquals("Explicitly specified 3 dp", 4, ((MultipleChoice)userDefinedFieldBuilder.UserDefinedFields[0]).List.Count);

				OptionGroup optionGroupUDF = (OptionGroup)userDefinedFieldBuilder.UserDefinedFields[1];
				AssertEquals("Explicitly specified 4 options", 4, optionGroupUDF.DescriptionCodePairList.Count);
				AssertEquals("Descriptions of UDF options1", "Desc1", optionGroupUDF.DescriptionCodePairList[0].Description);
				AssertEquals("Descriptions of UDF options2", "Desc2", optionGroupUDF.DescriptionCodePairList[1].Description);
				AssertEquals("Descriptions of UDF options3", "Desc3", optionGroupUDF.DescriptionCodePairList[2].Description);
				AssertEquals("Descriptions of UDF options4", "Desc4", optionGroupUDF.DescriptionCodePairList[3].Description);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLayoutProperties()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with layout.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				userDefinedFieldBuilder.Build();
				AssertEquals(0, userDefinedFieldBuilder.Errors.Count);
				AssertEquals(3, userDefinedFieldBuilder.UserDefinedFields.Count);

				AssertEquals("No layout properties for the first field", userDefinedFieldBuilder.UserDefinedFields[0].Unspecified, userDefinedFieldBuilder.UserDefinedFields[0].Left);
				AssertEquals("No layout properties for the first field", userDefinedFieldBuilder.UserDefinedFields[0].Unspecified, userDefinedFieldBuilder.UserDefinedFields[0].Top);
				AssertEquals("No layout properties for the first field", userDefinedFieldBuilder.UserDefinedFields[0].Unspecified, userDefinedFieldBuilder.UserDefinedFields[0].Width);
				AssertEquals("No layout properties for the first field", userDefinedFieldBuilder.UserDefinedFields[0].Unspecified, userDefinedFieldBuilder.UserDefinedFields[0].Height);

				AssertEquals("Left for the second field", ControlDpiScalingHelper.ScaleToCurrentDpiX(23), userDefinedFieldBuilder.UserDefinedFields[1].Left);
				AssertEquals("No Top for the second field", userDefinedFieldBuilder.UserDefinedFields[1].Unspecified, userDefinedFieldBuilder.UserDefinedFields[1].Top);
				AssertEquals("No Width for the second field", userDefinedFieldBuilder.UserDefinedFields[1].Unspecified, userDefinedFieldBuilder.UserDefinedFields[1].Width);
				AssertEquals("Height for the second field", ControlDpiScalingHelper.ScaleToCurrentDpiX(69), userDefinedFieldBuilder.UserDefinedFields[1].Height);

				AssertEquals("Left for the third field", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), userDefinedFieldBuilder.UserDefinedFields[2].Left);
				AssertEquals("Top for the third field", ControlDpiScalingHelper.ScaleToCurrentDpiX(200), userDefinedFieldBuilder.UserDefinedFields[2].Top);  // assert true
				AssertEquals("Width for the third field", ControlDpiScalingHelper.ScaleToCurrentDpiX(300), userDefinedFieldBuilder.UserDefinedFields[2].Width);
				AssertEquals("Height for the third field", ControlDpiScalingHelper.ScaleToCurrentDpiX(400), userDefinedFieldBuilder.UserDefinedFields[2].Height);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRedundantAddOfTheSameItemInBuild()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("UDF with layout.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(pack, excelTemplate))
			{
				rpt.PrepareForRender();
				UserDefinedFieldCollectionBuilder userDefinedFieldBuilder = new UserDefinedFieldCollectionBuilder(rpt.UDFSheet, rpt.Analyser.ValidatorPack, DummyEvaluator);
				AssertNoExceptionThrown(() => userDefinedFieldBuilder.Build());
				AssertNoExceptionThrown(() => userDefinedFieldBuilder.Build());
			}
		}

		DocumentPack pack;
		protected override void SetUp()
		{
			base.SetUp();
			pack = new DocumentPack();
		}

		string DummyEvaluator(Match match)
		{
			return match.Value;
		}
	}
}
