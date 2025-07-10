using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class EntrySelectionValidationTest : TestCaseWithFactory
{
	public void TestValidateSelected()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		var entrySelection1 = new EntrySelection(entryHeader);
		var entrySelection2 = new EntrySelection(entryHeader2);
		declaration.EntrySelections.Add(entrySelection1);
		declaration.EntrySelections.Add(entrySelection2);

		entrySelection1.Selected = true;
		entrySelection2.Selected = true;

		AssertHasError("There should be an error if there are 2 records selected", entrySelection2.SelectedInfo, "Only 1 entry can be selected.");

		entrySelection2.Selected = false;

		AssertNoError("There should be no error if only 1 record is selected", entrySelection2.SelectedInfo, "Only 1 entry can be selected.");
	}
}
