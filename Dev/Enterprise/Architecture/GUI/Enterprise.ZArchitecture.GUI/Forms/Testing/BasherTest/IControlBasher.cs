#if DEBUG
using System.Windows.Forms;

using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public interface IControlBasher
	{
		void Bash(Control control, INotifications notifications);
	}
}
#endif
