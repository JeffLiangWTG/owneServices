using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZTextLabelWhiteSpace : ZTextLabel
	{
		public ZTextLabelWhiteSpace(int width)
		{
			Width = width;
		}

		public override string Text
		{
			get { return base.Text; }
			set { throw new NotSupportedException("If you want a text label, use ZTextLabel instead."); }
		}
	}
}
