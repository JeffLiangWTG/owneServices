using CargoWise.Application;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors;

namespace Enterprise.Client.EDI.Telematics
{
	static class TelematicsExtension
	{
		public static void RegisterEdiProcessors()
		{
			var factory = ObjectFactory.Get<ITelematicsXmlMessageTypeProcessorsFactory>();
			factory.AddProcessorFunction(logger => new ServerRegistrationRequestMessageProcessor(logger));
		}
	}
}
