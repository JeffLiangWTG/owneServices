using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETDeclarationWrapperTest : SADDeclarationWrapperTest<ETDeclarationWrapper>
{
	public override void TestTypeDeclarationSubType3()
	{
		jobDeclaration.ZG_CTStatusID = ZString.Empty;
		AssertEquals(ZString.Empty, sadDeclarationWrapper.TypeDeclarationSubType3);

		jobDeclaration.ZG_CTStatusID = "ABC";
		AssertEquals("ABC", sadDeclarationWrapper.TypeDeclarationSubType3);
	}

	protected override ETDeclarationWrapper GetDeclarationWrapper(JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction) => new ETDeclarationWrapper(jobDeclaration, entryInstruction);
}
