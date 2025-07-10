using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Mexico;

namespace Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency.Testing
{
	public class EInvoicingDependencyFactoryTest : TestCaseWithFactory
	{
		public void TestEInvoicingDependencyFactory()
		{
			AssertType<EInvoicingDependencyFactory>(ObjectFactory.Get<IEInvoicingDependencyFactory>());
		}

		public void TestEInvoicingDependencyFactory_CountriesDependencies()
		{
			var eInvoicingDependencyFactory = new EInvoicingDependencyFactory() as IEInvoicingDependencyFactory;
			AssertType<MexicoEInvoicingDependencyFactory>(eInvoicingDependencyFactory.GetMexicoEInvoicingDependencyFactory());
			AssertType<ArgentinaEInvoicingDependencyFactory>(eInvoicingDependencyFactory.GetArgentinaEInvoicingDependencyFactory());
			AssertType<TransactionInfoHelper>(eInvoicingDependencyFactory.GetTransactionInfoHelper());
		}
	}
}
