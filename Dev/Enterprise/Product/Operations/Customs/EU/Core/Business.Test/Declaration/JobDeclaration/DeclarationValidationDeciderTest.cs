using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(IDeclarationValidationDecider))]
	public abstract class DeclarationValidationDeciderTest<T> : TestCaseWithFactory
		where T : class, IDeclarationValidationDecider
	{
		public void TestIsRuleC0002ActiveForGoodsDestination()
		{
			AssertEquals(ExpectedIsRuleC0002Active, validationDecider.IsRuleC0002Active);
		}

		public void TestIsRuleC0211Active()
		{
			AssertEquals(ExpectedIsRuleC0211Active, validationDecider.IsRuleC0211Active);
		}

		public void TestIsRuleC0623ActiveForAddInfoBox18TransportID()
		{
			AssertEquals(ExpectedIsRuleC0623Active, validationDecider.IsRuleC0623Active);
		}

		public void TestIsRuleC0646ActiveForAddInfoBox18TransportID()
		{
			AssertEquals(ExpectedIsRuleC0646Active, validationDecider.IsRuleC0646Active);
		}

		public void TestIsRuleC0729ActiveForIncoterm()
		{
			AssertEquals(ExpectedIsRuleC0729Active, validationDecider.IsRuleC0729Active);
		}

		public void TestIsRuleC0738ActiveForIncoterm()
		{
			AssertEquals(ExpectedIsRuleC0738Active, validationDecider.IsRuleC0738Active);
		}

		public void TestIsRuleC0841Active()
		{
			AssertEquals(ExpectedIsRuleC0841Active, validationDecider.IsRuleC0841Active);
		}

		public void TestIsRuleC0843Active()
		{
			AssertEquals(ExpectedIsRuleC0843Active, validationDecider.IsRuleC0843Active);
		}

		protected abstract bool ExpectedIsRuleC0002Active { get; }

		protected abstract bool ExpectedIsRuleC0211Active { get; }

		protected abstract bool ExpectedIsRuleC0623Active { get; }

		protected abstract bool ExpectedIsRuleC0646Active { get; }

		protected abstract bool ExpectedIsRuleC0729Active { get; }

		protected abstract bool ExpectedIsRuleC0738Active { get; }

		protected abstract bool ExpectedIsRuleC0841Active { get; }

		protected abstract bool ExpectedIsRuleC0843Active { get; }

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
