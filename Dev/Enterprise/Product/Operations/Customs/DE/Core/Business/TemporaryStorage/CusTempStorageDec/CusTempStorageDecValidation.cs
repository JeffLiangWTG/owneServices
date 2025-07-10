using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageDecValidation : EU.Business.CusTempStorage.CusTempStorageDecValidation
	{
		public CusTempStorageDecValidation(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		protected new CusTempStorageDec Parent => (CusTempStorageDec)base.Parent;

		protected override void CheckSTH_IdentificationIndicator()
		{
			base.CheckSTH_IdentificationIndicator();
			MandatoryValidation.CheckEntered(Parent.STH_IdentificationIndicatorInfo);
			ListValidation.ErrorIfInvalidCode(Parent.STH_IdentificationIndicatorInfo);
		}

		protected override void CheckSTH_OwnerReferenceNumber()
		{
			base.CheckSTH_OwnerReferenceNumber();

			var parent = Parent;
			if (ShouldValidateRegistrationNumberLengthAndMrnFormat && Parent.STH_IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG)
			{
				var targetInfo = parent.STH_OwnerReferenceNumberInfo;

				var referenceNumberValidationError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat(parent.STH_OwnerReferenceNumber, parent.Factory);
				if (!string.IsNullOrWhiteSpace(referenceNumberValidationError))
				{
					targetInfo.AddMessageError(referenceNumberValidationError);
				}
			}
		}

		public bool ShouldValidateRegistrationNumberLengthAndMrnFormat => ShouldValidateRegistrationNumberLengthAndMrnFormatCore;

		protected virtual bool ShouldValidateRegistrationNumberLengthAndMrnFormatCore => true;
	}
}
