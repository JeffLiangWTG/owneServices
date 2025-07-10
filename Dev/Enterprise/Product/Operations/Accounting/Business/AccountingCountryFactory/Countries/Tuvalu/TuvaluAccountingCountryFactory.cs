using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class TuvaluAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<ITaxIDMacroDataProvider>
	{
		ITaxIDMacroDataProvider IInstanceProvider<ITaxIDMacroDataProvider>.Get() => new TuvaluTaxIDMacroData();
	}
}
