using System.ComponentModel;
using System.Drawing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture
{
	public class ZHeaderLabel : ZLabel
	{
		public ZHeaderLabel()
		{
			this.Font = OFont.GetHeaderFont();
			this.ForeColor = Color.FromArgb(0, 52, 121);
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool IsFontBold
		{
			get; set;
		}
	}
}
