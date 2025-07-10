using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Test.Reports
{
	public class IncidentBillingReportTest : ClientSpecificReportTestCase
	{
		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase() => new IncidentBillingTemplateTest();
		public override ModuleIdentifier ModuleIdToTest => Modules.ClientModuleRegistration.CustomerServiceReports;
		public override string MenuName => @"Customer Service Incident - Billing Worksheet";
		public override string Hint => @"This worksheet lists billable incidents.

It can be filtered for a date range, country and customer service representative.

It is intended for use with MS Excel and all amount totals will update if the rate is modified.
";
	}
}
