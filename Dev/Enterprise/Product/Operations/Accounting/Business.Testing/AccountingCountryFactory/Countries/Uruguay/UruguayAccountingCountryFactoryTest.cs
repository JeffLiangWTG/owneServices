using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class UruguayAccountingCountryFactoryTest : TestCaseWithFactory
	{
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
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Uruguay);
	}
}
