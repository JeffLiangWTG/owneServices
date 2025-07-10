namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IMessageEventsProcessor
	{
		void OnMessageSent();
		void OnMessageWithdrawalSent();
		void OnResetToOriginal();
	}
}
