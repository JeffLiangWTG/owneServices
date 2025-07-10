using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	public class MENTAgedScoreExtractorNonTransactionTest : NonTransactionedTestCase
	{
		#region AggregationFunctions

		public void TestAggregationFunction_AVG()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "STEVE");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Oh look a bar graph", query, aggregationType: ExtractionTypes.Codes.Average, collectionColumn: MENTColumns.Codes.Score);

			var attributeValueColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.AttributeValue, true);

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 25.0m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "A9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-2), ZGuid.Empty, ZGuid.Empty, "A9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 16.0m, ZDateTime.Now.AddDays(-3), ZGuid.Empty, ZGuid.Empty, "A9018");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69.0m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "Z9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-2), ZGuid.Empty, ZGuid.Empty, "Z9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2.0m, ZDateTime.Now.AddDays(-3), ZGuid.Empty, ZGuid.Empty, "Z9018");

			var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);
			AssertEquals("A9018", extractedData[0].XCategoryString);
			AssertEquals(20m, extractedData[0].YValue);

			AssertEquals("Z9018", extractedData[1].XCategoryString);
			AssertEquals(30m, extractedData[1].YValue);
		}

		public void TestAggregationFunction_MIN()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "GEORGE");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Digital Dancing", query, aggregationType: ExtractionTypes.Codes.Minimum, collectionColumn: MENTColumns.Codes.Score);

			var attributeValueColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.AttributeValue, true);

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 25.0m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "A9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-2), ZGuid.Empty, ZGuid.Empty, "A9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 16.0m, ZDateTime.Now.AddDays(-3), ZGuid.Empty, ZGuid.Empty, "A9018");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69.0m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "Z9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-2), ZGuid.Empty, ZGuid.Empty, "Z9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2.0m, ZDateTime.Now.AddDays(-3), ZGuid.Empty, ZGuid.Empty, "Z9018");

			var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);
			AssertEquals("A9018", extractedData[0].XCategoryString);
			AssertEquals(16m, extractedData[0].YValue);

			AssertEquals("Z9018", extractedData[1].XCategoryString);
			AssertEquals(2m, extractedData[1].YValue);
		}

		public void TestAggregationFunction_MAX()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "CLINTOND");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "IsntThisFun?", query, aggregationType: ExtractionTypes.Codes.Maximum, collectionColumn: MENTColumns.Codes.Score);

			var attributeValueColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.AttributeValue, true);

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 25.0m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "A9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-2), ZGuid.Empty, ZGuid.Empty, "A9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 16.0m, ZDateTime.Now.AddDays(-3), ZGuid.Empty, ZGuid.Empty, "A9018");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69.0m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "Z9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-2), ZGuid.Empty, ZGuid.Empty, "Z9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2.0m, ZDateTime.Now.AddDays(-3), ZGuid.Empty, ZGuid.Empty, "Z9018");

			var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);
			AssertEquals("A9018", extractedData[0].XCategoryString);
			AssertEquals(25m, extractedData[0].YValue);

			AssertEquals("Z9018", extractedData[1].XCategoryString);
			AssertEquals(69m, extractedData[1].YValue);
		}

		#endregion

		#region Categories

		[TestDate(1995, 12, 23, 12, 0, 0)]
		public void TestXAxisColumns()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DNT";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			component1.FC_Name = "AAA";

			var query = MENTTestHelper.CreateQuery(Factory, "BLAIN");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Flamingpo Flange", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			var sequence = 0;
			foreach (var column in extraction.CategoryColumns.Cast<SQLColumnSpecification>().OrderBy(c => c.Code))
			{
				column.Selected = true;
				column.Sequence = sequence++;
			}

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-1), group1.PK, component1.PK, "Z9018");

			var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);
			AssertEquals("Z9018-AAA-DNT-19-NONE-22-12-1995", extractedData[0].XCategoryString);
		}

		#endregion

		#region Large Numbers

		[TestDate(1995, 12, 23, 12, 0, 0)]
		public void TestLargeScoresDontCauseOverflows()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "BLAIN");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Flamingpo Flange", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			var attributeColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.AttributeValue, true);
			var scoreColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.Score, true);

			Factory.Save();

			var numberWith9ZerosAllBeforeDecimal = 1000000000m;
			var numberWith9ZerosOneIsAfterDecimal = 100000000.0m;
			var numberWith10ZerosAllBeforeDecimal = 10000000000m;
			var numberWith10ZerosOneIsAfterDecimal = 1000000000.0m;

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, numberWith9ZerosAllBeforeDecimal, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, numberWith9ZerosOneIsAfterDecimal, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, numberWith10ZerosAllBeforeDecimal, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, numberWith10ZerosOneIsAfterDecimal, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "4");

			var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 4 }), 0);
			AssertEquals(numberWith9ZerosAllBeforeDecimal, extractedData[0].YValue);
			AssertEquals(numberWith9ZerosOneIsAfterDecimal, extractedData[1].YValue);
			AssertEquals(numberWith10ZerosAllBeforeDecimal, extractedData[2].YValue);
			AssertEquals(numberWith10ZerosOneIsAfterDecimal, extractedData[3].YValue);
		}

		#endregion

		#region Series

		public void TestExtractor_EverySeriesColumn()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Heart");

			foreach (SQLColumnSpecification column in extraction.SeriesColumns)
			{
				column.Selected = true;
			}

			var extractedData = MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }); // Empty Result
		}

		public void TestExtractor_SingleSeries()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "DEFL";

			var query = MENTTestHelper.CreateQuery(Factory, "BANBANNAH");
			var query2 = MENTTestHelper.CreateQuery(Factory, "TOITUS");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Heart", query);

			var rgColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.ReleaseGroup, true);
			rgColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now, group.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(1), group.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query2.MAQ_Code, 5, ZDateTime.Now.AddMinutes(2), group.PK, ZGuid.Empty, "1");

			var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);
			AssertEquals(2m, extractedData.First().YValue);
		}

		public void TestExtractor_MultipleSeries_Order1()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "DEFL";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			var component2 = Factory.NewWithValidTestData<BMComponent>();
			component2.FC_FS_System = system.PK;

			var query = MENTTestHelper.CreateQuery(Factory, "BANBANNAH");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Flames", query);

			var rgColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.ReleaseGroup, true);
			rgColumn.Sequence = 1;
			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 2;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group.PK, component2.PK, "1");

			var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);
			AssertEquals(true, extractedData.Any(r => r.YValue == 2));
			AssertEquals(true, extractedData.Any(r => r.YValue == 1));
		}

		public void TestExtractor_MultipleSeries_Order2()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DEFL";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "PKADL";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			var component2 = Factory.NewWithValidTestData<BMComponent>();
			component2.FC_FS_System = system.PK;

			var query = MENTTestHelper.CreateQuery(Factory, "BANBANNAH");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Flames", query);

			var rgColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.ReleaseGroup, true);
			rgColumn.Sequence = 2;
			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), group2.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(5), group2.PK, component1.PK, "1");

			var extractedData = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 4 }), 0);
			AssertEquals(true, extractedData.Any(r => r.YValue == 2));
			AssertEquals(3, extractedData.Where(r => r.YValue == 1).Count());
		}

		[TestDate(2015, 3, 30, 15, 12, 9)]
		public void TestExtractor_MultipleSeries_DateIncluded()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DEFL";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "PKADL";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			var component2 = Factory.NewWithValidTestData<BMComponent>();
			component2.FC_FS_System = system.PK;

			var query = MENTTestHelper.CreateQuery(Factory, "BANBANNAH");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Flames", query);

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;
			var dateColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.DateTime, true);
			dateColumn.Sequence = 3;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), group2.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(5), group2.PK, component1.PK, "1");

			var extractedData = MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 });
		}

		public void TestExtractor_MultipleSeries_DecimalIncluded()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DEFL";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "PKADL";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			var component2 = Factory.NewWithValidTestData<BMComponent>();
			component2.FC_FS_System = system.PK;

			var query = MENTTestHelper.CreateQuery(Factory, "BANBANNAH");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Flames", query);

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;
			var decimalColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Score, true);
			decimalColumn.Selected = true;
			decimalColumn.Sequence = 3;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), group2.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(5), group2.PK, component1.PK, "1");

			var extractedData = MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 3 });
		}

		#endregion

		#region Aggregation Type

		public void TestExtractor_Sum_SingleSeries()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DNT";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "PAK";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			component1.FC_Name = "AAA";
			var component2 = Factory.NewWithValidTestData<BMComponent>();
			component2.FC_FS_System = system.PK;
			component2.FC_Name = "BBB";

			var query = MENTTestHelper.CreateQuery(Factory, "KURT");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), group1.PK, component2.PK, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(5), group1.PK, component2.PK, "3");

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);

			AssertEquals(7m, rows[0].YValue);
			AssertEquals(15m, rows[1].YValue);
		}

		public void TestExtractor_Sum_MultipleSeries()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DNT";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "PAK";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			component1.FC_Name = "AAA";
			var component2 = Factory.NewWithValidTestData<BMComponent>();
			component2.FC_FS_System = system.PK;
			component2.FC_Name = "BBB";

			var query = MENTTestHelper.CreateQuery(Factory, "KURT");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;
			var rgColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.ReleaseGroup);
			rgColumn.Selected = true;
			rgColumn.Sequence = 2;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), group2.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 23, ZDateTime.Now.AddMinutes(5), group2.PK, component1.PK, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(6), group2.PK, component2.PK, "1");

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 4 }), 0);

			AssertEquals(7m, rows[0].YValue);
			AssertEquals(5m, rows[1].YValue);
			AssertEquals(28m, rows[2].YValue);
			AssertEquals(5m, rows[3].YValue);
		}

		public void TestExtractor_Count_Score_MultipleSeries()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DNT";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			component1.FC_Name = "AAA";

			var query = MENTTestHelper.CreateQuery(Factory, "KURT");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, collectionColumn: MENTColumns.Codes.Score);

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;
			var rgColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.ReleaseGroup);
			rgColumn.Selected = true;
			rgColumn.Sequence = 2;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component1.PK, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), group1.PK, component1.PK, "4");

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

			AssertEquals(4m, rows[0].YValue);
		}

		#endregion

		#region Null Values

		public void TestExtractor_InvalidColumns()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "KURT");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, collectionColumn: MENTColumns.Codes.Score);

			var attributeColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.AttributeValue, true);
			attributeColumn.Sequence = 1;
			attributeColumn.ColumnType = "DEC";

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), ZGuid.Empty, ZGuid.Empty, "4");

			try
			{
				var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);
			}
			catch (Exception)
			{
				AssertEquals("InvalidColumnValueCast", ErrorReporter.LastKeyReported);
			}

			ErrorReporter.Clear();
		}

		public void TestInstantaneousExtraction_WithDoubleTypeScore()
		{
			// Forcing Score to have float(double) type. Real example: ROUND(e.Value * 24, 3) * (1.0 + P9_EstimateVariationFactor) * 0.5  StdEstimate
			var extraction = MENTTestHelper.CreateInstantaneousExtraction(Factory, "Extraction", "select CAST(0.33 AS float) Score, null ReleaseGroup, null Component, '' AttributeValue, '' Staff");

			var series = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.Score);
			series.Selected = true;
			series.Sequence = 1;

			Factory.Save();

			var results = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);
			AssertEquals("GIVEN instantaneous-extraction with score-series selected WHEN extracting score with double-type SHOULD not raise InvalidColumnValueCast exception and return the correct value (0.33)", "0.33", results[0].XSeriesString);
		}

		public void TestExtractor_NoSeries_NoCategories()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "KURT");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, collectionColumn: MENTColumns.Codes.Score);

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), ZGuid.Empty, ZGuid.Empty, "4");

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

			AssertEquals(4m, rows.First().YValue);
			AssertEquals(string.Empty, rows.First().XSeriesString);
		}

		public void TestExtractor_NullCoalesce_NoCategories()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "Gran");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "FlameBoy", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), ZGuid.Empty, ZGuid.Empty, "4");

			var extractedData = MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 });
		}

		public void TestExtractor_NullCoalesce_WithRounding()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "Gran");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "FlameBoy", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;
			var scoreSeriesColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Score, true);
			scoreSeriesColumn.Sequence = 2;

			var scoreColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.Score, true);
			scoreColumn.ColumnFunction.FunctionType = ColumnFunctionTypes.Codes.Round;
			scoreColumn.ColumnFunction.Parameter1 = 10;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 6, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 7, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 8, ZDateTime.Now.AddMinutes(4), ZGuid.Empty, ZGuid.Empty, "4");

			var results = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 4 }), 0);
			AssertEquals(false, results.All(r => string.IsNullOrEmpty(r.XSeriesString)));
			AssertEquals(true, results.All(r => r.XCategoryString == "0"));
		}

		public void TestExtractor_NullCoalesce_StaffName()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "Quattro");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "DanSmith", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XAC";
			staff.GS_FullName = "XWing@Alliciousness";

			var staffColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.Staff);
			staffColumn.Selected = true;
			staffColumn.Sequence = 1;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), ZGuid.Empty, ZGuid.Empty, "1", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 6, ZDateTime.Now.AddMinutes(2), ZGuid.Empty, ZGuid.Empty, "2", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 7, ZDateTime.Now.AddMinutes(3), ZGuid.Empty, ZGuid.Empty, "3", staff.GS_Code);
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 8, ZDateTime.Now.AddMinutes(4), ZGuid.Empty, ZGuid.Empty, "4", staff.GS_Code);

			var results = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);
			AssertEquals("XWing@Alliciousness", results[0].XSeriesString);
			AssertEquals(26m, results[0].YValue);
		}

		public void TestExtractor_NoResults()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "Quattro");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "DanSmith", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			Factory.Save();

			var extractor = new MENTAgedScoreExtractor(extraction, new MENTTestHelper.TestExtractorFactoryProvider());

			var extractorResult = extractor.Extract();
			var extractionResults = extractorResult.ExtractionResults.ToArray();

			AssertEquals(1, extractionResults.Length);
			AssertEquals(0, extractionResults.Single().Results.Count);
		}

		#endregion

		#region Rounding

		public void TestExtractor_RoundScoreToNearest10()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "GAR";

			var query = MENTTestHelper.CreateQuery(Factory, "CHARLES");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Flame on", query, collectionColumn: MENTColumns.Codes.Score);

			var scoreColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.Score, true);
			scoreColumn.ColumnFunction.FunctionType = ColumnFunctionTypes.Codes.Round;
			scoreColumn.ColumnFunction.Parameter1 = 10;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 0, ZDateTime.Now.AddMinutes(1), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1.1m, ZDateTime.Now.AddMinutes(2), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 9.9m, ZDateTime.Now.AddMinutes(3), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 10, ZDateTime.Now.AddMinutes(4), group1.PK, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 10.1m, ZDateTime.Now.AddMinutes(5), group1.PK, ZGuid.Empty, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.1m, ZDateTime.Now.AddMinutes(6), group1.PK, ZGuid.Empty, "3");

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0);  //"Without rounding there would be 6 categories because they are all different values"

			AssertEquals(3m, rows[0].YValue);
			AssertEquals(3m, rows[1].YValue);
		}

		public void TestExtractor_RoundScoreToNearest1000()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "GAR";

			var query = MENTTestHelper.CreateQuery(Factory, "CHARLES");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Flame on", query, collectionColumn: MENTColumns.Codes.Score);

			var scoreColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.Score, true);
			scoreColumn.ColumnFunction.FunctionType = ColumnFunctionTypes.Codes.Round;
			scoreColumn.ColumnFunction.Parameter1 = 1000;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 0, ZDateTime.Now.AddMinutes(1), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1.1m, ZDateTime.Now.AddMinutes(2), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 9.9m, ZDateTime.Now.AddMinutes(3), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 10, ZDateTime.Now.AddMinutes(4), group1.PK, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 10.1m, ZDateTime.Now.AddMinutes(5), group1.PK, ZGuid.Empty, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.1m, ZDateTime.Now.AddMinutes(6), group1.PK, ZGuid.Empty, "3");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 999.9m, ZDateTime.Now.AddMinutes(7), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1000m, ZDateTime.Now.AddMinutes(8), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1000.01m, ZDateTime.Now.AddMinutes(9), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1000, ZDateTime.Now.AddMinutes(10), group1.PK, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1000.1m, ZDateTime.Now.AddMinutes(11), group1.PK, ZGuid.Empty, "3");

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0); //"Because of rounding every value is rounded into the two categories. Without it there would be 10 categories (1000m and 1000 combine as the same)"

			AssertEquals(7m, rows[0].YValue);
			AssertEquals(4m, rows[1].YValue);
		}

		public void TestExtractor_RoundScoreToNearest10_SumUsesNonRound()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "EXS";

			var query = MENTTestHelper.CreateQuery(Factory, "GlueFactry");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Greetings from ostrich zone 18", query, collectionColumn: MENTColumns.Codes.Score, aggregationType: ExtractionTypes.Codes.Sum);

			var scoreColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.Score, true);
			scoreColumn.ColumnFunction.FunctionType = ColumnFunctionTypes.Codes.Round;
			scoreColumn.ColumnFunction.Parameter1 = 10;

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 0, ZDateTime.Now.AddMinutes(1), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1.1m, ZDateTime.Now.AddMinutes(2), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 9.9m, ZDateTime.Now.AddMinutes(3), group1.PK, ZGuid.Empty, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 1.3m, ZDateTime.Now.AddMinutes(4), group1.PK, ZGuid.Empty, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 10.1m, ZDateTime.Now.AddMinutes(5), group1.PK, ZGuid.Empty, "2");

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 2 }), 0); //"Values are rounded for Categories"

			AssertEquals(12.3m, rows[0].YValue);
			AssertEquals("Rounded value is not used for YValue", 10.1m, rows[1].YValue);
		}

		#endregion

		#region Instantaneous Extraction

		public void TestInstantaneousQuery_SingleSeries_SingleResult()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "NEX";

			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var buffer = VisualBoardsTestHelper.CreateBuffer(system);
			var releaseGroup = VisualBoardsTestHelper.CreateReleaseGroup(system, group);

			var header = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = VisualBoardsTestHelper.CreateProcessHeader(header, buffer, "Grapeman is a beast. He has a pencil to write with", ZDateTime.Now);
			workflow1.FH_GG_ReleaseGroup = group.PK;

			Factory.Save();

			var query = MENTTestHelper.CreateQuery(Factory, "Mousecop");
			query.MAQ_SqlText = "select 64.98 as Score, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup as ReleaseGroup, '' as AttributeValue, '' as Staff from dbo.ProcessHeader where FH_FH_ParentHeader is not null";

			var extraction = MENTTestHelper.CreateExtraction(Factory, "The player formerly known as", query, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true, addScoreCategoryColumn: true);

			var rgColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.ReleaseGroup);
			rgColumn.Selected = true;
			rgColumn.Sequence = 1;

			Factory.Save();

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

			AssertEquals(64.98m, rows[0].YValue);
			AssertEquals("NEX", rows[0].XSeriesString);
		}

		public void TestInstantaneousQuery_NullScore_NoExceptions()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var buffer = VisualBoardsTestHelper.CreateBuffer(system);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NUE", "Number Eight");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BRP", "Burp");

			var header = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateProcessHeaderAndTask(header, buffer, 0, completionStatement: "So! Do you like...stuff?", taskLowEst: ZDateTime.Invalid, assignedResource: "NUE");
			var workflow2 = VisualBoardsTestHelper.CreateProcessHeaderAndTask(header, buffer, 0, completionStatement: "I'm learnding!", taskLowEst: new ZInt(30).GetDateTimeFromMinutes(), assignedResource: "BRP");
			var workflow3 = VisualBoardsTestHelper.CreateProcessHeaderAndTask(header, buffer, 0, completionStatement: "I choo choo choose you!", taskLowEst: ZDateTime.Invalid, assignedResource: "NUE");
			var workflow4 = VisualBoardsTestHelper.CreateProcessHeaderAndTask(header, buffer, 0, completionStatement: "Hi Super Nintendo Chalmers!", taskLowEst: new ZInt(30).GetDateTimeFromMinutes(), assignedResource: "BRP");

			var tag = VisualBoardsTestHelper.CreateTagDefinition(Factory, "BOB", "Baby on Board");
			var tagitude1 = VisualBoardsTestHelper.CreateTagMagnitude(tag, "WFB", "Wiggum Forever, Barney Never");
			var tagitude2 = VisualBoardsTestHelper.CreateTagMagnitude(tag, "BFW", "Barney Forever, Wiggum Never");
			BMSTestHelper.CreateTagLink(workflow, tagitude1);
			BMSTestHelper.CreateTagLink(workflow2, tagitude2);
			BMSTestHelper.CreateTagLink(workflow3, tagitude1);
			BMSTestHelper.CreateTagLink(workflow4, tagitude2);

			Factory.Save();

			var query = MENTTestHelper.CreateQuery(Factory, "RAFEWIGHAM");
			query.MAQ_SqlText = @"select SUM(((ROUND(duration.Value * 24, 3) * P9_EstimateVariationFactor) + ROUND(duration.Value * 24, 3)) / CAST(2 AS FLOAT)) As Score,
				null as Component, null as ReleaseGroup, TGM_Code as AttributeValue, P9_GS_NKAssignedStaffMember as Staff 
				FROM dbo.ProcessTasks
				JOIN dbo.ProcessHeader ON FH_PK = P9_FH_ProcessHeader
				CROSS APPLY dbo.GetDurationAsFloat(P9_EstDuration) duration 
				outer apply 
				(
					select top 1 *
					from dbo.TagLink
					join dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
					join dbo.TagDefinition on TGM_TGD_Tag = TGD_PK
					where
					1=1

					AND TGD_Code = 'BOB'
					AND TGM_Code in ('WFB','BFW')
					AND TGL_ParentId = FH_PK
					and TGL_ParentTableCode = 'FH'
					order by TGL_SystemCreateTimeUtc
				) as MostRecentAddedTag
				GROUP BY TGM_Code, P9_GS_NKAssignedStaffMember";
			query.MAQ_IsActive = true;

			var extraction = MENTTestHelper.CreateExtraction(Factory, "Apu de Beaumarchais", query,
				aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None,
				isInstantaneous: true, addScoreCategoryColumn: true);

			var scrSColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.Score);
			scrSColumn.Selected = true;
			scrSColumn.Sequence = 0;

			var abvSColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			abvSColumn.Selected = true;
			abvSColumn.Sequence = 1;

			var stfSColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.Staff);
			stfSColumn.Selected = true;
			stfSColumn.Sequence = 2;

			var scrCColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.Score);
			scrCColumn.Selected = true;
			scrCColumn.Sequence = 0;

			var abvCColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.AttributeValue);
			abvCColumn.Selected = true;
			abvCColumn.Sequence = 1;

			var stfCColumn = extraction.CategoryColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.Staff);
			stfCColumn.Selected = true;
			stfCColumn.Sequence = 2;

			Factory.Save();

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new int[] { 0 }), 0);
			AssertEquals(false, rows.Any());

			AssertMultilineASCIIEquals("",
@"The query returned a null score. Please correct the query. Details:

Series Columns:
Score: NULL
Attribute Value: WFB
Staff: Number Eight

Category Columns:
Score: NULL
Attribute Value: WFB
Staff: Number Eight", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestInstantaneousQuery_SingleSeries_SingleResultNonDecimal()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "NEX";

			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var buffer = VisualBoardsTestHelper.CreateBuffer(system);
			var releaseGroup = VisualBoardsTestHelper.CreateReleaseGroup(system, group);

			var header = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = VisualBoardsTestHelper.CreateProcessHeader(header, buffer, "Grapeman is a beast. He has a pencil to write with", ZDateTime.Now);
			workflow1.FH_GG_ReleaseGroup = group.PK;

			Factory.Save();

			var query = MENTTestHelper.CreateQuery(Factory, "Mousecop");
			query.MAQ_SqlText = "select 64 as Score, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup as ReleaseGroup, '' as AttributeValue, '' as Staff from dbo.ProcessHeader where FH_FH_ParentHeader is not null";

			var extraction = MENTTestHelper.CreateExtraction(Factory, "The player formerly known as", query, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true, addScoreCategoryColumn: true);

			var rgColumn = extraction.SeriesColumns.Cast<SQLColumnSpecification>().First(c => c.Code == MENTColumns.Codes.ReleaseGroup);
			rgColumn.Selected = true;
			rgColumn.Sequence = 1;

			Factory.Save();

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

			AssertEquals(64m, rows[0].YValue);
			AssertEquals("NEX", rows[0].XSeriesString);
		}

		public void TestInstantaneousQuery_SingleSeries_SingleResultNonDecimalAttributeValue()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "NEX";

			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var buffer = VisualBoardsTestHelper.CreateBuffer(system);
			var releaseGroup = VisualBoardsTestHelper.CreateReleaseGroup(system, group);

			var header = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = VisualBoardsTestHelper.CreateProcessHeader(header, buffer, "That was a silly move", ZDateTime.Now);
			workflow1.FH_GG_ReleaseGroup = group.PK;

			Factory.Save();

			var query = MENTTestHelper.CreateQuery(Factory, "Ninja");
			query.MAQ_SqlText = "select 64 as Score, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup as ReleaseGroup, 1 as AttributeValue, '' as Staff from dbo.ProcessHeader where FH_FH_ParentHeader is not null";

			var extraction = MENTTestHelper.CreateExtraction(Factory, "hashtag", query, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true);

			var attributeColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.AttributeValue, true);

			var rgColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.ReleaseGroup, true);
			rgColumn.Sequence = 1;

			Factory.Save();

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

			AssertEquals(64m, rows[0].YValue);
			AssertEquals("NEX", rows[0].XSeriesString);
		}

		public void TestInstantaneousQuery_MultipleSeries_MultipleResults()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "EXS";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "NXS";

			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var buffer = VisualBoardsTestHelper.CreateBuffer(system);
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var releaseGroup = VisualBoardsTestHelper.CreateReleaseGroup(system, group1);

			var header = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = VisualBoardsTestHelper.CreateProcessHeader(header, buffer, "Pineapples killed my son", ZDateTime.Now);
			workflow1.FH_GG_ReleaseGroup = group1.PK;
			workflow1.FH_VoteUpDownAmount = 1;
			var workflow2 = VisualBoardsTestHelper.CreateProcessHeader(header, bucket, "Alberqueueue is difficult to spell", ZDateTime.Now);
			workflow2.FH_GG_ReleaseGroup = group1.PK;
			workflow2.FH_VoteUpDownAmount = 2;
			var workflow3 = VisualBoardsTestHelper.CreateProcessHeader(header, buffer, "Liverpool has a pool with liver in it", ZDateTime.Now);
			workflow3.FH_GG_ReleaseGroup = group2.PK;
			workflow3.FH_VoteUpDownAmount = 3;
			var workflow4 = VisualBoardsTestHelper.CreateProcessHeader(header, bucket, "Imagine a mind with a turtle in it", ZDateTime.Now);
			workflow4.FH_GG_ReleaseGroup = group2.PK;
			workflow4.FH_VoteUpDownAmount = 4;

			Factory.Save();

			var query = MENTTestHelper.CreateQuery(Factory, "DanSmith");
			query.MAQ_SqlText = "select 64.98 * FH_VoteUpDownAmount as Score, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup as ReleaseGroup, '' as AttributeValue, '' as Staff FROM dbo.ProcessHeader where FH_FH_ParentHeader is not null";

			var extraction = MENTTestHelper.CreateExtraction(Factory, "Javaris Jamar Javarison-Lamar", query, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true, addScoreCategoryColumn: true);

			var releaseGroupColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.ReleaseGroup, true);
			releaseGroupColumn.Sequence = 1;

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 2;

			Factory.Save();

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 4 }), 0);

			rows.OrderBy(r => r.YValue);

			AssertEquals(64.98m, rows[0].YValue);
			AssertEquals("EXS-buffer", rows[0].XSeriesString);
			AssertEquals(129.96m, rows[1].YValue);
			AssertEquals("EXS-bucket", rows[1].XSeriesString);
			AssertEquals(194.94m, rows[2].YValue);
			AssertEquals("NXS-buffer", rows[2].XSeriesString);
			AssertEquals(259.92m, rows[3].YValue);
			AssertEquals("NXS-bucket", rows[3].XSeriesString);
		}

		public void TestInstantaneousQuery_AcceptabilityBandQuery()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var secondBuffer = BMSTestHelper.CreateBuffer(system, name: "bufferina");

			var job = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(job, "Leoz", currentComponent: buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(job, "Shower Handel", currentComponent: buffer);
			var workflow3 = BMSTestHelper.CreateWorkflow(job, "Billings Clyde", currentComponent: buffer);

			var workflow4 = BMSTestHelper.CreateWorkflow(job, "Power Pack to feed your nose", currentComponent: secondBuffer);
			var workflow5 = BMSTestHelper.CreateWorkflow(job, "Mouseman", currentComponent: secondBuffer);
			var workflow6 = BMSTestHelper.CreateWorkflow(job, "Central station aint close", currentComponent: secondBuffer);

			var queryToRun = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			var sqlToNotRun = @"select 69 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "DanSmith";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sqlToNotRun;

			MENTTestHelper.CreateRelatedBand(queryToRun, AcceptabilityBandTypes.Codes.Count, buffer.PK, sqlToNotRun);

			Factory.Save();

			var extraction = MENTTestHelper.CreateExtraction(Factory, "Beezer Twelve Washingbeard", queryToRun, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true, addScoreCategoryColumn: true);

			var releaseGroupColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.ReleaseGroup, true);
			releaseGroupColumn.Sequence = 1;

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 2;

			Factory.Save();

			var rows = MENTTestHelper.GetRowsFromExtractorResult(MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 1, new[] { 1 }), 0);

			AssertEquals(4.0m, rows[0].YValue);
			AssertEquals("NONE-buffer", rows[0].XSeriesString);
		}

		#endregion

		#region Performance

		public void TestQueryHeading_Instantaneous()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "Mousecop");
			query.MAQ_SqlText = "select 64.98 as Score, FH_FC_CurrentComponent as Component, FH_GG_ReleaseGroup as ReleaseGroup, '' as AttributeValue, '' as Staff from dbo.ProcessHeader where FH_FH_ParentHeader is not null";

			var extraction = MENTTestHelper.CreateExtraction(Factory, "The player formerly known as", query, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true, addScoreCategoryColumn: true);

			Factory.Save();

			MENTTestHelper.CreateExtractor(extraction).Extract();
			var command = SqlEventTracker.Instance.LastSqlQuery;
			AssertNotNull(command);

			AssertContains("The query should include the database name and MENT Query name so that we can track down bad queries that appear in Kibana. SAD!", $@"
-- Database: {TestConnection.CurrentDatabase}
-- Query: Mousecop", command);
		}

		public void TestQueryHeading_NonInstantaneous()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var query = MENTTestHelper.CreateQuery(Factory, "KURT");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), config.ReleaseGroup.PK, config.Bucket.PK, "1");

			MENTTestHelper.CreateExtractor(extraction).Extract();
			var command = SqlEventTracker.Instance.LastSqlQuery;
			AssertNotNull("We should succesfully execute our extractor query, however...", command);

			AssertContains("The query should include the database name and MENT Query name so that we can track down bad queries that appear in Kibana. SAD!", $@"
-- Database: {TestConnection.CurrentDatabase}
-- Query: KURT", command);
		}

		#endregion

		#region AdditionalExtractions

		public void TestAdditionalExtractions()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "DNT";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "PAK";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = Factory.NewWithValidTestData<BMComponent>();
			component1.FC_FS_System = system.PK;
			component1.FC_Name = "AAA";
			var component2 = Factory.NewWithValidTestData<BMComponent>();
			component2.FC_FS_System = system.PK;
			component2.FC_Name = "BBB";

			var query = MENTTestHelper.CreateQuery(Factory, "KURT");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Banana Spirit", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);

			var componentColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.Component, true);
			componentColumn.Sequence = 1;
			var rgColumn = MENTTestHelper.GetSqlColumnFromCollection(extraction.SeriesColumns, MENTColumns.Codes.ReleaseGroup, true);
			rgColumn.Sequence = 2;

			var additionalExtraction = MENTTestHelper.CreateExtraction(Factory, "Flying Mango", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);
			var additionalComponentColumn = MENTTestHelper.GetSqlColumnFromCollection(additionalExtraction.SeriesColumns, MENTColumns.Codes.Component, true);
			additionalComponentColumn.Sequence = 1;

			MENTTestHelper.CreateAdditionalExtractionLink(extraction, query, additionalExtraction);

			Factory.Save();

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(1), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2, ZDateTime.Now.AddMinutes(2), group1.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(3), group1.PK, component2.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(4), group2.PK, component1.PK, "1");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 23, ZDateTime.Now.AddMinutes(5), group2.PK, component1.PK, "2");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 5, ZDateTime.Now.AddMinutes(6), group2.PK, component2.PK, "1");

			var extractorResult = MENTTestHelper.CreateRunAndAssertExtractorResults(extraction, 2, new[] { 2, 4 });

			var firstRows = MENTTestHelper.GetRowsFromExtractorResult(extractorResult, 0);

			AssertEquals("Banana Spirit", extractorResult.ExtractionResults.ToArray()[0].Name);

			AssertEquals(7m, firstRows[0].YValue);
			AssertEquals(5m, firstRows[1].YValue);
			AssertEquals(28m, firstRows[2].YValue);
			AssertEquals(5m, firstRows[3].YValue);

			var secondRows = MENTTestHelper.GetRowsFromExtractorResult(extractorResult, 1);

			AssertEquals("Flying Mango", extractorResult.ExtractionResults.ToArray()[1].Name);

			AssertEquals(35m, secondRows[0].YValue);
			AssertEquals(10m, secondRows[1].YValue);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			MENTTestHelper.ClearMENTTables();
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
