namespace Enterprise.Customs.EU.Business.CusTempStorage;

public interface ITemporaryStoragePackedItemValidationDecider
{
	bool IsAPI_TariffMandatory { get; }
	bool IsAPI_GoodsDescriptionMandatory { get; }
}
