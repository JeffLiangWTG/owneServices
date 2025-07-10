using System;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	interface IRimEnrolmentRequestProcessorFactory
	{
		IRimEnrolmentRequestProcessor GetProcessor(TimeSpan requestTimeout);
	}
}
