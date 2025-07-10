using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGOFFCusTempStorageDec))]
	class CHGOFFCusTempStorageDecValidationTest : CusTempStorageDecValidationTest<CHGOFFCusTempStorageDec>
	{
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

		protected override CHGOFFCusTempStorageDec GetCusTempStorageDecToTest() => Factory.New<CHGOFFCusTempStorageDec>();
	}
}
