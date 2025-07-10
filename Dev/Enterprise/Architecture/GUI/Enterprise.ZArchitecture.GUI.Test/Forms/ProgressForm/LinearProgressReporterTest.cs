using System.Text.RegularExpressions;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.ProgressForm
{
	public class LinearProgressReporterTest : TestCase
	{
		public void TestWhenCancelled_ShouldUpdateReporter()
		{
			using (var parentForm = new ZChildForm())
			using (var reporter = new LinearProgressReporter(4, allowCancel: true))
			{
				parentForm.Show();
				reporter.ShowForm(parentForm, "");

				reporter.ReportOneItemProcessed();

				AssertMatch(new Regex(Regex.Escape("Items Processed: 1 / 4.\r\nEstimated Time Remaining: ") + @"\d seconds"), reporter.form.Status);
				AssertEquals(25, reporter.form.PercentComplete);
				AssertEquals(false, reporter.IsCancelled);

				reporter.form.CancelButton.PerformClick();

				AssertEquals(true, reporter.IsCancelled);
			}
		}

		public void TestWhenDisposed_ShouldDisposeForm()
		{
			using (var parentForm = new ZChildForm())
			using (var reporter = new LinearProgressReporter(10))
			{
				parentForm.Show();
				reporter.ShowForm(parentForm, "");

				AssertEquals(false, reporter.form.IsDisposed);

				reporter.Dispose();

				AssertEquals(true, reporter.form.IsDisposed);
			}
		}

		public void TestReportsLinearly()
		{
			const int amountToProcess = 5;

			using (var parentForm = new ZChildForm())
			using (var reporter = new LinearProgressReporter(amountToProcess))
			{
				parentForm.Show();
				reporter.ShowForm(parentForm, "");

				AssertEquals("Nothing is done yet - should be at 0", 0, reporter.form.PercentComplete);

				for (var i = 1; i <= amountToProcess; i++)
				{
					reporter.Report(i);

					AssertEquals(reporter.form.PercentComplete, i * (100 / amountToProcess)); //20, 30, 60, 80, 100
				}
			}
		}
	}
}
