using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class KoreaSouthAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIInstanceProvider_IInvoiceDateValidation()
		{
			var provider = GetCountryFactoryForThisCountry() as IInstanceProvider<IInvoiceDateValidation>;
			AssertNotNull("IInstanceProvider<IInvoiceDateValidation> is implemented in KoreaSouth", provider);
			AssertType<KoreaSouthEInvoicingValidation>(provider.Get());
		}

		public void TestIInstanceProvider_IReverseDateValidation()
		{
			var provider = GetCountryFactoryForThisCountry() as IInstanceProvider<IReverseDateValidation>;
			AssertNotNull("IInstanceProvider<IReverseDateValidation> is implemented in KoreaSouth", provider);
			AssertType<KoreaSouthEInvoicingValidation>(provider.Get());
		}

		public void TestIInstanceProvider_IEInvoicingActionProvider()
		{
			var provider = GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingActionProvider>;
			AssertNotNull("IInstanceProvider<IEInvoicingActionProvider> is implemented in KoreaSouth", provider);
			AssertType<KoreaSouthEInvoicingActionProvider>(provider.Get());
		}

		public void TestInstanceProvider_IEInvoicingPreEligibilityProvider()
		{
			var provider = GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPreEligibilityProvider>;
			AssertNotNull("IInstanceProvider<IEInvoicingEligibilityProvider> is implemented in Korea", provider);
			AssertType<KoreaSouthEInvoicingPreEligibilityProvider>(provider.Get());
		}

		public void TestInstanceProvider_IAmendStatusCodeProvider()
		{
			var provider = GetCountryFactoryForThisCountry() as IInstanceProvider<IAmendStatusCodeProvider>;
			AssertNotNull("IInstanceProvider<IAmendStatusCodeProvider> is implemented in Korea", provider);
			AssertType<KoreaSouthAmendStatusCodeProvider>(provider.Get());
		}

		public void TestInstanceProvider_IAmendStatusCodeValidationProvider()
		{
			var provider = GetCountryFactoryForThisCountry() as IInstanceProvider<IAmendStatusCodeValidationProvider>;
			AssertNotNull("IInstanceProvider<IAmendStatusCodeValidationProvider> is implemented in Korea", provider);
			AssertType<KoreaSouthAmendStatusCodeValidationProvider>(provider.Get());
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.KoreaSouth);
	}
}
