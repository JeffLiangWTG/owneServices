using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(ValidationModesCalculator))]
	public abstract class ValidationModesCalculatorAbstractTest<TCalculator, TSupport> : TestCaseWithFactory
		where TCalculator : ValidationModesCalculator
		where TSupport : IValidationModesSupporter
	{
		[ExpectNoExceptions]
		public void TestRecalculateValidationModes()
		{
			CombineAssertions(() =>
			{
				foreach (var testItem in GetRecalculateValidationModesTestList())
				{
					var support = testItem.support;
					var calculator = (TCalculator)Activator.CreateInstance(typeof(TCalculator), support);
					calculator.RecalculateValidationModes();
					var result = support.ValidationModes;
					NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(testItem.expectedResult), testItem.description);
				}
			});
		}

		protected abstract IEnumerable<(TSupport support, ValidationModes expectedResult, string description)> GetRecalculateValidationModesTestList();
	}
}
