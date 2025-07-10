using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Israel
{
	class IsraelComplianceReportDefaultValue : IComplianceReportDefaultValue
	{
		string IComplianceReportDefaultValue.GetReferenceNumber() => ZDateTime.UtcNow.ToString("yyMMddHHmmssfff");
	}
}
