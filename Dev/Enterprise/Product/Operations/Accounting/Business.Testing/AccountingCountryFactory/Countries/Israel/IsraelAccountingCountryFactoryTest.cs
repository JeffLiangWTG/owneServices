using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Israel;
using Enterprise.Accounting.Business.AccountingCountryFactory.Israel;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class IsraelAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIEInvoicingEligibilityDecider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingEligibilityDecider>)?.Get();
			AssertType<IsraelEInvoicingEligibilityDecider>(obj);
		}

		public void TestIComplianceReportDefaultValue()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IComplianceReportDefaultValue>)?.Get();
			AssertType<IsraelComplianceReportDefaultValue>(obj);
		}

		public void TestIGovernmentAllocatedIDValidationProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IGovernmentAllocatedIDValidationProvider>)?.Get();
			AssertType<IsraelGovernmentAllocatedIDPayablesValidationProvider>(obj);
		}

		public void TestIEInvoicingPivotStatusProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPivotStatusProvider>)?.Get();
			AssertType<IsraelEInvoicingPivotStatusProvider>(obj);
		}

		public void TestIEInvoicingPreEligibilityProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPreEligibilityProvider>)?.Get();
			AssertType<IsraelEInvoicingPreEligibilityProvider>(obj);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Israel);
	}
}
