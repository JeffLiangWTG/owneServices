using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KPictureBox : System.Windows.Forms.PictureBox
	{
		public KPictureBox()
			: base()
		{
		}
	}
}
