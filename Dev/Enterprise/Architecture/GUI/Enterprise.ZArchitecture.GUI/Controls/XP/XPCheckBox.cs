using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// Represents a Windows XP CheckBox control.
	/// </summary>
	[ToolboxItem(false)]
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class XPCheckBox : CheckBox, IGridControl
	{
		public XPCheckBox()
			: base()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);

			ButtonStyle = Style.XpStyle;
			Text = "";
			#if !WINZOR
			KeyPress += new KeyPressEventHandler(HandleKeyPress);
			#endif
			ThreeState = false;
			FlatStyle = FlatStyle.Standard;
			CheckAlign = ContentAlignment.MiddleCenter;
			devInfoPopupManager = new DevInfoPopupManager(this);
		}

		readonly internal DevInfoPopupManager devInfoPopupManager;

		#region Properties

		/// <summary>
		/// Being set by the GridColumn to prevent clicking
		/// </summary>
		public bool IsReadOnlyGrid { get; set; }

		public string CheckedValue
		{
			get { return Checked ? Constants.BooleanTrueString : Constants.BooleanFalseString; }
			set { Checked = (value == Constants.BooleanTrueString); }
		}

		/// <summary>
		/// Gets or sets the style of the button.
		/// </summary>
		public Style ButtonStyle
		{
			get { return buttonStyle; }
			set
			{
				buttonStyle = value;
				base.OnStyleChanged(EventArgs.Empty);
				base.Invalidate();
			}
		}
		Style buttonStyle;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public XPButtonState XPButtonState { get; set; }

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}
				DisposableLeakListener.Instance.UnRegisterDisposable(this);

				#if !WINZOR

				ResetThemeHandle();

				#endif
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Draw

		#if !WINZOR

		/// <summary>
		/// Called by the GridColumn to get the image of XP-style checkbox
		/// </summary>
		/// <param name="g">Graphics</param>
		/// <param name="bounds">Rectangle to fill</param>
		internal void Draw(Graphics g, Rectangle bounds)
		{
			OnPaint(new PaintEventArgs(g, bounds));
		}

		/// <summary>
		/// Draws a Windows XP style button.
		/// </summary>
		/// <param name="hDC">A managed pointer containing the handle to a control's device context.</param>
		/// <param name="bounds">A Rectangle structure containing the bounds of the control to draw on.</param>
		protected virtual int DrawXpButton(IntPtr hDC, Rectangle bounds)
		{
			var rect = new RECT(bounds);
			var result = 0;

			if (!base.Enabled)
			{
				result = NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, (int)XPButtonState.Disabled, ref rect, ref rect);
			}
			else
			{
				if (base.IsDefault)
				{
					result = NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, (int)XPButtonState.Defaulted, ref rect, ref rect);
				}

				int stateId;

				switch (XPButtonState)
				{
					case XPButtonState.Hot:
						stateId = Checked ? 6 : (int)XPButtonState.Hot;
						result = NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, stateId, ref rect, ref rect);
						break;

					case XPButtonState.Pressed:
						result = NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, (int)XPButtonState.Pressed, ref rect, ref rect);
						break;

					default:
						if (!base.IsDefault)
						{
							stateId = (Checked) ? 5 : (int)XPButtonState.Normal;
							result = NativeMethods.DrawThemeBackground(hTheme, hDC, buttonType, stateId, ref rect, ref rect);
						}

						break;
				}
			}

			return result;
		}

		readonly int buttonType = (int)ButtonType.CheckBox;
		const int INVALID_HANDLE_ErrorCode = -2147024890;//Hex: 80070006 

#if DEBUG
		internal
#endif
		IntPtr hTheme = IntPtr.Zero;

		internal void ResetThemeHandle()
		{
			if (hTheme != IntPtr.Zero)
			{
				var code = NativeMethods.CloseThemeData(hTheme);
				hTheme = IntPtr.Zero;

				if (code != 0 && code != INVALID_HANDLE_ErrorCode)   //exclude INVALID_HANDLE error.
				{
					var error = new Win32Exception(code,"method NativeMethods.CloseThemeData(IntPtr) inside XPCheckBox.cs has returned an error.");
					ErrorReporter.ReportOnce("", error);
				}
			}
		}

		bool paintHasFailedBefore;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		protected override void OnPaint(PaintEventArgs paintArgs)
		{
			if (paintHasFailedBefore)
			{
				PaintException(paintArgs);
				return;
			}

			var callBaseOnPaint = false;

			try
			{
				if (buttonStyle == Style.XpStyle && !this.IsDisposed)
				{
					if (hTheme == IntPtr.Zero && NativeMethods.IsThemeActive() == 1)
					{
						hTheme = NativeMethods.OpenThemeData(Handle, "Button");
					}
					if (hTheme != IntPtr.Zero)
					{
						var graphics = paintArgs.Graphics;
						var bounds = ClientRectangle;
						graphics.FillRectangle(BrushProvider.FromColor(BackColor), ClientRectangle);

						var hDC = graphics.GetHdc();
						try
						{
							var result = DrawXpButton(hDC, bounds);
							if (result != 0)
							{
								ResetThemeHandle();
								if (NativeMethods.IsThemeActive() == 1)
								{
									hTheme = NativeMethods.OpenThemeData(Handle, "Button");
								}
								if (hTheme != IntPtr.Zero)
								{
									DrawXpButton(hDC, bounds);
								}
							}
						}
						finally
						{
							graphics.ReleaseHdc(hDC);
						}
					}
					else
					{
						callBaseOnPaint = true;
					}
				}
				else
				{
					callBaseOnPaint = true;
				}

				if (callBaseOnPaint)
				{
					base.OnPaint(paintArgs);
				}
			}
			catch (ArgumentException)
			{
				//Parameter is not valid. at System.Drawing.Graphics.GetHdc() . Possibly caused by OOM/GDI object leak/faulty .NET installation/other?
				paintHasFailedBefore = true;
				PaintException(paintArgs);
			}
			catch (System.Runtime.InteropServices.ExternalException)
			{
				paintHasFailedBefore = true;
				PaintException(paintArgs);
			}
			catch (InvalidOperationException)
			{
				//ObjectDisposedException is subclass of InvalidOperationException and also goes here
				paintHasFailedBefore = true;
				PaintException(paintArgs);
			}
		}

		//copied from System.Windows.Forms.Control.PaintException and modified
		void PaintException(PaintEventArgs e)
		{
			try
			{
				using (var pen = new Pen(Color.Red, 2.0f))
				{
					var clientRectangle = ClientRectangle;
					var rect = clientRectangle;
					ControlDpiScalingHelper.SetX(ref rect, rect.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
					ControlDpiScalingHelper.SetY(ref rect, rect.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
					ControlDpiScalingHelper.SetWidth(ref rect, rect.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
					ControlDpiScalingHelper.SetHeight(ref rect, rect.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
					e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
					rect.Inflate(-1, -1);
					e.Graphics.FillRectangle(Brushes.White, rect);
					e.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Top, clientRectangle.Right, clientRectangle.Bottom);
					e.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Bottom, clientRectangle.Right, clientRectangle.Top);
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException()) { throw; }
			}
		}

		#endif

		#endregion

		#region User Events

		#if !WINZOR

		protected override void OnMouseEnter(EventArgs e)
		{
			if (!IsReadOnlyGrid)
			{
				base.OnMouseEnter(e);
			}

			XPButtonState = XPButtonState.Hot;
			base.Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			XPButtonState = XPButtonState.Normal;
			base.Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			if (!IsReadOnlyGrid)
			{
				base.OnMouseDown(mevent);
				XPButtonState = XPButtonState.Pressed;
			}

			base.Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			if (!IsReadOnlyGrid)
			{
				base.OnMouseUp(mevent);
				XPButtonState = XPButtonState.Normal;
			}

			base.Invalidate();
		}

		protected void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 32 && !IsReadOnlyGrid)
			{
				Checked = !Checked;
			}
		}

		protected override void OnEnter(EventArgs e)
		{
			if (!IsReadOnlyGrid)
			{
				BackColor = EnterpriseFormLookStrategy.SelectedControlColor;
			}
			base.OnEnter(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			if (!IsReadOnlyGrid)
			{
				BackColor = DefaultBackColor;
			}
			base.OnLeave(e);
		}

		#endif

		#endregion

		#region Windows Message

		#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			// ignore keyup to avoid problem with tabbing
			const int WM_KEYUP = 0x101;

			if (m.Msg != WM_KEYUP)
			{
				base.WndProc(ref m);

				if (m.Msg == WindowsMessage.WM_THEMECHANGED)
				{
					ResetThemeHandle();

					base.Invalidate();
				}
			}
		}

		#endif

		#endregion

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return 0; }
			set { }
		}

		int IGridControl.SelectionLength
		{
			get { return 0; }
			set { }
		}

		int IGridControl.ButtonWidth
		{
			get { return 0; }
		}

		int IGridControl.MaxLength
		{
			get { return 0; }
			set { }
		}

		void IGridControl.ActivateEditControl()
		{
			Focus();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return keyData == Keys.Space;
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return false; }
		}

		string IGridControl.Text
		{
			get { return CheckedValue; }
			set { CheckedValue = value; }
		}

		#endregion
	}
}
