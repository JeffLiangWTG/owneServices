using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZDynamicMultilineTextBoxColumnStyleInfo : ZTextBoxColumnStyleInfo
	{
		public ZDynamicMultilineTextBoxColumnStyleInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		public ZDynamicMultilineTextBoxColumnStyleInfo()
		{
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZDynamicMultilineTextBoxColumnStyle); }
		}
	}

	public class ZDynamicMultilineTextBoxColumnStyle : ZCustomControlColumnStyle
	{
		public ZDynamicMultilineTextBoxColumnStyle(ZTextBoxColumnStyleInfo columnInfo)
			: base(() => new ZTextBox(), columnInfo)
		{
			this.columnInfo = columnInfo;
		}
		readonly ZTextBoxColumnStyleInfo columnInfo;

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var zTextBox = (ZTextBox)control;
			zTextBox.CharacterCasing = columnInfo.CharacterCasing;
			zTextBox.IsDynamicMultiline = true;
		}

		ZTextBox ZTextBox => (ZTextBox)EditControl;

		protected override object EditValue
		{
			get { return (ZString)ZTextBox.Text; }
		}

		protected override void PrepareControlData(CurrencyManager source, int rowNum)
		{
			base.PrepareControlData(source, rowNum);
			ZTextBox.Text = GetColumnValueAtRow(source, rowNum).ToString();
		}

		protected override bool ShouldColumnHandleKey(Keys keyData)
		{
			return base.ShouldColumnHandleKey(keyData) || keyData == Keys.Tab;
		}
	}
}
