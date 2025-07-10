using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.Wow.Testing
{
	[TemplateName("Order Currency Requirement")]
	public class OrderCurrencyRequirementClientSpecificReportTest : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode
		{
			get
			{
				return Clients.WOW;
			}
		}

		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}
}
