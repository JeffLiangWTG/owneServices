using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(DeclarationValidationModesCalculator))]
	class DeclarationValidationModesCalculatorTest : ValidationModesCalculatorAbstractTest<DeclarationValidationModesCalculator, JobDeclaration>
	{
		protected override IEnumerable<(JobDeclaration support, ValidationModes expectedResult, string description)> GetRecalculateValidationModesTestList()
		{
			var declaration = Factory.New<JobDeclaration>();
			yield return (declaration, ValidationModes.None, "ValidationModes.None for empty Job.");

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.ValidationModes = ValidationModes.None;
			yield return (declaration, ValidationModes.None, "ValidationModes.None for 1 instruction with ValidationModes.None.");

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.ValidationModes = ValidationModes.Amendment;
			yield return (declaration, (ValidationModes)5, "5 for instructions {1, 4}");
		}
	}
}
