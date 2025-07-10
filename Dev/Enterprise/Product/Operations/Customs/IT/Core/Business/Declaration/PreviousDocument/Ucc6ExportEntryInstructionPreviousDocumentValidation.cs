namespace Enterprise.Customs.IT.Business.Declaration;

class Ucc6ExportEntryInstructionPreviousDocumentValidation : PreviousDocumentValidation
{
	public Ucc6ExportEntryInstructionPreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidatePreviousDocumentPresenceWithEntrySubStyleRuleB1905(Parent);
	}

	protected override void CheckCSI_Procedure()
	{
		// Intentionally kept blank: Validation not required
	}

	protected override void CheckCSI_SubType()
	{
		// Intentionally kept blank: Validation not required
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		// Intentionally kept blank: Validation not required
	}

	protected override void CheckCSI_UnitOfQuantity3()
	{
		// Intentionally kept blank: Validation not required
	}

	protected override IPreviousDocumentReferenceNumberValidator GetReferenceNumberValidator()
		=> new Ucc6ExportEntryInstructionPreviousDocumentReferenceNumberValidator(Parent, GetSettings(Parent));

	void ValidatePreviousDocumentPresenceWithEntrySubStyleRuleB1905(PreviousDocument parent)
	{
		var entryInstruction = parent.Parent as CusEntryInstruction;
		var isTransitionPeriod = entryInstruction?.JobDeclaration?.IsTransitionPeriodAES30 ?? false;
		if (entryInstruction is null || !isTransitionPeriod)
		{
			return;
		}

		var subStyle = entryInstruction.CEI_SubStyle;
		if (subStyle != ITEntrySubStyleList.Codes.SupplementaryDeclarationX && subStyle != ITEntrySubStyleList.Codes.SupplementaryDeclarationY)
		{
			parent.AddRowMessageError(ValidationCaptions.EntryInstruction.PreviousDocumentMustNotPresentInEntryInstructionsForSubStyleNotXorYRuleB1905);
		}
	}
}
