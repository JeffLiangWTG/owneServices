using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
#if !WINZOR
	[DesignerSerializer(typeof(TestControlCodeDomSerializerWithDelayedTabCreate), typeof(CodeDomSerializer))]
	class ControlCodeDomSerializerWithDelayedTabCreate_BaseTestForm_Base : Form
	{
	}
#endif
}
