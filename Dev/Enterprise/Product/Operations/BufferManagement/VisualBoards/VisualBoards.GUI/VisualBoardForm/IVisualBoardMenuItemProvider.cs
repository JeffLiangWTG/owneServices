using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.VisualBoards.GUI
{
	public interface IVisualBoardMenuItemProvider
	{
		IEnumerable<MenuItem> GetMenuItems();
	}
}
