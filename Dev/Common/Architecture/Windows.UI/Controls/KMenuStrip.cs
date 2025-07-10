using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KMenuStrip : System.Windows.Forms.MenuStrip
	{
		public KMenuStrip()
			: base()
		{
		}
	}
}
