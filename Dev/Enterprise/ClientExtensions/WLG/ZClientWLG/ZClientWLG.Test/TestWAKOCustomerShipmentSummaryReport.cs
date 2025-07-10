using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.WLG.Testing
{
	[TemplateName("WAKO Customer Shipment Summary Report")]
	public class TestWAKOCustomerShipmentSummaryReport : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode
		{
			get
			{
				return Clients.WLG;
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
