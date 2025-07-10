using CargoWise.Types;

namespace Enterprise.Integration.TransitWarehouse
{
	public interface IWhsItemConsignmentOrderReference
	{
		ZString ConsignmentOrderNumber { get; }
	}
}
