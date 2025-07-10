using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class SchedulableDateEditTest : ZFormBasherTest
	{
		[GuiTest]
		public void TestControlDoesntGetCutOffWhenYouGoToLongDate()
		{
			using (SchedulableDateEdit control = new SchedulableDateEdit())
			{
				control.DateTimeFormat = ZDateTimePickerFormat.Short;
				control.SetSchedule(null);
				using (var form = new ZForm())
				{
					form.Controls.Add(control);
					form.Show();

					Assert("control.Width >= control.ValueDateEdit.Right with Short Date Format", control.Width >= control.ValueDateEdit.Right);
				}
			}

			using (var control = new SchedulableDateEdit())
			{
				control.DateTimeFormat = ZDateTimePickerFormat.Long;
				control.SetSchedule(null);
				using (var form = new ZForm())
				{
					form.Controls.Add(control);
					form.Show();

					Assert("control.Width >= control.ValueDateEdit.Right with Long Date Format", control.Width >= control.ValueDateEdit.Right);
				}
			}
		}

		[GuiTest]
		public void TestSizeAndEditButtonWhenYouGoToLongDate()
		{
			using (var control = new SchedulableDateEdit())
			{
				control.DateTimeFormat = ZDateTimePickerFormat.Short;
				var widthForShortDateFormat = control.Width;
				var valueDateEditWidthForShortDateFormat = control.ValueDateEdit.Width;
				var editButtonOffset = control.EditButton.Location.X;

				control.DateTimeFormat = ZDateTimePickerFormat.Long;
				control.SetSchedule(null);
				Assert("widthForLongDateFormat > widthForShortDateFormat", control.Width > widthForShortDateFormat);
				Assert("Size of valueDateEdit should be longer wih longer date format", control.ValueDateEdit.Width > valueDateEditWidthForShortDateFormat);
				Assert("Edit button should be further to the left on short format", editButtonOffset < control.EditButton.Location.X);
			}
		}

		[GuiTest]
		public void TestSwitchingSchedulesWithDifferentPeriodTypesDoesntChangeLength()
		{
			using (var control = new SchedulableDateEdit())
			{
				control.DateTimeFormat = ZDateTimePickerFormat.Long;
				var schedule = new DateSchedule();
				schedule.ByHourAndMinute = true;
				control.SetSchedule(schedule);
				var hourMinuteWidth = control.ValueDateEdit.Width;
				schedule.ByHourAndMinute = false;
				schedule.ByWeek = true;
				var nonHourMinuteWidth = control.ValueDateEdit.Width;

				AssertEquals("Switching to non hour/minute scheduling shouldn't change width of control", hourMinuteWidth, nonHourMinuteWidth);
			}
		}

		[GuiTest]
		public void TestSetSchedule()
		{
			using (var control = new SchedulableDateEdit())
			{
				control.DateTimeFormat = ZDateTimePickerFormat.Short;
				control.SetSchedule(null);
				var widthForShortDateFormat = control.Width;
				var heightForShortDateFormat = control.Height;

				var schedule = new DateSchedule();
				control.DateTimeFormat = ZDateTimePickerFormat.Long;
				control.SetSchedule(schedule);
				var widthForLongDateFormat = control.Width;
				var heightForLongDateFormat = control.Height;
				AssertEquals("DateTimeFormat default value", ZDateTimePickerFormat.Short, control.DateTimeFormat);
				AssertEquals("Schedule", schedule, control.Schedule);
				AssertEquals("EditButton.Visible", true, control.EditButton.Visible);
				AssertEquals("Long date format should not mean greater height", control.Height, heightForShortDateFormat);

				control.SetSchedule(null);
				var bottom = control.ValueDateEdit.Bottom + ControlDpiScalingHelper.MarkAsScaled(2);
				AssertNull("Schedule", control.Schedule);
				AssertEquals("EditButton.Visible", false, control.EditButton.Visible);
				AssertEquals("Size", new Size(widthForLongDateFormat, heightForLongDateFormat), control.Size);

				using (var form = new ZForm())
				{
					form.Controls.Add(control);
					control.SetSchedule(schedule);
					form.Show();
					AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, control.DateTimeFormat);
					AssertEquals("Schedule", schedule, control.Schedule);
					AssertEquals("EditButton.Visible", true, control.EditButton.Visible);
				}
			}
		}

		public void TestBindTo()
		{
			using (var control = new SchedulableDateEdit())
			{
				control.BindTo = "x";
				AssertEquals("BindTo", "x", control.BindTo);
				AssertEquals("ValueDateEdit.BindTo", "x", control.ValueDateEdit.BindTo);
			}
		}

		public void TestDateTimeFormat()
		{
			using (var control = new SchedulableDateEdit())
			{
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, control.DateTimeFormat);
				AssertEquals("ValueDateEdit.DateTimeFormat", ZDateTimePickerFormat.Short, control.ValueDateEdit.DateTimeFormat);

				control.DateTimeFormat = ZDateTimePickerFormat.Long;
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, control.DateTimeFormat);
				AssertEquals("ValueDateEdit.DateTimeFormat", ZDateTimePickerFormat.Long, control.ValueDateEdit.DateTimeFormat);

				control.SetSchedule(new DateSchedule());
				control.DateTimeFormat = ZDateTimePickerFormat.Time;
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, control.DateTimeFormat);
				AssertEquals("ValueDateEdit.DateTimeFormat", ZDateTimePickerFormat.Short, control.ValueDateEdit.DateTimeFormat);
			}
		}

		public void TestResizingSnapsToProperSize()
		{
			using (var control = new SchedulableDateEdit())
			{
				control.Size = new Size(500, 600);
				AssertEquals("Size", new Size(control.EditButton.Right, control.EditButton.Bottom), control.Size);
			}
		}

		[GuiTest]
		public void TestEditSchedule()
		{
			using (var control = new SchedulableDateEdit())
			{
				var schedule = new DateSchedule();
				schedule.DayName = "!";
				control.SetSchedule(schedule);
				control.EditButton.PerformClick();
				using (DateScheduleForm scheduleForm = (DateScheduleForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("ZFormModaliser.LastFormShownDialogForTest.BusinessEntity.DayName", "!", ((DateSchedule)scheduleForm.LastDataSourceForTest).DayName);
				}
			}
		}

		public void TestErrorReportedWhenNullReferenceExceptionThrownInSetSchedule()
		{
			using (SchedulableDateEdit control = new SchedulableDateEdit())
			{
				DateSchedule schedule = new DateSchedule();
				var valueDateEdit = control.ValueDateEdit_Exposed;

				control.ValueDateEdit_Exposed = null;
				control.SetSchedule(schedule);
				AssertEquals("Error Setting Schedule, ValueDateEdit is null, EditButton is not null.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				control.ValueDateEdit_Exposed = valueDateEdit;
				control.EditButton_Exposed = null;
				control.SetSchedule(schedule);
				AssertEquals("Error Setting Schedule, ValueDateEdit is not null, EditButton is null.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Controls.Add(new SchedulableDateEdit());
			return result;
		}
	}
}
