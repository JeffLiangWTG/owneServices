using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class FijiAccountingCountryFactory : IAccountingCountryFactory, IInstanceProvider<IEInvoicingPivotStatusProvider>
	{
		IEInvoicingPivotStatusProvider IInstanceProvider<IEInvoicingPivotStatusProvider>.Get() => new FijiEInvoicingPivotStatusProvider();
	}
}
