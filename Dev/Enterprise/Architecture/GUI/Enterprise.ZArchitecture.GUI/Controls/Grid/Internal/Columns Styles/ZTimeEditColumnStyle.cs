using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	#region Time Edit Column Style Info

	public class ZTimeEditColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfo
	{
		public ZTimeEditColumnStyleInfo() // required for the ZGrid column designer
		{
		}

		public ZTimeEditColumnStyleInfo(string columnName, int width) : base(columnName, width)
		{
		}

		[ZColumnBindingMemberType(typeof(ZTime))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		public bool AllowNegative
		{
			get { return allowNegative; }
			set { allowNegative = value; }
		}
		bool allowNegative;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZTimeEditColumnStyle); }
		}
	}

	#endregion

	#region Column Style

	public partial class ZTimeEditColumnStyle : ZCustomControlColumnStyle, IZColumn, IZDateTimePickerFormat
	{
		public ZTimeEditColumnStyle(ZTimeEditColumnStyleInfo columnInfo) : this(() => new ZTimeTimeEditEx.Bare(), columnInfo)
		{
		}

		protected ZTimeEditColumnStyle(Func<Control> editControl, ZTextBoxColumnStyleInfo columnInfo) : base(editControl, columnInfo) { }

		#region Handling Input, Preparing Edit Control etc.

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			var isReadOnly = IsCellReadOnly(source, rowNum);

			if (isReadOnly)
			{
				ControlDpiScalingHelper.SetX(ref bounds, bounds.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
			}

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);

			if (isReadOnly)
			{
				TextBox.Text = GetTextFromTimeWithColonIfEmpty(source, rowNum);
				if (TextBox.Text != Core.EmptyText)
				{
					TextBox.SelectAll();
				}
				else
				{
					TextBox.Text = String.Empty;
				}
			}
		}

		protected override bool Commit(CurrencyManager source, int rowNum)
		{
			if (IsEditing)
			{
				if (source != null && source.Position == rowNum)
				{
					var existingValue = GetTime(source, rowNum);
					var currentEditValue = (ZTime)EditValue;

					if (existingValue != currentEditValue)
					{
						SetColumnValueAtRow(source, rowNum, currentEditValue);
					}

					if (!currentEditValue.IsValid)
					{
						SaveInvalidCodeOnInfo(source, rowNum, GetCurrentText());
					}
				}

				IsEditing = false;
			}

			return true;
		}

		protected override bool ShouldColumnHandleKey(Keys keyData)
		{
			return
				keyData == (Keys.Alt | Keys.Up) ||
				keyData == (Keys.Alt | Keys.Down) ||
				keyData == (Keys.Control | Keys.Up) ||
				keyData == (Keys.Control | Keys.Down) ||
				base.ShouldColumnHandleKey(keyData);
		}

		protected override object EditValue
		{
			get { return Core.GetTimeFromText(GetCurrentText()); }
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			ControlDpiScalingHelper.SetX(ref bounds, bounds.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
			ControlDpiScalingHelper.SetY(ref bounds, bounds.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
			EditControl.AllowNegative = ColumnInfo.AllowNegative;
			base.PrepareEditControl(source, rowNum, bounds, readOnly);

			if (!IsEditing)
			{
				GridControl.Text = GetTextFromTime(source, rowNum);
			}
		}

		protected ZTime GetTime(CurrencyManager source, int rowNum)
		{
			return (ZTime)GetColumnValueAtRow(source, rowNum);
		}

		protected virtual string GetTextFromTime(CurrencyManager source, int rowNum)
		{
			var currentTime = GetTime(source, rowNum);
			return currentTime.IsValid || currentTime.IsEmpty
				? Core.GetTextFromTime(currentTime, ColumnInfo.AllowNegative)
				: GetInvalidCodeFromInfo(source, rowNum);
		}

		protected override string GetInvalidCodeFromInfo(CurrencyManager source, int rowNum)
		{
			var result = GetInvalidCodeFromInfo(source.List[rowNum]);
			if (string.IsNullOrEmpty(result))
			{
				result = ZTime.InvalidLiteral.ToUpperInvariant();
			}
			return result;
		}

		protected virtual string GetTextFromTimeWithColonIfEmpty(CurrencyManager source, int rowNum)
		{
			var result = GetTextFromTime(source, rowNum);

			if (string.IsNullOrEmpty(result))
			{
				result = Core.EmptyText;
			}

			return result;
		}

		new ZTimeEditColumnStyleInfo ColumnInfo
		{
			get { return (ZTimeEditColumnStyleInfo)base.ColumnInfo; }
		}

		new ZTimeTimeEdit EditControl
		{
			get { return (ZTimeTimeEdit)base.EditControl; }
		}

		#endregion

		#region Core

		internal ZTimeTimeEditCore Core
		{
			get { return EditControl.Core; }
		}

		#endregion

		#region Painting

		#if !WINZOR

		protected
#if DEBUG
		internal
#endif
		override void PaintText(Graphics graphics, Rectangle bounds, CurrencyManager source, int rowNum, string cellText, Font cellFont, Brush backBrush, Brush foreBrush, bool isRightToLeft)
		{
			cellText = CurrentCellText;
			base.PaintText(graphics, bounds, source, rowNum, cellText, cellFont, backBrush, foreBrush, isRightToLeft);
		}

		protected override void Paint(Graphics graphics, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool isAlignedToRight)
		{
			CurrentCellText = GetTextFromTimeWithColonIfEmpty(source, paintingRowNum);
			base.Paint(graphics, bounds, source, paintingRowNum, backBrush, foreBrush, isAlignedToRight);
		}

		protected string CurrentCellText;

		#endif

		#endregion

		#region IZDateTimePickerFormat Members

		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes; }
		}

		#endregion
	}

	#endregion
}
