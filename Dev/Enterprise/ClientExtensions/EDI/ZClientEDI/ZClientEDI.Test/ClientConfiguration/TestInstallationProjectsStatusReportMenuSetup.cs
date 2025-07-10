using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Test.Reports
{
	using Enterprise.ReportTesting;

	public class TestInstallationProjectsStatusReportMenuSetup : ClientSpecificReportTestCase
	{
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.ProcessMgrReports;

		public override string MenuName => "Installation Projects Status Report";

		public override string Hint => @"This is an EDIPROD only report.
It is a report that lists key information about each Installation Project.";

		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase() => new TestInstallationProjectsStatusReport();
	}
}
