using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	public class CusTempStorageLineValidationTest : TestCaseWithFactory
	{
		public void TestCheckTSL_PackageQty()
		{
			var dec = Factory.New<ISTCusTempStorageDec>();
			var line1 = dec.CusTempStorageLines.AddNew();
			line1.TSL_PackageQty = 0;
			AssertHasErrorContaining(line1.TSL_PackageQtyInfo, "Package quantity must be greater than 0");
			line1.TSL_PackageQty = 1;
			AssertNoErrorContaining(line1.TSL_PackageQtyInfo, "Package quantity must be greater than 0");
		}

		public void TestOnlyOneAWBAllowed()
		{
			var dec = Factory.New<ISTCusTempStorageDec>();

			var line1 = dec.CusTempStorageLines.AddNew();
			line1.TSL_OwnerReferenceType = "AWB";

			var line2 = dec.CusTempStorageLines.AddNew();
			line2.TSL_OwnerReferenceType = "AWB";

			AssertHasMessageErrorContaining(line2.TSL_OwnerReferenceTypeInfo, "Only a single AWB is allowed on a declaration");

			line2.TSL_OwnerReferenceType = "HWB";
			AssertNoMessageErrorContaining(line2.TSL_OwnerReferenceTypeInfo, "Only a single AWB is allowed on a declaration");

			line1.TSL_OwnerReferenceType = "HWB";
			AssertNoMessageErrorContaining(line2.TSL_OwnerReferenceTypeInfo, "Only a single AWB is allowed on a declaration");
		}
	}
}
