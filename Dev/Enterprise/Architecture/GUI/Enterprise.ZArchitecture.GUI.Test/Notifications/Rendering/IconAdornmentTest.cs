using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class IconAdornmentTest : TestCase
	{
		public void TestClearHotSpotAndDoNotDrawIconWhenNotificationsAreEmpty()
		{
			var control = new TextBox();

			var adornment = new IconAdornment();
			adornment.Initialize(control, Presenter);
			adornment.IconLayout = new IconLayout(control, IconAlignment.Default);

			Assert(adornment.HotSpot == null);

			var notifications = new NotificationCollection();
			notifications.AddError("Err");

#if !WINZOR
			adornment.Notifications = notifications;
			adornment.Paint(new PaintEventArgs(control.CreateGraphics(), control.ClientRectangle));
			Assert(adornment.HotSpot != null);

			adornment.Notifications = NotificationCollection.Empty;
			adornment.Paint(new PaintEventArgs(control.CreateGraphics(), control.ClientRectangle));
			Assert(adornment.HotSpot == null);
#endif
#if WINZOR
			var container = new ContainerControl();
			container.Controls.Add(control);

			adornment.Notifications = notifications;
			Assert(adornment.HotSpot != null);

			adornment.Notifications = NotificationCollection.Empty;
			Assert(adornment.HotSpot == null);
#endif
		}

		public void TestResizesControlWhenNeededToAcommodateIcon()
		{
			var control = new TestControl { Width = 14 };

			var adornment = new IconAdornment();
			adornment.Initialize(control, Presenter);
			adornment.IconLayout = new TestIconLayout { Target = control };

			var notifications = new NotificationCollection();
			notifications.AddError("Err");

			adornment.Notifications = notifications;
			Assert(control.PreferredWithModifier == NotificationIconScheme.Instance.GetMiniImage(NotificationType.Error).Width + IconLayoutBase.XOffset);

			adornment.Notifications = NotificationCollection.Empty;
			Assert(control.PreferredWithModifier == 0);
		}

		class TestIconLayout : PreciseIconLayout
		{
			protected override bool ShouldResize
			{
				get { return true; }
			}

			protected override int GetPreciseWidth()
			{
				return ControlDpiScalingHelper.ScaleToCurrentDpiX(14);
			}

			protected override void AddWidth(int width)
			{
				((TestControl)Target).PreferredWithModifier = width;
			}

			protected override int GetAlignmentDirection()
			{
				return ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
			}
		}

		class TestControl : CheckBox
		{
			public int PreferredWithModifier;
		}

		class FakePresenter : NotificationPresenter
		{
			protected override void Display() { }
			protected override void Clear() { }
		}

		FakePresenter Presenter
		{
			get { return presenter ?? (presenter = new FakePresenter()); }
		}
		FakePresenter presenter;

		protected override void TearDown()
		{
			base.TearDown();
			if (presenter != null)
			{
				presenter.Dispose();
			}
		}
	}
}
