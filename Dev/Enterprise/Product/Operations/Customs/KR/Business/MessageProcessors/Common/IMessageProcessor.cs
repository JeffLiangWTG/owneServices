namespace Enterprise.Customs.KR.Business
{
	public interface IMessageProcessor
	{
		void Process(EDIMessage message);
	}
}
