namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IWarehouseCustomsDetailsChangeOfRegime
	{
		CustomsRegime IntoRegimeType { get; }
		CustomsRegime OutOfRegimeType { get; }
		OrganizationAddress NewWarehouse { get; }
	}
}
