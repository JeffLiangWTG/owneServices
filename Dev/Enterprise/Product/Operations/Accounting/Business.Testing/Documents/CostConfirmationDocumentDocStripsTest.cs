using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class CostConfirmationDocumentDocStripsTest : TestCaseWithFactory
	{
		public void TestMenuItemContainsAllRequiredDocStrips()
		{
			DocStripsTestHelper.AssertMenuItemContainsRequiredDocStripsInAllContexts(
				"Cost Confirmation Document",
				"Cost Confirmation Document",
				"System Document Elements",
				new List<(string, string)>
				{
					("HFP", "Invoice Logo"),
					("HFP", "Cost Confirmation Header Standard Style"),
					("HS2", "Cost Confirmation Header Second Page Style"),
					("BEX", "Cost Confirmation Document Lines"),
					("BEX", "Cost Confirmation Document Lines (Group By Job Style )"),
					("BEX", "Cost Confirmation Tax Transaction Listing"),
					("BDY", "Malaysia eInvoice QR Code"),
					("FLP", "Cost Confirmation Footer Standard Style"),
					("FAL", "Default Page Footer for All Documents"),
				});
		}
	}
}
