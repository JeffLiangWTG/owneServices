using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.DataTransfer.Common.GUI.Import
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class WhiteTextBox : TextBox
	{
		public override Color BackColor
		{
			get { return SystemColors.Window; }
			set { }
		}
	}
}