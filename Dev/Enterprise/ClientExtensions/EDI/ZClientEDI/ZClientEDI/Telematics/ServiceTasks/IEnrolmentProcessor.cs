using System;
using System.Collections.Generic;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Telematics.Tca;

namespace Enterprise.Client.EDI.Telematics.ServiceTasks
{
	interface IEnrolmentProcessor
	{
		EnrolmentReportType Process(IEnumerable<ClientTelRimRegistration> registrations, DateTimeOffset enrollmentPeriodStart, DateTimeOffset enrollmentPeriodEnd);
	}
}
