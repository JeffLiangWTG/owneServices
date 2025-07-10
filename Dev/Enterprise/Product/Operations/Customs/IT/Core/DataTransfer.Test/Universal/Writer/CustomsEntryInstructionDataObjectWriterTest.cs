using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class CustomsEntryInstructionDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
{
	public void TestPopulateSealInfoForExportDeclaration()
	{
		PopulateDeclarationWithSealsAndAssertExportedData("EXP", (universalEntryInstruction) => AssertOutputSealInfoFilled(universalEntryInstruction));
	}

	public void TestPopulateSealInfoForImportDeclaration()
	{
		PopulateDeclarationWithSealsAndAssertExportedData("IMP", (universalEntryInstruction) => AssertOutputSealInfoEmpty(universalEntryInstruction));
	}

	public void TestAddInfoSealCountIsNotPopulated()
	{
		entryInstruction.ZG_SealsCount = 99;
		var universalEntryInstruction = writer.GetDataObject(entryInstruction);
		var sealsCountAddInfo = universalEntryInstruction.AddInfoCollection.SingleOrDefault(x => x.Key.GetValueOrDefault() == "SealsCount");
		AssertNull("SealsCount AddInfo", sealsCountAddInfo);
	}

	void PopulateDeclarationWithSealsAndAssertExportedData(ZString messageType, Action<UniversalDataBuss.DataObjects.Universal.Customs.EntryInstruction> assertAction)
	{
		declaration.JE_MessageType = messageType;
		entryInstruction.ZG_SealsCount = 99;
		entryInstruction.Seals.AddNew().CY_Data = "ABC";
		entryInstruction.Seals.AddNew().CY_Data = "DEF";
		entryInstruction.Seals.AddNew().CY_Data = "GHI";
		entryInstruction.Seals.AddNew().CY_Data = "";

		var universalEntryInstruction = writer.GetDataObject(entryInstruction);
		assertAction(universalEntryInstruction);
	}

	void AssertOutputSealInfoFilled(UniversalDataBuss.DataObjects.Universal.Customs.EntryInstruction universalEntryInstruction)
	{
		AssertNotNull("SealInfo", universalEntryInstruction.SealInfo);
		AssertNotNull("SealNumberCollection", universalEntryInstruction.SealNumberCollection);
		AssertEquals("SealInfo.Quantity", 99, universalEntryInstruction.SealInfo.Quantity);
		AssertContainsExactElementsInAnyOrder("SealNumberCollection", new ZString[] { "ABC", "DEF", "GHI" }, universalEntryInstruction.SealNumberCollection.Select(x => x.Number).ToArray());
	}

	void AssertOutputSealInfoEmpty(UniversalDataBuss.DataObjects.Universal.Customs.EntryInstruction universalEntryInstruction)
	{
		AssertNull("SealInfo", universalEntryInstruction.SealInfo);
		AssertNull("SealNumberCollection", universalEntryInstruction.SealNumberCollection);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(null, entryInstruction)), new EU.DataTransfer.Universal.UniversalDataObjectWriterHelper(Factory.BOFactory, "IT"));
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CustomsEntryInstructionDataObjectWriter writer;
}
