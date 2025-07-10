using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Test.Reports
{
	public class LicenceExpiryReportTest : ClientSpecificReportTestCase
	{
		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase() => new LicenceExpiryTemplateTest();
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.ProcessMgrReports;
		public override string MenuName => @"Licence Expiry Report";
		public override string Hint => @"";
	}
}
