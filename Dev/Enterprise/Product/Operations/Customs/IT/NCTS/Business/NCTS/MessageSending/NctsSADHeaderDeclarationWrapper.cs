using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public abstract class NctsSADHeaderDeclarationWrapper : IDeclaration
{
	protected NctsSADHeaderDeclarationWrapper(NctsDepartureMovementHeader nctsMovementHeader)
	{
		this.nctsMovementHeader = Argument.NotNull(nctsMovementHeader, nameof(nctsMovementHeader));
	}
	readonly NctsDepartureMovementHeader nctsMovementHeader;

	public ZString TypeDeclarationSubType1 => ZString.Empty;

	public ZString TypeDeclarationSubType2 => TypeDeclarationSubType2Core;
	protected abstract ZString TypeDeclarationSubType2Core { get; }

	public ZString TypeDeclarationSubType3 => nctsMovementHeader.BM_InBondEntryType;
}
