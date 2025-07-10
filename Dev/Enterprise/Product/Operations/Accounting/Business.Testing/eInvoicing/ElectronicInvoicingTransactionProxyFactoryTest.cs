using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.eInvoicing.Testing
{
	public class ElectronicInvoicingTransactionProxyFactoryTest : TestCaseWithFactory
	{
		public void TestGetProxy_ForARInvoice()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var wrappedTransaction = new EInvoicingTransactionProxyFactory().GetProxy(arInvoice);
			AssertNotNull(wrappedTransaction);
			AssertEquals(arInvoice.PK, wrappedTransaction.PK);
		}

		public void TestGetProxy_ForBusinessInvoice()
		{
			var governmentInvoice = Factory.NewWithValidTestData<GovernmentInvoice>();
			var wrappedTransaction = new EInvoicingTransactionProxyFactory().GetProxy(governmentInvoice);
			AssertNotNull(wrappedTransaction);
			AssertEquals(governmentInvoice.PK, wrappedTransaction.PK);
		}
	}
}
