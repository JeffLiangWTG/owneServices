using Enterprise.ZArchitecture.GUI;
using WinzorFramework;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ChannelHeaderControl : ZUserControl
	{
		protected override EventAttribute EventAttributes => EventAttribute.Click | EventAttribute.ContextMenu;
	}
}
