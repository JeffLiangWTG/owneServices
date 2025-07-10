using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryHeaderManualReleaseTest : TestCaseWithFactory
{
	public void TestManualRelease_AddNew()
	{
		AssertEquals("PRE-CONDITION", 0, GetReleaseEntryNumbers(entryHeader).Length);
		var today = ZDate.Today;

		var manualRelease = new EntryManualReleaseHandler(entryHeader);
		manualRelease.ReleaseCode = "ARG01";
		manualRelease.ReleaseDate = today;

		entryHeader.DoManualReleaseAndSetAsChanged(manualRelease);

		var entryNumbers = GetReleaseEntryNumbers(entryHeader);

		CombineAssertions("New Release number", () =>
		{
			AssertEquals("Release Entry Number must be found", 1, entryNumbers.Length);
			var entryNumber = entryNumbers[0];

			AssertEquals("Release code must be ARG01", "ARG01", entryNumber.CE_EntryNum);
			AssertEquals("Release date must be today", today, entryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated must be false", false, entryNumber.CE_EntryIsSystemGenerated);
		});

		AssertEquals("EntryReleaseDate must be equal to entryNumber IssueDate", today, entryHeader.CH_EntryReleaseDate);
	}

	public void TestManualRelease_UpdateExisting()
	{
		var yesterday = ZDate.Today.AddDays(-1);
		var entryNumber = entryNumbersProvider.InsertOrUpdateReleaseCode("ARG01", yesterday);
		AssertEquals("PRE-CONDITION", 1, GetReleaseEntryNumbers(entryHeader).Length);

		var today = ZDate.Today;
		var manualRelease = new EntryManualReleaseHandler(entryHeader);
		manualRelease.ReleaseCode = "OVRWRT";
		manualRelease.ReleaseDate = today;

		entryHeader.DoManualReleaseAndSetAsChanged(manualRelease);

		var entryNumbers = GetReleaseEntryNumbers(entryHeader);

		CombineAssertions("Existing Release number", () =>
		{
			AssertEquals("Release Entry Number must be found", 1, entryNumbers.Length);
			var overWrittenEntryNumber = entryNumbers[0];

			AssertEquals("Release code must be OVRWRT", "OVRWRT", overWrittenEntryNumber.CE_EntryNum);
			AssertEquals("Release date must be today", today, overWrittenEntryNumber.CE_IssueDate);
			AssertEquals("CE_EntryIsSystemGenerated must be false", false, entryNumber.CE_EntryIsSystemGenerated);
		});

		AssertEquals("EntryReleaseDate must be equal to entryNumber IssueDate", today, entryHeader.CH_EntryReleaseDate);
	}

	public void TestEntryStatusWhenDeclarationIsIMP()
	{
		var manualRelease = new EntryManualReleaseHandler(entryHeader);
		manualRelease.ReleaseCode = "ARG01";
		manualRelease.ReleaseDate = ZDate.Today;

		declaration.JE_MessageType = "IMP";
		entryHeader.CH_EntryStatus = ZString.Empty;

		entryHeader.DoManualReleaseAndSetAsChanged(manualRelease);

		AssertEquals("After manual release, IMP EntryHeader status must be ICC", "ICC", entryHeader.CH_EntryStatus);
	}

	public void TestEntryStatusWhenDeclarationIsEXP()
	{
		var manualRelease = new EntryManualReleaseHandler(entryHeader);
		manualRelease.ReleaseCode = "ARG01";
		manualRelease.ReleaseDate = ZDate.Today;

		declaration.JE_MessageType = "EXP";
		entryHeader.CH_EntryStatus = ZString.Empty;

		entryHeader.DoManualReleaseAndSetAsChanged(manualRelease);

		AssertEquals("After manual release, EXP EntryHeader status must be ECC", "ECC", entryHeader.CH_EntryStatus);
	}

	public void TestEntryHeaderIsSetAsChangedOnManualRelease()
	{
		AssertEquals("PRE-CONDITION", false, entryHeader.HasChanges);

		var manualRelease = new EntryManualReleaseHandler(entryHeader);
		manualRelease.ReleaseCode = "ARG01";
		manualRelease.ReleaseDate = new ZDate();

		entryHeader.DoManualReleaseAndSetAsChanged(manualRelease);
		AssertEquals("EntryHeader must have HasChanges true because EntryNumber is created", true, entryHeader.HasChanges);

		Factory.Save();

		manualRelease.ReleaseCode = "ARG02";
		entryHeader.DoManualReleaseAndSetAsChanged(manualRelease);
		AssertEquals("EntryHeader must have HasChanges true also when EntryNumber is updated", true, entryHeader.HasChanges);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryNumbersProvider = new CusEntryHeaderEntryNumbersProvider(entryHeader);
	}
	CusEntryHeader entryHeader;
	CusEntryHeaderEntryNumbersProvider entryNumbersProvider;
	JobDeclaration declaration;

	CusEntryNumber[] GetReleaseEntryNumbers(CusEntryHeader entryHeader)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberConstants.EntryTypes.ClereanceCode);
		return Factory.Load<CusEntryNumber>(query);
	}
}
