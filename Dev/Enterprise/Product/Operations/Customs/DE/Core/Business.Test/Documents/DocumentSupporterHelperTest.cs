using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.DocumentWrappers;

namespace Enterprise.Customs.DE.Business.Documents.Testing;
sealed class DocumentSupporterHelperTest : TestCaseWithFactory
{
	public void TestGetSADHWrappers_Single()
	{
		var entryHeader = PrepareDataAndGetEntryHeader();
		var wrapper = DocumentSupporterHelper.GetSADHWrappers(entryHeader).Single();
		CombineAssertions(() =>
		{
			AssertSame("Wrapped Object", wrapper.WrappedObject, entryHeader);
			AssertType<DEDocSADH>("Wrapper Type", wrapper);
		});
	}

	public void TestGetSADHWrappers_Multiple()
	{
		var entryHeaders = PrepareDataAndGetMultipleEntryHeaders();
		var wrappers = DocumentSupporterHelper.GetSADHWrappers(entryHeaders);
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Wrapped Objects", wrappers.Select(x => x.WrappedObject), new[] { entryHeaders[0], entryHeaders[1] });
			AssertType<DEDocSADH>("Wrapper Type1", wrappers[0]);
			AssertType<DEDocSADH>("Wrapper Type2", wrappers[1]);
		});
	}

	public void TestGetCusEntryHeaderWrappers_Single()
	{
		var entryHeader = PrepareDataAndGetEntryHeader();
		var wrapper = DocumentSupporterHelper.GetCusEntryHeaderWrappers(entryHeader).Single();
		CombineAssertions(() =>
		{
			AssertSame("Wrapped Object", wrapper.WrappedObject, entryHeader);
			AssertType<DocCusEntryHeader>("Wrapper Type", wrapper);
		});
	}

	public void TestetCusEntryHeaderWrappers_Multiple()
	{
		var entryHeaders = PrepareDataAndGetMultipleEntryHeaders();
		var wrappers = DocumentSupporterHelper.GetCusEntryHeaderWrappers(entryHeaders);
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Wrapped Objects", wrappers.Select(x => x.WrappedObject), new[] { entryHeaders[0], entryHeaders[1] });
			AssertType<DocCusEntryHeader>("Wrapper Type1", wrappers[0]);
			AssertType<DocCusEntryHeader>("Wrapper Type2", wrappers[1]);
		});
	}

	CusEntryHeader PrepareDataAndGetEntryHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_MessageType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		return entryHeader;
	}

	CusEntryHeader[] PrepareDataAndGetMultipleEntryHeaders()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
		entryHeader1.CH_MessageType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
		entryHeader2.CH_MessageType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		return new[] { entryHeader1, entryHeader2 };
	}
}
