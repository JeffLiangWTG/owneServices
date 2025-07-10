using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PanamaAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIEInvoicingEligibilityDecider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingEligibilityDecider>)?.Get();
			AssertNotNull(obj);
		}

		public void TestIEInvoicingPivotsToRequeueFilterProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>)?.Get();
			AssertNotNull(obj);
		}

		public void TestIEInvoicingRequeuePivotsStatusProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>)?.Get();
			AssertNotNull(obj);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Panama);
	}
}
