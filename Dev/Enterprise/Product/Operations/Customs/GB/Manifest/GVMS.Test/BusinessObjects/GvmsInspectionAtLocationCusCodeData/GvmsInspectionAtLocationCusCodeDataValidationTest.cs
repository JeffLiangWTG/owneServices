using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	sealed class GvmsInspectionAtLocationCusCodeDataValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GVMSMessageTestHelper.SetupInspectionLocationsRefCusCodeList(Factory);
		}

		public void TestCY_CodeValidation()
		{
			var location = Factory.New<GvmsInspectionAtLocationCusCodeData>();

			CombineAssertions(() =>
			{
				location.Validation.ValidateCY_Code();
				AssertHasMessageError(location.CY_CodeInfo, "CY_Code cannot be empty");

				location.CY_Code = "I am an invalid value";
				location.Validation.ValidateCY_Code();
				AssertHasMessageError(location.CY_CodeInfo, "Invalid value for CY_Code");

				location.CY_Code = location.Lookups.TypeList[0].Code;
				location.Validation.ValidateCY_Code();
				AssertNoMessageErrors(location.CY_CodeInfo);
			});
		}

		public void TestCY_DataValidation()
		{
			var location = Factory.New<GvmsInspectionAtLocationCusCodeData>();

			CombineAssertions(() =>
			{
				location.Validation.ValidateCY_Data();
				AssertHasMessageError(location.CY_DataInfo, "CY_Data cannot be empty");

				location.CY_Data = "I am an invalid value";
				location.Validation.ValidateCY_Data();
				AssertHasMessageError(location.CY_DataInfo, "Invalid value for CY_Data");

				location.CY_Data = location.Lookups.InspectionLocationList[0].Code;
				location.Validation.ValidateCY_Data();
				AssertNoMessageErrors(location.CY_DataInfo);
			});
		}
	}
}

