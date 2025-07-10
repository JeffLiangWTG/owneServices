using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.EDI.Test.Reports
{
	[TemplateName("Customer Service Incident Managment Report")]
	public class CustomerServiceIncidentManagmentTemplateTest : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode => Clients.EDI;
		protected override bool ReportRequiresColumnHeadings => false;
	}
}
