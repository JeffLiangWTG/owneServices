using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico.Testing
{
	public class MexicoAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestGetInvoicePaymentMethod()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Mexico) as IInvoicePaymentMethodProvider;

			AssertType<MexicoInvoicePaymentMethod>(builder.GetInvoicePaymentMethodProvider());
		}

		public void TestGetEquivalentAgreedPaymentMethod()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Mexico) as IEquivalentAgreedPaymentMethodProvider;

			AssertType<MexicoEquivalentAgreedPaymentMethod>(builder.GetEquivalentAgreedPaymentMethodProvider());
		}

		public void TestGetReversalStatusCodeConfigurationDoesNotReturnNull()
		{
			var builder = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Mexico) as IInstanceProvider<IReversalStatusCodeConfiguration>;

			AssertNotNull(builder.Get());
		}
	}
}
