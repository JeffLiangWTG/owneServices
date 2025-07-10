using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Mexico;

namespace Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency
{
	public class EInvoicingDependencyFactory : IEInvoicingDependencyFactory
	{
		IArgentinaEInvoicingDependencyFactory IEInvoicingDependencyFactory.GetArgentinaEInvoicingDependencyFactory() => new ArgentinaEInvoicingDependencyFactory();
		IMexicoEInvoicingDependencyFactory IEInvoicingDependencyFactory.GetMexicoEInvoicingDependencyFactory() => new MexicoEInvoicingDependencyFactory();
		ITransactionInfoHelper IEInvoicingDependencyFactory.GetTransactionInfoHelper() => new TransactionInfoHelper();
	}
}
