using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class PaymentMatchingValidationTest : PaymentValidationTest
	{
		public override void TestOSExTaxAmount()
		{
			IMatchingCollection collection = new IMatchingCollection(Factory);
			collection.Add(TestPayment);    // create the matching context

			TestPayment.AH_OSExTaxAmount = 0m;
			Assert("OSAmount cannot be zero so should have errors", TestPayment.AH_OSExTaxAmountInfo.HasErrors());
			TestPayment.AH_OSExTaxAmount = 9m;
			Assert("Precondition: no errors", !TestPayment.AH_OSExTaxAmountInfo.HasErrors());
			TestPayment.AH_OSExTaxAmount = -9m;
			Assert("OSAmount cannot be less than zero so should have errors", TestPayment.AH_OSExTaxAmountInfo.HasErrors());
		}
	}
}
