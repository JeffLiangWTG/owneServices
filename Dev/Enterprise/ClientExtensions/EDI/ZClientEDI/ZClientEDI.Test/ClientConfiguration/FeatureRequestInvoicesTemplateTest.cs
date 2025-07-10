using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.EDI.Test.Reports
{
	[TemplateName("Feature Request Invoices")]
	public class FeatureRequestInvoicesTemplateTest : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode => Clients.EDI;
	}
}
