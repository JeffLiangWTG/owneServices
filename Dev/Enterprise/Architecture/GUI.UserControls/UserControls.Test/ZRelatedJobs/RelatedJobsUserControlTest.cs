using System.Drawing;
using CargoWise.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class RelatedJobsUserControlTest : RelatedJobsTest
	{
		#region TestEditButtonClickOpensEditForm, TestRowDoubleClickOpensEditForm

		public void TestEditButtonClickOpensEditForm()
		{
			Form.Show();
			Factory.Save(); // Dummy must exist in DB for the edit form to open it

			JobsUserControl.EditJobButton.PerformClick();
			AssertEditFormWasShown();
		}

		public void TestRowDoubleClickOpensEditForm()
		{
			Form.Show();
			Factory.Save(); // Dummy must exist in DB for the edit form to open it

			DoubleClickGrid(new Point(50, 30));
			AssertEditFormWasShown();
		}

		void AssertEditFormWasShown()
		{
			ZDummyForm dummyForm = (ZDummyForm)JobsUserControl.ControllerForTest.LastShownForm;
			AssertEquals(ODisplayMode.Browse, dummyForm.DisplayMode);
			dummyForm.Dispose();
		}

		#endregion

		#region TestEditButtonIsDisabledIfNoJobSelected

		public void TestEditButtonIsDisabledIfNoJobSelected()
		{
			Dummy.RelatedJobs.RemoveAll();
			Form.Show();
			AssertEquals(false, JobsUserControl.EditJobButton.Enabled);

			Dummy.RelatedJobs.Add(Dummy);
			AssertEquals(true, JobsUserControl.EditJobButton.Enabled);

			Dummy.RelatedJobs.RemoveAll();
			AssertEquals(false, JobsUserControl.EditJobButton.Enabled);
		}

		#endregion

		public void TestModuleResultsBusinessObject_WhenBizOHasRelatedJobs_IsNotNull()
		{
			var relatedJobs = new RelatedJobCollection(Factory);
			relatedJobs.Add(Dummy);
			Factory.Save();

			using (var form = new ZForm(relatedJobs))
			using (var control = new RelatedJobsUserControl())
			{
				control.RelatedJobsGrid.DataSource = relatedJobs;
				form.Controls.Add(control);
				form.Show();

				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					var popUpForm = (IBusinessForm)dialog;
					AssertNotNull(popUpForm.ModuleResultsBusinessObject);
					ErrorReporter.Clear();
				});

				ZFormModaliser.ShowDialogsInTest = true;
				control.EditJobButton.PerformClick();
			}
		}
	}
}
