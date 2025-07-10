using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	public class ZDateTimeOffsetEditColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfo
	{
		public ZDateTimeOffsetEditColumnStyleInfo() { } // required for ZGrid column designer

		public ZDateTimeOffsetEditColumnStyleInfo(string columnName, int width) : this(columnName, width, ZDateTimePickerFormat.Short) { }

		public ZDateTimeOffsetEditColumnStyleInfo(string columnName, int width, ZDateTimePickerFormat dateTimeFormat) : base(columnName, width)
		{
			this.DateTimeFormat = dateTimeFormat;
		}

		[ZColumnBindingMemberType(typeof(ZDateTimeOffset))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZDateTimeOffsetEditColumnStyle); }
		}

		#region DateTimeFormat

		[DefaultValue(DefaultDateTimeFormat)]
		public ZDateTimePickerFormat DateTimeFormat
		{
			get
			{
				return fDateTimeFormat;
			}
			set
			{
				fDateTimeFormat = value;
			}
		}

		protected const ZDateTimePickerFormat DefaultDateTimeFormat = ZDateTimePickerFormat.Long;
		protected ZDateTimePickerFormat fDateTimeFormat = DefaultDateTimeFormat;

		#endregion

		#region HasTimeZoneFindBox

		[DefaultValue(DefaultHasTimeZoneFindBox)]
		public bool HasTimeZoneFindBox
		{
			get
			{
				return fHasTimeZoneFindBox;
			}
			set
			{
				fHasTimeZoneFindBox = value;
			}
		}

		protected const bool DefaultHasTimeZoneFindBox = false;
		protected bool fHasTimeZoneFindBox = DefaultHasTimeZoneFindBox;

		#endregion
	}

	public class ZDateTimeOffsetEditColumnStyle : ZCustomControlColumnStyle, IZDateTimePickerFormat
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", Justification = "Same as ZDateEditColumnStyle")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZDateTimeOffsetEditColumnStyle(ZDateTimeOffsetEditColumnStyleInfo columnInfo)
			: base(() => new ZDateTimeOffsetEdit.Bare(), columnInfo)
		{
			this.columnInfo = columnInfo;
		}
		readonly ZDateTimeOffsetEditColumnStyleInfo columnInfo;

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var dateTimeControl = (ZDateTimeOffsetEdit.Bare)control;
			dateTimeControl.DateTimeFormat = columnInfo.DateTimeFormat;
			dateTimeControl.HasTimeZoneFindBox = columnInfo.HasTimeZoneFindBox;
			FormatInfo = null;
			dateTimeControl.IsOnGrid = true;
			dateTimeControl.ReadOnly = false;
		}

		protected override void HookControlEvents()
		{
			base.HookControlEvents();
			ZDateTimeOffsetEditControl.DateTimeOffsetValueChanged += ColumnTextBoxChanged;
		}

		protected override void UnHookControlEvents()
		{
			base.UnHookControlEvents();
			ZDateTimeOffsetEditControl.DateTimeOffsetValueChanged -= ColumnTextBoxChanged;
		}

		[DefaultValue(false)]
		public bool HasTimeZoneFindBox
		{
			get { return ZDateTimeOffsetEditControl.HasTimeZoneFindBox; }
			set { ZDateTimeOffsetEditControl.HasTimeZoneFindBox = value; }
		}

		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return ZDateTimeOffsetEditControl.DateTimeFormat; }
			set { ZDateTimeOffsetEditControl.DateTimeFormat = value; }
		}

		protected
#if DEBUG
		internal
#endif
		ZDateTimeOffsetEdit ZDateTimeOffsetEditControl => (ZDateTimeOffsetEdit)EditControl;

		protected override object EditValue
		{
			get { return SelectedZDateTimeOffset; }
		}

		ZDateTimeOffset SelectedZDateTimeOffset
		{
			get { return ZDateTimeOffsetEditControl.DateTimeOffsetValue; }
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if (!IsEditing)
			{
				Format = ZDateTimeOffsetEditControl.FormatString;
			}

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
		}

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			return ZDateTimeOffsetEditControl.DateTimeOffsetToString(ConvertToZDateTimeOffSet(propertyValue));
		}

		protected override bool Commit(CurrencyManager source, int rowNum)
		{
			ZDateTimeOffsetEditControl.PushValue();
			var result = base.Commit(source, rowNum);

			if (source != null && source.Position == rowNum)
			{
				var currentEditValue = ConvertToZDateTimeOffSet(GetColumnValueAtRow(source, rowNum));

				if (currentEditValue.HasValue && !currentEditValue.Value.IsValid && !currentEditValue.Value.IsEmpty)
				{
					ZDateTimeOffset val;
					ZDateTimeOffset.TryParseExact(ZDateTimeOffsetEditControl.Text, out val, ZDateTimeOffsetEditControl.FormatString);
					if (!val.IsValid && !val.IsEmpty)
					{
						SaveInvalidCodeOnInfo(source, rowNum, ZDateTimeOffsetEditControl.Text);
					}
				}
			}

			return result;
		}

		protected override void SetCurrentEditValue(CurrencyManager source, int rowNum, object currentEditValue)
		{
			SetColumnValueAtRow(source, rowNum, new ZDateTimeOffset(currentEditValue));
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			base.PrepareEditControl(source, rowNum, bounds, readOnly);
			if (!IsEditing && source.Position == rowNum)
			{
				var currentEditValue = ConvertToZDateTimeOffSet(GetColumnValueAtRow(source, rowNum));

				if (currentEditValue.HasValue)
				{
					if (!currentEditValue.Value.IsValid && !currentEditValue.Value.IsEmpty)
					{
						//CurrentText = GetInvalidCodeFromInfo(source, rowNum);
						GridControl.Text = GetInvalidCodeFromInfo(source, rowNum);
					}
					else
					{
						ZDateTimeOffsetEditControl.DateTimeOffsetValue = currentEditValue.Value;
					}
				}
				else
				{
					var currentEditValueString = ZDateTimeOffsetEditControl.DateTimeOffsetToString(currentEditValue, true);
					if (GridControl.Text != currentEditValueString)
					{
						GridControl.Text = currentEditValueString;
					}
				}
			}
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

		ZDateTimeOffset? ConvertToZDateTimeOffSet(object valueObj)
		{
			var result = valueObj as ZDateTimeOffset?;

			if (valueObj != null)
			{
				var converter = ZDateTimeOffsetTypeConverter.Instance;
				if (converter.CanConvertFrom(valueObj.GetType()))
				{
					result = (ZDateTimeOffset)converter.ConvertFrom(valueObj);
				}
			}

			return result;
		}

		#region INavigatingGridColumn Members

		protected override bool ShouldColumnHandleKey(Keys keyData)
		{
			var result =
				keyData == (Keys.Control | Keys.Up) || keyData == (Keys.Control | Keys.Down) ||
				keyData == (Keys.Alt | Keys.Up) || keyData == (Keys.Alt | Keys.Down) ||
				keyData == (Keys.Control | Keys.Alt | Keys.Up) || keyData == (Keys.Control | Keys.Alt | Keys.Down) ||
				keyData == (Keys.Control | Keys.Left) || keyData == (Keys.Control | Keys.Right) ||
				keyData == Keys.Tab || keyData == (Keys.Shift | Keys.Tab) ||
				base.ShouldColumnHandleKey(keyData);
			return result || keyData == Keys.Tab;
		}

		#endregion
	}
}
