using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KTableLayoutPanel : System.Windows.Forms.TableLayoutPanel
	{
		public KTableLayoutPanel()
			: base()
		{
		}
	}
}
