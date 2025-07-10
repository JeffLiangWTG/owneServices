using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlTestHelperTest : TestCase
	{
		public void TestAssertControlSize()
		{
			using (var form = NewScaledForm)
			{
				ControlTestHelper.AssertControlSize(850, 650, form);
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlSize(852, 648, form));
			}
		}

		public void TestAssertControlWidth()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form))
			{
				ControlTestHelper.AssertControlWidth(errorMessage, 800, control);
				ControlTestHelper.AssertControlWidth(errorMessage, 810, control, 10);
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlWidth(810, control));
			}
		}

		public void TestAssertControlHeight()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form))
			{
				ControlTestHelper.AssertControlHeight(errorMessage, 600, control);
				ControlTestHelper.AssertControlHeight(errorMessage, 610, control, 10);
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlHeight(610, control));
			}
		}

		public void TestAssertControlDimensions()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form))
			{
				ControlTestHelper.AssertControlDimensions(errorMessage, 800, 600, control);
				ControlTestHelper.AssertControlDimensions(errorMessage, 810, 590, control, 10);
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlDimensions(800, 610, control));
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlDimensions(790, 600, control));
			}
		}

		public void TestAssertControlXLocation()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form, 50, 50))
			{
				control.Location = ControlDpiScalingHelper.NewScaledPoint(100, 100);
				ControlTestHelper.AssertControlXLocation(errorMessage, 100, control);
				ControlTestHelper.AssertControlXLocation(errorMessage, 110, control, 10);
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlXLocation(110, control));
			}
		}

		public void TestAssertControlYLocation()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form, 50, 50))
			{
				control.Location = ControlDpiScalingHelper.NewScaledPoint(100, 100);
				ControlTestHelper.AssertControlYLocation(errorMessage, 100, control);
				ControlTestHelper.AssertControlYLocation(errorMessage, 110, control, 10);
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlYLocation(110, control));
			}
		}

		public void TestAssertControlLocation()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form, 50, 50))
			{
				control.Location = ControlDpiScalingHelper.NewScaledPoint(100, 200);
				ControlTestHelper.AssertControlLocation(errorMessage, 100, 200, control);
				ControlTestHelper.AssertControlLocation(errorMessage, 110, 190, control, 10);
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlLocation(110, 200, control));
				AssertExceptionThrown<AssertionFailedError>(() => ControlTestHelper.AssertControlLocation(100, 210, control));
			}
		}

		public void TestGetControlAbsoluteLeft()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form, 200, 200))
			using (var childControl = NewScaledControl(control, 50, 50))
			{
				control.Location = ControlDpiScalingHelper.NewScaledPoint(100, 100);
				childControl.Location = ControlDpiScalingHelper.NewScaledPoint(50, 50);

				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlTestHelper.GetControlAbsoluteLeft(control, form));
				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(50), ControlTestHelper.GetControlAbsoluteLeft(childControl, control));
				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(150), ControlTestHelper.GetControlAbsoluteLeft(childControl, form));
			}
		}

		public void TestGetControlAbsoluteRight()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form, 200, 200))
			using (var childControl = NewScaledControl(control, 50, 50))
			{
				control.Location = ControlDpiScalingHelper.NewScaledPoint(100, 100);
				childControl.Location = ControlDpiScalingHelper.NewScaledPoint(50, 50);

				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(300), ControlTestHelper.GetControlAbsoluteRight(control, form));
				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlTestHelper.GetControlAbsoluteRight(childControl, control));
				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(200), ControlTestHelper.GetControlAbsoluteRight(childControl, form));
			}
		}

		public void TestGetControlAbsoluteTop()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form, 200, 200))
			using (var childControl = NewScaledControl(control, 50, 50))
			{
				control.Location = ControlDpiScalingHelper.NewScaledPoint(100, 100);
				childControl.Location = ControlDpiScalingHelper.NewScaledPoint(50, 50);

				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlTestHelper.GetControlAbsoluteTop(control, form));
				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(50), ControlTestHelper.GetControlAbsoluteTop(childControl, control));
				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(150), ControlTestHelper.GetControlAbsoluteTop(childControl, form));
			}
		}

		public void TestGetControlAbsoluteBottom()
		{
			using (var form = NewScaledForm)
			using (var control = NewScaledControl(form, 200, 200))
			using (var childControl = NewScaledControl(control, 50, 50))
			{
				control.Location = ControlDpiScalingHelper.NewScaledPoint(100, 100);
				childControl.Location = ControlDpiScalingHelper.NewScaledPoint(50, 50);

				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(300), ControlTestHelper.GetControlAbsoluteBottom(control, form));
				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlTestHelper.GetControlAbsoluteBottom(childControl, control));
				AssertCloseEnough(errorMessage, ControlDpiScalingHelper.ScaleToCurrentDpiX(200), ControlTestHelper.GetControlAbsoluteBottom(childControl, form));
			}
		}

		#region Implementation

		const string errorMessage = "If this test fails it probably indicates that the helper method isn't functioning properly on scaled displays.";

		static Form NewScaledForm
		{
			get
			{
				return new Form
				{
					AutoScaleMode = AutoScaleMode.None,
					AutoScaleDimensions = new SizeF(6F, 13F),
					Size = ControlDpiScalingHelper.NewScaledSize(850, 650)
				};
			}
		}

		static Control NewScaledControl(Control parent, int width = 800, int height = 600)
		{
			var control = new Control { Size = ControlDpiScalingHelper.NewScaledSize(width, height) };
			parent.Controls.Add(control);

			return control;
		}

		#endregion
	}
}
