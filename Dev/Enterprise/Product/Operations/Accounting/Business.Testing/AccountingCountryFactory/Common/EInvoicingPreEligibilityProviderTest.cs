using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestsSubclassesOf(typeof(EInvoicingPreEligibilityProvider), ExcludeClientDlls = true)]
	public abstract class EInvoicingPreEligibilityProviderTest : TestCaseWithFactory
	{
		protected abstract string CountryCode { get; }
		protected virtual bool ExpectedCanEvaluateByComplianceDate_Before { get; } = true;
		protected virtual bool ExpectedCanEvaluateByComplianceDate_On { get; } = true;
		protected virtual bool ExpectedCanEvaluateByComplianceDate_After { get; }

		protected virtual bool ExpectedCanEvaluateByTransaction_IsInDatabase { get; }
		protected virtual bool ExpectedCanEvaluateByTransaction_IsNotInDatabase { get; } = true;

		public void TestIEInvoicingPreEligibilityProviderExists()
		{
			var provider = GetInvoicingPreEligibilityProvider();
			AssertNotNull("IEInvoicingPreEligibilityProvider has not been implemented for country " + CountryCode, provider);
		}

		public virtual void TestCanEvaluateByComplianceDate()
		{
			var testDateTime = ZDateTime.Now;
			var trans = Factory.NewWithValidTestData<AccTransactionHeader>();
			trans.AH_PostDate = testDateTime;

			var provider = GetInvoicingPreEligibilityProvider();
			var complianceDate = testDateTime.ToDateTime();
			var beforeAH_PostDate = testDateTime.AddDays(-1).ToDateTime();

			AssertEquals(false, provider?.CanEvaluateByComplianceDate(trans, DateTime.MinValue));

			AssertEquals(ExpectedCanEvaluateByComplianceDate_Before, provider?.CanEvaluateByComplianceDate(trans, beforeAH_PostDate));

			AssertEquals(ExpectedCanEvaluateByComplianceDate_On, provider?.CanEvaluateByComplianceDate(trans, complianceDate));

			var afterAH_PostDate = complianceDate.AddDays(1);
			AssertEquals(ExpectedCanEvaluateByComplianceDate_After, provider?.CanEvaluateByComplianceDate(trans, afterAH_PostDate));
		}

		public virtual void TestCanEvaluateByTransaction()
		{
			var trans = Factory.NewWithValidTestData<AccTransactionHeader>();
			var provider = GetInvoicingPreEligibilityProvider();
			AssertEquals(ExpectedCanEvaluateByTransaction_IsNotInDatabase, provider?.CanEvaluateByTransaction(trans));
			Factory.Save();
			AssertEquals(ExpectedCanEvaluateByTransaction_IsInDatabase, provider?.CanEvaluateByTransaction(trans));
		}
		protected IEInvoicingPreEligibilityProvider GetInvoicingPreEligibilityProvider()
		{
			var countryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode);
			return (countryFactory as IInstanceProvider<IEInvoicingPreEligibilityProvider>)?.Get();
		}
	}
}
