using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGSPOCusTempStorageDec))]
	class CHGSPOCusTempStorageDecValidationTest : CusTempStorageDecValidationTest<CHGSPOCusTempStorageDec>
	{
		public new void TestCheckSTH_IdentificationIndicator()
		{
			var storageDec = GetCusTempStorageDecToTest();
			storageDec.STH_IdentificationIndicator = "ERR";
			AssertHasError(storageDec.STH_IdentificationIndicatorInfo, "Enter a valid Identification Type.");
			storageDec.STH_IdentificationIndicator = ZString.Empty;
			AssertHasError(storageDec.STH_IdentificationIndicatorInfo, "Please enter an Identification Type.");
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertHasError(storageDec.STH_IdentificationIndicatorInfo, "Enter a valid Identification Type.");
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			AssertNoErrors(storageDec.STH_IdentificationIndicatorInfo);
		}

		public void TestCheckSTH_OwnerReferenceNumber()
		{
			CombineAssertions(() =>
			{
				var storageDec = GetCusTempStorageDecToTest();
				storageDec.Validation.ValidateSTH_OwnerReferenceNumber();
				AssertHasMessageErrorContaining("Message error for Mandatory", storageDec.STH_OwnerReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
				storageDec.STH_OwnerReferenceNumber = "AT/B/15/000001/03/2000/3001";
				AssertNoMessageErrorContaining("No Message Error for Mandatory", storageDec.STH_OwnerReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override CHGSPOCusTempStorageDec GetCusTempStorageDecToTest() => Factory.NewWithValidTestData<CHGSPOCusTempStorageDec>();
	}
}
