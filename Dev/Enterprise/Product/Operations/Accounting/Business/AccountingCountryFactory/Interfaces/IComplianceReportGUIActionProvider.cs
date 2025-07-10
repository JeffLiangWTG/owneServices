using Enterprise.Accounting.Business.ComplianceReport;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IComplianceReportGUIActionProvider
	{
		bool IsCountrySupportGenerateSAFT { get; }

		(bool IsNeedCompression, int AllowedMaxSize) GetSAFTFileCompressionInfo();

		bool IsNeedAggregateBeforeGenerate { get; }

		string ValidateBeforeGenerateSAFT(AccComplianceReport[] reports);
	}
}
