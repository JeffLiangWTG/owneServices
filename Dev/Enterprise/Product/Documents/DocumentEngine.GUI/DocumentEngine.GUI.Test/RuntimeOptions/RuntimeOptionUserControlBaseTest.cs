using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestsSubclassesOf(typeof(RuntimeOptionUserControl))]
	internal abstract class RuntimeOptionUserControlBaseTest<T> : TestCaseWithFactory where T : RuntimeOptionUserControl, new()
	{
		public virtual void TestChangeLabelSizeForAlignment()
		{
			using (var control = new T())
			{
				ZLabel label;
				if ((label = control.Controls.OfType<ZLabel>().SingleOrDefault()) != null)
				{
					control.Width = 500;
					control.ChangeLabelSizeForAlignment(200);
					AssertEquals("Label width should be 192", 200 - label.Left, label.Width);
					var editControl = control.Controls.Cast<Control>().Except(new[] { label }).Single();
					var expectedSize = editControl is SchedulableDateEdit || editControl is SchedulableDateTimeOffsetEdit ? 140 : editControl is SchedulablePeriodEdit ? 157 : editControl is SchedulablePeriodRangeEdit ? 218 : 300;
					AssertEquals("Control width should be 300", expectedSize, editControl.Width);
				}
				else
				{
					control.Width = 500;
					control.ChangeLabelSizeForAlignment(200);
					AssertEquals("Control left should be 200", 200, control.Controls[0].Left);
					AssertEquals("Control width should be 300", 300, control.Controls[0].Width);
				}
			}
		}

		public virtual void TestDesiredCaptionWidth()
		{
			using (var control = new T())
			{
				ZLabel label;
				if ((label = control.Controls.OfType<ZLabel>().SingleOrDefault()) != null)
				{
					var testText = "Expected default value of Label..Nooot!";
					label.CaptionResourceString = Res.GetData("31c26e92-cc35-44a7-b3cd-7641215c5de9", testText);
					label.GetExtension<ILabelCaptionRenderer>().Caption = testText;
					AssertEquals("Widths should be equal", TextRenderer.MeasureText(testText, label.GetExtension<ILabelCaptionRenderer>()?.Font).Width + label.Padding.Horizontal, control.DesiredCaptionWidth);
				}
				else
				{
					var renderer = control.Controls[0].GetExtension<ILabelCaptionRenderer>();
					var testText = "Some text to measure anything";
					renderer.Caption = testText;
					AssertEquals("renderer should have measured the same value", TextRenderer.MeasureText(testText, renderer.Font).Width, control.DesiredCaptionWidth);
				}
			}
		}

		public void TestDesiredCaptionWidthDoesNotThrowException()
		{
			using (var t = new T())
			{
				AssertNoExceptionThrown(() => t.ChangeLabelSizeForAlignment(t.DesiredCaptionWidth));
			}
		}
	}
}
