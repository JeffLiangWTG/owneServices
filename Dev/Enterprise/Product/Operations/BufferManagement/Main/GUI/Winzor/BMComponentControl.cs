using WinzorFramework;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMComponentControl : ComponentUserControl
	{
		protected override EventAttribute EventAttributes => base.EventAttributes | EventAttribute.ContextMenu;
	}
}
