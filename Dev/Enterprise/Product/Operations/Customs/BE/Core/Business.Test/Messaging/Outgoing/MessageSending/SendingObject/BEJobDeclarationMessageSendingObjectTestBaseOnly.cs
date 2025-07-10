using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class BEJobDeclarationMessageSendingObjectTestBaseOnly : TestCaseWithFactory
{
	public void TestProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("DeclarationType", "11", action.DeclarationType);
			AssertEquals("BGMReference", "REF000000001", action.BGMReference);
			AssertEquals("LocalReferenceNumber", "REF000000001", action.LocalReferenceNumber);
			AssertEquals("Description", "DESC", action.Description);
			AssertEquals("Test?", true, action.IsTestDeclaration);
		});
	}

	public void TestProcedureType()
	{
		AssertEquals("11", action.ProcedureType);
	}

	public void TestVariant()
	{
		AssertEquals("1", action.Variant);
	}

	public void TestTypeOfEntry_MaxLength()
	{
		AssertEquals(3, action.TypeOfEntryInfo.MaxLength);
	}

	public void TestTypeOfEntry_ReadOnly()
	{
		AssertEquals(false, action.TypeOfEntryInfo.ReadOnly);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.ZG_IsTrainingDeclaration = true;
		var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = "11";
		instruction.CEI_Description = "DESC";
		instruction.CEI_SubStyle = "1";
		var invoice = declaration.Invoices.AddNew();
		var line = invoice.InvoiceLines.AddNew();
		line.JI_CEI = instruction.PK;

		var entry = declaration.CustomsEntryHeaders.AddNew();
		entry.CH_CEI_Instruction = instruction.PK;
		entry.CH_MessageType = "CUS";
		entry.CH_EntryStatus = "09";
		entry.CH_Status = "CLO";
		entry.CH_BGMReference = "REF000000001";
		entry.EntryNumber = "ENT00000001";
		action = new BEJobDeclarationMessageSendingObjectForTesting(entry);
	}
	BEJobDeclarationMessageSendingObjectForTesting action;
}
