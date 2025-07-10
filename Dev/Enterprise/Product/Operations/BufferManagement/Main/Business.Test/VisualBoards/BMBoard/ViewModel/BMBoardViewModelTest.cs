using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Registry.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BoardViewModelTest : BMSTestCaseWithFactory
	{
		public void TestBoardName()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Name = "WTGDEV";
			var board1 = system.Boards.AddNew();
			board1.MB_Name = "International Logistics";
			var board2 = system.Boards.AddNew();
			board2.MB_Name = "PAVE";

			var viewModel = BMSTestHelper.CreateBoardViewModel(board1);
			AssertEquals("International Logistics", BMSTestHelper.CreateBoardViewModel(board1).BoardName);
			AssertEquals("PAVE", BMSTestHelper.CreateBoardViewModel(board2).BoardName);
		}

		public void TestSections()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket = CreateBucket(system);

			var board = system.Boards.AddNew();
			var section1 = CreateBoardSection(bucket, board);
			var section2 = CreateBoardSection(bucket, board);
			var section3 = CreateBoardSection(bucket, board);

			section3.Row = 0;       // First
			section3.Column = 0;

			section2.Row = 1;
			section2.Column = 1;    // Second

			section1.Row = 1;
			section1.Column = 2;    // Third

			Factory.Save();

			var viewModel = BMSTestHelper.CreateBoardViewModel(board);
			viewModel.Build(board);

			var sections = viewModel.GetSections().ToArray();

			AssertEquals(3, sections.Length);
			AssertEquals(section3.PK, sections[0].SectionPK);
			AssertEquals(section2.PK, sections[1].SectionPK);
			AssertEquals(section1.PK, sections[2].SectionPK);
		}

		public void TestPopulateEstimatedLoadTimeCache_ShouldUseParametrizedQuery()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Name = "WTGDEV";
			var board = system.Boards.AddNew();
			board.MB_Name = "Parametrized Query Test";

			AssertParameterizedQuery(board, nameof(EnabledState.Simple), true);
			AssertParameterizedQuery(board, nameof(EnabledState.Detailed), true);
			AssertParameterizedQuery(board, nameof(EnabledState.Disabled), false);
		}

		void AssertParameterizedQuery(BMBoard board, string mode, bool expectQuery)
		{
			using (SystemDataRegistry.Instance.StatisticsCollectionEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mode))
			using (Db.Connection.TrackExecutedCommands())
			{
				var viewModel = BMSTestHelper.CreateBoardViewModel(board);
				var commandText = Db.Connection.ExecutedCommands.FirstOrDefault(x => x.Contains("vw_StatisticsMeasurementsIncludingChildren"));

				if (expectQuery)
				{
					AssertNotNull("Database should be queried for section refresh statistics if statistics collection is enabled", commandText);
					AssertEquals("StatisticsHistoryRange parameter should appear in query", true, commandText.Contains("AND DateToUTC > GetUtcDate() - @StatisticsHistoryRange"));
					AssertEquals("StatisticsHistoryRange parameter should appear in query", 1, Regex.Matches(commandText, "Params(?s).+StatisticsHistoryRange").Count);
					AssertEquals("BoardName parameter should appear in query", true, commandText.Contains("AND SubName LIKE @BoardName"));
					AssertEquals("BoardName parameter name should be in list of params", 1, Regex.Matches(commandText, "Params(?s).+@BoardName").Count);
				}
				else
				{
					AssertNull("Database should not be queried for section refresh statistics if statistics collection is disabled", commandText);
				}
			}
		}
	}
}
