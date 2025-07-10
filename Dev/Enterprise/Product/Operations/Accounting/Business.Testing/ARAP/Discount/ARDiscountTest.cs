using Enterprise.Accounting.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARDiscount))]
	public class ARDiscountTest : DiscountTest
	{
		public void TestLedger()
		{
			AssertEquals("should be AR", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ((ARDiscount)GetNewBusinessObject()).Ledger_ForTestOnly);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			AssertEquals("GL Account must be set by default.", AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value, Discount.AH_AG);
		}
	}
}
