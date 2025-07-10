using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Jordan;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class JordanAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIEInvoicingEligibilityDecider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingEligibilityDecider>)?.Get();
			CombineAssertions("Test IEInvoicingEligibilityDecider", () =>
			{
				AssertNotNull("should exist" ,obj);
				AssertType<JordanEInvoicingEligibilityDecider>("should be JordanEInvoicingEligibilityDecider type", obj);
			});
		}

		public void TestIEInvoicingPreEligibilityProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPreEligibilityProvider>)?.Get();
			CombineAssertions("Test IEInvoicingPreEligibilityProvider", () =>
			{
				AssertNotNull("should exist", obj);
				AssertType<JordanEInvoicingPreEligibilityProvider>("should be JordanEInvoicingPreEligibilityProvider type", obj);
			});
		}

		public void TestGetTransactionQRCodeString()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var builder = GetCountryFactoryForThisCountry() as IQRCodeDataProvider;

			AssertNullOrEmpty(builder.GetTransactionQRCodeString(invoice));
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Jordan);
	}
}
