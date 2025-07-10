using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_LineNo()
		{
			storageLine.Validation.ValidateTSL_LineNo();
			AssertHasMessageErrorContaining(storageLine.TSL_LineNoInfo, MandatoryValidation.ValueCannotBeZero);

			storageLine.TSL_LineNo = 3;
			AssertNoMessageErrorContaining(storageLine.TSL_LineNoInfo, MandatoryValidation.ValueCannotBeZero);

			storageLine.TSL_LineNo = -1;
			AssertHasErrorContaining(storageLine.TSL_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);

			storageLine.TSL_LineNo = 3;
			AssertNoErrorContaining(storageLine.TSL_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckTSL_ReferenceNumberLine()
		{
			storageLine.TSL_ReferenceNumberLine = -1;
			AssertHasErrorContaining(storageLine.TSL_ReferenceNumberLineInfo, MandatoryValidation.ValueCannotBeNegative);

			storageLine.TSL_ReferenceNumberLine = 3;
			AssertNoErrorContaining(storageLine.TSL_ReferenceNumberLineInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckTSL_PackageQty()
		{
			storageLine.TSL_PackageQty = -1;
			AssertHasErrorContaining(storageLine.TSL_PackageQtyInfo, MandatoryValidation.ValueCannotBeNegative);

			storageLine.TSL_PackageQty = 3;
			AssertNoErrorContaining(storageLine.TSL_PackageQtyInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckTSL_GrossWeight()
		{
			storageLine.TSL_GrossWeight = -1;
			AssertHasErrorContaining(storageLine.TSL_GrossWeightInfo, MandatoryValidation.ValueCannotBeNegative);

			storageLine.TSL_GrossWeight = 3;
			AssertNoErrorContaining(storageLine.TSL_GrossWeightInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckTSL_ReferenceNumber2Line()
		{
			storageLine.TSL_ReferenceNumber2Line = -1;
			AssertHasErrorContaining(storageLine.TSL_ReferenceNumber2LineInfo, MandatoryValidation.ValueCannotBeNegative);

			storageLine.TSL_ReferenceNumber2Line = 1;
			AssertNoErrorContaining(storageLine.TSL_ReferenceNumber2LineInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<CusTempStorageLine>();
		}
		CusTempStorageLine storageLine;
	}
}
