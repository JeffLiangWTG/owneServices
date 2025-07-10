using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Integration;

namespace Enterprise.Messaging.Integration
{
	public interface IHttpXmlMessageHandler
	{
		IHttpXmlProcessingConfig ProcessingConfig { get; }

		IHttpXmlRequestResponse CreateRequestMessage(DisposableManager disposableManager = null);
		IHttpXmlRequestResponse CreateResponseMessage(DisposableManager disposableManager = null);
		IHttpXmlProcessingResult Process(IHttpXmlRequestResponse requestMessage, IHttpXmlMessageSaver messageSaver = null);
		IHttpXmlProcessingResult Process(SubStreamableStream stream, IHttpXmlRequestResponse requestMessage, IHttpXmlMessageSaver messageSaver = null);
	}
}
