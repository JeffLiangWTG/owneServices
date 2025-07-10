using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SortOrderCollectionBuilderTest : TempFileTestCase
	{
		public void TestEmptyCollectionOnConstruction()
		{
			AssertNotNull("SortOrderCollection should not be null", socb.SortOrderCollection);
			AssertEquals("Should be empty", 0, socb.SortOrderCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoEndMarker()
		{
			Build("EmptySheet");
			ExpectError("no #end marker");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoSortOrders()
		{
			Build("NoSortOrders");
			ExpectNoErrors();
			AssertEquals("Sort order count", 0, socb.SortOrderCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoSortOrderSheet()
		{
			Build("NoFilterSheet");
			ExpectNoErrors();
			AssertEquals("No Sort sheet should give an empty SOC", 0, socb.SortOrderCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOneSortOrder()
		{
			Build("OneSortOrder");
			ExpectNoErrors();
			AssertEquals("Count", 1, socb.SortOrderCollection.Count);
			AssertEquals("Display name", "Alpha", socb.SortOrderCollection[0].DisplayName);
			AssertEquals("Field list", "a, b, c", socb.SortOrderCollection[0].FieldList);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestManySortOrders()
		{
			Build("ThreeSortOrders");
			ExpectNoErrors();
			AssertEquals("Count", 3, socb.SortOrderCollection.Count);

			AssertEquals("Display name", "Alpha", socb.SortOrderCollection[0].DisplayName);
			AssertEquals("Field list", "a, b, c", socb.SortOrderCollection[0].FieldList);

			AssertEquals("Display name", "Beta", socb.SortOrderCollection[1].DisplayName);
			AssertEquals("Field list", "b, c, a", socb.SortOrderCollection[1].FieldList);

			AssertEquals("Display name", "Gamma", socb.SortOrderCollection[2].DisplayName);
			AssertEquals("Field list", "c, a, b", socb.SortOrderCollection[2].FieldList);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExplicitDefault()
		{
			Build("DefaultSortOrder");
			ExpectNoErrors();
			AssertEquals("Count", 3, socb.SortOrderCollection.Count);

			AssertEquals("Default display name", "x", socb.SortOrderCollection.SelectedOrder.DisplayName);
			AssertEquals("Default field list", "f", socb.SortOrderCollection.SelectedOrder.FieldList);
			AssertEquals("DefaultSortOrder should update on the collection", socb.SortOrderCollection.SelectedOrder, socb.SortOrderCollection.DefaultOrder);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImplicitDefault()
		{
			Build("ThreeSortOrders");
			ExpectNoErrors();
			AssertEquals("Count", 3, socb.SortOrderCollection.Count);

			AssertEquals("Default display name", "Alpha", socb.SortOrderCollection.SelectedOrder.DisplayName);
			AssertEquals("Default field list", "a, b, c", socb.SortOrderCollection.SelectedOrder.FieldList);
			AssertEquals("DefaultSortOrder should update on the collection", socb.SortOrderCollection.SelectedOrder, socb.SortOrderCollection.DefaultOrder);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleDefaults()
		{
			Build("MultipleDefaultSortOrder");
			ExpectError("one default");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestErrorInSortOrder()
		{
			Build("SortOrderError");
			ExpectError("one and only one");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoSortOrdersAreReturnedWhenAnyErrorIsPresent()
		{
			Build("SortOrderError");
			AssertEquals("Should not get any sort orders when any errors are present", 0, socb.SortOrderCollection.Count);
		}

		SortOrderCollectionBuilder socb;
		ExcelInterface xl;
		ExcelWorkSheet workSheet;
		Report report;
		ExcelTemplateForUnitTesting excelTemplate;

		protected override void SetUp()
		{
			base.SetUp();
			xl = new ExcelInterface();
			workSheet = xl.GetEmptyWorkSheetForUnitTests();
			socb = new SortOrderCollectionBuilder(workSheet);
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
			excelTemplate = new ExcelTemplateForUnitTesting(filename + ".xls", TestFilesSubFolder.ReportTestFiles);
			report = new Report(new DocumentPack(), excelTemplate);
			socb = new SortOrderCollectionBuilder(report.SortSheet);
			socb.Build();
		}

		void ExpectNoErrors()
		{
			var errorList = "";
			foreach (ReportProcessingError error in socb.Errors)
			{
				errorList += error + System.Environment.NewLine;
			}

			Assert("There should not be any errors, but these errors were present:\n" + errorList, !socb.HasErrors);
		}

		void ExpectError(string errorMessage)
		{
			foreach (ReportProcessingError error in socb.Errors)
			{
				if (Regex.IsMatch(error.ToString(), errorMessage, RegexOptions.IgnoreCase))
				{
					AssertionCount++;
					return;
				}
			}

			Fail("Expected error message <" + errorMessage + "> was not found. Error count: " + socb.Errors.Count);
		}
	}
}
