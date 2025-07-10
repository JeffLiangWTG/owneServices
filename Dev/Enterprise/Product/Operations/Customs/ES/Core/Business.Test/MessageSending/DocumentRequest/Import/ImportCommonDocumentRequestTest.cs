using System;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing;

public abstract class ImportCommonDocumentRequestTest<T> : EntryHeaderDocumentRequestTest<T>
	where T : ImportCommonDocumentRequest
{
	public void TestConstructorMRNCode()
	{
		AssertExceptionThrown<ArgumentException>("mrnCode null", () => GetDocumentRequestClass(declaration.CustomsEntryHeaders.AddNew(), "A"));
	}

	public abstract void TestRequestMissingDocuments_M031Doc();

	public abstract void TestRequestMissingDocuments_M032Doc();

	public abstract void TestRequestMissingDocuments_J031Doc();

	public abstract void TestRequestMissingDocuments_J032Doc();

	protected override void SetUp()
	{
		base.SetUp();

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		businessObject.CH_CEI_Instruction = entryInstruction.PK;
		businessObject.Factory.Save();
	}

	protected CusEntryInstruction entryInstruction;

	protected const string CanaryIslandCode = "61";

	protected void CanaryIslandSetUp()
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, CanaryIslandCode, "Test 61");
	}
}
