using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.CusTempStorage;
sealed class TemporaryStoragePackedItemValidationDecider : ITemporaryStoragePackedItemValidationDecider
{
	public bool IsAPI_TariffMandatory => false;
	public bool IsAPI_GoodsDescriptionMandatory => true;
}
