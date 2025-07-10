using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(IInvoiceHeaderValidationDecider))]
	public abstract class InvoiceHeaderValidationDeciderTest<T> : TestCaseWithFactory
		where T : class, IInvoiceHeaderValidationDecider
	{
		public void TestIsRuleR0012Active()
		{
			AssertEquals(ExpectedIsRuleR0012Active, validationDecider.IsRuleR0012Active);
		}

		public void TestIsRuleC0002Active()
		{
			AssertEquals(ExpectedIsRuleC0002Active, validationDecider.IsRuleC0002Active);
		}

		public void TestIsRuleC0624Active()
		{
			AssertEquals(ExpectedIsRuleC0624Active, validationDecider.IsRuleC0624Active);
		}

		public void TestIsRuleC0627Active()
		{
			AssertEquals(ExpectedIsRuleC0627Active, validationDecider.IsRuleC0627Active);
		}

		public void TestIsRuleC0729Active()
		{
			AssertEquals(ExpectedIsRuleC0729Active, validationDecider.IsRuleC0729Active);
		}

		public void TestIsRuleC0728Active()
		{
			AssertEquals(ExpectedIsRuleC0728Active, validationDecider.IsRuleC0728Active);
		}

		public void TestIsRuleC0738Active()
		{
			AssertEquals(ExpectedIsRuleC0738Active, validationDecider.IsRuleC0738Active);
		}

		protected abstract bool ExpectedIsRuleR0012Active { get; }

		protected abstract bool ExpectedIsRuleC0002Active { get; }

		protected abstract bool ExpectedIsRuleC0624Active { get; }

		protected abstract bool ExpectedIsRuleC0627Active { get; }

		protected abstract bool ExpectedIsRuleC0729Active { get; }

		protected abstract bool ExpectedIsRuleC0728Active { get; }

		protected abstract bool ExpectedIsRuleC0738Active { get; }

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = GetNewValidationDecider();
		}

		protected virtual T GetNewValidationDecider()
		{
			return Activator.CreateInstance<T>();
		}

		protected T validationDecider;
	}
}
