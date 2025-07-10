using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Balloons
{
	public interface IBalloonWindow
	{
		BalloonDescriptor Descriptor { get; }
	}

	[SuppressFormDesignerAnalysis]
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
#if DEBUG
	public
#else
	internal
#endif
 class BalloonWindow : Form, IBalloonWindow, ICaptionedComponents
	{
		public BalloonWindow()
		{
			StartPosition = FormStartPosition.Manual;
			BackColor = System.Drawing.SystemColors.Info;
			FormBorderStyle = FormBorderStyle.None;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "UberBalloonForm";
			ShowInTaskbar = false;
			SizeGripStyle = SizeGripStyle.Hide;
			MakeOpaque();
			EnableAlphaBlendingIfAppropriate();

			translationFeedbackManager = new TranslationFeedbackManager(this, TranslationFeedbackManager.ClickMode.OnMouseUp);
			devInfoPopupManager = new DevInfoPopupManager(this, TranslationFeedbackManager.ClickMode.OnMouseUp);
		}

		void EnableAlphaBlendingIfAppropriate()
		{
			AlphaBlendingEnabled = !ObjectFactory.Get<TerminalService>().IsWTSSession;
		}

		public BalloonDescriptor Descriptor
		{
			get { return descriptor; }
			set
			{
				if (value != descriptor)
				{
					if (descriptor != null)
					{
						UnhookAnchorControlAndParentFormEvents();
					}

					descriptor = value;
					if (value == null)
					{
						Hide();
					}
					else
					{
						HideQuickly();
					}

					if (descriptor != null)
					{
						SizeDirty = true;
						PositionDirty = true;
						UpdateSizeAndPosition();
						HookAnchorControlAndParentFormEvents();
					}
				}
			}
		}
		BalloonDescriptor descriptor;

		#region DPI scaling overrides

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				this.AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
				this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		#endregion

		#region Disabling Balloon on Parent Change

		bool hasHookedFormDispose;

		void HookAnchorControlAndParentFormEvents()
		{
			if (Descriptor.AnchorControl != null)
			{
				HookTabPageEvents();

				Descriptor.AnchorControl.Disposed += DisposeBalloonOnFormOrControlDispose;
				Descriptor.AnchorControl.VisibleChanged += DisableBalloonOnFormOrControlChange;
				Descriptor.AnchorControl.LocationChanged += DisableBalloonOnFormOrControlChange;
			}

			if (Descriptor.ParentForm != null)
			{
				if (!hasHookedFormDispose)
				{
					Descriptor.ParentForm.Disposed += DisposeBalloonOnFormOrControlDispose;
					hasHookedFormDispose = true;
				}

				Descriptor.ParentForm.LocationChanged += DisableBalloonOnFormOrControlChange;
				Descriptor.ParentForm.VisibleChanged += DisableBalloonOnFormOrControlChange;
				Descriptor.ParentForm.Deactivate += DisableBalloonOnFormDeactive;
			}
		}

		void UnhookAnchorControlAndParentFormEvents()
		{
			if (Descriptor.AnchorControl != null)
			{
				UnhookTabPageEvents();

				Descriptor.AnchorControl.Disposed -= DisposeBalloonOnFormOrControlDispose;
				Descriptor.AnchorControl.VisibleChanged -= DisableBalloonOnFormOrControlChange;
				Descriptor.AnchorControl.LocationChanged -= DisableBalloonOnFormOrControlChange;
			}

			if (Descriptor.ParentForm != null)
			{
				Descriptor.ParentForm.LocationChanged -= DisableBalloonOnFormOrControlChange;
				Descriptor.ParentForm.VisibleChanged -= DisableBalloonOnFormOrControlChange;
				Descriptor.ParentForm.Deactivate -= DisableBalloonOnFormDeactive;
			}
		}

		internal void DisableBalloonOnFormOrControlChange(object sender, EventArgs e)
		{
			Descriptor = null;
			HideQuickly();
		}

		internal void DisableBalloonOnFormDeactive(object sender, EventArgs e)
		{
			if (Descriptor?.ParentForm != null && !Descriptor.ParentForm.Disposing)
			{
				Descriptor = null;
				HideQuickly();
			}
		}

		void DisposeBalloonOnFormOrControlDispose(object sender, EventArgs e)
		{
			Descriptor = null;
			Dispose();
		}

		void HookTabPageEvents()
		{
			var tab = FindHostTab(Descriptor.AnchorControl);

			if (tab != null)
			{
				tab.Leave += DisableBalloonOnFormOrControlChange;
			}
		}

		void UnhookTabPageEvents()
		{
			var tab = FindHostTab(Descriptor.AnchorControl);

			if (tab != null)
			{
				tab.Leave -= DisableBalloonOnFormOrControlChange;
			}
		}

		static ZTabPage FindHostTab(Control control)
		{
			while (control.Parent != null)
			{
				if (control.Parent is ZTabPage)
				{
					return (ZTabPage)control.Parent;
				}

				control = control.Parent;
			}

			return null;
		}

		#endregion

		#region Show / Hide

		internal new void Show()
		{
			if (!Visible)
			{
				if (FadeControlTimer != null && FadeControlTimer.Enabled)
				{
					FadeControlTimer.Stop();
				}

				if (AlphaBlendingEnabled)
				{
					MakeTransparent();
					ShownButNotPainted = true;
				}

				SafeNativeMethods.SetWindowPos(new HandleRef(this, Handle), NativeMethods.HWND_TOP, PlacedLocation.X, PlacedLocation.Y, fBalloonSize.Width, fBalloonSize.Height, NativeMethods.SWP_NOCOPYBITS | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE);

				SafeNativeMethods.ShowWindow(new HandleRef(this, Handle), (int)ShowWindowsOptions.SW_SHOWNOACTIVATE);

				SetMouseHook();

				Visible = true;
			}
		}

		new void Hide()
		{
			RemoveMouseHook();
			if (Visible)
			{
				ShownButNotPainted = false;
				if (AlphaBlendingEnabled)
				{
					StartFadeControlTimer();
				}
				else
				{
					HideQuickly();
				}
			}
		}

		void HideQuickly()
		{
			RemoveMouseHook();
			if (Visible)
			{
				ShownButNotPainted = false;
				Visible = false;
				base.Hide();
			}
		}

		bool ShownButNotPainted;

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (Descriptor != null && Descriptor.HideWhenMouseOverBalloon && !TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				Hide();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (!TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				HideQuickly();
			}
		}

		#endregion

		#region Mouse Hook

		SafeNativeMethods.MouseHookProc mouseHookProcEventHandler;
#if DEBUG
		internal
#endif
 IntPtr mouseHook;

		protected void SetMouseHook()
		{
			if (mouseHook == IntPtr.Zero && Descriptor != null && Descriptor.Notifications != null)
			{
				var count = Descriptor.AllUniqueNotifications.Count();
				if (count <= BalloonDescriptor.NotificationsCap)
				{
					return;
				}
				if (mouseHookProcEventHandler == null)
				{
					mouseHookProcEventHandler = new SafeNativeMethods.MouseHookProc(MouseHookProc);
				}

				mouseHook = UnsafeNativeMethods.SetWindowsHookEx(WindowsMessage.WH_MOUSE, mouseHookProcEventHandler, IntPtr.Zero, SafeNativeMethods.GetCurrentThreadId());
			}
		}

		IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			//may be called twice per message, like the one in ZDropButton.cs

			var handled = false;

			if (nCode >= 0 && !this.Disposing)
			{
				var mouseHookInfoEx = (SafeNativeMethods.MouseHookInfoEx)Marshal.PtrToStructure(lParam, typeof(SafeNativeMethods.MouseHookInfoEx));
				var mouseHookInfo = mouseHookInfoEx.MouseHookInfo;

				switch ((int)wParam)
				{
					case SafeNativeMethods.WM_MOUSEWHEEL:
						handled = HandleMouseScroll(mouseHookInfoEx.MouseData);
						break;
				}
			}

			if (!handled)
			{
				return UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, mouseHook), nCode, wParam, lParam);
			}
			else
			{
				return new IntPtr(1);
			}
		}

		void RemoveMouseHook()
		{
			if (mouseHook != IntPtr.Zero)
			{
				UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, mouseHook));
				mouseHook = IntPtr.Zero;
			}
		}

		const int scrollWheelUnit = 24;
		const int messageOffsetDelta = 1;
		int currentMessageOffset;
		int scrollCounter;
		bool ForceNewRegion;

#if DEBUG
		internal
#endif
 protected bool HandleMouseScroll(int mouseData)
		{
			// See this site for info on the algorithm below.
			// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookstructures/mousehookstruct.asp

			// Because enhanced mousewheels send deltas less than 120, we must keep a counter of accumulated deltas thus far.
			// http://www.codeproject.com/Articles/155717/Handling-Enhanced-Mouse-Wheels-in-your-Application

			if (IsDisposed || !Visible || Descriptor == null || Descriptor.Notifications == null)
			{
				RemoveMouseHook();
				return false;
			}

			var oldMessageOffset = currentMessageOffset;
			var count = Descriptor.AllUniqueNotifications.Count();
			var delta = mouseData >> 16;
			scrollCounter += delta;

			while (scrollCounter <= -scrollWheelUnit)
			{
				currentMessageOffset = Math.Min(currentMessageOffset + messageOffsetDelta, count - BalloonDescriptor.NotificationsCap);
				scrollCounter += scrollWheelUnit;
			}
			while (scrollCounter >= scrollWheelUnit)
			{
				currentMessageOffset = Math.Max(currentMessageOffset - messageOffsetDelta, 0);
				scrollCounter -= scrollWheelUnit;
			}

			//forces redraw of balloon
			if (oldMessageOffset != currentMessageOffset)
			{
				Region = null;
				SizeDirty = true;
				PositionDirty = true;
				ForceNewRegion = true;
				this.Invalidate();
				return true;
			}

			return false; //return true so the scroll doesn't scroll away the thing you were checking the notifications of?
		}

		#endregion

		#region CreateParams

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.ClassName = "tooltips_class32";
				cp.ExStyle |= NativeMethods.WindowStyles.WS_EX_LAYERED;
				cp.ClassStyle |= NativeMethods.CS_SAVEBITS;
				return cp;
			}
		}

		#endregion

		#region Calculate Window Size

		[return: DpiState(DpiState.ScaledVariant)]
		Size CalcWindowSize()
		{
			SizeDirty = false;
			var result = CalcClientSize();
			ControlDpiScalingHelper.SetWidth(ref result, result.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN * 2), false);
			ControlDpiScalingHelper.SetHeight(ref result, result.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(nTIP_TAIL + nTIP_MARGIN), false);
			return result;
		}

		Size CalcClientSize()
		{
			using (var g = Graphics.FromHwnd(this.Handle))
			{
				HeaderSize = CalcHeaderSize(g);
				DescriptionSize = CalcDescriptionSize(g);
				ContentSize = CalcContentSize(g);
			}

			var compositeWidth = Math.Max(HeaderSize.Width, ContentSize.Width);
			compositeWidth = Math.Max(DescriptionSize.Width, compositeWidth);
			if (ContentSize.Height > 0)
			{
				return ControlDpiScalingHelper.NewScaledSize(compositeWidth, HeaderSize.Height + DescriptionSize.Height + ContentSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(nTIP_MARGIN + nTIP_MARGIN), false);
			}
			else
			{
				return ControlDpiScalingHelper.NewScaledSize(compositeWidth, HeaderSize.Height + DescriptionSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(nTIP_MARGIN), false);
			}
		}

		[return: DpiState(DpiState.ScaledVariant)]
		Size CalcHeaderSize(Graphics g)
		{
			//Size IconSize = new Size(16, 16);

			var availableWidth = (AnchorScreenBounds.Width / 2) /*- IconSize.Width */;

			var captionStringFormat = new StringFormat(StringFormatFlags.NoWrap);
			captionStringFormat.Alignment = StringAlignment.Near;
			var captionSize = TextRendererHelper.MeasureText(g, descriptor.Caption, CaptionFont, ControlDpiScalingHelper.NewScaledSize(availableWidth, 0, false), captionStringFormat);
			HeaderSize = ControlDpiScalingHelper.NewScaledSize(/*IconSize.Width  + */(int)Math.Ceiling(captionSize.Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN),
				(int)/*Math.Max(IconSize.Height, */Math.Ceiling(captionSize.Height), false);

			return HeaderSize;
		}

		Size CalcStringSize(Graphics g, string aString, Font aFont, int widthUsed)
		{
			var availableWidth = AnchorScreenBounds.Width / 2 - ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN * 2) - widthUsed;
			var aStringSize = TextRendererHelper.MeasureText(g, aString, aFont, ControlDpiScalingHelper.NewScaledSize(availableWidth, 0, false), new StringFormat());
			return ControlDpiScalingHelper.NewScaledSize((int)Math.Ceiling(aStringSize.Width), (int)Math.Ceiling(aStringSize.Height), false);
		}

		[return: DpiState(DpiState.ScaledVariant)]
		Size CalcStringSize(Graphics g, string aString, Font aFont)
		{
			return CalcStringSize(g, aString, aFont, 0);
		}

		[return: DpiState(DpiState.ScaledVariant)]
		Size CalcDescriptionSize(Graphics g)
		{
			DescriptionSize = Size.Empty;
			if (!string.IsNullOrEmpty(descriptor.Description))
			{
				DescriptionSize = CalcStringSize(g, descriptor.Description, DescriptionFont);
				ControlDpiScalingHelper.SetWidth(ref DescriptionSize, DescriptionSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN), false);
			}
			return DescriptionSize;
		}

		bool SizeDirty = true;
		bool PositionDirty = true;

		#endregion

		#region Sizing And Positioning Window and Region

		public bool ForceTopOfScreen { get; internal set; }

		enum BalloonQuadrant
		{
			TopRight,
			TopLeft,
			BottomRight,
			BottomLeft
		}

		Point AnchorPoint;
		BalloonQuadrant CurrentQuadrant;

		void UpdateAnchorPointAndQuadrant()
		{
			var anchor = descriptor.AnchorRectOnControl;
			var topOfControl = descriptor.AnchorControl.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(anchor.Width / 2 + anchor.X, anchor.Y, false));
			if (ForceTopOfScreen)
			{
				AnchorPoint = topOfControl;
			}
			else
			{
				CurrentQuadrant = GetQuadrant(topOfControl);
				switch (CurrentQuadrant)
				{
					case BalloonQuadrant.TopLeft:
					case BalloonQuadrant.TopRight:
						AnchorPoint = descriptor.AnchorControl.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(anchor.Width / 2 + anchor.X, anchor.Height + anchor.Y, false)); // bottom of control
						break;

					default:
						AnchorPoint = topOfControl;
						break;
				}
			}
		}

		void UpdateSizeAndPosition()
		{
			UpdateAnchorPointAndQuadrant();

			if (SizeDirty)
			{
				fBalloonSize = CalcWindowSize();
				Size = fBalloonSize;
			}

			if (PositionDirty)
			{
				PositionWindow();
			}
		}

		Point[] PositionBalloonTail(ref Point ptTopLeft, ref Point ptBottomRight)
		{
			var result = new Point[3];
			var scaledTail = ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_TAIL);
			switch (CurrentQuadrant)
			{
				case BalloonQuadrant.TopLeft:
					ControlDpiScalingHelper.SetY(ref ptTopLeft, nTIP_TAIL, true);
					ControlDpiScalingHelper.SetX(ref result[0], (fBalloonSize.Width - scaledTail) / 4 + scaledTail, false);
					ControlDpiScalingHelper.SetY(ref result[0], nTIP_TAIL + 1, true);
					ControlDpiScalingHelper.SetX(ref result[2], (fBalloonSize.Width - scaledTail) / 4, false);
					ControlDpiScalingHelper.SetY(ref result[2], result[0].Y, false);
					ControlDpiScalingHelper.SetX(ref result[1], result[2].X, false);
					ControlDpiScalingHelper.SetY(ref result[1], 1, true);
					break;

				case BalloonQuadrant.TopRight:
					ControlDpiScalingHelper.SetY(ref ptTopLeft, nTIP_TAIL, true);
					ControlDpiScalingHelper.SetX(ref result[0], (fBalloonSize.Width - scaledTail) / 4 * 3, false);
					ControlDpiScalingHelper.SetY(ref result[0], nTIP_TAIL + 1, true);
					ControlDpiScalingHelper.SetX(ref result[2], (fBalloonSize.Width - scaledTail) / 4 * 3 + scaledTail, false);
					ControlDpiScalingHelper.SetY(ref result[2], result[0].Y, false);
					ControlDpiScalingHelper.SetX(ref result[1], result[2].X, false);
					ControlDpiScalingHelper.SetY(ref result[1], 1, true);
					break;

				case BalloonQuadrant.BottomLeft:
					ControlDpiScalingHelper.SetY(ref ptBottomRight, fBalloonSize.Height - scaledTail, false);
					ControlDpiScalingHelper.SetX(ref result[0], (fBalloonSize.Width - scaledTail) / 4 + scaledTail, false);
					ControlDpiScalingHelper.SetY(ref result[0], fBalloonSize.Height - scaledTail - ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
					ControlDpiScalingHelper.SetX(ref result[2], (fBalloonSize.Width - scaledTail) / 4, false);
					ControlDpiScalingHelper.SetY(ref result[2], result[0].Y, false);
					ControlDpiScalingHelper.SetX(ref result[1], result[2].X, false);
					ControlDpiScalingHelper.SetY(ref result[1], fBalloonSize.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					break;

				case BalloonQuadrant.BottomRight:
					ControlDpiScalingHelper.SetY(ref ptBottomRight, fBalloonSize.Height - scaledTail, false);
					ControlDpiScalingHelper.SetX(ref result[0], (fBalloonSize.Width - scaledTail) / 4 * 3, false);
					ControlDpiScalingHelper.SetY(ref result[0], fBalloonSize.Height - scaledTail - ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					ControlDpiScalingHelper.SetX(ref result[2], (fBalloonSize.Width - scaledTail) / 4 * 3 + scaledTail, false);
					ControlDpiScalingHelper.SetY(ref result[2], result[0].Y, false);
					ControlDpiScalingHelper.SetX(ref result[1], result[2].X, false);
					ControlDpiScalingHelper.SetY(ref result[1], fBalloonSize.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
					break;
			}

			// adjust for very narrow balloons
			var scaledMargin = ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN);
			if (result[0].X < scaledMargin)
			{
				ControlDpiScalingHelper.SetX(ref result[0], scaledMargin, false);
			}
			if (result[0].X > fBalloonSize.Width - scaledMargin)
			{
				ControlDpiScalingHelper.SetX(ref result[0], fBalloonSize.Width - scaledMargin, false);
			}
			if (result[1].X < scaledMargin)
			{
				ControlDpiScalingHelper.SetX(ref result[1], scaledMargin, false);
			}
			if (result[1].X > fBalloonSize.Width - scaledMargin)
			{
				ControlDpiScalingHelper.SetX(ref result[1], fBalloonSize.Width - scaledMargin, false);
			}
			if (result[2].X < scaledMargin)
			{
				ControlDpiScalingHelper.SetX(ref result[2], scaledMargin, false);
			}
			if (result[2].X > fBalloonSize.Width - scaledMargin)
			{
				ControlDpiScalingHelper.SetX(ref result[2], fBalloonSize.Width - scaledMargin, false);
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Most calculations already done on current DPI")]
		void PositionWindow()
		{
			try
			{
				if (Handle != IntPtr.Zero)
				{
					var ptTopLeft = Point.Empty;
					var ptBottomRight = new Point(fBalloonSize.Width, fBalloonSize.Height);

					// force recalculation of desktop
					//fScreenRect = Rectangle.Empty;
					var ptTail = PositionBalloonTail(ref ptTopLeft, ref ptBottomRight);

					// get window position
					var ptAnchor = AnchorPoint;
					var ptOffs = new Point(ptAnchor.X - ptTail[1].X, ptAnchor.Y - ptTail[1].Y);

					// adjust position so all is visible
					var rectScreen = AnchorScreenBounds;
					var nAdjustX = 0;
					var nAdjustY = 0;

					if (ptOffs.X < rectScreen.Left)
					{
						nAdjustX = rectScreen.Left - ptOffs.X;
					}
					else if (ptOffs.X + fBalloonSize.Width >= rectScreen.Right)
					{
						nAdjustX = rectScreen.Right - (ptOffs.X + fBalloonSize.Width);
					}
					if (ptOffs.Y + nTIP_TAIL < rectScreen.Top)
					{
						nAdjustY = rectScreen.Top - (ptOffs.Y + nTIP_TAIL);
					}
					else if (ptOffs.Y + fBalloonSize.Height - nTIP_TAIL >= rectScreen.Bottom)
					{
						nAdjustY = rectScreen.Bottom - (ptOffs.Y + fBalloonSize.Height - nTIP_TAIL);
					}

					// reposition tail
					// uncomment two commented lines below to move entire tail
					// instead of just anchor point

					//ptTail[0].x -= nAdjustX;
					ptTail[1].X -= nAdjustX;
					//ptTail[2].x -= nAdjustX;
					ptOffs.X += nAdjustX;
					ptOffs.Y += nAdjustY;

					Location = ptOffs;
					PlacedLocation = Location;

					if (PreviousTail == null)
					{
						PreviousTail = new Point[3];
					}

					if (ForceNewRegion || PreviousTail[1] != ptTail[1] || PreviousTail[0] != ptTail[0] || PreviousTail[2] != ptTail[2]
						|| ptTopLeft != PreviousTopLeftPoint || ptBottomRight != PreviousBottomRightPoint)
					{
						PreviousTail[0] = ptTail[0];
						PreviousTail[1] = ptTail[1];
						PreviousTail[2] = ptTail[2];
						PreviousTopLeftPoint = ptTopLeft;
						PreviousBottomRightPoint = ptBottomRight;

						// apply region
						var tailRegionHRGN = UnsafeNativeMethods.CreatePolygonRgn(ptTail, 3, 1);
						var roundOutlineHRGN = UnsafeNativeMethods.CreateRoundRectRgn(ptTopLeft.X, ptTopLeft.Y, ptBottomRight.X, ptBottomRight.Y, nTIP_MARGIN * 3, nTIP_MARGIN * 3);
						var combineResult = UnsafeNativeMethods.CombineRgn(roundOutlineHRGN, roundOutlineHRGN, tailRegionHRGN, 2);
						Region = Region.FromHrgn(roundOutlineHRGN);

						SafeNativeMethods.DeleteObject(new HandleRef(this, tailRegionHRGN));
						SafeNativeMethods.DeleteObject(new HandleRef(this, roundOutlineHRGN));

						ForceNewRegion = false;
					}
					base.Update();
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Globals.Message.ShowDeveloperException(e);
				Region = null;
			}
			PositionDirty = false;
		}

#if DEBUG
		public virtual // for mock
#endif
		Rectangle AnchorScreenBounds
		{
			get { return CachedScreenInfo.Instance.FromPoint(AnchorPoint); }
		}

		BalloonQuadrant GetQuadrant(Point point)
		{
			var desktop = CachedScreenInfo.Instance.FromPoint(point);

			if (point.Y < desktop.Top + desktop.Height / 2)
			{
				return (point.X < desktop.Left + desktop.Width / 2) ? BalloonQuadrant.TopLeft : BalloonQuadrant.TopRight;
			}
			else
			{
				return (point.X < desktop.Left + desktop.Width / 2) ? BalloonQuadrant.BottomLeft : BalloonQuadrant.BottomRight;
			}
		}

		#endregion

		#region Painting Balloon

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
#if DEBUG
				if (Globals.IsTest)
				{
					e.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
				}
#endif
				if (Descriptor != null && (Visible || (AlphaBlendingEnabled && WindowAlpha > MinAlpha)))
				{
					captionedComponents.Clear();

					if (Region == null)
					{
						UpdateSizeAndPosition();
					}

					DrawFrame(e);
					var offsetY = 0;
					switch (CurrentQuadrant)
					{
						case BalloonQuadrant.TopLeft:
						case BalloonQuadrant.TopRight:
							offsetY = ControlDpiScalingHelper.ScaleToCurrentDpiY(nTIP_TAIL);
							break;
					}
					DrawHeader(e, offsetY);
					offsetY += HeaderSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(nTIP_MARGIN);
					DrawDescription(e, offsetY);
					offsetY += DescriptionSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(nTIP_MARGIN);
					offsetY += DrawContent(e, offsetY);
					base.OnPaint(e);

					if (AlphaBlendingEnabled && ShownButNotPainted)
					{
						MakeOpaque();
						ShownButNotPainted = false;
					}
				}
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
		}

		void DrawFrame(PaintEventArgs e)
		{
			using (var windowRegion = new Region())
			{
				e.Graphics.FillRegion(SystemBrushes.Info, windowRegion);
			}
			var hBrushHL = UnsafeNativeMethods.CreateSolidBrush(Color.Black.ToArgb());
			var regionPointer = Region.GetHrgn(e.Graphics);
			var hDC = IntPtr.Zero;
			try
			{
				hDC = e.Graphics.GetHdc();
				UnsafeNativeMethods.FrameRgn(new HandleRef(this, hDC), new HandleRef(Region, regionPointer), new HandleRef(this, hBrushHL), 1, 1);
			}
			finally
			{
				if (hDC != IntPtr.Zero)
				{ e.Graphics.ReleaseHdc(hDC); }
				SafeNativeMethods.DeleteObject(new HandleRef(this, hBrushHL));
				SafeNativeMethods.DeleteObject(new HandleRef(Region, regionPointer));
			}
		}

		protected virtual void DrawHeader(PaintEventArgs e, int offsetY)
		{
			// Paint Icon in Upper Right Corner
			//			e.Graphics.DrawIcon(Icon, new Rectangle(nTIP_MARGIN, nTIP_MARGIN + OffsetY, 16, 16));

			// Paint Caption
			using (var captionStringFormat = new StringFormat(StringFormatFlags.NoWrap))
			{
				captionStringFormat.Alignment = StringAlignment.Near;
				float captionStringX = /*16  + */ nTIP_MARGIN /* * 2*/;
				float captionStringWidth = fBalloonSize.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX((int)captionStringX + nTIP_MARGIN);
				var captionLayoutRect = new RectangleF(captionStringX, ControlDpiScalingHelper.ScaleToCurrentDpiY(nTIP_MARGIN) + offsetY, captionStringWidth, ControlDpiScalingHelper.ScaleToCurrentDpiY(16));
				TextRendererHelper.DrawText(e.Graphics, descriptor.Caption, CaptionFont, captionLayoutRect, SystemBrushes.WindowText, captionStringFormat);
				if (!string.IsNullOrEmpty(descriptor.Caption))
				{
					captionedComponents.Add(new CaptionedComponent(Rectangle.Ceiling(captionLayoutRect), descriptor.Caption));
				}
			}
		}

		protected virtual void DrawDescription(PaintEventArgs e, int offsetY)
		{
			using (var descriptionStringFormat = new StringFormat())
			{
				var stringWidth = DescriptionSize.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN);
				var descriptionStringRectangle = new RectangleF(ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN), offsetY, stringWidth, DescriptionSize.Height);
				TextRendererHelper.DrawText(e.Graphics, descriptor.Description, DescriptionFont, descriptionStringRectangle, SystemBrushes.WindowText, descriptionStringFormat);
				if (!string.IsNullOrEmpty(descriptor.Description))
				{
					captionedComponents.Add(new CaptionedComponent(Rectangle.Ceiling(descriptionStringRectangle), descriptor.Description));
				}
			}
		}

		int DrawContent(PaintEventArgs e, int offsetY)
		{
			if (descriptor.UniqueNotifications(currentMessageOffset) != null)
			{
				DrawMessages(descriptor.UniqueNotifications(currentMessageOffset), e, ref offsetY);
			}
			return offsetY;
		}

		void DrawMessages(IEnumerable<INotification> notifications, PaintEventArgs e, ref int offsetY)
		{
			if (notifications != null)
			{
				foreach (var notification in notifications.OrderBy(item => item.Type.Severity))
				{
					DrawMessageLine(
						notification.Message,
						NotificationIconScheme.Instance.GetIcon(notification.Type),
						BrushProvider.FromColor(GUI.Notifications.NotificationColorScheme.GetFontColor(notification.Type)),
						e, ref offsetY);
				}
			}
		}

		Font ContentFont
		{
			get { return Enterprise.ZArchitecture.Core.OFont.GetFont(); }
		}

		void DrawMessageLine(string message, Icon icon, Brush messageBrush, PaintEventArgs e, ref int offsetY)
		{
			var stringSize = CalcStringSize(e.Graphics, message, ContentFont, icon.Width);
			var iconRect = ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN), offsetY, icon.Size.Width, icon.Size.Height, false);
			e.Graphics.DrawIcon(icon, iconRect);
			var stringLayout = new RectangleF(ControlDpiScalingHelper.ScaleToCurrentDpiX((nTIP_MARGIN * 2)) + icon.Width, offsetY, stringSize.Width, stringSize.Height);
			TextRendererHelper.DrawText(e.Graphics, message, ContentFont, stringLayout, messageBrush, new StringFormat());
			captionedComponents.Add(new CaptionedComponent(Rectangle.Ceiling(stringLayout), message));
			offsetY += (int)Math.Max(icon.Height, stringLayout.Height);
		}

		[return: DpiState(DpiState.ScaledVariant)]
		Size CalcContentSize(Graphics g)
		{
			var result = Size.Empty;
			result = CalcNotificationSize(g, descriptor.UniqueNotifications(currentMessageOffset), result);
			return result;
		}

		Size CalcNotificationSize(Graphics g, IEnumerable<INotification> notifications, Size originalSize)
		{
			var result = originalSize;
			if (notifications != null)
			{
				foreach (var notification in notifications)
				{
					var icon = NotificationIconScheme.Instance.GetIcon(notification.Type);
					result = IncrementSizeByMessageAndIcon(g, notification.Message, icon, result);
				}
			}
			return result;
		}

		Size IncrementSizeByMessageAndIcon(Graphics g, string message, Icon icon, Size currentSize)
		{
			var result = currentSize;

			var stringSize = CalcStringSize(g, message, ContentFont, icon.Width);
			ControlDpiScalingHelper.SetHeight(ref stringSize, Math.Max(stringSize.Height, icon.Width), false);
			ControlDpiScalingHelper.SetWidth(ref result, Math.Max(stringSize.Width + icon.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(nTIP_MARGIN * 3), result.Width), false);
			ControlDpiScalingHelper.SetHeight(ref result, result.Height + stringSize.Height, false);
			return result;
		}

		const int nTIP_TAIL = 20;
		const int nTIP_MARGIN = 8;

		Size fBalloonSize;
		Size HeaderSize;
		Size DescriptionSize;
		Size ContentSize;

		Point PlacedLocation = Point.Empty;
		Point[] PreviousTail;
		Point PreviousTopLeftPoint = Point.Empty;
		Point PreviousBottomRightPoint = Point.Empty;

#if DEBUG
		protected Point fAnchorPoint;
		protected Control fAnchorParent;
		public byte WindowAlphaExposed => WindowAlpha;
#endif

		protected Font CaptionFont
		{
			get { return OFont.GetFontBold(); }
		}

		protected Font DescriptionFont
		{
			get { return OFont.GetFont(); }
		}

		#endregion

		#region Alpha Blending

		void MakeTransparent()
		{
			WindowAlpha = MinAlpha;
		}

		void MakeOpaque()
		{
			WindowAlpha = MaxAlpha;
		}

		byte WindowAlpha
		{
			get
			{
				return fWindowAlpha;
			}
			set
			{
				fWindowAlpha = value;
				SetWindowAlpha(fWindowAlpha);
			}
		}
		byte fWindowAlpha = MaxAlpha;

		void SetWindowAlpha(byte newAlpha)
		{
			if (!IsDisposed)
			{
				try
				{
					UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this, Handle), 0, newAlpha, NativeMethods.LWA_ALPHA);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		void StartFadeControlTimer()
		{
			if (FadeControlTimer == null)
			{
				FadeControlTimer = new Timer();
				FadeControlTimer.Interval = 40;
				FadeControlTimer.Tick += new EventHandler(FadeControlTick);
			}
			StartTickCount = SafeNativeMethods.GetTickCount();
			LastTickCount = StartTickCount;
			FadeControlTimer.Start();
		}

		void FadeControlTick(object sender, EventArgs e)
		{
			try
			{
				var currentTickCount = SafeNativeMethods.GetTickCount();
				float ellapsedTickCount = currentTickCount - LastTickCount;
				var deltaAlpha = (ellapsedTickCount / AnimationLength) * MaxAlpha;
				int alpha = WindowAlpha;

				if (alpha >= MinAlpha) // hiding
				{
					alpha -= (int)deltaAlpha;
					if (alpha <= MinAlpha)
					{
						WindowAlpha = MinAlpha;
						FadeControlTimer.Stop();
						HideQuickly();
					}
					else
					{
						WindowAlpha = (byte)alpha;
					}
				}
				LastTickCount = currentTickCount;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				FadeControlTimer.Stop();
				ErrorReporter.ReportOnce("BaloonWindow.FadeControlTick", "Balloon Error on Balloon Form Fading", ex);
			}
		}

		bool AlphaBlendingEnabled;
		Timer FadeControlTimer;
		int StartTickCount;
		int LastTickCount;

		const byte MaxAlpha = 255;
		const byte MinAlpha = 0;
		const int AnimationLength = 200;  // ms

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (translationFeedbackManager != null)
				{
					translationFeedbackManager.Dispose();
					translationFeedbackManager = null;
				}
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}
				if (FadeControlTimer != null)
				{
					FadeControlTimer.Stop();
					FadeControlTimer.Dispose();
				}
				if (Region != null)
				{
					Region.Dispose();
				}

				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region ICaptionedComponents

		public object GetCaptionedComponentAt(Point p)
		{
			foreach (var captionedComponent in captionedComponents)
			{
				if (captionedComponent.region.Contains(p))
				{
					return captionedComponent;
				}
			}
			return null;
		}

		public Rectangle GetCaptionedComponentRect(object component)
		{
			return ((CaptionedComponent)component).region;
		}

		public object GetCaptionedComponentData(object component)
		{
			var data = ((CaptionedComponent)component).caption;
			var colonBreak = data.IndexOf(": ");
			if (colonBreak > -1)
			{
				data = data.Substring(colonBreak + 2);
			}
			return data;
		}

		struct CaptionedComponent
		{
			public CaptionedComponent(Rectangle region, string caption)
			{
				this.region = region;
				this.caption = caption;
			}

			public readonly Rectangle region;
			public readonly string caption;
		}

		readonly List<CaptionedComponent> captionedComponents = new List<CaptionedComponent>();

		internal TranslationFeedbackManager translationFeedbackManager;
		readonly internal DevInfoPopupManager devInfoPopupManager;
		#endregion
	}
}
