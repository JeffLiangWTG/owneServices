using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KToolBar : System.Windows.Forms.ToolBar
	{
		public KToolBar() : base() { }
	}
}
