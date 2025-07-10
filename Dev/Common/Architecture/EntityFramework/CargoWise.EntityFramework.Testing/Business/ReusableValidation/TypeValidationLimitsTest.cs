using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TypeValidationLimitsTest : TestCase
	{
		public void TestConstructors()
		{
			TypeValidationLimits typeValidationLimits = new TypeValidationLimits();
			AssertEquals("Default FutureYearsBeforeError", 5, typeValidationLimits.FutureYearsBeforeError);
			AssertEquals("Default PastYearsBeforeError", 10, typeValidationLimits.PastYearsBeforeError);
			AssertEquals("Default FutureYearsBeforeWarning", 1, typeValidationLimits.FutureYearsBeforeWarning);
			AssertEquals("Default PastYearsBeforeWarning", 1, typeValidationLimits.PastYearsBeforeWarning);

			typeValidationLimits = new TypeValidationLimits() { FutureYearsBeforeError = 20, FutureYearsBeforeWarning = 2, PastYearsBeforeError = 15, PastYearsBeforeWarning = 3 };
			AssertEquals("Overridden FutureYearsBeforeError", 20, typeValidationLimits.FutureYearsBeforeError);
			AssertEquals("Overridden PastYearsBeforeError", 15, typeValidationLimits.PastYearsBeforeError);
			AssertEquals("Overridden FutureYearsBeforeWarning", 2, typeValidationLimits.FutureYearsBeforeWarning);
			AssertEquals("Overridden PastYearsBeforeWarning", 3, typeValidationLimits.PastYearsBeforeWarning);

			typeValidationLimits.FutureYearsBeforeWarning = 5;
			typeValidationLimits.FutureYearsBeforeError = 21;
			typeValidationLimits.PastYearsBeforeError = 16;
			typeValidationLimits.PastYearsBeforeWarning = 4;
			AssertEquals("Overridden FutureYearsBeforeError", 21, typeValidationLimits.FutureYearsBeforeError);
			AssertEquals("Overridden PastYearsBeforeError", 16, typeValidationLimits.PastYearsBeforeError);
			AssertEquals("Overridden FutureYearsBeforeWarning", 5, typeValidationLimits.FutureYearsBeforeWarning);
			AssertEquals("Overridden PastYearsBeforeWarning", 4, typeValidationLimits.PastYearsBeforeWarning);
		}
	}
}
