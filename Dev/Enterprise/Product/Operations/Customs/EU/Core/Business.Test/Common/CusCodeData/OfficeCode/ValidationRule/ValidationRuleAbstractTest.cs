using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(ValidationRule))]
	public abstract class ValidationRuleAbstractTest<T> : TestCaseWithFactory
		where T : ValidationRule
	{
		public abstract void TestIsApplied();

		public abstract void TestValidate();

		protected abstract T Rule { get; }
	}
}
