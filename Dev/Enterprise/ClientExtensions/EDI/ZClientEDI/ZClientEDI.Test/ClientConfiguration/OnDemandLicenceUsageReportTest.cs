using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Test.Reports
{
	public class OnDemandLicenceUsageReportTest : ClientSpecificReportTestCase
	{
		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase() => new OnDemandLicenceUsageTemplateTest();
		public override string Hint => "Provides Licence Usage Statistics for a specific licence or organisation.";
		public override string MenuName => "On Demand Licence Usage Report";
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.ProcessMgrReports;
	}
}
