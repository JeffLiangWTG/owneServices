using System;
using System.Collections.Generic;
using Enterprise.Client.EDI.DeviceManagement.Business;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	public interface IRimEnrolmentRequestProcessor
	{
		bool TryProcessEnrolmentRequest(
			ClientDeviceHeader clientDeviceHeader,
			string schemeCode,
			string vin,
			string vehicleRegistration,
			string registrationState,
			string deviceLocation,
			DateTimeOffset installationDateTimeOffset,
			DateTimeOffset commencementDateTimeOffset,
			DateTimeOffset approvalDateTimeOffset,
			out string message);

		bool TryProcessEnrolmentCancellation(
			IEnumerable<ClientTelRimRegistration> selectedRegistrations,
			DateTimeOffset cancellationTime,
			out string message);
	}
}
