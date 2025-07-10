using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public sealed class ImportEntryMessageSendingActionLookups : BEJobDeclarationMessageSendingObjectLookups<ImportEntryMessageSendingAction>
{
	public ImportEntryMessageSendingActionLookups(ImportEntryMessageSendingAction action) : base(action)
	{
	}

	public override CodeDescriptionPairList EntryTypeList => Factory.GetCachedValue<BEImportEntryTypeList>();
}
