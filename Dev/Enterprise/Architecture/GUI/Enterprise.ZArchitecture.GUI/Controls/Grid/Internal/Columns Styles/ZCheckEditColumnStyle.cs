using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
#if WINZOR
using Graphics = System.Drawing.BGraphics;
#endif

namespace Enterprise.ZArchitecture
{
	public enum CheckBoxImageType
	{
		HotUnChecked, HotChecked, UnChecked, Checked, ReadOnlyChecked, ReadOnlyUnChecked, SelectedChecked, SelectedUnChecked, AlternatingReadOnlyChecked, AlternatingReadOnlyUnChecked, CustomBackColourChecked, CustomBackColourUnChecked, EditableBackColorChecked, EditableBackColorUnChecked
	}

	public class ZCheckBoxColumnStyleInfo : ZTextBoxColumnStyleInfo
	{
		public ZCheckBoxColumnStyleInfo() // required for ZGrid column designer
		{
		}

		public ZCheckBoxColumnStyleInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		[ZColumnBindingMemberType(typeof(ZBool))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZCheckBoxColumnStyle); }
		}
	}

	public partial class ZCheckBoxColumnStyle : ZCustomControlColumnStyle
	{
		public ZCheckBoxColumnStyle(ZCheckBoxColumnStyleInfo columnInfo)
			: base(() => new XPCheckBox(), columnInfo)
		{
			#if !WINZOR
			checkBoxForDrawing = new XPCheckBox();
			#endif
		}

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			TextBox.Enabled = false;
			var columnCheckEdit = (XPCheckBox)control;
			columnCheckEdit.Text = "";

			columnCheckEdit.ButtonStyle = Style.XpStyle;
			columnCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 80);

			columnCheckEdit.CheckedChanged += ColumnCheckEditStateChanged;
			columnCheckEdit.Enter += ColumnCheckEditEnter;
			columnCheckEdit.ForeColor = columnCheckEdit.BackColor;
		}

		internal bool Checked
		{
			get { return ColumnCheckEdit.Checked; }
		}

		public override int Width
		{
			get { return base.Width; }
			[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Inside a Width setter")]
			set { base.Width = value < ControlDpiScalingHelper.ScaleToCurrentDpiX(MinWidth) ? ControlDpiScalingHelper.ScaleToCurrentDpiX(MinWidth) : value; }
		}
		const int MinWidth = 40;

		protected override bool ShouldSetValue(object currentEditValue, CurrencyManager source, int rowNum)
		{
			var existingValue = (ZBool)GetColumnValueAtRow(source, rowNum);
			return (existingValue == ZBool.True && (string)currentEditValue == "N") || (existingValue == ZBool.False && (string)currentEditValue == "Y");
		}

		protected override Size GetPreferredSize(Graphics g, object value) // for double click resizing
		{
			return ControlDpiScalingHelper.NewScaledSize(MinWidth, 0, true);
		}

#if DEBUG
		internal
#if !WINZOR
			new
#endif
			Size GetPreferredSizeExposed(Graphics g, object value)
		{
			return GetPreferredSize(g, value);
		}
#endif

		public int ColumnNumber
		{
			get { return fColumnNumber; }
			set { fColumnNumber = value; }
		}
		protected int fColumnNumber = -1;

		#region ZBool Support

		protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
		{
			base.SetColumnValueAtRow(source, rowNum, new ZBool(value));
		}

		#endregion

		#region ShouldMoveCursor

		protected override bool ShouldMoveCursor(Keys keyData)
		{
			keyData &= Keys.KeyCode;
			return !(keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Enter);
		}

		#endregion

		#region Mouse Handlers

		public void HookMouseHandler(IMouseUpMouseMove grid)
		{
			if (!mouseHandlerHooked)
			{
				grid.MouseUp += HandleMouseUp;
				grid.MouseMove += HandleMouseMove;
				mouseHandlerHooked = true;
			}
		}

		public void UnHookMouseHandler(ZGrid grid)
		{
			if (mouseHandlerHooked)
			{
				grid.MouseUp -= HandleMouseUp;
				grid.MouseMove -= HandleMouseMove;
				mouseHandlerHooked = false;
			}
		}

		bool mouseHandlerHooked;

		void HandleMouseMove(object sender, MouseEventArgs e)
		{
			var hitInfo = parentDataGrid.HitTest(ControlDpiScalingHelper.NewScaledPoint(e.X, e.Y, false));

			if (FocusedRow != hitInfo.Row || FocusedCol != hitInfo.Column)
			{
				var wasFocused = (FocusedCol == ColumnNumber);
				FocusedRow = hitInfo.Row;
				FocusedCol = hitInfo.Column;
				if (FocusedCol == ColumnNumber || wasFocused)
				{
					Invalidate();
				}
			}
		}

		void HandleMouseUp(object sender, MouseEventArgs e)
		{
			var hitInfo = parentDataGrid.HitTest(ControlDpiScalingHelper.NewScaledPoint(e.X, e.Y, false));
			var isClickInCell = hitInfo.Column == ColumnNumber && hitInfo.Row > -1;

			if (sourceData != null && sourceData.Count > hitInfo.Row)
			{
				if (isClickInCell && !IsCellReadOnly(sourceData, hitInfo.Row))
				{
					var rowValue = ColumnTextAtRow(sourceData, hitInfo.Row);
					var currentValue = rowValue == Constants.BooleanTrueString;
					ColumnCheckEdit.Checked = !currentValue;
				}

				Invalidate();
			}
		}

		#endregion

		#region Implementation

		XPCheckBox ColumnCheckEdit => (XPCheckBox)EditControl;

		protected int FocusedRow = -1;
		protected int FocusedCol = -1;

		#region Overrides

		/// <summary>
		/// Show the edit control and start editing.
		/// </summary>
		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool isCellVisible)
		{
#if !WINZOR
			if (CanEdit && !IsCellReadOnly(source, rowNum))
			{
#else
			var isEditable = CanEdit && !IsCellReadOnly(source, rowNum);
#endif
			var rect =
				bounds.Width < MinWidthForErrorProviderAndTickBox
					? NotificationProvider.AdjustEditControlBoundsForNotificationIconIfRequired(bounds, source, rowNum)
					: NotificationProvider.ShrinkBoundsForNotificationIcon(bounds);

			base.Edit(source, rowNum, rect, readOnly, instantText, isCellVisible);
			TextBox.Bounds = Rectangle.Empty;
			sourceData = source;

			ColumnCheckEdit.Parent = TextBox.Parent;
			ColumnCheckEdit.Location = rect.Location;
			ColumnCheckEdit.Size = rect.Size;
			ColumnCheckEdit.CheckedValue = ColumnTextAtRow(source, rowNum);

#if WINZOR
			EditControl.SetReadOnly(!isEditable);
#endif
			EditControl.Visible = true;
			EditControl.BringToFront();
			EditControl.Focus();
#if !WINZOR
		}
			else
			{
				NotificationProvider.OnEnterEditControl(source, rowNum, bounds);
			}
#endif
		}

		const int MinWidthForErrorProviderAndTickBox = 20;

#if !WINZOR

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "See Issue 00852474., See Issue 00852474. Imperfectly correlated with low available physical || virtual memory.")]
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			try
			{
				var customRowBackgroundColour = parentZGrid.GetCustomRowBackgroundColour(paintingRowNum, IsCellReadOnly(source, paintingRowNum));
				backBrush = GetBackgroundColourBrush(backBrush, source, paintingRowNum);
				g.FillRectangle(backBrush, bounds);

				var boundsIfLittleColumn = Rectangle.Empty;
				if (IsValidSource(source))
				{
					boundsIfLittleColumn = NotificationProvider.PaintNotificationIconIfRequiredAndReturnRemainingAreaForPainting(g, source, paintingRowNum, bounds, backBrush);
				}

				if (bounds.Width < MinWidthForErrorProviderAndTickBox && !boundsIfLittleColumn.IsEmpty)
				{
					bounds = boundsIfLittleColumn;
				}
				else
				{
					bounds = NotificationProvider.ShrinkBoundsForNotificationIcon(bounds); // if we've got room, always leave space for notification icon, so check box doesn't bounce around when field gets a notification, tested in ZGridCheckBoxColumn
				}

				var isChecked = (ColumnTextAtRow(source, paintingRowNum) == Constants.BooleanTrueString);
				var isHotTrack = (paintingRowNum == FocusedRow) && (ColumnNumber == FocusedCol) && !IsCellReadOnly(source, paintingRowNum);

				var isCurrent = (parentDataGrid.CurrentRowIndex == paintingRowNum && parentDataGrid.CurrentCell.ColumnNumber == ColumnNumber);
				if (!isCurrent || !EditControl.Focused || parentDataGrid.IsSelected(paintingRowNum))
				{
					if (parentDataGrid.IsSelected(paintingRowNum))
					{
						var imageType = isChecked ? CheckBoxImageType.SelectedChecked : CheckBoxImageType.SelectedUnChecked;
						DrawCheckBox(g, bounds, imageType, customRowBackgroundColour);
					}
					else if (customRowBackgroundColour.ToArgb() != 0)
					{
						var imageType = isChecked ? CheckBoxImageType.CustomBackColourChecked : CheckBoxImageType.CustomBackColourUnChecked;
						DrawCheckBox(g, bounds, imageType, customRowBackgroundColour);
					}
					else if (IsCellReadOnly(source, paintingRowNum))
					{
						if (paintingRowNum % 2 == 0)
						{
							var imageType = isChecked ? CheckBoxImageType.ReadOnlyChecked : CheckBoxImageType.ReadOnlyUnChecked;
							DrawCheckBox(g, bounds, imageType, customRowBackgroundColour);
						}
						else
						{
							var imageType = isChecked ? CheckBoxImageType.AlternatingReadOnlyChecked : CheckBoxImageType.AlternatingReadOnlyUnChecked;
							DrawCheckBox(g, bounds, imageType, customRowBackgroundColour);
						}
					}
					else if (!isHotTrack)
					{
						var imageType = isChecked ? CheckBoxImageType.Checked : CheckBoxImageType.UnChecked;
						DrawCheckBox(g, bounds, imageType, customRowBackgroundColour);
					}
					else
					{
						var imageType = isChecked ? CheckBoxImageType.HotChecked : CheckBoxImageType.HotUnChecked;
						DrawCheckBox(g, bounds, imageType, customRowBackgroundColour);
					}
				}
				else
				{
					var imageType = isChecked ? CheckBoxImageType.EditableBackColorChecked : CheckBoxImageType.EditableBackColorUnChecked;
					DrawCheckBox(g, bounds, imageType, customRowBackgroundColour);
				}
			}
			catch (InvalidOperationException ex) { if (ex.Message != "Object is currently in use elsewhere.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (System.Runtime.InteropServices.ExternalException ex) { if (ex.Message != "A generic error occurred in GDI+." && ex.Message != "External component has thrown an exception.") { throw; } ZUserControl.ReportGraphicsDisplayFailure(ex); }
			catch (IndexOutOfRangeException ex)
			{
				ErrorReporter.ReportOnce("IndexOutOfRangeException_ZCheckEditColumnStyle_Paint",
							"IndexOutOfRangeException occured while calling Paint\r\n"
							+ "TextBox Value: " + TextBox.Text + "\r\n"
							+ "Current Row Number: " + paintingRowNum.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "MappingName : " + MappingName + "\r\n"
							+ "Read Only: " + ReadOnly.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Is Submissive: " + IsSubmissive.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Font: " + HeaderFont.ToString() + "\r\n"
							+ "Header Text: " + HeaderText + "\r\n"
							+ "Last Focus Cell Column Number: " + LastFocusedCell.ColumnNumber.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Last Focus Cell Row Number: " + LastFocusedCell.RowNumber.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Current Cell Column Number: " + parentDataGrid.CurrentCell.ColumnNumber.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Current Cell Row Number: " + parentDataGrid.CurrentCell.RowNumber.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Data Source : " + source?.List?.GetType().FullName + "\r\n"
							+ "Data Source Position: " + source.Position.ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Data Source Count :" + (source?.List?.Count ?? -1).ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Data Source Filter : " + (source?.List as IBusinessObjectCollection)?.CompleteFilter.LiteralTextADO + "\r\n"
							+ "Data Source Equals Source Data: " + source.Equals(sourceData).ToString(CultureInfo.InvariantCulture) + "\r\n"
							+ "Stack Trace : " + ex.StackTrace, ex);
			}
		}

#endif

		protected override object EditValue
		{
			get { return ColumnCheckEdit.CheckedValue; }
		}

		protected override int GridControlTextLength
		{
			get
			{
				return 0;
			}
		}

#endregion

#if !WINZOR

		protected static Bitmap DrawCheckBoxToImage(XPCheckBox checkBox, Rectangle bounds, bool isChecked)
		{
			if (bounds.Width <= 0)
			{
				ControlDpiScalingHelper.SetWidth(ref bounds, 1, true);
			}

			if (bounds.Height <= 0)
			{
				ControlDpiScalingHelper.SetHeight(ref bounds, 1, true);
			}

			var checkBoxImage = new Bitmap(bounds.Width, bounds.Height);
			using (var checkBoxGraphics = Graphics.FromImage(checkBoxImage))
			{
				checkBox.Checked = isChecked;
				checkBox.Draw(checkBoxGraphics, ControlDpiScalingHelper.NewScaledRectangle(0, 0, checkBoxImage.Width, checkBoxImage.Height, false));
			}
			return checkBoxImage;
		}

#if DEBUG
		internal
#endif
		XPCheckBox checkBoxForDrawing;
#if DEBUG
		virtual
#endif
 protected void DrawCheckBox(Graphics g, [DpiState(DpiState.ScaledVariant)] Rectangle bounds, CheckBoxImageType imageType, Color customRowBackgroundColour)
		{
			Bitmap buttonBitmap = null;
			checkBoxForDrawing.Size = bounds.Size;
			checkBoxForDrawing.BackColor = parentDataGrid.SelectionBackColor;
			checkBoxForDrawing.XPButtonState = XPButtonState.Normal;
			switch (imageType)
			{
				case CheckBoxImageType.Checked:
				case CheckBoxImageType.UnChecked:
					checkBoxForDrawing.BackColor = SystemColors.Window;
					buttonBitmap = DrawCheckBoxToImage(checkBoxForDrawing, bounds, imageType == CheckBoxImageType.Checked);
					break;

				case CheckBoxImageType.HotChecked:
				case CheckBoxImageType.HotUnChecked:
					checkBoxForDrawing.XPButtonState = XPButtonState.Hot;
					buttonBitmap = DrawCheckBoxToImage(checkBoxForDrawing, bounds, imageType == CheckBoxImageType.HotChecked);
					break;

				case CheckBoxImageType.ReadOnlyChecked:
				case CheckBoxImageType.ReadOnlyUnChecked:
					checkBoxForDrawing.BackColor = parentZGrid != null ? parentZGrid.ReadOnlyColorForRowNum(0) : SystemColors.Control;
					buttonBitmap = DrawCheckBoxToImage(checkBoxForDrawing, bounds, imageType == CheckBoxImageType.ReadOnlyChecked);
					break;

				case CheckBoxImageType.AlternatingReadOnlyChecked:
				case CheckBoxImageType.AlternatingReadOnlyUnChecked:
					checkBoxForDrawing.BackColor = parentZGrid != null ? parentZGrid.ReadOnlyColorForRowNum(1) : SystemColors.Control;
					buttonBitmap = DrawCheckBoxToImage(checkBoxForDrawing, bounds, imageType == CheckBoxImageType.AlternatingReadOnlyChecked);
					break;

				case CheckBoxImageType.CustomBackColourChecked:
				case CheckBoxImageType.CustomBackColourUnChecked:
					checkBoxForDrawing.BackColor = parentZGrid != null ? customRowBackgroundColour : SystemColors.Control;
					buttonBitmap = DrawCheckBoxToImage(checkBoxForDrawing, bounds, imageType == CheckBoxImageType.CustomBackColourChecked);
					break;

				case CheckBoxImageType.SelectedChecked:
				case CheckBoxImageType.SelectedUnChecked:
					checkBoxForDrawing.BackColor = parentDataGrid.SelectionBackColor;
					buttonBitmap = DrawCheckBoxToImage(checkBoxForDrawing, bounds, imageType == CheckBoxImageType.SelectedChecked);
					break;

				case CheckBoxImageType.EditableBackColorChecked:
				case CheckBoxImageType.EditableBackColorUnChecked:
					checkBoxForDrawing.BackColor = EnterpriseFormLookStrategy.SelectedControlColor;
					buttonBitmap = DrawCheckBoxToImage(checkBoxForDrawing, bounds, imageType == CheckBoxImageType.EditableBackColorChecked);
					break;
			}

			if (buttonBitmap != null)
			{
				g.DrawImage(buttonBitmap, bounds.Location);
				buttonBitmap.Dispose();
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (checkBoxForDrawing != null)
			{
				checkBoxForDrawing.Dispose();
			}
		}

#endif

		protected void ColumnCheckEditEnter(object sender, EventArgs e)
		{
			ColumnCheckEdit.IsReadOnlyGrid = ReadOnly;
		}

		protected void ColumnCheckEditStateChanged(object sender, EventArgs e)
		{
			var oldValue = ColumnTextAtRow(sourceData, EditingRowNum);
			if (!EditValue.Equals(oldValue))
			{
				IsEditing = true;
				Commit(sourceData, EditingRowNum);
				if (parentZGrid != null)
				{
					CurrentText = GetCurrentText();
				}
				ColumnStartedEditing((Control)sender);
			}
		}

#if !WINZOR
		protected
#if DEBUG
		internal
#endif
		override void PaintText(Graphics graphics, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool isRightToLeft) { }

#endif

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			return CoerceToBool(propertyValue) ? Constants.BooleanTrueString : Constants.BooleanFalseString;
		}

#endregion
	}
}
