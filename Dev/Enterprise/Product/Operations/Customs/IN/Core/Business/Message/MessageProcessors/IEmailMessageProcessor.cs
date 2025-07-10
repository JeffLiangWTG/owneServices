using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IN.Business;

public interface IEmailMessageProcessor
{
	bool CanProcess(EmailInfo emailInfo);

	bool Process(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger);

	BusinessObject GetLinkedObject(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger);
}
