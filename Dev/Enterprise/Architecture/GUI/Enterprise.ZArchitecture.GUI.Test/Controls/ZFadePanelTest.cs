using System;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZFadePanelTest : TestCase
	{
		public void TestPropertyRanges()
		{
			using (var panel = new ZFadePanel())
			{
				AssertExceptionThrown<ArgumentException>(() => panel.GradientSizePercent = 1.1f);
				AssertExceptionThrown<ArgumentException>(() => panel.GradientSizePercent = -0.1f);
				AssertNoExceptionThrown(() => panel.GradientSizePercent = 1.0f);
				AssertNoExceptionThrown(() => panel.GradientSizePercent = null);
				AssertNoExceptionThrown(() => panel.GradientSizePercent = 0f);

				AssertExceptionThrown<ArgumentException>(() => panel.GradientStartPercent = 1.1f);
				AssertExceptionThrown<ArgumentException>(() => panel.GradientStartPercent = -0.1f);
				AssertNoExceptionThrown(() => panel.GradientStartPercent = 1.0f);
				AssertNoExceptionThrown(() => panel.GradientStartPercent = null);
				AssertNoExceptionThrown(() => panel.GradientStartPercent = 0f);
			}
		}

		[ExpectNoExceptions]
		public void TestSetManyTimes()
		{
			using (var panel = new ZFadePanel())
			{
				panel.SetFadePercentProgressively(.99f);
				panel.SetFadePercentProgressively(.55f);
				panel.SetFadePercentProgressively(.22f);

				Application.DoEvents();

				while (panel.GradientStartPercent != .22f && panel.GradientStartPercent >= 0f && panel.GradientStartPercent < 1f)
				{
					Thread.Sleep(30);
					Application.DoEvents();
				}

				for (var i = 0; i < 100; i++)
				{
					Thread.Sleep(30);
					Application.DoEvents();
				}

				AssertEquals("Even though we let the timer run longer, the gradient was correct, and the previous gradients were ignored.", .22f, panel.GradientStartPercent);
			}
		}

		[ExpectNoExceptions]
		public void TestSetTwice()
		{
			using (var panel = new ZFadePanel())
			{
				panel.SetFadePercentProgressively(.99f);
				panel.SetFadePercentProgressively(.22f);

				Application.DoEvents();

				while (panel.GradientStartPercent != .22f && panel.GradientStartPercent >= 0f && panel.GradientStartPercent < 1f)
				{
					Thread.Sleep(30);
					Application.DoEvents();
				}

				for (var i = 0; i < 100; i++)
				{
					Thread.Sleep(30);
					Application.DoEvents();
				}

				AssertEquals("Even though we let the timer run longer, the gradient was correct, and the previous gradients were ignored.", .22f, panel.GradientStartPercent);
			}
		}

		public void TestContinuouslyChanging()
		{
			using (var panel = new ZFadePanel())
			{
				panel.SetFadePercentProgressively(.99f);

				AssertEquals(true, panel.IsFadeContinuouslyChanging);

				panel.ForceCompleteAnimation();
				AssertEquals(false, panel.IsFadeContinuouslyChanging);
				AssertEquals(.99f, panel.GradientStartPercent);
			}
		}
	}
}
