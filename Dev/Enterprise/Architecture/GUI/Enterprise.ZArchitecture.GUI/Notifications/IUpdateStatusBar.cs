using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IUpdateStatusBar
	{
		void UpdateStatusBar(string notification, INotificationType state);
	}
}
