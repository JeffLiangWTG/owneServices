using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitHeaderDeclarationWrapper : NctsSADHeaderDeclarationWrapper
{
	public TransitHeaderDeclarationWrapper(NctsDepartureMovementHeader nctsMovementHeader) : base(nctsMovementHeader)
	{
	}

	protected override ZString TypeDeclarationSubType2Core => ITEntrySubStyleList.Codes.StandardDeclarationA;
}
