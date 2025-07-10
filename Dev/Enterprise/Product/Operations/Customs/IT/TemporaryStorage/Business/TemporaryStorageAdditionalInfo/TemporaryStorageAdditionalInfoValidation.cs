using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageAdditionalInfoValidation : EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoValidation
{
	public TemporaryStorageAdditionalInfoValidation(TemporaryStorageAdditionalInfo parent) : base(parent)
	{
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();

		ValidateTypeAndDescription(Parent.CSI_DescriptionInfo);
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		ValidateTypeAndDescription(Parent.CSI_CodeInfo);
	}

	void ValidateTypeAndDescription(ZPropertyInfo propertyInfo)
	{
		if (Parent.IsAnAdditionalInformation && IsCSI_DescriptionAndCodeEmpty())
		{
			propertyInfo.AddMessageError(ValidationCaptions.TemporaryStorageAdditionalInfo.TypeOrDescriptionRequired);
		}
	}

	bool IsCSI_DescriptionAndCodeEmpty() => Parent.CSI_Description.IsEmpty && Parent.CSI_Code.IsEmpty;
}
