namespace Enterprise.Client.EDI.DevTools.Events
{
	public sealed class UserEvent
	{
		public EventType Type { get; set; }
		public string GroupName { get; set; }
		public string UserEmailAddress { get; set; }
	}
}
