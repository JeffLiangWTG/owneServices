using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IPreviousOperationInfo
{
	IPreviousAdministrativeReference PreviousAllibrament { get; }
	ZString MRN { get; }
	IPreviousAdministrativeReference PreviousProcedure { get; }
	ZInt? NumberOfPackages { get; }
	ZDecimal? GrossMass { get; }
	ZString CombinedNomenclature { get; }
	ZDecimal? NetMass { get; }
	ZDecimal? SupplementaryUnit { get; }
}
