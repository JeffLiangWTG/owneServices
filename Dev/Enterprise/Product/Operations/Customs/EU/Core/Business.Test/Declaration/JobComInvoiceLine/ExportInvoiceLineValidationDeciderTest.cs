using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(IExportInvoiceLineValidationDecider))]
	public abstract class ExportInvoiceLineValidationDeciderTest<T> : TestCaseWithFactory
		where T : class, IExportInvoiceLineValidationDecider
	{
		public void TestIsRuleR0222()
		{
			AssertEquals(ExpectedIsRuleR0222Active, decider.IsRuleR0222Active);
		}
		protected abstract bool ExpectedIsRuleR0222Active { get; }

		public void TestIsRuleR0223()
		{
			AssertEquals(ExpectedIsRuleR0223Active, decider.IsRuleR0223Active);
		}
		protected abstract bool ExpectedIsRuleR0223Active { get; }

		public void TestIsRuleR0224()
		{
			AssertEquals(ExpectedIsRuleR0224Active, decider.IsRuleR0224Active);
		}
		protected abstract bool ExpectedIsRuleR0224Active { get; }

		protected override void SetUp()
		{
			base.SetUp();
			decider = GetNewValidationDecider();
		}

		protected virtual T GetNewValidationDecider()
		{
			return Activator.CreateInstance<T>();
		}

		protected T decider;
	}
}
