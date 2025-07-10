using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KMonthCalendar : System.Windows.Forms.MonthCalendar
	{
		public KMonthCalendar() : base() { }
	}
}
