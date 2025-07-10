using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KFlowLayoutPanel : System.Windows.Forms.FlowLayoutPanel
	{
		public KFlowLayoutPanel() : base() { }
	}
}
