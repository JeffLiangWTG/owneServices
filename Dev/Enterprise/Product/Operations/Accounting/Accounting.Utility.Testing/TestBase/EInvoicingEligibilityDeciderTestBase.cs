using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Utility.Testing
{
	public abstract class EInvoicingEligibilityDeciderTestBase : TestCaseWithFactory
	{
		protected abstract string CountryCode { get; }
		protected abstract string ExpectedAdditionalTraceLog { get; }

		protected abstract FakeEligibilityLiteTransaction CreateEligibleTransaction();

		public void TestFakeEligibleTransaction_IsReallyEligible()
		{
			var decider = GetEligibilityDecider();
			var t = CreateEligibleTransaction();
			Assert("Fake eligible transaction for testing should actually be eligible", decider.IsTransactionEligible(t));
		}

		public virtual void TestGetAdditionalTraceLog()
		{
			var decider = GetEligibilityDecider();
			var t = CreateEligibleTransaction();
			AssertEquals("Expected the following log", ExpectedAdditionalTraceLog, decider.GetAdditionalTraceLog(t));
		}

		protected void AssertEligibilityForClosedSet(IEnumerable<string> allPossibleValues, IEnumerable<string> eligibleValues, Action<FakeEligibilityLiteTransaction, string> setValue, string validMessage)
		{
			var decider = GetEligibilityDecider();
			var t = CreateEligibleTransaction();

			foreach (var value in allPossibleValues)
			{
				setValue(t, value);
				var expectedResult = eligibleValues.Contains(value);
				AssertEquals($"Only {validMessage} should be eligible, but '{value}' is reported eligible.", expectedResult, decider.IsTransactionEligible(t));
			}
		}

		protected void AssertEligibilityForNonBlank(IEnumerable<string> allPossibleValues, Action<FakeEligibilityLiteTransaction, string> setValue, string validMessage)
		{
			var decider = GetEligibilityDecider();
			var t = CreateEligibleTransaction();

			foreach (var value in allPossibleValues)
			{
				setValue(t, value);
				var expectedResult = !string.IsNullOrEmpty(value);
				AssertEquals($"Non-blank {validMessage} should be eligible, but '{value}' is reported eligible.", expectedResult, decider.IsTransactionEligible(t));
			}
		}

		protected IEInvoicingEligibilityDecider GetEligibilityDecider()
			=> ((IInstanceProvider<IEInvoicingEligibilityDecider>)ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode)).Get();
	}
}
