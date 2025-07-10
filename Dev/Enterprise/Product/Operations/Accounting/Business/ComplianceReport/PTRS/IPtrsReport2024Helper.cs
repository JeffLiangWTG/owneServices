using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public interface IPtrsReport2024Helper : IService
	{
		PtrsReport2024Data CalculatePtrsReport2024Data(AccComplianceReport complianceReport, PtrsAllPaymentsReport2024Data tcpData);
		PtrsAllPaymentsReport2024Data CalculatePtrsAllPaymentsReport2024Data(AccComplianceReport complianceReport);
	}
}
