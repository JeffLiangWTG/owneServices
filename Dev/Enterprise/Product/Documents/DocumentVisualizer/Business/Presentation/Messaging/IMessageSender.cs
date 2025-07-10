namespace Enterprise.DocumentVisualizer.Presentation
{
	interface IMessageSender<in T>
	{
		bool Send(T parameters);
	}
}