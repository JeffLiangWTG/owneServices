namespace Enterprise.Messaging.Integration
{
	public interface IXtMessageEventData
	{
		int EventIndex { get; }
		int LogEvent { get; }
		string LogText { get; }
		string LogTime { get; }
		ulong XtMsgId { get; }
	}
}
