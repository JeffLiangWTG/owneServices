using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IWarehouseCustomsOutwardEntryNumberProvider
	{
		ZString? GetEntryNumber();
	}
}
