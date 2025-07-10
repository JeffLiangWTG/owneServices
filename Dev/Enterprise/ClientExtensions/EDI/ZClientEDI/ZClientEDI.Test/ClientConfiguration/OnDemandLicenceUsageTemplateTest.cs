using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.EDI.Test.Reports
{
	[TemplateName("On Demand Licence Usage Report")]
	public class OnDemandLicenceUsageTemplateTest : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode => Clients.EDI;
	}
}
