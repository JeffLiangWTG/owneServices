#if !WINZOR
using System;
using System.ComponentModel;
using System.Drawing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDropButtonOnlyTest : TestCase
	{
		public void TestControlCanBeCreated()
		{
			Assert("ZDropButtonOnly is null", Button != null);
		}

		public void TestResize_RightEdgeUnlocked()
		{
			var desiredWidth = ZGUISystemInformation.VerticalScrollBarWidth;

			((ISupportInitialize)Button).BeginInit();

			Button.Location = new Point(20, 20);
			Button.lockRightEdge = false;
			Button.Size = new Size(desiredWidth - 2, 20);
			AssertEquals(new Rectangle(20, 20, desiredWidth - 2, 20), Button.Bounds);

			((ISupportInitialize)Button).EndInit();
			AssertEquals(new Rectangle(20, 20, desiredWidth, 20), Button.Bounds);

			Button.Width = desiredWidth + 2;
			AssertEquals(new Rectangle(20, 20, desiredWidth, 20), Button.Bounds);

			Button.Bounds = new Rectangle(10, 10, 10, 10);
			AssertEquals(new Rectangle(10, 10, desiredWidth, 10), Button.Bounds);
		}

		public void TestResize_RightEdgeLocked()
		{
			var desiredWidth = ZGUISystemInformation.VerticalScrollBarWidth;

			((ISupportInitialize)Button).BeginInit();

			Button.Location = new Point(20, 20);
			Button.lockRightEdge = true;
			Button.Size = new Size(desiredWidth - 2, 20);
			AssertEquals(new Rectangle(20, 20, desiredWidth - 2, 20), Button.Bounds);

			((ISupportInitialize)Button).EndInit();
			AssertEquals(new Rectangle(18, 20, desiredWidth, 20), Button.Bounds);

			Button.Width = desiredWidth + 2;
			AssertEquals(new Rectangle(18, 20, desiredWidth, 20), Button.Bounds);

			Button.Bounds = new Rectangle(10, 10, 10, 10);
			AssertEquals(new Rectangle(10, 10, desiredWidth, 10), Button.Bounds);
		}

		public void TestTabStop()
		{
			Assert(Button.TabStop);
		}

		public void TestFocus()
		{
			AssertEquals("default", 0, Button.CurrentDrawState);
			Button.InvokeGotFocusExposed(Button, new EventArgs());
			AssertEquals("Selected", XpThemeAPI.Constants.CBXS_HOT, Button.CurrentDrawState);
			Button.InvokeLostFocusExposed(Button, new EventArgs());
			AssertEquals("Normal", XpThemeAPI.Constants.CBXS_NORMAL, Button.CurrentDrawState);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Button = new ZDropButtonOnly();
		}

		protected override void TearDown()
		{
			Button.Dispose();
			base.TearDown();
		}

		ZDropButtonOnly Button;

		#endregion
	}
}
#endif
