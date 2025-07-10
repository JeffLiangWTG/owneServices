using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class SchedulablePeriodRangeEditTest : ZFormBasherTest
	{
		public void TestBindToLowAndBindToHigh()
		{
			using (SchedulablePeriodRangeEdit control = new SchedulablePeriodRangeEdit())
			{
				control.BindToLow = "x";
				control.BindToHigh = "y";
				AssertEquals("BindToLow", "x", control.BindToLow);
				AssertEquals("LowValuePeriodEdit.BindTo", "x", control.LowValuePeriodEdit.BindTo);
				AssertEquals("BindToHigh", "y", control.BindToHigh);
				AssertEquals("HighValuePeriodEdit.BindTo", "y", control.HighValuePeriodEdit.BindTo);
			}
		}

		public void TestSetSchedules()
		{
			using (SchedulablePeriodRangeEdit control = new SchedulablePeriodRangeEdit())
			{
				AccPeriodSchedule lowSchedule = new AccPeriodSchedule();
				AccPeriodSchedule highSchedule = new AccPeriodSchedule();
				control.SetSchedules(lowSchedule, highSchedule);
				AssertEquals("LowValuePeriodEdit.Schedule", lowSchedule, control.LowValuePeriodEdit.Schedule);
				AssertEquals("LowSchedule", lowSchedule, control.LowSchedule);
				AssertEquals("HighValuePeriodEdit.Schedule", highSchedule, control.HighValuePeriodEdit.Schedule);
				AssertEquals("HighSchedule", highSchedule, control.HighSchedule);
			}
		}

		public void TestResizingSnapsToProperSize()
		{
			using (SchedulablePeriodRangeEdit control = new SchedulablePeriodRangeEdit())
			{
				control.Size = new Size(500, 600);
				AssertEquals("Size", new Size(control.HighValuePeriodEdit.Right, control.HighValuePeriodEdit.Bottom), control.Size);
			}
		}

		public void TestReadOnly()
		{
			using (SchedulablePeriodRangeEdit control = new SchedulablePeriodRangeEdit())
			{
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("LowValuePeriodEdit.ReadOnly", false, control.LowValuePeriodEdit.ReadOnly);
				AssertEquals("HighValuePeriodEdit.ReadOnly", false, control.HighValuePeriodEdit.ReadOnly);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("LowValuePeriodEdit.ReadOnly", true, control.LowValuePeriodEdit.ReadOnly);
				AssertEquals("HighValuePeriodEdit.ReadOnly", true, control.HighValuePeriodEdit.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("LowValuePeriodEdit.ReadOnly", false, control.LowValuePeriodEdit.ReadOnly);
				AssertEquals("HighValuePeriodEdit.ReadOnly", false, control.HighValuePeriodEdit.ReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Width = 400;
			result.Controls.Add(new SchedulablePeriodRangeEdit());
			return result;
		}
	}
}
