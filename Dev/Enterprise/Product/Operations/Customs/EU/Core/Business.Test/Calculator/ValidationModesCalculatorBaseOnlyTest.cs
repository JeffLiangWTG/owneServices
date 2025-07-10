using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class ValidationModesCalculatorBaseOnlyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsThisValidationOn()
		{
			var testItem = ValidationModesCalculatorForBaseOnlyTest.New();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)1, ValidationModes.None), NUnit.Framework.Is.EqualTo(true), "1 <-> 1, true");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)2, ValidationModes.None), NUnit.Framework.Is.EqualTo(false), "2 <-> 1, false");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)3, ValidationModes.None), NUnit.Framework.Is.EqualTo(true), "3 <-> 1, true");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)4, ValidationModes.None), NUnit.Framework.Is.EqualTo(false), "4 <-> 1, false");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)5, ValidationModes.None), NUnit.Framework.Is.EqualTo(true), "5 <-> 1, true");

				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)1, ValidationModes.Original), NUnit.Framework.Is.EqualTo(false), "1 <-> 1, false");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)2, ValidationModes.Original), NUnit.Framework.Is.EqualTo(true), "2 <-> 1, true");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)3, ValidationModes.Original), NUnit.Framework.Is.EqualTo(true), "3 <-> 1, true");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)4, ValidationModes.Original), NUnit.Framework.Is.EqualTo(false), "4 <-> 1, false");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)5, ValidationModes.Original), NUnit.Framework.Is.EqualTo(false), "5 <-> 1, false");

				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)1, ValidationModes.Amendment), NUnit.Framework.Is.EqualTo(false), "1 <-> 1, false");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)2, ValidationModes.Amendment), NUnit.Framework.Is.EqualTo(false), "2 <-> 1, false");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)3, ValidationModes.Amendment), NUnit.Framework.Is.EqualTo(false), "3 <-> 1, false");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)4, ValidationModes.Amendment), NUnit.Framework.Is.EqualTo(true), "4 <-> 1, true");
				NUnit.Framework.Assert.That(testItem.IsThisValidationOn((ValidationModes)5, ValidationModes.Amendment), NUnit.Framework.Is.EqualTo(true), "5 <-> 1, true");
			});
		}

		[ExpectNoExceptions]
		public void TestUpdateValidationModes()
		{
			var support = new ValidationModesSupporterForBaseOnlyTest();

			var calculator = new ValidationModesCalculatorForBaseOnlyTest(support);

			CombineAssertions(() =>
			{
				AssertUpdateValidationModes(support, calculator, 0, ValidationModes.None, true, (ValidationModes)1);
				AssertUpdateValidationModes(support, calculator, 1, ValidationModes.None, true, (ValidationModes)1);
				AssertUpdateValidationModes(support, calculator, 2, ValidationModes.None, true, (ValidationModes)3);
				AssertUpdateValidationModes(support, calculator, 3, ValidationModes.None, true, (ValidationModes)3);
				AssertUpdateValidationModes(support, calculator, 4, ValidationModes.None, true, (ValidationModes)5);
				AssertUpdateValidationModes(support, calculator, 0, ValidationModes.None, false, 0);
				AssertUpdateValidationModes(support, calculator, 1, ValidationModes.None, false, 0);
				AssertUpdateValidationModes(support, calculator, 2, ValidationModes.None, false, (ValidationModes)2);
				AssertUpdateValidationModes(support, calculator, 3, ValidationModes.None, false, (ValidationModes)2);
				AssertUpdateValidationModes(support, calculator, 4, ValidationModes.None, false, (ValidationModes)4);

				AssertUpdateValidationModes(support, calculator, 1, ValidationModes.Original, true, (ValidationModes)3);
				AssertUpdateValidationModes(support, calculator, 2, ValidationModes.Original, true, (ValidationModes)2);
				AssertUpdateValidationModes(support, calculator, 3, ValidationModes.Original, true, (ValidationModes)3);
				AssertUpdateValidationModes(support, calculator, 4, ValidationModes.Original, true, (ValidationModes)6);
				AssertUpdateValidationModes(support, calculator, 5, ValidationModes.Original, true, (ValidationModes)7);
				AssertUpdateValidationModes(support, calculator, 1, ValidationModes.Original, false, (ValidationModes)1);
				AssertUpdateValidationModes(support, calculator, 2, ValidationModes.Original, false, 0);
				AssertUpdateValidationModes(support, calculator, 3, ValidationModes.Original, false, (ValidationModes)1);
				AssertUpdateValidationModes(support, calculator, 4, ValidationModes.Original, false, (ValidationModes)4);
				AssertUpdateValidationModes(support, calculator, 5, ValidationModes.Original, false, (ValidationModes)5);

				AssertUpdateValidationModes(support, calculator, 0, ValidationModes.Amendment, true, (ValidationModes)4);
				AssertUpdateValidationModes(support, calculator, 1, ValidationModes.Amendment, true, (ValidationModes)5);
				AssertUpdateValidationModes(support, calculator, 2, ValidationModes.Amendment, true, (ValidationModes)6);
				AssertUpdateValidationModes(support, calculator, 3, ValidationModes.Amendment, true, (ValidationModes)7);
				AssertUpdateValidationModes(support, calculator, 4, ValidationModes.Amendment, true, (ValidationModes)4);
				AssertUpdateValidationModes(support, calculator, 5, ValidationModes.Amendment, true, (ValidationModes)5);
				AssertUpdateValidationModes(support, calculator, 0, ValidationModes.Amendment, false, 0);
				AssertUpdateValidationModes(support, calculator, 1, ValidationModes.Amendment, false, (ValidationModes)1);
				AssertUpdateValidationModes(support, calculator, 2, ValidationModes.Amendment, false, (ValidationModes)2);
				AssertUpdateValidationModes(support, calculator, 3, ValidationModes.Amendment, false, (ValidationModes)3);
				AssertUpdateValidationModes(support, calculator, 5, ValidationModes.Amendment, false, (ValidationModes)1);
			});
		}

		[ExpectNoExceptions]
		void AssertUpdateValidationModes(IValidationModesSupporter support, ValidationModesCalculator calculator, int originalValidationModes, ValidationModes positionToSet, bool turnOnOrOff, ValidationModes expectedValue)
		{
			support.ValidationModes = (ValidationModes)originalValidationModes;
			calculator.UpdateValidationModes(positionToSet, turnOnOrOff);
			NUnit.Framework.Assert.That(support.ValidationModes, NUnit.Framework.Is.EqualTo(expectedValue), $"Originally {originalValidationModes}, attempt {positionToSet} {turnOnOrOff}, result should be {expectedValue}");
		}
	}

	class ValidationModesCalculatorForBaseOnlyTest : ValidationModesCalculator
	{
		public static ValidationModesCalculator New() => new ValidationModesCalculatorForBaseOnlyTest(new ValidationModesSupporterForBaseOnlyTest());

		public ValidationModesCalculatorForBaseOnlyTest(ValidationModesSupporterForBaseOnlyTest supporter) : base(supporter) { }

		public ValidationModes ValidationModesForRecalculateValidationModesCore { get; set; }
		protected override ValidationModes RecalculateValidationModesCore() => ValidationModesForRecalculateValidationModesCore;

		public new void UpdateValidationModes(ValidationModes modeToCheckAgainst, bool isSpecificModeEnabled)
		{
			UpdateValidationModes(modeToCheckAgainst, isSpecificModeEnabled);
		}
	}

	class ValidationModesSupporterForBaseOnlyTest : IValidationModesSupporter
	{
		ValidationModes fValidationModes;
		public ValidationModes ValidationModes { get => fValidationModes; set => fValidationModes = value; }
	}
}
