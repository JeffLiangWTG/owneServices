using System.Drawing;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public interface INotificationColorProvider
	{
		Color GetColor();
		Color GetFontColor();
	}
}
