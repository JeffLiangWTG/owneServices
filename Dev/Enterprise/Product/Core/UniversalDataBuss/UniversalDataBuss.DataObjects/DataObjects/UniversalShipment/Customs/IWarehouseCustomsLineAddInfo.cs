using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IWarehouseCustomsLineAddInfo
	{
		ZString Type { get; }
		ZString AddInfoData { get; }
		ZString NAddInfoData { get; }
	}
}
