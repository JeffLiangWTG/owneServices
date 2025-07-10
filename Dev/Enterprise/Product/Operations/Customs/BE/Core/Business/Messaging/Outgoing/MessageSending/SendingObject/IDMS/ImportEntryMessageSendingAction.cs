using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;
public sealed class ImportEntryMessageSendingAction : BEJobDeclarationMessageSendingObject, IObsoleteValidation
{
	public ImportEntryMessageSendingAction(Declaration.CusEntryHeader entry) : base(entry)
	{
	}
	public override ZString TypeOfEntry
	{
		get => typeOfEntry;
		set
		{
			CheckMaximumLength(TypeOfEntryInfo, value);
			SetNonPersistentPropertyValue(TypeOfEntryInfo, ref typeOfEntry, value);

			if (!IsValidationSuspended)
			{
				Validation.ValidateTypeOfEntry();
			}
		}
	}
	ZString typeOfEntry;

	public ImportEntryMessageSendingActionLookups Lookups => fLookups ?? (fLookups = new ImportEntryMessageSendingActionLookups(this));
	ImportEntryMessageSendingActionLookups fLookups;

	public new ImportEntryMessageSendingActionValidation Validation => (ImportEntryMessageSendingActionValidation)base.Validation;

	protected override JobDeclarationMessageSendingObjectValidation GetNewValidation() => new ImportEntryMessageSendingActionValidation(this);
}
