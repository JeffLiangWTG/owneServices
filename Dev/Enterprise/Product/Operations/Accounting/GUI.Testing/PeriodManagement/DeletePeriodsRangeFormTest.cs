using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GUI.PeriodManagement;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.PeriodManagement
{
	[TestedType(typeof(DeletePeriodsFromForm))]
	public class DeletePeriodsRangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DeletePeriodsFromForm(DeletePeriodsRangeSetting, new PeriodManager(Factory));
		}

		public void TestFormCaption()
		{
			using (var testForm = (DeletePeriodsFromForm)GetFormToBash())
			{
				testForm.Show();
				AssertEquals("Delete Period From", testForm.Text);
			}
		}

		public void TestOKButtonClick()
		{
			using (var testForm = GetFormToBash())
			{
				testForm.Show();
				var button = testForm.GetControl<ZButton>("OKButton");

				DeletePeriodsRangeSetting.DeletePeriodsFromInfo.AddError("Please enter a Start Date");
				button.PerformClick();
				AssertContains(@"There are errors that need to be corrected.", UnitTestUserNotification.Instance.LastMessage.Text);

				DeletePeriodsRangeSetting.DeletePeriodsFromInfo.ClearAllNotifications();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriodsForEntireYear(2022);
				DeletePeriodsRangeSetting.DeletePeriodsFrom = new CargoWise.Types.ZDateTime(2022, 01, 01);
				button.PerformClick();
				AssertEquals("Are you sure you want to delete all periods from the specified date in company [Eagle Datamation International]?", UnitTestUserNotification.Instance.LastMessage.Text);

				DeletePeriodsRangeSetting.DeletePeriodsFrom = new CargoWise.Types.ZDateTime(2022, 07, 01);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				button.PerformClick();
				AssertEquals("All periods from specified date have been deleted", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		DeletePeriodsFromSetting DeletePeriodsRangeSetting => deletePeriodsRangeSetting ?? (deletePeriodsRangeSetting = new DeletePeriodsFromSetting());
		DeletePeriodsFromSetting deletePeriodsRangeSetting;
	}
}
