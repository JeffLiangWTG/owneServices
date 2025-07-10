using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	public interface IComplianceReportAdditionalDataCollector
	{
		IComplianceReportTransactionHeaderDetails GetTransactionHeader(ZGuid headerPK);
	}
}
