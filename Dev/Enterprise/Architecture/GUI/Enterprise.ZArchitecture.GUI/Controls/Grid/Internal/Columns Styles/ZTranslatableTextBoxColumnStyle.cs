using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	public class ZTranslatableTextBoxColumnStyleInfo : ZTextBoxColumnStyleInfo, IZColumnStyleInfo, IOverridablePropertyDescriptor
	{
		public ZTranslatableTextBoxColumnStyleInfo() // required for ZGrid column designer
		{
		}

		public ZTranslatableTextBoxColumnStyleInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		public override Type ColumnStyleType
		{
			get { return typeof(ZTranslatableTextBoxColumnStyle); }
		}

		PropertyDescriptor IOverridablePropertyDescriptor.PropertyDescriptor { get; set; }
	}

	public class ZTranslatableTextBoxColumnStyle : ZCustomControlColumnStyle
	{
		public ZTranslatableTextBoxColumnStyle(ZTranslatableTextBoxColumnStyleInfo columnInfo)
			: base(() => new ZTranslatableTextControl() { CharacterCasing = columnInfo.CharacterCasing }, columnInfo)
		{ }

		new ZTranslatableTextControl EditControl
		{
			get { return (ZTranslatableTextControl)base.EditControl; }
		}

		protected override object EditValue
		{
			get { return (ZString)EditControl.Text; }
		}

		protected override bool EditControlShownForReadOnlyCore
		{
			get { return true; }
		}

		protected override void PrepareEditControl(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly)
		{
			base.PrepareEditControl(source, rowNum, bounds, readOnly);
			EditControl.Text = GetColumnValueAtRow(source, rowNum).ToString();
			EditControl.ReadOnly = readOnly;
		}

		protected override void PrepareControlData(System.Windows.Forms.CurrencyManager source, int rowNum)
		{
			base.PrepareControlData(source, rowNum);
			EditControl.GridMember = MappingName;
			EditControl.GridCurrent = source.GetCurrent();
		}
	}
}
