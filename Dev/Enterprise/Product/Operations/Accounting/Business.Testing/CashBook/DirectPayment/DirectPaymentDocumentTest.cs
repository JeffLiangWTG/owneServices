using CargoWise.Definitions;
using Enterprise.Accounting.Business.Base.Transaction.Testing;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment.Testing
{
	public class DirectPaymentDocumentTest : IDocumentSupportableTest
	{
		new public void TestBusinessObject()
		{
			DirectPayment payment = Factory.NewWithValidTestData<DirectPayment>();
			AssertEquals(BusinessContext.CBDirectPayment, payment.DocumentSupporter.BusinessContext);
		}
	}
}
