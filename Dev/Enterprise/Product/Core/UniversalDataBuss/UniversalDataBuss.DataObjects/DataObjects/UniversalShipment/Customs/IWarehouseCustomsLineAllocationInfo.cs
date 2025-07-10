using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IWarehouseCustomsLineAllocationInfo
	{
		ZString AllocationKey { get; }
		ZDecimal Quantity { get; }
	}
}
