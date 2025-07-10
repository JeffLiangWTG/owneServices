using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(EntryManualReleaseHandler))]
sealed class EntryManualReleaseHandlerTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when EntryHeader is null", () => new EntryManualReleaseHandler(null));
	}

	public void TestReleaseCodeMaxLength()
	{
		var manualRelease = new EntryManualReleaseHandler(entryHeader);
		AssertEquals("Release Code MaxLength", 6, manualRelease.ReleaseCodeInfo.MaxLength);
	}

	public void TestValidationType()
	{
		var manualRelease = new EntryManualReleaseHandler(entryHeader);
		AssertType<EntryManualReleaseHandlerValidation>("Validation Type", manualRelease.Validation);
	}

	protected override BusinessObject GetNewBusinessObject() => new EntryManualReleaseHandler(entryHeader);

	public void TestCanDoManualRelease_WithNoRegistrationNumberNorMRN()
	{
		var handler = new EntryManualReleaseHandler(entryHeader);
		var result = handler.CanDoManualRelease();

		CombineAssertions("Cannot do manual release without REG nor MRN", () =>
		{
			AssertEquals("Expected false CanDoManualRelease", false, result.CanDoManualRelease);
			AssertEquals("Error message expected", "This Entry has no Registration number nor MRN, it is not possible to insert the Release code.", result.ErrorMessage);
		});
	}

	public void TestCanDoManualRelease_WhenEntryHasMRN()
	{
		entryHeader.MovementReferenceNumberSetter("123");
		var handler = new EntryManualReleaseHandler(entryHeader);
		var result = handler.CanDoManualRelease();

		CombineAssertions("Manual release must be possible with available MRN", () =>
		{
			AssertEquals("Expected true CanDoManualRelease", true, result.CanDoManualRelease);
			AssertEquals("No error message expected", ZString.Empty, result.ErrorMessage);
		});
	}

	public void TestCanDoManualRelease_WhenEntryHasRegistrationNumber()
	{
		Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-2343G", issueDate: null);
		var handler = new EntryManualReleaseHandler(entryHeader);
		var result = handler.CanDoManualRelease();

		CombineAssertions("Manual release must be possible with available REG", () =>
		{
			AssertEquals("Expected true CanDoManualRelease", true, result.CanDoManualRelease);
			AssertEquals("No error message expected", ZString.Empty, result.ErrorMessage);
		});
	}

	public void TestCanDoManualRelease_WithExistingSystemGeneratedReleaseCode()
	{
		Factory.NewCusEntryNumber(entryHeader, entryType: "CLR", entryNum: "4 T-2343G", issueDate: null);
		var handler = new EntryManualReleaseHandler(entryHeader);
		var result = handler.CanDoManualRelease();

		CombineAssertions("Manual release must not be possible with existing System generated CLR", () =>
		{
			AssertEquals("Expected false CanDoManualRelease", false, result.CanDoManualRelease);
			AssertEquals("Error message expected", "This Entry has a System generated Release Code, it is not possible to change it.", result.ErrorMessage);
		});
	}

	public void TestCanDoManualRelease_WithExistingNotSystemGeneratedReleaseCode()
	{
		entryHeader.MovementReferenceNumberSetter("123");
		var entryNum = Factory.NewCusEntryNumber(entryHeader, entryType: "CLR", entryNum: "4 T-2343G", issueDate: null);
		entryNum.CE_EntryIsSystemGenerated = false;

		var handler = new EntryManualReleaseHandler(entryHeader);
		var result = handler.CanDoManualRelease();

		CombineAssertions("Manual release must be possible when existing CLR is not System generated", () =>
		{
			AssertEquals("Expected true CanDoManualRelease", true, result.CanDoManualRelease);
			AssertEquals("No error message expected", ZString.Empty, result.ErrorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader = Factory.New<CusEntryHeader>();
	}
	CusEntryHeader entryHeader;
}
