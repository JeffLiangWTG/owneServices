using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class SchedulablePeriodEditTest : ZFormBasherTest
	{
		public void TestBindTo()
		{
			using (SchedulablePeriodEdit control = new SchedulablePeriodEdit())
			{
				control.BindTo = "x";
				AssertEquals("BindTo", "x", control.BindTo);
				AssertEquals("ValuePeriodEdit.BindTo", "x", control.ValuePeriodEdit.BindTo);
			}
		}

		[GuiTest]
		public void TestSetSchedule()
		{
			using (SchedulablePeriodEdit control = new SchedulablePeriodEdit())
			{
				AccPeriodSchedule schedule = new AccPeriodSchedule();
				control.SetSchedule(schedule);
				AssertEquals("Schedule", schedule, control.Schedule);
				AssertEquals("EditButton.Visible", true, control.EditButton.Visible);
				AssertEquals("Size", new Size(control.EditButton.Right, control.EditButton.Bottom), control.Size);

				control.SetSchedule(null);
				AssertNull("Schedule", control.Schedule);
				AssertEquals("EditButton.Visible", false, control.EditButton.Visible);
				AssertEquals("Size", new Size(control.EditButton.Left, control.ValuePeriodEdit.Bottom), control.Size);

				using (ZForm form = new ZForm())
				{
					form.Controls.Add(control);
					control.SetSchedule(schedule);
					form.Show();
					AssertEquals("Schedule", schedule, control.Schedule);
					AssertEquals("EditButton.Visible", true, control.EditButton.Visible);
					AssertEquals("Size", new Size(control.EditButton.Right, control.EditButton.Bottom), control.Size);
				}
			}
		}

		[GuiTest]
		public void TestEditSchedule()
		{
			using (SchedulablePeriodEdit control = new SchedulablePeriodEdit())
			{
				AccPeriodSchedule schedule = new AccPeriodSchedule();
				schedule.PeriodScope = "!";
				control.SetSchedule(schedule);
				control.EditButton.PerformClick();
				using (AccPeriodScheduleForm scheduleForm = (AccPeriodScheduleForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("ZFormModaliser.LastFormShownDialogForTest.BusinessEntity.PeriodScope", "!", ((Schedule)scheduleForm.LastDataSourceForTest).PeriodScope);
				}
			}
		}

		public void TestResizingSnapsToProperSize()
		{
			using (SchedulablePeriodEdit control = new SchedulablePeriodEdit())
			{
				control.Size = new Size(500, 600);
				AssertEquals("Size", new Size(control.EditButton.Right, control.EditButton.Bottom), control.Size);
			}
		}

		public void TestReadOnly()
		{
			using (SchedulablePeriodEdit control = new SchedulablePeriodEdit())
			{
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("ValuePeriodEdit.ReadOnly", false, control.ValuePeriodEdit.ReadOnly);
				AssertEquals("EditButton.ReadOnly", false, control.EditButton.ReadOnly);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("ValuePeriodEdit.ReadOnly", true, control.ValuePeriodEdit.ReadOnly);
				AssertEquals("EditButton.ReadOnly", true, control.EditButton.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("ValuePeriodEdit.ReadOnly", false, control.ValuePeriodEdit.ReadOnly);
				AssertEquals("EditButton.ReadOnly", false, control.EditButton.ReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Controls.Add(new SchedulablePeriodEdit());
			return result;
		}
	}
}
