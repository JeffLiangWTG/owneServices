namespace Enterprise.Messaging.Business.Testing
{
	public class InboundMessageCreatorTestHelper : IInboundMessageCreator
	{
		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			interchange.EI_HeaderText = "TEST PROCESSED";
		}
	}
}
