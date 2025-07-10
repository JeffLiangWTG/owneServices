using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMDeclarationWrapperTest : SADDeclarationWrapperTest<IMDeclarationWrapper>
{
	public override void TestTypeDeclarationSubType3()
	{
		AssertEquals(ZString.Empty, sadDeclarationWrapper.TypeDeclarationSubType3);
	}

	protected override IMDeclarationWrapper GetDeclarationWrapper(JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction) => new IMDeclarationWrapper(jobDeclaration, entryInstruction);
}
