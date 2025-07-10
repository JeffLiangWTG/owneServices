using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TIRHeaderDeclarationWrapper : NctsSADHeaderDeclarationWrapper
{
	public TIRHeaderDeclarationWrapper(NctsDepartureMovementHeader nctsMovementHeader) : base(nctsMovementHeader)
	{
	}

	protected override ZString TypeDeclarationSubType2Core => ZString.Empty;
}
