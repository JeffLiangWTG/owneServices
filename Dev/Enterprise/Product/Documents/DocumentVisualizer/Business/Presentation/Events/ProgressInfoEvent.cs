namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class ProgressInfoEvent
	{
		public ProgressInfoEvent(string message)
		{
			this.Message = message;
		}

		public string Message { get; private set; }
	}
}