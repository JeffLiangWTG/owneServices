namespace Enterprise.Client.EDI.Messaging
{
	using Enterprise.Messaging.Module;

	public class EDIEDIMessageFilterBusinessObject : EDIMessageFilterBusinessObject
	{
		protected override bool AreSystemMessagesHidden
		{
			get { return false; }
		}
	}
}
