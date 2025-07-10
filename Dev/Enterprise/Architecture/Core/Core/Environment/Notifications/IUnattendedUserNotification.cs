namespace Enterprise.ZArchitecture.Environment
{
	public interface IUnattendedUserNotification
	{
		void ShowInfrastructureError(string message, string caption);
	}
}
