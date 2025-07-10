using System.ComponentModel.Design.Serialization;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KRichTextBox : System.Windows.Forms.RichTextBox
	{
		public KRichTextBox() : base()
		{
		}
	}
}
