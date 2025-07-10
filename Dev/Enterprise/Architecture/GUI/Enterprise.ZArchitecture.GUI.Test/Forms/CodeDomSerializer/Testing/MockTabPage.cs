using System;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
#if !WINZOR
	[DesignerSerializer(typeof(ControlCodeDomSerializerWithDelayedTabCreate), typeof(CodeDomSerializer))]
	sealed class MockTabPage : TabPage
	{
		public void RunWhenBindingOrFirstShown(EventHandler handler)
		{
			handler(this, EventArgs.Empty);
		}
	}
#endif
}
