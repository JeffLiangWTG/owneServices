using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Test.Reports
{
	public class CustomerServiceIncidentManagmentReportTest : ClientSpecificReportTestCase
	{
		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase() => new CustomerServiceIncidentManagmentTemplateTest();
		public override ModuleIdentifier ModuleIdToTest => Modules.ClientModuleRegistration.CustomerServiceReports;
		public override string MenuName => @"Customer Service Incident Management Report";
		public override string Hint => "";
	}
}
