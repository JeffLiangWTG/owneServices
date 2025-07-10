using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	class DummyNotification : INotification
	{
		public INotificationType Type => null;
		public string Message => string.Empty;

		public INotification ReplaceMessage(string message) => null;
	}
}
