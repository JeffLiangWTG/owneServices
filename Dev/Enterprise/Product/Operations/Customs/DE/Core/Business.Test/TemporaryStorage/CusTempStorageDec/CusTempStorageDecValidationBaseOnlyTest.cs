using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CusTempStorageDecValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestShouldValidateRegistrationNumberLengthAndMrnFormat()
		{
			const string message = "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.).";
			const string mrnMessage = "Please enter a MRN in the following format with only numbers and upper case letters";
			var storageDec = Factory.New<CusTempStorageDecForTest>();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			var validation = new CusTempStorageDecValidation(storageDec);
			AssertEquals(true, storageDec.Validation.ShouldValidateRegistrationNumberLengthAndMrnFormat);
			AssertEquals(true, validation.ShouldValidateRegistrationNumberLengthAndMrnFormat);

			storageDec.STH_OwnerReferenceNumber = "".PadLeft(18, 'A');
			validation.ValidateSTH_OwnerReferenceNumber();

			AssertHasMessageErrorContaining(storageDec.STH_OwnerReferenceNumberInfo, mrnMessage);
			AssertNoMessageErrorContaining(storageDec.STH_OwnerReferenceNumberInfo, message);

			storageDec.STH_OwnerReferenceNumber = "".PadLeft(22, 'A');
			validation.ValidateSTH_OwnerReferenceNumber();

			AssertNoMessageErrorContaining(storageDec.STH_OwnerReferenceNumberInfo, mrnMessage);
			AssertHasMessageErrorContaining(storageDec.STH_OwnerReferenceNumberInfo, message);

			storageDec.STH_OwnerReferenceNumber = "".PadLeft(21, 'A');
			validation.ValidateSTH_OwnerReferenceNumber();

			AssertNoMessageErrorContaining(storageDec.STH_OwnerReferenceNumberInfo, mrnMessage);
			AssertNoMessageErrorContaining(storageDec.STH_OwnerReferenceNumberInfo, message);

			validation = new CusTempStorageDecValidationForTest(storageDec);
			AssertEquals(false, validation.ShouldValidateRegistrationNumberLengthAndMrnFormat);
			storageDec.STH_OwnerReferenceNumber = "".PadLeft(22, 'A');
			validation.ValidateSTH_OwnerReferenceNumber();

			AssertNoMessageErrorContaining(storageDec.STH_OwnerReferenceNumberInfo, mrnMessage);
			AssertNoMessageErrorContaining(storageDec.STH_OwnerReferenceNumberInfo, message);
		}

		sealed class CusTempStorageDecValidationForTest : CusTempStorageDecValidation
		{
			public CusTempStorageDecValidationForTest(AutoCusTempStorageDec parent) : base(parent)
			{
			}

			protected override bool ShouldValidateRegistrationNumberLengthAndMrnFormatCore => false;
		}
	}
}
