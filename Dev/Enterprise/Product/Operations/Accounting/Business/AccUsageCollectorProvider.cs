using System;
using CargoWise.EntityFramework;
using Enterprise.Billing.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business
{
	public class AccUsageCollectorProvider : IAccUsageCollectorProvider
	{
		public void ReportGeneralLedgerProcess(Guid companyPk)
		{
			Report(UsageFeatures.Codes.AccGeneralLedgerProcess, companyPk);
		}

		public void Report(string featureCode, Guid companyPk)
		{
			var isCurrentCompany = companyPk == Guid.Empty || companyPk == GlbCompany.CurrentCompany.PK.ToGuid();
			GlbCompany company = null;

			if (!isCurrentCompany)
			{
				var factory = new BusinessObjectFactory();
				company = factory.Load<GlbCompany>(companyPk);
			}

			using (isCurrentCompany ? null : DisposableEnvironment.ForCompany(company.GC_Code))
			{
				UsageCollector.Report(featureCode);
			}
		}

		public void ReportAccConsolidationGroupsGenerateExportFile()
		{
			UsageCollector.Report(UsageFeatures.Codes.AccConsolidationGroupGenerateExportFile,
				(UsageProperties.ConsolidationGroupClientID, GlbCompany.CurrentCompany.LicenceKeyIdentifier));
		}

		public void ReportAccConsolidationGroupsCreateEliminationJournals()
		{
			UsageCollector.Report(UsageFeatures.Codes.AccConsolidationGroupCreateEliminationJournals,
				(UsageProperties.ConsolidationGroupClientID, GlbCompany.CurrentCompany.LicenceKeyIdentifier));
		}
	}
}
