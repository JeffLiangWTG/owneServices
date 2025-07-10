using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TIRHeaderDeclarationWrapperTest : NctsSADHeaderDeclarationWrapperTest<TIRHeaderDeclarationWrapper>
{
	protected override ZString ExpectedTypeDeclarationSubType2 => ZString.Empty;

	protected override TIRHeaderDeclarationWrapper GetWrapper(NctsDepartureMovementHeader nctsDepartureMovementHeader) => new TIRHeaderDeclarationWrapper(nctsDepartureMovementHeader);
}
