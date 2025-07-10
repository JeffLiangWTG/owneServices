using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class AllowOverlapExtensionMethodsTest : TestCase
	{
		public void TestErrorThrownIfControlCallsAllowOverlapOnItself()
		{
			using (var control = new Control() { Name = "control" })
			{
				AssertExceptionThrown(typeof(ArgumentException), "control cannot overlap itself.", () =>
				{
					control.AllowOverlap(control);
				});
			}
		}

		public void TestErrorThrownIfAllowOverlapForControlsFormsACycle()
		{
			using (var control1 = new Control() { Name = "control1" })
			using (var control2 = new Control() { Name = "control2" })
			{
				AssertExceptionThrown(typeof(ArgumentException), "control2 cannot overlap control1 as control1 is already overlapping control2.", () =>
				{
					control1.AllowOverlap(control2);
					control2.AllowOverlap(control1);
				});
			}

			using (var control1 = new Control() { Name = "control1" })
			using (var control2 = new Control() { Name = "control2" })
			using (var control3 = new Control() { Name = "control3" })
			{
				AssertExceptionThrown(typeof(ArgumentException), "control3 cannot overlap control1 as control1 is already overlapping control3.", () =>
				{
					control1.AllowOverlap(control2);
					control2.AllowOverlap(control3);
					control3.AllowOverlap(control1);
				});
			}

			using (var control1 = new Control() { Name = "control1" })
			using (var control2 = new Control() { Name = "control2" })
			using (var control3 = new Control() { Name = "control3" })
			{
				AssertNoExceptionThrown(() =>
				{
					control1.AllowOverlap(control2);
					control1.AllowOverlap(control3);
					control2.AllowOverlap(control3);
				});
			}
		}

		public void TestNoErrorThrownIfOverlapControlIsDisposed()
		{
			using (var control1 = new Control() { Name = "control1" })
			using (var control2 = new Control() { Name = "control2" })
			using (var control3 = new Control() { Name = "control3" })
			{
				AssertNoExceptionThrown(() =>
				{
					control2.AllowOverlap(control3);
					control3.Dispose();
					control1.AllowOverlap(control2);
				});
			}
		}
	}
}
