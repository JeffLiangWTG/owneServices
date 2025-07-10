using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.MFI.Testing
{
	[TemplateName("MFI Job Profit by Tariff Level")]
	public class TestJobProfit_ForwardingByTariffLevelRoutine : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode
		{
			get
			{
				return Clients.MFI;
			}
		}
	}
}
