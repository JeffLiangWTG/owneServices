using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.Windows.UI.Interop;
using CargoWise.Windows.UI.Testing;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// The form that hosts the list control for auto-list operations.
	/// </summary>
	[SuppressFormDesignerAnalysis]
	internal class AutoListControlHostForm : KForm
	{
		public AutoListControlHostForm(AutoListManager owner)
		{
			this.owner = owner;
			this.FormBorderStyle = FormBorderStyle.None;
			this.ShowInTaskbar = false;

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		// Calculate and set the location of the auto-list list control.
		public void UpdateAutoListFormLocation()
		{
			if (this.owner.TextBox != null)
			{
				NativeMethods.POINT nativeLocation = new NativeMethods.POINT();
				UnsafeNativeMethods.GetCaretPos(nativeLocation);

				// calculate the top/left of the list box
				Point location = owner.TextBox.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(nativeLocation.x, nativeLocation.y + this.owner.TextBox.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false));
				int left = location.X;
				int top = location.Y;

				if (left + Size.Width > EntireWorkingAreaOfAllScreens.Width)
				{
					left = (EntireWorkingAreaOfAllScreens.Width - Size.Width);
					if (left < 0)
					{
						left = 0;
					}
				}
				if (top + Size.Height > EntireWorkingAreaOfAllScreens.Height)
				{
					top -= (owner.TextBox.Height + Size.Height);
				}

				SafeNativeMethods.SetWindowPos(
					new HandleRef(this, Handle),
					new HandleRef(this, new IntPtr(SetWindowPosFlags.HWND_TOPMOST)),
					left, top, Width, Height,
					SetWindowPosFlags.SWP_NOACTIVATE);
			}
		}

		Rectangle EntireWorkingAreaOfAllScreens
		{
			get { return entireWorkingAreaOfAllScreens ?? (Rectangle)(entireWorkingAreaOfAllScreens = GetEntireWorkingAreaOfAllScreens()); }
		}
		Rectangle? entireWorkingAreaOfAllScreens;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "The value is cached")]
		[return:DpiState(DpiState.ScaledVariant)]
		static Rectangle GetEntireWorkingAreaOfAllScreens()
		{
			Rectangle result = ControlDpiScalingHelper.NewScaledRectangle(0, 0, 0, 0);
			foreach (Screen screen in Screen.AllScreens) // The value is cached
			{
				result = Rectangle.Union(result, screen.WorkingArea);
			}
			return result;
		}

		public Control HostedControl
		{
			get { return this.Controls.Count == 0 ? null : this.Controls[0]; }
			set
			{
				if (HostedControl != null)
				{
					HostedControl.SizeChanged -= new EventHandler(HostControl_SizeChanged);
				}
				this.Controls.Clear();
				if (value != null)
				{
					this.Size = value.Size;
					value.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
					this.Controls.Add(value);
					value.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
					value.SizeChanged += new EventHandler(HostControl_SizeChanged);
					value.LocationChanged += new EventHandler(HostControl_LocationChanged);
				}
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams result = base.CreateParams;
				result.ExStyle =
					result.ExStyle |
					WindowExStyles.WS_EX_TOOLWINDOW;
				return result;
			}
		}

		protected override void SetVisibleCore(bool value)
		{
			if (value)
			{
				if (!this.IsHandleCreated)
				{
					CreateHandle();
				}
				UpdateAutoListFormLocation();
			}
			base.SetVisibleCore(value);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		#region Implementation

		readonly AutoListManager owner;

		void HostControl_SizeChanged(object sender, EventArgs e)
		{
			if (HostedControl != null)
			{
				Size = HostedControl.Size;
			}
			UpdateAutoListFormLocation();
		}

		void HostControl_LocationChanged(object sender, EventArgs e)
		{ ((Control)sender).Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0); }

		#endregion
	}
}
