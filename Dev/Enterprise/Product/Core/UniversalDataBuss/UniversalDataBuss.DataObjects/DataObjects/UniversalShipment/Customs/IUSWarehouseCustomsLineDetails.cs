using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IUSWarehouseCustomsLineDetails : IWarehouseCustomsLineDetails
	{
		ZString? ZoneStatus { get; }
		ZBool? FromOtherFTZ { get; }
		OutwardType? OutwardType { get; }
	}

	public enum OutwardType { Consumption, Exports, ToOtherFTZ }
}