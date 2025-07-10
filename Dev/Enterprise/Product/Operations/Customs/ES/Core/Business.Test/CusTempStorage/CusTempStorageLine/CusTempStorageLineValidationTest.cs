using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	public class CusTempStorageLineValidationTest : TestCaseWithFactory
	{
		public void TestOnlyOneAWBAllowed()
		{
			var dec = Factory.New<CusTempStorageDec>();

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
