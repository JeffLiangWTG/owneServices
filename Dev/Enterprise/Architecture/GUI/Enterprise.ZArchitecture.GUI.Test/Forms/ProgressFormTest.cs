using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	[TestedType(typeof(ProgressForm))]
	public class ProgressFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ProgressForm();
		}

		protected override bool AllowFormSizeFixed => true;
	}

	public class ProgressFormTest : TestCase
	{
		#region Test Control

		public class TestProgressForm : ProgressForm
		{
			public ProgressBar ExposedProgressBar
			{
				get
				{
					UpdateProgressFormAndStatus();
					return ProgressBar;
				}
			}

			public ZLabel ExposedProgressLabel
			{
				get
				{
					UpdateProgressFormAndStatus();
					return ProgressLabel;
				}
			}

			public ZButton ExposedCancelButton
			{
				get { return CancelProgressButton; }
			}

			public int ExposedProgressBarLengthWithoutButton
			{
				get { return ProgressBarLengthWithoutButton; }
			}

			public TimeSpan ExposedSleepBetweenRefresh
			{
				get { return SleepBetweenRefresh; }
			}

			public bool ShouldAddFormActivityLog_Expose => ShouldAddFormActivityLog;
		}

		public class DummyProgressForm : ProgressForm
		{
			protected override void ModifyStatusAndPercentComplete(ref string status, ref int percentComplete)
			{
				status = "Dummy Status";
				percentComplete = 55;
			}
		}

		#endregion

		public void TestIsFast()
		{
			var sw = Stopwatch.StartNew();
			using (var myTestProgressForm = new ProgressForm())
			{
				const long runs = 5000000;
				myTestProgressForm.Show();
				for (long i = 0; i < runs; i++)
				{
					var pc = (int)(100 * i / runs);
					myTestProgressForm.SetStatusAndPercentComplete("Percent Complete : " + pc, pc);
				}
			}
			Assert(sw.ElapsedMilliseconds < 15000);
		}

		public void TestModifyStatusAndPercentComplete()
		{
			using (var myTestProgressForm = new DummyProgressForm())
			{
				myTestProgressForm.SetStatusAndPercentComplete("Wrong data", 22);
				AssertEquals(55, myTestProgressForm.PercentComplete);
				AssertEquals("Dummy Status", myTestProgressForm.Status);
			}
		}

		public void TestControlsAreNotPlacedOutsideVisibleArea()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.Show();
				CheckControlsWithinBoundaries(myTestProgressForm);
			}
		}

		void CheckControlsWithinBoundaries(Control currentControl)
		{
			try
			{
				if (currentControl.Controls.Count > 0)
				{
					foreach (Control nestedControl in currentControl.Controls)
					{
						CheckControlsWithinBoundaries(nestedControl);
					}
				}
				else if (currentControl.Visible && currentControl.Parent != null)
				{
					Assert("Top is visible", currentControl.Top >= 0);
					Assert("Left is visible", currentControl.Left >= 0);
					Assert("Control is within horiztonal boundary", currentControl.Left + currentControl.Width <= currentControl.Parent.Width);
					Assert("Control is within vertical boundary", currentControl.Top + currentControl.Height <= currentControl.Parent.Height);
				}
			}
			catch (Exception e)
			{
				throw new Exception(String.Format("Boundary Check Failure: {0}", currentControl.Name), e);
			}
		}

		[ExpectNoExceptions]
		public void TestShowAndClose()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.Show();
				AssertEquals(true, myTestProgressForm.Visible);

				myTestProgressForm.Close();
				AssertEquals(false, myTestProgressForm.Visible);
			}
		}

		public void TestStatusSet()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.Show();

				myTestProgressForm.Status = "Testing 1 2 3";
				AssertEquals("Testing 1 2 3", myTestProgressForm.ExposedProgressLabel.Text);
				AssertEquals("Testing 1 2 3", myTestProgressForm.Status);

				myTestProgressForm.Status = "Finishing...";
				AssertEquals("Finishing...", myTestProgressForm.ExposedProgressLabel.Text);
				AssertEquals("Finishing...", myTestProgressForm.Status);
			}
		}

		public void TestShowProgressBar()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.ShowProgressBar = true;
				myTestProgressForm.Show();

				AssertEquals("Height should be same as nothing is hidden", DefaultHeight, myTestProgressForm.Height);
			}
		}

		public void TestHideProgressBar()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.ShowProgressBar = false;
				myTestProgressForm.Show();

				AssertEquals("Height should be same as only progress bar is hidden", DefaultHeight, myTestProgressForm.Height);
			}
		}

		public void TestShowCancelButton()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.ShowCancelButton = true;
				myTestProgressForm.Show();

				AssertEquals("Height should be same as nothing is hidden", DefaultHeight, myTestProgressForm.Height);
			}
		}

		public void TestEnableCancelButton()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.ExposedCancelButton.Enabled = true;
				myTestProgressForm.EnableCancelButton = false;
				myTestProgressForm.Show();

				AssertEquals("CancelButton should be disabled", false, myTestProgressForm.ExposedCancelButton.Enabled);
			}
		}

		public void TestHideCancelButton()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.ShowCancelButton = false;
				myTestProgressForm.Show();

				AssertEquals("Height should be same as only cancel button is hidden", DefaultHeight, myTestProgressForm.Height);
			}
		}

		public void TestHideCancelButtonAndProgressBar()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				var initialHeight = myTestProgressForm.Height;

				myTestProgressForm.ShowCancelButton = false;
				myTestProgressForm.ShowProgressBar = false;

				myTestProgressForm.Show();
				Application.DoEvents();

				Assert("Height should be reduced as both the progress bar and cancel button are hidden.", initialHeight > myTestProgressForm.Height);
			}
		}

		public void TestProgressBarValueSet()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.Show();

				myTestProgressForm.PercentComplete = 22;
				AssertEquals(22, myTestProgressForm.PercentComplete);
				AssertEquals(22, myTestProgressForm.ExposedProgressBar.Value);
			}
		}

		public void TestCancelEventFired()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.Cancelled += new EventHandler(TestProgressForm_Cancelled);
				myTestProgressForm.Show();

				AssertEquals("Pre-condition", false, Cancelled);
				myTestProgressForm.CancelButton.PerformClick();
				AssertEquals("Event fired", true, Cancelled);
				AssertEquals(false, myTestProgressForm.Visible);
			}
		}

		public void TestLengthOfProgressBar()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.ShowCancelButton = true;
				AssertEquals("Progress bar is short enough to fit cancel button", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(ProgressForm.ProgressBarOriginalLengthWithButton), myTestProgressForm.ExposedProgressBar.Size.Width);

				myTestProgressForm.ShowCancelButton = false;
				AssertEquals("Progress bar takes up the width of the form", myTestProgressForm.ExposedProgressBarLengthWithoutButton, myTestProgressForm.ExposedProgressBar.Size.Width);
			}
		}

		public void TestCancelProgressButtonText()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.ShowCancelButton = true;
				myTestProgressForm.Show();
				AssertEquals("Button text", "Cancel", myTestProgressForm.CancelProgressButtonText);

				myTestProgressForm.ExposedCancelButton.Text = "New text";
				AssertEquals("Button text", "New text", myTestProgressForm.CancelProgressButtonText);
			}
		}

		public void TestCancelProgressButtonIfDoNotClose()
		{
			using (var testProgressForm = new TestProgressForm())
			{
				testProgressForm.Show();
				testProgressForm.DoNotCloseWhenClickCancel = true;
				var cancelProgressButton = testProgressForm.Find(c => c.Name == "CancelProgressButton").First();
				AssertEquals(true, cancelProgressButton.Enabled);
				testProgressForm.CancelButton.PerformClick();
				AssertEquals(false, cancelProgressButton.Enabled);
			}
		}

		public void TestCancelProgressButtonIfClose()
		{
			using (var testProgressForm = new TestProgressForm())
			{
				testProgressForm.Show();
				testProgressForm.DoNotCloseWhenClickCancel = false;
				AssertEquals(true, testProgressForm.Visible);
				testProgressForm.CancelButton.PerformClick();
				AssertEquals(false, testProgressForm.Visible);
			}
		}

		public void TestCallDoEventsWhenCancelButtonIsHidden()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				myTestProgressForm.ShowCancelButton = false;
				var eventHandled = false;
				myTestProgressForm.Shown += (_, __) =>
				{
					eventHandled = true;
				};

				myTestProgressForm.Show();
				AssertEquals("pre condition", false, eventHandled);

				myTestProgressForm.ShowProgressBar = true;
				AssertEquals("Application.DoEvents should be called when update form", true, eventHandled);
			}
		}

		public void TestSleepBetweenRefresh()
		{
			using (var myTestProgressForm = new TestProgressForm())
			{
				AssertEquals(new TimeSpan(0, 0, 0, 0, 250), myTestProgressForm.ExposedSleepBetweenRefresh);

				myTestProgressForm.SleepBetweenRefreshMilliseconds = 10;
				AssertEquals(new TimeSpan(0, 0, 0, 0, 10), myTestProgressForm.ExposedSleepBetweenRefresh);
			}
		}

		[UseSnapshotProtection]
		public void TestShouldAddFormActivityLog()
		{
			var originalValue = EnvProxy.Instance.Registry.UserEventTrackingEnterprise;
			try
			{
				ZFormActivityLogger.Instance.StatLogs.Clear();
				EnvProxy.Instance.Registry.UserEventTrackingEnterprise = true;

				using (var testProgressForm = new TestProgressForm())
				{
					AssertEquals("Should have no activity logs", 0, ZFormActivityLogger.Instance.StatLogs.Count);
					AssertEquals("ShouldAddFormActivityLog should be false", false, testProgressForm.ShouldAddFormActivityLog_Expose);
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.UserEventTrackingEnterprise = originalValue;
			}
		}

		int DefaultHeight
		{
			get
			{
				using (var testProgressForm = new TestProgressForm())
				{
					testProgressForm.Show();
					return testProgressForm.Height;
				}
			}
		}

		bool Cancelled;
		void TestProgressForm_Cancelled(object sender, EventArgs e)
		{
			Cancelled = true;
		}
	}
}
