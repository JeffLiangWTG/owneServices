namespace Enterprise.Customs.EU.Business.Declaration
{
	public class DeclarationValidationModesCalculator : ValidationModesCalculator
	{
		public DeclarationValidationModesCalculator(JobDeclaration declaration)
			: base(declaration)
		{ }

		protected override ValidationModes RecalculateValidationModesCore()
		{
			var result = ValidationModes.None;
			foreach (var instruction in supporter.CustomsEntryInstructions)
			{
				result |= instruction.ValidationModes;
			}
			return result;
		}

		protected new JobDeclaration supporter => (JobDeclaration)base.supporter;
	}
}
