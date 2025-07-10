using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Integration;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public interface IDropFormParent
	{
		Font Font { get; }
		int MaxItemsToShowInDropDown { get; }
		string Code { get; }
		ZDropEdit.ShowInDropDownList ShowInDropDown { get; }
		int MinDropDownWidth { get; }
		ICodeDescription LastSelectedItem { get; }

		[SuppressWeaklyTypedCollectionMessage]
		IList GetFilteredListForDropDown();
		bool IsItemValidForAutoComplete(ICodeDescription item);
		MultilingualString GetMultilingualValue(ICodeDescription item);
		bool ShouldCloseOnMouseDown(Point mouseLocation);

		void OnDropDownClosed();
		void OnItemSelected(ICodeDescription item, bool commitValue);

		bool ShowHorizontalScrollBar { get; }
		bool SupportsEmptyCode { get; }
		char PasswordChar { get; }
	}

	[SuppressFormDesignerAnalysis]
	public class ZDropForm : KForm, INoActivateModalForm
	{
		public ZDropForm(IDropFormParent parentDropEdit)
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

			ParentDropEdit = parentDropEdit;
			VisibleItems = parentDropEdit.MaxItemsToShowInDropDown;

			Font = parentDropEdit.Font;
			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.Manual;
			ShowInTaskbar = false;
			BackColor = SystemColors.Window;

			ShowHorizontalScrollBar = parentDropEdit.ShowHorizontalScrollBar;
			SetSizing();

			SelectItemFromCode();

			//if we somehow activate and later deactivate (only possible if 'Activate a window by hovering over it with the mouse' is turned on), then close.
			this.Deactivate += (o, e) => { HideDropDown(); };
		}

		readonly bool ShowHorizontalScrollBar;
		const int AlwaysOnTop = -1;
		public void ShowDropDown([DpiState(DpiState.ScaledVariant)] Point pointInScreenCoords, bool mouseIsCurrentlyDown)
		{
			readyForMouseUp = mouseIsCurrentlyDown;

			SetMouseHook();

			Location = pointInScreenCoords;
			SafeNativeMethods.ShowWindow(this, ShowWindowsOptions.SW_SHOWNOACTIVATE);
			SafeNativeMethods.SetWindowPos(this, AlwaysOnTop, Location.X, Location.Y, Width, Height, 0x0010);
		}

		public void HideDropDown()
		{
			if (Visible)
			{
				Visible = false;
				Enabled = false;

				RemoveMouseHook(); //If something modal happens in OnDropDownClosed(), hook remains attached until that modal operation finishes. This is safer.
				ParentDropEdit.OnDropDownClosed();
			}
		}

		#region Mouse Hooks

		[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
#if DEBUG
		internal
#endif
		IntPtr mouseHook;
		bool readyForMouseUp;
		SafeNativeMethods.MouseHookProc mouseHookProcEventHandler;

		protected void SetMouseHook()
		{
			if (mouseHook == IntPtr.Zero)
			{
				if (mouseHookProcEventHandler == null)
				{
					mouseHookProcEventHandler = new SafeNativeMethods.MouseHookProc(MouseHookProc);
				}
				mouseHook = UnsafeNativeMethods.SetWindowsHookEx(WindowsMessage.WH_MOUSE, mouseHookProcEventHandler, IntPtr.Zero, SafeNativeMethods.GetCurrentThreadId());
			}
		}

#if DEBUG
		internal
#endif
		   IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			//this method gets called twice for every message it listens for - fair warning!

			//For some reason, whenever UserIdleDetecter.cs mouseHook is hooked, mouse hook in ZDropForm.cs stops working. (I don't know why.)
			//Usually mouse hook is hooked only briefly though, so it's OK so long as that remains true.

			var handled = false;

			try
			{
#if DEBUG
				if (Globals.IsTest)
				{
					ThrowExceptionForTestIfNeeded();
				}
#endif

				if (nCode >= 0 && !this.Disposing)
				{
					var mouseHookInfoEx = (SafeNativeMethods.MouseHookInfoEx)Marshal.PtrToStructure(lParam, typeof(SafeNativeMethods.MouseHookInfoEx));
					var mouseHookInfo = mouseHookInfoEx.MouseHookInfo;

					var location = ControlDpiScalingHelper.NewScaledPoint(mouseHookInfo.Location.X, mouseHookInfo.Location.Y, false);

					switch (wParam.ToInt64()) // In 64-bit process wParam might go beyond Int32 and can cause Arithmetic Overflow. Always using Int64 is a better practice.
					{
						case SafeNativeMethods.WM_NCLBUTTONDOWN:
						case SafeNativeMethods.WM_NCRBUTTONDOWN:
						case SafeNativeMethods.WM_LBUTTONDOWN:
						case SafeNativeMethods.WM_RBUTTONDOWN:
							readyForMouseUp = DidNotClickInSpecialAreas(location);
							HandleMouseDown(location);
							break;

						case SafeNativeMethods.WM_NCLBUTTONUP:
						case SafeNativeMethods.WM_NCRBUTTONUP:
						case SafeNativeMethods.WM_LBUTTONUP:
							if (readyForMouseUp)
							{
								handled = HandleClick(location);
							}
							else if (!DidNotClickInSpecialAreas(location))
							{
								handled = HandleClickInSpecialArea(PointToClient(location));
							}
							break;

						case SafeNativeMethods.WM_RBUTTONUP:
							if (readyForMouseUp)
							{
								handled = HandleClick(location);
							}
							if (!handled)
							{
								// Dragging the scroll bar and right clicking outside the window causes an AccessViolationException.
								// We don't pass this message on to avoid the exception.
								handled = ((Control.MouseButtons & (MouseButtons.Left | MouseButtons.Right)) != 0) && !MouseWithinForm(location);
							}
							break;

						case SafeNativeMethods.WM_MOUSEWHEEL:
							HandleMouseScroll(mouseHookInfoEx.MouseData);
							handled = true; //to prevent scrolling from 'falling through'
							break;
					}
				}

				if (!handled)
				{
					return UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, mouseHook), nCode, wParam, lParam);
				}
			}
			catch (Exception ex)
			{
				Application.OnThreadException(ex);
			}
			return new IntPtr(1);
		}

		void HandleMouseDown(Point screenLocation)
		{
			// Hacky way to make dropdown close properly
			if (!ClientRectangle.Contains(PointToClient(screenLocation)) && ParentDropEdit.ShouldCloseOnMouseDown(screenLocation))
			{
				HideDropDown();
			}
		}

		bool HandleClick(Point screenLocation)
		{
			var result = false;

			var locationForDropForm = PointToClient(screenLocation);

			if (ClientRectangle.Contains(locationForDropForm))
			{
				result = HandleDropDownClick(locationForDropForm);
			}
			else
			{
				HideDropDown();
			}

			return result;
		}

#if DEBUG
		protected virtual void ThrowExceptionForTestIfNeeded()
		{ }
#endif

		protected bool HandleDropDownClick(Point location)
		{
			var isHandled = false;

			if (GetSpecialAreas().All(r => !r.Contains(location)))
			{
				var item = ItemAtLocation(location);

				if (TranslationFeedbackManager.InTranslationFeedbackMode())
				{
					ParentDropEdit.OnItemSelected(item, false);
					HideDropDown();
					isHandled = true;
				}
				else if (IsItemSelectable(item))
				{
					ParentDropEdit.OnItemSelected(item, true);
					HideDropDown();
					isHandled = true;
				}
			}

			return isHandled;
		}

		bool MouseWithinForm(Point screenLocation)
		{
			return ParentForm != null && ParentForm.ClientRectangle.Contains(ParentForm.PointToClient(screenLocation));
		}

		bool DidNotClickInSpecialAreas(Point screenLocation)
		{
			var locationForDropForm = PointToClient(screenLocation);
			return GetSpecialAreas().All(r => !r.Contains(locationForDropForm));
		}

		void RemoveMouseHook()
		{
			if (mouseHook != IntPtr.Zero)
			{
				UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, mouseHook));
				mouseHook = IntPtr.Zero;
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing && mouseHook != IntPtr.Zero)
			{
				RemoveMouseHook();
			}

			base.Dispose(disposing);
		}

		protected override void OnEnter(EventArgs e)
		{
			try
			{
				if (!IsDisposed && IsHandleCreated)
				{
					base.OnEnter(e);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ExceptionInZDropOnEnterOrOnLeave" + Parent?.Parent?.Name,
					"Exception occurred in ZDropForm in OnEnter or OnLeave."
					, ex);
				throw;
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			try
			{
				if (!IsDisposed && IsHandleCreated)
				{
					base.OnLeave(e);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ExceptionInZDropOnEnterOrOnLeave" + Parent?.Parent?.Name,
					"Exception occurred in ZDropForm in OnEnter or OnLeave."
					, ex);
				throw;
			}
		}

		Rectangle ScrollBarRectangle
		{
			get
			{
				return ScrollBar == null ?
					Rectangle.Empty :
					ControlDpiScalingHelper.NewScaledRectangle(ScrollBar.Location.X, ScrollBar.Location.Y, ScrollBar.Size.Width, ScrollBar.Size.Height, false);
			}
		}

		Rectangle HorizontalScrollBarRectangle
		{
			get
			{
				return HorizontalScrollBar == null ?
					Rectangle.Empty :
					ControlDpiScalingHelper.NewScaledRectangle(HorizontalScrollBar.Location.X, HorizontalScrollBar.Location.Y, HorizontalScrollBar.Size.Width, HorizontalScrollBar.Size.Height, false);
			}
		}

		internal IEnumerable<Rectangle> GetSpecialAreas()
		{
			yield return ScrollBarRectangle;
			if (ShowHorizontalScrollBar)
			{
				yield return HorizontalScrollBarRectangle;
			}

			foreach (var rectangle in GetSpecialAreasCore())
			{
				yield return rectangle;
			}
		}

		protected virtual IEnumerable<Rectangle> GetSpecialAreasCore()
		{
			return Enumerable.Empty<Rectangle>();
		}

		internal void SelectItemFromCode(string value = null)
		{
			HighlightedItem = -1;
			var currentText = value ?? ParentDropEdit.Code;

			if (!string.IsNullOrEmpty(currentText) || ParentDropEdit.SupportsEmptyCode)
			{
				foreach (var comparison in AutoCompleteSearchComparers)
				{
					var match = AutocompleteSearchHelper.FindIndex(ParentDropEdit, List, currentText, comparison, ParentDropEdit.LastSelectedItem);
					if (match >= 0)
					{
						HighlightedItem = match;
						EnsureHighlightedItemVisible();

						break;
					}
				}
			}
		}

		protected virtual IEnumerable<AutoCompleteStringComparisonType> AutoCompleteSearchComparers => new[] { AutoCompleteStringComparisonType.Exact, AutoCompleteStringComparisonType.StartsWith };

		internal ICodeDescription SelectedItem
		{
			get { return (List != null && HighlightedItem < List.Count && HighlightedItem >= 0) ? (ICodeDescription)List[HighlightedItem] : null; }
		}

		internal ICodeDescription FirstItem
		{
			get { return List?.Cast<ICodeDescription>().FirstOrDefault(); }
		}

		internal bool HandleCommandKey(Keys keyData)
		{
			var keyWasHandled = true;
			switch (keyData)
			{
				case Keys.Down:
					if (List != null && HighlightedItem < List.Count - 1)
					{
						HighlightedItem = NextItemIndex;
					}
					break;

				case Keys.Up:
					if (HighlightedItem > 0)
					{
						HighlightedItem = PreviousItemIndex;
					}
					break;

				case Keys.PageDown:
					if (List != null && HighlightedItem < List.Count - 1)
					{
						var newIndex = Math.Max(HighlightedItem + VisibleItems - 1, VisibleItems - 1);
						HighlightedItem = Math.Min(List.Count - 1, newIndex);
					}
					break;

				case Keys.PageUp:
					if (HighlightedItem > 0)
					{
						var newIndex = HighlightedItem - VisibleItems + 1;
						HighlightedItem = Math.Max(0, newIndex);
					}
					break;

				default:
					keyWasHandled = false;
					break;
			}

			if (keyWasHandled)
			{
				EnsureHighlightedItemVisible();
			}

			return keyWasHandled;
		}

		protected virtual int NextItemIndex
		{
			get { return HighlightedItem + 1; }
		}

		protected virtual int PreviousItemIndex
		{
			get { return HighlightedItem - 1; }
		}

		[DpiState(DpiState.Unscaled)]
		protected virtual int VerticalIndent
		{
			get { return 1; }
		}

		[DpiState(DpiState.Unscaled)]
		protected virtual int ExtraSpaceHeight
		{
			get { return 0; }
		}

		protected virtual int MinimumWidth
		{
			get { return 0; }
		}

		int ItemIndexAtLocation(Point location)
		{
			if (List != null && List.Count > 0)
			{
				var yOffset = ControlDpiScalingHelper.ScaleToCurrentDpiY(VerticalIndent);
				var listBounds = new Rectangle(ControlDpiScalingHelper.ScaleToCurrentDpiX(1), yOffset, Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(2), Height + yOffset);

				if (listBounds.Contains(location))
				{
					var yIndex = PaintItemOffset + ((location.Y - yOffset) / ItemHeight);
					if (yIndex < List.Count)
					{
						return yIndex;
					}
				}
			}

			return -1;
		}

		internal ICodeDescription ItemAtLocation(Point location)
		{
			var index = ItemIndexAtLocation(location);

			return index >= 0 ? (ICodeDescription)List[index] : null;
		}

		const int scrollWheelUnit = 120;
		int scrollCounter;

		internal void HandleMouseScroll(int mouseData)
		{
			if (ScrollBar != null)
			{
				// See this site for info on the algorithm below.
				// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookstructures/mousehookstruct.asp

				// Because enhanced mousewheels send deltas less than 120, we must keep a counter of accumulated deltas thus far.
				// http://www.codeproject.com/Articles/155717/Handling-Enhanced-Mouse-Wheels-in-your-Application

				var delta = mouseData >> 16;
				scrollCounter += (delta * 2); //to compensate for the fact that we are not handling scrolls twice by accident anymore

				while (scrollCounter <= -scrollWheelUnit)
				{
					var max = ScrollBar.Maximum - (ScrollBar.LargeChange - 1);
					ScrollBar.Value = Math.Min(max, ScrollBar.Value + ScrollBar.SmallChange);
					scrollCounter += scrollWheelUnit;
				}
				while (scrollCounter >= scrollWheelUnit)
				{
					ScrollBar.Value = Math.Max(ScrollBar.Minimum, ScrollBar.Value - ScrollBar.SmallChange);
					scrollCounter -= scrollWheelUnit;
				}
			}
		}

		internal protected virtual bool HandleClickInSpecialArea(Point locationForDropForm)
		{
			return false;
		}

		#region Implementation

		protected const int MA_NOACTIVATE = 3;

		protected int VisibleItems;
		protected const string ItemSpacer = "    ";

		protected ZVScrollBar ScrollBar;
		protected ZHScrollBar HorizontalScrollBar;
		protected int HighlightedItem = -1;
		protected int PaintItemOffset;
		protected int HorizontalPaintItemOffset;
		protected IDropFormParent ParentDropEdit;
		protected int HighlightedItemForMouse = -1;
		Rectangle LastHighlightedRectForMouse;

		IList _list;
		protected IList List
		{
			get
			{
				if (_list == null)
				{
					RefreshList();
				}
				return _list;
			}
		}

		public void RefreshList()
		{
			_list = ParentDropEdit.GetFilteredListForDropDown();
			if (_list != null)
			{
				SetSizing();
				SelectItemFromCode();

				Invalidate();
			}
		}

#if DEBUG
		[SuppressWeaklyTypedCollectionMessage]
		internal IList List_Exposed
		{
			get { return List; }
		}

		internal int HighlightedItem_Exposed
		{
			get { return HighlightedItem; }
			set { HighlightedItem = value; }
		}
#endif

		#region Painting

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (ParentDropEdit.ShowInDropDown == ZDropEdit.ShowInDropDownList.ShowCodeAndDescription)
			{
				PaintDescriptionBackground(e.Graphics);
			}

			PaintList(e.Graphics);
			PaintBorder(e.Graphics, e.ClipRectangle);
		}

		protected void PaintList(Graphics graphics)
		{
			var list = this.List;
			if (list != null && list.Count > 0)
			{
				var currentTop = ControlDpiScalingHelper.ScaleToCurrentDpiY(VerticalIndent);
				var horizontalPadding = ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
				var left = horizontalPadding / 2;
				var longestItemDescriptionLength = 0;
				if (ShowHorizontalScrollBar)
				{
					longestItemDescriptionLength = list.Cast<ICodeDescription>().Max(item => item.Description.Length);
				}

				for (var index = Math.Max(PaintItemOffset, 0); index < list.Count; index++)
				{
					var item = (ICodeDescription)list[index];
					var codeRect = ControlDpiScalingHelper.NewScaledRectangle(left, currentTop, Width - horizontalPadding, ItemHeight, false);

					PaintItem(graphics, item, codeRect, (index == HighlightedItem), longestItemDescriptionLength, (index == HighlightedItemForMouse));

					currentTop += ItemHeight;
					if (currentTop > Height)
					{
						break;
					}
				}
			}
		}

		protected void PaintItem(Graphics graphics, ICodeDescription item, [DpiState(DpiState.ScaledVariant)] Rectangle codeRect, bool isHighlighted, int longestItemDescriptionLength = 0, bool isHighlightedForMouseMove = false)
		{
			var textBrush = SystemBrushes.WindowText;
			Brush brush = null;
			Brush descriptionBrush = null;

			if (isHighlighted)
			{
				brush = SystemBrushes.Highlight;
			}
			else if (!isHighlighted && isHighlightedForMouseMove)
			{
				brush = BrushForMouseOver;
				descriptionBrush = brush;
				LastHighlightedRectForMouse = codeRect;
			}

			if (brush != null && !TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				if (IsItemSelectable(item))
				{
					textBrush = SystemBrushes.HighlightText;
					graphics.FillRectangle(brush, codeRect);

					if (ParentDropEdit.ShowInDropDown == ZDropEdit.ShowInDropDownList.ShowCodeAndDescription)
					{
						PaintDarkenedDescriptionItem(graphics, codeRect, descriptionBrush);
					}
				}
			}

			var cellText = ParentDropEdit.GetMultilingualValue(item).ToString();
			if (ParentDropEdit.PasswordChar != '\0')
			{
				cellText = new string(ParentDropEdit.PasswordChar, cellText.Length);
			}
			PaintItemText(item, cellText, graphics, Font, textBrush, codeRect);

			if (brush != null && TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				TranslationFeedbackManager.PaintCaptionHighlight(graphics, Rectangle.Ceiling(codeRect));
			}

			if (ParentDropEdit.ShowInDropDown == ZDropEdit.ShowInDropDownList.ShowCodeAndDescription)
			{
				var descRect = ControlDpiScalingHelper.NewScaledRectangle(CodeWidth, codeRect.Y, codeRect.Width - CodeWidth, codeRect.Height, false);
				var startPosToShow = 0;
				if (HorizontalScrollBar != null && HorizontalScrollBar.Visible && longestItemDescriptionLength != 0)
				{
					startPosToShow = longestItemDescriptionLength * HorizontalPaintItemOffset / HorizontalScrollBar.Maximum;
					if (startPosToShow > item.Description.Length)
					{
						startPosToShow = item.Description.Length;
					}
				}
				TextRendererHelper.DrawText(graphics, ItemSpacer + item.Description.Substring(startPosToShow), Font, descRect, textBrush);

				if (brush != null && TranslationFeedbackManager.InTranslationFeedbackMode())
				{
					TranslationFeedbackManager.PaintCaptionHighlight(graphics, Rectangle.Ceiling(descRect));
				}
			}
		}

		protected virtual void PaintItemText(ICodeDescription item, ZString text, Graphics graphics, Font font, Brush textBrush, Rectangle codeRectangle)
		{
			TextRendererHelper.DrawText(graphics, text, font, codeRectangle, textBrush);
		}

		public virtual bool IsItemSelectable(ICodeDescription item)
		{
			return item != null;
		}

		protected void PaintBorder(Graphics graphics, Rectangle rect)
		{
			ControlPaint.DrawBorder(graphics, rect, Color.Black, ButtonBorderStyle.Solid);
		}

		protected virtual void PaintDescriptionBackground(Graphics graphics)
		{
			var backgroundStart = CodeWidth + ControlDpiScalingHelper.ScaleToCurrentDpiX(DescriptionBackgroundOverlap);
			var backgroundWidth = Width - backgroundStart;

			if (ScrollBar != null)
			{
				backgroundWidth += ScrollBar.Width;
			}

			ControlPaint.DrawBorder(graphics, DottedLineBounds, SystemColors.ControlDarkDark, ButtonBorderStyle.Dotted);
			graphics.FillRectangle(SystemBrushes.ControlLight, backgroundStart, 0, backgroundWidth, Height);
		}

		protected Rectangle DottedLineBounds
		{
			get
			{
				var xCoord = CodeWidth + ControlDpiScalingHelper.ScaleToCurrentDpiX(DescriptionBackgroundOverlap - 1); // Just a graphics thing, perfection is not needed.
				return ControlDpiScalingHelper.NewScaledRectangle(xCoord, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(2), Height, false);
			}
		}

		[DpiState(DpiState.Unscaled)]
		protected virtual int DescriptionBackgroundOverlap
		{
			get { return 8; }
		}

		protected virtual void PaintDarkenedDescriptionItem(Graphics graphics, Rectangle selectedItemRect, Brush descriptionBrush = null)
		{
			var backgroundStart = DottedLineBounds.Left;
			var backgroundWidth = Width - backgroundStart;

			if (ScrollBar != null)
			{
				backgroundWidth += ScrollBar.Width;
			}

			var descriptionRect = ControlDpiScalingHelper.NewScaledRectangle(backgroundStart, selectedItemRect.Y, backgroundWidth, selectedItemRect.Height, false);

			if (descriptionBrush == null)
			{
				descriptionBrush = BrushProvider.FromColor(Color.FromArgb(
						 Math.Max(SystemColors.Highlight.R - 30, 0),
						 Math.Max(SystemColors.Highlight.G - 30, 0),
						 Math.Max(SystemColors.Highlight.B - 30, 0)));
			}

			graphics.FillRectangle(descriptionBrush, descriptionRect);
		}

		Brush BrushForMouseOver => BrushProvider.FromColor(Color.FromArgb(Math.Max(SystemColors.Highlight.A - 120, 0), SystemColors.Highlight));

		#endregion

		#region Sizing

		const int MaxDropWidth = 1350; // The current minimum width requirement for user screen is 1366px

		[DpiState(DpiState.ScaleY)]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		int ItemHeight
		{
			get
			{
				if (itemHeight <= 0)
				{
					using (var graphics = Graphics.FromHwnd(Handle))
					{
						graphics.PageUnit = GraphicsUnit.Pixel;
						itemHeight = (int)graphics.MeasureString("Item", Font, Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1)).Height;
					}
				}

				return itemHeight;
			}
		}
		int itemHeight = -1;

		protected internal void SetSizing()
		{
			fCodeWidth = -1;
			var totalWidth = 0;
			if (ParentDropEdit.ShowInDropDown != ZDropEdit.ShowInDropDownList.OnlyShowDescription)
			{
				totalWidth += CodeWidth;
			}
			if (ParentDropEdit.ShowInDropDown != ZDropEdit.ShowInDropDownList.OnlyShowCode)
			{
				totalWidth += DescriptionWidth;
			}

			var itemsToDisplay = Math.Min(List == null ? 0 : List.Count, VisibleItems);
			var height = Math.Max(ItemHeight * itemsToDisplay, ItemHeight) + ControlDpiScalingHelper.ScaleToCurrentDpiY(ExtraSpaceHeight);
			var maxDropWidthToCurrentDpiX = Math.Min(MaxDropWidth, ControlDpiScalingHelper.ScaleToCurrentDpiX(MaxDropWidth));
			var dropWidth = Math.Min(maxDropWidthToCurrentDpiX, Math.Max(totalWidth + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), ParentDropEdit.MinDropDownWidth));

			var shouldShowHorizontalScrollBar = ShowHorizontalScrollBar && totalWidth > dropWidth;
			if (List != null)
			{
				if (List.Count > VisibleItems)
				{
					if (ScrollBar == null)
					{
						ScrollBar = new ZVScrollBar();
						ScrollBar.ValueChanged += ScrollBar_ValueChanged;
					}
					ScrollBar.Visible = true;
					ControlDpiScalingHelper.SetHeight(ref ScrollBar, height, false);
					ScrollBar.Maximum = List.Count + ScrollBar.LargeChange - VisibleItems - 1;
					ScrollBar.ContextMenu = new ContextMenu(); // prevent the silly default context menu

					dropWidth = Math.Min(
							maxDropWidthToCurrentDpiX,
						Math.Max(dropWidth + ZGUISystemInformation.VerticalScrollBarWidth, ParentDropEdit.MinDropDownWidth));

					ScrollBar.Location = ControlDpiScalingHelper.NewScaledPoint(dropWidth - ZGUISystemInformation.VerticalScrollBarWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
					Controls.Add(ScrollBar);
				}
				else
				{
					PaintItemOffset = 0;
					if (ScrollBar != null)
					{
						ScrollBar.Value = 0;
						ScrollBar.Visible = false;
					}
				}

				if (shouldShowHorizontalScrollBar)
				{
					if (HorizontalScrollBar == null)
					{
						HorizontalScrollBar = new ZHScrollBar();
						HorizontalScrollBar.ValueChanged += HorizontalScrollBar_ValueChanged;
					}
					HorizontalScrollBar.Visible = true;
					ControlDpiScalingHelper.SetWidth(ref HorizontalScrollBar, dropWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
					HorizontalScrollBar.ContextMenu = new ContextMenu(); // prevent the silly default context menu

					HorizontalScrollBar.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.ScaleToCurrentDpiX(1), height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
					Controls.Add(HorizontalScrollBar);
				}
				else
				{
					HorizontalPaintItemOffset = 0;
					if (HorizontalScrollBar != null)
					{
						HorizontalScrollBar.Value = 0;
						HorizontalScrollBar.Visible = false;
					}
				}
			}

			dropWidth = Math.Max(dropWidth, MinimumWidth);
			var hScrollBarHeight = shouldShowHorizontalScrollBar ? HorizontalScrollBar.Height : 0;
			Size = ControlDpiScalingHelper.NewScaledSize(dropWidth, height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2) + hScrollBarHeight, false);
			Region = new Region(ControlDpiScalingHelper.NewScaledRectangle(0, 0, Size.Width, Size.Height, false));
		}

		[return: DpiState(DpiState.ScaleX)]
		protected float GetStringWidth(string @string, Graphics graphics)
		{
			return string.IsNullOrEmpty(@string) ? 0 : (int)Math.Ceiling(TextRendererHelper.MeasureText(graphics, @string, Font).Width);
		}

		[DpiState(DpiState.ScaleX)]
		protected int DescriptionWidth
		{
			get
			{
				if (List != null && List.Count > 0 && ParentDropEdit.ShowInDropDown != ZDropEdit.ShowInDropDownList.OnlyShowCode)
				{
					using (var graphics = Graphics.FromHwnd(Handle))
					{
						graphics.PageUnit = GraphicsUnit.Pixel;

						return (int)Math.Ceiling(List.Cast<ICodeDescription>().Max(item => GetStringWidth(ItemSpacer + item.Description, graphics)));
					}
				}

				return 0;
			}
		}

		[DpiState(DpiState.ScaleX)]
		protected int CodeWidth
		{
			get
			{
				if (fCodeWidth < 0 && List != null && ParentDropEdit.ShowInDropDown != ZDropEdit.ShowInDropDownList.OnlyShowDescription)
				{
					using (var graphics = Graphics.FromHwnd(Handle))
					{
						graphics.PageUnit = GraphicsUnit.Pixel;

						fCodeWidth = (int)Math.Ceiling(List.Cast<ICodeDescription>()
							.Where(IncludeItemInCodeWidthCalculation)
							.Select(item => GetStringWidth(item.GetMultilingualCode(), graphics))
							.Aggregate(0.0f, Math.Max));
					}
				}

				return fCodeWidth;
			}
		}
		int fCodeWidth = -1;

		protected virtual bool IncludeItemInCodeWidthCalculation(ICodeDescription item)
		{
			return true;
		}

		#endregion

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		protected sealed class ZVScrollBar : VScrollBar
		{
			public ZVScrollBar()
			{
			}

			protected override void WndProc(ref Message m)
			{
				try
				{
					base.WndProc(ref m);
				}
				catch (ObjectDisposedException)
				{
					// occurs in the button click handler occasionally
				}
			}
		}

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		protected sealed class ZHScrollBar : HScrollBar
		{
			public ZHScrollBar()
			{
			}

			protected override void WndProc(ref Message m)
			{
				try
				{
					base.WndProc(ref m);
				}
				catch (ObjectDisposedException)
				{
					// occurs in the button click handler occasionally
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!LastHighlightedRectForMouse.Contains(e.Location))
			{
				var itemIndex = ItemIndexAtLocation(e.Location);
				if (itemIndex >= 0)
				{
					HighlightItem(itemIndex);
				}
			}

			base.OnMouseMove(e);
		}

		protected virtual void HighlightItem(int itemIndex)
		{
			HighlightedItemForMouse = itemIndex;
			Invalidate();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == CargoWise.Interop.WindowsMessage.WM_MOUSEACTIVATE)
			{
				m.Result = new IntPtr(MA_NOACTIVATE);
			}
			else
			{
				base.WndProc(ref m);
			}
		}

		void ScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (PaintItemOffset != ScrollBar.Value)
			{
				PaintItemOffset = ScrollBar.Value;
				Invalidate();
			}
		}

		void HorizontalScrollBar_ValueChanged(object sender, EventArgs e)
		{
			if (HorizontalPaintItemOffset != HorizontalScrollBar.Value)
			{
				HorizontalPaintItemOffset = HorizontalScrollBar.Value;
				Invalidate();
			}
		}

		protected void EnsureHighlightedItemVisible()
		{
			if (HighlightedItem != -1)
			{
				if (ScrollBar != null)
				{
					if (HighlightedItem < PaintItemOffset)
					{
						PaintItemOffset = HighlightedItem;
						ScrollBar.Value = Math.Max(PaintItemOffset, ScrollBar.Minimum);
					}

					if (HighlightedItem > PaintItemOffset + (VisibleItems - 1))
					{
						PaintItemOffset = HighlightedItem - (VisibleItems - 1);
						ScrollBar.Value = Math.Min(PaintItemOffset, ScrollBar.Maximum);
					}
				}
				Invalidate();
			}
		}

		#endregion
	}
}
