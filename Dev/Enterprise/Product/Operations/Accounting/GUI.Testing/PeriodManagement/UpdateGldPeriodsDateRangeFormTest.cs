using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	[TestedType(typeof(UpdateGldPeriodsDateRangeForm))]
	public class UpdateGldPeriodsDateRangeFormTest : GldDateRangeFormTest<UpdateGldPeriodsDateRangeSetting>
	{
		public void TestFormCaption()
		{
			using (var testForm = (UpdateGldPeriodsDateRangeForm)GetFormToBash())
			{
				testForm.Show();
				AssertEquals("Update General Ledger Data Records", testForm.Text);
			}
		}

		public override void TestOKButtonClick()
		{
			using (var testForm = GetFormToBash())
			{
				testForm.Show();
				var button = testForm.GetControl<ZButton>("OKButton");

				AssertNoErrors("PreCondition, setting has no error", UpdateGldPeriodsDateRangeSetting);

				button.PerformClick();
				AssertContains("All journal entries within the specified date range have been successfully updated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override GldDateRangeForm<UpdateGldPeriodsDateRangeSetting> GetGldDateRangeForm()
			=> new UpdateGldPeriodsDateRangeForm(UpdateGldPeriodsDateRangeSetting);

		UpdateGldPeriodsDateRangeSetting UpdateGldPeriodsDateRangeSetting => updateGldPeriodsDateRangeSetting ?? (updateGldPeriodsDateRangeSetting = new UpdateGldPeriodsDateRangeSetting());
		UpdateGldPeriodsDateRangeSetting updateGldPeriodsDateRangeSetting;
	}
}
