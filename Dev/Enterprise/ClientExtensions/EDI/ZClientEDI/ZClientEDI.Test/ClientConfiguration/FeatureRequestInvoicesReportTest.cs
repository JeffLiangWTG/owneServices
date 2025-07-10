using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Test.Reports
{
	public class FeatureRequestInvoicesReportTest : ClientSpecificReportTestCase
	{
		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase() => new FeatureRequestInvoicesTemplateTest();
		public override ModuleIdentifier ModuleIdToTest => Modules.ClientModuleRegistration.CustomerServiceReports;
		public override string MenuName => @"Feature Request Invoices";
		public override string Hint => @"This report details invoices raised on Feature Requests.";
	}
}
