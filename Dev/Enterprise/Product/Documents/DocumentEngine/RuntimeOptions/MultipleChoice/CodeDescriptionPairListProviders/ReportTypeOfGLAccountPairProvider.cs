using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ReportTypeOfGLAccountPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("ALL", ResString.GetMultilingualString("ADDFDB22-AFE7-425F-A470-B829A77BA799", "Include all report types"));

			var reportTypes = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value;

			foreach (ComplianceReportType reportType in reportTypes.Cast<ComplianceReportType>().Where(reportType => !result.ContainsCode(reportType.ReportType)))
			{
				result.AddPair(reportType.ReportType, reportType.ReportTypeDescription);
			}

			var userDefineReportTypes = AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value;

			foreach (ComplianceReportType reportType in userDefineReportTypes.Cast<ComplianceReportType>().Where(reportType => !result.ContainsCode(reportType.ReportType)))
			{
				result.AddPair(reportType.ReportType, reportType.ReportTypeDescription);
			}

			return result;
		}
	}
}
