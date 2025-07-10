namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IUserNotifications
	{
		void ShowMessage(string message, string caption);
		bool ShowConfirmation(string message, string caption);
		bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString);
	}
}
