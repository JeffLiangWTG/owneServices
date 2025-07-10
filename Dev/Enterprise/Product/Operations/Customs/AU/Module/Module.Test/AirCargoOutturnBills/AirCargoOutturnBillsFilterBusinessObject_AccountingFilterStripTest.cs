using Enterprise.Accounting.Integration.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class AirCargoOutturnBillsFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<CusUnderbond>
	{
		protected override CusUnderbond GetNewBusinessObjectForFilterCollection() => Factory.NewWithValidTestData<CusUnderbond>();

		protected override ModuleIdentifier FilterStripModuleID => ModuleIDs.Customs.AU.AirCargoOutturnBills;

		protected override bool ShouldUseBillingFilters => false;
	}
}
