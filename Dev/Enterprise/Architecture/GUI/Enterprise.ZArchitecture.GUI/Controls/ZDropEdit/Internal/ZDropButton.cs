using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CargoWise.Common;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI.Internal
{
#if DEBUG
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
#endif
	[ToolboxItem(false)]
	public partial class ZDropButton : ZUserControl, IDisposeStackProvider
	{
		public ZDropButton()
		{
			this.CaptionRenderingEnabled = true;
			TabStop = false;
			UserEventTracker.Instance.AddUserEventToControl(this);
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			RegisterHotkeys();
		}

		#region Paint and Layout

		internal void UpdateSelectedItem()
		{
			if (IsDroppedDown)
			{
				DropDown.RefreshList();
				DropDown.SelectItemFromCode();
			}
		}

#if !WINZOR

		protected bool HasXPSupport
		{
			get { return XpUtils.IsAppUsingXpTheme && XpUtils.IsWindowsXpOrGreater && XpUtils.IsWindowsUsingXpTheme; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				OnPaintCore(e);
				base.OnPaint(new ZPaintEventArgs(e.Graphics, GetActualRectangleForNotificationIcon()));
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
		}

		protected virtual void OnPaintCore(PaintEventArgs e)
		{
			var buttonRectangle = ButtonRectangle;

			if (HasXPSupport && ComboBoxRenderer.IsSupported)
			{
				if (IsOnGrid)
				{
					ControlDpiScalingHelper.SetHeight(ref buttonRectangle, buttonRectangle.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				}
				var textState = !ReadOnly ? TextBoxState.Normal : TextBoxState.Disabled;
				ZTextBoxRenderer.DrawTextBox(e.Graphics, ClientRectangle, textState, ShouldDrawBorder, ParentDropEdit.CodeBox.BackColor);
				ZComboBoxRenderer.DrawDropDownButton(e.Graphics, buttonRectangle, ReadOnly ? ComboBoxState.Disabled : comboState);
			}
			else
			{
				if (ReadOnly)
				{
					e.Graphics.FillRectangle(SystemBrushes.Control, ClientRectangle);
				}
				else
				{
					e.Graphics.FillRectangle(SystemBrushes.Window, ClientRectangle);
					var buttonState = (comboState == ComboBoxState.Pressed) ? ButtonState.Pushed : ButtonState.Normal;
					if (buttonRectangle.Width > 0 && buttonRectangle.Height > 0)
					{
						try
						{
							ControlPaint.DrawComboButton(e.Graphics, buttonRectangle, buttonState);
						}
						catch (ArgumentException) { } // rare exception
					}
				}

				if (ShouldDrawBorder)
				{
					ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
				}
			}
		}

#endif

		protected virtual Point ButtonPoint
		{
			// The pixel adjustments are independent of font scaling, and serve to provide an effect of depth on the button
			get { return ControlDpiScalingHelper.NewScaledPoint(ClientRectangle.Right - ZGUISystemInformation.VerticalScrollBarWidth - 1, ClientRectangle.Top + 1, false); }
		}

		protected virtual Size ButtonSize
		{
			get
			{
				// The  2 pixel adjustment is independent of font scaling, to account for the top and bottom lines of the border. Having a 2 pixel shorter button will give an effect of depth
				return ControlDpiScalingHelper.NewScaledSize(ZGUISystemInformation.VerticalScrollBarWidth, ClientRectangle.Height - 2, false);
			}
		}

		protected virtual bool ShouldDrawBorder
		{
			get { return true; }
		}

		Rectangle ButtonRectangle
		{
			get
			{
				var point = ButtonPoint;
				return ControlDpiScalingHelper.NewScaledRectangle(point.X, point.Y, ButtonSize.Width, ButtonSize.Height, false);
			}
		}

		internal Rectangle GetActualRectangleForNotificationIcon()
		{
			return ControlDpiScalingHelper.NewScaledRectangle(ButtonRectangle.X + 1, ButtonRectangle.Y + 1, ButtonRectangle.Width, ButtonRectangle.Height, false);
		}

		#region Theme Data
#if !WINZOR

		protected IntPtr ButtonThemeData
		{
			get
			{
				if (buttonThemeData == IntPtr.Zero)
				{
					buttonThemeData = XpThemeAPI.OpenThemeData(this.Handle, "ComboBox");
				}

				return buttonThemeData;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		protected IntPtr EditThemeData
		{
			get
			{
				if (editThemeData == IntPtr.Zero)
				{
					editThemeData = XpThemeAPI.OpenThemeData(this.Handle, "Edit");
				}
				return editThemeData;
			}
		}

		IntPtr buttonThemeData;
		IntPtr editThemeData;

#endif

		#endregion

		#endregion

		protected override void Dispose(bool isNotFinalizing)
		{
			try
			{
				if (DropDown != null && !DropDown.IsDisposed)
				{
					DropDown.Close();
					DropDown.Dispose();
					DropDown = null;
				}

#if !WINZOR
				if (buttonThemeData != IntPtr.Zero) // so the designer doesn't need to load a dll on finalize
				{
					CloseThemeData();
				}
#endif

				if (TrackDisposedAccess)
				{
					disposeStack = new StackTrace();
					disposeControlPath = ControlDescription.GetControlPath(this);
				}
			}
			finally
			{
				base.Dispose(isNotFinalizing);
			}
		}

		#region Handle

		/// <summary>
		/// Overrides base CreateHandle method with adding information about dispose stack.
		/// </summary>
		protected override void CreateHandle()
		{
			try
			{
				if (!IsDisposed)
				{
					base.CreateHandle();
				}
			}
			catch (ObjectDisposedException ex)
			{
				throw new ObjectDisposedException(this.BuildDisposeInformation(currentControlInformation), ex);
			}
		}

		#endregion

		#region Disposed access tracking

		[DefaultValue(false)]
		[Browsable(false)]
		public bool TrackDisposedAccess { get; set; }

		public StackTrace DisposeStack => disposeStack;
		StackTrace disposeStack;

		public string DisposeControlPath => disposeControlPath;
		string disposeControlPath;

#pragma warning disable IDE0044 //var currentControlInformation is getting modified, conflicting readonly property
		string currentControlInformation = String.Empty;
#pragma warning restore IDE0044

		#endregion

		#region AllowDrop

		public override bool AllowDrop => false; // Can you *really* drag-drop into a button?

		#endregion

		#region DropDown Visibility

		protected internal virtual void ShowDropDown(bool lButtonDown)
		{
			if (!ParentDropEdit.ReadOnly)
			{
				ParentDropEdit.InvalidateList(); // ensure the list is updated in case user did not leave control but the list has changed

				if (DropDown != null && (DropDown.Visible || DropDown.Enabled))
				{
					ErrorReporter.ReportOnce("ZDropEdit.DropForm.Visible", "Needed to hide (Visible = false) DropForm from " + ParentDropEdit.Name + ".");
				}

				if (DropDown != null && !DropDown.IsDisposed)
				{
					DropDown.Dispose();
				}

				DropDown = NewDropForm();
				DropDown.ShowDropDown(GetDropDownLocation(DropDown), !lButtonDown);
			}
		}

		protected virtual ZDropForm NewDropForm()
		{
			return new ZDropForm(ParentDropEdit);
		}

		protected Point GetDropDownLocation(ZDropForm dropDown)
		{
			var result = PointToScreen(ControlDpiScalingHelper.NewScaledPoint(Left, Bottom, false));

			var dropDownRightEdge = result.X + dropDown.Width;
			var horizontalStateRight = CachedScreenInfo.Instance.GetHorizontalState(ControlDpiScalingHelper.NewScaledPoint(dropDownRightEdge, result.Y, false));
			if (horizontalStateRight == HorizontalState.OffScreenRight)
			{
				var screenControlIsOn = CachedScreenInfo.Instance.FromControl(this);
				ControlDpiScalingHelper.SetX(ref result, screenControlIsOn.Right - dropDown.Width, false);
			}

			var verticalStateBottom = CachedScreenInfo.Instance.GetVerticalState(result, dropDown.Height);
			if (verticalStateBottom == VerticalState.OffScreenBottom)
			{
				var pointOnScreen = PointToScreen(ControlDpiScalingHelper.NewScaledPoint(Left, Top, false));
				ControlDpiScalingHelper.SetY(ref result, pointOnScreen.Y - dropDown.Height, false); // place on top of the control
			}
			if (verticalStateBottom == VerticalState.OffScreenTop)
			{
				ControlDpiScalingHelper.SetY(ref result, CachedScreenInfo.Instance.getCurrentScreen(result).Top, false);
			}
			return result;
		}

		protected internal virtual void HideDropDown()
		{
			DropDown?.HideDropDown();
		}

		internal bool IsDroppedDown
		{
			get { return DropDown?.Visible ?? false; }
		}

		#endregion

		#region Key Handling

		void RegisterHotkeys()
		{
			var showDropdown = Res.GetString("08cff2fd-13d2-4005-9f76-3201746ee6ff", "Show drop menu");
			Hotkeys.RegisterHotKey(Keys.F4, HandleShowDropDown, showDropdown);
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Down, HandleShowDropDown, showDropdown);

			Hotkeys.RegisterHotKey(Keys.Escape, HandleHideDropDown, Res.GetString("c08dc0ed-ef3e-40cc-8735-edc48320035b", "Hide drop menu"));
		}

		bool HandleHideDropDown(object sender, Keys key)
		{
			var wasHandled = IsDroppedDown;
			HideDropDown();
			return wasHandled;
		}

		internal void HandleCommandKey(KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Down:
					HandleDown(e);
					break;

				case Keys.Up:
					HandleNavigate(e);
					break;

				case Keys.PageUp:
					HandleNavigate(e);
					break;

				case Keys.PageDown:
					HandleNavigate(e);
					break;

				case Keys.Tab:
				case Keys.Enter:
					e.Handled = AcceptSelection();
					break;
				default:
					e.Handled = Hotkeys.ProcessCmdKey(this, e.KeyData);
					break;
			}
		}

		void HandleShowDropDown()
		{
			if (IsDroppedDown)
			{
				HideDropDown();
			}
			else
			{
				ShowDropDown(MouseButtons.HasFlag(MouseButtons.Left));
			}
		}

		void HandleDown(KeyEventArgs e)
		{
			if (IsDroppedDown)
			{
				DropDown.HandleCommandKey(e.KeyData);
				UpdateSelectionFromDropItem();
			}
			else
			{
				ShowDropDown(MouseButtons.HasFlag(MouseButtons.Left));
			}

			e.Handled = true;
		}

		void HandleNavigate(KeyEventArgs e)
		{
			if (IsDroppedDown)
			{
				DropDown.HandleCommandKey(e.KeyData);
				UpdateSelectionFromDropItem();
			}

			e.Handled = true;
		}

		#endregion

		#region Accept or Change Selection

		protected bool AcceptSelection()
		{
			if (IsDroppedDown)
			{
				UpdateSelectionFromDropItem();
				HideDropDown();
				ParentDropEdit.CommitBoundValue();

				return true;
			}

			return false;
		}

		void UpdateSelectionFromDropItem()
		{
			if (IsDroppedDown && DropDown.IsItemSelectable(DropDown.SelectedItem))
			{
				ParentDropEdit.OnItemSelected(DropDown.SelectedItem, false);
			}
		}

		#endregion

		#region Mouse Events

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e); // this will call OnEnter() which will also attempt to pull the list

			if (!ReadOnly
#if !WINZOR
				&& ButtonRectangle.Contains(e.X, e.Y)
#endif
			)
			{
#if !WINZOR
				if (e.Button == MouseButtons.Left && e.Clicks == 1)
#else
				if (e.Button == MouseButtons.Left && !IsDroppedDown)
#endif
				{
					comboState = ComboBoxState.Pressed;
					if (!IsDroppedDown)
					{
						ShowDropDown(true);
						ParentDropEdit.PreventOnEnterFromPullingList = true;
						base.Invalidate();
					}
				}
				else if (IsDroppedDown)
				{
					HideDropDown();
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (!ReadOnly && ButtonRectangle.Contains(e.X, e.Y))
			{
				comboState = ComboBoxState.Hot;
				base.Invalidate();
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			comboState = ComboBoxState.Normal;
			if (!ReadOnly)
			{
				Invalidate();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!ReadOnly)
			{
				if (ButtonRectangle.Contains(e.X, e.Y))
				{
					if (comboState != ComboBoxState.Pressed)
					{
						comboState = ComboBoxState.Hot;
						Invalidate();
					}
				}
				else
				{
					comboState = ComboBoxState.Normal;
					Invalidate();
				}
			}
		}

		ComboBoxState comboState;

		#endregion

		#region Implementation

		protected internal ZDropForm DropDown { get; private set; }

#if DEBUG
		public ZDropForm DropDown_Exposed
		{
			get { return DropDown; }
		}
#endif

		protected ZDropEdit ParentDropEdit
		{
			get { return (ZDropEdit)Parent; }
		}

		protected bool ReadOnly
		{
			get { return ParentDropEdit == null || ParentDropEdit.ReadOnly; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		protected override void OnEnter(EventArgs e)
		{
			try
			{
				if (!IsDisposed && IsHandleCreated && Parent != null && !Parent.IsDisposed && Parent.IsHandleCreated)
				{
					var editParent = Parent as ZDropEdit;
					if (editParent.CodeBox != null && !editParent.CodeBox.IsDisposed && editParent.CodeBox.IsHandleCreated)
					{
						editParent.ActiveControl = editParent.CodeBox;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ExceptionInZDropOnEnterOrOnLeave" + Parent?.Parent?.Name,
					string.Format(CultureInfo.InvariantCulture, "Exception occurred in ZDropButton in OnEnter or OnLeave. DisposeControlPath = {0}\r\nDisposeStack = {1}\r\n", DisposeControlPath, DisposeStack?.ToString() ?? "Empty")
					, ex);
				throw;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		protected override void OnLeave(EventArgs e)
		{
			try
			{
				if (!IsDisposed && IsHandleCreated && Parent != null && !Parent.IsDisposed && Parent.IsHandleCreated)
				{
					base.OnLeave(e);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ExceptionInZDropOnEnterOrOnLeave" + Parent?.Parent?.Name,
					string.Format(CultureInfo.InvariantCulture, "Exception occurred in ZDropButton in OnEnter or OnLeave. DisposeControlPath = {0}\r\nDisposeStack = {1}\r\n", DisposeControlPath, DisposeStack?.ToString() ?? "Empty")
					, ex);
				throw;
			}
		}

#if !WINZOR

		bool fullyPainted;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected override void WndProc(ref Message m)
		{
			try
			{
				if (m.Msg != WindowsMessage.WM_ERASEBKGND)
				{
					base.WndProc(ref m);
				}
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }

			if (m.Msg == WindowsMessage.WM_THEMECHANGED)
			{
				CloseThemeData();
				Invalidate();
			}

			if (m.Msg == WindowsMessage.WM_PAINT && !fullyPainted)
			{
				fullyPainted = true;
				var form = FindForm();
				var parentFilterStrip = GetParentFilterStrip(this);
				#region SuppressResourceStringsCheckRegion
				currentControlInformation = FormattableString.Invariant($@"Name: {Name}
ControlPath: {ControlDescription.GetControlPath(this)}
ParentDropEdit: {ParentDropEdit?.Name}, with text ""{ParentDropEdit?.Text}""
FilterStrip(If exists): {parentFilterStrip?.CurrentDataItem?.FilterDescription}
Form: {form?.Text}, with typeof {form?.GetType().FullName}");
				#endregion
			}
		}

		void CloseThemeData()
		{
			if (buttonThemeData != IntPtr.Zero)
			{
				XpThemeAPI.NativeMethods.CloseThemeData(buttonThemeData);
				buttonThemeData = IntPtr.Zero;
			}

			if (editThemeData != IntPtr.Zero)
			{
				XpThemeAPI.NativeMethods.CloseThemeData(editThemeData);
				editThemeData = IntPtr.Zero;
			}
		}

		ZFilterStrip GetParentFilterStrip(Control control)
		{
			var current = control;
			while (current != null)
			{
				if (current is ZFilterStrip filterStrip)
				{
					return filterStrip;
				}

				current = current.Parent;
			}

			return null;
		}

#endif
		#endregion

		public void SelectItem(string value)
		{
			ShowDropDown(false);
			if (DropDown != null)
			{
				DropDown.SelectItemFromCode(value);
			}

			UpdateSelectionFromDropItem();
			HideDropDown();
		}

		public override bool IsOnGrid { get; set; }

		[DefaultValue(false)]
		public bool ShowHorizontalScrollBar { get; set; }

		#region Test Helpers
#if DEBUG

		internal bool isParentReadOnlyExposed
		{
			get { return ReadOnly; }
		}

		internal ZDropForm DropDownExposed
		{
			get { return DropDown; }
		}

		internal Rectangle RectangleExposed
		{
			get { return ButtonRectangle; }
		}

		internal void OnMouseDownExposed(MouseEventArgs e)
		{
			OnMouseDown(e);
		}

#endif
		#endregion
	}
}
