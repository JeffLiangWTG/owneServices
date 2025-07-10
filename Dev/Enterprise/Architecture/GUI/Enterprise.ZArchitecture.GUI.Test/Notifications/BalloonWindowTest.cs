using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Notifications
{
	public class BalloonWindowTest : TransactionedTestCase
	{
		public void TestConstruction()
		{
			AssertEquals(System.Drawing.SystemColors.Info, Window.BackColor);
			AssertEquals(FormStartPosition.Manual, Window.StartPosition);
			AssertEquals(false, Window.MaximizeBox);
			AssertEquals(false, Window.MinimizeBox);
			AssertEquals(false, Window.ShowInTaskbar);
			AssertEquals(SizeGripStyle.Hide, Window.SizeGripStyle);
			AssertEquals("BollonWindow should Opaque after init", (byte)255, Window.WindowAlphaExposed);
		}

		public void TestDescriptor()
		{
			Form.Location = new Point(100, 100);
			Form.Show();

			AssertEquals("Visible", false, Window.Visible);

			Window.Descriptor = Descriptor1;
			Window.Show();
			Application.DoEvents();
			AssertEquals("get/set", Descriptor1, Window.Descriptor);
			AssertEquals("Visible", true, Window.Visible);

			Window.Descriptor = Descriptor2;
			Window.Show();
			Application.DoEvents();
			AssertEquals("get/set", Descriptor2, Window.Descriptor);
			AssertEquals("Visible", true, Window.Visible);

			Window.Descriptor = null;
			for (var i = 0; i < 50; i++)
			{
				System.Threading.Thread.Sleep(50);
				Application.DoEvents();
				if (!Window.Visible)
				{
					break;
				}
			}
			AssertEquals("get/set", null, Window.Descriptor);
			AssertEquals("Visible", false, Window.Visible);
		}

		public void TestPositioning_GDI()
		{
			TextRendererHelper.UseTextRendererFromReg();
			EnvProxy.Instance.Registry.GraphicRenderingEngineRegItem = nameof(TextRendererType.GDI);

			var screenRect = new Rectangle(0, 0, 1024, 768);
			WindowMock.SetupGet(o => o.AnchorScreenBounds).Returns(screenRect);
			AssertEquals(screenRect, Window.AnchorScreenBounds);

			Form.FormBorderStyle = FormBorderStyle.None;
			Form.Show();
			Form.Size = new Size(300, 300);

			Form.Location = new Point(200, 200);
			Window.Descriptor = Descriptor1;
			Window.Show();
			Application.DoEvents();
			AssertEquals(new Size(505, 134), Window.Size);
			AssertEquals(new Point(154, 263), Window.Location);

			Form.Location = new Point(0, 0);
			Window.Descriptor = Descriptor2;
			Window.Show();
			Application.DoEvents();
			AssertEquals(new Size(534, 486), Window.Size);
			AssertEquals(new Point(0, 72), Window.Location);

			Form.Location = new Point(0, 740);
			Window.Descriptor = Descriptor1;
			Window.Show();
			Application.DoEvents();
			AssertEquals(new Size(505, 134), Window.Size);
			AssertEquals(new Point(0, 654), Window.Location);

			Form.Location = new Point(1000, 0);
			Window.Descriptor = Descriptor2;
			Window.Show();
			Application.DoEvents();
			AssertEquals(new Size(534, 486), Window.Size);
			AssertEquals(new Point(490, 72), Window.Location);

			Form.Location = new Point(1000, 740);
			Window.Descriptor = Descriptor1;
			Window.Show();
			Application.DoEvents();
			AssertEquals(new Size(505, 134), Window.Size);
			AssertEquals(new Point(519, 654), Window.Location);
		}

		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBalloonLooksRight_GDI()
		{
			AssertBallon(Descriptor1, "BalloonScreenShot");
		}

		[DeveloperOnlyTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBalloonLooksRight_GDI_LongText()
		{
			AssertBallon(Descriptor2, "BalloonScreenShotLongText");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "Primary Bounds are not supported in Enterprise.Core.Forms.CachedScreenInfo.PrimaryScreenInfo")]
		void AssertBallon(BalloonDescriptor balloonDescriptor, string fileToLoad)
		{
			TextRendererHelper.UseTextRendererFromReg();
			EnvProxy.Instance.Registry.GraphicRenderingEngineRegItem = nameof(TextRendererType.GDI);

			var screenWith = Screen.PrimaryScreen.Bounds.Width; // Primary Bounds are not supported in Enterprise.Core.Forms.CachedScreenInfo.PrimaryScreenInfo
			var screenHeight = Screen.PrimaryScreen.Bounds.Height; // Primary Bounds are not supported in Enterprise.Core.Forms.CachedScreenInfo.PrimaryScreenInfo
			if (ObjectFactory.Get<TerminalService>().IsWTSSession)
			{
				Assert("terminal services when disconnected leads to an empty screen shot", true);
				return;
			}

			if (screenWith < 1920 || screenHeight < 1080)
			{
				Assert("Minimum screen bounds are 1920 * 1080", true);
				return;
			}

			Form.Show();
			Form.Location = new Point(200, 200);
			Form.Size = new Size(400, 400);
			Form.BackColor = Color.Black;

			Window.Descriptor = balloonDescriptor;
			Window.Show();
			Application.DoEvents();

			var expectedFilename = fileToLoad + "_" + screenWith + screenHeight + ".bmp";
			var imgActual = new Bitmap(Window.Width, Window.Height);
			var imgExpected = (Bitmap)Image.FromFile(BaseSourcePath + @"Enterprise\Architecture\GUI\Notifications\Testing\" + expectedFilename);
			{
				Window.DrawToBitmap(imgActual, new Rectangle(0, 0, Window.Width, Window.Height));
				TextRendererHelperTest.AssertBitmapsAreSimilar(imgExpected, imgActual, 0.95f, 0.1f);
			}
		}

		public void TestMouseHookNotInstalledForLowNotifications()
		{
			Form.Size = new Size(100, 100);
			Form.Location = new Point(100, 100);
			Form.Show();

			Window.Descriptor = Descriptor1;
			Window.Show();
			Application.DoEvents();

			Assert(Window.Descriptor.AllUniqueNotifications.Count() <= BalloonDescriptor.NotificationsCap);
			Assert("no mouse hook with few notifications", Window.mouseHook == IntPtr.Zero);

			var count = BalloonDescriptor.NotificationsCap * 2;
			var notifications = new INotification[count * 2];
			for (var i = 0; i < count; ++i)
			{
				notifications[i] = new Notification(CargoWise.EntityFramework.NotificationType.Warning, i.ToString());
				notifications[i + count] = new Notification(CargoWise.EntityFramework.NotificationType.Warning, i.ToString());
			}

			var largeDescriptor = new BalloonDescriptor(Label, "a", "b", notifications);
			Window.Descriptor = largeDescriptor;
			Window.Show();
			Form.Show();
			Application.DoEvents();

			Assert(Window.Descriptor.AllUniqueNotifications.Count() > BalloonDescriptor.NotificationsCap);
			Assert("mouse hook with many notifications", Window.mouseHook != IntPtr.Zero);

			Window.Hide();
			Window.HandleMouseScroll(0);

			Assert("no mouse hook after hiding", Window.mouseHook == IntPtr.Zero);

			count = BalloonDescriptor.NotificationsCap;
			notifications = new INotification[count];
			for (var i = 0; i < count; ++i)
			{
				notifications[i] = new Notification(CargoWise.EntityFramework.NotificationType.Warning, i.ToString());
			}

			largeDescriptor = new BalloonDescriptor(Label, "a", "b", notifications);

			Form.Hide();
			Form.Show();
			Window.Descriptor = largeDescriptor;
			Window.Show();
			Application.DoEvents();

			Assert("One short of being scrollable? No mouse hook!", Window.mouseHook == IntPtr.Zero);
		}

		public void TestHideOnMoveDisposeOrVisibleChanged()
		{
			Form.Size = new Size(100, 100);
			Form.Location = new Point(100, 100);
			Form.Show();

			CheckVisibleThenGoesInvisible(delegate
			{ Form.Location = new Point(200, 200); }, Descriptor1);
			CheckVisibleThenGoesInvisible(delegate
			{ Label2.Location = new Point(2, 2); }, Descriptor2);
			CheckVisibleThenGoesInvisible(delegate
			{ Label.Visible = false; }, Descriptor1);
			Label.Visible = true;
			CheckVisibleThenGoesInvisible(delegate
			{ Form.Visible = false; }, Descriptor2);
			Form.Visible = true;
			CheckVisibleThenGoesInvisible(delegate
			{ Label.Dispose(); }, Descriptor1);
			CheckVisibleThenGoesInvisible(delegate
			{ Form.Dispose(); }, Descriptor2);
		}

		public void TestHideOnParentFormDeactive()
		{
			Form.Size = new Size(100, 100);
			Form.Location = new Point(100, 100);
			Form.Show();

			Window.Descriptor = Descriptor1;
			Window.Show();
			AssertEquals("Visible", true, Window.Visible);

			var propertyinfo = Form.GetType().GetProperty("Active", BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance);
			propertyinfo.SetValue(Form, false);
			Application.DoEvents();

			AssertEquals("Visible", false, Window.Visible);
		}

		public void TestDisableBalloonOnFormDeactive_NullReferenceException()
		{
			Form.Size = new Size(100, 100);
			Form.Location = new Point(100, 100);
			Form.Show();

			Window.Descriptor = Descriptor1;
			Window.Show();

			Window.DisableBalloonOnFormOrControlChange(this, new EventArgs());
			AssertNoExceptionThrown(() => Window.DisableBalloonOnFormDeactive(this, new EventArgs()));
		}

		public void TestDisposeOnParentFormDisposed()
		{
			Form.Show();
			Window.Descriptor = Descriptor1;
			Window.Show();

			Assert("The BalloonWindows should not be disposed until the parent form is disposed.", !Window.IsDisposed);

			Form.Dispose();
			Application.DoEvents();

			Assert("The BalloonWindow should have been disposed along with the parent form.", Window.IsDisposed);
		}

		delegate void ChangeControlsDelegate();

		void CheckVisibleThenGoesInvisible(ChangeControlsDelegate change, BalloonDescriptor descriptor)
		{
			AssertEquals("Visible", false, Window.Visible);
			Window.Descriptor = descriptor;
			Window.Show();
			AssertEquals("Visible", true, Window.Visible);
			change();
			Application.DoEvents();
			AssertEquals("Visible", false, Window.Visible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			WindowMock = new Mock<BalloonWindow>() { CallBase = true };
			Window = WindowMock.Object;
			Form = new ZChildForm();
			Label = new ZLabel();
			Label.Location = new Point(50, 50);
			Label.Size = new Size(50, 14);
			Form.Controls.Add(Label);

			Label2 = new ZLabel();
			Label2.Location = new Point(5, 50);
			Form.Controls.Add(Label2);

			var errors = new string[] { "Errors are bad", "Errors are bad" }; // duplicates should be eliminated
			var warnings = new string[] { "Warnings are not so bad" };
			var messageErrors = new string[] { "Message Errors", "Heaps and heaps that are long. Aussi paraxodal que cela puisse paraitre, c'est a un balloon!" };
			Descriptor1 = new BalloonDescriptor(Label, "Welcome to the club...", "Balloons are cool. Yeah. They really are. They will help you to use Enterprise. Lucky you! ", CreateNotifications(errors, warnings, messageErrors));

			var longMesageErrors = new string[] { GetLongString(), GetLongString() + "2" };
			Descriptor2 = new BalloonDescriptor(Label2, "Thanks and So Long", GetLongString(), CreateNotifications(Array.Empty<string>(), Array.Empty<string>(), longMesageErrors));
		}

		NotificationCollection CreateNotifications(string[] errors, string[] warnings, string[] messageErrors)
		{
			var result = new NotificationCollection();
			foreach (var error in errors)
			{
				result.AddError(error);
			}
			foreach (var warning in warnings)
			{
				result.AddWarning(warning);
			}
			foreach (var messageError in messageErrors)
			{
				result.AddMessageError(messageError);
			}
			return result;
		}

		string GetLongString()
		{
			var result = new ZStringBuilder();
			for (var i = 0; i < 20; i++)
			{
				result.Append("Hello this is a long balloon string. Teapot! Noodle!");
			}
			return result.ToString();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Window.Dispose();
			Form.Dispose();
		}

		BalloonWindow Window;
		ZChildForm Form;
		ZLabel Label;
		ZLabel Label2;

		BalloonDescriptor Descriptor1;
		BalloonDescriptor Descriptor2;

		Mock<BalloonWindow> WindowMock;
	}
}
