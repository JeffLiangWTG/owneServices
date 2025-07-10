using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class DateTimeOffsetFieldUserControlFormBasherTest : ZFormBasherTest
	{
		public void TestSetFilter()
		{
			using (var control = new DateTimeOffsetFieldUserControl())
			{
				var filter = new DateTimeOffsetField(Factory);
				filter.DisplayName = "&Chomp";
				filter.PickerFormat = DocEngineDatePickerFormats.Long;

				control.SetFilter(filter);
				AssertEquals("FieldLabel.Text", "&&Chomp", control.FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("FieldDateEdit.DateTimeFormat", ZDateTimePickerFormat.Long, control.FieldDateEdit.DateTimeFormat);
				AssertNull("FieldDateEdit.Schedule", control.FieldDateEdit.Schedule);

				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				filter.SetScheduleTask(scheduleTask);
				control.SetFilter(filter);
				AssertEquals("FieldDateEdit.Schedule", filter.Schedule, control.FieldDateEdit.Schedule);
				AssertNotNull("FieldDateEdit.Schedule", control.FieldDateEdit.Schedule);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Controls.Add(new DateTimeOffsetFieldUserControl());
			return result;
		}
	}
}
