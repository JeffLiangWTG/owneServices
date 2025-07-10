namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryHeaderValidationTest : EU.Business.Declaration.Testing.CusEntryHeaderValidationTest
{
	public void TestEntryLinesCountLimitForImportEntry()
	{
		declaration.JE_MessageType = "IMP";

		var validation = new CusEntryHeaderValidationWithEntryLinesCountLimitExposed(entryHeader);
		AssertEquals("Entry Lines Count Limit", 999, validation.GetEntryLinesCountLimitExposed());
	}

	public void TestEntryLinesCountLimitForExportUcc6Entry()
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";

		AssertEquals("Is UCC6", true, declaration.IsUCC6);

		var validation = new CusEntryHeaderValidationWithEntryLinesCountLimitExposed(entryHeader);
		AssertEquals("Entry Lines Count Limit", 9999, validation.GetEntryLinesCountLimitExposed());
	}

	public void TestEntryLinesCountLimitForNonUcc6Entry()
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "TXT";

		AssertEquals("Is UCC6", false, declaration.IsUCC6);

		var validation = new CusEntryHeaderValidationWithEntryLinesCountLimitExposed(entryHeader);
		AssertEquals("Entry Lines Count Limit", 40, validation.GetEntryLinesCountLimitExposed());
	}

	public void TestEntryLinesCountLimitForStandaloneEntry()
	{
		var standaloneEntryHeader = Factory.New<CusEntryHeader>();

		var validation = new CusEntryHeaderValidationWithEntryLinesCountLimitExposed(standaloneEntryHeader);
		AssertEquals("Entry Lines Count Limit", 40, validation.GetEntryLinesCountLimitExposed());
	}

	public void TestCheckLinesCount()
	{
		const string messageError = "This Entry has more than 2 Lines, consider adding a new Entry Instruction.";

		var validationWithOverride = new CusEntryHeaderValidationWithEntryLinesCountLimitOverride(entryHeader);
		entryHeader.ClearRowNotifications();
		validationWithOverride.CheckLinesCount();
		AssertNoRowMessageError("When the entry does not have entry lines", entryHeader, messageError);

		entryHeader.MergedLines.AddNew();
		entryHeader.MergedLines.AddNew();
		entryHeader.ClearRowNotifications();
		validationWithOverride.CheckLinesCount();
		AssertNoRowMessageError("When the entry lines limit has been reached", entryHeader, messageError);

		entryHeader.MergedLines.AddNew();
		entryHeader.ClearRowNotifications();
		validationWithOverride.CheckLinesCount();
		AssertHasRowMessageError("When the entry lines limit has been exceeded", entryHeader, messageError);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	class CusEntryHeaderValidationWithEntryLinesCountLimitExposed : CusEntryHeaderValidation
	{
		public CusEntryHeaderValidationWithEntryLinesCountLimitExposed(CusEntryHeader parent) : base(parent)
		{
		}

		public int GetEntryLinesCountLimitExposed() => GetEntryLinesCountLimit();
	}

	class CusEntryHeaderValidationWithEntryLinesCountLimitOverride : CusEntryHeaderValidation
	{
		public CusEntryHeaderValidationWithEntryLinesCountLimitOverride(CusEntryHeader parent) : base(parent)
		{
		}

		protected override int GetEntryLinesCountLimit() => 2;
	}
}
