using System;
using CargoWise.Common.ErrorManagement;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[Serializable]
	class IncidentInvalidTimestampException : Exception, IErrorReporterExtender
	{
		public IncidentInvalidTimestampException(string message, SupportIncident incident)
			: base(AddIncidentNumberToMessage(message, incident))
		{
		}

#if NETFRAMEWORK
		protected IncidentInvalidTimestampException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		static string AddIncidentNumberToMessage(string message, SupportIncident incident)
		{
			return incident.IM_IncidentNumber + message;
		}
		public bool ShouldReportAlwaysInReportOnce => true;
	}
}
