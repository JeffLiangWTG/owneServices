using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Balloons
{
	public interface IBalloon
	{
		bool IsVisible { get; }

		void Show(BalloonDescriptor descriptor);
		void Show(IExtendedControl control);
		void Show(IEnumerable<INotification> notifications, Control anchorControl, Rectangle anchorRectangle, bool hideOnMouseHover);

		void Hide();
	}

	public sealed class Balloon : IBalloon
	{
		internal Balloon() { }

		public static Balloon Instance
		{
			get { return instance ?? (instance = new Balloon()); }
		}
		[ThreadStatic]
		static Balloon instance;

		public bool IsVisible
		{
			get { return ShouldSimulateShow() ? isSimulatedVisible : isVisible; }
		}
		internal bool isVisible, isSimulatedVisible;

		public void Show(IExtendedControl control)
		{
			var notifications = NotificationCollection.Empty;

			if (control.Extensions.Supports<INotificationExtension>())
			{
				notifications = control.Extensions.Get<INotificationExtension>().Notifications;
			}

			Show(notifications.GetUniqueNotifications(), control.Host, control.Host.ClientRectangle, false);
		}

		public void Show(IEnumerable<INotification> notifications, Control anchorControl, Rectangle anchorRectangle, bool hideOnMouseHover)
		{
			var caption = "";
			var description = "";

			var hint = GetHint(anchorControl);
			if (hint != null)
			{
				caption = hint.Caption;
				description = hint.Description;
			}
			if (notifications.Count() > 0 && anchorControl is ZDateEdit zDateEdit)
			{
				Show(caption, description, zDateEdit.CalendarButton, zDateEdit.CalendarButton.ClientRectangle, hideOnMouseHover, false, notifications);
			}
			else
			{
				Show(caption, description, anchorControl, anchorRectangle, hideOnMouseHover, false, notifications);
			}
		}

		public void Show(string caption, string message, Control anchorControl, Rectangle anchorRectangle, bool hideOnMouseHover, bool forceTopOfScreen)
		{
			Show(caption, message, anchorControl, anchorRectangle, hideOnMouseHover, forceTopOfScreen, null);
		}

		void Show(string caption, string message, Control anchorControl, Rectangle anchorRectangle, bool hideOnMouseHover, bool forceTopOfScreen, IEnumerable<INotification> notifications)
		{
			var balloonDescriptor = new BalloonDescriptor(anchorControl, anchorRectangle, caption, message, notifications);
			BalloonWindow.ForceTopOfScreen = forceTopOfScreen;
			if (balloonDescriptor.ParentForm?.TopMost ?? false)
			{
				BalloonWindow.TopMost = true;
			}
			try
			{
				balloonDescriptor.HideWhenMouseOverBalloon = hideOnMouseHover;
				Show(balloonDescriptor);
			}
			finally
			{
				if (HasBaloonWindow)
				{
					BalloonWindow.ForceTopOfScreen = false;
				}
			}
		}

		public void Show(BalloonDescriptor descriptor)
		{
			if (descriptor == null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			if (isVisible || descriptor.IsEmpty)
			{
				Hide();
			}

			if (!descriptor.IsEmpty)
			{
				BalloonWindow.Descriptor = descriptor;

				if (ShouldSimulateShow())
				{
					isSimulatedVisible = true;
				}
				else
				{
					BalloonWindow.Show();
					isVisible = true;
				}
			}
		}

		bool ShouldSimulateShow()
		{
			return Globals.IsTest && !IsShownDuringTesting;
		}

		public bool IsShownDuringTesting { get; set; }

		public void Hide()
		{
			if (!TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				if (balloonWindow != null)
				{
					var tempBalloonWindow = balloonWindow;
					balloonWindow = null;
					tempBalloonWindow.Visible = false;
					tempBalloonWindow.Descriptor = null;
					tempBalloonWindow.Dispose();
				}
				isVisible = false;
				isSimulatedVisible = false;
			}
		}

		static IHintExtension GetHint(Control control)
		{
			return (control as IExtendedControl)?.Extensions.Get<IHintExtension>()
				?? (control is ZDropButton || control is ZButton.Bare ? (control.Parent as IExtendedControl)?.Extensions.Get<IHintExtension>() : null);
		}

		#region Implementation

		internal BalloonWindow BalloonWindow
		{
			get
			{
				if (!HasBaloonWindow)
				{
					balloonWindow = new BalloonWindow();
				}
				return balloonWindow;
			}
		}
		internal BalloonWindow balloonWindow;

		bool HasBaloonWindow
		{
			get { return balloonWindow != null && !balloonWindow.IsDisposed; }
		}

		#endregion

		#region Test
#if DEBUG

		public IBalloonWindow BalloonWindowExposedForTesting
		{
			get { return BalloonWindow; }
		}
		public BalloonWindow BalloonWindowExposedForTestingPortalAndPopup
		{
			get { return BalloonWindow; }
		}

#endif
		#endregion
	}
}
