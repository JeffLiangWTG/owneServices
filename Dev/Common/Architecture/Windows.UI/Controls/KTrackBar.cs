using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KTrackBar : System.Windows.Forms.TrackBar
	{
		public KTrackBar() : base() { }
	}
}
