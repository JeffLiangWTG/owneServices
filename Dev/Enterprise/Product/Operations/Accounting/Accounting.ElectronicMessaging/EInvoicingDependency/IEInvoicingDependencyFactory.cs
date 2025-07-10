using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Mexico;

namespace Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency
{
	public interface IEInvoicingDependencyFactory
	{
		IArgentinaEInvoicingDependencyFactory GetArgentinaEInvoicingDependencyFactory();
		IMexicoEInvoicingDependencyFactory GetMexicoEInvoicingDependencyFactory();
		ITransactionInfoHelper GetTransactionInfoHelper();
	}
}
