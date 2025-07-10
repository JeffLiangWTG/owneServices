using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	[TestedType(typeof(VisualBoardForm))]
	public class VisualBoardFormBasherTest : ZFormBasherTest
	{
		public void TestControlButtons_ShouldPlaceInTopRightCornerAndNotOverlap()
		{
			var borderWidth = 2;
			var system = VisualBoardsTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Show();
				var visibleButtons = form.controlsPanel.FindAll<ZButton>().Where(x => x.Visible).OrderByDescending(b => b.Right).ToArray();
				AssertEquals("1 button should be visible", 1, visibleButtons.Length);

				AssertEquals(false, form.controlsPanel.SearchBox.Visible);
				AssertEquals(false, form.controlsPanel.RefreshButton.Visible);
				AssertEquals(true, form.controlsPanel.CountdownLabel.Visible);

				form.controlsPanel.Expand();
				Application.DoEvents();

				AssertEquals(true, form.controlsPanel.SearchBox.Visible);
				AssertEquals(true, form.controlsPanel.RefreshButton.Visible);
				AssertEquals(true, form.controlsPanel.CountdownLabel.Visible);

				var buttons = form.controlsPanel.FindAll<ZButton>().OrderBy(b => b.Left).ToArray();
				var expandButtonWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding) + buttons[0].Width;
				var leftOffset = expandButtonWidth + form.controlsPanel.SearchBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ControlGripSize + BoardControlsPanel.ButtonPadding) + borderWidth;

				AssertEquals(7, buttons.Length);

				CombineAssertions("Control button locations", () =>
				{
					for (int i = 1; i < buttons.Length; i++)
					{
						var button = buttons[i];
						var message = $"Button [{button.Name}] at position [{i}]";

						AssertEquals(message, leftOffset, button.Left);
						leftOffset = ControlDpiScalingHelper.ScaleToCurrentDpiX(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(buttons[i].Right) + BoardControlsPanel.ButtonPadding);
					}
				});

				var searchBox = form.FindAll<ZSearchBox>().Single();
				leftOffset = ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding + BoardControlsPanel.ControlGripSize);

				AssertEquals(leftOffset + expandButtonWidth, searchBox.Left);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			Factory.Save();

			var viewModel = GetViewModel(board);
			return new VisualBoardForm(viewModel);
		}

		public override bool AllowUntranslatableFormTitle()
		{
			// shows the board's name which comes from data
			return true;
		}

		public static BoardSlideshowViewModel GetViewModel(IBMBoard board, bool useStatusCache = true)
		{
			return VisualBoardsTestHelper.CreateSlideshowViewModel(board, useStatusCache: useStatusCache);
		}

		protected override void SetUp()
		{
			base.SetUp();
			asyncDisabler = VisualBoardsTestCase.DisableAsyncBehaviour();
			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override void TearDown()
		{
			base.TearDown();
			asyncDisabler.Dispose();
		}

		IDisposable asyncDisabler;

		#endregion
	}
}
