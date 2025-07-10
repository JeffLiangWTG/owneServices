using System.Collections.Specialized;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class OptionGroupBuilderTest : FilterBuilderTestWithTempFile
	{
		public void TestRequiredThrowsException()
		{
			var root = new StringTreeNode();
			root.Value = "my field";

			var type = new StringTreeNode();
			type.Value = "Type";

			var checkbox = new StringTreeNode();
			checkbox.Value = "Checkbox";

			var options = new StringTreeNode();
			options.Value = "Options";

			var optionCode = new StringTreeNode();
			optionCode.Value = "OptionCode";

			var optionValue = new StringTreeNode();
			optionValue.Value = "OptionValue";

			var required = new StringTreeNode();
			required.Value = "Required";

			optionCode.Children.Add(optionValue);
			options.Children.Add(optionCode);
			root.Children.Add(options);
			root.Children.Add(type);

			var businessObjectFactory = new BusinessObjectFactory();
			var builder = new OptionGroupBuilder(new ValidatorPack(), businessObjectFactory, new MatchEvaluator(DummyEvaluator), ReportRunningType.Report);
			var expectedParameters = new StringCollection();
			expectedParameters.Add(root.Value);
			AssertNotNull(builder.Build(root, expectedParameters));

			root.Children.Add(required);
			try
			{
				builder.Build(root, expectedParameters);
				Fail("Should have thrown an exception");
			}
			catch (TemplateDefinitionException ex)
			{
				AssertEquals("Wrong message", OptionGroupBuilder.RequiredErrorMessage, ex.Message);
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWithDefaults()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("OptionGroupFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);

				var list = ((OptionGroup)(testFilterCollectionBuilder.IFilterCollection[1])).DescriptionCodePairList;
				AssertEquals(ZBool.True, list[list.Count - 4].Value);
				AssertEquals(ZBool.False, list[list.Count - 3].Value);
				AssertEquals(ZBool.True, list[list.Count - 2].Value);
				AssertEquals(ZBool.False, list[list.Count - 1].Value);
			}
		}

		public void TestOptionsThrowsException()
		{
			var root = new StringTreeNode();
			root.Value = "my field";

			var type = new StringTreeNode();
			type.Value = "Type";

			var checkbox = new StringTreeNode();
			checkbox.Value = "Checkbox";

			root.Children.Add(type);

			var businessObjectFactory = new BusinessObjectFactory();
			var builder = new OptionGroupBuilder(new ValidatorPack(), businessObjectFactory, new MatchEvaluator(DummyEvaluator), ReportRunningType.Report);
			var expectedParameters = new StringCollection();
			expectedParameters.Add(root.Value);
			AssertExceptionThrown<TemplateDefinitionException>("should throw an exception", () => builder.Build(root, expectedParameters));
			AssertNoExceptionThrown("should not throw an exception", () => builder.Build(root, expectedParameters, false));
		}
	}
}
