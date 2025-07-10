using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class GermanyComplianceReportsProvider : IComplianceReportsProvider
	{
		CodeDescriptionPairList IComplianceReportsProvider.GetReportTypeList(Guid companyPK)
		{
			var reportTypes = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty).GetReportTypeList();
			var reportStatus = AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			reportStatus.Cast<CodeDescriptionBool>().Where(v => !v.Bool).ForEach(x => reportTypes.RemoveCode(x.Code));
			return reportTypes;
		}
	}
}
