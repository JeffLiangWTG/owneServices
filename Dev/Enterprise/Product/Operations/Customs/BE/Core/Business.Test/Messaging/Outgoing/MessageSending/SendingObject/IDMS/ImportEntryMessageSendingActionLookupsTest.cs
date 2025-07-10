using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(ImportEntryMessageSendingActionLookups))]
sealed class ImportEntryMessageSendingActionLookupsTest : BEJobDeclarationMessageSendingObjectLookupsTest<ImportEntryMessageSendingActionLookups, ImportEntryMessageSendingAction>
{
	public override void TestEntryTypeList()
	{
		AssertEquals("DEC", lookups.EntryTypeList.CodesAsString);
		AssertEquals("Import Declaration", lookups.EntryTypeList.GetDescriptionFromCode("DEC"));
	}
}
