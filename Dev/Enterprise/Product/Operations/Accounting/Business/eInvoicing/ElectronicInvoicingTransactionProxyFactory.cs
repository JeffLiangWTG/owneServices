using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business.EInvoicing
{
	[CodeAlive("Used through interface declared in Spring.Net xml")]
	public class EInvoicingTransactionProxyFactory : IEInvoicingTransactionProxyFactory
	{
		public IEInvoicingTransaction GetProxy(AccTransactionHeader transaction)
			=> new ElectronicInvoicingTransactionProxy(transaction);
	}
}
