namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public abstract class NCTSMessageDataObjectTest<T, TMessage> : FREDIMessageDataObjectTest<T, TMessage, NCTSFREDIMessage>
		where T : NCTSMessageDataObject<TMessage>
		where TMessage : class
	{
	}
}
