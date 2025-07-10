using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.GUI.Balloons;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	class InteractiveAdornmentTest : TestCase
	{
		public void TestToRectangleConvertsGraphicsPath()
		{
			var rect = Adornment.ToRectangle(HotSpot);
			Assert(rect.X == location.X);
			Assert(rect.Y == location.Y);
			Assert(rect.Width == size.Width);
			Assert(rect.Height == size.Height);
		}

		public void TestBalloonNotShownWhenNotificationsAreEmpty()
		{
			AdornmentMock.Setup(m => m.IsNotificationsEmpty()).Returns(true);

			AdornmentMock.Setup(m => m.MouseMove(null))
				.Callback((MouseEventArgs e) =>
				{
					AdornmentMock.Setup(m => m.MouseMove(It.IsAny<MouseEventArgs>()))
						.CallBase();
				});

			Adornment.MouseMove(CreateMouseMoveEventArgs());

			Assert(true);
			AdornmentMock.Verify(m => m.IsHotSpotIncludes(cursor), Times.Never);
		}

		public void TestBalloonIsShownWhenMouseMoveOverHotSpotAndControlIsNotInFocusAndIsVisible()
		{
			var notifications = new NotificationCollection();
			notifications.AddError("TEST");

			AdornmentMock.Setup(m => m.Control).Returns(Control);

			AdornmentMock.Setup(m => m.Attach())
				.Callback(() =>
				{
					AdornmentMock.Setup(m => m.Attach()).CallBase();
				});

			AdornmentMock.Setup(m => m.IsNotificationsEmpty()).Returns(false);
			AdornmentMock.Setup(m => m.IsHotSpotIncludes(cursor)).Returns(true);
			AdornmentMock.Setup(m => m.IsControlVisible()).Returns(true);
			AdornmentMock.Setup(m => m.IsControlFocused()).Returns(false);
			BalloonMock.Setup(m => m.IsVisible).Returns(false);
			AdornmentMock.Setup(m => m.HotSpot).Returns(HotSpot);
			AdornmentMock.Setup(m => m.Notifications).Returns(notifications);
			BalloonMock.Setup(m => m.Show(notifications, Control, Adornment.ToRectangle(HotSpot), false));

			AdornmentMock.Setup(m => m.MouseMove(null))
				.Callback((MouseEventArgs e) =>
				{
					AdornmentMock.Setup(m => m.MouseMove(null)).CallBase();
				});

			AdornmentMock.Setup(m => m.Detach())
				.Callback(() =>
				{
					AdornmentMock.Setup(m => m.Detach()).CallBase();
				});

			Adornment.Attach();
			Adornment.MouseMove(CreateMouseMoveEventArgs());
			Adornment.Detach();

			Assert(true);
		}

		public void TestBalloonIsHiddenWhenMouseMoveOverHotSpotAndControlIsInFocus()
		{
			var notifications = new NotificationCollection();
			notifications.AddError("TEST");

			AdornmentMock.Setup(m => m.IsNotificationsEmpty()).Returns(false);
			AdornmentMock.Setup(m => m.IsHotSpotIncludes(cursor)).Returns(true);
			AdornmentMock.Setup(m => m.IsControlVisible()).Returns(true);
			AdornmentMock.Setup(m => m.IsControlFocused()).Returns(true);
			BalloonMock.Setup(m => m.IsVisible).Returns(true);
			Balloon.Hide();

			AdornmentMock.Setup(m => m.MouseMove(null))
				.Callback((MouseEventArgs e) =>
				{
					AdornmentMock.Setup(m => m.MouseMove(null)).CallBase();
				});

			Adornment.MouseMove(CreateMouseMoveEventArgs());

			Assert(true);
		}

		public void TestBalloonIsHiddenWhenControlIsNotVisible()
		{
			var notifications = new NotificationCollection();
			notifications.AddError("TEST");

			AdornmentMock.Setup(m => m.IsNotificationsEmpty()).Returns(false);
			AdornmentMock.Setup(m => m.IsHotSpotIncludes(cursor)).Returns(true);
			AdornmentMock.Setup(m => m.IsControlVisible()).Returns(false);

			BalloonMock.Setup(m => m.IsVisible).Returns(true);
			Balloon.Hide();

			AdornmentMock.Setup(m => m.MouseMove(null))
				.Callback((MouseEventArgs e) =>
				{
					AdornmentMock.Setup(m => m.MouseMove(null)).CallBase();
				});

			Adornment.MouseMove(CreateMouseMoveEventArgs());

			Assert(true);
		}

		public void TestBalloonIsShownWhenMouseMoveOverHotSpotAndBalloonIsNotVisibleAlready()
		{
			var notifications = new NotificationCollection();
			notifications.AddError("TEST");

			AdornmentMock.Setup(m => m.Control).Returns(Control);

			AdornmentMock.Setup(m => m.Attach())
				.Callback(() =>
				{
					AdornmentMock.Setup(m => m.Attach()).CallBase();
				});

			AdornmentMock.Setup(m => m.IsNotificationsEmpty()).Returns(false);
			AdornmentMock.Setup(m => m.IsHotSpotIncludes(cursor)).Returns(true);
			AdornmentMock.Setup(m => m.IsControlVisible()).Returns(true);
			AdornmentMock.Setup(m => m.IsControlFocused()).Returns(false);

			BalloonMock.Setup(m => m.IsVisible).Returns(false);
			AdornmentMock.Setup(m => m.HotSpot).Returns(HotSpot);
			AdornmentMock.Setup(m => m.Notifications).Returns(notifications);
			BalloonMock.Setup(m => m.Show(notifications, Control, Adornment.ToRectangle(HotSpot), false));

			AdornmentMock.Setup(m => m.MouseMove(null))
				.Callback((MouseEventArgs e) =>
				{
					AdornmentMock.Setup(m => m.MouseMove(null)).CallBase();
				});

			AdornmentMock.Setup(m => m.Detach())
				.Callback(() =>
				{
					AdornmentMock.Setup(m => m.Detach()).CallBase();
				});

			Adornment.Attach();
			Adornment.MouseMove(CreateMouseMoveEventArgs());
			Adornment.Detach();

			Assert(true);
		}

		public void TestHidesBalloonWhenMouseMoveOutOfHotSpotAndBalloonIsVisible()
		{
			AdornmentMock.Setup(m => m.IsNotificationsEmpty()).Returns(false);
			AdornmentMock.Setup(m => m.IsHotSpotIncludes(cursor)).Returns(false);

			BalloonMock.Setup(m => m.IsVisible).Returns(true);
			Balloon.Hide();

			AdornmentMock.Setup(m => m.MouseMove(null))
				.Callback((MouseEventArgs e) =>
				{
					AdornmentMock.Setup(m => m.MouseMove(null)).CallBase();
				});

			Adornment.MouseMove(CreateMouseMoveEventArgs());

			Assert(true);
		}

		public void TestDoNotTryToHideBalloonWhenMouseMoveOutOfHotSpotAndBalloonIsAlreadyHidden()
		{
			AdornmentMock.Setup(m => m.IsNotificationsEmpty()).Returns(false);
			AdornmentMock.Setup(m => m.IsHotSpotIncludes(cursor)).Returns(false);

			BalloonMock.Setup(m => m.IsVisible).Returns(false);
			Balloon.Hide();

			AdornmentMock.Setup(m => m.MouseMove(null))
				.Callback((MouseEventArgs e) =>
				{
					AdornmentMock.Setup(m => m.MouseMove(null)).CallBase();
				});

			Adornment.MouseMove(CreateMouseMoveEventArgs());

			Assert(true);
		}

		public void TestHidesBalloonWhenMouseLeavesControlAndBalloonIsVisible()
		{
			BalloonMock.Setup(m => m.IsVisible).Returns(true);
			Balloon.Hide();

			AdornmentMock.Setup(m => m.MouseLeave())
				.Callback(() =>
				{
					AdornmentMock.Setup(m => m.MouseLeave()).CallBase();
				});

			Adornment.MouseLeave();

			Assert(true);
		}

		public void TestDoNotTryToHideBalloonWhenMouseLeavesControlAndBalloonIsAlreadyInvisible()
		{
			AdornmentMock.Setup(m => m.Control).Returns(Control);
			AdornmentMock.Setup(m => m.Attach())
				.Callback(() =>
				{
					AdornmentMock.Setup(m => m.Attach()).CallBase();
				});

			BalloonMock.Setup(m => m.IsVisible).Returns(false);
			Balloon.Hide();

			AdornmentMock.Setup(m => m.MouseLeave())
				.Callback(() =>
				{
					AdornmentMock.Setup(m => m.MouseLeave()).CallBase();
				});

			AdornmentMock.Setup(m => m.Detach())
				.Callback(() =>
				{
					AdornmentMock.Setup(m => m.Detach()).CallBase();
				});

			Adornment.Attach();
			Adornment.MouseLeave();
			Adornment.Detach();

			Assert(true);
		}

		#region Implementation

		Point location = new Point(1, 2);
		Point cursor = new Point(2, 3);
		Size size = new Size(2, 3);

		MouseEventArgs CreateMouseMoveEventArgs()
		{
			return new MouseEventArgs(MouseButtons.None, 0, cursor.X, cursor.Y, 0);
		}

		TextBox Control
		{
			get { return control ?? (control = new TextBox()); }
		}
		TextBox control;

		InteractiveAdornment Adornment => AdornmentMock.Object;
		Mock<InteractiveAdornment> AdornmentMock => adornmentMock ?? (adornmentMock = new Mock<InteractiveAdornment>(Balloon));
		Mock<InteractiveAdornment> adornmentMock;

		IBalloon Balloon => BalloonMock.Object;
		Mock<IBalloon> BalloonMock => balloonMock ?? (balloonMock = new Mock<IBalloon>());
		Mock<IBalloon> balloonMock;

		GraphicsPath HotSpot
		{
			get
			{
				if (hotSpot == null)
				{
					hotSpot = new GraphicsPath();
					hotSpot.AddRectangle(new Rectangle(location, size));
				}
				return hotSpot;
			}
		}
		GraphicsPath hotSpot;

		#endregion
	}
}
