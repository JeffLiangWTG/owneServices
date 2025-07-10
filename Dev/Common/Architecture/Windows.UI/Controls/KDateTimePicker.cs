using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KDateTimePicker : System.Windows.Forms.DateTimePicker
	{
		public KDateTimePicker() : base() { }
	}
}
