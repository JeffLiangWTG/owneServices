namespace Enterprise.Client.EDI.Messaging
{
	using Enterprise.Messaging.Module;

	public class EDIEDIInterchangeFilterBusinessObject : EDIInterchangeFilterBusinessObject
	{
		protected override bool AreSystemMessagesHidden
		{
			get { return false; }
		}
	}
}
