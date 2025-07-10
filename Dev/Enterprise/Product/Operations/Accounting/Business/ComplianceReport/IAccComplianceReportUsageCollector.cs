using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	/// <summary>
	/// The using scope of an instance of this class determines both - the duration of an action and reporting the collected data.
	/// </summary>
	public interface IAccComplianceReportUsageCollector : IDisposable
	{
		void AddChangedStatus(string oldStatus, string newStatus);
		void AddStatusMessage(string statusMessage);
		void AddAction(AccComplianceReportUsageCollectorAction action);
		void AddContext(AccComplianceReportUsageCollectorContext context);
		/// <summary>
		/// A new GUID shall be created for each run of the CRQ service task to be collected by the usage collector.
		/// </summary>
		/// <param name="sessionId"></param>
		void AddSessionId(Guid sessionId);
		void AddLogonUser(IUser user);
		IEnumerable<(string name, object value)> ReturnReportProperties();
	}
}
