namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EntryInstructionValidationModesCalculator : ValidationModesCalculator
	{
		public EntryInstructionValidationModesCalculator(CusEntryInstruction instruction)
			: base(instruction)
		{ }

		protected new CusEntryInstruction supporter => (CusEntryInstruction)base.supporter;

		protected override ValidationModes RecalculateValidationModesCore() => ValidationModes.None;
	}
}
