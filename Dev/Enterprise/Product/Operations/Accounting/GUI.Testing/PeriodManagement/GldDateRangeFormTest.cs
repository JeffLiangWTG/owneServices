using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	public abstract class GldDateRangeFormTest<T> : ZFormBasherTest
		where T : GldDateRangeSetting
	{
		public void TestDataSourceType()
		{
			using (var testForm = GetGldDateRangeForm())
			{
				AssertType<T>(testForm.DataSource);
			}
		}

		public abstract void TestOKButtonClick();

		public void TestOKButtonClick_WhenAnyError()
		{
			using (var testForm = GetGldDateRangeForm())
			{
				testForm.Show();
				var button = testForm.GetControl<ZButton>("OKButton");

				var setting = testForm.DataSource as T;
				AssertMessageWhenPropertyHasError("When Start Date has Error.", setting.StartDateInfo, button);
				AssertMessageWhenPropertyHasError("When End Date has Error.", setting.EndDateInfo, button);
			}

			void AssertMessageWhenPropertyHasError(string comment, ZPropertyInfo info, ZButton okButton)
			{
				info.ClearAllNotifications();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				info.AddError("Dummy Error");
				okButton.PerformClick();
				AssertContains(comment, "There are errors that need to be corrected.", UnitTestUserNotification.Instance.LastMessage.Text);

				info.ClearAllNotifications();
			}
		}

		public void TestCloseButtonClick()
		{
			using (var testForm = GetGldDateRangeForm())
			{
				testForm.Show();
				AssertEquals("PreCondition, DialogResult before clicking button.", DialogResult.None, testForm.DialogResult);

				var button = testForm.GetControl<ZButton>("CloseButton");
				button.PerformClick();
				AssertEquals("DialogResult after clicking button", DialogResult.Cancel, testForm.DialogResult);
			}
		}

		protected sealed override Form GetFormToBashCore() => GetGldDateRangeForm();

		protected abstract GldDateRangeForm<T> GetGldDateRangeForm();

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
