using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class GroupByCollectionBuilderTest : TempFileTestCase
	{
		public void TestEmptyCollectionOnConstruction()
		{
			AssertNotNull("GroupByCollection should not be null", gbcb.GroupByCollection);
			AssertEquals("Should be empty", 0, gbcb.GroupByCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoEndMarker()
		{
			Build("EmptySheet");
			ExpectError("no #end marker");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoGroupBys()
		{
			Build("NoGroupBys");
			ExpectNoErrors();
			AssertEquals("GroupBy count", 0, gbcb.GroupByCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoGroupBySheet()
		{
			Build("NoFilterSheet");
			ExpectNoErrors();
			AssertEquals("No GroupBy sheet should give an empty GBC", 0, gbcb.GroupByCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOneGroupBy()
		{
			Build("OneGroupBy");
			ExpectNoErrors();
			AssertEquals("Count", 1, gbcb.GroupByCollection.Count);
			AssertEquals("Display name", "Alpha", gbcb.GroupByCollection[0].DisplayName);
			AssertEquals("Field list", "a,b,c", gbcb.GroupByCollection[0].FieldList);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestManyGroupBys()
		{
			Build("ThreeGroupBysWithSelected");
			ExpectNoErrors();
			AssertEquals("Count", 3, gbcb.GroupByCollection.Count);

			AssertEquals("Display name", "Alpha", gbcb.GroupByCollection[0].DisplayName);
			AssertEquals("Field list", "a,b,c", gbcb.GroupByCollection[0].FieldList);

			AssertEquals("Display name", "Beta", gbcb.GroupByCollection[1].DisplayName);
			AssertEquals("Field list", "b,c,a", gbcb.GroupByCollection[1].FieldList);

			AssertEquals("Display name", "Gamma", gbcb.GroupByCollection[2].DisplayName);
			AssertEquals("Field list", "c,a,b", gbcb.GroupByCollection[2].FieldList);

			AssertEquals("Default groupby", gbcb.GroupByCollection.DefaultGroupBy, gbcb.GroupByCollection[1]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExplicitDefault()
		{
			Build("DefaultGroupBy");
			ExpectNoErrors();
			AssertEquals("Count", 3, gbcb.GroupByCollection.Count);

			AssertEquals("Default display name", "x", gbcb.GroupByCollection.SelectedGroupBy.DisplayName);
			AssertEquals("Default field list", "f", gbcb.GroupByCollection.SelectedGroupBy.FieldList);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImplicitDefault()
		{
			Build("ThreeGroupBys");
			ExpectNoErrors();
			AssertEquals("Count", 3, gbcb.GroupByCollection.Count);

			AssertEquals("Default display name", "Alpha", gbcb.GroupByCollection.SelectedGroupBy.DisplayName);
			AssertEquals("Default field list", "a,b,c", gbcb.GroupByCollection.SelectedGroupBy.FieldList);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void Test_None_Option()
		{
			Build("GroupByWithNoneOption");
			ExpectNoErrors();
			AssertEquals("Count", 3, gbcb.GroupByCollection.Count);

			AssertEquals("Default display name", "this is a report with no groupbys", gbcb.GroupByCollection.SelectedGroupBy.DisplayName);
			AssertEquals("Default field list", "", gbcb.GroupByCollection.SelectedGroupBy.FieldList);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleDefaults()
		{
			Build("MultipleDefaultGroupBy");
			ExpectError("one default");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestErrorInGroupBy()
		{
			Build("GroupByError");
			ExpectError("one and only one");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoGroupBysAreReturnedWhenAnyErrorIsPresent()
		{
			Build("GroupByError");
			AssertEquals("Should not get any GroupBys when any errors are present", 0, gbcb.GroupByCollection.Count);
		}

		GroupByCollectionBuilder gbcb;
		ExcelInterface xl;
		ExcelWorkSheet workSheet;
		Report report;

		protected override void SetUp()
		{
			base.SetUp();
			xl = new ExcelInterface();
			workSheet = xl.GetEmptyWorkSheetForUnitTests();
			gbcb = new GroupByCollectionBuilder(workSheet);
		}

		protected override void TearDown()
		{
			base.TearDown();
			xl?.Dispose();
			workSheet?.Dispose();
			report?.Dispose();
		}

		// Helpers
		void Build(string filename)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(filename + ".xls", TestFilesSubFolder.ReportTestFiles);
			{
				report = new Report(new DocumentPack(), excelTemplate);
				gbcb = new GroupByCollectionBuilder(report.GroupBySheet);
				gbcb.Build();
			}
		}

		void ExpectNoErrors()
		{
			var errorList = "";
			foreach (TemplateDefinitionException error in gbcb.Errors)
			{
				errorList += error + System.Environment.NewLine;
			}

			Assert("There should not be any errors, but these errors were present:\n" + errorList, !gbcb.HasErrors);
		}

		void ExpectError(string errorMessage)
		{
			foreach (ReportProcessingError error in gbcb.Errors)
			{
				if (Regex.IsMatch(error.ToString(), errorMessage, RegexOptions.IgnoreCase))
				{
					AssertionCount++;
					return;
				}
			}

			Fail("Expected error message <" + errorMessage + "> was not found. Error count: " + gbcb.Errors.Count);
		}
	}
}
