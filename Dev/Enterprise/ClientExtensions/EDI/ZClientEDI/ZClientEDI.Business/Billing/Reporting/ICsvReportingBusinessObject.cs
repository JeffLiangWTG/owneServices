using System;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface ICsvReportingBusinessObject
	{
		void GetCsvUsageReport(Action<string> action);
		void GetCsvUsageReport(ICsvUsageReportWriter writer);
	}
}
