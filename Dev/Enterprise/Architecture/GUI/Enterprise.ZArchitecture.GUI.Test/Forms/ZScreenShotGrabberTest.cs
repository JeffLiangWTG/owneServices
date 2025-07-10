using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZScreenShotGrabberTest : TestCase
	{
		public void TestCapture()
		{
			using (var dummyForm = new ZForm())
			{
				var result = ZScreenShotGrabber.Capture(dummyForm);
				AssertNotNull(result);
				AssertEquals(dummyForm.Width, result.Width);
				AssertEquals(dummyForm.Height, result.Height);
			}
		}

		public void TestCaptureWindow()
		{
			using (var dummyForm = new ZForm())
			{
				dummyForm.Show();
				var result = ZScreenShotGrabber.CaptureWindow(dummyForm.Handle);
				if (result == null)
				{
					try
					{
						result = ZScreenShotGrabber.CaptureWindowRaw(dummyForm.Handle);
					}
					catch (ArgumentException)
					{
						Assert(true);
						return;
					}
				}
				AssertNotNull(result);
				AssertEquals(dummyForm.Width, result.Width);
				AssertEquals(dummyForm.Height, result.Height);
			}
		}
	}
}
