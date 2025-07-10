using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	public class VisualBoardFormDisplayerStrategyTest : NonTransactionedTestCase
	{
		public void TestBoardSlideshowPivotCollectionConstructor_OpenSlideshow_ShouldUseFactoryOnCurrentThread()
		{
			var board1 = Factory.NewWithValidTestData<BMBoard>();
			var board2 = Factory.NewWithValidTestData<BMBoard>();

			var boardSlideShow = Factory.NewWithValidTestData<BMBoardSlideshow>();
			boardSlideShow.MD_Name = "Board 1";

			var boardPivot1 = boardSlideShow.BoardPivots.AddNew();
			boardPivot1.MC_MB_Board = board1.PK;
			boardPivot1.MC_Sequence = 2;

			var boardPivot2 = boardSlideShow.BoardPivots.AddNew();
			boardPivot2.MC_MB_Board = board2.PK;
			boardPivot2.MC_Sequence = 1;

			Factory.Save();

			var reloadedSlideShow = new BusinessObjectFactory().Load<BMBoardSlideshow>(boardSlideShow.PK);
			AssertNotNull("Precondition", reloadedSlideShow);

			using (var form = VisualBoardFormDisplayer.ShowBoard(reloadedSlideShow))
			{
				Application.DoEvents();
				var boardProvider = form.SlideShowViewModel.Source;
				AssertNotNull(boardProvider);
				var boardsMessage = @"
						Given a slideshow with 2 boards
						When a opening it in a VisualBoardForm on a new thread
						Then the 2 boards should be in the Boards collection of the boardProvider";
				var expectedPKs = new ZGuid[] { board1.PK, board2.PK };
				AssertContainsExactElementsInAnyOrder(boardsMessage, expectedPKs, boardProvider.Boards.Select(w => w.BoardPK));

				AssertEquals(true, form.BusinessEntityForPersistingForm.Factory.IsOwnedByCurrentThread);

				Application.DoEvents();
			}
		}

		[ExpectNoExceptions]
		public void TestBoardSlideshow_OverloadedBoardMemoryConsumption()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Dat System";
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var listOfBoards = new List<Tuple<BMBoard, int>>(10);
			for (var i = 0; i <= 10; i++)
			{
				var boardName = "board" + i.ToString();
				var board = BMSTestHelper.CreateBoard(system, boardName, "something for test");
				var section = BMSTestHelper.CreateBoardSection(bucket, board);
				section.BackgroundColor = GetColor(i);

				listOfBoards.Add(Tuple.Create(board, 60));
			}

			for (int i = 1; i <= 300; i++)
			{
				var jobHeader = VisualBoardsTestHelper.CreateJobHeader<SalesEnquiry>(Factory);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = bucket.PK;
				var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "something to show");
			}

			var slideShow = BMSTestHelper.CreateSlideshow(Factory, listOfBoards.ToArray());
			Factory.Save();

			var reloadedSlideShow = new BusinessObjectFactory().Load<BMBoardSlideshow>(slideShow.PK);
			AssertNotNull("Precondition", reloadedSlideShow);

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(reloadedSlideShow);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				for (int i = 1; i <= 10; i++)
				{
					form.MoveNext();
				}
			}
		}

		public void TestAllOpenedFormShouldBeReportedWhenCrashedNotOnMainThread()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var board = VisualBoardsTestHelper.CreateBoard(system);

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				Application.DoEvents();
				AssertEquals(@"Main thread should dectect the opened form in the cache", 1, OpenedFormCache.GetInstance().Count);
			}
		}

		public void TestVisualBoardLayoutTable_ShouldNotBreakOnDisposalFailure()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Dat System";
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var boardName = "ply";
			var board = BMSTestHelper.CreateBoard(system, boardName, "something for test");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			section.BackgroundColor = GetColor(0);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<SalesEnquiry>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = bucket.PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "something to show");

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);
			AssertNoExceptionThrown("We should succ up InvalidOperationExceptions resulting from VisualBoardTableLayoutPanel disposal", () =>
			{
				using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
				using (var form = new VisualBoardForm(viewModel))
				{
					form.Show();
					var dummyTableLayout = new VisualBoardTableLayoutPanel_TestingSubclass(form.VisualBoardTableLayoutPanel)
					{
						UsurpDisposal = true
					};

					form.SetVisualBoardTableLayoutPanel_ForTest(dummyTableLayout);

					form.RefreshNow_ForTest(forceReload: true);
				}
			});

			AssertEquals("We should have been alerted to the closing of the visual board", $"The board [{boardName}] failed to display properly. Please close and reopen this window.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation

		string GetColor(int index)
		{
			switch (index)
			{
				case 0:
					return ColorList.NameFromColor(Color.HotPink);
				case 1:
					return ColorList.NameFromColor(Color.PaleGoldenrod);
				case 2:
					return ColorList.NameFromColor(Color.Aqua);
				case 3:
					return ColorList.NameFromColor(Color.Lavender);
				case 4:
					return ColorList.NameFromColor(Color.DarkBlue);
				case 5:
					return ColorList.NameFromColor(Color.Red);
				case 6:
					return ColorList.NameFromColor(Color.LightSalmon);
				case 7:
					return ColorList.NameFromColor(Color.LightGreen);
				case 8:
					return ColorList.NameFromColor(Color.SandyBrown);
				case 9:
					return ColorList.NameFromColor(Color.SeaGreen);
				default:
					return ColorList.NameFromColor(Color.White);
			}
		}

		protected override void SetUp()
		{
			BMSTestCaseWithFactory.SetupAndClearTables();

			base.SetUp();
		}

		#endregion
	}
}
