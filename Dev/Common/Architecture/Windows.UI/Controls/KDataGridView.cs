using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KDataGridView : System.Windows.Forms.DataGridView
	{
		public KDataGridView() : base() { }
	}
}
