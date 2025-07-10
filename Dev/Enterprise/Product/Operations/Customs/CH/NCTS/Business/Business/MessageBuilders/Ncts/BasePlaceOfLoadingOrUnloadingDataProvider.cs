using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class BasePlaceOfLoadingOrUnloadingDataProvider : IPlaceOfLoadingOrUnloading
{
	protected BasePlaceOfLoadingOrUnloadingDataProvider(NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = movementHeader;
	}
	protected readonly NctsDepartureMovementHeader movementHeader;

	protected virtual ZString PortCode { get; }

	protected virtual ZString PortLocation { get; }

	public string UNLocode => PortCode.Length > 2 ? (string)PortCode : null;

	public string Location => PortLocation.ReturnNullIfEmpty();

	public string Country => PortCode.Left(2).ReturnNullIfEmpty();
}
