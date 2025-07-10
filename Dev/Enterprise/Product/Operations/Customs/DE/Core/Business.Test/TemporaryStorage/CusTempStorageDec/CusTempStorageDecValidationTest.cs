using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(CusTempStorageDec))]
	abstract class CusTempStorageDecValidationTest<T> : BusinessObjectValidationTestCase
		where T : CusTempStorageDec
	{
		public virtual void TestCheckSTH_IdentificationIndicator()
		{
			var storageDec = GetCusTempStorageDecToTest();
			storageDec.STH_IdentificationIndicator = "ERR";
			AssertHasError(storageDec.STH_IdentificationIndicatorInfo, "Enter a valid Identification Type.");
			storageDec.STH_IdentificationIndicator = ZString.Empty;
			AssertHasError(storageDec.STH_IdentificationIndicatorInfo, "Please enter an Identification Type.");
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertNoErrors(storageDec.STH_IdentificationIndicatorInfo);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			AssertNoErrors(storageDec.STH_IdentificationIndicatorInfo);
		}

		public void TestSTH_OwnerReferenceNumberValidation()
		{
			var storageDec = GetCusTempStorageDecToTest();
			if (!storageDec.Validation.ShouldValidateRegistrationNumberLengthAndMrnFormat)
			{
				Assert("Validation test not applicable", true);
				return;
			}
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("STH_OwnerReferenceNumber should not be validated as Atlas / MRN for AWB", storageDec.STH_OwnerReferenceNumberInfo);

			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			TestHelper.AssertMRNFormatValidatedOr21CharactersLong("STH_OwnerReferenceNumber should be validated as Atlas / MRN for REG", storageDec.STH_OwnerReferenceNumberInfo);
		}

		protected abstract T GetCusTempStorageDecToTest();
	}
}
