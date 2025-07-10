using System.Collections.Generic;
using System.Drawing;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup.Testing
{
	sealed class ScreenResolutionCheckerTest : AbstractApplicationStartupTaskTest<ScreenResolutionChecker>
	{
		class ScreenResolutionCheckerWithMockBounds : ScreenResolutionChecker
		{
			readonly List<Rectangle> screenBounds = new List<Rectangle>();
			protected override IEnumerable<Rectangle> AllScreenBounds => screenBounds;
			public void AddScreen(int x, int y, int w, int h) => screenBounds.Add(new Rectangle(x, y, w, h));
			public string WarningMessageExposed => WarningMessage;
		}

		ScreenResolutionCheckerWithMockBounds TestChecker;

		public override int DefaultErrorExitCode => ExitCodes.ScreenResolutionCheckerError;

		protected override void SetUp()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestChecker = new ScreenResolutionCheckerWithMockBounds();
		}

		public void TestTooNarrow()
		{
			TestChecker.AddScreen(0, 0, 1361, 768);
			var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
			Assert("Task should return true", result);
			Assert("Screen resolution warning should have been displayed", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(@"Your display settings do not meet the minimum requirements. The minimum display requirements are a resolution of at least 1366 x 768 without any scaling.
The resolution for monitor 1 is 1361 x 768.
Please adjust your resolution and scaling so that they meet the minimum requirements."));
		}

		public void TestTooShort()
		{
			TestChecker.AddScreen(0, 0, 1366, 763);
			var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
			Assert("Task should return true", result);
			Assert("Screen resolution warning should have been displayed", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(@"Your display settings do not meet the minimum requirements. The minimum display requirements are a resolution of at least 1366 x 768 without any scaling.
The resolution for monitor 1 is 1366 x 763.
Please adjust your resolution and scaling so that they meet the minimum requirements."));
		}

		public void TestExactlyRight()
		{
			TestChecker.AddScreen(0, 0, 1366, 768);
			var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
			Assert("Task should return true", result);
			Assert("No messages should have been displayed", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestOversize()
		{
			TestChecker.AddScreen(0, 0, 2048, 1536);
			var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
			Assert("Task should return true", result);
			Assert("No messages should have been displayed", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestMessage()
		{
			Assert("Should mention width", TestChecker.WarningMessageExposed.IndexOf("1366") >= 0);
			Assert("Should mention height", TestChecker.WarningMessageExposed.IndexOf("768") >= 0);
		}

		public void TestMultipleScreens_AllAboveCutoff()
		{
			TestChecker.AddScreen(0, 0, 2100, 2010);
			TestChecker.AddScreen(0, 0, 1920, 1080);
			TestChecker.AddScreen(0, 0, 1366, 768);

			var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
			Assert("Task should return true", result);
			Assert("All screens are above the minimum resolution. Should say its ok", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestMultipleScreens_OneBelowCutoff()
		{
			TestChecker.AddScreen(0, 0, 2100, 2010);
			TestChecker.AddScreen(0, 0, 1920, 1080);
			TestChecker.AddScreen(0, 0, 640, 480);

			var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
			Assert("Task should return true", result);
			Assert("Only one screen is below the minimum, should still be considered ok", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestMultipleScreens_AllBelowCutoff()
		{
			TestChecker.AddScreen(0, 0, 1360, 768);
			TestChecker.AddScreen(0, 0, 1280, 720);
			TestChecker.AddScreen(0, 0, 640, 480);

			var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
			Assert("Task should return true", result);
			Assert("All screens are below the minimum resolution, a warning should be shown", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(TestChecker.WarningMessageExposed));
			Assert("All screens are below the minimum resolution, warning should include all current screen resolutions", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(@"Your display settings do not meet the minimum requirements. The minimum display requirements are a resolution of at least 1366 x 768 without any scaling.
The resolution for monitor 1 is 1360 x 768.
The resolution for monitor 2 is 1280 x 720.
The resolution for monitor 3 is 640 x 480.
Please adjust your resolution and scaling so that they meet the minimum requirements."));
		}

		public void TestShowWarningMessage_CanShowDialogs()
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			using (Globals.SetIsWinzorForTest(false))
			using (Globals.SetIsWebForTest(false))
			{
				TestChecker.AddScreen(0, 0, 1366, 700);

				using (Globals.SetIsConsoleSessionForTest(true))
				{
					AssertEquals(nameof(Globals.CanShowDialogs), false, Globals.CanShowDialogs);

					var result1 = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
					Assert("Task should return true", result1);
					Assert("Screen resolution warning should not have been displayed when CanShowDialogs is false", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				}

				AssertEquals(nameof(Globals.CanShowDialogs), true, Globals.CanShowDialogs);

				var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));

				Assert("Task should return true", result);
				Assert("Screen resolution warning should have been displayed when CanShowDialogs is true", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(@"Your display settings do not meet the minimum requirements. The minimum display requirements are a resolution of at least 1366 x 768 without any scaling.
The resolution for monitor 1 is 1366 x 700.
Please adjust your resolution and scaling so that they meet the minimum requirements."));
			}
		}

		public void TestExecuteScreenResolutionChecker()
		{
			TestChecker.AddScreen(0, 0, 1366, 700);

			var result = TestChecker.Execute(new ApplicationArguments(System.Array.Empty<string>()));
			Assert("Task should return true", result);

			var lastMessage = UnitTestUserNotification.Instance.LastMessage;
			Assert(lastMessage.Contains(@"Your display settings do not meet the minimum requirements. The minimum display requirements are a resolution of at least 1366 x 768 without any scaling.
The resolution for monitor 1 is 1366 x 700.
Please adjust your resolution and scaling so that they meet the minimum requirements."));

			var context = lastMessage.Context;
			AssertEquals(context.Buttons, ZMessageBoxButtons.OK);
			AssertEquals(context.Caption, TestChecker.TaskDescription);
			AssertEquals(context.CheckBoxCaption, TestChecker.CheckboxCaption);
			AssertEquals(context.DefaultResult, ZDialogResult.None);
			AssertEquals(context.Icon, ZMessageBoxIcon.Exclamation);
			Assert(context.GlobalOnly);
			Assert(context.ShowCheckboxOnly);
		}
	}
}
