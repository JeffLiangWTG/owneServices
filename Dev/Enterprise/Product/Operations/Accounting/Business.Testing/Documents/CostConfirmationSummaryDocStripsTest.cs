using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class CostConfirmationSummaryDocStripsTest : TestCaseWithFactory
	{
		public void TestMenuItemContainsAllRequiredDocStrips()
		{
			DocStripsTestHelper.AssertMenuItemContainsRequiredDocStripsInAllContexts(
				"Cost Confirmation Summary",
				"Cost Confirmation Summary",
				"System Document Elements",
				new List<(string, string)>
				{
					("HFP", "Invoice Logo"),
					("HFP", "Cost Confirmation Header Standard Style"),
					("HS2", "Cost Confirmation Header Second Page Style"),
					("BEX", "Cost Confirmation Summary Lines"),
					("BEX", "Cost Confirmation Tax Transaction Listing"),
					("BDY", "Malaysia eInvoice QR Code"),
					("FLP", "Cost Confirmation Footer Standard Style"),
					("FAL", "Default Page Footer for All Documents"),
				});
		}
	}
}
