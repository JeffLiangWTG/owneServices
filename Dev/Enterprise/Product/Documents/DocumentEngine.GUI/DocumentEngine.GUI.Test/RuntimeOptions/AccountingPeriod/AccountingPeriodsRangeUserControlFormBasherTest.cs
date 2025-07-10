using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class AccountingPeriodsRangeUserControlFormBasherTest : ZFormBasherTest
	{
		public void TestSetFilter()
		{
			using (var form = new ZForm())
			using (AccountingPeriodsRangeUserControl control = new AccountingPeriodsRangeUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				AccountingPeriodsRangeField filter = new AccountingPeriodsRangeField(Factory);
				filter.DisplayName = "&Chomp";
				filter.SetScheduleTask(scheduleTask);

				control.SetFilter(filter);
				AssertEquals("FieldLabel.Text", "&&Chomp", control.FieldLabel.Text);
				AssertEquals("PeriodRangeEdit.LowSchedule", filter.LowSchedule, control.PeriodRangeEdit.LowSchedule);
				AssertNotNull("PeriodRangeEdit.LowSchedule", control.PeriodRangeEdit.LowSchedule);
				AssertEquals("PeriodRangeEdit.HighSchedule", filter.HighSchedule, control.PeriodRangeEdit.HighSchedule);
				AssertNotNull("PeriodRangeEdit.HighSchedule", control.PeriodRangeEdit.HighSchedule);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Width = 500;
			result.Controls.Add(new AccountingPeriodsRangeUserControl());
			return result;
		}
	}
}
