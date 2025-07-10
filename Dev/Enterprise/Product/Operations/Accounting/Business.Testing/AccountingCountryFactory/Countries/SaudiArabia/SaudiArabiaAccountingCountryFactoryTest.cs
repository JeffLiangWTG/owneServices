using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class SaudiArabiaAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestGetTransactionQRCodeString()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var builder = GetCountryFactoryForThisCountry() as IQRCodeDataProvider;

			AssertNullOrEmpty(builder.GetTransactionQRCodeString(invoice));
		}

		public void TestIEInvoicingEligibilityDecider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingEligibilityDecider>)?.Get();
			AssertNotNull(obj);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.SaudiArabia);
	}
}
