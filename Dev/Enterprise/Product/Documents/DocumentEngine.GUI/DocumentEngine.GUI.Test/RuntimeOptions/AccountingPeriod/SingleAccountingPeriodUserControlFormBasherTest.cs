using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class SingleAccountingPeriodUserControlFormBasherTest : ZFormBasherTest
	{
		public void TestSetFilter()
		{
			using (var form = new ZForm())
			using (SingleAccountingPeriodUserControl control = new SingleAccountingPeriodUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				SingleAccountingPeriodField filter = new SingleAccountingPeriodField(Factory);
				filter.DisplayName = "&Blah";
				filter.SetScheduleTask(scheduleTask);

				control.SetFilter(filter);
				AssertEquals("FieldLabel.Text", "&&Blah", control.FieldLabel.Text);
				AssertEquals("FieldPeriodEdit.Schedule", filter.Schedule, control.FieldPeriodEdit.Schedule);
				AssertNotNull("FieldPeriodEdit.Schedule", control.FieldPeriodEdit.Schedule);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Controls.Add(new SingleAccountingPeriodUserControl());
			return result;
		}
	}
}
