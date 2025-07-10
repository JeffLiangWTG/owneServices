using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KDataGrid : System.Windows.Forms.DataGrid
	{
		public KDataGrid() : base() { }
	}
}
