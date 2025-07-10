using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Billing.Business.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardFormSlideshowTest : BMSGUITestCase
	{
		public void TestSlideshow_WhenHaveOneBoard_ThenShouldRefreshBoardInsteadOfNextSlide()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1 description");

			var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
			section1.BackgroundColor = "Blue";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board1, 5));

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);

			viewModel.RefreshSeconds = 10;
			var boardRefreshCount = 0;

			Factory.Save();

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.RefreshFinished += (s, e) =>
				{
					boardRefreshCount++;
				};

				form.Show();

				Application.DoEvents();
				AssertEquals("WHEN board is shown, it should refresh automatically", 1, boardRefreshCount);

				PulseAndDoEvents(form.AutoRefreshForTest, totalPulse: 5);
				AssertEquals("WHEN waiting for 5 seconds, board should not refresh", 1, boardRefreshCount);

				PulseAndDoEvents(form.AutoRefreshForTest, totalPulse: 5);
				AssertEquals("WHEN waiting for 10 seconds, board should refresh", 2, boardRefreshCount);
			}
		}

		public void TestSlideshow_WhenHaveMoreThanOneBoard_ShouldReportVisualBoardOpen()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", offsetMinutes: 0, sequence: 0);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", offsetMinutes: 0, sequence: 0);

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1 description");
			var section1 = BMSTestHelper.CreateBoardSection(bucket1, board1);

			var board2 = BMSTestHelper.CreateBoard(system, "board2", "board2 description");
			var section2 = BMSTestHelper.CreateBoardSection(bucket2, board2);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board1, 5), Tuple.Create(board2, 5));
			slideshow.MD_Name = "Slideshow";

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);

			using (var visualBoardForm = VisualBoardFormDisplayer.ShowBoard(slideshow))
			{
			}

			var currentBranch = Env.CurrentBranch.Code;
			var messages = new UsageCollectorTestHelper(Factory).LoadUsageMessages().ToArray();

			AssertEquals("Expected 2 message to be present, but was: " + messages.Length, 2, messages.Length);

			void AssertMessage(BMBoard board, IUsageEDIMessage message)
			{
				var messageText = message.EM_MessageText;
				AssertNotNull("Board should be in message", messageText);
				AssertContains("Message must contain FeatureCode: VBO", "\"FeatureCode\": \"VBO\"", messageText);
				AssertContains("Message must contain Module: PAV", "\"Module\": \"PAV\"", messageText);
				AssertContains("Message must contain BranchCode: " + currentBranch, currentBranch, message.Branch.Code);
				AssertContains("Message must contain PK:" + board.PK, $"\"PK\": \"{board.PK}\"", messageText);
				AssertContains("Message must contain Name:" + board.MB_Name, $"\"Name\": \"{board.MB_Name}\"", messageText);
				AssertContains("Message must contain Slideshow:" + slideshow.MD_Name, $"\"Slideshow Name\": \"{slideshow.MD_Name}\"", messageText);
				AssertContains("Message must contain CMP: 1", "\"CMP\": \"1\"", messageText);
				AssertContains("Message must contain BUC: 1", "\"BUC\": \"1\"", messageText);
			}

			var messageForBoard1 = messages.FirstOrDefault(m => m.EM_MessageText.Contains($"\"PK\": \"{board1.PK}\""));
			var messageForBoard2 = messages.FirstOrDefault(m => m.EM_MessageText.Contains($"\"PK\": \"{board2.PK}\""));

			AssertMessage(board1, messageForBoard1);
			AssertMessage(board2, messageForBoard2);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;
	}
}
