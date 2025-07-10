using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Progress.Test
{
	public class ProgressFormAdapterTest : TestCase
	{
		public void TestSetExpectedCount()
		{
			using (ProgressForm form = new ProgressForm())
			{
				ProgressFormAdapter progress = new ProgressFormAdapter(form);
				progress.SetExpectedCount(9);
				AssertEquals(9, progress.ExpectedCount);
				AssertEquals(-1, progress.CurrentCount);
			}
		}

		public void TestUpdateCurrentCount()
		{
			using (ProgressForm form = new ProgressForm())
			{
				ProgressFormAdapter progress = new ProgressFormAdapter(form);
				progress.SetExpectedCount(2);
				progress.UpdateCurrentCount("frist!");
				AssertEquals(2, progress.ExpectedCount);
				AssertEquals(0, progress.CurrentCount);
				AssertEquals("frist! (1 of 2)", form.Status);
				AssertEquals(0, form.PercentComplete);

				progress.UpdateCurrentCount("second");
				AssertEquals(1, progress.CurrentCount);
				AssertEquals("second (2 of 2)", form.Status);
				AssertEquals(50, form.PercentComplete);

				progress.UpdateCurrentCount("extra");
				AssertEquals(2, progress.CurrentCount);
				AssertEquals("extra (3 of 2)", form.Status);
				AssertEquals(100, form.PercentComplete);
			}
		}

		public void TestExpectedCountZero()
		{
			using (ProgressForm form = new ProgressForm())
			{
				ProgressFormAdapter progress = new ProgressFormAdapter(form);
				progress.SetExpectedCount(0);
				AssertEquals(0, progress.ExpectedCount);
				AssertEquals(-1, progress.CurrentCount);

				progress.UpdateCurrentCount("1");
				AssertEquals(0, progress.ExpectedCount);
				AssertEquals(0, progress.CurrentCount);
				AssertEquals("1 (1 of 0)", form.Status);
				AssertEquals(0, form.PercentComplete);

				progress.UpdateCurrentCount("2");
				AssertEquals(0, progress.ExpectedCount);
				AssertEquals(1, progress.CurrentCount);
				AssertEquals("2 (2 of 0)", form.Status);
				AssertEquals(0, form.PercentComplete);
			}
		}

		public void TestSetStatusAndPercentComplete()
		{
			using (ProgressForm form = new ProgressForm())
			{
				ProgressFormAdapter progress = new ProgressFormAdapter(form);
				progress.SetStatusAndPercentComplete("AA", 49);
				AssertEquals("AA", form.Status);
				AssertEquals(49, form.PercentComplete);
			}
		}

		public void TestIsCancelled()
		{
			using (Form parent = new Form())
			using (ProgressForm form = new ProgressForm())
			{
				ProgressFormAdapter progress = new ProgressFormAdapter(form);
				ZFormModaliser.Show(form, parent);
				AssertEquals(false, progress.IsCancelled);
				form.CancelButton.PerformClick();
				AssertEquals(true, progress.IsCancelled);
			}
		}
	}
}