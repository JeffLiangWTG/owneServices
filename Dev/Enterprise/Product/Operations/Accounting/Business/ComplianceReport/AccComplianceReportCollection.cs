
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportCollection : ActiveBusinessObjectCollection<AccComplianceReport>
	{
		public AccComplianceReportCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		public AccComplianceReportCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{ }
	}
}

