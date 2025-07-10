using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Invoicing;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingLineTaxDateCacheProviderTest : TestCaseWithFactory
	{
		public void TestBindChargeEvents()
		{
			var transactionLineTaxDate = new Mock<ITransactionLineTaxDate>();
			var invoiceTaxDataCacheProvider = new Mock<InvoiceTaxDateCacheProvider>();
			var invoicingLineTaxDateCacheProvider = new InvoicingLineTaxDateCacheProvider(transactionLineTaxDate.Object, () => invoiceTaxDataCacheProvider.Object);
			var chargeCode1 = TestObjectCreator.CC1;
			var chargeCode2 = TestObjectCreator.CC2;

			chargeCode1.AC_ChargeType = "CM1";

			invoiceTaxDataCacheProvider.Verify(x => x.StaleCache(), Times.Never, "Pre-condition");

			using (invoicingLineTaxDateCacheProvider.BindChargeEvents())
			{
				transactionLineTaxDate.Setup(x => x.ChargeCode).Returns(chargeCode1);
			}
			chargeCode1.AC_ChargeType = "CM2";

			invoiceTaxDataCacheProvider.Verify(x => x.StaleCache(), Times.Once, "Should stale header tax date cache via chargeCode1 property change event.");

			using (invoicingLineTaxDateCacheProvider.BindChargeEvents())
			{
				transactionLineTaxDate.Setup(x => x.ChargeCode).Returns(chargeCode2);
			}
			chargeCode1.AC_ChargeType = "CM3";
			chargeCode2.AC_ChargeType = "CM4";

			invoiceTaxDataCacheProvider.Verify(x => x.StaleCache(), Times.Exactly(2), "Should stale header tax date cache once more via chargeCode2 property change event. And chargeCode1 property change event should be unhooked.");
			Assert("Mock Verified.", true);
		}

		public void TestStaleCache()
		{
			var transactionLineTaxDate = new Mock<ITransactionLineTaxDate>();
			var invoiceTaxDataCacheProvider = new Mock<InvoiceTaxDateCacheProvider>();
			var invoicingLineTaxDateCacheProvider = new InvoicingLineTaxDateCacheProvider(transactionLineTaxDate.Object, () => invoiceTaxDataCacheProvider.Object);

			invoiceTaxDataCacheProvider.Verify(x => x.StaleCache(), Times.Never, "Pre-condition");

			invoicingLineTaxDateCacheProvider.StaleCache();

			invoiceTaxDataCacheProvider.Verify(x => x.StaleCache(), Times.Once, "Should stale header tax date cache.");
			Assert("Mock Verified.", true);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
