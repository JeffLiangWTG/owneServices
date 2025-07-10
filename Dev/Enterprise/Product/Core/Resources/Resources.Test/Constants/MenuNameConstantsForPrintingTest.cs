using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class MenuNameConstantsForPrintingTest : TestCase
	{
		public void TestMenuNameConstants()
		{
			AssertEquals("Consol Job Profit Document", Core.Constants.MenuNameConstantsForPrinting.ConsolJobProfitDocument);
			AssertEquals("Quotation Pack", Core.Constants.MenuNameConstantsForPrinting.QuotationPack);
			AssertEquals("Agent Pricing Page", Core.Constants.MenuNameConstantsForPrinting.AgentPricingPage);
		}
	}
}
