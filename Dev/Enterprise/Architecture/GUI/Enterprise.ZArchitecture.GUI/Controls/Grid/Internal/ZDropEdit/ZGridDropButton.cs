using System.ComponentModel;
using System.Drawing;

using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public class ZGridDropButton : ZDropButton
	{
		protected internal bool IsVisible
		{
			get { return visible;  }
			set { visible = value; }
		}

		protected override Point ButtonPoint
		{
			get { return ControlDpiScalingHelper.NewScaledPoint(ClientRectangle.Right - ZGUISystemInformation.VerticalScrollBarWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), ClientRectangle.Top, false); }
		}

		protected override Size ButtonSize
		{
			get { return IsVisible ? ControlDpiScalingHelper.NewScaledSize(ZGUISystemInformation.VerticalScrollBarWidth, ClientRectangle.Height, false) : ControlDpiScalingHelper.NewScaledSize(0, 0); }
		}

		protected override bool ShouldDrawBorder
		{
			get { return false; }
		}

		public override bool IsOnGrid
		{
			get { return true; }
			set { }
		}

		bool visible = true;
	}
}
