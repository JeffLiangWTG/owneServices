using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.WLG.Testing
{
	[TemplateName("WAKO Cargo Received Class B Report")]
	public class TestWAKOCargoReceivedClassBReport : ClientSpecificTemplateTestCase
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
