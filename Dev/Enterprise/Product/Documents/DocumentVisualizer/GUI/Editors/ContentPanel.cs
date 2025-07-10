using System.Windows.Forms.Layout;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class ContentPanel : ZPanel
	{
		public override LayoutEngine LayoutEngine => layoutEngine ?? (layoutEngine = new ContentPanelLayoutEngine());
		ContentPanelLayoutEngine layoutEngine;
	}
}