using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class RegistryChangesNotifier : IRegistryChangesNotifier
	{
		public void Notify(string message)
		{
			Globals.Message.ShowWarning(message);
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string comfirmString, ZMessageBoxIcon messageBoxIcon)
		{
			return Globals.Message.ShowConfirmation(message, caption, comfirmString, messageBoxIcon);
		}
	}
}
