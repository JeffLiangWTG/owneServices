using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Test.Reports
{
	public class CustomerServiceSurveyResultReporttTest : ClientSpecificReportTestCase
	{
		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase() => new CustomerServiceSurveyResultReportTemplateTest();
		public override ModuleIdentifier ModuleIdToTest => Modules.ClientModuleRegistration.CustomerServiceReports;
		public override string MenuName => @"Customer Service Survey Results Report";
		public override string Hint => "";
	}
}
