using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;
sealed class TemporaryStoragePackedItemValidationDecider : ITemporaryStoragePackedItemValidationDecider
{
	public bool IsAPI_TariffMandatory => false;
	public bool IsAPI_GoodsDescriptionMandatory => false;
}
