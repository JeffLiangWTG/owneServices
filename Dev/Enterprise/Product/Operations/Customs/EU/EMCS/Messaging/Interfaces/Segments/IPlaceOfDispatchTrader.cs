using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IPlaceOfDispatchTrader : ITrader
	{
		ZString ReferenceOfTaxWarehouse { get; set; }
	}
}
