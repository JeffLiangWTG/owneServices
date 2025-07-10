using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IWarehouseCustomsLinePackDetails
	{
		ZString PackID { get; }
		ZInt PackageQty { get; }
		ZDecimal PackedQty { get; }
	}
}
