namespace Enterprise.Customs.EU.Business.CusTempStorage;

public sealed class TemporaryStoragePackedItemValidationDecider : ITemporaryStoragePackedItemValidationDecider
{
	public bool IsAPI_TariffMandatory => true;
	public bool IsAPI_GoodsDescriptionMandatory => true;
}
