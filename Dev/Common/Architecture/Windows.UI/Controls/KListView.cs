using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KListView : System.Windows.Forms.ListView
	{
		public KListView()
			: base()
		{
		}
	}
}
