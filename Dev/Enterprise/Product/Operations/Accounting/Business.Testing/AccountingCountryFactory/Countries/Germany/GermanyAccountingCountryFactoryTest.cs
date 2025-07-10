using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class GermanyAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIEInvoicingEligibilityDecider()
		{
			var eligibilityDecider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>()
				.GetCountryFactory(Constants.CountryCodes.Germany) as IInstanceProvider<IEInvoicingEligibilityDecider>)
				.Get();

			AssertType<GermanyEInvoicingEligibilityDecider>(eligibilityDecider);
		}
	}
}
