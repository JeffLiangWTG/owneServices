namespace Enterprise.Customs.FR.Business.MessageSending;

public interface IMessageSender
{
	bool SendAndThrowExceptionIfAny(out string result);

	bool Send(bool shouldDelaySave, out string result);
}
