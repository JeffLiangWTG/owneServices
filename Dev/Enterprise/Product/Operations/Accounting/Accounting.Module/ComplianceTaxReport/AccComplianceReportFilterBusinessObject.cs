using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccComplianceReportFilterBusinessObject : FilterStripBusinessObject
	{
		public AccComplianceReportFilterBusinessObject() : base()
		{ }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var branchFilter = result.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccComplianceReportSchema.ACR_GB_Branch, BranchCollection);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("AccComplianceReportFilter|Branch", "Branch");

			var typeFilter = result.AddTextFilter("Report Type", AccComplianceReportSchema.ACR_ReportType, ReportTypeList);
			typeFilter.Category = FilterCategories.ModesAndTypes;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("AccComplianceReportFilter|ReportType", "Report Type");

			var statusFilter = result.AddTextFilter("Report Status", AccComplianceReportSchema.ACR_Status, ReportStatusList);
			statusFilter.Category = FilterCategories.ModesAndTypes;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("AccComplianceReportFilter|ReportStatus", "Report Status");

			var periodicityFilter = result.AddTextFilter("Report Periodicity", AccComplianceReportSchema.ACR_Periodicity, ReportPeriodicityList);
			periodicityFilter.Category = FilterCategories.ModesAndTypes;
			periodicityFilter.MultilingualDescription = ResString.GetMultilingualString("AccComplianceReportFilter|ReportPeriodicity", "Report Periodicity");

			var descriptionFilter = result.AddTextFilter("Report Description", AccComplianceReportSchema.ACR_Description);
			descriptionFilter.Category = FilterCategories.TextSearch;
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("AccComplianceReportFilter|ReportDescription", "Report Description");

			var dateFromFilter = result.AddDateFilter("Report Post From", AccComplianceReportSchema.ACR_DateFrom);
			dateFromFilter.Category = FilterCategories.Dates;
			dateFromFilter.MultilingualDescription = ResString.GetMultilingualString("AccComplianceReportFilter|ReportPostFrom", "Report Post From");

			var dateToFilter = result.AddDateFilter("Report Post To", AccComplianceReportSchema.ACR_DateTo);
			dateToFilter.Category = FilterCategories.Dates;
			dateToFilter.MultilingualDescription = ResString.GetMultilingualString("AccComplianceReportFilter|ReportPostTo", "Report Post To");

			return result;
		}

		GlbBranchCollection BranchCollection =>
			new GlbBranchCollection(
				Factory,
				new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK).AddToFilter(GlbBranchSchema.GB_IsActive, true));

		CodeDescriptionPairList ReportTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Cast<ComplianceReportConfiguration>().
					Select(x => new CodeDescriptionPair(x.ReportCode.ToString(), (NoResString)x.ReportTitle)).ToArray());
				return result;
			}
		}

		CodeDescriptionPairList ReportStatusList => AccComplianceReportLookups.GetReportStatusList();

		CodeDescriptionPairList ReportPeriodicityList => ComplianceReportConfigurationLookups.GetReportPeriodicityList(Env.CurrentCompany.Country.Code);

		public override ZQuery Filter => base.Filter.AddToFilter(AccComplianceReportSchema.ACR_GC_Company, Env.CurrentCompany.PK);
	}
}
