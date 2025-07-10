using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace Enterprise.DocumentVisualizer.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public sealed class PagesLayoutPanel : Panel
	{
		public PagesLayoutPanel()
		{
			BackColor = Color.Gray;
			layoutEngine = new PageLayoutEngine(this);
		}

		readonly PageLayoutEngine layoutEngine;

		public override LayoutEngine LayoutEngine => layoutEngine;

		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			e.Control.Visible = false;
			e.Control.Name = $"page{Controls.Count + 1}";
		}

		public void NotifyPagesCreated()
		{
			AutoScroll = true;
			layoutEngine.PerformLayout();
#if !WINZOR
			Scroll += (s,e) => layoutEngine.HandleScroll();
			MouseWheel += (s,e) => layoutEngine.HandleScroll();
#endif
		}

		public void NotifyZoomChanged()
		{
			layoutEngine.PerformLayout();
		}
	}
}
