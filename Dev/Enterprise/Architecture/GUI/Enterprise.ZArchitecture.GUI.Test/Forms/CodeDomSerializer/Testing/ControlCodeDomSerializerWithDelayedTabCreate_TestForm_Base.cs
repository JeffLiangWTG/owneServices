using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
#if !WINZOR
	[DesignerSerializer(typeof(TestControlCodeDomSerializerWithDelayedTabCreate), typeof(CodeDomSerializer))]
	class ControlCodeDomSerializerWithDelayedTabCreate_TestForm_Base : ControlCodeDomSerializerWithDelayedTabCreate_BaseTestForm
	{
		[SuppressMessage("CargoWiseOne", "CW1042", Justification = "Mock form for testing")]
		public ControlCodeDomSerializerWithDelayedTabCreate_TestForm_Base()
		{
			#region Instructions

			var label = new Label();
			label.Text = @"
Follow the instructions below to test the ControlCodeDomSerializerWithDelayedTabCreate:

1. Make sure ZArchitecture is built.
2. Move a control below 1 pixel up then 1 pixel down.
3. Right-click and choose 'View Code'.
4. Compare with Source Control for the file that contains the InitializeComponent() method.
";
			Controls.Add(label);
			label.Visible = true;
			label.Location = new Point(10, 0);
			label.Width = 1000;
			label.Height = 120;

			#endregion
		}
	}
#endif
}
