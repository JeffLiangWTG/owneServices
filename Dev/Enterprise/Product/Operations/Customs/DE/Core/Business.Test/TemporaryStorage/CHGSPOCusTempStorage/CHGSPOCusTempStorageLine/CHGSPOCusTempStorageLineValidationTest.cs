using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CHGSPOCusTempStorageLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_LineNo()
		{
			CusTempStorageLineValidationTestHelper.AssertLineNoIsUnique(storageLine.TSL_LineNoInfo);
		}

		public void TestCheckTSL_OwnerReferenceType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceTypeInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo);
		}

		public void TestCheckTSL_CustodianIdentifier()
		{
			storageLine.Validation.ValidateTSL_CustodianIdentifier();
			AssertNoNotifications(storageLine.TSL_CustodianIdentifierInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageDec = Factory.New<CusTempStorageJobHeader>().CHGSPOCusTempStorageDecs.AddNew();
			storageLine = storageDec.CusTempStorageLines.AddNew();
		}
		CHGSPOCusTempStorageLine storageLine;
		CHGSPOCusTempStorageDec storageDec;
	}
}
