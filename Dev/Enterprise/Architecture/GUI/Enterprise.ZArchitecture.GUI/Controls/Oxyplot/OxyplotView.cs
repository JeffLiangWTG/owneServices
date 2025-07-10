using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace Enterprise.ZArchitecture.GUI
{
#if !WINZOR
	[ToolboxItem(false)]
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class OxyplotView : OxyPlot.WindowsForms.PlotView
	{
	}
#else
	public partial class OxyplotView : System.Windows.Forms.Control
	{
		public OxyplotView()
		{
		}

		protected override System.Drawing.Size DefaultSize => new System.Drawing.Size(800, 300);
	}
#endif
}
