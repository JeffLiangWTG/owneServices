using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Integration
{
	public interface IHttpXmlMessageSaver
	{
		IHttpXmlRequestResponse RequestMessage { get; }
		IHttpXmlRequestResponse ResponseMessage { get; }
		BusinessObjectFactory Factory { get; }
		void Save(IHttpXmlProcessingResult processingResult);
		bool IsFinalProcessingAttempt { get; set; }
	}
}
