using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KChart : System.Windows.Forms.DataVisualization.Charting.Chart
	{
		public KChart() : base() { }
	}
}
