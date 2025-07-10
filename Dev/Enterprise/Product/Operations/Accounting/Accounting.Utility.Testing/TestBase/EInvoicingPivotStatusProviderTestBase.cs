using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
namespace Enterprise.Accounting.Utility.Testing
{
	public abstract class EInvoicingPivotStatusProviderTestBase<TExpectedPivotStatusProvider> : TestCaseWithFactory
		  where TExpectedPivotStatusProvider : IEInvoicingPivotStatusProvider
	{
		protected abstract string CountryCode { get; }

		TestObjectCreator testObjectCreator;
		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));

		public void TestIEInvoicingPivotStatusProvider()
		{
			AssertType<TExpectedPivotStatusProvider>(GetEInvoicingPivotStatusProvider());
		}

		public virtual void TestCanCreateNotEligibleForEInvoicingPivot()
		{
			var provider = GetEInvoicingPivotStatusProvider();
			AssertNotNull("Precondition : Provider should not be null", provider);

			var trans = Factory.NewWithValidTestData<AccTransactionHeader>();
			var supportsNotEligibleStatus = provider.CanCreateNotEligibleForEInvoicingPivot(trans);

			AssertCanCreateNotEligibleForEInvoicingPivot(trans, false);
		}

		protected void AssertInitialPivotStatusEquals(ITransactionHeader transaction, string expectedStatus, string message = "", Action asserions = null)
		{
			var provider = GetEInvoicingPivotStatusProvider();
			AssertNotNull("Precondition : Provider should not be null", provider);

			var actualStatus = provider.GetInitialPivotStatus(transaction);

			var assertionMessage = string.IsNullOrEmpty(message) ? $"Expected status: {expectedStatus}, but got: {actualStatus}" : message;

			if (string.IsNullOrEmpty(expectedStatus))
			{
				AssertNullOrEmpty(assertionMessage, actualStatus);
			}
			else
			{
				AssertEquals(assertionMessage, expectedStatus, actualStatus);
			}
		}

		protected void AssertCanCreateNotEligibleForEInvoicingPivot(AccTransactionHeader transaction, bool expectedResult)
		{
			var provider = GetEInvoicingPivotStatusProvider();
			AssertNotNull("Precondition : Provider should not be null", provider);
			var actualResult = provider.CanCreateNotEligibleForEInvoicingPivot(transaction);
			AssertEquals($"Expected CanCreateNotEligibleForEInvoicingPivot  for country {CountryCode} to be {expectedResult}, but got {actualResult}",
					expectedResult, actualResult);
		}

		IEInvoicingPivotStatusProvider GetEInvoicingPivotStatusProvider()
		   => ((IInstanceProvider<IEInvoicingPivotStatusProvider>)ObjectFactory
			   .Get<IGlobalAccountingCountryFactory>()
			   .GetCountryFactory(CountryCode))
			   .Get();
	}
}
