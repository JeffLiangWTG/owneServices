using System.ComponentModel;
using System.Drawing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal class ZTextBoxWithFont : ZTextBox
	{
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override Font Font
		{
			get { return base.Font;  }
			set { base.Font = value; }
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(false)]
		public new bool ReadOnly
		{
			get { return base.ReadOnly;  }
			set { base.ReadOnly = value; }
		}
	}
}