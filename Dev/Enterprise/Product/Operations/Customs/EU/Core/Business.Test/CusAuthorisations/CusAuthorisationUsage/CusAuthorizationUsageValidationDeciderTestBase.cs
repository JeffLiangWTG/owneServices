using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(ICusAuthorizationUsageValidationDecider))]
	public abstract class CusAuthorizationUsageValidationDeciderTestBase<TCusAuthorizationUsageValidationDecider> : TestCaseWithFactory
		where TCusAuthorizationUsageValidationDecider : class, ICusAuthorizationUsageValidationDecider
	{
		public void TestIsRuleR0010Active() => AssertEquals(ExpectedIsRuleR0010Active, validationDecider.IsRuleR0010Active);

		public void TestIsRuleR0675Active() => AssertEquals(ExpectedIsRuleR0675Active, validationDecider.IsRuleR0675Active);

		protected abstract bool ExpectedIsRuleR0010Active { get; }

		protected abstract bool ExpectedIsRuleR0675Active { get; }

		protected override void SetUp()
		{
			base.SetUp();

			validationDecider = Activator.CreateInstance<TCusAuthorizationUsageValidationDecider>();
		}

		TCusAuthorizationUsageValidationDecider validationDecider;
	}
}
