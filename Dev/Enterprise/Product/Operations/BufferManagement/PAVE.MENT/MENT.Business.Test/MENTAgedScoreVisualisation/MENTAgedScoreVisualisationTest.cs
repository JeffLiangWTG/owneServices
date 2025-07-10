using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(MENTAgedScoreVisualisation))]
	class MENTAgedScoreVisualisationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Pina Colada");

			AssertEquals(true, visualisation.MVI_IsCustomised);
			AssertEquals(GraphTypes.Codes.Column, visualisation.MVI_GraphType);

			AssertEquals(true, visualisation.ShowLegend);
			AssertEquals(false, visualisation.ShowVerticalGridLines);
			AssertEquals(false, visualisation.ShowHorizontalGridLines);
			AssertEquals(false, visualisation.AllowZoom);
		}

		public void TestVisualisationClone()
		{
			var visualisation = Factory.NewWithValidTestData<MENTAgedScoreVisualisation>();
			visualisation.GraphTitle = "I AM ABOUT TO BE CLONED BRO";
			visualisation.AllowArrowAnnotations = true;
			var sequences = visualisation.CategorySequenceCollection;
			var column1 = sequences.AddNew();
			column1.Column = "Some Staff Initials";
			column1.Selected = true;
			var column2 = sequences.AddNew();
			column2.Column = "Some Staff Initials";
			column2.ColumnDisplay = "Override Intials";
			column2.Selected = true;
			var column3 = sequences.AddNew();
			column3.Column = "Some Staff Initials";
			column3.Sequence = 10;
			column3.Selected = false;

			var copy = (MENTAgedScoreVisualisation)visualisation.Clone();
			AssertEquals(visualisation.GraphTitle, copy.GraphTitle);
			AssertEquals(visualisation.AllowArrowAnnotations, copy.AllowArrowAnnotations);
			AssertEquals(visualisation.CategorySequenceCollection.Count, copy.CategorySequenceCollection.Count);
			AssertEquals(column1.Column, copy.CategorySequenceCollection[0].Column);
			AssertEquals(column1.Selected, copy.CategorySequenceCollection[0].Selected);
			AssertEquals(column2.Column, copy.CategorySequenceCollection[1].Column);
			AssertEquals(column2.ColumnDisplay, copy.CategorySequenceCollection[1].ColumnDisplay);
			AssertEquals(column2.Selected, copy.CategorySequenceCollection[1].Selected);
			AssertEquals(column3.Column, copy.CategorySequenceCollection[2].Column);
			AssertEquals(column3.Sequence, copy.CategorySequenceCollection[2].Sequence);
			AssertEquals(column3.Selected, copy.CategorySequenceCollection[2].Selected);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return MENTTestHelper.CreateVisualisation(Factory, "VisualFun", queryCode: "CHARLIE");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return MENTTestHelper.CreateVisualisation(Factory, "VisualFun", queryCode: "SNOOPY");
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "AllowArrowAnnotations";
				yield return "AllowZoom";
				yield return "CategorySequenceCollection";
				yield return "GraphTitle";
				yield return "IsNormalised";
				yield return "LowerBoundAggregationSequence";
				yield return "PerformLowerBoundAggregation";
				yield return "PerformUpperBoundAggregation";
				yield return "ShowAcceptabilityBands";
				yield return "ShowHorizontalGridLines";
				yield return "ShowLegend";
				yield return "ShowVerticalGridLines";
				yield return "SmoothCurve";
				yield return "UpperBoundAggregationSequence";
				yield return "UseOverriddenCategorySequence";
				yield return "XAxisLabel";
				yield return "XAxisUnits";
				yield return "YAxisLabel";
				yield return "YAxisUnits";
			}
		}

		#endregion
	}

	[UseSnapshotProtection]
	public class MENTAgedScoreVisualisationNonTransactionTest : TestCase
	{
		public void TestPopulateCategorySequenceCollection_NoExtraction()
		{
			var visualisation = Factory.NewWithValidTestData<MENTAgedScoreVisualisation>();
			visualisation.MVI_MEX = ZGuid.Empty;

			AssertEquals(0, visualisation.CategorySequenceCollection.Count);

			visualisation.PopulateCategorySequenceCollection();

			AssertEquals(0, visualisation.CategorySequenceCollection.Count);
		}

		public void TestPopulateCategorySequenceCollection_PerformsExtraction_0Results()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Vene Vidi Vici", extractionName: "For Rome!");
			var extraction = visualisation.Extraction;

			var attributeColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			attributeColumn.Selected = true;
			attributeColumn.Sequence = 1;

			Factory.Save();

			AssertEquals(0, visualisation.CategorySequenceCollection.Count);
			visualisation.PopulateCategorySequenceCollection();
			AssertEquals(0, visualisation.CategorySequenceCollection.Count);
		}

		public void TestPopulateCategorySequenceCollection_PerformsExtraction_HasResults()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Never lose the upper hand of an ambush", extractionName: "Teutoburg Forest is the grave of all");
			var extraction = visualisation.Extraction;

			var attributeColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			attributeColumn.Selected = true;
			attributeColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(4), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(5), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(6), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(7), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(8), ZGuid.Empty, ZGuid.Empty, "CC");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(9), ZGuid.Empty, ZGuid.Empty, "CC");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(10), ZGuid.Empty, ZGuid.Empty, "DD");

			var extractor = MENTTestHelper.CreateExtractor(extraction);
			var extractedData = extractor.Extract();
			AssertEquals("Precondition: Collecting based on count of attribute value, there are 4 unique attribute values", 4, extractedData.ExtractionResults.Single().Results.Count);

			AssertEquals(0, visualisation.CategorySequenceCollection.Count);
			visualisation.PopulateCategorySequenceCollection();
			AssertEquals(4, visualisation.CategorySequenceCollection.Count);

			var rows = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().ToArray();

			AssertVisualisationColumnSpecification(rows[0], "AA", "AA", 0, true);
			AssertVisualisationColumnSpecification(rows[1], "BB", "BB", 1, true);
			AssertVisualisationColumnSpecification(rows[2], "CC", "CC", 2, true);
			AssertVisualisationColumnSpecification(rows[3], "DD", "DD", 3, true);
		}

		public void TestPopulateCategorySequenceCollection_PerformsExtraction_RemovesPrevious()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Greek Soldiers Shaved the hair of disgraced soldiers", extractionName: "Fancy being a bald eagle");
			var extraction = visualisation.Extraction;

			var attributeColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			attributeColumn.Selected = true;
			attributeColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(4), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(5), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(6), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(7), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(8), ZGuid.Empty, ZGuid.Empty, "CC");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(9), ZGuid.Empty, ZGuid.Empty, "CC");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(10), ZGuid.Empty, ZGuid.Empty, "DD");

			var extractor = MENTTestHelper.CreateExtractor(extraction);
			var extractedData = extractor.Extract();
			AssertEquals("Precondition: Collecting based on count of attribute value, there are 4 unique attribute values", 4, extractedData.ExtractionResults.Single().Results.Count);

			visualisation.CategorySequenceCollection.Add(new VisualisationColumnSpecification(visualisation) { Column = "EE", Selected = true, Sequence = 0 });
			visualisation.CategorySequenceCollection.Add(new VisualisationColumnSpecification(visualisation) { Column = "FF", Selected = true, Sequence = 1 });

			AssertEquals(2, visualisation.CategorySequenceCollection.Count);
			visualisation.PopulateCategorySequenceCollection();
			AssertEquals(4, visualisation.CategorySequenceCollection.Count);

			var rows = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().ToArray();

			AssertVisualisationColumnSpecification(rows[0], "AA", "AA", 0, true);
			AssertVisualisationColumnSpecification(rows[1], "BB", "BB", 1, true);
			AssertVisualisationColumnSpecification(rows[2], "CC", "CC", 2, true);
			AssertVisualisationColumnSpecification(rows[3], "DD", "DD", 3, true);
		}

		public void TestPopulateCategorySequenceCollection_PerformsExtraction_IfNoResultsReturnedDoesntRemovePrevious()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Fight with honour men!", extractionName: "We fight to the last. Then we are no more");
			var extraction = visualisation.Extraction;

			var attributeColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			attributeColumn.Selected = true;
			attributeColumn.Sequence = 1;

			Factory.Save();

			var extractor = MENTTestHelper.CreateExtractor(extraction);
			var extractedData = extractor.Extract();
			AssertEquals("Precondition: Collecting based on count of attribute value, there are 0 unique attribute values", 0, extractedData.ExtractionResults.Single().Results.Count);

			visualisation.CategorySequenceCollection.Add(new VisualisationColumnSpecification(visualisation) { Column = "EE", Selected = true, Sequence = 0 });
			visualisation.CategorySequenceCollection.Add(new VisualisationColumnSpecification(visualisation) { Column = "FF", Selected = true, Sequence = 1 });

			AssertEquals(2, visualisation.CategorySequenceCollection.Count);
			visualisation.PopulateCategorySequenceCollection();
			AssertEquals(2, visualisation.CategorySequenceCollection.Count);

			var rows = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().ToArray();

			AssertVisualisationColumnSpecification(rows[0], "EE", "EE", 0, true);
			AssertVisualisationColumnSpecification(rows[1], "FF", "FF", 1, true);
		}

		public void TestPopulateCategorySequenceCollection_WithInlineComment()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Never lose the upper hand of an ambush", extractionName: "Teutoburg Forest is the grave of all");
			var extraction = visualisation.Extraction;

			extraction.RelatedQuery.MAQ_SqlText = "select top 1 94 Score, null ReleaseGroup, null Component, '' AttributeValue, '' Staff from dbo.MENTAgedScoreMetric --blah blah blah";
			extraction.IsInstantaneous = true;

			var attributeColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			attributeColumn.Selected = true;
			attributeColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(5), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(8), ZGuid.Empty, ZGuid.Empty, "CC");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(10), ZGuid.Empty, ZGuid.Empty, "DD");

			var extractor = MENTTestHelper.CreateExtractor(extraction);
			var extractedData = extractor.Extract();
			AssertEquals("Precondition: Collecting based on count of attribute value, there are 1 unique attribute values", 1, extractedData.ExtractionResults.Single().Results.Count);

			AssertEquals(0, visualisation.CategorySequenceCollection.Count);
			visualisation.PopulateCategorySequenceCollection();
			AssertEquals(1, visualisation.CategorySequenceCollection.Count);
		}

		public void TestPopulateCategorySequenceCollection_MultipleExtractions()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Greek Soldiers Shaved the hair of disgraced soldiers", extractionName: "Fancy being a bald eagle");
			var extraction = visualisation.Extraction;

			var attributeColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.AttributeValue, true);
			attributeColumn.Sequence = 1;

			var additionalExtraction = MENTTestHelper.CreateExtraction(Factory, "Ozamataz", extraction.RelatedQuery);
			var additionalAttributeColumn = MENTTestHelper.GetSqlColumnFromCollection(additionalExtraction.CategoryColumns, MENTColumns.Codes.AttributeValue, true);
			additionalAttributeColumn.Sequence = 1;

			MENTTestHelper.CreateAdditionalExtractionLink(extraction, extraction.RelatedQuery, additionalExtraction);

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(4), ZGuid.Empty, ZGuid.Empty, "AA");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(5), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(6), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(7), ZGuid.Empty, ZGuid.Empty, "BB");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(8), ZGuid.Empty, ZGuid.Empty, "CC");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(9), ZGuid.Empty, ZGuid.Empty, "CC");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(10), ZGuid.Empty, ZGuid.Empty, "DD");

			var extractor = MENTTestHelper.CreateExtractor(extraction);
			var extractedData = extractor.Extract();
			AssertEquals(2, extractedData.ExtractionResults.Count());
			AssertEquals(4, extractedData.ResultCategoryIndexMap.Count());

			visualisation.CategorySequenceCollection.Add(new VisualisationColumnSpecification(visualisation) { Column = "EE", Selected = true, Sequence = 0 });
			visualisation.CategorySequenceCollection.Add(new VisualisationColumnSpecification(visualisation) { Column = "FF", Selected = true, Sequence = 1 });

			AssertEquals(2, visualisation.CategorySequenceCollection.Count);
			visualisation.PopulateCategorySequenceCollection();
			AssertEquals(4, visualisation.CategorySequenceCollection.Count);

			var rows = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().ToArray();

			AssertVisualisationColumnSpecification(rows[0], "AA", "AA", 0, true);
			AssertVisualisationColumnSpecification(rows[1], "BB", "BB", 1, true);
			AssertVisualisationColumnSpecification(rows[2], "CC", "CC", 2, true);
			AssertVisualisationColumnSpecification(rows[3], "DD", "DD", 3, true);
		}

		public void TestCategorySequenceCollectionColumn_SortedAscending_ContainsColumnResultsInAlphanumericOrder()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Revenge is a dish best served cold.", extractionName: "Leave the gun. Take the cannoli");
			var extraction = visualisation.Extraction;

			var attributeColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			attributeColumn.Selected = true;
			attributeColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "Banana");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "5");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "Apple");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "200");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "42");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "100000000");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "2cool4u");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "3cool5u");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "2cool42");

			var extractor = MENTTestHelper.CreateExtractor(extraction);
			var extractedData = extractor.Extract();
			AssertEquals("Precondition", 10, extractedData.ExtractionResults.Single().Results.Count);

			CombineAssertions(() =>
			{
				AssertEquals(0, visualisation.CategorySequenceCollection.Count);
				visualisation.PopulateCategorySequenceCollection();
				AssertEquals(10, visualisation.CategorySequenceCollection.Count);

				var rows = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Select(x => x.ColumnDisplay.ToString()).ToArray();
				AssertArrayEqualsByElements("Precondition", new[] { "1", "100000000", "200", "2cool42", "2cool4u", "3cool5u", "42", "5", "Apple", "Banana" }, rows);

				visualisation.CategorySequenceCollection.Sort("ColumnDisplay", ListSortDirection.Ascending);
				var rows2 = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Select(x => x.ColumnDisplay.ToString()).ToArray();
				var expected = new[] { "1", "2cool4u", "2cool42", "3cool5u", "5", "42", "200", "100000000", "Apple", "Banana" };
				AssertArrayEqualsByElements("Should be sorted", expected, rows2);

				visualisation.CategorySequenceCollection.Sort("Column", ListSortDirection.Ascending);
				rows2 = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Select(x => x.Column.ToString()).ToArray();
				AssertArrayEqualsByElements("Should be sorted", expected, rows2);
			});
		}

		public void TestCategorySequenceCollectionColumn_SortedDescending_ContainsColumnResultsInReverseAlphanumericOrder()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Keep your friends close, but your enemies closer.", extractionName: "How do you say banana daiquiri?");
			var extraction = visualisation.Extraction;

			var attributeColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			attributeColumn.Selected = true;
			attributeColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "Banana");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "5");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "Apple");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "200");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "42");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "100000000");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "2cool4u");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "3cool5u");
			MENTTestHelper.InsertTestDataRow(extraction.RelatedQueryCode, 10, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "2cool42");

			var extractor = MENTTestHelper.CreateExtractor(extraction);
			var extractedData = extractor.Extract();
			AssertEquals("Precondition", 10, extractedData.ExtractionResults.Single().Results.Count);

			CombineAssertions(() =>
			{
				AssertEquals(0, visualisation.CategorySequenceCollection.Count);
				visualisation.PopulateCategorySequenceCollection();
				AssertEquals(10, visualisation.CategorySequenceCollection.Count);

				var rows = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Select(x => x.ColumnDisplay.ToString()).ToArray();
				AssertArrayEqualsByElements("Precondition", new[] { "1", "100000000", "200", "2cool42", "2cool4u", "3cool5u", "42", "5", "Apple", "Banana" }, rows);

				visualisation.CategorySequenceCollection.Sort("ColumnDisplay", ListSortDirection.Descending);
				var rows2 = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Select(x => x.ColumnDisplay.ToString()).ToArray();
				var expected = new[] { "Banana", "Apple", "100000000", "200", "42", "5", "3cool5u", "2cool42", "2cool4u", "1" };
				AssertArrayEqualsByElements("Should be sorted", expected, rows2);

				visualisation.CategorySequenceCollection.Sort("Column", ListSortDirection.Descending);
				rows2 = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Select(x => x.Column.ToString()).ToArray();
				AssertArrayEqualsByElements("Should be sorted", expected, rows2);
			});
		}

		#region Implementation

		void AssertVisualisationColumnSpecification(VisualisationColumnSpecification columnToTest, string expectedColumn, string expectedColumnDisplay, int expectedSequence, bool expectedSelected)
		{
			AssertNotNull(columnToTest);
			AssertEquals(expectedColumn, columnToTest.Column);
			AssertEquals(expectedSequence, columnToTest.Sequence);
			AssertEquals(expectedSelected, columnToTest.Selected);
		}

		protected override void SetUp()
		{
			MENTTestHelper.ClearMENTTables();
			base.SetUp();
			Factory = new BusinessObjectFactory();
			BMSTestHelper.EnableBMSInRegistry();
		}

		BusinessObjectFactory Factory;

		#endregion
	}

	public static class VisualisationExtensions
	{
		public static void PopulateCategorySequenceCollection(this MENTAgedScoreVisualisation visualisation)
		{
			visualisation.PopulateCategorySequenceCollection(new MENTTestHelper.TestExtractorFactoryProvider());
		}
	}
}
