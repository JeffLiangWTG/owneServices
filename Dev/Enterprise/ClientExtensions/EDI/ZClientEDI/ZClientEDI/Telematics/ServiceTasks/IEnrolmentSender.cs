using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Telematics.Tca;

namespace Enterprise.Client.EDI.Telematics.ServiceTasks
{
	interface IEnrolmentSender
	{
		void Send(BusinessObjectFactory factory, EnrolmentReportType enrolments);
	}
}
