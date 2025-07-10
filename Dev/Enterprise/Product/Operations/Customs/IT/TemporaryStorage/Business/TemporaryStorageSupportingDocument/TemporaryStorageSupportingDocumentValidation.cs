using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageSupportingDocumentValidation : EU.Business.CusTempStorage.TemporaryStorageSupportingDocumentValidation
{
	public TemporaryStorageSupportingDocumentValidation(TemporaryStorageSupportingDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
	}
}
