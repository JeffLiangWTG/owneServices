using System.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressControlRequiresTextBasher]
	[SuppressFormDesignerAnalysis]
	[SuppressFormsLocalizedTest]
	public partial class ZDropButtonOnly : ZButton, ISupportInitialize
	{
		public void BeginInit()
		{
		}

		public void EndInit()
		{
			var desiredWidth = ZGUISystemInformation.VerticalScrollBarWidth;
			Bounds = ControlDpiScalingHelper.NewScaledRectangle(Left, Top, desiredWidth, Height, false);
		}
	}
}
