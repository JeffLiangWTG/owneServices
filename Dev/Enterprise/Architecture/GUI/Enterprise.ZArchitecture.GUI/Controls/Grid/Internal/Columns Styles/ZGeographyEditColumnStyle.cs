using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	#region ZGeographyEditColumnStyleInfo class

	public class ZGeographyEditColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfo
	{
		public ZGeographyEditColumnStyleInfo() { } // required for ZGrid column designer

		public ZGeographyEditColumnStyleInfo(string columnName, int width) : base(columnName, width)
		{
		}

		[ZColumnBindingMemberType(typeof(ZGeography))]
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set { base.ColumnName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZGeographyEditColumnStyle); }
		}
	}

	public class ZGeographyEditColumnStyle : ZCustomControlColumnStyle
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZGeographyEditColumnStyle(ZGeographyEditColumnStyleInfo columnInfo)
			: base(() => new ZGeographyEdit.Bare(), columnInfo)
		{
			FormatInfo = null;
		}

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var editControl = (ZGeographyEdit)control;
			editControl.IsOnGrid = true;
			editControl.ReadOnly = false;
		}

		protected override void HookControlEvents()
		{
			base.HookControlEvents();
			ZGeographyEditControl.GeographyValueChanged += ColumnTextBoxChanged;
		}

		protected override void UnHookControlEvents()
		{
			base.UnHookControlEvents();
			ZGeographyEditControl.GeographyValueChanged -= ColumnTextBoxChanged;
		}

		protected ZGeographyEdit ZGeographyEditControl => (ZGeographyEdit)EditControl;

		protected override object EditValue
		{
			get { return SelectedZGeography; }
		}

		ZGeography SelectedZGeography
		{
			get { return ZGeographyEditControl.GeographyValue; }
		}

		protected override string FormatValueObjectCore(object source, object propertyValue)
		{
			return ZGeographyToString(propertyValue);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		protected override bool Commit(CurrencyManager source, int rowNum)
		{
			ZGeographyEditControl.PushValue();
			var result = base.Commit(source, rowNum);

			if (source != null && source.Position == rowNum)
			{
				var currentEditValue = ConvertToZGeography(GetColumnValueAtRow(source, rowNum));

				if (currentEditValue.HasValue && !currentEditValue.Value.IsValid && !currentEditValue.Value.IsEmpty)
				{
					ZGeography val;
					ZGeography.TryParse(ZGeographyEditControl.Text, out val);
					if (!val.IsValid && !val.IsEmpty)
					{
						SaveInvalidCodeOnInfo(source, rowNum, ZGeographyEditControl.Text);
					}
				}
			}

			return result;
		}

		protected override void SetCurrentEditValue(CurrencyManager source, int rowNum, object currentEditValue)
		{
			SetColumnValueAtRow(source, rowNum, currentEditValue);
		}

		protected override void PrepareEditControl(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
		{
			base.PrepareEditControl(source, rowNum, bounds, readOnly);
			if (!IsEditing && source.Position == rowNum)
			{
				var currentEditValue = ConvertToZGeography(GetColumnValueAtRow(source, rowNum));

				if (currentEditValue.HasValue && !currentEditValue.Value.IsValid && !currentEditValue.Value.IsEmpty)
				{
					//CurrentText = GetInvalidCodeFromInfo(source, rowNum);
					GridControl.Text = GetInvalidCodeFromInfo(source, rowNum);
				}
				else
				{
					var currentEditValueString = ZGeographyToString(currentEditValue);
					if (GridControl.Text != currentEditValueString)
					{
						GridControl.Text = currentEditValueString;
					}
				}
			}
		}

		string ZGeographyToString(object geography1)
		{
			var geography = ConvertToZGeography(geography1);

			return geography.HasValue ? geography.Value.ToString().ToUpperInvariant() : string.Empty;
		}

		ZGeography? ConvertToZGeography(object valueObj)
		{
			var result = valueObj as ZGeography?;

			if (valueObj != null && !result.HasValue)
			{
				var converter = ZGeographyTypeConverter.Instance;
				if (converter.CanConvertFrom(valueObj.GetType()))
				{
					result = (ZGeography)converter.ConvertFrom(valueObj);
				}
			}

			return result;
		}
	}

	#endregion
}
