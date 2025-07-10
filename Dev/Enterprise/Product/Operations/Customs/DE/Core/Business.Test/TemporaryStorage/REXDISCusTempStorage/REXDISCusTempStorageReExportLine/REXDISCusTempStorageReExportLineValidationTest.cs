using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class REXDISCusTempStorageReExportLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_CustodianIdentifier_NotMandatory()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_PackageQty_Between()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIsBetween1And99999(storageLine.TSL_PackageQtyInfo);
		}

		public void TestCheckTSL_OwnerReferenceType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceTypeInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<REXDISCusTempStorageReExportLine>();
		}
		REXDISCusTempStorageReExportLine storageLine;
	}
}
