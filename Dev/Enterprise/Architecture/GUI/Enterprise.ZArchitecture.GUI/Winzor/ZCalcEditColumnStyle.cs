using System.Drawing;

namespace Enterprise.ZArchitecture 
{
	public partial class ZCalcEditColumnStyle
	{ 
		protected internal override void HideEditControl()
		{
			if (TextBox.Visible)
			{
				parentZGrid.IsHidingControl = true;
				TextBox.Bounds = Rectangle.Empty;
				parentZGrid.IsHidingControl = false;
			}
		}
	}
}
