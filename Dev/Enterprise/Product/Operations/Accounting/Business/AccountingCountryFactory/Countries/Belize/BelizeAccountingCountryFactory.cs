using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class BelizeAccountingCountryFactory : IAccountingCountryFactory, IInstanceProvider<ITaxIDMacroDataProvider>
	{
		ITaxIDMacroDataProvider IInstanceProvider<ITaxIDMacroDataProvider>.Get() => new BelizeTaxIDMacroData();
	}
}
