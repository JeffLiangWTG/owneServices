using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	#region interface IZDateTimePickerFormat

	interface IZDateTimePickerFormat
	{
		ZDateTimePickerFormat DateTimeFormat
		{
			get;
		}
	}

	#endregion

	public class ZDateEditColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfo
	{
		public ZDateEditColumnStyleInfo() { } // required for ZGrid column designer

		public ZDateEditColumnStyleInfo(string columnName, int width) : this(columnName, width, ZDateTimePickerFormat.Short) { }

		public ZDateEditColumnStyleInfo(string columnName, int width, ZDateTimePickerFormat dateTimeFormat) : base(columnName, width)
		{
			this.DateTimeFormat = dateTimeFormat;
		}

		[ZColumnBindingMemberType(typeof(ZDateTime))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZDateEditColumnStyle); }
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
	}

	public class ZDateEditColumnStyle : ZCustomControlColumnStyle, IZDateTimePickerFormat
	{
		public ZDateEditColumnStyle(ZDateEditColumnStyleInfo columnInfo)
			: base(() => new ZDateEdit.Bare(), columnInfo)
		{
			FormatInfo = null;
			this.columnInfo = columnInfo;
		}
		readonly ZDateEditColumnStyleInfo columnInfo;

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var dateControl = (ZDateEdit)control;
			dateControl.IsOnGrid = true;
			dateControl.DateTimeFormat = columnInfo.DateTimeFormat;
			dateControl.ReadOnly = false;
		}

		protected override void HookControlEvents()
		{
			base.HookControlEvents();
			ZDateEditControl.DateTimeValueChanged += ColumnTextBoxChanged;
		}

		protected override void UnHookControlEvents()
		{
			base.UnHookControlEvents();
			ZDateEditControl.DateTimeValueChanged -= ColumnTextBoxChanged;
		}

		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return ZDateEditControl.DateTimeFormat; }
			set { ZDateEditControl.DateTimeFormat = value; }
		}

		protected ZDateEdit ZDateEditControl => (ZDateEdit)EditControl;

		protected override object EditValue
		{
			get { return SelectedZDate; }
		}

		ZDateTime SelectedZDate
		{
			get { return ZDateEditControl.DateTimeValue; }
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if (!IsEditing)
			{
				Format = ZDateEditControl.FormatString;
			}

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
		}

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			return ZDateTimeToString(propertyValue);
		}

		protected override bool Commit(CurrencyManager source, int rowNum)
		{
			ZDateEditControl.PushValue();
			var result = base.Commit(source, rowNum);

			if (source != null && source.Position == rowNum)
			{
				var currentEditValue = ConvertToZDateTime(GetColumnValueAtRow(source, rowNum));

				if (currentEditValue.HasValue && !currentEditValue.Value.IsValid && !currentEditValue.Value.IsEmpty)
				{
					ZDateTime val;
					ZDateTime.TryParseExact(ZDateEditControl.Text, out val, ZDateEditControl.FormatString);
					if (!val.IsValid && !val.IsEmpty)
					{
						SaveInvalidCodeOnInfo(source, rowNum, ZDateEditControl.Text);
					}
				}
			}

			return result;
		}

		protected override void SetCurrentEditValue(CurrencyManager source, int rowNum, object currentEditValue)
		{
			var bizObj = source.GetCurrent() as BusinessObject;
			var info = ZPropertyInfoRetriever.GetZPropertyInfo(this, bizObj);

			if (info != null && info.PropertyType == typeof(ZDate))
			{
				SetColumnValueAtRow(source, rowNum, new ZDate(currentEditValue));
			}
			else
			{
				SetColumnValueAtRow(source, rowNum, currentEditValue);
			}
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			base.PrepareEditControl(source, rowNum, bounds, readOnly);
			if (!IsEditing && source.Position == rowNum)
			{
				var currentEditValue = ConvertToZDateTime(GetColumnValueAtRow(source, rowNum));

				if (currentEditValue.HasValue)
				{
					if (!currentEditValue.Value.IsValid && !currentEditValue.Value.IsEmpty)
					{
						//CurrentText = GetInvalidCodeFromInfo(source, rowNum);
						GridControl.Text = GetInvalidCodeFromInfo(source, rowNum);
					}
					else
					{
						ZDateEditControl.DateTimeValue = currentEditValue.Value;
					}
				}
				else
				{
					var currentEditValueString = ZDateTimeToString(currentEditValue);
					if (GridControl.Text != currentEditValueString)
					{
						GridControl.Text = currentEditValueString;
					}
				}
			}
		}
#if DEBUG
		internal
#endif
		string ZDateTimeToString(object date1)
		{
			var date = ConvertToZDateTime(date1);

			return date.HasValue ? date.Value.ToString(ZDateEditControl.FormatString).ToUpper() : string.Empty;
		}

		// To support possible use with ZDate columns
		internal static ZDateTime? ConvertToZDateTime(object valueObj)
		{
			var result = valueObj as ZDateTime?;

			if (valueObj != null && !result.HasValue)
			{
				var converter = ZDateTimeTypeConverter.Instance;
				if (converter.CanConvertFrom(valueObj.GetType()))
				{
					result = (ZDateTime)converter.ConvertFrom(valueObj);
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
