using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class EntryInstructionProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
{
	public void TestCustomsEntryInstructions()
	{
		var test = provider.CustomsEntryInstructions;
		AssertType<CusEntryInstructionCollection>(provider.CustomsEntryInstructions);
	}

	public void TestEntryInstructionComparer()
	{
		AssertType<CusEntryInstructionComparer>(provider.EntryInstructionComparer);
	}

	public void TestDefaultValuesForExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		AssertEquals("Default style", "B1", instruction.CEI_Style);
		AssertEquals("Default substyle", "A", instruction.CEI_SubStyle);
		AssertEquals("Default procedure", "10", instruction.CEI_Procedure);
		AssertEquals("Default transaction nature", "11", instruction.ZG_TransNature);
	}

	protected override void SetUp()
	{
		base.SetUp();
		provider = new EntryInstructionProvider(Factory.New<JobDeclaration>());
	}
	EntryInstructionProvider provider;
}

