using System.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal class MonthYearEdit : ZCalcEdit
	{
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool IsCalculatorEnabled => false;

		public new bool Multiline => false;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool ShowGroupSeparators => base.ShowGroupSeparators;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int Decimals => base.Decimals;

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				if (value == null || value == "0")
				{
					base.Text = string.Empty;
				}
				else
				{
					base.Text = value.PadLeft(MaxLength, '0');
				}
			}
		}

		public MonthYearEdit()
		{
			base.Decimals = 0;
			base.ShowGroupSeparators = false;
		}
	}
}
