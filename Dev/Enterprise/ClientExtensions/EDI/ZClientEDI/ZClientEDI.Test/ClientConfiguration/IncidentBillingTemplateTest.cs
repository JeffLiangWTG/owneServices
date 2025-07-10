using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.EDI.Test.Reports
{
	[TemplateName("Incident Billing Worksheet")]
	public class IncidentBillingTemplateTest : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode => Clients.EDI;
	}
}
