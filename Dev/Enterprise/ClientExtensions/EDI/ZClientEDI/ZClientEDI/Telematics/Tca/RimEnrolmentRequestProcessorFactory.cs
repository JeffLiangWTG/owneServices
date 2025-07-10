using System;
using CargoWise.Application;
using Enterprise.Telematics.ServiceTasks;
using WTG.Foundation.Http;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	class RimEnrolmentRequestProcessorFactory : IRimEnrolmentRequestProcessorFactory
	{
		public IRimEnrolmentRequestProcessor GetProcessor(TimeSpan requestTimeout)
		{
			return new RimEnrolmentRequestProcessor(ObjectFactory.Get<IHttpClientFactory>(), new EHubMessageSender(), requestTimeout);
		}
	}
}
