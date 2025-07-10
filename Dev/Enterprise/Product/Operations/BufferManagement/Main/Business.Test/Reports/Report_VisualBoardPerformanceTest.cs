using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestDate(2015, 7, 14)]
	class Report_VisualBoardPerformanceTest : BMSTestCaseWithFactory
	{
		#region Overall (aggregating boards and sections)

		public void TestReportContents_Overall()
		{
			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Overall",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'H',
			},
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24), AverageSecondsPerPeriod = 9.75m, MaxSecondsPerPeriod = 10m, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24).AddHours(12), AverageSecondsPerPeriod = 20m, MaxSecondsPerPeriod = 20m, CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20), AverageSecondsPerPeriod = 8.75m, MaxSecondsPerPeriod = 9m, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20).AddHours(12), AverageSecondsPerPeriod = 18m, MaxSecondsPerPeriod = 18m, CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16), AverageSecondsPerPeriod = 7.75m, MaxSecondsPerPeriod = 8m, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16).AddHours(12), AverageSecondsPerPeriod = 16m, MaxSecondsPerPeriod = 16m, CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12), AverageSecondsPerPeriod = 6.75m, MaxSecondsPerPeriod = 7m, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12).AddHours(12), AverageSecondsPerPeriod = 14m, MaxSecondsPerPeriod = 14m, CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8), AverageSecondsPerPeriod = 5.75m, MaxSecondsPerPeriod = 6m, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8).AddHours(12), AverageSecondsPerPeriod = 12m, MaxSecondsPerPeriod = 12m, CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4), AverageSecondsPerPeriod = 4.75m, MaxSecondsPerPeriod = 5m, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4).AddHours(12), AverageSecondsPerPeriod = 10m, MaxSecondsPerPeriod = 10m, CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1), AverageSecondsPerPeriod = 3.75m, MaxSecondsPerPeriod = 4m, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1).AddHours(12), AverageSecondsPerPeriod = 8m, MaxSecondsPerPeriod = 8m, CountPerPeriod = 1 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Overall",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'D',
			},
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24), AverageSecondsPerPeriod = 13.1667m, MaxSecondsPerPeriod = 20m, CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20), AverageSecondsPerPeriod = 11.8333m, MaxSecondsPerPeriod = 18m, CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16), AverageSecondsPerPeriod = 10.5m, MaxSecondsPerPeriod = 16m, CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12), AverageSecondsPerPeriod = 9.1667m, MaxSecondsPerPeriod = 14m, CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8), AverageSecondsPerPeriod = 7.8333m, MaxSecondsPerPeriod = 12m, CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4), AverageSecondsPerPeriod = 6.5m, MaxSecondsPerPeriod = 10m, CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1), AverageSecondsPerPeriod = 5.1667m, MaxSecondsPerPeriod = 8m, CountPerPeriod = 3 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Overall",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'M',
			},
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 11.8333m, MaxSecondsPerPeriod = 20m, CountPerPeriod = 9 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 7.1667m, MaxSecondsPerPeriod = 14m, CountPerPeriod = 12 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Overall",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-21),
				ReportPeriodEndUtc = ZDateTime.UtcNow.AddDays(-5),
				GroupByPeriod = 'M',
			},
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 11.1667m, MaxSecondsPerPeriod = 18m, CountPerPeriod = 6 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 8.5m, MaxSecondsPerPeriod = 14m, CountPerPeriod = 6 }
			);
		}

		#endregion

		#region Per Board (aggregating sections)

		public void TestReportContents_PerBoard()
		{
			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Board",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'H',
			},
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24), AverageSecondsPerPeriod = 9.75m, MaxSecondsPerPeriod = 10m, BoardPK = board1.PK, BoardName = "Narya", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24).AddHours(12), AverageSecondsPerPeriod = 20m, MaxSecondsPerPeriod = 20m, BoardPK = board1.PK, BoardName = "Narya", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20), AverageSecondsPerPeriod = 8.75m, MaxSecondsPerPeriod = 9m, BoardPK = board1.PK, BoardName = "Narya", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20).AddHours(12), AverageSecondsPerPeriod = 18m, MaxSecondsPerPeriod = 18m, BoardPK = board1.PK, BoardName = "Narya", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16), AverageSecondsPerPeriod = 7.75m, MaxSecondsPerPeriod = 8m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16).AddHours(12), AverageSecondsPerPeriod = 16m, MaxSecondsPerPeriod = 16m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12), AverageSecondsPerPeriod = 6.75m, MaxSecondsPerPeriod = 7m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12).AddHours(12), AverageSecondsPerPeriod = 14m, MaxSecondsPerPeriod = 14m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8), AverageSecondsPerPeriod = 5.75m, MaxSecondsPerPeriod = 6m, BoardPK = board3.PK, BoardName = "Vilya", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8).AddHours(12), AverageSecondsPerPeriod = 12m, MaxSecondsPerPeriod = 12m, BoardPK = board3.PK, BoardName = "Vilya", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4), AverageSecondsPerPeriod = 4.75m, MaxSecondsPerPeriod = 5m, BoardPK = board3.PK, BoardName = "Vilya", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4).AddHours(12), AverageSecondsPerPeriod = 10m, MaxSecondsPerPeriod = 10m, BoardPK = board3.PK, BoardName = "Vilya", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1), AverageSecondsPerPeriod = 3.75m, MaxSecondsPerPeriod = 4m, BoardPK = ZGuid.Empty, BoardName = string.Empty, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1).AddHours(12), AverageSecondsPerPeriod = 8m, MaxSecondsPerPeriod = 8m, BoardPK = ZGuid.Empty, BoardName = string.Empty, CountPerPeriod = 1 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Board",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'D',
			},
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24), AverageSecondsPerPeriod = 13.1667m, MaxSecondsPerPeriod = 20m, BoardPK = board1.PK, BoardName = "Narya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20), AverageSecondsPerPeriod = 11.8333m, MaxSecondsPerPeriod = 18m, BoardPK = board1.PK, BoardName = "Narya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16), AverageSecondsPerPeriod = 10.5m, MaxSecondsPerPeriod = 16m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12), AverageSecondsPerPeriod = 9.1667m, MaxSecondsPerPeriod = 14m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8), AverageSecondsPerPeriod = 7.8333m, MaxSecondsPerPeriod = 12m, BoardPK = board3.PK, BoardName = "Vilya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4), AverageSecondsPerPeriod = 6.5m, MaxSecondsPerPeriod = 10m, BoardPK = board3.PK, BoardName = "Vilya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1), AverageSecondsPerPeriod = 5.1667m, MaxSecondsPerPeriod = 8m, BoardPK = ZGuid.Empty, BoardName = string.Empty, CountPerPeriod = 3 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Board",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'M',
			},
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 12.5m, MaxSecondsPerPeriod = 20m, BoardPK = board1.PK, BoardName = "Narya", CountPerPeriod = 6 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 10.5m, MaxSecondsPerPeriod = 16m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 5.1667m, MaxSecondsPerPeriod = 8m, BoardPK = ZGuid.Empty, BoardName = string.Empty, CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 9.1667m, MaxSecondsPerPeriod = 14m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 7.1667m, MaxSecondsPerPeriod = 12m, BoardPK = board3.PK, BoardName = "Vilya", CountPerPeriod = 6 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Board",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-21),
				ReportPeriodEndUtc = ZDateTime.UtcNow.AddDays(-5),
				GroupByPeriod = 'M',
			},
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 11.8333m, MaxSecondsPerPeriod = 18m, BoardPK = board1.PK, BoardName = "Narya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 10.5m, MaxSecondsPerPeriod = 16m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 9.1667m, MaxSecondsPerPeriod = 14m, BoardPK = board2.PK, BoardName = "Nenya", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 7.8333m, MaxSecondsPerPeriod = 12m, BoardPK = board3.PK, BoardName = "Vilya", CountPerPeriod = 3 }
			);
		}

		#endregion

		#region Per Section (aggregating by time only)

		public void TestReportContents_PerSection()
		{
			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Section",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'H',
			},
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24), AverageSecondsPerPeriod = 9.75m, MaxSecondsPerPeriod = 10m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Bucket", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24).AddHours(12), AverageSecondsPerPeriod = 20m, MaxSecondsPerPeriod = 20m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Bucket", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20), AverageSecondsPerPeriod = 8.75m, MaxSecondsPerPeriod = 9m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Gandalf", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20).AddHours(12), AverageSecondsPerPeriod = 18m, MaxSecondsPerPeriod = 18m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Gandalf", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16), AverageSecondsPerPeriod = 7.75m, MaxSecondsPerPeriod = 8m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Bucket", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16).AddHours(12), AverageSecondsPerPeriod = 16m, MaxSecondsPerPeriod = 16m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Bucket", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12), AverageSecondsPerPeriod = 6.75m, MaxSecondsPerPeriod = 7m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Galadriel", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12).AddHours(12), AverageSecondsPerPeriod = 14m, MaxSecondsPerPeriod = 14m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Galadriel", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8), AverageSecondsPerPeriod = 5.75m, MaxSecondsPerPeriod = 6m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Bucket", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8).AddHours(12), AverageSecondsPerPeriod = 12m, MaxSecondsPerPeriod = 12m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Bucket", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4), AverageSecondsPerPeriod = 4.75m, MaxSecondsPerPeriod = 5m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Elrond", CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4).AddHours(12), AverageSecondsPerPeriod = 10m, MaxSecondsPerPeriod = 10m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Elrond", CountPerPeriod = 1 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1), AverageSecondsPerPeriod = 3.75m, MaxSecondsPerPeriod = 4m, BoardPK = ZGuid.Empty, BoardName = string.Empty, SectionName = string.Empty, CountPerPeriod = 2 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1).AddHours(12), AverageSecondsPerPeriod = 8m, MaxSecondsPerPeriod = 8m, BoardPK = ZGuid.Empty, BoardName = string.Empty, SectionName = string.Empty, CountPerPeriod = 1 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Section",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'D',
			},
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-24), AverageSecondsPerPeriod = 13.1667m, MaxSecondsPerPeriod = 20m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Bucket", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-20), AverageSecondsPerPeriod = 11.8333m, MaxSecondsPerPeriod = 18m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Gandalf", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-16), AverageSecondsPerPeriod = 10.5m, MaxSecondsPerPeriod = 16m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Bucket", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-12), AverageSecondsPerPeriod = 9.1667m, MaxSecondsPerPeriod = 14m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Galadriel", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-8), AverageSecondsPerPeriod = 7.8333m, MaxSecondsPerPeriod = 12m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Bucket", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-4), AverageSecondsPerPeriod = 6.5m, MaxSecondsPerPeriod = 10m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Elrond", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = ZDateTime.UtcNow.AddDays(-1), AverageSecondsPerPeriod = 5.1667m, MaxSecondsPerPeriod = 8m, BoardPK = ZGuid.Empty, BoardName = string.Empty, SectionName = string.Empty, CountPerPeriod = 3 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Section",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-25),
				ReportPeriodEndUtc = ZDateTime.UtcNow,
				GroupByPeriod = 'M',
			},
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 13.1667m, MaxSecondsPerPeriod = 20m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Bucket", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 11.8333m, MaxSecondsPerPeriod = 18m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Gandalf", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 10.5m, MaxSecondsPerPeriod = 16m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Bucket", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 5.1667m, MaxSecondsPerPeriod = 8m, BoardPK = ZGuid.Empty, BoardName = string.Empty, SectionName = string.Empty, CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 9.1667m, MaxSecondsPerPeriod = 14m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Galadriel", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 7.8333m, MaxSecondsPerPeriod = 12m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Bucket", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 6.5m, MaxSecondsPerPeriod = 10m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Elrond", CountPerPeriod = 3 }
			);

			AssertRows(new ReportOptions
			{
				ReportName = "Report_VisualBoardPerformance_Section",
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-21),
				ReportPeriodEndUtc = ZDateTime.UtcNow.AddDays(-5),
				GroupByPeriod = 'M',
			},
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 11.8333m, MaxSecondsPerPeriod = 18m, BoardPK = board1.PK, BoardName = "Narya", SectionName = "Gandalf", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 6, 1), AverageSecondsPerPeriod = 10.5m, MaxSecondsPerPeriod = 16m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Bucket", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 9.1667m, MaxSecondsPerPeriod = 14m, BoardPK = board2.PK, BoardName = "Nenya", SectionName = "Galadriel", CountPerPeriod = 3 },
				new RowDetails { PeriodStartTime = new ZDateTime(2015, 7, 1), AverageSecondsPerPeriod = 7.8333m, MaxSecondsPerPeriod = 12m, BoardPK = board3.PK, BoardName = "Vilya", SectionName = "Bucket", CountPerPeriod = 3 }
			);
		}

		#endregion

		#region Assertions

		class ReportOptions
		{
			internal string ReportName { get; set; }
			internal ZDateTime ReportPeriodStartUtc { get; set; }
			internal ZDateTime ReportPeriodEndUtc { get; set; }
			internal char GroupByPeriod { get; set; }

			public override string ToString()
			{
				return $"Report: {ReportName}, StartRange: {ReportPeriodStartUtc.SqlFormat}, EndRange: {ReportPeriodStartUtc}, GroupBy: {GroupByPeriod}";
			}
		}

		class RowDetails
		{
			internal ZDateTime PeriodStartTime { get; set; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			internal decimal AverageSecondsPerPeriod { get; set; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			internal decimal MaxSecondsPerPeriod { get; set; }
			internal long CountPerPeriod { get; set; }
			internal ZGuid BoardPK { get; set; }
			internal string BoardName { get; set; }
			internal string SectionName { get; set; }

			public override string ToString()
			{
				return $"Time: {PeriodStartTime}, Avg Secs: {Math.Round(AverageSecondsPerPeriod, 4).ToString("0.0000")}, Max Secs: {Math.Round(MaxSecondsPerPeriod, 4).ToString("0.0000")}, Count: {CountPerPeriod}, Board PK: {BoardPK}, Board Name: {BoardName}, Section Name: {SectionName}";
			}
		}

		void AssertRows(ReportOptions options, params RowDetails[] expectedRows)
		{
			var expectedResults = expectedRows.Select(r => r.ToString()).ToArray();
			var results = GetReportResults(options).Select(r => r.ToString()).ToArray();

			AssertMultilineASCIIEquals(options.ToString(),
				string.Join(System.Environment.NewLine, expectedResults),
				string.Join(System.Environment.NewLine, results));
		}

		#endregion

		#region Implementation

		BMBoard board1, board2, board3;

		protected override void SetUp()
		{
			base.SetUp();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Bucket.FC_Name = "Bucket";
			config.Buffer.FC_Name = "Buffer";

			board1 = BMSTestHelper.CreateBoard(config.System, name: "Narya");
			board2 = BMSTestHelper.CreateBoard(config.System, name: "Nenya");
			board3 = BMSTestHelper.CreateBoard(config.System, name: "Vilya");

			var section1_1 = BMSTestHelper.CreateBoardSection(config.Bucket, board1);
			var section1_2 = BMSTestHelper.CreateBoardSection(config.Buffer, board1);
			BMSTestHelper.SetOverriddenSectionName(section1_2, "Gandalf");

			var section2_1 = BMSTestHelper.CreateBoardSection(config.Bucket, board2);
			var section2_2 = BMSTestHelper.CreateBoardSection(config.Buffer, board2);
			BMSTestHelper.SetOverriddenSectionName(section2_2, "Galadriel");

			var section3_1 = BMSTestHelper.CreateBoardSection(config.Bucket, board3);
			var section3_2 = BMSTestHelper.CreateBoardSection(config.Buffer, board3);
			BMSTestHelper.SetOverriddenSectionName(section3_2, "Elrond");

			BMSTestHelper.CreateSectionPerformanceStatistics(section1_1, ZDateTime.UtcNow.AddDays(-24), TimeSpan.FromSeconds(10.0));
			BMSTestHelper.CreateSectionPerformanceStatistics(section1_1, ZDateTime.UtcNow.AddDays(-24).AddMinutes(1), TimeSpan.FromSeconds(9.5));
			BMSTestHelper.CreateSectionPerformanceStatistics(section1_1, ZDateTime.UtcNow.AddDays(-24).AddHours(12), TimeSpan.FromSeconds(20.0));

			BMSTestHelper.CreateSectionPerformanceStatistics(section1_2, ZDateTime.UtcNow.AddDays(-20), TimeSpan.FromSeconds(9.0));
			BMSTestHelper.CreateSectionPerformanceStatistics(section1_2, ZDateTime.UtcNow.AddDays(-20).AddMinutes(1), TimeSpan.FromSeconds(8.5));
			BMSTestHelper.CreateSectionPerformanceStatistics(section1_2, ZDateTime.UtcNow.AddDays(-20).AddHours(12), TimeSpan.FromSeconds(18.0));

			BMSTestHelper.CreateSectionPerformanceStatistics(section2_1, ZDateTime.UtcNow.AddDays(-16), TimeSpan.FromSeconds(8.0));
			BMSTestHelper.CreateSectionPerformanceStatistics(section2_1, ZDateTime.UtcNow.AddDays(-16).AddMinutes(1), TimeSpan.FromSeconds(7.5));
			BMSTestHelper.CreateSectionPerformanceStatistics(section2_1, ZDateTime.UtcNow.AddDays(-16).AddHours(12), TimeSpan.FromSeconds(16.0));

			BMSTestHelper.CreateSectionPerformanceStatistics(section2_2, ZDateTime.UtcNow.AddDays(-12), TimeSpan.FromSeconds(7.0));
			BMSTestHelper.CreateSectionPerformanceStatistics(section2_2, ZDateTime.UtcNow.AddDays(-12).AddMinutes(1), TimeSpan.FromSeconds(6.5));
			BMSTestHelper.CreateSectionPerformanceStatistics(section2_2, ZDateTime.UtcNow.AddDays(-12).AddHours(12), TimeSpan.FromSeconds(14.0));

			BMSTestHelper.CreateSectionPerformanceStatistics(section3_1, ZDateTime.UtcNow.AddDays(-8), TimeSpan.FromSeconds(6.0));
			BMSTestHelper.CreateSectionPerformanceStatistics(section3_1, ZDateTime.UtcNow.AddDays(-8).AddMinutes(1), TimeSpan.FromSeconds(5.5));
			BMSTestHelper.CreateSectionPerformanceStatistics(section3_1, ZDateTime.UtcNow.AddDays(-8).AddHours(12), TimeSpan.FromSeconds(12.0));

			BMSTestHelper.CreateSectionPerformanceStatistics(section3_2, ZDateTime.UtcNow.AddDays(-4), TimeSpan.FromSeconds(5.0));
			BMSTestHelper.CreateSectionPerformanceStatistics(section3_2, ZDateTime.UtcNow.AddDays(-4).AddMinutes(1), TimeSpan.FromSeconds(4.5));
			BMSTestHelper.CreateSectionPerformanceStatistics(section3_2, ZDateTime.UtcNow.AddDays(-4).AddHours(12), TimeSpan.FromSeconds(10.0));

			BMSTestHelper.CreateSectionPerformanceStatistics(section3_2, ZDateTime.UtcNow.AddDays(-1), TimeSpan.FromSeconds(4.0), true);
			BMSTestHelper.CreateSectionPerformanceStatistics(section3_2, ZDateTime.UtcNow.AddDays(-1).AddMinutes(1), TimeSpan.FromSeconds(3.5), true);
			BMSTestHelper.CreateSectionPerformanceStatistics(section3_2, ZDateTime.UtcNow.AddDays(-1).AddHours(12), TimeSpan.FromSeconds(8.0), true);

			Factory.Save();
			StatisticsTestHelper.ExecStatisticsServiceSummarise(TestConnection);
		}

		List<RowDetails> GetReportResults(ReportOptions options)
		{
			var results = new List<RowDetails>();
			var sql = string.Format($"SELECT * FROM {options.ReportName}('{options.ReportPeriodStartUtc.SqlFormat}', '{options.ReportPeriodEndUtc.SqlFormat}', '{options.GroupByPeriod}') ORDER BY 1");

			if (options.ReportName.EndsWith("_Board"))
			{
				sql += ", 6";
			}

			if (options.ReportName.EndsWith("_Section"))
			{
				sql += ", 6, 7";
			}

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					results.Add(new RowDetails
					{
						PeriodStartTime = reader.GetDateTime(0),
						AverageSecondsPerPeriod = reader.GetDecimal(1),
						MaxSecondsPerPeriod = reader.GetDecimal(2),
						CountPerPeriod = reader.GetInt64(3),
						BoardPK = reader.FieldCount > 4 && !string.IsNullOrEmpty(reader.GetValue(4).ToString()) ? reader.GetGuid(4) : ZGuid.Empty,
						BoardName = reader.FieldCount > 5 ? reader.GetString(5) : null,
						SectionName = reader.FieldCount > 6 ? reader.GetString(6) : null,
					});
				}
			}

			return results;
		}

		#endregion
	}
}
