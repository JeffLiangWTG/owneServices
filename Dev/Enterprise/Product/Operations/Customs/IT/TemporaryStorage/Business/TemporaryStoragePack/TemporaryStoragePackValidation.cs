using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStoragePackValidation : EU.Business.CusTempStorage.TemporaryStoragePackValidation
{
	public TemporaryStoragePackValidation(TemporaryStoragePack parent) : base(parent)
	{
	}

	protected override void CheckAPA_MarksAndNumbers()
	{
		base.CheckAPA_MarksAndNumbers();

		const int marksAndNumbersMaxLength = 512;
		var parent = Parent;

		if (parent.APA_MarksAndNumbers.Length > marksAndNumbersMaxLength)
		{
			parent.APA_MarksAndNumbersInfo.AddMessageError(ValidationCaptions.TemporaryStoragePack.MarksAndNumbersExceedMaxLength(marksAndNumbersMaxLength));
		}
	}

	protected override void CheckAPA_PackQty()
	{
		base.CheckAPA_PackQty();

		const int packQtyMaxAllowedValue = 99999999;
		var parent = Parent;

		if (parent.APA_PackQty > packQtyMaxAllowedValue)
		{
			parent.APA_PackQtyInfo.AddMessageError(ValidationCaptions.TemporaryStoragePack.PackQtyExceedMaxLength);
		}
	}
}
