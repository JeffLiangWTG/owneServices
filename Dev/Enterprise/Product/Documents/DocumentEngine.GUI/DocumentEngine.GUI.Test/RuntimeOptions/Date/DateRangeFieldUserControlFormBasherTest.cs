using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class DateRangeFieldUserControlFormBasherTest : ZFormBasherTest
	{
		public void TestSetFilter()
		{
			using (var form = new ZForm())
			using (DateRangeFieldUserControl control = new DateRangeFieldUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				DateRangeField filter = new DateRangeField(Factory);
				filter.DisplayName = "&Chomp";
				filter.PickerFormat = DocEngineDatePickerFormats.Long;

				control.SetFilter(filter);
				AssertEquals("FieldLabel.Text", "&&Chomp", control.FieldLabel.Text);
				AssertEquals("FromDateEdit.DateTimeFormat", ZDateTimePickerFormat.Long, control.dateControl.FromDateEdit.DateTimeFormat);
				AssertEquals("ToDateEdit.DateTimeFormat", ZDateTimePickerFormat.Long, control.dateControl.ToDateEdit.DateTimeFormat);
				AssertNull("FromDateEdit.Schedule", control.dateControl.FromDateEdit.Schedule);
				AssertNull("ToDateEdit.Schedule", control.dateControl.ToDateEdit.Schedule);

				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				filter.SetScheduleTask(scheduleTask);
				control.SetFilter(filter);
				AssertEquals("FromDateEdit.Schedule", filter.LowSchedule, control.dateControl.FromDateEdit.Schedule);
				AssertEquals("ToDateEdit.Schedule", filter.HighSchedule, control.dateControl.ToDateEdit.Schedule);
				AssertNotNull("FromDateEdit.Schedule", control.dateControl.FromDateEdit.Schedule);
				AssertNotNull("ToDateEdit.Schedule", control.dateControl.ToDateEdit.Schedule);
			}
		}

		public void TestLabelCaptionNotPrematurelyTruncates()
		{
			AssertLabelCaptionNotTruncated("ETA", 34);
			AssertLabelCaptionNotTruncated("ETD", 34);
			AssertLabelCaptionNotTruncated("First Dhcg ATA", 86);
		}

		public void TestLabelCaptionTruncatesLongCaptions()
		{
			AssertLabelCaptionTruncated("First Dhcg ATA", 58);
			AssertLabelCaptionTruncated("WTGWTGWTGWTGWTGWTGWTGWTGWTGWTGWTGWTGWTG", 99);
		}

		public void AssertLabelCaptionNotTruncated(string caption, int expectedWidth)
		{
			using (var form = new ZForm())
			using (var control = new DateRangeFieldUserControl())
			{
				form.Controls.Add(control);

				var filter = new DateRangeField(Factory);
				filter.DisplayName = caption;

				control.ChangeLabelSizeForAlignment(expectedWidth);
				control.SetFilter(filter);

				form.Show();
				Application.DoEvents();

				AssertEquals(caption, control.FieldLabel.Text);
			}
		}

		public void AssertLabelCaptionTruncated(string caption, int expectedWidth)
		{
			using (var form = new ZForm())
			using (var control = new DateRangeFieldUserControl())
			{
				form.Controls.Add(control);

				var filter = new DateRangeField(Factory);
				filter.DisplayName = caption;

				control.ChangeLabelSizeForAlignment(expectedWidth);
				control.SetFilter(filter);

				form.Show();
				Application.DoEvents();

				var assertMessage = "Should start with " + caption[0];
				AssertNotEquals(caption, control.FieldLabel.Text);
				AssertStartsWith(assertMessage, caption.Substring(0, 1), control.FieldLabel.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Width = 700;
			result.Controls.Add(new DateRangeFieldUserControl());
			return result;
		}
	}
}
