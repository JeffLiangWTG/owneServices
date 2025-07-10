using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using static Enterprise.Core.Forms.CachedScreenInfo;

namespace Enterprise.Core.Forms
{
	sealed class CachedScreenInfoTest : TestCase
	{
		public void TestFromControl_ForForm()
		{
			var primaryScreen = new Rectangle(0, 0, 1600, 1200);
			var secondaryScreen = new Rectangle(1600, 0, 2624, 768);
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = primaryScreen });
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = secondaryScreen });

			using (var form = new Form())
			{
				form.Bounds = primaryScreen;
				AssertEquals("Primary Screen", primaryScreen, ScreenInfo.FromControl(form));

				form.Bounds = secondaryScreen;
				AssertEquals("Secondary Screen", secondaryScreen, ScreenInfo.FromControl(form));
			}
		}

		public void TestFromControl_ForControl()
		{
			var primaryScreen = new Rectangle(0, 0, 1600, 1200);
			var secondaryScreen = new Rectangle(1600, 0, 2624, 768);
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = primaryScreen });
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = secondaryScreen });

			using (var form = new Form())
			{
				form.Bounds = new Rectangle(0, 0, 2624, 1200);
				var textBox = new TextBox();
				form.Controls.Add(textBox);

				textBox.Bounds = primaryScreen;
				AssertEquals("Primary Screen", primaryScreen, ScreenInfo.FromControl(textBox));

				textBox.Bounds = secondaryScreen;
				AssertEquals("Secondary Screen", secondaryScreen, ScreenInfo.FromControl(textBox));
			}
		}

		public void TestGetHorizontalStateOffScreenLeft()
		{
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 0, 1023, 767) });
			AssertEquals(HorizontalState.OffScreenLeft, ScreenInfo.GetHorizontalState(new Point(-1, 0)));
		}

		public void TestGetHorizontalStateOffScreenRight()
		{
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 0, 1023, 767) });
			AssertEquals(HorizontalState.OffScreenRight, ScreenInfo.GetHorizontalState(new Point(1024, 0)));
		}

		public void TestGetHorizontalStateVisible1()
		{
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 0, 1023, 767) });
			AssertEquals(HorizontalState.Visible, ScreenInfo.GetHorizontalState(new Point(0, 0)));
		}

		public void TestGetHorizontalStateVisible2()
		{
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 0, 1023, 767) });
			AssertEquals(HorizontalState.Visible, ScreenInfo.GetHorizontalState(new Point(1023, 0)));
		}

		public void TestGetHorizontalStateFrikkingWeird()
		{
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 0, 1023, 767) }); // Standard Screen
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(1024, -768, 2047, -1) }); // Up right
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(2048, 0, 3071, 767) }); // Standard / really right (leaves gap under middle monitor)
			AssertEquals(HorizontalState.OffScreenLeft, ScreenInfo.GetHorizontalState(new Point(1025, 0)));
		}

		public void TestGetVerticalStateVisible()
		{
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, -1080, 1920, 1040) });
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 1080, 1920, 1040) });
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 0, 1920, 1040) });
			AssertEquals(VerticalState.Visible, ScreenInfo.GetVerticalState(new Point(300, -800), 500));
		}

		public void TestGetVerticalStateOffScreenBottom()
		{
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, -1080, 1920, 1040) });
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 1080, 1920, 1040) });
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 0, 1920, 1040) });
			AssertEquals(VerticalState.OffScreenBottom, ScreenInfo.GetVerticalState(new Point(300, -150), 500));
		}

		public void TestGetVerticalStateOffScreenTop()
		{
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, -768, 1024, 728) });
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 768, 1024, 728) });
			ScreenInfo.allScreens.Add(new PhysicalScreenInfoImpl { WorkingArea = new Rectangle(0, 0, 1024, 728) });
			AssertEquals(VerticalState.OffScreenTop, ScreenInfo.GetVerticalState(new Point(10, 300), 500));
		}

		public void TestPopulateInfo_IsThreadSafe()
		{
			var start = new ManualResetEvent(false);
			var tasks = new Task[10];
			var count = 0;

			for (var i = 0; i < 10; ++i)
			{
				tasks[i] = Task.Factory.StartNew(() =>
				{
					start.WaitOne();
					ScreenInfo.PopulateInfo();
					++count;
				});
			}

			start.Set();
			Task.WaitAll(tasks);

			AssertEquals(10, count);
		}

		#region Implementation

		CachedScreenInfo ScreenInfo
		{
			get
			{
				if (screenInfo == null)
				{
					screenInfo = new CachedScreenInfo();
				}
				return screenInfo;
			}
		}
		CachedScreenInfo screenInfo;

		#endregion
	}
}
