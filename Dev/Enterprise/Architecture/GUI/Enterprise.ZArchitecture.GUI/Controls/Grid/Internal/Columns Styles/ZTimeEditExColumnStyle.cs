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

	public class ZTimeEditExColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfo
	{
		public ZTimeEditExColumnStyleInfo() // required for the ZGrid column designer
		{
		}

		public ZTimeEditExColumnStyleInfo(string columnName, int width) : base(columnName, width)
		{
		}

		[ZColumnBindingMemberType(typeof(ZDateTime))]
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
			get { return typeof(ZTimeEditExColumnStyle); }
		}
	}

	#endregion

	#region Column Style

	public partial class ZTimeEditExColumnStyle : ZCustomControlColumnStyle, IZColumn, IZDateTimePickerFormat
	{
		public ZTimeEditExColumnStyle(ZTimeEditExColumnStyleInfo columnInfo) : this(() => new ZTimeEditEx.Bare(), columnInfo)
		{
		}

		protected ZTimeEditExColumnStyle(Func<Control> editControl, ZTextBoxColumnStyleInfo columnInfo) : base(editControl, columnInfo) { }

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
				TextBox.Text = GetTextFromDateTimeWithColonIfEmpty(source, rowNum);
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
					var existingValue = GetDateTime(source, rowNum);
					var currentEditValue = (ZDateTime)EditValue;

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
				GridControl.Text = GetTextFromDateTime(source, rowNum);
			}
		}

		protected ZDateTime GetDateTime(CurrencyManager source, int rowNum)
		{
			return (ZDateTime)GetColumnValueAtRow(source, rowNum);
		}

		protected virtual string GetTextFromDateTime(CurrencyManager source, int rowNum)
		{
			var currentDateTime = GetDateTime(source, rowNum);
#if NET
			if (currentDateTime.IsValid && !currentDateTime.IsEmpty && currentDateTime.Millisecond != currentDateTime.AddTicks(1).Millisecond)
			{
				// There could be a datetime precision difference between .NET Framework and .NET
				// When that happens, currentDateTime is 1 Tick less in Winzor than that in CW1
				// See WI00709297 for details.
				currentDateTime = currentDateTime.AddTicks(1);
			}
#endif
			return currentDateTime.IsValid || currentDateTime.IsEmpty
				? Core.GetTextFromTime(currentDateTime, ColumnInfo.AllowNegative)
				: GetInvalidCodeFromInfo(source, rowNum);
		}

		protected override string GetInvalidCodeFromInfo(CurrencyManager source, int rowNum)
		{
			var result = GetInvalidCodeFromInfo(source.List[rowNum]);
			if (string.IsNullOrEmpty(result))
			{
				result = ZDateTime.InvalidLiteral.ToUpperInvariant();
			}
			return result;
		}

		protected virtual string GetTextFromDateTimeWithColonIfEmpty(CurrencyManager source, int rowNum)
		{
			var result = GetTextFromDateTime(source, rowNum);

			if (string.IsNullOrEmpty(result))
			{
				result = Core.EmptyText;
			}

			return result;
		}

		new ZTimeEditExColumnStyleInfo ColumnInfo
		{
			get { return (ZTimeEditExColumnStyleInfo)base.ColumnInfo; }
		}

		new ZTimeEdit EditControl
		{
			get { return (ZTimeEdit)base.EditControl; }
		}

		#endregion

		#region Core

		internal ZTimeEditCore Core
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
			CurrentCellText = GetTextFromDateTimeWithColonIfEmpty(source, paintingRowNum);
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
