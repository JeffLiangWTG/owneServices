using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public interface IRegistryChangesNotifier
	{
		void Notify(string message);

		ZDialogResult ShowConfirmation(string message, string caption, string comfirmString, ZMessageBoxIcon messageBoxIcon);
	}
}
