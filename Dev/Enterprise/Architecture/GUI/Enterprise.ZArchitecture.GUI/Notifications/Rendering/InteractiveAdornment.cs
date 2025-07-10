using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Balloons;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public abstract class InteractiveAdornment : NotificationAdornment
	{
		#region Constructors

		public InteractiveAdornment()
			: this(Balloon.Instance)
		{
		}

		public InteractiveAdornment(IBalloon balloon)
		{
			this.balloon = balloon;
		}

		#endregion

		#region Mounting

		bool attached;

		public override void Attach()
		{
			if (attached)
			{
				throw new InvalidOperationException("Can not attach twice");
			}

			var host = Control.Parent;

			if (host != null && !host.IsDisposed)
			{
				host.MouseMove += OnHostMouseMove;
			}

#if WINZOR
			Control.MouseOver += OnMouseOver;
#endif
			Control.MouseMove += OnControlMouseMove;
			Control.ParentChanged += OnParentChanged;

			attached = true;
		}

		public override void Detach()
		{
			if (!attached)
			{
				return;
			}

			ResumeUserIdleWorker();
			var host = Control.Parent;

			if (host != null)
			{
				host.MouseMove -= OnHostMouseMove;
			}

#if WINZOR
			Control.MouseOver -= OnMouseOver;
#endif

			Control.MouseMove -= OnControlMouseMove;
			Control.ParentChanged -= OnParentChanged;

			attached = false;
		}

		#region Control Event Handlers

		bool wasOverControl;

		void OnHostMouseMove(object sender, MouseEventArgs e)
		{
			var parent = (Control)sender;
			var aCandidate = parent.GetChildAtPoint(e.Location);

			if (Control == aCandidate)
			{
				wasOverControl = true;
				var clientPoint = Control.PointToClient(parent.PointToScreen(e.Location));
				MouseMove(new MouseEventArgs(e.Button, 0, clientPoint.X, clientPoint.Y, 0));
			}
			else
			{
				if (wasOverControl)
				{
					wasOverControl = false;
					MouseLeave();
				}
			}
		}

		void OnControlMouseMove(object sender, MouseEventArgs e)
		{
			MouseMove(e);
		}

		void OnParentChanged(object sender, EventArgs e)
		{
			if (attached)
			{
				Detach();

				if (Control.Parent != null && !Control.Parent.IsDisposed)
				{
					Attach();
				}
			}
		}

		#endregion

		#endregion

		protected internal virtual void OnMouseOver(object sender, MouseEventArgs e)
		{
			UpdateToolTip(e);
		}

		/// <summary>
		/// Track mouse movements over the control.
		/// </summary>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		protected internal virtual void MouseMove(MouseEventArgs e)
		{
#if WINZOR
			HideTip();
#endif
			UpdateToolTip(e);
		}

		protected void UpdateToolTip(MouseEventArgs e)
		{
			if (IsNotificationsEmpty())
			{
				return;
			}

			if (CanShowTip(e.Location))
			{
				ShowTip();
			}
			else
			{
				HideTip();
			}
		}

		bool CanShowTip(Point location)
		{
			return IsHotSpotIncludes(location) && IsControlVisible() && !IsControlFocused();
		}

		/// <summary>
		/// Called when mouse leave the control.
		/// </summary>
		protected internal virtual void MouseLeave()
		{
			HideTip();
		}

		protected internal virtual bool IsHotSpotIncludes(Point point)
		{
			return HotSpot != null && HotSpot.IsVisible(point);
		}

		protected internal virtual bool IsNotificationsEmpty()
		{
			return State == null;
		}

		protected internal virtual bool IsControlFocused()
		{
			return Control.Focused && !(Control is ZCheckBox);
		}

		protected internal virtual bool IsControlVisible()
		{
			return Control.Visible;
		}

		public Rectangle ToRectangle(GraphicsPath path)
		{
			var bounds = path.GetBounds();
			return ControlDpiScalingHelper.NewScaledRectangle(Convert.ToInt32(bounds.X), Convert.ToInt32(bounds.Y), Convert.ToInt32(bounds.Width), Convert.ToInt32(bounds.Height), false);
		}

		protected internal abstract GraphicsPath HotSpot { get; }

		#region Implementation

		[ThreadStatic]
		static IDisposable userIdleWorkerSuspender;
		readonly IBalloon balloon;

		void HideTip()
		{
			if (balloon.IsVisible)
			{
				balloon.Hide();
			}
			ResumeUserIdleWorker();
		}

		void ShowTip()
		{
			if (!balloon.IsVisible)
			{
				ResumeUserIdleWorker();
				userIdleWorkerSuspender = UserIdleWorker.Suspend();

				balloon.Show(Notifications, Control, ToRectangle(HotSpot), false);
			}
		}

		static void ResumeUserIdleWorker()
		{
			if (userIdleWorkerSuspender != null)
			{
				userIdleWorkerSuspender.Dispose();
				userIdleWorkerSuspender = null;
			}
		}

		#endregion
	}
}
