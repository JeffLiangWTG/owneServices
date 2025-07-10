using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class RomaniaAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIEInvoicingEligibilityDecider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingEligibilityDecider>)?.Get();
			AssertNotNull(obj);
		}

		public void TestIBatchQueueInvoicesForEInvoicingProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IBatchQueueInvoicesForEInvoicingProvider>)?.Get();
			AssertNotNull(obj);
		}

		public void TestISupportResetStatusToDelivered()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<ISupportResetStatusToDelivered>)?.Get();
			AssertNotNull(obj);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry(string countryCode = Constants.CountryCodes.Romania)
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode);
	}
}
