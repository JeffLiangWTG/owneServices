using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	class DummyNotificationWithMessage : INotification
	{
		public INotificationType Type => null;
		public string Message => "some error message";

		public INotification ReplaceMessage(string message) => null;
	}
}
