using Enterprise.Accounting.Business.ComplianceReport;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class PortugalComplianceReportGUIActionProvider : IComplianceReportGUIActionProvider
	{
		bool IComplianceReportGUIActionProvider.IsCountrySupportGenerateSAFT => true;

		(bool IsNeedCompression, int AllowedMaxSize) IComplianceReportGUIActionProvider.GetSAFTFileCompressionInfo()
		{
			return (false, 0);
		}

		bool IComplianceReportGUIActionProvider.IsNeedAggregateBeforeGenerate => false;

		string IComplianceReportGUIActionProvider.ValidateBeforeGenerateSAFT(AccComplianceReport[] reports)
		{
			return string.Empty;
		}
	}
}
