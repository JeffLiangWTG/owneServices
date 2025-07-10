using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.Wow.Testing
{
	[TemplateName("Outstanding Orders Report")]
	public class WOWOutstandingOrdersReportTemplateTest : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode
		{
			get
			{
				return Clients.WOW;
			}
		}
	}
}
