using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
sealed class CusEntryHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestGetDocumentWrappers_SADHDataContext()
	{
		var entryHeader = PrepareDataAndGetEntryHeader();
		var documentSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);
		var wrapper = documentSupporter.GetDocumentWrappers(DataContext.SADH, Factory.New<IStmMenuItem>())[0];
		CombineAssertions(() =>
		{
			AssertType<DEDocSADH>("Wrapper Type", wrapper);
			AssertEquals("Wrapped Object", entryHeader, wrapper.WrappedObject);
		});
	}

	public void TestGetDocumentWrappers_CusEntryHeaderDataContext()
	{
		var entryHeader = PrepareDataAndGetEntryHeader();
		var documentSupporter = new CusEntryHeaderDocumentSupporter(entryHeader);
		var wrapper = documentSupporter.GetDocumentWrappers(DataContext.CusEntryHeader, Factory.New<IStmMenuItem>())[0];
		CombineAssertions(() =>
		{
			AssertType<DocCusEntryHeader>("Wrapper Type", wrapper);
			AssertEquals("Wrapped Object", entryHeader, wrapper.WrappedObject);
		});
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => PrepareDataAndGetEntryHeader();

	CusEntryHeader PrepareDataAndGetEntryHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_MessageType = DeclarationApplicationCodeList.Codes.Builtin;
		return entryHeader;
	}
}
