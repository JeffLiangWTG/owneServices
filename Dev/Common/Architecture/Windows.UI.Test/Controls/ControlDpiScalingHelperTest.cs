using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlDpiScalingHelperTest : TestCase
	{
		public void TestUsingOverridenValues()
		{
			var twelveXScaled = ControlDpiScalingHelper.ScaleToCurrentDpiX(12);
			var twelveYScaled = ControlDpiScalingHelper.ScaleToCurrentDpiY(12);

			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(ControlDpiScalingHelper.DpiX * 2, ControlDpiScalingHelper.DpiY / 2))
			{
				AssertEquals("Should have doubled the scaled value", twelveXScaled * 2, ControlDpiScalingHelper.ScaleToCurrentDpiX(12));
				AssertEquals("Should have halved the scaled value", twelveYScaled / 2, ControlDpiScalingHelper.ScaleToCurrentDpiY(12));
			}

			AssertEquals("Should restore the original value - X", twelveXScaled, ControlDpiScalingHelper.ScaleToCurrentDpiX(12));
			AssertEquals("Should restore the original value - Y", twelveYScaled, ControlDpiScalingHelper.ScaleToCurrentDpiY(12));
		}

		public void TestScaledConstructors()
		{
			Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
			float dpiScaleX = graphics.DpiX / ControlDpiScalingHelper.BaseDpiX;
			float dpiScaleY = graphics.DpiY / ControlDpiScalingHelper.BaseDpiY;
			int expectedX;
			int expectedY;

			// When setting with parameters on standard DPI, values should be scaled
			expectedX = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScaleX);
			expectedY = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScaleY);

			Padding testPadding = ControlDpiScalingHelper.NewScaledPadding(120, true);
			AssertEquals(expectedX, testPadding.Left);
			AssertEquals(expectedX, testPadding.Right);
			AssertEquals(expectedY, testPadding.Top);
			AssertEquals(expectedY, testPadding.Bottom);

			testPadding = ControlDpiScalingHelper.NewScaledPadding(120, 120, 120, 120, true);
			AssertEquals(expectedX, testPadding.Left);
			AssertEquals(expectedX, testPadding.Right);
			AssertEquals(expectedY, testPadding.Top);
			AssertEquals(expectedY, testPadding.Bottom);

			Point testPoint = ControlDpiScalingHelper.NewScaledPoint(120, 120, true);
			AssertEquals(expectedX, testPoint.X);
			AssertEquals(expectedY, testPoint.Y);

			Rectangle testRectangle = ControlDpiScalingHelper.NewScaledRectangle(new Point(120, 120), new Size(120, 120), true);
			AssertEquals(expectedX, testRectangle.X);
			AssertEquals(expectedX, testRectangle.Width);
			AssertEquals(expectedY, testRectangle.Y);
			AssertEquals(expectedY, testRectangle.Height);

			testRectangle = ControlDpiScalingHelper.NewScaledRectangle(120, 120, 120, 120, true);
			AssertEquals(expectedX, testRectangle.X);
			AssertEquals(expectedX, testRectangle.Width);
			AssertEquals(expectedY, testRectangle.Y);
			AssertEquals(expectedY, testRectangle.Height);

			Size testSize = ControlDpiScalingHelper.NewScaledSize(new Size(120, 120), true);
			AssertEquals(expectedX, testSize.Width);
			AssertEquals(expectedY, testSize.Height);

			testSize = ControlDpiScalingHelper.NewScaledSize(120, 120, true);
			AssertEquals(expectedX, testSize.Width);
			AssertEquals(expectedY, testSize.Height);

			// When setting with parameters on current DPI, values shouldn't be scaled
			expectedX = 120;
			expectedY = 120;

			testPadding = ControlDpiScalingHelper.NewScaledPadding(120, false);
			AssertEquals(expectedX, testPadding.Left);
			AssertEquals(expectedX, testPadding.Right);
			AssertEquals(expectedY, testPadding.Top);
			AssertEquals(expectedY, testPadding.Bottom);

			testPadding = ControlDpiScalingHelper.NewScaledPadding(120, 120, 120, 120, false);
			AssertEquals(expectedX, testPadding.Left);
			AssertEquals(expectedX, testPadding.Right);
			AssertEquals(expectedY, testPadding.Top);
			AssertEquals(expectedY, testPadding.Bottom);

			testPoint = ControlDpiScalingHelper.NewScaledPoint(120, 120, false);
			AssertEquals(expectedX, testPoint.X);
			AssertEquals(expectedY, testPoint.Y);

			testRectangle = ControlDpiScalingHelper.NewScaledRectangle(new Point(120, 120), new Size(120, 120), false);
			AssertEquals(expectedX, testRectangle.X);
			AssertEquals(expectedX, testRectangle.Width);
			AssertEquals(expectedY, testRectangle.Y);
			AssertEquals(expectedY, testRectangle.Height);

			testRectangle = ControlDpiScalingHelper.NewScaledRectangle(120, 120, 120, 120, false);
			AssertEquals(expectedX, testRectangle.X);
			AssertEquals(expectedX, testRectangle.Width);
			AssertEquals(expectedY, testRectangle.Y);
			AssertEquals(expectedY, testRectangle.Height);

			testSize = ControlDpiScalingHelper.NewScaledSize(new Size(120, 120), false);
			AssertEquals(expectedX, testSize.Width);
			AssertEquals(expectedY, testSize.Height);

			testSize = ControlDpiScalingHelper.NewScaledSize(120, 120, false);
			AssertEquals(expectedX, testSize.Width);
			AssertEquals(expectedY, testSize.Height);
		}

		public void TestGenericSetters()
		{
			Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
			float dpiScaleX = graphics.DpiX / ControlDpiScalingHelper.BaseDpiX;
			float dpiScaleY = graphics.DpiY / ControlDpiScalingHelper.BaseDpiY;
			int expectedX;
			int expectedY;

			using (var testControl = new Control())
			{
				// First test the setters that receive objects

				// When setting with paramter on standard DPI, values should be scaled
				expectedX = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScaleX);
				expectedY = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScaleY);

				ControlDpiScalingHelper.SetWidth(testControl, 120, true);
				ControlDpiScalingHelper.SetLeft(testControl, 120, true);
				ControlDpiScalingHelper.SetHeight(testControl, 120, true);
				ControlDpiScalingHelper.SetTop(testControl, 120, true);

				AssertEquals(expectedX, testControl.Width);
				AssertEquals(expectedX, testControl.Left);
				AssertEquals(expectedY, testControl.Height);
				AssertEquals(expectedY, testControl.Top);

				// When setting with paramter on current DPI, values should not be scaled

				expectedX = 120;
				expectedY = 120;

				ControlDpiScalingHelper.SetWidth(testControl, 120, false);
				ControlDpiScalingHelper.SetLeft(testControl, 120, false);
				ControlDpiScalingHelper.SetHeight(testControl, 120, false);
				ControlDpiScalingHelper.SetTop(testControl, 120, false);

				AssertEquals(expectedX, testControl.Width);
				AssertEquals(expectedX, testControl.Left);
				AssertEquals(expectedY, testControl.Height);
				AssertEquals(expectedY, testControl.Top);
			}

			// Now test the setters that receive references. Use structures so we make sure they're being boxed properly

			var testPadding = new Padding(0);
			var testPoint = new Point(0, 0);

			// When setting with paramter on standard DPI, values should be scaled

			expectedX = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScaleX);
			expectedY = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScaleY);

			ControlDpiScalingHelper.SetLeft(ref testPadding, 120, true);
			ControlDpiScalingHelper.SetRight(ref testPadding, 120, true);
			ControlDpiScalingHelper.SetTop(ref testPadding, 120, true);
			ControlDpiScalingHelper.SetBottom(ref testPadding, 120, true);
			ControlDpiScalingHelper.SetX(ref testPoint, 120, true);
			ControlDpiScalingHelper.SetY(ref testPoint, 120, true);

			AssertEquals(expectedX, testPadding.Left);
			AssertEquals(expectedX, testPadding.Right);
			AssertEquals(expectedY, testPadding.Top);
			AssertEquals(expectedY, testPadding.Bottom);
			AssertEquals(expectedX, testPoint.X);
			AssertEquals(expectedY, testPoint.Y);

			// When setting with paramter on standard DPI, values should not be scaled

			expectedX = 120;
			expectedY = 120;

			ControlDpiScalingHelper.SetLeft(ref testPadding, 120, false);
			ControlDpiScalingHelper.SetRight(ref testPadding, 120, false);
			ControlDpiScalingHelper.SetTop(ref testPadding, 120, false);
			ControlDpiScalingHelper.SetBottom(ref testPadding, 120, false);
			ControlDpiScalingHelper.SetX(ref testPoint, 120, false);
			ControlDpiScalingHelper.SetY(ref testPoint, 120, false);

			AssertEquals(expectedX, testPadding.Left);
			AssertEquals(expectedX, testPadding.Right);
			AssertEquals(expectedY, testPadding.Top);
			AssertEquals(expectedY, testPadding.Bottom);
			AssertEquals(expectedX, testPoint.X);
			AssertEquals(expectedY, testPoint.Y);
		}

		public void TestScaleToCurrentDpiX()
		{
			using (var testControl = new Control())
			{
				float dpiScale = testControl.CreateGraphics().DpiX / ControlDpiScalingHelper.BaseDpiX;

				// Test using several random sizes
				int expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScale);
				AssertEquals("Scaling on X should return {0}, instead got {1}", expected, ControlDpiScalingHelper.ScaleToCurrentDpiX(120));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(3000 * dpiScale);
				AssertEquals("Scaling on X should return {0}, instead got {1}", expected, ControlDpiScalingHelper.ScaleToCurrentDpiX(3000));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(50000 * dpiScale);
				AssertEquals("Scaling on X should return {0}, instead got {1}", expected, ControlDpiScalingHelper.ScaleToCurrentDpiX(50000));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(2 * dpiScale);
				AssertEquals("Scaling on X should return {0}, instead got {1}", expected, ControlDpiScalingHelper.ScaleToCurrentDpiX(2));
			}
		}

		public void TestScaleToCurrentDpiY()
		{
			using (var testControl = new Control())
			{
				float dpiScale = testControl.CreateGraphics().DpiY / ControlDpiScalingHelper.BaseDpiY;

				// Test using several random sizes
				int expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScale);
				AssertEquals("Scaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.ScaleToCurrentDpiY(120));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(3000 * dpiScale);
				AssertEquals("Scaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.ScaleToCurrentDpiY(3000));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(50000 * dpiScale);
				AssertEquals("Scaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.ScaleToCurrentDpiY(50000));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(2 * dpiScale);
				AssertEquals("Scaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.ScaleToCurrentDpiY(2));
			}
		}

		public void TestUnscaleFromCurrentDpiX()
		{
			using (var testControl = new Control())
			{
				float dpiScale = ControlDpiScalingHelper.BaseDpiX / testControl.CreateGraphics().DpiX;

				// Test using several random sizes
				int expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScale);
				AssertEquals("Unscaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(120));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(3000 * dpiScale);
				AssertEquals("Unscaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(3000));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(50000 * dpiScale);
				AssertEquals("Unscaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(50000));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(2 * dpiScale);
				AssertEquals("Unscaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(2));
			}
		}

		public void TestUnscaleFromCurrentDpiY()
		{
			using (var testControl = new Control())
			{
				float dpiScale = ControlDpiScalingHelper.BaseDpiY / testControl.CreateGraphics().DpiY;

				// Test using several random sizes
				int expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScale);
				AssertEquals("Unscaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(120));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(3000 * dpiScale);
				AssertEquals("Unscaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(3000));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(50000 * dpiScale);
				AssertEquals("Unscaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(50000));

				expected = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(2 * dpiScale);
				AssertEquals("Unscaling on Y should return {0}, instead got {1}", expected, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(2));
			}
		}

		public void TestDPIScalingFunctionsUseReflectionSometimes()
		{
			// Test using several random sizes
			Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
			float dpiScaleX = graphics.DpiX / ControlDpiScalingHelper.BaseDpiX;
			float dpiScaleY = graphics.DpiY / ControlDpiScalingHelper.BaseDpiY;
			int expectedX;
			int expectedY;

			var testControl = new FakeControl();
			// First test the setters that receive objects

			// When setting with paramter on standard DPI, values should be scaled
			expectedX = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScaleX);
			expectedY = ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(120 * dpiScaleY);

			ControlDpiScalingHelper.SetWidth(testControl, 120, true);
			ControlDpiScalingHelper.SetLeft(testControl, 120, true);
			ControlDpiScalingHelper.SetHeight(testControl, 120, true);
			ControlDpiScalingHelper.SetTop(testControl, 120, true);

			AssertEquals(expectedX, testControl.Width);
			AssertEquals(expectedX, testControl.Left);
			AssertEquals(expectedY, testControl.Height);
			AssertEquals(expectedY, testControl.Top);

			// When setting with paramter on current DPI, values should not be scaled

			expectedX = 120;
			expectedY = 120;

			ControlDpiScalingHelper.SetWidth(testControl, 120, false);
			ControlDpiScalingHelper.SetLeft(testControl, 120, false);
			ControlDpiScalingHelper.SetHeight(testControl, 120, false);
			ControlDpiScalingHelper.SetTop(testControl, 120, false);

			AssertEquals(expectedX, testControl.Width);
			AssertEquals(expectedX, testControl.Left);
			AssertEquals(expectedY, testControl.Height);
			AssertEquals(expectedY, testControl.Top);
		}

		class FakeControl
		{
			public int Width { get; set; }
			public int Height { get; set; }
			public int Top { get; set; }
			public int Left { get; set; }
		}

		/// <summary>
		/// Tests if ReflectAndSetProperty correctly converts an int to a string property.
		/// This test case targets the scenario where Convert.ChangeType is used.
		/// </summary>
		public void TestReflectAndSetProperty_StringProperty_UsesConvertChangeType()
		{
			var testObject = new TestClass();
			string propertyName = "StringProperty";
			int value = 42;

			ControlDpiScalingHelper.ReflectAndSetProperty(testObject, propertyName, value);

			// Assert: Verify that the string property was correctly set with the converted value
			AssertEquals(testObject.StringProperty, "42");
		}

		/// <summary>
		/// Tests if ReflectAndSetProperty correctly uses a custom type's constructor to set a property.
		/// This test case targets the scenario where a constructor taking an int parameter is used.
		/// </summary>
		public void TestReflectAndSetProperty_CustomProperty_UsesConstructor()
		{
			var testObject = new TestClass();
			int value = 42;
			string propertyName = "CustomProperty";

			ControlDpiScalingHelper.ReflectAndSetProperty(testObject, propertyName, value);

			// Assert: Verify that the custom property was correctly set using its constructor
			AssertEquals(testObject.CustomProperty.Value, value);

			var testObject2 = new TestClass();
			ControlDpiScalingHelper.ReflectAndSetProperty(testObject2, propertyName, value);
			AssertEquals(testObject2.CustomProperty.Value, value);
		}

		class CustomType
		{
			public int Value { get; }

			public CustomType(int value)
			{
				Value = value;
			}
		}

		/// <summary>
		/// A test class with properties of different types for testing ReflectAndSetProperty.
		/// </summary>
		class TestClass
		{
			public string StringProperty { get; set; }
			public CustomType CustomProperty { get; set; }
		}
	}
}
