using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class AccountingPeriodFieldUserControlFormBasherTest : ZFormBasherTest
	{
		public void TestSetFilter()
		{
			using (AccountingPeriodFieldUserControl control = new AccountingPeriodFieldUserControl())
			{
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				AccountingPeriodField filter = new AccountingPeriodField(Factory);
				filter.DisplayName = "&Oof";
				filter.SetScheduleTask(scheduleTask);

				control.SetFilter(filter);
				AssertEquals("FieldGroupBox.Text", "&&Oof", control.FieldGroupBox.Text);

				AssertEquals("SinglePeriodEdit.Schedule", filter.SinglePeriodSchedule, control.SinglePeriodEdit.Schedule);
				AssertNotNull("SinglePeriodEdit.Schedule", control.SinglePeriodEdit.Schedule);

				AssertEquals("PeriodRangeEdit.LowSchedule", filter.FromPeriodSchedule, control.PeriodRangeEdit.LowSchedule);
				AssertNotNull("PeriodRangeEdit.LowSchedule", control.PeriodRangeEdit.LowSchedule);

				AssertEquals("PeriodRangeEdit.HighSchedule", filter.ToPeriodSchedule, control.PeriodRangeEdit.HighSchedule);
				AssertNotNull("PeriodRangeEdit.HighSchedule", control.PeriodRangeEdit.HighSchedule);

				AssertEquals("YearToPeriodEdit.Schedule", filter.YearToPeriodSchedule, control.YearToPeriodEdit.Schedule);
				AssertNotNull("YearToPeriodEdit.Schedule", control.YearToPeriodEdit.Schedule);
			}
		}

		public void TestControlReadOnlyStatuses()
		{
			using (ZForm form = new ZForm())
			{
				AccountingPeriodFieldUserControl control = new AccountingPeriodFieldUserControl();
				form.Controls.Add(control);
				form.Show();

				AccountingPeriodField filter = new AccountingPeriodField(Factory);
				filter.UseAllPeriods = true;

				control.SetFilter(filter);
				AssertEquals("SinglePeriodEdit.ReadOnly", true, control.SinglePeriodEdit.ReadOnly);
				AssertEquals("PeriodRangeEdit.ReadOnly", true, control.PeriodRangeEdit.ReadOnly);
				AssertEquals("YearToPeriodEdit.ReadOnly", true, control.YearToPeriodEdit.ReadOnly);

				control.SingleRadioButton.PerformClick();
				AssertEquals("SinglePeriodEdit.ReadOnly", false, control.SinglePeriodEdit.ReadOnly);
				AssertEquals("PeriodRangeEdit.ReadOnly", true, control.PeriodRangeEdit.ReadOnly);
				AssertEquals("YearToPeriodEdit.ReadOnly", true, control.YearToPeriodEdit.ReadOnly);

				control.RangeRadioButton.PerformClick();
				AssertEquals("SinglePeriodEdit.ReadOnly", true, control.SinglePeriodEdit.ReadOnly);
				AssertEquals("PeriodRangeEdit.ReadOnly", false, control.PeriodRangeEdit.ReadOnly);
				AssertEquals("YearToPeriodEdit.ReadOnly", true, control.YearToPeriodEdit.ReadOnly);

				control.YearToPeriodRadioButton.PerformClick();
				AssertEquals("SinglePeriodEdit.ReadOnly", true, control.SinglePeriodEdit.ReadOnly);
				AssertEquals("PeriodRangeEdit.ReadOnly", true, control.PeriodRangeEdit.ReadOnly);
				AssertEquals("YearToPeriodEdit.ReadOnly", false, control.YearToPeriodEdit.ReadOnly);

				control.AllRadioButton.PerformClick();
				AssertEquals("SinglePeriodEdit.ReadOnly", true, control.SinglePeriodEdit.ReadOnly);
				AssertEquals("PeriodRangeEdit.ReadOnly", true, control.PeriodRangeEdit.ReadOnly);
				AssertEquals("YearToPeriodEdit.ReadOnly", true, control.YearToPeriodEdit.ReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(520);
			result.CaptionRenderingEnabled = true;
			result.Controls.Add(new AccountingPeriodFieldUserControl());
			return result;
		}
	}
}
