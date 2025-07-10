using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	public interface IComplianceReportTransactionHeaderDetails
	{
		ZInt HeaderSequence { get; set; }
		ZString Description { get; set; }
		ZString CreateUserCode { get; set; }
		ZString CreateUserName { get; set; }
		ZDateTime CreateTime { get; set; }
	}
}
