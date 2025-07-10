using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestsSubclassesOf(typeof(ITaxFrameworkThresholdMethodProvider),
		RestrictedToAssemblies = new[] { "Enterprise.Accounting.Business" },
		ExcludeClientDlls = true)]
	public abstract class ThresholdMethodProviderTest : TestCaseWithFactory
	{
		public void TestIsTransactionLevelGroupThresholdMethodSupportedValue()
		{
			var actualIsTransactionLevelGroupThresholdMethodSupportedValue = ((IInstanceProvider<ITaxFrameworkThresholdMethodProvider>)ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode)).Get().IsTransactionLevelGroupThresholdMethodSupported;
			AssertEquals(ExpectedIsTransactionLevelGroupThresholdMethodSupported, actualIsTransactionLevelGroupThresholdMethodSupportedValue);
		}

		public void TestIsTransactionLevelTaxBaseThresholdMethodSupportedValue()
		{
			var actualIsTransactionLevelTaxBaseThresholdMethodSupported = ((IInstanceProvider<ITaxFrameworkThresholdMethodProvider>)ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode)).Get().IsTransactionLevelTaxBaseThresholdMethodSupported;
			AssertEquals(ExpectedIsTransactionLevelTaxBaseThresholdMethodSupported, actualIsTransactionLevelTaxBaseThresholdMethodSupported);
		}

		protected abstract string CountryCode { get; }

		protected abstract bool ExpectedIsTransactionLevelGroupThresholdMethodSupported { get; }

		protected abstract bool ExpectedIsTransactionLevelTaxBaseThresholdMethodSupported { get; }
	}
}
