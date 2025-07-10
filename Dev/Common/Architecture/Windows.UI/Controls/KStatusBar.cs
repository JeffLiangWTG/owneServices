using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KStatusBar : System.Windows.Forms.StatusBar
	{
		public KStatusBar() : base() { }
	}
}
