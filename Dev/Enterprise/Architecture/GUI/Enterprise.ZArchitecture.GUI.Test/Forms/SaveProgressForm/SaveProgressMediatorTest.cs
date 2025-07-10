using System.Drawing;
using System.Threading;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class SaveProgressMediatorTest : NUnit.Framework.TestCase
	{
		public void TestFormIsShownAfterDelay()
		{
			Mediator.ShowModalProgressForm(ControlDpiScalingHelper.NewScaledRectangle(100, 200, 300, 400), "status", 10);
			AssertNull("Not shown right away", Mediator.form);
			System.Threading.Thread.Sleep(500);
			AssertNull("Not shown right away", Mediator.form);
			System.Threading.Thread.Sleep(2000);
			AssertNotNull("form", Mediator.form);
			AssertEquals("topmost", true, Mediator.form.TopMost);

			Mediator.Dispose();
			Mediator.FormDisposed.WaitOne();

			var watch = System.Diagnostics.Stopwatch.StartNew();

			while (!Mediator.form.IsDisposed && watch.ElapsedMilliseconds < 10000)
			{
				Thread.Yield();
			}
			AssertEquals("form disposed", true, Mediator.form.IsDisposed);
		}

		public void TestFormIsNotCreated()
		{
			Mediator.ShowModalProgressForm(new Rectangle(100, 200, 300, 400), "status", 10);
			AssertNull("Not shown right away", Mediator.form);
			System.Threading.Thread.Sleep(500);
			AssertNull("Not shown right away", Mediator.form);

			Mediator.Dispose();
			AssertNull("form disposed", Mediator.form);
		}

		public void TestMultipleStatusUpdatesAreMergedIntoOne()
		{
			using (var logger = new SaveProgressMediator.Logger())
			{
				Mediator.ShowModalProgressForm(new Rectangle(100, 200, 300, 400), "status", 10);
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status2", 20);
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status3", 30);
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status4", 40);
				Mediator.ProgressTextChanged.WaitOne();

				Mediator.Dispose();
				Mediator.FormDisposed.WaitOne();
				AssertEquals(
					@"2: Show form
2: Update status: 'status4' percentComplete: 40
2: Dispose form",
					logger.LogThread2.ToString());
			}
		}

		public void TestHideDoesNotHappenIfNotVisible()
		{
			using (var logger = new SaveProgressMediator.Logger())
			{
				Mediator.ShowModalProgressForm(new Rectangle(100, 200, 300, 400), "status", 10);
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status2", 20);
				Mediator.HideForm();
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status3", 30);
				Mediator.ProgressTextChanged.WaitOne();
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status4", 40);
				Mediator.ProgressTextChanged.WaitOne();

				Mediator.Dispose();
				Mediator.FormDisposed.WaitOne();
				AssertEquals(
					@"2: Show form
2: Update status: 'status3' percentComplete: 30
2: Update status: 'status4' percentComplete: 40
2: Dispose form",
					logger.LogThread2.ToString());
			}
		}

		public void TestHideHappensIfVisibleAndThereIsAnotherDelayAfterHiding()
		{
			using (var logger = new SaveProgressMediator.Logger())
			{
				Mediator.ShowModalProgressForm(new Rectangle(100, 200, 300, 400), "status", 10);
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status2", 20);
				Mediator.ProgressTextChanged.WaitOne();
				Mediator.HideForm();
				Mediator.FormDisposed.WaitOne();
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status3", 30);
				Mediator.ProgressTextChanged.WaitOne();
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status4", 40);
				Mediator.ProgressTextChanged.WaitOne();
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status5", 50);
				Mediator.ProgressTextChanged.WaitOne();

				Mediator.Dispose();
				Mediator.FormDisposed.WaitOne();
				AssertEquals(
					@"2: Show form
2: Update status: 'status2' percentComplete: 20
2: Dispose form
2: Show form
2: Update status: 'status3' percentComplete: 30
2: Update status: 'status4' percentComplete: 40
2: Update status: 'status5' percentComplete: 50
2: Dispose form",
					logger.LogThread2.ToString());
			}
		}

		public void TestProgressBoxGetsHiddenWhenModaliserShowsAForm()
		{
			using (var logger = new SaveProgressMediator.Logger())
			{
				Mediator.ShowModalProgressForm(new Rectangle(100, 200, 300, 400), "status", 10);
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status2", 20);
				Mediator.ProgressTextChanged.WaitOne();
				try
				{
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate
					{ Mediator.HideForm(); });
					using (var form = new ZForm())
					using (var form2 = new ZForm())
					{
						ZFormModaliser.Show(form, form2);
					}
				}
				finally
				{
					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				}

				Mediator.Dispose();
				Mediator.FormDisposed.WaitOne();
				AssertEquals(
					@"2: Show form
2: Update status: 'status2' percentComplete: 20
2: Dispose form",
					logger.LogThread2.ToString());
			}
		}

		public void TestHideAndNeverShow()
		{
			using (var logger = new SaveProgressMediator.Logger())
			{
				Mediator.ShowModalProgressForm(new Rectangle(100, 200, 300, 400), "status", 10);
				Mediator.HideForm();
				System.Threading.Thread.Sleep(2100);

				Mediator.Dispose();
				AssertEquals("", logger.LogThread2.ToString());
			}
		}

		public void TestHideBeforeShow()
		{
			using (var logger = new SaveProgressMediator.Logger())
			{
				Mediator.ShowModalProgressForm(new Rectangle(100, 200, 300, 400), "status", 10);
				Mediator.HideForm();
				System.Threading.Thread.Sleep(2100);
				Mediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible("status2", 20);
				Mediator.ProgressTextChanged.WaitOne();
				Mediator.Dispose();
				Mediator.FormDisposed.WaitOne();

				AssertEquals(
					@"2: Show form
2: Update status: 'status2' percentComplete: 20
2: Dispose form",
					logger.LogThread2.ToString());
			}
		}

		#region Implementation

		SaveProgressMediator Mediator
		{
			get { return mediator ?? (mediator = new SaveProgressMediator()); }
		}
		SaveProgressMediator mediator;

		protected override void TearDown()
		{
			base.TearDown();
			if (mediator != null)
			{
				mediator.Dispose();
			}
		}

		#endregion
	}
}
