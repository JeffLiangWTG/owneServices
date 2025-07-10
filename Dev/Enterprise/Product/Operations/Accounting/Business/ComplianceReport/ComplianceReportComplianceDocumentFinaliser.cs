using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class ComplianceReportComplianceDocumentFinaliser : IComplianceReportComplianceDocumentFinaliser
	{
		public ComplianceReportComplianceDocumentFinaliser(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		BusinessObjectFactory Factory { get; set; }

		#region IComplianceReport

		public ZBool IsInFinalisedRange(ZString subtype, ZDateTime startDate, ZDateTime endDate, ZString ledger)
		{
			if (!startDate.IsEmpty && !endDate.IsEmpty)
			{
				var reportCodes = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Cast<ComplianceReportConfiguration>().Where(x => x.Country == GlbCompany.CurrentCompany.Country.Code &&
								x.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.ComplianceDocumentHeader &&
								x.Settings.Cast<ComplianceReportConfigurationSetting>().Any(y => y.ComplianceSubType == subtype && y.LedgerType == ledger)).Select(x => x.ReportCode);

				if (reportCodes != null && reportCodes.Any())
				{
					var query = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(AccComplianceReportSchema.ACR_ReportType, reportCodes);
					query.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, SQLComparisonOperator.LessThanOrEqualTo, startDate);
					query.AddToFilter(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);
					query.AddToFilter(AccComplianceReportSchema.ACR_Status, Status.ReportFinalised);
					return Factory.Exists(typeof(AccComplianceReport), query);
				}
			}
			return false;
		}

		#endregion

		public void FinaliseComplianceDocuments(AccComplianceReport accComplianceReport)
		{
			if (accComplianceReport.ReportBaseTablePrefix == AccComplianceDocumentHeaderSchema.Constants.Prefix)
			{
				var allLinesPks = accComplianceReport.ReportLines.Select(x => x.AH_PK).Distinct();
				foreach (var chunkLinesPks in AccountingUtils.ChunksOf(allLinesPks, AccountingUtils.ChunkBatchSize))
				{
					var headers = Factory.Load<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.PK, chunkLinesPks));
					headers.ForEach(x => x.Finalise(accComplianceReport));
				}
			}
		}
	}
}
