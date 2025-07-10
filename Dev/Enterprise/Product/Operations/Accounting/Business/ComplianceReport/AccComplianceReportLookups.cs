//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccComplianceReportLookups
//
//    This class should be used for overriding collections in AutoAccComplianceReportLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportLookups : AutoAccComplianceReportLookups
	{
		public AccComplianceReportLookups(AutoAccComplianceReport parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ReportTypeList =>
			Parent is AutoAccComplianceReport complianceReport &&
			(ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(complianceReport.Company.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceReportsProvider>)?.Get() is IComplianceReportsProvider countryCompliance
			? countryCompliance.GetReportTypeList(complianceReport.Company.PK.ToGuid())
			: AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.GetReportTypeList();

		public CodeDescriptionPairList PeriodicityList
		{
			get
			{
				var parent = Parent as AutoAccComplianceReport;
				return ComplianceReportConfigurationLookups.GetReportPeriodicityList(parent.Company.GC_RN_NKCountryCode);
			}
		}

		public CodeDescriptionPairList ReportStatusList => (reportStatusList ??= GetReportStatusList());
		CodeDescriptionPairList reportStatusList;

		public static CodeDescriptionPairList GetReportStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(AccComplianceReport.Status.ReportCreated, Res.GetString("2d05ac02-827e-4264-8ad8-80eaad954041", "Report Created"));
			result.AddPair(AccComplianceReport.Status.ReportPendingQueueing, Res.GetString("b3adb224-7c1d-42ab-8431-73815e5330a0", "Report Pending Data Queuing"));
			result.AddPair(AccComplianceReport.Status.ReportDataQueued, Res.GetString("2e0b7d11-93f9-4cdf-a174-38d38c7a5866", "Report Data Queued"));
			result.AddPair(AccComplianceReport.Status.ReportGenerated, Res.GetString("276221ca-9769-46f4-9bad-4596810aee96", "Report Generated"));
			result.AddPair(AccComplianceReport.Status.ReportOutputGenerated, Res.GetString("8fcd63fc-36d3-4a68-af1b-1a1a72835ede", "Output File(s) Generated"));
			result.AddPair(AccComplianceReport.Status.ReportInvalidated, Res.GetString("8f73f69f-6add-41cc-9c63-dd8c3a331b8e", "Report Invalidated by Transactions updated"));
			result.AddPair(AccComplianceReport.Status.ReportFinalised, Res.GetString("48627990-79c9-4741-8b1d-4982d30d3946", "Report Finalized"));
			result.AddPair(AccComplianceReport.Status.ReportError, Res.GetString("47892819-8d27-4da9-b92b-a42dc7505f50", "Report Failed"));
			return result;
		}
	}
}
