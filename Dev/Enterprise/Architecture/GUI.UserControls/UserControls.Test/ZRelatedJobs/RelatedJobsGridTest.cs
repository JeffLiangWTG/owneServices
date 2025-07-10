using System.Drawing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class RelatedJobsGridTest : RelatedJobsTest
	{
		#region TestConstructedState

		public void TestConstructedState()
		{
			AssertEquals(true, Grid.IsWholeRowSelectedOnClick);
		}

		#endregion

		#region TestRowChanged

		public void TestRowChanged_AfterBind()
		{
			Grid.RowChanged += delegate
			{
				Assert("RowChanged event should be fired on parent form .Show().", true);
			};

			Form.Show();
		}

		public void TestRowChanged_WhenChangingRows()
		{
			int rowChangedEventFired = 0;

			Grid.RowChanged += delegate
			{
				rowChangedEventFired++;
			};

			Form.Show(); // this should fire row changed once
			Dummy.RelatedJobs.Add(Dummy.Clone());
			Grid.ListManager.Position = 1; // this should fire it again

			AssertEquals(2, rowChangedEventFired);
		}

		#endregion

		#region TestRowDoubleClick

		public void TestRowDoubleClick()
		{
			Grid.RowDoubleClick += delegate
			{
				Assert("DoubleClick event was fired", true);
			};

			Form.Show();
			DoubleClickGrid(new Point(50, 30));

			((ZDummyForm)JobsUserControl.ControllerForTest.LastShownForm).Close();
		}

		public void TestRowDoubleClick_DoesNotFireIfMouseNotOverRow()
		{
			Grid.RowDoubleClick += delegate
			{
				Fail("DoubleClick event was fired when the mouse was not over a row.");
			};

			Form.Show();
			DoubleClickGrid(new Point(10, 10));
			Assert(true);
		}

		#endregion

		#region TestSelectedJob

		public void TestSelectedJob()
		{
			Form.Show();
			AssertEquals(Dummy, JobsUserControl.RelatedJobsGrid.SelectedJob);
		}

		#endregion
	}
}
