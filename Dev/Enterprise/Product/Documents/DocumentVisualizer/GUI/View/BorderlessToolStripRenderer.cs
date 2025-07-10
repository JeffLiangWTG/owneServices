using System.Windows.Forms;

namespace Enterprise.DocumentVisualizer.GUI
{
#if !WINZOR
	sealed class BorderlessToolStripRenderer : ToolStripSystemRenderer
	{
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			// do not paint border
		}
	}
#endif
}
