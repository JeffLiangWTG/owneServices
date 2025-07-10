namespace Enterprise.Messaging.Integration
{
	public interface IEAdaptorSupportMessageSender
	{
		string Send(string message);
		string SendInRollbackMode(string message);
	}
}
