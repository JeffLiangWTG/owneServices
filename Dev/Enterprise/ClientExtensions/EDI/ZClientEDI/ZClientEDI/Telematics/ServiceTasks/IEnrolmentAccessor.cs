using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.DeviceManagement.Business;

namespace Enterprise.Client.EDI.Telematics.ServiceTasks
{
	interface IEnrolmentAccessor
	{
		IEnumerable<ClientTelRimRegistration> GetEnrollments(IFactory factory, DateTimeOffset enrollmentPeriodStart, DateTimeOffset enrollmentPeriodEnd);
	}
}
