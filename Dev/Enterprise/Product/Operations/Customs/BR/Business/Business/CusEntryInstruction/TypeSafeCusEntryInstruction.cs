namespace Enterprise.Customs.BR.Business;

partial class CusEntryInstruction
{
	#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

	public new CusEntryInstruction Clone() => (CusEntryInstruction)base.Clone();

	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

	#endregion

	#region Implementation

	#region protected override

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

	#endregion

	#endregion
}
