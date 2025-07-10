using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitHeaderDeclarationWrapperTest : NctsSADHeaderDeclarationWrapperTest<TransitHeaderDeclarationWrapper>
{
	protected override ZString ExpectedTypeDeclarationSubType2 => "A";

	protected override TransitHeaderDeclarationWrapper GetWrapper(NctsDepartureMovementHeader nctsDepartureMovementHeader) => new TransitHeaderDeclarationWrapper(nctsDepartureMovementHeader);
}
