using CargoWise.Types;
using WTG.Rules;
using RuleNotificationType = WTG.Rules.NotificationType;

namespace CargoWise.EntityFramework.Testing
{
	static class TestValidationMethods
	{
		[ValidationRule("GreaterThan", "Fail if A < B")]
		public static RuleValidationResult GreaterThan(ZInt a, int b)
		{
			if (a <= b)
			{
				return RuleValidationResult.New("Must be greater than " + b, RuleNotificationType.Error);
			}

			return null;
		}

		[ValidationRule("AddWarning", "Always returns a warning")]
		public static RuleValidationResult AddWarning()
		{
			return RuleValidationResult.New("Be warned...", RuleNotificationType.Warning);
		}

		[ValidationRule("WarnIfNotEqual", "Should be equal")]
		public static RuleValidationResult WarnIfNotEqual(ZString a, ZString b)
		{
			return !Equals(a, b) ?
				RuleValidationResult.New($"'{a}' is not the same as '{b}'", RuleNotificationType.Warning) :
				RuleValidationResult.ValidResult;
		}
	}
}
