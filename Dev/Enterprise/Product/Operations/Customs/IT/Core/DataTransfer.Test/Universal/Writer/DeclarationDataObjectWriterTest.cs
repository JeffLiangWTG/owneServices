using CargoWise.EntityFramework;
using Enterprise.Customs.EU.DataTransfer.Universal.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class DeclarationDataObjectWriterTest : DeclarationDataObjectWriterAbstractTest<JobDeclaration, DeclarationDataObjectWriter>
{
	public void TestEntryInstructionDataObjectWriterType()
	{
		((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
		var declaration = Factory.BOFactory.New<JobDeclaration>();
		var declarationDataObjectWriter = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
		declarationDataObjectWriter.GetDataObject(declaration);
		AssertType<CustomsEntryInstructionDataObjectWriter>("EntryInstruction writer type", declarationDataObjectWriter.GetNewCustomsEntryInstructionDataObjectWriterExposed());
	}

	public void TestEntryHeaderDataObjectWriterType()
	{
		((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
		var declaration = Factory.BOFactory.New<JobDeclaration>();
		var declarationDataObjectWriter = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
		declarationDataObjectWriter.GetDataObject(declaration);
		AssertType<CustomsEntryHeaderDataObjectWriter>("EntryHeader writer type", declarationDataObjectWriter.GetNewCustomsEntryHeaderDataObjectWriterExposed());
	}
}

class DeclarationDataObjectWriterForTest : DeclarationDataObjectWriter
{
	public DeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
	{
	}

	public Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriterExposed() => GetNewCustomsEntryInstructionDataObjectWriter();
	public Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriterExposed() => GetNewCustomsEntryHeaderDataObjectWriter();
}
