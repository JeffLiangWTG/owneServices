using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressControlRequiresTextBasher]
	[SuppressFormDesignerAnalysis]
	[SuppressFormsLocalizedTest]
	public class ZDropButtonOnly : ZButton, ISupportInitialize
	{
		public ZDropButtonOnly()
		{
			UserEventTracker.Instance.AddUserEventToControl(this);
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
		}

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(false)]
		[Description("Should the control resize by shifting the left edge or right edge")]
		public bool LockRightEdge
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return lockRightEdge; }
			[System.Diagnostics.DebuggerStepThrough]
			set { lockRightEdge = value; }
		}

		#region Paint and Layout

		protected bool HasXPSupport
		{
			get { return XpUtils.IsAppUsingXpTheme && XpUtils.IsWindowsXpOrGreater && XpUtils.IsWindowsUsingXpTheme; }
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (!isInitialising)
			{
				width = ZGUISystemInformation.VerticalScrollBarWidth;
			}

			base.SetBoundsCore(x, y, width, height, specified);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			ButtonRectangle = ControlDpiScalingHelper.NewScaledRectangle(ButtonPoint.X, ButtonPoint.Y, ButtonSize.Width, ButtonSize.Height, false);

			if (HasXPSupport)
			{
				var buttonRect = new RECT(ButtonRectangle);

				var hDC = e.Graphics.GetHdc();

				try
				{
					XpThemeAPI.DrawThemeBackground(ButtonThemeData, hDC, 1, CurrentDrawState, ref buttonRect, ref buttonRect);
				}
				finally
				{
					e.Graphics.ReleaseHdc(hDC);
				}
			}
			else
			{
				var buttonState = (CurrentDrawState == XpThemeAPI.Constants.CBXS_PRESSED) ? ButtonState.Pushed : ButtonState.Normal;
				if (ButtonRectangle.Width > 0 && ButtonRectangle.Height > 0)
				{
					ControlPaint.DrawComboButton(e.Graphics, ButtonRectangle, buttonState);
				}
			}
		}

		protected virtual Point ButtonPoint
		{
			get { return ControlDpiScalingHelper.NewScaledPoint(ClientRectangle.Left, ClientRectangle.Top, false); }
		}

		protected virtual Size ButtonSize
		{
			get { return ControlDpiScalingHelper.NewScaledSize(ClientRectangle.Width, ClientRectangle.Height, false); }
		}

		protected virtual bool ShouldDrawBorder
		{
			get { return true; }
		}

		#region Theme Data

		protected IntPtr ButtonThemeData
		{
			get
			{
				if (fButtonThemeData == IntPtr.Zero)
				{
					fButtonThemeData = XpThemeAPI.OpenThemeData(this.Handle, "ComboBox");
				}

				return fButtonThemeData;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		protected IntPtr EditThemeData
		{
			get
			{
				if (fEditThemeData == IntPtr.Zero)
				{
					fEditThemeData = XpThemeAPI.OpenThemeData(this.Handle, "Edit");
				}

				return fEditThemeData;
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			CloseThemeData();
			base.Dispose(isNotFinalizing);
		}

		IntPtr fButtonThemeData = IntPtr.Zero;
		IntPtr fEditThemeData = IntPtr.Zero;

		#endregion

		#endregion

		#region Focus Events

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			CurrentDrawState = XpThemeAPI.Constants.CBXS_HOT;
			Invalidate();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			CurrentDrawState = XpThemeAPI.Constants.CBXS_NORMAL;
			Invalidate();
		}

		#endregion

		#region Mouse Events

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (ButtonRectangle.Contains(e.X, e.Y))
			{
				if (e.Button == MouseButtons.Left && e.Clicks == 1)
				{
					CurrentDrawState = XpThemeAPI.Constants.CBXS_PRESSED;
					base.Invalidate();
				}
			}

			base.OnMouseDown(e); // this will call OnEnter() which will also attempt to pull the list
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (ButtonRectangle.Contains(e.X, e.Y))
			{
				CurrentDrawState = XpThemeAPI.Constants.CBXS_HOT;
				Invalidate();
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			CurrentDrawState = XpThemeAPI.Constants.CBXS_NORMAL;
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (ButtonRectangle.Contains(e.X, e.Y))
			{
				if (CurrentDrawState != XpThemeAPI.Constants.CBXS_PRESSED)
				{
					CurrentDrawState = XpThemeAPI.Constants.CBXS_HOT;
					Invalidate();
				}
			}
			else
			{
				CurrentDrawState = XpThemeAPI.Constants.CBXS_NORMAL;
				Invalidate();
			}
		}
#if DEBUG
		internal
#endif
		int CurrentDrawState;

		#endregion

		#region Implemenation

		protected override void WndProc(ref Message m)
		{
			if (m.Msg != WindowsMessage.WM_ERASEBKGND)
			{
				base.WndProc(ref m);
			}

			if (m.Msg == WindowsMessage.WM_THEMECHANGED)
			{
				CloseThemeData();
				Invalidate();
			}
		}

		void CloseThemeData()
		{
			if (fButtonThemeData != IntPtr.Zero)
			{
				XpThemeAPI.NativeMethods.CloseThemeData(fButtonThemeData);
				fButtonThemeData = IntPtr.Zero;
			}

			if (fEditThemeData != IntPtr.Zero)
			{
				XpThemeAPI.NativeMethods.CloseThemeData(fEditThemeData);
				fEditThemeData = IntPtr.Zero;
			}
		}

		#endregion

		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{
			isInitialising = true;
		}

		void ISupportInitialize.EndInit()
		{
			isInitialising = false;

			var xOffset = 0;
			var desiredWidth = ZGUISystemInformation.VerticalScrollBarWidth;

			if (LockRightEdge)
			{
				xOffset = this.Width - desiredWidth;
			}

			Bounds = ControlDpiScalingHelper.NewScaledRectangle(Left + xOffset, Top, desiredWidth, Height, false);
		}

		#endregion
		bool isInitialising;
#if DEBUG
		internal
#endif
		bool lockRightEdge;
		Rectangle ButtonRectangle;

#if DEBUG
		internal void InvokeGotFocusExposed(Control control, EventArgs e)
		{
			InvokeGotFocus(control, e);
		}

		internal void InvokeLostFocusExposed(Control control, EventArgs e)
		{
			InvokeLostFocus(control, e);
		}
#endif
	}
}
