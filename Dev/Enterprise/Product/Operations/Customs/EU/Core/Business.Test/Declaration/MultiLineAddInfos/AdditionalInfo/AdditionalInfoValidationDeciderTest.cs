using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestsSubclassesOf(typeof(IAdditionalInfoValidationDecider))]
	public abstract class AdditionalInfoValidationDeciderTest<T> : TestCaseWithFactory
		where T : class, IAdditionalInfoValidationDecider
	{
		public void TestIsBR2038RuleActive()
		{
			AssertEquals(ExpectedIsBR2038RuleActive, validationDecider.IsBR2038Rule);
		}

		public void TestIsC0612RuleActive()
		{
			AssertEquals(ExpectedIsC0612RuleActive, validationDecider.IsC0612Rule);
		}

		protected abstract bool ExpectedIsBR2038RuleActive { get; }

		protected abstract bool ExpectedIsC0612RuleActive { get; }

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
