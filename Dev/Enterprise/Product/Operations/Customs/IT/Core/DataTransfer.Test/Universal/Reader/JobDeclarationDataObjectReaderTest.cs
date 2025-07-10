using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.DataTransfer.Universal.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class JobDeclarationDataObjectReaderTest : JobDeclarationDataObjectReaderAbstractTest<JobDeclaration, JobDeclarationDataObjectReader>
{
	public void TestEntryInstructionDataObjectReaderType()
	{
		AssertType<CustomsEntryInstructionDataObjectReader>("EntryInstruction reader type", declarationDataObjectReader.CreateCustomsEntryInstructionDataObjectReaderExposed(new EntryInstruction(), declaration));
	}

	public void TestEntryHeaderDataObjectReaderType()
	{
		AssertType<CustomsEntryHeaderDataObjectReader>("EntryHeader reader type", declarationDataObjectReader.CreateCustomsEntryHeaderDataObjectReaderExposed(new EntryHeader(), declaration));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.BOFactory.New<JobDeclaration>();
		declarationDataObjectReader = new JobDeclarationDataObjectReaderForTest(new Shipment(), new TestErrorLogger(), Factory);
	}
	JobDeclaration declaration;
	JobDeclarationDataObjectReaderForTest declarationDataObjectReader;
}

class JobDeclarationDataObjectReaderForTest : JobDeclarationDataObjectReader
{
	public JobDeclarationDataObjectReaderForTest(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		: base(declarationDataObject, logger, factory, forwardingShipment: null)
	{
	}

	public Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReaderExposed(EntryInstruction entryInstructionDataObject, EU.Business.Declaration.JobDeclaration declaration)
	{
		return CreateCustomsEntryInstructionDataObjectReader(entryInstructionDataObject, declaration);
	}

	public Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectReader CreateCustomsEntryHeaderDataObjectReaderExposed(EntryHeader entryHeaderDataObject, EU.Business.Declaration.JobDeclaration declaration, List<ZString> matchingKeys = null)
	{
		return CreateCustomsEntryHeaderDataObjectReader(entryHeaderDataObject, declaration, matchingKeys);
	}
}
