using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class ZDropButton
	{
		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in ZDropButton.razor")]
		protected Rectangle ButtonBounds
		{
			get
			{
				var buttonRectangle = ButtonRectangle;
				if (IsOnGrid)
				{
					ControlDpiScalingHelper.SetHeight(ref buttonRectangle, buttonRectangle.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				}
				return buttonRectangle;
			}
		}
	}
}
