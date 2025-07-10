using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class CustomsEntryInstructionDataObjectReaderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
{
	public void TestPopulateSealInfoForExportDeclaration()
	{
		PopulateDeclarationAndUniversalShipmentWithSealsAndAssertImportedData("EXP", (entryInstruction) => AssertImportedSealsInfoFilled(entryInstruction));
	}

	public void TestPopulateSealInfoForImportDeclaration()
	{
		PopulateDeclarationAndUniversalShipmentWithSealsAndAssertImportedData("IMP", (entryInstruction) => AssertImportedSealsInfoEmpty(entryInstruction));
	}

	void PopulateDeclarationAndUniversalShipmentWithSealsAndAssertImportedData(ZString messageType, Action<CusEntryInstruction> assertAction)
	{
		declaration.JE_MessageType = messageType;

		var sealNumbers = new List<SealNumber>()
		{
			new SealNumber() { Number = "ABC" },
			new SealNumber() { Number = "DEF" },
			new SealNumber() { Number = "GHI" },
			new SealNumber() { Number = "" },
			null,
		};

		var universalShipmentEntryInstruction = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance);
		universalShipmentEntryInstruction.SealInfo = new UniversalDataBuss.DataObjects.Universal.SealInfo() { Quantity = 99 };
		universalShipmentEntryInstruction.SetSealNumberCollection(() => sealNumbers);

		var customsEntryInstructionDataObjectReader = new CustomsEntryInstructionDataObjectReader(universalShipmentEntryInstruction, new TestErrorLogger(), new UniversalDataObjectReaderHelper(Factory, "IT", "IT"), Factory, declaration);
		var createdEntryInstruction = (CusEntryInstruction)customsEntryInstructionDataObjectReader.ReadIntoBusinessObject();
		assertAction(createdEntryInstruction);
	}

	void AssertImportedSealsInfoFilled(CusEntryInstruction createdEntryInstruction)
	{
		AssertEquals("ZG_SealsCount", 99, createdEntryInstruction.ZG_SealsCount);
		AssertContainsExactElementsInAnyOrder("Seals", new ZString[] { "ABC", "DEF", "GHI" }, createdEntryInstruction.Seals.Cast<EU.Business.SealNumber>().Select(x => x.CY_Data).ToArray());
	}

	void AssertImportedSealsInfoEmpty(CusEntryInstruction createdEntryInstruction)
	{
		AssertEquals("ZG_SealsCount", 0, createdEntryInstruction.ZG_SealsCount);
		AssertEquals("Seals Count", 0, createdEntryInstruction.Seals.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;
}
