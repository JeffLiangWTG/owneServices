namespace Enterprise.Messaging.GUI
{
	public interface ILogger
	{
		void Notify(string text, bool isError);
	}
}
