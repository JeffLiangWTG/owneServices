using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class GridEntityFormOpenerTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public void TestDoubleClick_WhenBusinessEntityHasChanges_ShouldShowDialog_AnsweringYes()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var childBizo = bizo.Collection.AddNew();
			Factory.Save();
			bizo.HasChanges = true;
			using (var form = new DummyFormWithGrid(bizo))
			{
				form.Show();
				form.Grid.PerformMouseDoubleClickForTest(0);
				AssertEquals("You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, bizo.HasChanges);
				using (var childForm = Application.OpenForms.OfType<ZDummyForm>().SingleOrDefault())
				{
					AssertNotNull(childForm);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public void TestDoubleClick_WhenBusinessEntityHasChanges_ShouldShowDialog_AnsweringNo()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var childBizo = bizo.Collection.AddNew();
			Factory.Save();
			bizo.HasChanges = true;
			using (var form = new DummyFormWithGrid(bizo))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.Show();
				form.Grid.PerformMouseDoubleClickForTest(0);
				AssertEquals("You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, bizo.HasChanges);
				using (var childForm = Application.OpenForms.OfType<ZDummyForm>().SingleOrDefault())
				{
					AssertNull(childForm);
				}
			}
		}

		public void TestDoubleClickRowHeader_ShouldNotShowDialog_WhenRequiringSaveBeforeOpeningGridEntityForm()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var childBizo = bizo.Collection.AddNew();
			Factory.Save();
			using (var form = new DummyFormWithGrid(bizo))
			{
				form.Show();
				form.Grid.PerformMouseDoubleClickForTest(-1);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDoubleClickRowHeader_ShouldNotShowDialog()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			var childBizo = bizo.Collection.AddNew();
			Factory.Save();
			using (var form = new DummyFormWithGrid(bizo))
			{
				form.RequireSavedFormBeforeOpeningGridEntityForm = false;
				form.Show();
				form.Grid.PerformMouseDoubleClickForTest(-1);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDoubleClickEmptyRow_ShouldNotShowDialog_WhenRequiringSaveBeforeOpeningGridEntityForm()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();
			using (var form = new DummyFormWithGrid(bizo))
			{
				form.Show();
				form.Grid.PerformMouseDoubleClickForTest(0);
				AssertEquals("Please select a saved row to edit.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDoubleClickEmptyRow_ShouldNotShowDialog()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();
			using (var form = new DummyFormWithGrid(bizo))
			{
				form.RequireSavedFormBeforeOpeningGridEntityForm = false;
				form.Show();
				form.Grid.PerformMouseDoubleClickForTest(0);
				AssertEquals("Please select a saved row to edit.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}