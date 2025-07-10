using System;
using Enterprise.Accounting.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APDiscount))]
	public class APDiscountTest : DiscountTest
	{
		public void TestLedger()
		{
			AssertEquals("should be AP", ZArchitecture.Core.LedgerTypes.AccountsPayable, ((APDiscount)GetNewBusinessObject()).Ledger_ForTestOnly);
		}

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			AssertEquals("GL Account must be set by default.", AccountingConfigurationRegistry.Instance.APDiscountAccount.Value, Discount.AH_AG);

			using (AccountingConfigurationRegistry.Instance.APDiscountAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			using (AccountingConfigurationRegistry.Instance.ARDiscountAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid()))
			{
				var discount = PrepareTransactionHeaderForTest() as Discount;
				AssertEquals("GL Account must be set by default, fall back to AR Discount Account", AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value, discount.AH_AG);
			}
		}
	}
}
