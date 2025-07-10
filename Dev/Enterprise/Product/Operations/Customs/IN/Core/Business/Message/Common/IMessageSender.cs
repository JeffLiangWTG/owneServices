namespace Enterprise.Customs.IN.Business;

public interface IMessageSender
{
	public EDIMessage Send(MessageSendingContext context);
}
