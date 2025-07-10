using System.Windows.Forms;

namespace Enterprise.VisualBoards.GUI
{
	public interface IHotkeyHandler
	{
		bool ShouldHandle(Keys pressedKeys);
		void HandleHotkeys(Keys pressedKeys);
	}
}
