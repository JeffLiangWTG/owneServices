namespace Enterprise.Customs.IN.Business;

public interface IEmailMessageProcessorsProvider
{
	IEmailMessageProcessor[] GetMessageProcessors();
}
