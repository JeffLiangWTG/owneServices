using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.PAVE.MENT.Business.ServiceTasks;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OxyPlot;

namespace Enterprise.PAVE.MENT.Business.Test
{
	public static class MENTTestHelper
	{
		#region Create New Objects

		#region Base MENT Objects

		public static MENTAgedScoreQuery CreateQuery(BusinessObjectFactory factory, string code, ZDateTime? nextRunTime = null, string sql = "", bool save = true)
		{
			var query = factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query.MAQ_Code = code;

			if (nextRunTime != null)
			{
				query.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = nextRunTime.Value;
			}
			query.MAQ_SqlText = sql;

			if (save)
			{
				factory.Save();
			}

			return query;
		}

		public static string CreateInsertSql(string score = "1", string releaseGroup = "null", string component = "null", string attributeValue = "''", string staff = "''", string table = MENTAgedScoreQuerySchema.Constants.TableName)
		{
			return string.Format("select {0} as score, {1} as releaseGroup, {2} as component, {3} as attributeValue, {4} as staff from {5}",
				/*0*/ score,
				/*1*/ releaseGroup,
				/*2*/ component,
				/*3*/ attributeValue,
				/*4*/ staff,
				/*5*/ table);
		}

		public static BMComponentAcceptabilityBand CreateRelatedBand(MENTAgedScoreQuery query, string type, ZGuid componentPK, string sql)
		{
			var band = query.Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = type;
			band.BAB_SqlText = sql;
			band.BAB_FC_Component = componentPK;

			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			return band;
		}

		public static BMBoardSection CreateMENTBoardSection(BusinessObjectFactory factory, BMBoard board, ZGuid extractionPK)
		{
			if (board == null)
			{
				board = factory.NewWithValidTestData<BMBoard>();
			}

			var section = board.Sections.AddNew();
			section.MS_SectionType = MENTConstants.ChartSectionType;
			((ChartSectionConfiguration)section.Configuration).ExtractionPK = extractionPK;
			var defaultVisualisation = ((ChartSectionConfiguration)section.Configuration).Extraction.DefaultVisualisation;

			return section;
		}

		public static MENTAgedScoreExtraction CreateInstantaneousExtraction(BusinessObjectFactory factory, string name, string sql)
		{
			var extraction = CreateExtraction(factory, name, aggregationType: ExtractionTypes.Codes.None, collectionColumn: MENTColumns.Codes.None, isInstantaneous: true, addScoreCategoryColumn: true);
			extraction.RelatedQuery.MAQ_SqlText = sql;

			return extraction;
		}

		public static MENTAgedScoreExtraction CreateExtraction(BusinessObjectFactory factory, string name, MENTAgedScoreQuery query = null, string queryCode = "TESTQUEERY", string aggregationType = ExtractionTypes.Codes.Count, string collectionColumn = MENTColumns.Codes.AttributeValue, bool isInstantaneous = false, bool addScoreCategoryColumn = false, bool save = true)
		{
			if (query == null)
			{
				query = CreateQuery(factory, queryCode, save: save);
			}

			var extraction = factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			extraction.MEX_Name = name;
			extraction.MEX_MAQ = query.PK;
			extraction.AggregationType = aggregationType;
			extraction.CollectionColumn = collectionColumn;
			extraction.IsInstantaneous = isInstantaneous;

			if (addScoreCategoryColumn)
			{
				GetSqlColumnFromCollection(extraction.CategoryColumns, MENTColumns.Codes.Score, true);
			}

			if (save)
			{
				factory.Save();
			}

			return extraction;
		}

		public static AdditionalExtractionLink CreateAdditionalExtractionLink(MENTAgedScoreExtraction extractionToAddTo, MENTAgedScoreQuery queryLink, MENTAgedScoreExtraction extractionLink)
		{
			var link = extractionToAddTo.AdditionalExtractions.AddNew();
			link.QueryPK = queryLink.PK;
			link.ExtractionPK = extractionLink.PK;
			return link;
		}

		public static MENTDataRow CreateMENTDataRow(ZString series, IZType[] category, decimal yValue)
		{
			return CreateMENTDataRow(new IZType[] { series }, category, yValue);
		}

		public static MENTDataRow CreateMENTDataRow(IZType[] series, IZType[] category, decimal yValue)
		{
			var seriesSegmentDefinitions = series.Select((item, index) => new RowSegmentDefinitionAndData(index, item));
			var categorySegmentDefinitions = category.Select((item, index) => new RowSegmentDefinitionAndData(index, item));

			return new MENTDataRow(seriesSegmentDefinitions.ToArray(), categorySegmentDefinitions.ToArray(), yValue);
		}

		public static MENTDataRow CreateMENTDataRow(ZString series, ZString category, decimal yValue)
		{
			var seriesSegmentDefinition = new RowSegmentDefinitionAndData(0, series);
			var categorySegmentDefinition = new RowSegmentDefinitionAndData(0, category);

			return new MENTDataRow(new RowSegmentDefinitionAndData[] { seriesSegmentDefinition }, new RowSegmentDefinitionAndData[] { categorySegmentDefinition }, yValue);
		}

		public static MENTAgedScoreExtractor CreateExtractor(MENTAgedScoreExtraction extraction)
		{
			return new MENTAgedScoreExtractor(extraction, new TestExtractorFactoryProvider());
		}

		#region Insert Directly to DB

		public static void InsertTestDataRow(string mAQ_Code, decimal score, ZDateTime dateTimeOfCollection, ZGuid releaseGroupPK, ZGuid componentPK, string attributeValue, string staffCode)
		{
			var componentValue = componentPK == ZGuid.Empty ? "NULL" : string.Format("'{0}'", componentPK.ToString());
			var releaseGroupValue = releaseGroupPK == ZGuid.Empty ? "NULL" : string.Format("'{0}'", releaseGroupPK.ToString());

			var insertSql = string.Format(InsertSqlForMENTAgedScoreMetric,
				/*0*/ MENTAgedScoreMetricSchema.Constants.TableName,
				/*1*/ MENTAgedScoreMetricSchema.Constants.MAS_MAQ_NKCode,
				/*2*/ MENTAgedScoreMetricSchema.Constants.MAS_AgedScoreValue,
				/*3*/ MENTAgedScoreMetricSchema.Constants.MAS_TimeRecordedUtc,
				/*4*/ MENTAgedScoreMetricSchema.Constants.MAS_GG_ReleaseGroup,
				/*5*/ MENTAgedScoreMetricSchema.Constants.MAS_FC_Component,
				/*6*/ MENTAgedScoreMetricSchema.Constants.MAS_AttributeValue,
				/*7*/ mAQ_Code,
				/*8*/ score,
				/*9*/ releaseGroupValue,
				/*10*/ componentValue,
				/*11*/ attributeValue,
				/*12*/ dateTimeOfCollection.SqlFormat,
				/*13*/ MENTAgedScoreMetricSchema.Constants.MAS_GS_NKStaffCode,
				/*14*/ staffCode
				);

			Db.Connection.ExecuteNonQuery(insertSql); // Work in progress will do eventually. No DTO is currently available
		}

		public static void InsertTestDataRow(string mAQ_Code, decimal score, ZDateTime dateTimeOfCollection, ZGuid releaseGroupPK, ZGuid componentPK, string attributeValue)
		{
			InsertTestDataRow(mAQ_Code, score, dateTimeOfCollection, releaseGroupPK, componentPK, attributeValue, string.Empty);
		}

		public static List<int> InsertTenTestMetrics(MENTAgedScoreQuery query)
		{
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 25.0m, ZDateTime.Now, ZGuid.Empty, ZGuid.Empty, "A9018");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 25.0m, ZDateTime.Now, ZGuid.Empty, ZGuid.Empty, "A9019");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 25.0m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "A9021");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 69.0m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "A9024");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2.1m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "A9027");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2.2m, ZDateTime.Now.AddDays(-1), ZGuid.Empty, ZGuid.Empty, "A9028");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-2), ZGuid.Empty, ZGuid.Empty, "A9022");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 19.0m, ZDateTime.Now.AddDays(-2), ZGuid.Empty, ZGuid.Empty, "A9025");

			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 16.0m, ZDateTime.Now.AddDays(-3), ZGuid.Empty, ZGuid.Empty, "A9023");
			MENTTestHelper.InsertTestDataRow(query.MAQ_Code, 2.0m, ZDateTime.Now.AddDays(-3), ZGuid.Empty, ZGuid.Empty, "A9026");

			var today = GetMetricsThisManyDaysOld(query.MAQ_Code, 0);
			var oneDayOld = GetMetricsThisManyDaysOld(query.MAQ_Code, 1);
			var twoDayOld = GetMetricsThisManyDaysOld(query.MAQ_Code, 2);
			var threeDayOld = GetMetricsThisManyDaysOld(query.MAQ_Code, 3);

			var perDayMetricQuantities = new List<int>();
			perDayMetricQuantities.Add(today);
			perDayMetricQuantities.Add(oneDayOld);
			perDayMetricQuantities.Add(twoDayOld);
			perDayMetricQuantities.Add(threeDayOld);
			var total = GetMetricsForThisCode(query.MAQ_Code);

			return perDayMetricQuantities;
		}

		public static (string expectedLog, int currentDayQueries) CreateManyTestMetricsAndGetExpectedLogs(BusinessObjectFactory factory, string queryName, int queriesToMake, int purgeDays, int purgeAllBut)
		{
			var expectedPurgeLogs = new ZStringBuilder();
			var currentDayQueries = 0;

			for (int i = 0; i < queriesToMake; i++)
			{
				var query = MENTTestHelper.CreateQuery(factory, queryName + i.ToString(), save: false);
				query.MAQ_PurgeDays = purgeDays;
				query.MAQ_PurgeAllButLatestQuantity = purgeAllBut;
				var perDayMetricQuantities = InsertTenTestMetrics(query);

				var purgeAllButCount = SetupLogs(perDayMetricQuantities, purgeAllBut);
				var purgeDaysCount = SetupLogs(perDayMetricQuantities, purgeDays);
				var purgeDaysCountAfterPurgeAllBut = purgeDaysCount - purgeAllButCount; // subtract here because purgeDaysCount is reduced by the number of purgeAllButCount - can't delete a row twice

				if (purgeAllButCount > 0)
				{
					expectedPurgeLogs.AppendLine($"BMP|MENT Data Purge: Purged {purgeAllButCount} records from {query.MAQ_Code} by PurgeAllButLatest Quantity.");
					if (purgeDaysCountAfterPurgeAllBut >= 0)
					{
						expectedPurgeLogs.AppendLine($"BMP|MENT Data Purge: Purged {purgeDaysCountAfterPurgeAllBut} records from {query.MAQ_Code} by Purge Days.");
					}
				}
				else if (purgeDaysCount > 0)
				{
					expectedPurgeLogs.AppendLine($"BMP|MENT Data Purge: Purged {purgeAllButCount} records from {query.MAQ_Code} by Purge Days.");
				}

				currentDayQueries += perDayMetricQuantities[0];
			}

			return (expectedPurgeLogs.ToString(), currentDayQueries);
		}

		static int SetupLogs(List<int> perDayMetricQuantities, int purgePeriod)
		{
			int subSetQuantity(int startDay) => perDayMetricQuantities.GetRange(startDay, perDayMetricQuantities.Count - 1).Sum();
			switch (purgePeriod)
			{
				case 1:
					return subSetQuantity(1);
				case 2:
					return subSetQuantity(2);
				case 3:
					return subSetQuantity(3);
				default:
					return subSetQuantity(4);
			}
		}

		#endregion

		#endregion

		#region Advanced MENT Objects

		public static MENTAgedScoreVisualisation CreateVisualisation(BusinessObjectFactory factory, string name, MENTAgedScoreExtraction extraction = null, string extractionName = "TESTEXXX", bool isNormalised = false, string queryCode = "TESTQUEERY", bool save = true)
		{
			var visualisation = factory.NewWithValidTestData<MENTAgedScoreVisualisation>();
			if (extraction == null)
			{
				extraction = visualisation.Extraction;
				extraction.AggregationType = "CNT";
				extraction.CollectionColumn = "ABV";
				extraction.IsInstantaneous = false;
				extraction.MEX_Name = extractionName;
				extraction.RelatedQuery.MAQ_Code = queryCode;
			}
			else
			{
				visualisation.MVI_MEX = extraction.PK;
			}

			visualisation.IsNormalised = isNormalised;

			if (save)
			{
				factory.Save();
			}

			return visualisation;
		}

		public static ChartSectionConfiguration CreateChartSectionConfiguration(BusinessObjectFactory factory, IBMBoardSection section = null, MENTAgedScoreExtraction extraction = null)
		{
			if (section == null)
			{
				section = factory.New<IBMBoardSection>();
			}

			var configuration = new ChartSectionConfiguration(section);

			if (extraction != null)
			{
				configuration.ExtractionPK = extraction.PK;
			}

			return configuration;
		}

		public static PlotModel CreateAndRunVisualiser(MENTAgedScoreVisualisation visualisation, MENTAgedScoreExtractorResult result)
		{
			var visualiser = new MENTAgedScoreVisualiser(visualisation);

			return visualiser.Visualise(result);
		}

		public static MENTAgedScoreExtractionResult GetSpecificExtractionResultFromExtractorResult(MENTAgedScoreExtractorResult result, int index)
		{
			return result.ExtractionResults.ToArray()[index];
		}

		public static MENTDataRow[] GetRowsFromExtractorResult(MENTAgedScoreExtractorResult result, int index)
		{
			var extractionResult = GetSpecificExtractionResultFromExtractorResult(result, index);

			return extractionResult.Results.ToArray();
		}

		public static void TurnAllSeriesAndCategoryColumnsOnForExtraction(MENTAgedScoreExtraction extraction)
		{
			foreach (SQLColumnSpecification column in extraction.SeriesColumns.Union(extraction.CategoryColumns))
			{
				column.Selected = true;
			}
		}

		public static SQLColumnSpecification GetSqlColumnFromCollection(SQLColumnSpecificationCollection columns, ZString code, ZBool select)
		{
			var column = columns.Cast<SQLColumnSpecification>().First(c => c.Code == code);
			column.Selected = select;
			return column;
		}

		#endregion

		#endregion

		#region Get Properties On Objects

		public static MENTAgedScoreExtractorResult CreateRunAndAssertExtractorResults(MENTAgedScoreExtraction extraction, int expectedExtractionResults, IEnumerable<int> expectedExtractionRows)
		{
			var extractor = CreateExtractor(extraction);

			var extractorResult = extractor.Extract();
			var results = extractorResult.ExtractionResults.ToArray();

			var sortedResults = results.Select(r => r.Results.Count).OrderBy(i => i).ToArray();
			Assertion.AssertArrayEqualsByElements(expectedExtractionRows.OrderBy(i => i).ToArray(), sortedResults);

			return extractorResult;
		}

		public static VisualisationColumnSpecification CreateAndAddVisualisationColumn(MENTAgedScoreVisualisation visualisation, VisualisationColumnSpecificationCollection collectionToAddTo, string column, string display, bool selected, int sequence)
		{
			var columnSpecification = new VisualisationColumnSpecification(visualisation) { Column = column, ColumnDisplay = display, Selected = selected, Sequence = sequence };
			collectionToAddTo.Add(columnSpecification);
			return columnSpecification;
		}

		public static PlotModel CreateAndRunVisualiser(MENTAgedScoreVisualisation visualisation, Collection<MENTDataRow> items, string resultName = "jilliumz")
		{
			var result = new MENTAgedScoreExtractorResult(new[] { new MENTAgedScoreExtractionResult(items, resultName) });

			return CreateAndRunVisualiser(visualisation, result);
		}

		public static int GetMetricsThisManyDaysOld(string code, int daysAgo)
		{
			var startDate = ZDateTime.UtcNow.AddDays(-1 * daysAgo).AddSeconds(1).SqlFormat;
			var endDate = ZDateTime.UtcNow.AddDays((-1 * daysAgo) - 1).SqlFormat;

			return (int)Db.Connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, MENTTestHelper.MENTTotalRowCountSql_InDateRange, code, startDate, endDate)); // We can't get this value with Factory or ZQuery
		}

		public static int GetMetricsForThisCode(string code)
		{
			return (int)Db.Connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, MENTTestHelper.MENTTotalRowCountSql_ForThisCode, code)); // We can't get this value with Factory or ZQuery
		}

		#endregion

		public static void ClearMENTTables()
		{
			TestCaseHelper.ClearTable("MENTAgedScoreMetric");
			TestCaseHelper.ClearTable("MENTAgedScoreQuery");
		}

		#region SqlQueries

		public const string InsertSqlForMENTAgedScoreMetric = @"
		INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {13}) VALUES ('{7}', {8}, '{12}', {9}, {10}, '{11}', '{14}')";

		public const string MENTRowCountForNameSql = @"SELECT count(*) FROM dbo.MENTAgedScoreMetric WHERE MAS_MAQ_NKCode = '{0}'";

		public const string MENTSumForNameSql = @"SELECT sum(MAS_AgedScoreValue) FROM dbo.MENTAgedScoreMetric WHERE MAS_MAQ_NKCode = '{0}'";

		public const string MENTTotalRowCountSql = @"SELECT count(*) FROM dbo.MENTAgedScoreMetric";

		public const string MENTTotalRowCountSql_ForThisCode = @"
			SELECT count(*)
			FROM dbo.MENTAgedScoreMetric
			WHERE MAS_MAQ_NKCode = '{0}'";

		public const string MENTTotalRowCountSql_InDateRange = @"
			SELECT count(*)
			FROM dbo.MENTAgedScoreMetric
			WHERE MAS_MAQ_NKCode = '{0}'
			AND MAS_TimeRecordedUtc < '{1}'
			AND MAS_TimeRecordedUtc > '{2}'";

		public const string MENTRowCountForComponentSql = @"
			SELECT count(*) FROM dbo.MENTAgedScoreMetric
			WHERE MAS_MAQ_NKCode = '{0}' AND
			MAS_FC_Component = '{1}'";

		public const string MENTRowCountForComponentAndReleaseGroupAndValueSql = @"
			SELECT count(*) FROM dbo.MENTAgedScoreMetric
			WHERE MAS_MAQ_NKCode = '{0}' AND
			MAS_FC_Component = '{1}' AND
			MAS_GG_ReleaseGroup = '{2}' AND
			MAS_AgedScoreValue = {3}";

		#endregion

		#region ServiceTaskMock

		#region MENTDataPurgeServiceTaskForTest

		public class MENTDataPurgeServiceTaskForTest : MENTDataPurgeServiceTask
		{
			public void Run()
			{
				ServiceLogger = new BufferManagementLogger();

				using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					RunTaskCore();
				}
			}

			public int BatchLimit_ForTest => base.BatchLimit;
		}

		#endregion

		#region MENTDataPurgeServiceTaskForTest_WithOnBatchProcessed

		public class MENTDataPurgeServiceTaskWithPerBatchActionForTest : MENTDataPurgeServiceTaskForTest
		{
			public MENTDataPurgeServiceTaskWithPerBatchActionForTest(int batchLimit, Action<int> onBatchProcessedAction)
				: base()
			{
				CustomBatchLimit = batchLimit;
				OnBatchProcessedAction = onBatchProcessedAction;
			}

			#region New Properties

			int CustomBatchLimit { get; set; }

			readonly Action<int> OnBatchProcessedAction;

			#endregion

			#region Overrides

			protected override int BatchLimit => CustomBatchLimit;

			protected override void OnBatchProcessed(int lastBatchProcessedCount)
			{
				OnBatchProcessedAction?.Invoke(lastBatchProcessedCount);
			}

			#endregion
		}

		#endregion

		#endregion

		public class TestExtractorFactoryProvider : BoardFactoryProvider
		{
			public override BusinessObjectFactory GetBoardGUIThreadFactory()
			{
				throw new NotImplementedException();
			}

			public override ReadOnlyBusinessObjectFactory GetNewBackgroundThreadLoaderFactory(string nameForDebug = "BoardBackgroundThread")
			{
				throw new NotImplementedException();
			}

			public override BusinessObjectFactory GetNewEditFactory(string nameForDebug = "ExtractorEdit")
			{
				return new BusinessObjectFactory { NameForDebugging = nameForDebug };
			}

			public override ISecondaryServerConnectionProvider GetSecondaryServerConnectionProvider()
			{
				return SecondaryServerConnectionProviderProvider.GetProvider();
			}
		}
	}
}
