using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business
{
	public interface ISupportQueueingForComplianceReports : IBusiness
	{
		ComplianceSubTypeRule ComplianceMatchingRule { get; }

		bool CheckIsValidForQueueing(BusinessObjectFactory factory);

		IComplianceReportQueuer Queuer { get; }
	}
}
